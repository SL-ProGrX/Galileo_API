using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.DataBaseTier
{
    
    public class FrmCajasRoeAnularDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasRoeAnularDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        private static ErrorDto<CajasRoeAnularLista> CrearRespuestaLista() => new()
        {
            Code = 0,
            Description = "Ok",
            Result = CrearListaVacia()
        };

        private static CajasRoeAnularLista CrearListaVacia() => new()
        {
            total = 0,
            lista = new List<CajasRoeAnularData>()
        };

        private static object CrearParametrosConsulta(FiltrosCajasRoeAnularData filtros, int pagina, int paginacion) => new
        {
            fecha_inicio = filtros.fecha_desde,
            fecha_fin = filtros.fecha_hasta,
            cedula = filtros.IDENTIFICACION_DEPO,
            nombre = filtros.NOMBRE_DEPO,
            filtro = filtros.filtro,
            pagina,
            paginacion
        };

        private static void AsignarError(ErrorDto<CajasRoeAnularLista> result, Exception ex)
        {
            result.Code = -1;
            result.Description = ex.Message;
            result.Result = CrearListaVacia();
        }

        private static (int pagina, int paginacion) ObtenerPaginacion(FiltrosCajasRoeAnularData filtros) =>
            (Math.Max(0, filtros.pagina.GetValueOrDefault()), filtros.paginacion.GetValueOrDefault() <= 0 ? 10 : filtros.paginacion.GetValueOrDefault());

        private const string QueryBase = @"select ID_ROE, TIPOROE, rtrim(CEDULA_ASO) as 'CEDULA_ASO', IDENTIFICACION_DEPO, NOMBRE_DEPO, FECHA, USUARIO, MONTO_LOCAL, MONTO_DOL,TIPO_CAMBIO
                                , REGISTRO_FECHA, REGISTRO_USUARIO, ACTUALIZA_FECHA, ACTUALIZA_USUARIO, USUARIO_ANULACION, FECHA_ANULACION, OBSERV_ANULACION, IMPRIME_FECHA, IMPRIME_USUARIO
                                , ISNULL(ID_SESION,'') AS 'ID_SESION', ESTADO
                                From CAJAS_ROE WHERE ESTADO = 'A'  ";

        private static string AgregarFiltroSiAplica(string query, bool condicion, string fragmento) => condicion ? string.Concat(query, fragmento) : query;

        private static string ObtenerOrderBySeguro(FiltrosCajasRoeAnularData filtros)
        {
            bool asc = (filtros.sortOrder ?? 0) != 0;
            string campo = (filtros.sortField ?? string.Empty).Trim().ToUpperInvariant();

            var camposValidos = new Dictionary<string, string>
            {
                { "TIPOROE", "TIPOROE" },
                { "CEDULA_ASO", "CEDULA_ASO" },
                { "IDENTIFICACION_DEPO", "IDENTIFICACION_DEPO" },
                { "NOMBRE_DEPO", "NOMBRE_DEPO" },
                { "FECHA", "FECHA" },
                { "USUARIO", "USUARIO" },
                { "MONTO_LOCAL", "MONTO_LOCAL" },
                { "MONTO_DOL", "MONTO_DOL" },
                { "TIPO_CAMBIO", "TIPO_CAMBIO" },
                { "REGISTRO_FECHA", "REGISTRO_FECHA" },
                { "REGISTRO_USUARIO", "REGISTRO_USUARIO" },
                { "ACTUALIZA_FECHA", "ACTUALIZA_FECHA" },
                { "ACTUALIZA_USUARIO", "ACTUALIZA_USUARIO" },
                { "USUARIO_ANULACION", "USUARIO_ANULACION" },
                { "FECHA_ANULACION", "FECHA_ANULACION" },
                { "OBSERV_ANULACION", "OBSERV_ANULACION" },
                { "IMPRIME_FECHA", "IMPRIME_FECHA" },
                { "IMPRIME_USUARIO", "IMPRIME_USUARIO" },
                { "ID_SESION", "ID_SESION" },
                { "ESTADO", "ESTADO" }
            };

            string campoOrden = camposValidos.ContainsKey(campo) ? camposValidos[campo] : "ID_ROE";
            string direccion = asc ? "ASC" : "DESC";
            return $" order by {campoOrden} {direccion} ";
        }

        private static string ConstruirQuery(FiltrosCajasRoeAnularData filtros)
        {
            var filtrosQuery = new[]
            {
                (!filtros.rango_fechas, @" and Fecha between @fecha_inicio and @fecha_fin "),
                (!string.IsNullOrWhiteSpace(filtros.IDENTIFICACION_DEPO), @" and ( Cedula_Aso like '%'+@cedula+'%' or IDENTIFICACION_DEPO like '%'+@cedula+'%' )"),
                (!string.IsNullOrWhiteSpace(filtros.NOMBRE_DEPO), @" and NOMBRE_DEPO like '%'+@nombre+'%'"),
                (!string.IsNullOrWhiteSpace(filtros.filtro), @" and (
                            CAST(ID_ROE AS varchar(50)) like '%'+@filtro+'%'
                            or TIPOROE like '%'+@filtro+'%'
                            or CEDULA_ASO like '%'+@filtro+'%'
                            or IDENTIFICACION_DEPO like '%'+@filtro+'%'
                            or NOMBRE_DEPO like '%'+@filtro+'%') ")
            };

            var query = filtrosQuery.Aggregate(QueryBase, (actual, item) => AgregarFiltroSiAplica(actual, item.Item1, item.Item2));
            query = string.Concat(query, ObtenerOrderBySeguro(filtros));
            return string.Concat(query, @"
                             OFFSET @pagina ROWS
                             FETCH NEXT @paginacion ROWS ONLY");
        }

        /// <summary>
        /// Consulta los ROE de cajas para anular
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CajasRoeAnularLista> CajasRoeAnular_Obtener(int CodEmpresa, FiltrosCajasRoeAnularData filtros)
        {
            var result = CrearRespuestaLista();
            result.Result ??= CrearListaVacia();

            try
            {
                filtros ??= new FiltrosCajasRoeAnularData();
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                var totalQuery = "select COUNT(1) from CAJAS_ROE";
                result.Result.total = connection.Query<int>(totalQuery).FirstOrDefault();

                var (pagina, paginacion) = ObtenerPaginacion(filtros);
                var query = ConstruirQuery(filtros);

                result.Result.lista = connection.Query<CajasRoeAnularData>(
                    query,
                    CrearParametrosConsulta(filtros, pagina, paginacion)).ToList();
            }
            catch (Exception ex)
            {
                AsignarError(result, ex);
            }

            return result;
        }

        /// <summary>
        /// Anula un ROE de caja
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="roe"></param>
        /// <param name="notas"></param>
        /// <returns></returns>
        public ErrorDto CajasRoeAnular_Anular(int CodEmpresa, string usuario, string roe, string notas)
        {
            const string sql = "spCajas_ROE_Anula";
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = DbHelper.ExecuteStoredProcedureSingle<int>(
                connectionString,
                sql,
                0,
                new
                {
                    roe,
                    notas,
                    usuario
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(result.Description ?? "Error desconocido", result.Code ?? -1);
        }
    }
}