using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de la configuración de Bancos habilitados para Beneficios (frmAF_BeneficiosBancosX).
    /// </summary>
    public partial class FrmAfBeneficiosBancosXdb
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosBancosXdb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlBancosFrom = @"
FROM afi_bene_Bancos_X X
INNER JOIN Tes_Bancos B ON X.id_banco = B.id_Banco
WHERE (@filtro IS NULL)
   OR (X.id_banco LIKE @like)
   OR (B.descripcion LIKE @like)
";

        private const string SqlBancosCount = "SELECT COUNT(1) " + SqlBancosFrom;

        private const string SqlBancosSelect =
            "SELECT X.id_banco, B.descripcion, X.cheque, X.transferencia " + SqlBancosFrom;

        /// <summary>
        /// Obtiene la lista de bancos habilitados para beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de bancos y total de registros.</returns>
        public ErrorDto<AfBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, FiltrosLazyLoadData filtros)
        {
            // Inicializa los bancos faltantes antes de consultar, igual que el proceso original.
            BeneficiosBancosX_Existe(CodCliente);

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = QueryBancos(connection, filtros, true, out var total);
                return new AfBeneficiosBancosDataLista { total = total, bancosX = lista };
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<AfBeneficiosBancosDataLista>(
                    "BeneficiosBancosX_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Exporta la lista de bancos aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de bancos sin paginar.</returns>
        public ErrorDto<List<AfBeneficiosBancosData>> BeneficiosBancosX_Exportar(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                QueryBancos(connection, filtros, false, out _));
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
                return DbHelper.CreateErrorResponse<AfBeneficiosBancosData>(
                    "BeneficiosBancosX_Actualizar - " + result.Description);
            }

            return result;
        }

        // ==========================
        // Helpers privados
        // ==========================

        /// <summary>
        /// Consulta los bancos aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de bancos.</returns>
        private static List<AfBeneficiosBancosData> QueryBancos(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlBancosCount, new { filtro, like });

            var sqlList = SqlBancosSelect + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            if (usarPaginacion && fetch > 0)
            {
                sqlList += "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
            }
            else
            {
                sqlList += ";";
            }

            return connection.Query<AfBeneficiosBancosData>(sqlList, new { filtro, like, offset, fetch }).ToList();
        }

        /// <summary>
        /// Construye el texto de filtro y su patrón LIKE. Devuelve nulos cuando no hay filtro.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el filtro normalizado y su patrón LIKE.</returns>
        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return (null, null);
            }

            return (texto, $"%{texto}%");
        }

        /// <summary>
        /// Resuelve el campo y la dirección de ordenamiento usando una lista blanca de columnas.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el campo y la dirección de ordenamiento.</returns>
        private static (string sortField, string sortOrder) ResolveSort(FiltrosLazyLoadData filtros)
        {
            // ORDER BY seguro (whitelist), nunca se concatena texto recibido del usuario.
            var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "descripcion" => "B.descripcion",
                "cheque" => "X.cheque",
                "transferencia" => "X.transferencia",
                _ => "B.id_banco"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
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
