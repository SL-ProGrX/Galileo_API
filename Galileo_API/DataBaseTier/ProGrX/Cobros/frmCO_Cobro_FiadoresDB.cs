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

        public ErrorDto<FrmCOCobroFiadoresPendientesListaResult> Co_CobroFiadores_Pendientes_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,FrmCOCobroFiadoresPendientesConsultaDto dto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<FrmCOCobroFiadoresPendientesListaResult>()
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmCOCobroFiadoresPendientesListaResult()
                {
                    total = 0,
                    lista = new List<FrmCOCobroFiadoresPendienteData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                int pInstitucion = dto?.institucionId ?? 0;
                int pEstadoPersona = dto?.estadoPersonaId ?? 0;
                int pCuotas = dto?.cuotasAtrasadas ?? 2;
                int pDisponibles = (dto?.mostrarDisponibles ?? false) ? 1 : 0;

                string filtro = (filtros?.filtro ?? "").Trim();
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 0;

                bool exportAll = pagina == 0 || paginacion == 0;

                string sortFieldIn = (filtros?.sortField ?? "").Trim();
                string sortField = sortFieldIn switch
                {
                    "codigo" => "codigo",
                    "cedula" => "cedula",
                    "nombre" => "nombre",
                    "n_cuota" => "n_cuota",
                    "mora_financiera" => "mora_financiera",
                    "saldo" => "saldo",
                    "notifica_fecha" => "notifica_fecha",
                    "estadoPersona_desc" => "estadoPersona_desc",
                    "linea_desc" => "linea_desc",
                    "institucion_desc" => "institucion_desc",
                    _ => "id_solicitud"
                };

                string sortOrder = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";

                var p = new DynamicParameters();

                if (pInstitucion == 0) p.Add("@Institucion", null);
                else p.Add("@Institucion", pInstitucion);

                if (pEstadoPersona == 0) p.Add("@EstadoPersona", null);
                else p.Add("@EstadoPersona", pEstadoPersona);

                p.Add("@Filtro", filtro);
                p.Add("@NCuotas", pCuotas);
                p.Add("@Disponible", pDisponibles);


                var data = connection.Query<FrmCOCobroFiadoresPendienteData>(
                    "dbo.spCBR_Cobro_Fiadores_Pendientes",
                    p,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();


                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    string q = filtro.Trim().ToUpper();
                    data = data.Where(x =>
                        ((x.codigo ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.cedula ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.nombre ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.notifica_fecha ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.estadoPersona_desc ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.linea_desc ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.institucion_desc ?? "").Trim().ToUpper().Contains(q))
                    ).ToList();
                }

                result.Result.total = data.Count;

   
                bool asc = (sortOrder ?? "ASC").Trim().ToUpper() == "ASC";
                if (sortField == "codigo") data = asc ? data.OrderBy(x => x.codigo ?? "").ToList() : data.OrderByDescending(x => x.codigo ?? "").ToList();
                else if (sortField == "cedula") data = asc ? data.OrderBy(x => x.cedula ?? "").ToList() : data.OrderByDescending(x => x.cedula ?? "").ToList();
                else if (sortField == "nombre") data = asc ? data.OrderBy(x => x.nombre ?? "").ToList() : data.OrderByDescending(x => x.nombre ?? "").ToList();
                else if (sortField == "n_cuota") data = asc ? data.OrderBy(x => x.n_cuota).ToList() : data.OrderByDescending(x => x.n_cuota).ToList();
                else if (sortField == "mora_financiera") data = asc ? data.OrderBy(x => x.mora_financiera).ToList() : data.OrderByDescending(x => x.mora_financiera).ToList();
                else if (sortField == "saldo") data = asc ? data.OrderBy(x => x.saldo).ToList() : data.OrderByDescending(x => x.saldo).ToList();
                else if (sortField == "notifica_fecha") data = asc ? data.OrderBy(x => x.notifica_fecha ?? "").ToList() : data.OrderByDescending(x => x.notifica_fecha ?? "").ToList();
                else if (sortField == "estadoPersona_desc") data = asc ? data.OrderBy(x => x.estadoPersona_desc ?? "").ToList() : data.OrderByDescending(x => x.estadoPersona_desc ?? "").ToList();
                else if (sortField == "linea_desc") data = asc ? data.OrderBy(x => x.linea_desc ?? "").ToList() : data.OrderByDescending(x => x.linea_desc ?? "").ToList();
                else if (sortField == "institucion_desc") data = asc ? data.OrderBy(x => x.institucion_desc ?? "").ToList() : data.OrderByDescending(x => x.institucion_desc ?? "").ToList();
                else data = asc ? data.OrderBy(x => x.id_solicitud).ToList() : data.OrderByDescending(x => x.id_solicitud).ToList();

                if (exportAll)
                {
                    result.Result.lista = data;
                    return result;
                }

                int offset = pagina;
                int fetch = paginacion;
                if (offset < 0) offset = 0;
                if (fetch <= 0) fetch = 30;

                result.Result.lista = data.Skip(offset).Take(fetch).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<FrmCOCobroFiadoresPendienteData>();
            }

            return result;
        }

        // <summary>
        /// Obtiene la lista de activos con lazyloading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        public ErrorDto<FrmCOCobroFiadoresActivosListaResult> Co_CobroFiadores_Activos_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,FrmCOCobroFiadoresActivosConsultaDto dto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<FrmCOCobroFiadoresActivosListaResult>()
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmCOCobroFiadoresActivosListaResult()
                {
                    total = 0,
                    lista = new List<FrmCOCobroFiadoresActivoData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                int pInstitucion = dto?.institucionId ?? 0;
                int pEstadoPersona = dto?.estadoPersonaId ?? 0;

                string filtro = (filtros?.filtro ?? "").Trim();
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 0;

                bool exportAll = pagina == 0 || paginacion == 0;

                string sortFieldIn = (filtros?.sortField ?? "").Trim();
                string sortField = sortFieldIn switch
                {
                    "codigo" => "codigo",
                    "cedula" => "cedula",
                    "nombre" => "nombre",
                    "cuota" => "cuota",
                    "d_operacion" => "d_operacion",
                    _ => "id_solicitud"
                };

                string sortOrder = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";

                var p = new DynamicParameters();

                if (pInstitucion == 0) p.Add("@Institucion", null);
                else p.Add("@Institucion", pInstitucion);

                if (pEstadoPersona == 0) p.Add("@EstadoPersona", null);
                else p.Add("@EstadoPersona", pEstadoPersona);

                p.Add("@Filtro", filtro);

                var data = connection.Query<FrmCOCobroFiadoresActivoData>(
                    "dbo.spCBR_Cobro_Fiadores_Activos",
                    p,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    string q = filtro.Trim().ToUpper();
                    data = data.Where(x =>
                        ((x.codigo ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.cedula ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.nombre ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.d_operacion ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.d_codigo ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.d_cedula ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.d_nombre ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.estadoPersona_desc ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.linea_desc ?? "").Trim().ToUpper().Contains(q))
                    ).ToList();
                }

                result.Result.total = data.Count;

                bool asc = (sortOrder ?? "ASC").Trim().ToUpper() == "ASC";
                if (sortField == "codigo") data = asc ? data.OrderBy(x => x.codigo ?? "").ToList() : data.OrderByDescending(x => x.codigo ?? "").ToList();
                else if (sortField == "cedula") data = asc ? data.OrderBy(x => x.cedula ?? "").ToList() : data.OrderByDescending(x => x.cedula ?? "").ToList();
                else if (sortField == "nombre") data = asc ? data.OrderBy(x => x.nombre ?? "").ToList() : data.OrderByDescending(x => x.nombre ?? "").ToList();
                else if (sortField == "cuota") data = asc ? data.OrderBy(x => x.cuota).ToList() : data.OrderByDescending(x => x.cuota).ToList();
                else if (sortField == "d_operacion") data = asc ? data.OrderBy(x => x.d_operacion ?? "").ToList() : data.OrderByDescending(x => x.d_operacion ?? "").ToList();
                else data = asc ? data.OrderBy(x => x.id_solicitud).ToList() : data.OrderByDescending(x => x.id_solicitud).ToList();

                if (exportAll)
                {
                    result.Result.lista = data;
                    return result;
                }

                int offset = pagina;
                int fetch = paginacion;
                if (offset < 0) offset = 0;
                if (fetch <= 0) fetch = 30;

                result.Result.lista = data.Skip(offset).Take(fetch).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<FrmCOCobroFiadoresActivoData>();
            }

            return result;
        }

        // <summary>
        /// Obtiene la lista de consultas con lazyloading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="dto"></param>
        /// <returns></returns>

        public ErrorDto<FrmCOCobroFiadoresConsultasListaResult> Co_CobroFiadores_Consultas_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,FrmCOCobroFiadoresConsultasConsultaDto dto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<FrmCOCobroFiadoresConsultasListaResult>()
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmCOCobroFiadoresConsultasListaResult()
                {
                    total = 0,
                    lista = new List<FrmCOCobroFiadoresConsultaData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string inicioIn = (dto?.inicio ?? "").Trim();
                string corteIn = (dto?.corte ?? "").Trim();

                DateTime dtInicio = Convert.ToDateTime(inicioIn + " 00:00:00");
                DateTime dtCorte = Convert.ToDateTime(corteIn + " 23:59:59");

                string accion = (dto?.accion ?? "A").Trim().ToUpper();
                accion = string.IsNullOrWhiteSpace(accion) ? "A" : accion.Substring(0, 1);
                if (accion != "A" && accion != "C") accion = "A";

                string filtro = (filtros?.filtro ?? "").Trim();
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 0;

                bool exportAll = pagina == 0 || paginacion == 0;

                string sortFieldIn = (filtros?.sortField ?? "").Trim();
                string sortField = sortFieldIn switch
                {
                    "codigo" => "codigo",
                    "cedula" => "cedula",
                    "nombre" => "nombre",
                    "n_cuota" => "n_cuota",
                    "mora_financiera" => "mora_financiera",
                    "saldo_original" => "saldo_original",
                    "saldo_actual" => "saldo_actual",
                    "accion_fecha" => "accion_fecha",
                    _ => "id_solicitud"
                };

                string sortOrder = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";

                var p = new DynamicParameters();
                p.Add("@fInicio", dtInicio);
                p.Add("@fCorte", dtCorte);
                p.Add("@Accion", accion);
                p.Add("@Filtro", filtro);
                var data = connection.Query<FrmCOCobroFiadoresConsultaData>(
                    "dbo.spCBR_Cobro_Fiadores_Consulta",
                    p,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    string q = filtro.Trim().ToUpper();
                    data = data.Where(x =>
                        ((x.codigo ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.cedula ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.nombre ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.acccion_tipo ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.accion_fecha ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.notifica_fecha ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.estadoPersona_desc ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.linea_desc ?? "").Trim().ToUpper().Contains(q)) ||
                        ((x.institucion_desc ?? "").Trim().ToUpper().Contains(q))
                    ).ToList();
                }

                result.Result.total = data.Count;

                bool asc = (sortOrder ?? "ASC").Trim().ToUpper() == "ASC";
                if (sortField == "codigo") data = asc ? data.OrderBy(x => x.codigo ?? "").ToList() : data.OrderByDescending(x => x.codigo ?? "").ToList();
                else if (sortField == "cedula") data = asc ? data.OrderBy(x => x.cedula ?? "").ToList() : data.OrderByDescending(x => x.cedula ?? "").ToList();
                else if (sortField == "nombre") data = asc ? data.OrderBy(x => x.nombre ?? "").ToList() : data.OrderByDescending(x => x.nombre ?? "").ToList();
                else if (sortField == "n_cuota") data = asc ? data.OrderBy(x => x.n_cuota).ToList() : data.OrderByDescending(x => x.n_cuota).ToList();
                else if (sortField == "mora_financiera") data = asc ? data.OrderBy(x => x.mora_financiera).ToList() : data.OrderByDescending(x => x.mora_financiera).ToList();
                else if (sortField == "saldo_original") data = asc ? data.OrderBy(x => x.saldo_original).ToList() : data.OrderByDescending(x => x.saldo_original).ToList();
                else if (sortField == "saldo_actual") data = asc ? data.OrderBy(x => x.saldo_actual).ToList() : data.OrderByDescending(x => x.saldo_actual).ToList();
                else if (sortField == "accion_fecha") data = asc ? data.OrderBy(x => x.accion_fecha ?? "").ToList() : data.OrderByDescending(x => x.accion_fecha ?? "").ToList();
                else data = asc ? data.OrderBy(x => x.id_solicitud).ToList() : data.OrderByDescending(x => x.id_solicitud).ToList();

                if (exportAll)
                {
                    result.Result.lista = data;
                    return result;
                }

                int offset = pagina;
                int fetch = paginacion;
                if (offset < 0) offset = 0;
                if (fetch <= 0) fetch = 30;

                result.Result.lista = data.Skip(offset).Take(fetch).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<FrmCOCobroFiadoresConsultaData>();
            }

            return result;
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
            if (dto?.ids == null || dto.ids.Count == 0)
                return DbHelper.ErrorResponse("Debe Seleccionar al menos un caso!", -2);

            var exec = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, conn =>
            {
                int existe25 = conn.QueryFirstOrDefault<int>(@"
                    SELECT COUNT(*)
                    FROM dbo.CATALOGO
                    WHERE codigo IN (
                        SELECT valor
                        FROM dbo.CBR_PARAMETROS
                        WHERE COD_PARAMETRO = '25'
                    );");

                if (existe25 == 0)
                    throw new Exception("No se encuentra configurada la Línea/Retención para Cobro a Fiador.");

                foreach (var id in dto.ids.Where(x => x > 0))
                {
                    conn.Execute("dbo.spCBR_Cobro_Fiadores_Notifica",
                        new { Operacion = id, Usuario = usuario },
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return true;
            });

            if (exec.Code != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error al notificar.");

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Cobro a Fiadores: Notifica Advertencia - Casos {dto.ids.Count}",
                Movimiento = "Procesa - WEB",
                Modulo = vModulo
            });

            return DbHelper.OkResponse("Ok");
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
            if (dto?.ids == null || dto.ids.Count == 0)
                return DbHelper.ErrorResponse("Debe Seleccionar al menos un caso!", -2);

            var exec = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, conn =>
            {
                int existe25 = conn.QueryFirstOrDefault<int>(@"
                    SELECT COUNT(*)
                    FROM dbo.CATALOGO
                    WHERE codigo IN (
                        SELECT valor
                        FROM dbo.CBR_PARAMETROS
                        WHERE COD_PARAMETRO = '25'
                    );");

                if (existe25 == 0)
                    throw new Exception("No se encuentra configurada la Línea/Retención para Cobro a Fiador.");

                foreach (var id in dto.ids.Where(x => x > 0))
                {
                    conn.Execute("dbo.spCBR_Cobro_Fiadores_Procesa",
                        new { Operacion = id, Usuario = usuario },
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return true;
            });

            if (exec.Code != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error al procesar.");

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Cobro a Fiadores: Procesa Cobros - Casos {dto.ids.Count}",
                Movimiento = "Procesa - WEB",
                Modulo = vModulo
            });

            return DbHelper.OkResponse("Ok");
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                if (dto?.ids == null || dto.ids.Count == 0)
                {
                    result.Code = -2;
                    result.Description = "Debe Seleccionar al menos un caso!";
                    return result;
                }
                var qExiste25 = @"
                    SELECT COUNT(*) AS Existe
                    FROM dbo.CATALOGO
                    WHERE codigo IN (
                        SELECT valor
                        FROM dbo.CBR_PARAMETROS
                        WHERE COD_PARAMETRO = '25'
                    );";

                int existe25 = connection.QueryFirstOrDefault<int>(qExiste25);
                if (existe25 == 0)
                {
                    result.Code = -2;
                    result.Description = "No se encuentra configurada la Línea/Retención para Cobro a Fiador, verifique los parámetros de cobro [25]";
                    return result;
                }
                var qExiste27 = @"
                    SELECT COUNT(*) AS Existe
                    FROM dbo.FND_PLANES
                    WHERE COD_PLAN IN (
                        SELECT valor
                        FROM dbo.CBR_PARAMETROS
                        WHERE COD_PARAMETRO = '27'
                    );";

                int existe27 = connection.QueryFirstOrDefault<int>(qExiste27);
                if (existe27 == 0)
                {
                    result.Code = -2;
                    result.Description = "No se encuentra configurado el Fondo de Devolución para Cobro a Fiador, verifique los parámetros de cobro [27]";
                    return result;
                }

                foreach (var id in dto.ids)
                {
                    if (id <= 0) continue;

                    var p = new DynamicParameters();
                    p.Add("@FIA_Operacion", id);
                    p.Add("@Usuario", usuario);
                    p.Add("@Notas", "");

                    connection.Execute(
                        "dbo.spCbr_Cobro_Fiadores_Cancela",
                        p,
                        commandType: System.Data.CommandType.StoredProcedure
                    );
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Cobro a Fiadores: Cancela Cobro - Casos {dto.ids.Count}",
                    Movimiento = "Procesa - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}
