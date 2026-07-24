using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de la configuración de Bancos habilitados para Beneficios (frmAF_BeneficiosBancosX).
    /// </summary>
    public partial class FrmAfBeneficiosBancosXDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosBancosXDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de bancos habilitados para beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de bancos y total.</returns>
        public ErrorDto<AfBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, string filtros)
        {
            // Inicializa registros faltantes antes de consultar.
            BeneficiosBancosX_Existe(CodCliente);

            var filtro = JsonConvert.DeserializeObject<AfBeneficioBancosfiltros>(filtros) ?? new AfBeneficioBancosfiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfBeneficiosBancosDataLista();

                const string sqlCount = @"SELECT COUNT(*)
                                          FROM afi_bene_Bancos_X X
                                          INNER JOIN Tes_Bancos B ON X.id_banco = B.id_Banco";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT X.id_banco, B.descripcion, X.cheque, X.transferencia
                                     FROM afi_bene_Bancos_X X
                                     INNER JOIN Tes_Bancos B ON X.id_banco = B.id_Banco
                                     WHERE (@like IS NULL OR X.id_banco LIKE @like OR B.descripcion LIKE @like)
                                     ORDER BY B.id_banco
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.bancosX = connection.Query<AfBeneficiosBancosData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<AfBeneficiosBancosDataLista>("BeneficiosBancosX_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Actualiza la configuración de un banco (cheque/transferencia) para beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="data">Datos del banco a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<AfBeneficiosBancosData> BeneficiosBancosX_Actualizar(int CodCliente, AfBeneficiosBancosData data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"UPDATE afi_bene_Bancos_X
                                     SET cheque = @cheque, transferencia = @transferencia
                                     WHERE id_banco = @id_banco";

                connection.Execute(sql, new
                {
                    cheque = data.cheque ? 1 : 0,
                    transferencia = data.transferencia ? 1 : 0,
                    id_banco = data.id_banco
                });

                return data;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<AfBeneficiosBancosData>("BeneficiosBancosX_Actualizar - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Inserta los bancos faltantes en afi_bene_Bancos_X respecto a Tes_Bancos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        private void BeneficiosBancosX_Existe(int CodCliente)
        {
            const string sql = @"INSERT INTO afi_bene_Bancos_X (id_banco, cheque, transferencia)
                                 SELECT id_banco, 0, 0 FROM Tes_Bancos
                                 WHERE id_Banco NOT IN (SELECT id_Banco FROM afi_bene_Bancos_X)";

            DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql);
        }
    }
}
