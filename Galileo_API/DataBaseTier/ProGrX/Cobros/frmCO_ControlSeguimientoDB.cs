using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlSeguimientoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 4;

        private const string FiltroCedula = "cedula";
        private const string FiltroTexto = "texto";
        private const string SqlOffsetFetch = @" OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        public FrmCoControlSeguimientoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene lista de expedientes (socios) para buscador (F4).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Expedientes_Obtener(int CodEmpresa, string? texto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
            select
                rtrim(S.cedula) as item,
                rtrim(S.nombre) as descripcion
            from socios S
            where (@texto = '' or S.cedula like @like or S.nombre like @like)
            order by S.nombre;";

                return conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    texto,
                    like
                }).ToList();
            });
        }


        /// <summary>
        /// Obtiene lista de gestiones para buscador (F4).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Gestiones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(cod_gestion) as item, rtrim(descripcion) as descripcion
                    from cbr_gestiones
                    where estado = 1 and nivel_gestion = 'U'
                    order by cod_gestion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de causas de morosidad para buscador (F4).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_CausasMora_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(cod_causa) as item, rtrim(descripcion) as descripcion
                    from cbr_causas_morosidad
                    where activa = 1
                    order by cod_causa;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de arreglos para buscador (F4).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Arreglos_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select rtrim(cod_arreglo) as item, rtrim(descripcion) as descripcion
                    from cbr_tipos_arreglos
                    where activo = 1
                    order by cod_arreglo;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene información de una gestión y valida acceso del usuario con fxCBRGestionUsuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_gestion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CoControlSegGestionInfoDto> CO_Gestion_Info_Obtener(int CodEmpresa, string cod_gestion, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<CoControlSegGestionInfoDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoControlSegGestionInfoDto()
            };

            try
            {
                cod_gestion = (cod_gestion ?? string.Empty).Trim();
                usuario = (usuario ?? string.Empty).Trim().ToUpperInvariant();

                const string sql = @"
                    select 
                        rtrim(cod_gestion) as cod_gestion,
                        rtrim(descripcion) as descripcion,
                        isnull(monto,0) as monto,
                        isnull(modifica_usuario,0) as modifica_usuario,
                        isnull(modifica_desviacion,0) as modifica_desviacion
                    from cbr_gestiones
                    where estado = 1 and nivel_gestion = 'U' and cod_gestion = @cod_gestion;";

                var info = conn.QueryFirstOrDefault<CoControlSegGestionInfoDto>(sql, new { cod_gestion })
                           ?? new CoControlSegGestionInfoDto();

                const string sqlAcceso = @"select dbo.fxCBRGestionUsuario(@cod_gestion, @usuario) as acceso;";
                info.acceso = conn.QueryFirstOrDefault<int>(sqlAcceso, new { cod_gestion, usuario });

                response.Result = info;
                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlSegGestionInfoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Calcula rango de vencimiento según parámetro 01 y configuración del usuario (tiempo_resolucion_com).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CoControlSegVenceRangoDto> CO_ControlSeguimiento_Vence_Rango_Obtener(int CodEmpresa, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<CoControlSegVenceRangoDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoControlSegVenceRangoDto()
            };

            try
            {
                usuario = (usuario ?? string.Empty).Trim().ToUpperInvariant();

                int dias = 30;

                const string sqlParam01 = @"select valor from cbr_parametros where cod_parametro = '01';";
                var v = conn.QueryFirstOrDefault<string>(sqlParam01) ?? string.Empty;
                if (int.TryParse(v.Trim(), out var p01)) dias = p01;

                const string sqlUsr = @"select isnull(tiempo_resolucion_com,0) as tiempo_resolucion_com from cbr_usuarios where usuario = @usuario;";
                var usrDias = conn.QueryFirstOrDefault<int>(sqlUsr, new { usuario });

                if (usrDias > 0 && usrDias <= dias) dias = usrDias;

                const string sqlHoy = @"select dbo.MyGetdate();";
                var hoy = conn.QueryFirstOrDefault<DateTime>(sqlHoy);

                response.Result.fecha_min = hoy.Date;
                response.Result.dias_max = dias;
                response.Result.fecha_max = hoy.Date.AddDays(dias);

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlSegVenceRangoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Lista historial de gestiones del expediente (CBR_Seguimiento) por cédula, con filtro texto y sort/paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistGestiones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                          ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CoControlSegHistGestionDto>()
                }
            };

            try
            {
                var cedula = ExtractKeyFromFiltro(filtros.filtro, FiltroCedula);
                cedula = (cedula ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return response;
                }

                var texto = ExtractKeyFromFiltro(filtros.filtro, FiltroTexto);
                texto = (texto ?? string.Empty).Trim();

                bool hasTexto = !string.IsNullOrWhiteSpace(texto);
                var like = hasTexto ? $"%{texto}%" : null;

                int pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
                int fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;
                bool usarPaginacion = fetch > 0;
                int offset = usarPaginacion ? pagina * fetch : 0;

                string sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
                int sortOrder = filtros.sortOrder == 1 ? 1 : 0;

                const string filtroSql = @"
          and (
               @texto is null
            or cast(S.cod_seg as nvarchar(50)) like @like
            or rtrim(S.usuario) like @like
            or S.notas like @like
            or rtrim(S.cod_gestion) like @like
            or isnull(rtrim(G.descripcion),'') like @like
            or rtrim(S.cod_arreglo) like @like
            or isnull(rtrim(A.descripcion),'') like @like
            or rtrim(S.cod_causa) like @like
            or isnull(rtrim(C.descripcion),'') like @like
            or cast(isnull(S.monto,0) as nvarchar(50)) like @like
          )";

                const string sqlCount = @"
        select count(1)
        from CBR_Seguimiento S
        left join cbr_gestiones G
               on S.cod_gestion = G.cod_gestion
        left join CBR_CAUSAS_MOROSIDAD C
               on S.cod_causa = C.cod_causa
        left join CBR_TIPOS_ARREGLOS A
               on S.cod_arreglo = A.cod_arreglo
        where S.cedula = @cedula" + filtroSql + ";";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    cedula,
                    texto = hasTexto ? texto : null,
                    like
                });

                var sql = @"
        select
            S.cod_seg,
            S.fecha,
            S.tiempo_resolucion,
            dateadd(day, isnull(S.tiempo_resolucion,0), S.fecha) as vence,
            rtrim(S.cod_gestion) as cod_gestion,
            isnull(rtrim(G.descripcion),'') as gestion,
            S.notas,
            rtrim(S.usuario) as usuario,
            S.monto,
            rtrim(S.cod_arreglo) as cod_arreglo,
            isnull(rtrim(A.descripcion),'') as arreglo,
            S.arreglo_vence,
            rtrim(S.cod_causa) as cod_causa,
            isnull(rtrim(C.descripcion),'') as causa
        from CBR_Seguimiento S
        left join cbr_gestiones G
               on S.cod_gestion = G.cod_gestion
        left join CBR_CAUSAS_MOROSIDAD C
               on S.cod_causa = C.cod_causa
        left join CBR_TIPOS_ARREGLOS A
               on S.cod_arreglo = A.cod_arreglo
        where S.cedula = @cedula" + filtroSql + @"
        order by
            case when @sf = 'cod_seg' and @so = 1 then S.cod_seg end asc,
            case when @sf = 'cod_seg' and @so = 0 then S.cod_seg end desc,

            case when @sf = 'fecha' and @so = 1 then S.fecha end asc,
            case when @sf = 'fecha' and @so = 0 then S.fecha end desc,

            case when @sf = 'vence' and @so = 1 then dateadd(day, isnull(S.tiempo_resolucion,0), S.fecha) end asc,
            case when @sf = 'vence' and @so = 0 then dateadd(day, isnull(S.tiempo_resolucion,0), S.fecha) end desc,

            case when @sf = 'usuario' and @so = 1 then S.usuario end asc,
            case when @sf = 'usuario' and @so = 0 then S.usuario end desc,

            case when @sf = 'gestion' and @so = 1 then G.descripcion end asc,
            case when @sf = 'gestion' and @so = 0 then G.descripcion end desc,

            case when @sf = 'arreglo' and @so = 1 then A.descripcion end asc,
            case when @sf = 'arreglo' and @so = 0 then A.descripcion end desc,

            case when @sf = 'causa' and @so = 1 then C.descripcion end asc,
            case when @sf = 'causa' and @so = 0 then C.descripcion end desc,

            case when @sf = 'monto' and @so = 1 then S.monto end asc,
            case when @sf = 'monto' and @so = 0 then S.monto end desc,

            case when @sf = 'notas' and @so = 1 then S.notas end asc,
            case when @sf = 'notas' and @so = 0 then S.notas end desc,

            S.cod_seg desc";

                if (usarPaginacion)
                {
                    sql += @"
        OFFSET @offset ROWS
        FETCH NEXT @fetch ROWS ONLY";
                }

                response.Result.lista = conn.Query<CoControlSegHistGestionDto>(sql, new
                {
                    cedula,
                    texto = hasTexto ? texto : null,
                    like,
                    sf,
                    so = sortOrder,
                    offset,
                    fetch
                }).ToList();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta historial de gestiones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistGestiones_Lista_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CO_ControlSeguimiento_HistGestiones_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Lista historial de oficiales asignados (cbr_asignacion_h) por cédula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistOficiales_Lista_Obtener(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CoControlSegHistOficialDto>()
                }
            };

            try
            {
                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros.sortOrder == 1 ? 1 : 0;

                var fx = ParseFiltrosBasicos(filtros);

                if (string.IsNullOrWhiteSpace(fx.Cedula))
                {
                    response.Result.total = 0;
                    response.Result.lista = new List<CoControlSegHistOficialDto>();
                    return response;
                }

                var cedula = fx.Cedula;
                var texto = fx.Texto;
                var like = fx.Like;

                var offset = fx.Offset;
                var fetch = fx.Fetch;
                var usarPaginacion = fetch > 0;

                const string sqlCount = @"
            select count(1)
            from cbr_asignacion_h
            where cedula = @cedula
              and (
                   @texto is null
                or usuario like @like
              );";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    cedula,
                    texto,
                    like
                });

                var sql = @"
            select 
                fecha_asignacion,
                rtrim(usuario) as usuario,
                isnull(mantener,0) as mantener,
                isnull(rebajo_doble,0) as rebajo_doble,
                isnull(aplica_mora,0) as aplica_mora
            from cbr_asignacion_h
            where cedula = @cedula
              and (
                   @texto is null
                or usuario like @like
              )
            order by
                case when @sortField = 'fecha_asignacion' and @sortOrder = 1 then fecha_asignacion end asc,
                case when @sortField = 'fecha_asignacion' and @sortOrder = 0 then fecha_asignacion end desc,
                case when @sortField = 'usuario' and @sortOrder = 1 then usuario end asc,
                case when @sortField = 'usuario' and @sortOrder = 0 then usuario end desc,
                fecha_asignacion desc";

                if (usarPaginacion)
                {
                    sql += SqlOffsetFetch;
                }

                response.Result.lista = conn.Query<CoControlSegHistOficialDto>(sql, new
                {
                    cedula,
                    texto,
                    like,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }


        /// <summary>
        /// Exporta historial de oficiales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistOficiales_Lista_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CO_ControlSeguimiento_HistOficiales_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Actualiza flags (mantener/rebajo_doble/aplica_mora) de un registro de historial de oficiales.
        /// Registra bitácora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CO_ControlSeguimiento_HistOficiales_Actualizar(int CodEmpresa, CoControlSegHistOficialActualizarDto data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedula = (data.cedula ?? string.Empty).Trim();
                var usuarioAsignado = (data.usuario_asignado ?? string.Empty).Trim();
                var usuario = (data.usuario ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cedula))
                    return DbHelper.ErrorResponse("Cédula requerida.");

                if (string.IsNullOrWhiteSpace(usuarioAsignado))
                    return DbHelper.ErrorResponse("Usuario asignado requerido.");

                const string sql = @"
                    update cbr_asignacion_h
                    set mantener = @mantener,
                        rebajo_doble = @rebajo_doble,
                        aplica_mora = @aplica_mora
                    where cedula = @cedula
                      and usuario = @usuarioAsignado
                      and fecha_asignacion = @fecha_asignacion;";

                var rows = conn.Execute(sql, new
                {
                    cedula,
                    usuarioAsignado,
                    fecha_asignacion = data.fecha_asignacion,
                    mantener = data.mantener,
                    rebajo_doble = data.rebajo_doble,
                    aplica_mora = data.aplica_mora
                });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se actualizó ningún registro. Verifique la clave (cedula/usuario/fecha).");

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"HistOficiales: {cedula} / {usuarioAsignado} / {data.fecha_asignacion:yyyy-MM-dd HH:mm:ss} [M={data.mantener}, R2x={data.rebajo_doble}, Mora={data.aplica_mora}]",
                    Movimiento = "MODIFICA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Actualizado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Lista fiadores por expediente (cédula), con opción soloOperacionesAtrasadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <param name="soloOperacionesAtrasadas"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Fiadores_Lista_Obtener(int CodEmpresa, string parametros, bool soloOperacionesAtrasadas)
        {
            var response = PrepareListaConCedula<CoControlSegFiadorDto>(
                CodEmpresa,
                parametros,
                out var conn,
                out var filtros,
                out var fx);

            if (response.Code != 0 || string.IsNullOrWhiteSpace(fx.Cedula) || conn == null)
                return response;

            using (conn)
            {
                try
                {
                    var fx2 = ResolveCommonBlock(
                        filtros,
                        new Dictionary<string, string>
                        {
                            ["id_solicitud"] = "id_solicitud",
                            ["cedula"] = "cedula",
                            ["nombre"] = "nombre",
                            ["estado_persona"] = "estado_persona",
                            ["institucion"] = "institucion",
                            ["estado_mora"] = "estado_mora"
                        },
                        "id_solicitud");

                    var cedula = fx2.Cedula;
                    var texto = fx2.Texto;
                    var like = fx2.Like;
                    var offset = fx2.Offset;
                    var fetch = fx2.Fetch;
                    var usarPaginacion = fx2.UsarPaginacion;
                    var orderByCol = fx2.OrderByCol;
                    var orderDir = fx2.OrderDir;

                    const string sqlGrouped = @"
                select
                    isnull(M.ESTADO,'') as estado_mora,
                    F.Id_Solicitud as id_solicitud,
                    S.cedula as cedula,
                    S.nombre as nombre,
                    E.descripcion as estado_persona,
                    I.descripcion as institucion
                from fiadores F
                inner join Socios S
                    on F.cedulaf = S.cedula
                inner join Instituciones I
                    on S.cod_institucion = I.cod_institucion
                inner join Reg_Creditos R
                    on F.Id_Solicitud = R.Id_Solicitud
                inner join AFI_ESTADOS_PERSONA E
                    on E.cod_estado = S.estadoActual
                left join MOROSIDAD M
                    on F.Id_Solicitud = M.Id_Solicitud
                    and M.Estado = 'A'
                where F.estado = 'A'
                  and R.cedula = @cedula
                  and R.estado = 'A'
                  and (@soloOperacionesAtrasadas = 0 or M.ESTADO = 'A')
                  and (
                       @texto is null
                    or S.cedula like @like
                    or S.nombre like @like
                    or E.descripcion like @like
                    or I.descripcion like @like
                    or cast(F.Id_Solicitud as nvarchar(50)) like @like
                  )
                group by
                    F.Id_Solicitud,
                    S.cedula,
                    M.ESTADO,
                    S.nombre,
                    E.descripcion,
                    I.descripcion";

                    var sqlCount = @"
                select count(1)
                from (
                " + sqlGrouped + @"
                ) x;";

                    response.Result ??= new TablasListaGenericaModel
                    {
                        total = 0,
                        lista = new List<CoControlSegFiadorDto>()
                    };

                    response.Result.total = conn.QuerySingle<int>(sqlCount, new
                    {
                        cedula,
                        soloOperacionesAtrasadas = soloOperacionesAtrasadas ? 1 : 0,
                        texto,
                        like
                    });

                    var sqlLista = @"
                select *
                from (
                " + sqlGrouped + @"
                ) t
                order by " + orderByCol + " " + orderDir;

                    if (usarPaginacion)
                    {
                        sqlLista += @"
                    offset @offset rows
                    fetch next @fetch rows only";
                    }

                    response.Result.lista = conn.Query<CoControlSegFiadorDto>(sqlLista, new
                    {
                        cedula,
                        soloOperacionesAtrasadas = soloOperacionesAtrasadas ? 1 : 0,
                        texto,
                        like,
                        offset,
                        fetch
                    }).ToList();

                    return response;
                }
                catch (SqlException ex)
                {
                    return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
                }
            }
        }


        /// <summary>
        /// Exporta lista de fiadores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <param name="soloOperacionesAtrasadas"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Fiadores_Lista_Export(int CodEmpresa, string parametros, bool soloOperacionesAtrasadas)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CO_ControlSeguimiento_Fiadores_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros), soloOperacionesAtrasadas);
        }
        /// <summary>
        /// Lista comisiones por expediente (cédula) desde cbr_comisiones_detalle/cbr_segdetalle/cbr_seguimiento, con filtro y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Comisiones_Lista_Obtener(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CoControlSegComisionDto>()
                }
            };

            try
            {
                var cedula = ExtractKeyFromFiltro(filtros.filtro, FiltroCedula);
                cedula = (cedula ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    response.Result.total = 0;
                    response.Result.lista = new List<CoControlSegComisionDto>();
                    return response;
                }

                var texto = ExtractKeyFromFiltro(filtros.filtro, FiltroTexto);
                texto = (texto ?? string.Empty).Trim();
                var hasTexto = !string.IsNullOrWhiteSpace(texto);
                var like = hasTexto ? $"%{texto}%" : null;
                var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
                var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;

                var usarPaginacion = fetch > 0;
                var offset = usarPaginacion ? (pagina * fetch) : 0;

                const string sqlCount = @"
            select count(1)
            from (
                select C.cod_remesa
                from cbr_comisiones_detalle C
                inner join cbr_segdetalle D on C.cod_remesa = D.cod_remesa
                inner join cbr_seguimiento S on D.cod_seg = S.cod_seg
                where S.cedula = @cedula
                  and (
                       @texto is null
                    or C.usuario like @like
                    or C.cod_remesa like @like
                    or isnull(C.tesoreria_numero,'') like @like
                  )
                group by C.cod_remesa
            ) X;";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    cedula,
                    texto = hasTexto ? texto : null,
                    like
                });

                var sql = @"
            select 
                C.cod_remesa,
                rtrim(C.usuario) as usuario,
                C.monto,
                C.tesoreria_numero,
                C.tesoreria_fecha
            from cbr_comisiones_detalle C
            inner join cbr_segdetalle D on C.cod_remesa = D.cod_remesa
            inner join cbr_seguimiento S on D.cod_seg = S.cod_seg
            where S.cedula = @cedula
              and (
                   @texto is null
                or C.usuario like @like
                or C.cod_remesa like @like
                or isnull(C.tesoreria_numero,'') like @like
              )
            group by C.usuario, C.cod_remesa, C.monto, C.tesoreria_numero, C.tesoreria_fecha
            order by C.tesoreria_fecha desc";

                if (usarPaginacion)
                {
                    sql += SqlOffsetFetch;
                }

                response.Result.lista = conn.Query<CoControlSegComisionDto>(sql, new
                {
                    cedula,
                    texto = hasTexto ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta lista de comisiones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_Comisiones_Lista_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CO_ControlSeguimiento_Comisiones_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }



        /// <summary>
        /// Registra seguimiento llamando spCBRControlSGT.
        /// Registra bitácora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <summary>
        public ErrorDto CO_ControlSeguimiento_Registrar(int CodEmpresa, CoControlSegRegistrarDto data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedula = (data.cedula ?? string.Empty).Trim();
                var usuario = (data.usuario ?? string.Empty).Trim().ToUpperInvariant();
                var codGestion = (data.cod_gestion ?? string.Empty).Trim();
                var notas = (data.notas ?? string.Empty).Trim();
                var oficina = (data.oficina ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cedula))
                    return DbHelper.ErrorResponse("Cédula requerida.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse("Usuario requerido.");

                if (string.IsNullOrWhiteSpace(codGestion))
                    return DbHelper.ErrorResponse("Gestión requerida.");

                if (string.IsNullOrWhiteSpace(notas))
                    return DbHelper.ErrorResponse("No se especificó ninguna observación...");

                if (!data.vence.HasValue)
                    return DbHelper.ErrorResponse("Debe indicar la fecha de vencimiento.");

                var pre = ValidarPrecondicionesRegistro(conn, cedula, usuario, codGestion);
                if (pre != null) return pre;

                var montoResult = ResolverMontoGestion(conn, codGestion, data.monto, out var montoFinal);
                if (montoResult != null) return montoResult;

                var venceError = ValidarRangoVencimiento(conn, usuario, data.vence.Value.Date);
                if (venceError != null) return venceError;
                var p = new DynamicParameters();
                p.Add("@Cedula", cedula);
                p.Add("@Usuario", usuario);
                p.Add("@CodGestion", codGestion);
                p.Add("@Vence", data.vence.Value.Date);
                p.Add("@Notas", notas);
                p.Add("@Oficina", string.IsNullOrWhiteSpace(oficina) ? null : oficina);
                p.Add("@Monto", montoFinal);
                p.Add("@Operacion", data.operacion);
                p.Add("@Causa", (data.cod_causa ?? string.Empty).Trim());
                p.Add("@Arreglo", (data.cod_arreglo ?? string.Empty).Trim());

                conn.Execute("spCBRControlSGT", p, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"SGT Registrar: {cedula} / Gestion {codGestion} / Vence {data.vence:yyyy-MM-dd}",
                    Movimiento = "REGISTRA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Seguimiento Registrado Satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el resumen/estado del expediente para el tab Estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CoControlSegEstadoDto> CO_ControlSeguimiento_Estado_Obtener(int CodEmpresa, string cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = DbHelper.CreateOkResponse(new CoControlSegEstadoDto());
            response.Result ??= new CoControlSegEstadoDto();

            try
            {
                cedula = (cedula ?? string.Empty).Trim();
                response.Result.cedula = cedula;

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    response.Result.texto = "";
                    response.Result.estado_tag = "N";
                    response.Result.operaciones_activas = 0;
                    response.Result.operaciones_mora_activa = 0;
                    return response;
                }

                const string sql = @"
                select
                    isnull(count(distinct R.Id_Solicitud), 0) as operaciones_activas,
                    isnull(count(distinct M.Id_Solicitud), 0) as operaciones_mora_activa
                from Reg_Creditos R
                left join MOROSIDAD M
                       on M.Id_Solicitud = R.Id_Solicitud
                      and M.Estado = 'A'
                where R.cedula = @cedula
                  and R.estado = 'A';";

                var r = conn.QueryFirstOrDefault<dynamic>(sql, new { cedula });

                int ops = Convert.ToInt32(r?.operaciones_activas ?? 0);
                int mora = Convert.ToInt32(r?.operaciones_mora_activa ?? 0);

                response.Result.operaciones_activas = ops;
                response.Result.operaciones_mora_activa = mora;

                if (ops <= 0)
                {
                    response.Result.texto = "Sin operaciones activas.";
                    response.Result.estado_tag = "N";
                }
                else if (mora > 0)
                {
                    response.Result.texto = $"Operaciones con mora activa: {mora} de {ops}.";
                    response.Result.estado_tag = "S";
                }
                else
                {
                    response.Result.texto = "Operaciones Activas al Día.";
                    response.Result.estado_tag = "N";
                }

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlSegEstadoDto>(ex.Message);
            }
        }
        /// <summary>
        /// Lista historial - detalle de operaciones del expediente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistDetalle_Lista_Obtener(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CoControlSegHistDetalleDto>()
                }
            };

            response.Result ??= new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<CoControlSegHistDetalleDto>()
            };

            try
            {
                var fx2 = ResolveCommonBlock(
                    filtros,
                    new Dictionary<string, string>
                    {
                        ["operacion"] = "operacion",
                        ["codigo"] = "codigo",
                        ["cuotas"] = "cuotas",
                        ["mora"] = "mora",
                        ["saldo"] = "saldo",
                        ["abono"] = "abono",
                        ["estado_actual"] = "estado_actual"
                    },
                    "operacion");

                if (string.IsNullOrWhiteSpace(fx2.Cedula))
                {
                    response.Result.total = 0;
                    response.Result.lista = new List<CoControlSegHistDetalleDto>();
                    return response;
                }

                var cedula = fx2.Cedula;
                var texto = fx2.Texto;
                var like = fx2.Like;
                var offset = fx2.Offset;
                var fetch = fx2.Fetch;
                var usarPaginacion = fx2.UsarPaginacion;
                var orderByCol = fx2.OrderByCol;
                var orderDir = fx2.OrderDir;
                const string sqlBase = @"
            select
                R.Id_Solicitud                 as operacion,
                rtrim(isnull(R.CODIGO,''))     as codigo,
                isnull(R.CUOTA,0)              as cuotas,
                isnull(V.cuota,0)              as mora,
                isnull(R.SALDO,0)              as saldo,
                isnull(R.AMORTIZA,0)           as abono,
                rtrim(isnull(R.ESTADO,''))     as estado_actual
            from Reg_Creditos R
            left join VISTA_MOROSIDAD V
                   on R.Id_Solicitud = V.Id_Solicitud
            where R.CEDULA = @cedula
              and R.ESTADO = 'A'
              and (
                   @texto is null
                or cast(R.Id_Solicitud as nvarchar(50)) like @like
                or isnull(R.CODIGO,'') like @like
                or isnull(R.ESTADO,'') like @like
              )";

                var sqlCount = @"
            select count(1)
            from (
            " + sqlBase + @"
            ) X;";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    cedula,
                    texto,
                    like
                });

                var sql = @"
            select *
            from (
            " + sqlBase + @"
            ) T
            order by " + orderByCol + " " + orderDir;

                if (usarPaginacion)
                {
                    sql += @"
            offset @offset rows fetch next @fetch rows only";
                }

                response.Result.lista = conn.Query<CoControlSegHistDetalleDto>(sql, new
                {
                    cedula,
                    texto,
                    like,
                    offset,
                    fetch
                }).ToList();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }
        /// <summary>
        /// Exporta historial - detalle de operaciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CO_ControlSeguimiento_HistDetalle_Lista_Export(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CO_ControlSeguimiento_HistDetalle_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }


        /// <summary>
        /// Extrae una llave del filtro.
        /// </summary>
        /// <param name="filtro"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string? ExtractKeyFromFiltro(string? filtro, string key)
        {
            if (string.IsNullOrWhiteSpace(filtro)) return null;

            var s = filtro.Trim();
            if (!s.StartsWith("{", StringComparison.Ordinal) || !s.EndsWith("}", StringComparison.Ordinal))
            {
                if (key.Equals(FiltroCedula, StringComparison.OrdinalIgnoreCase)) return s;
                return null;
            }

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
                if (dict == null) return null;

                dict.TryGetValue(key, out var value);
                return value;
            }
            catch (JsonException)
            {
                return null;
            }
        }
        /// <summary>
        /// Extrea e interpreta los filtros básicos comunes.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>

        private static (string Cedula, string? Texto, string? Like, int Pagina, int Fetch, int Offset) ParseFiltrosBasicos(FiltrosLazyLoadData filtros)
        {
            var cedula = (ExtractKeyFromFiltro(filtros.filtro, FiltroCedula) ?? string.Empty).Trim();

            var texto = (ExtractKeyFromFiltro(filtros.filtro, FiltroTexto) ?? string.Empty).Trim();
            var hasTexto = !string.IsNullOrWhiteSpace(texto);
            var like = hasTexto ? $"%{texto}%" : null;

            var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;
            var offset = (fetch > 0) ? (pagina * fetch) : 0;

            return (cedula, hasTexto ? texto : null, like, pagina, fetch, offset);
        }
        private ErrorDto<TablasListaGenericaModel> PrepareListaConCedula<TDto>(
      int codEmpresa,
      string parametros,
      out SqlConnection? conn,
      out FiltrosLazyLoadData filtros,
      out (string Cedula, string? Texto, string? Like, int Pagina, int Fetch, int Offset) fx)
        {
            conn = null;
            filtros = new FiltrosLazyLoadData();
            fx = default;

            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                    ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(jex.Message);
            }

            conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<TDto>()
                }
            };

            response.Result ??= new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<TDto>()
            };

            fx = ParseFiltrosBasicos(filtros);

            if (string.IsNullOrWhiteSpace(fx.Cedula))
            {
                response.Result.total = 0;
                response.Result.lista = new List<TDto>();
                conn.Dispose();
                conn = null;
            }

            return response;
        }
        private static (string Cedula, string? Texto, string? Like, int Offset, int Fetch, bool UsarPaginacion) GetCommonFx(FiltrosLazyLoadData filtros)
        {
            var fx = ParseFiltrosBasicos(filtros);
            var fetch = fx.Fetch;
            var offset = fx.Offset;
            var usarPaginacion = fetch > 0;

            return (fx.Cedula, fx.Texto, fx.Like, offset, fetch, usarPaginacion);
        }
        private static (string Cedula,string? Texto,string? Like,int Offset,int Fetch,bool UsarPaginacion,string OrderByCol,string OrderDir) ResolveCommonBlock(FiltrosLazyLoadData filtros,IReadOnlyDictionary<string, string> sortWhitelist,string defaultSortCol)
        {
            var common = GetCommonFx(filtros);

            var cedula = common.Cedula;
            var texto = common.Texto;
            var like = common.Like;
            var offset = common.Offset;
            var fetch = common.Fetch;
            var usarPaginacion = common.UsarPaginacion;

            var sf = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderByCol = sortWhitelist.TryGetValue(sf, out var col)
                ? col
                : defaultSortCol;

            var orderDir = (filtros.sortOrder == 1) ? "ASC" : "DESC";

            return (cedula, texto, like, offset, fetch, usarPaginacion, orderByCol, orderDir);
        }
        private ErrorDto? ValidarPrecondicionesRegistro(SqlConnection conn,string cedula,string usuario,string codGestion)
        {
            const string sqlUsr = @"select isnull(count(*),0) from cbr_usuarios where usuario=@usuario and estado=1;";
            if (conn.QueryFirstOrDefault<int>(sqlUsr, new { usuario }) <= 0)
                return DbHelper.ErrorResponse("El usuario actual no se encuentra activo...");

            const string sqlGest = @"select isnull(count(*),0) from cbr_gestiones where cod_gestion=@cod and estado=1 and nivel_gestion='U';";
            if (conn.QueryFirstOrDefault<int>(sqlGest, new { cod = codGestion }) <= 0)
                return DbHelper.ErrorResponse("La gestión actual no se encuentra activa...");

            const string sqlP05 = @"select valor from cbr_parametros where cod_parametro='05';";
            var p05 = (conn.QueryFirstOrDefault<string>(sqlP05) ?? "").Trim();
            if (p05.Length == 0 || p05[0] != 'S')
            {
                const string sqlAsg = @"select isnull(count(*),0) from cbr_asignacion where usuario=@usuario and cedula=@cedula;";
                if (conn.QueryFirstOrDefault<int>(sqlAsg, new { usuario, cedula }) <= 0)
                    return DbHelper.ErrorResponse("Este expediente no se encuentra asignado al usuario actual, verifique...");
            }

            const string sqlAcc = @"select dbo.fxCBRGestionUsuario(@cod,@usuario);";
            if (conn.QueryFirstOrDefault<int>(sqlAcc, new { cod = codGestion, usuario }) == 0)
                return DbHelper.ErrorResponse("El usuario no tiene acceso a esta gestión");

            return null;
        }
        private ErrorDto? ResolverMontoGestion(SqlConnection conn,string codGestion,decimal? montoInput,out decimal montoFinal)
        {
            montoFinal = 0;

            const string sql = @"
            select 
                isnull(monto,0) as monto,
                isnull(modifica_usuario,0) as modifica_usuario,
                isnull(modifica_desviacion,0) as modifica_desviacion
            from cbr_gestiones
            where estado = 1 and nivel_gestion = 'U' and cod_gestion = @cod;";

            var g = conn.QueryFirstOrDefault<dynamic>(sql, new { cod = codGestion });

            decimal baseMonto = Convert.ToDecimal(g?.monto ?? 0);
            int modUsr = Convert.ToInt32(g?.modifica_usuario ?? 0);
            decimal desv = Math.Abs(Convert.ToDecimal(g?.modifica_desviacion ?? 0));

            if (modUsr == 0)
            {
                montoFinal = baseMonto;
                return null;
            }
            if (!montoInput.HasValue)
                return DbHelper.ErrorResponse("Debe indicar el monto.");

            montoFinal = montoInput.Value;

            if (montoFinal < baseMonto - desv)
                return DbHelper.ErrorResponse("El monto es menor que la desviación mínima");

            if (montoFinal > baseMonto + desv)
                return DbHelper.ErrorResponse("El monto es mayor que la desviación máxima");

            return null;
        }
        private static ErrorDto? ValidarRangoVencimiento( SqlConnection conn,string usuario,DateTime vence)
        {
            int dias = 30;

            const string sqlP01 = @"select valor from cbr_parametros where cod_parametro='01';";
            var p01 = (conn.QueryFirstOrDefault<string>(sqlP01) ?? string.Empty).Trim();
            if (int.TryParse(p01, out var d) && d > 0) dias = d;
            const string sqlUsr = @"select isnull(tiempo_resolucion_com,0) from cbr_usuarios where usuario=@usuario;";
            var usrDias = conn.QueryFirstOrDefault<int>(sqlUsr, new { usuario });
            if (usrDias > 0 && usrDias <= dias) dias = usrDias;
            const string sqlHoy = @"select dbo.MyGetdate();";
            var hoy = conn.QueryFirstOrDefault<DateTime>(sqlHoy).Date;
            if (vence < hoy || vence > hoy.AddDays(dias))
                return DbHelper.ErrorResponse($"La fecha de vencimiento debe estar entre {hoy:yyyy-MM-dd} y {hoy.AddDays(dias):yyyy-MM-dd}.");

            return null;
        }



    }
}
