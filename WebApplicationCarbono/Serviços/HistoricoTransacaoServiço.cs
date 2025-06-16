using Npgsql;
using WebApplicationCarbono.Dtos.WebApplicationCarbono.Dtos;
using WebApplicationCarbono.Interface;

namespace WebApplicationCarbono.Serviços
{
    public class HistoricoTransacaoServiço : IHistoricoTransacao
    {
        // passando a conexão do banco
        private readonly String _stringConexao;
        public HistoricoTransacaoServiço(IConfiguration configuaração)
        {
            _stringConexao = configuaração.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("DefaultConnection não está configurada.");
        }

        public List<HistoricoTransacaoDto> BuscarTransacoes(int idUsuario, DateTime? dataInicio, DateTime? dataFim, string? tipo)
        {
            var lista = new List<HistoricoTransacaoDto>();

            using var conexao = new NpgsqlConnection(_stringConexao);
            conexao.Open();

            // Se não informar datas, busca dos últimos 7 dias
            if (!dataInicio.HasValue && !dataFim.HasValue)
            {
                dataInicio = DateTime.UtcNow.AddDays(-7);
                dataFim = DateTime.UtcNow;
            }

            var sql = @"
            SELECT data_hora, valor_creditos, tipo_transacao, descricao, status_transacao 
            FROM saldo_usuario_dinamica
            WHERE id_usuario = @IdUsuario";

            if (dataInicio.HasValue)
                sql += " AND data_hora >= @DataInicio";

            if (dataFim.HasValue)
                sql += " AND data_hora <= @DataFim";

            if (!string.IsNullOrEmpty(tipo))
            {
                var tipoLower = tipo.ToLower();
                if (tipoLower == "transferência" || tipoLower == "transferencias" || tipoLower == "transferencia")
                {
                    sql += " AND (tipo_transacao = 'transferência_entrada' OR tipo_transacao = 'transferência_saida')";
                }
                else
                {
                    sql += " AND tipo_transacao = @Tipo";
                }
            }

            sql += " ORDER BY data_hora DESC"; // ordena da mais recente para a mais antiga

            using var comando = new NpgsqlCommand(sql, conexao);
            comando.Parameters.AddWithValue("@IdUsuario", idUsuario);

            if (dataInicio.HasValue)
                comando.Parameters.AddWithValue("@DataInicio", dataInicio.Value);

            if (dataFim.HasValue)
                comando.Parameters.AddWithValue("@DataFim", dataFim.Value);

            if (!string.IsNullOrEmpty(tipo) &&
                tipo.ToLower() != "transferência" &&
                tipo.ToLower() != "transferencia" &&
                tipo.ToLower() != "transferencias")
            {
                comando.Parameters.AddWithValue("@Tipo", tipo.ToLower());
            }

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new HistoricoTransacaoDto
                {
                    DataHora = Convert.ToDateTime(reader["data_hora"]),
                    Quantidade = Convert.ToDecimal(reader["valor_creditos"]),
                    Tipo = reader["tipo_transacao"].ToString() ?? "",
                    Descricao = reader["descricao"].ToString() ?? "",
                    Status = reader["status_transacao"].ToString() ?? ""
                });
            }

            return lista;
        }

    }
}
