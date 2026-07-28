using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de la Tabla de Coberturas/Aplicación Fosol (frmFSL_TablaCoberturas).
    /// </summary>
    public partial class FrmFslTablaCoberturasDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslTablaCoberturasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la tabla de aplicación (coberturas) por tipo con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con tipo, filtro y paginación.</param>
        /// <returns>Lista de coberturas y total.</returns>
        public ErrorDto<FslTablaAplicacionDataLista> TablaAplicacion_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FslTablaAplicacionFiltros>(filtros) ?? new FslTablaAplicacionFiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslTablaAplicacionDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_TABLAS_APLICACION";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT TIPO AS tipo, LINEA AS linea, MES_INICIO AS mes_inicio, MES_CORTE AS mes_corte, COBERTURA AS cobertura
                                     FROM FSL_TABLAS_APLICACION
                                     WHERE TIPO = @tipo
                                       AND (@like IS NULL OR TIPO LIKE @like OR LINEA LIKE @like)
                                     ORDER BY LINEA
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.coberturas = connection.Query<FslTablaAplicacionData>(sql, new { tipo = filtro.tipo, like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FslTablaAplicacionDataLista>("TablaAplicacion_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Guarda una cobertura (inserta si no existe, o actualiza si ya existe).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="aplicacion">Datos de la cobertura.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Cobertura_Guardar(int CodCliente, FslTablaAplicacionData aplicacion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return ExisteCobertura(connection, aplicacion.tipo, aplicacion.linea)
                    ? TablaAplicacion_Actualizar(connection, aplicacion)
                    : TablaAplicacion_Insertar(connection, aplicacion);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe una cobertura por tipo y línea.
        /// </summary>
        private static bool ExisteCobertura(SqlConnection connection, string tipo, int linea)
        {
            const string sql = "SELECT ISNULL(COUNT(*), 0) FROM FSL_TABLAS_APLICACION WHERE Tipo = @tipo AND linea = @linea";
            return connection.QueryFirstOrDefault<int>(sql, new { tipo, linea }) > 0;
        }

        /// <summary>
        /// Inserta una cobertura calculando su consecutivo por tipo.
        /// </summary>
        private static ErrorDto TablaAplicacion_Insertar(SqlConnection connection, FslTablaAplicacionData aplicacion)
        {
            var linea = connection.QueryFirstOrDefault<int>(
                "SELECT ISNULL(MAX(Linea), 0) + 1 AS Ultimo FROM FSL_TABLAS_APLICACION WHERE Tipo = @tipo", new { aplicacion.tipo });

            const string sql = @"INSERT INTO FSL_TABLAS_APLICACION
                                    (Tipo, Linea, Mes_Inicio, Mes_Corte, Cobertura, registra_fecha, registra_usuario)
                                 VALUES
                                    (@tipo, @linea, @mes_inicio, @mes_corte, @cobertura, GETDATE(), @registra_usuario)";

            connection.Execute(sql, new
            {
                aplicacion.tipo,
                linea,
                aplicacion.mes_inicio,
                aplicacion.mes_corte,
                aplicacion.cobertura,
                aplicacion.registra_usuario
            });

            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Actualiza una cobertura existente.
        /// </summary>
        private static ErrorDto TablaAplicacion_Actualizar(SqlConnection connection, FslTablaAplicacionData aplicacion)
        {
            const string sql = @"UPDATE FSL_TABLAS_APLICACION
                                 SET Mes_Inicio = @mes_inicio, Mes_Corte = @mes_corte, Cobertura = @cobertura
                                 WHERE Tipo = @tipo AND Linea = @linea";

            connection.Execute(sql, new { aplicacion.mes_inicio, aplicacion.mes_corte, aplicacion.cobertura, aplicacion.tipo, aplicacion.linea });
            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Elimina una cobertura por tipo y línea.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo">Tipo de tabla.</param>
        /// <param name="linea">Línea de la cobertura.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TablaAplicacion_Eliminar(int CodCliente, string tipo, int linea)
        {
            const string sql = "DELETE FSL_TABLAS_APLICACION WHERE Tipo = @tipo AND Linea = @linea";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { tipo, linea });
        }
    }
}
