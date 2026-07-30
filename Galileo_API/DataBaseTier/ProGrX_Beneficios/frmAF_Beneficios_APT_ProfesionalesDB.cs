using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Profesionales Apremiantes de Beneficios (frmAF_Beneficios_APT_Profesionales).
    /// Consultas aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneficiosAptProfesionalesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosAptProfesionalesDB(IConfiguration config)
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

        private const string SqlProfesionalesSelect = @"
SELECT ID_PROFESIONAL AS id_profesional, IDENTIFICACION AS identificacion,
       NOMBRE AS nombre, USUARIO AS usuario, ACTIVO AS activo,
       REGISTRO_FECHA AS registro_fecha, REGISTRO_USUARIO AS registro_usuario,
       MODIFICA_FECHA AS modifica_fecha, MODIFICA_USUARIO AS modifica_usuario
FROM AFI_BENE_APT_PROFESIONALES
";

        private const string SqlProfesionalesWhere = @"
WHERE (@filtro IS NULL)
   OR (CONVERT(varchar(20), ID_PROFESIONAL) LIKE @like)
   OR (IDENTIFICACION LIKE @like)
   OR (NOMBRE LIKE @like)
   OR (USUARIO LIKE @like)
";

        private const string SqlProfesionalesCount = @"
SELECT COUNT(1)
FROM AFI_BENE_APT_PROFESIONALES
" + SqlProfesionalesWhere;

        /// <summary>
        /// Obtiene la lista de profesionales apremiantes con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de profesionales y total de registros.</returns>
        public ErrorDto<BeneAptProfesionalesDataLista> AfBeneAptPro_Obtener(int CodCliente, FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = QueryProfesionales(connection, filtros, true, out var total);
                return new BeneAptProfesionalesDataLista { total = total, lista = lista };
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<BeneAptProfesionalesDataLista>(
                    "AfBeneAptPro_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Exporta la lista de profesionales aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de profesionales sin paginar.</returns>
        public ErrorDto<List<BeneAptProfesionalesData>> AfBeneAptPro_Exportar(int CodCliente, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                QueryProfesionales(connection, filtros, false, out _));
        }

        /// <summary>
        /// Obtiene la lista de usuarios activos para asignar al profesional.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de usuarios con su código y descripción.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfBeneAptProUsuarios_Obtener(int CodCliente)
        {
            const string sql = "SELECT Nombre AS item, descripcion FROM usuarios WHERE estado = 'A' ORDER BY Nombre";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(CreatePortalDb(), CodCliente, sql);
        }

        // ==========================
        // Helpers privados de consulta
        // ==========================

        /// <summary>
        /// Consulta los profesionales aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de profesionales.</returns>
        private static List<BeneAptProfesionalesData> QueryProfesionales(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlProfesionalesCount, new { filtro, like });

            var sqlList = SqlProfesionalesSelect + SqlProfesionalesWhere + $"\nORDER BY {sortField} {sortOrder}";

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

            return connection.Query<BeneAptProfesionalesData>(sqlList, new { filtro, like, offset, fetch }).ToList();
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
                "identificacion" => "IDENTIFICACION",
                "nombre" => "NOMBRE",
                "usuario" => "USUARIO",
                "activo" => "ACTIVO",
                _ => "ID_PROFESIONAL"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }
    }
}
