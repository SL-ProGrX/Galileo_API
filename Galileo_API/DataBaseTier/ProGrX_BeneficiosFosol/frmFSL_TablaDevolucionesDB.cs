using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de la Tabla de Devoluciones Fosol (frmFSL_TablaDevoluciones).
    /// </summary>
    public partial class FrmFslTablaDevolucionesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslTablaDevolucionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene el catálogo de tipos de garantía.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de garantías.</returns>
        public ErrorDto<List<FslGarantiasData>> FslGarantias_Obtener(int CodCliente)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT Garantia AS item, RTRIM(Garantia) + ' - ' + RTRIM(descripcion) AS descripcion
                                     FROM CRD_Garantia_Tipos ORDER BY Garantia";
                return connection.Query<FslGarantiasData>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslGarantiasData>>("FslGarantias_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene la lista de devoluciones con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por código o descripción de garantía.</param>
        /// <returns>Lista de devoluciones.</returns>
        public ErrorDto<FslDevolucionesDataLista> FslDevoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslDevolucionesDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_TABLA_DEVOLUCIONES";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT Fsl.COD_DEVOLUCION AS cod_devolucion, Fsl.Fecha_Inicio AS fecha_inicio, Fsl.Fecha_Corte AS fecha_corte,
                                            RTRIM(Fsl.Garantia) AS garantia, BASE_APLICACION AS _base,
                                            Fsl.Porcentaje AS porcentaje, Fsl.Registro_Fecha AS registro_fecha, Fsl.Registro_Usuario AS registro_usuario
                                     FROM FSL_TABLA_DEVOLUCIONES Fsl
                                     INNER JOIN CRD_Garantia_Tipos Gar ON Fsl.Garantia = Gar.Garantia
                                     WHERE (@like IS NULL OR Fsl.COD_DEVOLUCION LIKE @like OR Gar.descripcion LIKE @like)
                                     ORDER BY Fsl.Fecha_Inicio
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.devoluciones = connection.Query<FslDevolucionesData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FslDevolucionesDataLista>("FslDevoluciones_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Guarda una devolución (inserta si no existe, o actualiza si ya existe).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="devolucion">Datos de la devolución.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ParametroDevolucion_Guardar(int CodCliente, FslDevolucionesData devolucion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return Devolucion_Existe(connection, devolucion.cod_devolucion)
                    ? FslDevolucion_Actualizar(connection, devolucion)
                    : FslDevolucion_Insertar(connection, devolucion);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe una devolución por código.
        /// </summary>
        private static bool Devolucion_Existe(SqlConnection connection, int cod_devolucion)
        {
            const string sql = "SELECT ISNULL(COUNT(*), 0) FROM FSL_TABLA_DEVOLUCIONES WHERE COD_DEVOLUCION = @cod_devolucion";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_devolucion }) > 0;
        }

        /// <summary>
        /// Inserta una devolución calculando su consecutivo.
        /// </summary>
        private static ErrorDto FslDevolucion_Insertar(SqlConnection connection, FslDevolucionesData devolucion)
        {
            var ultimo = connection.QueryFirstOrDefault<int>("SELECT COALESCE(MAX(COD_DEVOLUCION), 0) + 1 AS Ultimo FROM FSL_TABLA_DEVOLUCIONES");

            const string sql = @"INSERT INTO FSL_TABLA_DEVOLUCIONES
                                    (COD_DEVOLUCION, Fecha_Inicio, Fecha_Corte, Garantia, Base_Aplicacion, Porcentaje, registro_fecha, registro_usuario)
                                 VALUES
                                    (@ultimo, @fecha_inicio, @fecha_corte, @garantia, @base, @porcentaje, GETDATE(), @registro_usuario)";

            connection.Execute(sql, new
            {
                ultimo,
                devolucion.fecha_inicio,
                devolucion.fecha_corte,
                devolucion.garantia,
                @base = devolucion._base,
                devolucion.porcentaje,
                devolucion.registro_usuario
            });

            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Actualiza una devolución existente.
        /// </summary>
        private static ErrorDto FslDevolucion_Actualizar(SqlConnection connection, FslDevolucionesData devolucion)
        {
            const string sql = @"UPDATE FSL_TABLA_DEVOLUCIONES
                                 SET Fecha_Inicio = @fecha_inicio, Fecha_Corte = @fecha_corte, Garantia = @garantia,
                                     Base_Aplicacion = @base, Porcentaje = @porcentaje
                                 WHERE COD_DEVOLUCION = @cod_devolucion";

            connection.Execute(sql, new
            {
                devolucion.fecha_inicio,
                devolucion.fecha_corte,
                devolucion.garantia,
                @base = devolucion._base,
                devolucion.porcentaje,
                devolucion.cod_devolucion
            });

            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Elimina una devolución.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_devolucion">Código de la devolución.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslDevolucion_Eliminar(int CodCliente, int cod_devolucion)
        {
            const string sql = "DELETE FROM FSL_TABLA_DEVOLUCIONES WHERE COD_DEVOLUCION = @cod_devolucion";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_devolucion });
        }
    }
}
