using Npgsql;
using System.Threading.Tasks;

public class CompraCreditosServico : ICompraCreditos
{
    private readonly string _conexao;
    private readonly PagamentoServico _pagamentoServico;
    private const decimal VALOR_UNITARIO_CREDITO = 0.01m;

    public CompraCreditosServico(IConfiguration config, PagamentoServico pagamentoServico)
    {
        _conexao = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
        _pagamentoServico = pagamentoServico;
    }

    public CompraCreditoResultado IniciarCompraCredito(ComprarCredito compra)
    {
        if (compra.quantidadeCredito <= 0)
            throw new ArgumentException("Quantidade de créditos deve ser maior que zero.");

        decimal valorTotal = compra.quantidadeCredito * VALOR_UNITARIO_CREDITO;

        var pagamento = _pagamentoServico.CriarPagamentoPixAsync(valorTotal, compra.emailUsuario).Result;

        var qrCodePix = pagamento.PointOfInteraction.TransactionData.QrCode;
        var idPagamento = pagamento.Id?.ToString();

        // Verifica se o id do pagamento é nulo
        if (idPagamento == null)
            throw new Exception("Falha ao gerar ID do pagamento.");

        using var conexao = new NpgsqlConnection(_conexao);
        conexao.Open();

        using var cmd = new NpgsqlCommand(@"
        INSERT INTO saldo_usuario_dinamica
        (id_usuario, tipo_transacao, valor_creditos, data_hora, descricao, id_usuario_destino, status_transacao, id_pagamento_mercadopago)
        VALUES (@id_usuario, 'compra', 0, NOW(), @descricao, NULL, 'Pendente', @id_pagamento);", conexao);

        cmd.Parameters.AddWithValue("@id_usuario", compra.idUsuario);
        cmd.Parameters.AddWithValue("@id_pagamento", idPagamento);
        cmd.Parameters.AddWithValue("@descricao", $"Compra iniciada via Pix - quantidade: {compra.quantidadeCredito}");

        cmd.ExecuteNonQuery();


        return new CompraCreditoResultado
        {
            qrCode = qrCodePix,
            pagamentoId = idPagamento
        };
    }

    public async Task<string> ConfirmarCompraWebhookAsync(MercadoPagoNotification notification)
    {
        var pagamentoId = notification.Data.Id;

        var status = await _pagamentoServico.ObterStatusPagamentoAsync(pagamentoId);

        if (status == "approved")
        {
            using var conexao = new NpgsqlConnection(_conexao);
            conexao.Open();

            using var selectCmd = new NpgsqlCommand(@"
            SELECT descricao, id_usuario FROM saldo_usuario_dinamica
            WHERE id_pagamento_mercadopago = @pagamentoId;", conexao);

            selectCmd.Parameters.AddWithValue("@pagamentoId", pagamentoId);

            using var reader = selectCmd.ExecuteReader();

            if (!reader.Read())
                return "Registro pendente não encontrado.";

            string descricao = reader.GetString(0);
            int idUsuario = reader.GetInt32(1);

            reader.Close();

            int quantidadeCredito = 0;
            var partes = descricao.Split("quantidade:");
            if (partes.Length == 2)
                int.TryParse(partes[1].Trim(), out quantidadeCredito);

            if (quantidadeCredito == 0)
                return "Erro ao extrair a quantidade de créditos.";

            // Calcula o valor pago com base na constante
            decimal valorPago = quantidadeCredito * VALOR_UNITARIO_CREDITO;

            // Atualiza a transação com os dados completos
            using var updateCmd = new NpgsqlCommand(@"
            UPDATE saldo_usuario_dinamica
            SET valor_creditos = @quantidadeCredito,
                status_transacao = 'Aprovado',
                descricao = @descricao
            WHERE id_pagamento_mercadopago = @pagamentoId;", conexao);

            updateCmd.Parameters.AddWithValue("@quantidadeCredito", quantidadeCredito);
            updateCmd.Parameters.AddWithValue("@pagamentoId", pagamentoId);
            updateCmd.Parameters.AddWithValue("@descricao", $"Compra aprovada via Pix - {quantidadeCredito} créditos (R$ {valorPago:F2})");

            int linhasAfetadas = updateCmd.ExecuteNonQuery();

            if (linhasAfetadas > 0)
                return $"Compra confirmada. {quantidadeCredito} créditos adicionados para o usuário {idUsuario}.";
        }

        return "Pagamento ainda não aprovado.";
    }

}
