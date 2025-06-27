using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Npgsql;

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


    // Método para verificar pagamentos pendentes e atualizar o status
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


}