using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTipoEsDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTipoEsDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTipoEsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de tipos de entrada y salida.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static TipoESList CrearListaVacia() => new()
        {
            Total = 0,
            Lista = new List<TipoEsDto>()
        };

        /// <summary>
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Deserializa el filtro del listado de tipos de entrada y salida.
        /// </summary>
        /// <param name="filtros">Cadena JSON con filtros.</param>
        /// <returns>Filtro tipado inicializado.</returns>
        private static TipoESFiltros ObtenerFiltros(string filtros)
        {
            return JsonConvert.DeserializeObject<TipoESFiltros>(filtros) ?? new TipoESFiltros();
        }

        /// <summary>
        /// Agrega filtro de búsqueda al query del listado.
        /// </summary>
        /// <param name="filtro">Texto filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroListado(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(@" WHERE T.cod_entsal LIKE @Filtro
                                   OR T.descripcion LIKE @Filtro
                                   OR T.cod_cuenta LIKE @Filtro
                                   OR C.descripcion LIKE @Filtro ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH al query del listado.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (!pagina.HasValue || !paginacion.HasValue)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

        /// <summary>
        /// Normaliza el código de cuenta quitando guiones.
        /// </summary>
        /// <param name="codCuenta">Código de cuenta.</param>
        /// <returns>Código de cuenta normalizado.</returns>
        private static string NormalizarCodCuenta(string? codCuenta)
        {
            return (codCuenta ?? string.Empty).Replace("-", string.Empty);
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene los tipos de entrada y salida paginados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Cadena JSON con filtros.</param>
        /// <returns>Listado de tipos de entrada y salida.</returns>
        public ErrorDto<TipoESList> TipoES_Obtener(int CodEmpresa, string filtros)
        {
            var filtro = ObtenerFiltros(filtros);
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = CrearListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) FROM pv_entrada_salida T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta");

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder(@"SELECT T.cod_entsal,
                                                             T.descripcion as descripcion,
                                                             T.tipo,
                                                             T.cod_cuenta,
                                                             T.activo,
                                                             C.descripcion AS ctaDesc,
                                                             T.mancomunado
                                                      FROM pv_entrada_salida T
                                                      LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta");

                AgregarFiltroListado(filtro.filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY T.cod_entsal ");
                AgregarPaginacion(filtro.pagina, filtro.paginacion, queryBuilder, parametros);

                respuesta.Lista = connection.Query<TipoEsDto>(queryBuilder.ToString(), parametros)
                                          .GroupBy(x => x.Cod_Entsal)
                                          .Select(x => x.First())
                                          .ToList();

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tipos de entrada y salida.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Busca tipos de transacciones o movimientos por tipo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Tipo">Tipo de movimiento.</param>
        /// <returns>Listado de tipos de entrada y salida.</returns>
        public ErrorDto<List<TipoEsDto>> TipoES_Buscar(int CodEmpresa, string Tipo)
        {
            var result = DbHelper.ExecuteListQuery<TipoEsDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT DISTINCT T.cod_entsal,
                                  T.descripcion as descripcion,
                                  T.tipo,
                                  T.cod_cuenta,
                                  T.activo,
                                  C.descripcion AS ctaDesc
                  FROM pv_entrada_salida T
                  LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta
                  WHERE T.tipo = @Tipo
                  ORDER BY T.cod_entsal",
                new { Tipo });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TipoEsDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al buscar tipos de entrada y salida.", result.Code.GetValueOrDefault(-1), new List<TipoEsDto>());
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza un tipo de transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del tipo de transacción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoES_Actualizar(int CodEmpresa, TipoEsDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE pv_entrada_salida
                  SET descripcion = @Descripcion,
                      cod_cuenta = @Cod_Cuenta,
                      tipo = @Tipo,
                      activo = @Activo,
                      mancomunado = @Mancomunado
                  WHERE cod_Entsal = @Cod_Entsal",
                new
                {
                    request.Cod_Entsal,
                    request.Descripcion,
                    request.Tipo,
                    Cod_Cuenta = NormalizarCodCuenta(request.Cod_Cuenta),
                    request.Activo,
                    request.Mancomunado
                });

            return CrearRespuestaNonQuery(result, "Registro actualizado correctamente", "Error al actualizar el tipo de transacción.");
        }

        /// <summary>
        /// Inserta un nuevo tipo de transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del tipo de transacción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoES_Insertar(int CodEmpresa, TipoEsDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT INTO pv_entrada_salida(cod_Entsal, descripcion, tipo, cod_cuenta, activo, mancomunado)
                  VALUES(@Cod_Entsal, @Descripcion, @Tipo, @Cod_Cuenta, @Activo, @Mancomunado)",
                new
                {
                    request.Cod_Entsal,
                    request.Descripcion,
                    request.Tipo,
                    Cod_Cuenta = NormalizarCodCuenta(request.Cod_Cuenta),
                    request.Activo,
                    request.Mancomunado
                });

            return CrearRespuestaNonQuery(result, "Registro agregado correctamente", "Error al insertar el tipo de transacción.");
        }

        /// <summary>
        /// Elimina un tipo de transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codTiposES">Código del tipo de transacción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoES_Eliminar(int CodEmpresa, string codTiposES)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_entrada_salida WHERE cod_Entsal = @Cod_Entsal",
                new { Cod_Entsal = codTiposES });

            return CrearRespuestaNonQuery(result, "Registro eliminado correctamente", "Error al eliminar el tipo de transacción.");
        }

        #endregion
    }
}