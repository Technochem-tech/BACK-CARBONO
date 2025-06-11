using Npgsql;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplicationCarbono.Interface;

public class VendaCreditoServico : IVendaCredito
{
    private readonly string _conexao;
    private const decimal VALOR_UNITARIO_VENDA = 0.05m;

    public VendaCreditoServico(IConfiguration config, IPagamento pagamentoServico)
    {
        _conexao = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection");
    }

    public async Task<string> RealizarVendaAsync(int idUsuario, decimal quantidadeCreditos)
    {
        if (quantidadeCreditos <= 0)
        {
            return "A quantidade que deseja vender deve ser maior que 0.";
        }

        await using var conexao = new NpgsqlConnection(_conexao);
        await conexao.OpenAsync();

        var saldoDisponivel = await VerificarSaldoAsync(conexao, idUsuario);

        if (saldoDisponivel < quantidadeCreditos)
        {
            return $"Saldo insuficiente. Seu saldo atual é de {saldoDisponivel} créditos.";
        }

        decimal valorDinheiro = quantidadeCreditos * VALOR_UNITARIO_VENDA;

        // Insere a venda na tabela com os créditos negativos e saldo_dinheiro
        await using var insertCmd = new NpgsqlCommand(@"
            INSERT INTO saldo_usuario_dinamica 
            (id_usuario, tipo_transacao, valor_creditos, saldo_dinheiro, data_hora, descricao, status_transacao)
            VALUES
            (@idUsuario, 'venda', @valorNegativo, @valorDinheiro, NOW(), @descricao, 'Concluido');", conexao);

        insertCmd.Parameters.AddWithValue("@idUsuario", idUsuario);
        insertCmd.Parameters.AddWithValue("@valorNegativo", -quantidadeCreditos);
        insertCmd.Parameters.AddWithValue("@valorDinheiro", valorDinheiro);
        insertCmd.Parameters.AddWithValue("@descricao", $"Venda de {quantidadeCreditos} créditos (R$ {valorDinheiro:F2})");

        await insertCmd.ExecuteNonQueryAsync();

        return $"Venda realizada com sucesso. Você recebeu R$ {valorDinheiro:F2}.";
    }

    private async Task<decimal> VerificarSaldoAsync(NpgsqlConnection conexao, int idUsuario)
    {
        await using var saldoCmd = new NpgsqlCommand(@"
            SELECT COALESCE(SUM(valor_creditos), 0) 
            FROM saldo_usuario_dinamica 
            WHERE id_usuario = @idUsuario;", conexao);

        saldoCmd.Parameters.AddWithValue("@idUsuario", idUsuario);
        var saldoObj = await saldoCmd.ExecuteScalarAsync();
        return Convert.ToDecimal(saldoObj);
    }
}
