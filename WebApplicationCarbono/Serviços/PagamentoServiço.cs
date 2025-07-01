using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Npgsql;
using WebApplicationCarbono.Interface;

public class PagamentoServico : IPagamento
{
    private readonly string _conexao;

    public PagamentoServico(IConfiguration config)
    {
        _conexao = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");

    }
    public async Task<Payment> CriarPagamentoPixAsync(decimal valor, string emailCliente)
    {
        var paymentRequest = new PaymentCreateRequest
        {
            TransactionAmount = valor,
            Description = "Pagamento via Pix - Créditos de Carbono",
            PaymentMethodId = "pix",
            Payer = new PaymentPayerRequest { Email = emailCliente },
            DateOfExpiration = DateTime.UtcNow.AddMinutes(10) // ← expira em 2 horas
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
        SELECT id, id_pagamento_mercadopago, creditos_reservados, id_projetos, id_usuario
        FROM saldo_usuario_dinamica
        WHERE status_transacao = 'Pendente';", conexao);

        using var reader = await comando.ExecuteReaderAsync();

        var pendentes = new List<(int Id, string PagamentoId, decimal Reservados, int ProjetoId, int UsuarioId)>();

        while (await reader.ReadAsync())
        {
            pendentes.Add((
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                reader.GetInt32(4)
            ));
        }

        await reader.CloseAsync();

        foreach (var item in pendentes)
        {
            var status = await ObterStatusPagamentoAsync(item.PagamentoId);

            if (status == "approved")
            {
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

                string nomeUsuario = ObterNomeUsuario(conexao, item.UsuarioId);
                Console.WriteLine($"Compra confirmada para {nomeUsuario}. Créditos: {item.Reservados:F2}");
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
}