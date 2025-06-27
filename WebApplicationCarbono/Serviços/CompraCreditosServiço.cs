using Npgsql;
using System.Threading.Tasks;
using WebApplicationCarbono.Modelos;

public class CompraCreditosServico : ICompraCreditos
{
    private readonly string _conexao;
    private readonly PagamentoServico _pagamentoServico;

    public CompraCreditosServico(IConfiguration config, PagamentoServico pagamentoServico)
    {
        _conexao = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        _pagamentoServico = pagamentoServico;
    }
    // Método para iniciar a compra de créditos
    public CompraCreditoResultado IniciarCompraCredito(ComprarCredito compra)
    {
        if (compra.ValorReais <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.");

        decimal valorUnitarioProjeto;
        decimal creditosDisponiveis;

        using var conexao = new NpgsqlConnection(_conexao);
        conexao.Open();

        using (var cmdProjeto = new NpgsqlCommand("SELECT valor, creditos_disponivel FROM projetos WHERE id = @idProjeto", conexao))
        {
            cmdProjeto.Parameters.AddWithValue("@idProjeto", compra.IdProjeto);
            using var reader = cmdProjeto.ExecuteReader();

            if (!reader.Read())
                throw new Exception("Projeto não encontrado.");

            valorUnitarioProjeto = reader.GetDecimal(0);
            creditosDisponiveis = reader.GetDecimal(1);
        }

        decimal quantidadeCredito = compra.ValorReais / valorUnitarioProjeto;

        if (quantidadeCredito <= 0)
            throw new Exception("Valor insuficiente para comprar créditos.");

        if (quantidadeCredito > creditosDisponiveis)
            throw new Exception("Créditos insuficientes no projeto.");

        var pagamento = _pagamentoServico.CriarPagamentoPixAsync(compra.ValorReais, compra.EmailUsuario).Result;

        string qrCodePix = pagamento.PointOfInteraction.TransactionData.QrCode;
        string? idPagamento = pagamento.Id?.ToString();

        if (idPagamento == null)
            throw new Exception("Erro ao gerar pagamento.");

        using (var cmdDescontar = new NpgsqlCommand("UPDATE projetos SET creditos_disponivel = creditos_disponivel - @qtd WHERE id = @idProjeto", conexao))
        {
            cmdDescontar.Parameters.AddWithValue("@qtd", quantidadeCredito);
            cmdDescontar.Parameters.AddWithValue("@idProjeto", compra.IdProjeto);
            cmdDescontar.ExecuteNonQuery();
        }

        using var insertCmd = new NpgsqlCommand(@"
            INSERT INTO saldo_usuario_dinamica
            (id_usuario, tipo_transacao, valor_creditos, creditos_reservados, data_hora, descricao, id_usuario_destino, status_transacao, id_pagamento_mercadopago, id_projetos)
            VALUES (@id_usuario, 'compra', 0, @creditos_reservados, NOW(), @descricao, NULL, 'Pendente', @id_pagamento, @id_projetos);", conexao);

        insertCmd.Parameters.AddWithValue("@id_usuario", compra.IdUsuario);
        insertCmd.Parameters.AddWithValue("@creditos_reservados", quantidadeCredito);
        insertCmd.Parameters.AddWithValue("@descricao", $"Compra via Pix - R$ {compra.ValorReais:F2} para {quantidadeCredito:F2} créditos");
        insertCmd.Parameters.AddWithValue("@id_pagamento", idPagamento);
        insertCmd.Parameters.AddWithValue("@id_projetos", compra.IdProjeto);
        insertCmd.ExecuteNonQuery();

        return new CompraCreditoResultado
        {
            QrCode = qrCodePix,
            PagamentoId = idPagamento
        };
    }
    // Método chamado pelo webhook do Mercado Pago para confirmar a compra
    public async Task<string> ConfirmarCompraWebhookAsync(MercadoPagoNotification notification)
    {
        var pagamentoId = notification.Data.Id;
        var status = await _pagamentoServico.ObterStatusPagamentoAsync(pagamentoId);

        using var conexao = new NpgsqlConnection(_conexao);
        conexao.Open();

        using var selectCmd = new NpgsqlCommand(@"
        SELECT id, descricao, id_usuario, id_projetos, valor_creditos, creditos_reservados, data_hora, status_transacao 
        FROM saldo_usuario_dinamica 
        WHERE id_pagamento_mercadopago = @pagamentoId", conexao);

        selectCmd.Parameters.AddWithValue("@pagamentoId", pagamentoId);

        using var reader = selectCmd.ExecuteReader();

        if (!reader.Read())
            return "Transação não encontrada.";

        int registroId = reader.GetInt32(0);
        string descricao = reader.GetString(1);
        int idUsuario = reader.GetInt32(2);
        int idProjeto = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);  // <<< aqui
        decimal valorCreditos = reader.GetDecimal(4);
        decimal creditosReservados = reader.GetDecimal(5);
        DateTime dataHora = reader.GetDateTime(6);
        string statusTransacao = reader.GetString(7);
        reader.Close();

        if (status == "approved")
        {
            if (statusTransacao == "Concluído")
                return "Compra já confirmada.";

            using var updateCmd = new NpgsqlCommand(@"
            UPDATE saldo_usuario_dinamica
            SET valor_creditos = creditos_reservados,
                creditos_reservados = 0,
                status_transacao = 'Concluído',
                descricao = @descricaoFinal
            WHERE id = @idRegistro;", conexao);

            updateCmd.Parameters.AddWithValue("@descricaoFinal", $"Compra aprovada via Pix - {creditosReservados:F2} créditos");
            updateCmd.Parameters.AddWithValue("@idRegistro", registroId);
            updateCmd.ExecuteNonQuery();

            string nomeUsuario = ObterNomeUsuario(conexao, idUsuario);
            return $"Compra confirmada. {creditosReservados:F2} créditos adicionados ao usuário {nomeUsuario}.";
        }
        else if (status == "expired" && statusTransacao == "Pendente")
        {
            using var transaction = conexao.BeginTransaction();

            try
            {
                using var updateCmd = new NpgsqlCommand(@"
                UPDATE saldo_usuario_dinamica
                SET status_transacao = 'Expirado-Falhou',
                    creditos_reservados = 0
                WHERE id = @idRegistro;", conexao, transaction);

                updateCmd.Parameters.AddWithValue("@idRegistro", registroId);
                updateCmd.ExecuteNonQuery();

                using var revertCmd = new NpgsqlCommand(@"
                UPDATE projetos
                SET creditos_disponivel = creditos_disponivel + @creditos
                WHERE id = @idProjeto;", conexao, transaction);

                revertCmd.Parameters.AddWithValue("@creditos", creditosReservados);
                revertCmd.Parameters.AddWithValue("@idProjeto", idProjeto);
                revertCmd.ExecuteNonQuery();

                transaction.Commit();
                return "Pagamento expirado. Créditos devolvidos ao projeto.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return $"Erro ao expirar pagamento: {ex.Message}";
            }
        }

        return "Pagamento ainda não aprovado.";
    }

   

    private string ObterNomeUsuario(NpgsqlConnection conexao, int usuarioId)
    {
        var comando = new NpgsqlCommand("SELECT nome FROM usuarios WHERE id = @id", conexao);
        comando.Parameters.AddWithValue("id", usuarioId);

        var resultado = comando.ExecuteScalar();
        return resultado?.ToString() ?? "Desconhecido";
    }
}
