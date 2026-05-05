using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOCobroFiadoresDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCOCobroFiadoresDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene el catalago de instituciones.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>

        public ErrorDto<List<DropDownListaGenericaModel>> Co_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            const string q = @"
                    SELECT
                        CAST(COD_INSTITUCION AS varchar(20)) AS item,
                        RTRIM(DESCRIPCION)                   AS descripcion
                    FROM dbo.INSTITUCIONES
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, q);
        }
        /// <summary>
        /// Obtiene el catalago de estados de persona.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            const string q = @"
                    SELECT
                        RTRIM(COD_ESTADO)  AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.AFI_ESTADOS_PERSONA
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, q);
        }

        /// <summary>
        /// Obtiene la lista de pendientes con lazyloading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        public ErrorDto<FrmCOCobroFiadoresPendientesListaResult> Co_CobroFiadores_Pendientes_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, FrmCOCobroFiadoresPendientesConsultaDto dto)
        {
            var filtrosSeguros = filtros ?? new FiltrosLazyLoadData();
            var filtro = NormalizarTexto(filtrosSeguros.filtro);
            var dataResult = DbHelper.ExecuteStoredProcedureList<FrmCOCobroFiadoresPendienteData>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa),
                "dbo.spCBR_Cobro_Fiadores_Pendientes",
                CrearParametrosPendientes(dto, filtro));

            if (dataResult.Code != 0)
            {
                return CrearErrorPendientes(dataResult.Description ?? "Error al consultar pendientes de cobro a fiadores.");
            }

            var data = FiltrarPendientes(dataResult.Result ?? new List<FrmCOCobroFiadoresPendienteData>(), filtro).ToList();
            var total = data.Count;
            data = OrdenarPendientes(data, filtrosSeguros).ToList();

            return CrearOkPendientes(total, Paginar(data, filtrosSeguros).ToList());
        }

        // <summary>
        /// Obtiene la lista de activos con lazyloading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        public ErrorDto<FrmCOCobroFiadoresActivosListaResult> Co_CobroFiadores_Activos_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, FrmCOCobroFiadoresActivosConsultaDto dto)
        {
            var filtrosSeguros = filtros ?? new FiltrosLazyLoadData();
            var filtro = NormalizarTexto(filtrosSeguros.filtro);
            var dataResult = DbHelper.ExecuteStoredProcedureList<FrmCOCobroFiadoresActivoData>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa),
                "dbo.spCBR_Cobro_Fiadores_Activos",
                CrearParametrosActivos(dto, filtro));

            if (dataResult.Code != 0)
            {
                return CrearErrorActivos(dataResult.Description ?? "Error al consultar activos de cobro a fiadores.");
            }

            var data = FiltrarActivos(dataResult.Result ?? new List<FrmCOCobroFiadoresActivoData>(), filtro).ToList();
            var total = data.Count;
            data = OrdenarActivos(data, filtrosSeguros).ToList();

            return CrearOkActivos(total, Paginar(data, filtrosSeguros).ToList());
        }

        // <summary>
        /// Obtiene la lista de consultas con lazyloading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        public ErrorDto<FrmCOCobroFiadoresConsultasListaResult> Co_CobroFiadores_Consultas_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, FrmCOCobroFiadoresConsultasConsultaDto dto)
        {
            var filtrosSeguros = filtros ?? new FiltrosLazyLoadData();
            var filtro = NormalizarTexto(filtrosSeguros.filtro);
            var parametrosResult = CrearParametrosConsultas(dto, filtro);

            if (parametrosResult.Code != 0)
            {
                return CrearErrorConsultas(parametrosResult.Description ?? "Error al preparar parámetros de consulta.");
            }

            var dataResult = DbHelper.ExecuteStoredProcedureList<FrmCOCobroFiadoresConsultaData>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa),
                "dbo.spCBR_Cobro_Fiadores_Consulta",
                parametrosResult.Result);

            if (dataResult.Code != 0)
            {
                return CrearErrorConsultas(dataResult.Description ?? "Error al consultar histórico de cobro a fiadores.");
            }

            var data = FiltrarConsultas(dataResult.Result ?? new List<FrmCOCobroFiadoresConsultaData>(), filtro).ToList();
            var total = data.Count;
            data = OrdenarConsultas(data, filtrosSeguros).ToList();

            return CrearOkConsultas(total, Paginar(data, filtrosSeguros).ToList());
        }

        /// <summary>
        /// Envía notificaciones de advertencias a fiadores.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>"
        /// </summary>
        /// <returns></returns>

        public ErrorDto Co_CobroFiadores_NotificaAdvertencia_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return EjecutarAccionBulk(
                CodEmpresa,
                usuario,
                dto,
                new CobroFiadoresBulkActionSpec
                {
                    Procedimiento = "dbo.spCBR_Cobro_Fiadores_Notifica",
                    ParametroOperacion = "Operacion",
                    DetalleBase = "Cobro a Fiadores: Notifica Advertencia",
                    MensajeError = "Error al notificar."
                });
        }
        /// <summary>
        /// Procesa cobros a fiadores.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>"
        /// </summary>
        /// <returns></returns>

        public ErrorDto Co_CobroFiadores_ProcesaCobros_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return EjecutarAccionBulk(
                CodEmpresa,
                usuario,
                dto,
                new CobroFiadoresBulkActionSpec
                {
                    Procedimiento = "dbo.spCBR_Cobro_Fiadores_Procesa",
                    ParametroOperacion = "Operacion",
                    DetalleBase = "Cobro a Fiadores: Procesa Cobros",
                    MensajeError = "Error al procesar."
                });
        }
        /// <summary>
        /// Cancela cobros a fiadores.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>"
        /// </summary>
        /// <returns></returns>

        public ErrorDto Co_CobroFiadores_CancelaCobro_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return EjecutarAccionBulk(
                CodEmpresa,
                usuario,
                dto,
                new CobroFiadoresBulkActionSpec
                {
                    Procedimiento = "dbo.spCbr_Cobro_Fiadores_Cancela",
                    ParametroOperacion = "FIA_Operacion",
                    DetalleBase = "Cobro a Fiadores: Cancela Cobro",
                    MensajeError = "Error al cancelar cobro.",
                    ValidarFondoDevolucion = true,
                    IncluirNotas = true
                });
        }

        private ErrorDto EjecutarAccionBulk(
            int codEmpresa,
            string usuario,
            FrmCOCobroFiadoresAccionBulkDto dto,
            CobroFiadoresBulkActionSpec spec)
        {
            var ids = ObtenerIdsValidos(dto).ToList();
            if (ids.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe Seleccionar al menos un caso!", -2);
            }

            var exec = DbHelper.WithConn(new PortalDB(_config), codEmpresa, conn =>
            {
                ValidarParametrosCobroFiador(conn, spec.ValidarFondoDevolucion);

                foreach (var id in ids)
                {
                    conn.Execute(
                        spec.Procedimiento,
                        CrearParametrosAccionBulk(spec.ParametroOperacion, id, usuario, spec.IncluirNotas),
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return true;
            });

            if (exec.Code != 0)
            {
                return DbHelper.ErrorResponse(exec.Description ?? spec.MensajeError);
            }

            RegistrarBitacora(codEmpresa, usuario, $"{spec.DetalleBase} - Casos {ids.Count}", "Procesa - WEB");
            return DbHelper.OkResponse("Ok");
        }

        private static IEnumerable<int> ObtenerIdsValidos(FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return dto?.ids?.Where(id => id > 0).Select(id => (int)id) ?? Enumerable.Empty<int>();
        }

        private static DynamicParameters CrearParametrosAccionBulk(string parametroOperacion, int id, string usuario, bool incluirNotas)
        {
            var parametros = new DynamicParameters();
            parametros.Add($"@{parametroOperacion}", id);
            parametros.Add("@Usuario", usuario);

            if (incluirNotas)
            {
                parametros.Add("@Notas", string.Empty);
            }

            return parametros;
        }

        private static void ValidarParametrosCobroFiador(SqlConnection conn, bool validarFondoDevolucion)
        {
            ValidarParametroCatalogo(conn);

            if (validarFondoDevolucion)
            {
                ValidarParametroFondoDevolucion(conn);
            }
        }

        private static void ValidarParametroCatalogo(SqlConnection conn)
        {
            const string query = @"
                    SELECT COUNT(*)
                    FROM dbo.CATALOGO
                    WHERE codigo IN (
                        SELECT valor
                        FROM dbo.CBR_PARAMETROS
                        WHERE COD_PARAMETRO = '25'
                    );";

            if (conn.QueryFirstOrDefault<int>(query) == 0)
            {
                throw new InvalidOperationException("No se encuentra configurada la Línea/Retención para Cobro a Fiador.");
            }
        }

        private static void ValidarParametroFondoDevolucion(SqlConnection conn)
        {
            const string query = @"
                    SELECT COUNT(*)
                    FROM dbo.FND_PLANES
                    WHERE COD_PLAN IN (
                        SELECT valor
                        FROM dbo.CBR_PARAMETROS
                        WHERE COD_PARAMETRO = '27'
                    );";

            if (conn.QueryFirstOrDefault<int>(query) == 0)
            {
                throw new InvalidOperationException("No se encuentra configurado el Fondo de Devolución para Cobro a Fiador, verifique los parámetros de cobro [27]");
            }
        }

        private static DynamicParameters CrearParametrosPendientes(FrmCOCobroFiadoresPendientesConsultaDto dto, string filtro)
        {
            var parametros = new DynamicParameters();
            AgregarParametroEnteroNulable(parametros, "@Institucion", dto?.institucionId ?? 0);
            AgregarParametroEnteroNulable(parametros, "@EstadoPersona", dto?.estadoPersonaId ?? 0);
            parametros.Add("@Filtro", filtro);
            parametros.Add("@NCuotas", dto?.cuotasAtrasadas ?? 2);
            parametros.Add("@Disponible", (dto?.mostrarDisponibles ?? false) ? 1 : 0);
            return parametros;
        }

        private static DynamicParameters CrearParametrosActivos(FrmCOCobroFiadoresActivosConsultaDto dto, string filtro)
        {
            var parametros = new DynamicParameters();
            AgregarParametroEnteroNulable(parametros, "@Institucion", dto?.institucionId ?? 0);
            AgregarParametroEnteroNulable(parametros, "@EstadoPersona", dto?.estadoPersonaId ?? 0);
            parametros.Add("@Filtro", filtro);
            return parametros;
        }

        private static ErrorDto<DynamicParameters> CrearParametrosConsultas(FrmCOCobroFiadoresConsultasConsultaDto dto, string filtro)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            if (!DateTime.TryParse($"{NormalizarTexto(dto?.inicio)} 00:00:00", culture, System.Globalization.DateTimeStyles.None, out var fechaInicio) ||
                !DateTime.TryParse($"{NormalizarTexto(dto?.corte)} 23:59:59", culture, System.Globalization.DateTimeStyles.None, out var fechaCorte))
            {
                return DbHelper.CreateErrorResponse("Rango de fechas inválido.", -2, new DynamicParameters());
            }

            var parametros = new DynamicParameters();
            parametros.Add("@fInicio", fechaInicio);
            parametros.Add("@fCorte", fechaCorte);
            parametros.Add("@Accion", NormalizarAccion(dto?.accion));
            parametros.Add("@Filtro", filtro);
            return DbHelper.CreateOkResponse(parametros);
        }

        private static void AgregarParametroEnteroNulable(DynamicParameters parametros, string nombre, int valor)
        {
            parametros.Add(nombre, valor == 0 ? null : valor);
        }

        private static string NormalizarAccion(string? accion)
        {
            var valor = NormalizarCodigo(accion);
            if (string.IsNullOrWhiteSpace(valor))
            {
                return "A";
            }

            valor = valor[..1];
            return valor is "A" or "C" ? valor : "A";
        }

        private static IEnumerable<FrmCOCobroFiadoresPendienteData> FiltrarPendientes(IEnumerable<FrmCOCobroFiadoresPendienteData> data, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return data;
            }

            var q = NormalizarCodigo(filtro);
            return data.Where(item => Contiene(item.codigo, q) ||
                                      Contiene(item.cedula, q) ||
                                      Contiene(item.nombre, q) ||
                                      Contiene(item.notifica_fecha, q) ||
                                      Contiene(item.estadoPersona_desc, q) ||
                                      Contiene(item.linea_desc, q) ||
                                      Contiene(item.institucion_desc, q));
        }

        private static IEnumerable<FrmCOCobroFiadoresActivoData> FiltrarActivos(IEnumerable<FrmCOCobroFiadoresActivoData> data, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return data;
            }

            var q = NormalizarCodigo(filtro);
            return data.Where(item => Contiene(item.codigo, q) ||
                                      Contiene(item.cedula, q) ||
                                      Contiene(item.nombre, q) ||
                                      Contiene(item.d_operacion, q) ||
                                      Contiene(item.d_codigo, q) ||
                                      Contiene(item.d_cedula, q) ||
                                      Contiene(item.d_nombre, q) ||
                                      Contiene(item.estadoPersona_desc, q) ||
                                      Contiene(item.linea_desc, q));
        }

        private static IEnumerable<FrmCOCobroFiadoresConsultaData> FiltrarConsultas(IEnumerable<FrmCOCobroFiadoresConsultaData> data, string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return data;
            }

            var q = NormalizarCodigo(filtro);
            return data.Where(item => Contiene(item.codigo, q) ||
                                      Contiene(item.cedula, q) ||
                                      Contiene(item.nombre, q) ||
                                      Contiene(item.acccion_tipo, q) ||
                                      Contiene(item.accion_fecha, q) ||
                                      Contiene(item.notifica_fecha, q) ||
                                      Contiene(item.estadoPersona_desc, q) ||
                                      Contiene(item.linea_desc, q) ||
                                      Contiene(item.institucion_desc, q));
        }

        private static IEnumerable<FrmCOCobroFiadoresPendienteData> OrdenarPendientes(IEnumerable<FrmCOCobroFiadoresPendienteData> data, FiltrosLazyLoadData filtros)
        {
            var asc = EsAscendente(filtros);
            var keySelector = ObtenerKeyPendientes(filtros.sortField);
            return asc ? data.OrderBy(keySelector) : data.OrderByDescending(keySelector);
        }

        private static IEnumerable<FrmCOCobroFiadoresActivoData> OrdenarActivos(IEnumerable<FrmCOCobroFiadoresActivoData> data, FiltrosLazyLoadData filtros)
        {
            var asc = EsAscendente(filtros);
            var keySelector = ObtenerKeyActivos(filtros.sortField);
            return asc ? data.OrderBy(keySelector) : data.OrderByDescending(keySelector);
        }

        private static IEnumerable<FrmCOCobroFiadoresConsultaData> OrdenarConsultas(IEnumerable<FrmCOCobroFiadoresConsultaData> data, FiltrosLazyLoadData filtros)
        {
            var asc = EsAscendente(filtros);
            var keySelector = ObtenerKeyConsultas(filtros.sortField);
            return asc ? data.OrderBy(keySelector) : data.OrderByDescending(keySelector);
        }

        private static Func<FrmCOCobroFiadoresPendienteData, object> ObtenerKeyPendientes(string? sortField)
        {
            return NormalizarTexto(sortField) switch
            {
                "codigo" => x => x.codigo ?? string.Empty,
                "cedula" => x => x.cedula ?? string.Empty,
                "nombre" => x => x.nombre ?? string.Empty,
                "n_cuota" => x => x.n_cuota,
                "mora_financiera" => x => x.mora_financiera,
                "saldo" => x => x.saldo,
                "notifica_fecha" => x => x.notifica_fecha ?? string.Empty,
                "estadoPersona_desc" => x => x.estadoPersona_desc ?? string.Empty,
                "linea_desc" => x => x.linea_desc ?? string.Empty,
                "institucion_desc" => x => x.institucion_desc ?? string.Empty,
                _ => x => x.id_solicitud
            };
        }

        private static Func<FrmCOCobroFiadoresActivoData, object> ObtenerKeyActivos(string? sortField)
        {
            return NormalizarTexto(sortField) switch
            {
                "codigo" => x => x.codigo ?? string.Empty,
                "cedula" => x => x.cedula ?? string.Empty,
                "nombre" => x => x.nombre ?? string.Empty,
                "cuota" => x => x.cuota,
                "d_operacion" => x => x.d_operacion ?? string.Empty,
                _ => x => x.id_solicitud
            };
        }

        private static Func<FrmCOCobroFiadoresConsultaData, object> ObtenerKeyConsultas(string? sortField)
        {
            return NormalizarTexto(sortField) switch
            {
                "codigo" => x => x.codigo ?? string.Empty,
                "cedula" => x => x.cedula ?? string.Empty,
                "nombre" => x => x.nombre ?? string.Empty,
                "n_cuota" => x => x.n_cuota,
                "mora_financiera" => x => x.mora_financiera,
                "saldo_original" => x => x.saldo_original,
                "saldo_actual" => x => x.saldo_actual,
                "accion_fecha" => x => x.accion_fecha ?? string.Empty,
                _ => x => x.id_solicitud
            };
        }

        private static IEnumerable<T> Paginar<T>(IEnumerable<T> data, FiltrosLazyLoadData filtros)
        {
            var pagina = filtros.pagina;
            var paginacion = filtros.paginacion;

            if (pagina == 0 || paginacion == 0)
            {
                return data;
            }

            var offset = Math.Max(0, pagina);
            var fetch = paginacion <= 0 ? 30 : paginacion;
            return data.Skip(offset).Take(fetch);
        }

        private static bool Contiene(string? valor, string filtro)
        {
            return NormalizarCodigo(valor).Contains(filtro);
        }

        private static bool EsAscendente(FiltrosLazyLoadData filtros)
        {
            return filtros.sortOrder != 0;
        }

        private static string NormalizarCodigo(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpper();
        }

        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private static ErrorDto<FrmCOCobroFiadoresPendientesListaResult> CrearOkPendientes(int total, List<FrmCOCobroFiadoresPendienteData> lista)
        {
            return DbHelper.CreateOkResponse(new FrmCOCobroFiadoresPendientesListaResult
            {
                total = total,
                lista = lista
            });
        }

        private static ErrorDto<FrmCOCobroFiadoresPendientesListaResult> CrearErrorPendientes(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new FrmCOCobroFiadoresPendientesListaResult
                {
                    total = 0,
                    lista = new List<FrmCOCobroFiadoresPendienteData>()
                });
        }

        private static ErrorDto<FrmCOCobroFiadoresActivosListaResult> CrearOkActivos(int total, List<FrmCOCobroFiadoresActivoData> lista)
        {
            return DbHelper.CreateOkResponse(new FrmCOCobroFiadoresActivosListaResult
            {
                total = total,
                lista = lista
            });
        }

        private static ErrorDto<FrmCOCobroFiadoresActivosListaResult> CrearErrorActivos(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new FrmCOCobroFiadoresActivosListaResult
                {
                    total = 0,
                    lista = new List<FrmCOCobroFiadoresActivoData>()
                });
        }

        private static ErrorDto<FrmCOCobroFiadoresConsultasListaResult> CrearOkConsultas(int total, List<FrmCOCobroFiadoresConsultaData> lista)
        {
            return DbHelper.CreateOkResponse(new FrmCOCobroFiadoresConsultasListaResult
            {
                total = total,
                lista = lista
            });
        }

        private static ErrorDto<FrmCOCobroFiadoresConsultasListaResult> CrearErrorConsultas(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new FrmCOCobroFiadoresConsultasListaResult
                {
                    total = 0,
                    lista = new List<FrmCOCobroFiadoresConsultaData>()
                });
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
    internal sealed class CobroFiadoresBulkActionSpec
    {
        public string Procedimiento { get; init; } = string.Empty;
        public string ParametroOperacion { get; init; } = string.Empty;
        public string DetalleBase { get; init; } = string.Empty;
        public string MensajeError { get; init; } = string.Empty;
        public bool ValidarFondoDevolucion { get; init; }
        public bool IncluirNotas { get; init; }
    }
}
