using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using MimeKit;
using MailKit.Net.Smtp;
using Npgsql;
using WebApplicationCarbono.Interface;
using WebApplicationCarbono.Serviços;

public class PagamentoServico : IPagamento
{
    private readonly string _conexao;
    private readonly IConfiguration _config;
    private readonly GmailServico _gmailServico;

    public PagamentoServico(IConfiguration config, GmailServico gmailServico)
    {
        _config = config;
        _conexao = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        _gmailServico = gmailServico;
    }

    public async Task<Payment> CriarPagamentoPixAsync(decimal valor, string emailCliente)
    {
        var paymentRequest = new PaymentCreateRequest
        {
            TransactionAmount = valor,
            Description = "Pagamento via Pix - Créditos de Carbono",
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest { Email = emailCliente },
            DateOfExpiration = DateTime.UtcNow.AddMinutes(10) // ← expira em 10 min
        };

        var client = new PaymentClient();
        var payment = await client.CreateAsync(paymentRequest);
        return payment;
    }

    // Método para obter o status do pagamento
    public async Task<string> ObterStatusPagamentoAsync(string pagamentoId)
    {
        if (!long.TryParse(pagamentoId, out long idPagamento))
            throw new ArgumentException("ID de pagamento inválido.");

        var client = new PaymentClient();
        var pagamento = await client.GetAsync(idPagamento);
        return pagamento.Status; // "approved", "pending", etc.
    }


    // Método para verificar pagamentos pendentes e atualizar o status se aprovados
    public async Task VerificarPagamentosAprovadosasync()
    {
        using var conexao = new NpgsqlConnection(_conexao);
        await conexao.OpenAsync();

        var comando = new NpgsqlCommand(@"
        SELECT id, id_pagamento_mercadopago, creditos_reservados, id_projetos, id_usuario, valor_creditos, valor_compra
        FROM saldo_usuario_dinamica
        WHERE status_transacao = 'Pendente';", conexao);

        using var reader = await comando.ExecuteReaderAsync();

        var pendentes = new List<(int Id, string PagamentoId, decimal Reservados, int ProjetoId, int UsuarioId, decimal ValorCreditos, decimal ValorCompra)>();

        while (await reader.ReadAsync())
        {
            pendentes.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetDecimal(5),
                reader.IsDBNull(6) ? 0 : reader.GetDecimal(6) // 🔹 pega valor_compra
            ));
        }

        await reader.CloseAsync();

        foreach (var item in pendentes)
        {
            var status = await ObterStatusPagamentoAsync(item.PagamentoId);

            if (status == "approved")
            {
                // Atualiza saldo_usuario_dinamica
                using var updateCmd = new NpgsqlCommand(@"
                UPDATE saldo_usuario_dinamica
                SET valor_creditos = creditos_reservados,
                creditos_reservados = 0,
                status_transacao = 'Concluído',
                descricao = @descricaoFinal
                WHERE id = @idRegistro;", conexao);

                updateCmd.Parameters.AddWithValue("@descricaoFinal", $"Compra aprovada via Pix - {item.Reservados:F2} créditos");
                updateCmd.Parameters.AddWithValue("@idRegistro", item.Id);
                await updateCmd.ExecuteNonQueryAsync();

                // Criar registro na tabela compra_btc
                string nomeUsuario = ObterNomeUsuario(conexao, item.UsuarioId);

                using var insertBtcCmd = new NpgsqlCommand(@"
                INSERT INTO compra_btc 
                (id_usuario, nome_usuario, valor_reais, quantidade_creditos, quantidade_btc, status, descricao, data_criacao)
                VALUES 
                (@idUsuario, @nomeUsuario, @valorReais, @creditos, NULL, 'Pendente', @descricao, NOW());", conexao);

                insertBtcCmd.Parameters.AddWithValue("@idUsuario", item.UsuarioId);
                insertBtcCmd.Parameters.AddWithValue("@nomeUsuario", nomeUsuario);
                insertBtcCmd.Parameters.AddWithValue("@valorReais", item.ValorCompra); // 🔹 salva valor da compra
                insertBtcCmd.Parameters.AddWithValue("@creditos", item.Reservados);
                insertBtcCmd.Parameters.AddWithValue("@descricao", "Compra de BTC ainda pendente");
                await insertBtcCmd.ExecuteNonQueryAsync();

                Console.WriteLine($"Compra confirmada para {nomeUsuario}. Créditos: {item.Reservados:F2}");

                // Envia confirmação por e-mail
                await EnviarEmailConfirmacaoAsync(conexao, item.UsuarioId, item.PagamentoId, nomeUsuario);
            }
        }
    }


    // Método para verificar pagamentos pendentes e atualizar o status se expirados ou cancelados
    public async Task VerificarPagamentosPendentesAsync()
    {
        using var conexao = new NpgsqlConnection(_conexao);
        await conexao.OpenAsync();

        var comando = new NpgsqlCommand(@"
        SELECT id, id_pagamento_mercadopago, creditos_reservados, id_projetos
        FROM saldo_usuario_dinamica
        WHERE status_transacao = 'Pendente';", conexao);

        using var reader = await comando.ExecuteReaderAsync();

        var pendentes = new List<(int Id, string PagamentoId, decimal Reservados, int? ProjetoId)>();

        while (await reader.ReadAsync())
        {
            pendentes.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3)
            ));
        }

        await reader.CloseAsync();

        foreach (var item in pendentes)
        {
            var status = await ObterStatusPagamentoAsync(item.PagamentoId);

            if ((status == "expired" || status == "cancelled") && item.ProjetoId.HasValue)
            {
                using var transaction = conexao.BeginTransaction();

                try
                {
                    var update = new NpgsqlCommand(@"
                    UPDATE saldo_usuario_dinamica
                    SET status_transacao = 'Expirado-Falhou', creditos_reservados = 0
                    WHERE id = @id;", conexao, transaction);

                    update.Parameters.AddWithValue("@id", item.Id);
                    await update.ExecuteNonQueryAsync();

                    var devolver = new NpgsqlCommand(@"
                    UPDATE projetos
                    SET creditos_disponivel = creditos_disponivel + @creditos
                    WHERE id = @idProjeto;", conexao, transaction);

                    devolver.Parameters.AddWithValue("@creditos", item.Reservados);
                    devolver.Parameters.AddWithValue("@idProjeto", item.ProjetoId.Value);
                    await devolver.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }
        }
    }

    private string ObterNomeUsuario(NpgsqlConnection conexao, int usuarioId)
    {
        var comando = new NpgsqlCommand("SELECT nome FROM usuarios WHERE id = @id", conexao);
        comando.Parameters.AddWithValue("id", usuarioId);

        var resultado = comando.ExecuteScalar();
        return resultado?.ToString() ?? "Desconhecido";
    }

    private async Task EnviarEmailConfirmacaoAsync(NpgsqlConnection conexao, int usuarioId, string pagamentoId, string nomeUsuario)
    {
        var comando = new NpgsqlCommand("SELECT email FROM usuarios WHERE id = @id", conexao);
        comando.Parameters.AddWithValue("id", usuarioId);

        var resultado = await comando.ExecuteScalarAsync();
        if (resultado == null)
        {
            Console.WriteLine($"Usuário com ID {usuarioId} não encontrado para envio de e-mail.");
            return;
        }

        var email = resultado.ToString();
        _gmailServico.EnviarEmail(
        email,
        "Compra Aprovada - Créditos de Carbono",
        $"<p>Olá <strong>{nomeUsuario}</strong>, sua compra foi <strong>aprovada com sucesso</strong>.</p>" +
        $"<p>ID do Pagamento: <strong>{pagamentoId}</strong></p>" +
        $"<p>Obrigado por sua contribuição ao meio ambiente!</p>"
        );


        //var email = resultado.ToString();

        //var mensagem = new MimeMessage();
        //mensagem.From.Add(new MailboxAddress("Suporte", _config["EmailSettings:From"]));
        //mensagem.To.Add(new MailboxAddress("", email));
        //mensagem.Subject = "Compra Aprovada - Créditos de Carbono";

        //mensagem.Body = new TextPart("html")
        //{
        //    Text = $"<p>Olá <strong>{nomeUsuario}</strong>, sua compra foi <strong>aprovada com sucesso</strong>.</p>" +
        //            $"<p>ID do Pagamento: <strong>{pagamentoId}</strong></p>" +
        //            $"<p>Obrigado por sua contribuição ao meio ambiente!</p>"
        //};


        //using var client = new SmtpClient();
        //await client.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]), true);
        //await client.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
        //await client.SendAsync(mensagem);
        //await client.DisconnectAsync(true);
    }
}