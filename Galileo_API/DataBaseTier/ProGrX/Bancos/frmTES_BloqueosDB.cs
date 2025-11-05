using Microsoft.Data.SqlClient;
using Dapper;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using Newtonsoft.Json;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_BloqueosDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;
        private mSecurityMainDb DBBitacora;

        public frmTES_BloqueosDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(config);
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener la informaci�n de una solicitud mediante el n�mero de contabilidad y el n�mero de solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Contabilidad"></param>
        /// <param name="Solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TES_Bloqueo_TransaccionDTO> TES_Bloqueos_Solicitud_Obtener(int CodEmpresa, int Contabilidad, int Solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_Bloqueo_TransaccionDTO>
            {
                Code = 0,
                Result = new TES_Bloqueo_TransaccionDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select C.*,B.descripcion as BancoX,X.descripcion as ConceptoX
                    ,U.descripcion as UnidadX,T.descripcion as TipoX
                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_Banco = B.id_Banco
                    inner join tes_conceptos X on C.cod_concepto = X.cod_concepto
                    inner join CntX_unidades U on C.cod_unidad = U.cod_unidad and U.cod_contabilidad = @contabilidad
                    inner join tes_Tipos_doc T on C.Tipo = T.tipo 
                    where C.estado = 'P' and C.nsolicitud = @nsolicitud";
                    response.Result = connection.Query<TES_Bloqueo_TransaccionDTO>(query,
                        new { 
                            contabilidad = Contabilidad,
                            nsolicitud = Solicitud
                        }).FirstOrDefault();

                    if (response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "No se encontr&oacute; el n&uacute;mero de solicitud, o no se encuentra pendiente";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener la lista de solicitudes bloqueadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_Bloqueos_SolicitudesBloquedas_Obtener(int CodEmpresa, string filtros)
        {
            TES_BloqueosFiltros filtro = JsonConvert.DeserializeObject<TES_BloqueosFiltros>(filtros) ?? new TES_BloqueosFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                var where = "";
                var fechaInicio = filtro.fecha_inicio?.Date;
                var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @"select COUNT(C.nsolicitud)
                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_Banco = B.id_Banco
                    where user_hold is not null";

                    var query = @"select C.nsolicitud,C.codigo,C.beneficiario,C.monto,C.fecha_solicitud
                    ,C.fecha_Hold,B.descripcion as BancoX,C.Tipo
                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_Banco = B.id_Banco
                    where user_hold is not null";

                    if (!filtro.todas_fechas)
                    {
                        where += " and fecha_hold between @FechaInicio and @FechaFin ";
                    }

                    if (!filtro.todas_solicitudes)
                    {
                        where += " and (nsolicitud >= @SolicitudInicio and nsolicitud <= @SolicitudCorte ) ";
                    }

                    if (!string.IsNullOrEmpty(filtro.filtro))
                    {
                        where += $@" and (C.nsolicitud like @filtro OR C.codigo like @filtro
                            OR C.beneficiario like @filtro OR B.descripcion like @filtro)  ";
                    }

                    queryT += where;
                    query += where + @$" ORDER BY C.nsolicitud desc
                                    OFFSET @pagina ROWS 
                                    FETCH NEXT @paginacion ROWS ONLY";

                    var parametros = new
                    {
                        FechaInicio = fechaInicio,
                        FechaFin = fechaCorte,
                        SolicitudInicio = filtro.solicitud_inicio,
                        SolicitudCorte = filtro.solicitud_corte,
                        filtro = $"%{filtro.filtro}%",
                        pagina = filtro.pagina,
                        paginacion = filtro.paginacion
                    };
                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, parametros);
                    response.Result.lista = connection.Query<TES_Bloqueo_TransaccionDTO>(query, parametros).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }
            return response;
        }

        /// <summary>
        /// Bloquear una solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Solicitud"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_Bloqueos_Solicitud_Bloquear(int CodEmpresa, int Solicitud, string razon ,string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"update Tes_Transacciones set user_hold = @usuario, fecha_hold = dbo.MyGetdate(), razon_hold = @razon
                        where nsolicitud = @nsolicitud";
                    connection.Execute(query, new
                    {
                        nsolicitud = Solicitud,
                        usuario = Usuario,
                        razon = (razon.Length > 99) ? razon.Substring(0, 99) : razon
                    });

                    string detalleBitacora = $"Bloqueo de Solicitud : {Solicitud} , por: {razon}";

                    mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Solicitud, "05", detalleBitacora, Usuario.ToUpper());

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = detalleBitacora,
                        Movimiento = "APLICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Solicitud Bloqueada Satisfactoriamente...";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Desbloquear una solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Solicitud"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_Bloqueos_Solicitud_Desbloquear(int CodEmpresa, int Solicitud, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"update Tes_Transacciones set user_hold = Null, fecha_hold = null
                        where nsolicitud = @nsolicitud";
                    connection.Execute(query, new { nsolicitud = Solicitud });

                    mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Solicitud, "06", "", Usuario.ToUpper());

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = $"Desbloqueo de Solicitud : {Solicitud}",
                        Movimiento = "APLICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Solicitud Des-Bloqueada Satisfactoriamente...";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}