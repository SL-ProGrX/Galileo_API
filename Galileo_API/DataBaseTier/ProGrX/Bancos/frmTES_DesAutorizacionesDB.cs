using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.TES;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_DesAutorizacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;

        public frmTES_DesAutorizacionesDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(config);
        }

        /// <summary>
        /// Obtener solicitudes autorizadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TES_SolicitudesLista> TES_DesAutorizaciones_Obtener(int CodEmpresa, string filtros)
        {
            TES_DesAutorizacionesFiltros filtro = JsonConvert.DeserializeObject<TES_DesAutorizacionesFiltros>(filtros) ?? new TES_DesAutorizacionesFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_SolicitudesLista>
            {
                Code = 0,
                Description = "",
                Result = new TES_SolicitudesLista(),
            };
            var fechaInicio = filtro.fecha_inicio.Date;
            var fechaCorte = filtro.fecha_corte.Date.AddDays(1).AddTicks(-1);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryR = @"select rango_gen_Inicio, rango_gen_corte, firmas_gen_inicio, firmas_gen_corte 
                    from TES_AUTORIZACIONES where NOMBRE = @usuario";
                    var Rangos = connection.Query<TES_AutorizacionData>(queryR,
                        new { usuario = filtro.usuario }).FirstOrDefault();

                    if (Rangos != null)
                    {
                        filtro.monto_inicio = Rangos.rango_gen_inicio ?? 0;
                        filtro.monto_fin = Rangos.rango_gen_corte ?? 0;
                    }

                    var query = $@"select T.nsolicitud,T.codigo,T.beneficiario,T.monto,T.fecha_solicitud,T.cta_Ahorros
                    ,0 as 'duplicado', dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) as 'Cta_Verifica'
                    , T.Detalle1 + T.detalle2 as 'Detalle', isnull(T.cod_App,'') as 'AppId'
                    from Tes_Transacciones T inner join Tes_Bancos B on T.id_banco = B.id_banco
                    where T.estado = 'P' and B.id_banco = @Banco and T.Tipo = @TipoDoc";

                    if (!filtro.todas_fechas)
                    {
                        query += " and T.fecha_solicitud between @FechaInicio and @FechaFin ";
                    }

                    if (!filtro.todas_solicitudes)
                    {
                        query += " and (T.nsolicitud >= @SolicitudInicio and nsolicitud <= @SolicitudCorte ) ";
                    }

                    if (!filtro.casos_bloqueados)
                    {
                        query += " and T.fecha_hold is null";
                    }

                    if (filtro.tipo_autorizacion == 0)
                    {
                        query += " and T.fecha_autorizacion is not null and T.monto between @MontoInicio and @MontoFin ";
                    }
                    else
                    {
                        query += @" and T.FIRMAS_AUTORIZA_FECHA is not null and T.monto > B.firmas_hasta";
                    }

                    if (!string.IsNullOrWhiteSpace(filtro.detalle))
                    {
                        query += " and (T.DETALLE1 + T.DETALLE2) like @Detalle ";
                    }

                    if (!string.IsNullOrWhiteSpace(filtro.appid))
                    {
                        query += " and isnull(T.COD_APP,'') like @CodigoApp ";
                    }

                    response.Result.solicitudes = connection.Query<TES_SolicitudesData>(query,
                        new
                        {
                            Banco = filtro.id_banco,
                            TipoDoc = filtro.tipo_doc,
                            FechaInicio = fechaInicio,
                            FechaFin = fechaCorte,
                            SolicitudInicio = filtro.solicitud_inicio,
                            SolicitudCorte = filtro.solicitud_corte,
                            MontoInicio = filtro.monto_inicio,
                            MontoFin = filtro.monto_fin,
                            Detalle = $"%{filtro.detalle}%",
                            CodigoApp = $"%{filtro.appid}%"
                        }).ToList();
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
        /// Aplicar la des-autorización de las solicitudes seleccionadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="clave"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_autorizacion"></param>
        /// <param name="solicitudesLista"></param>
        /// <returns></returns>
        public ErrorDto TES_DesAutorizaciones_Aplicar(int CodEmpresa, string clave, string usuario, int tipo_autorizacion, string solicitudesLista)
        {
            List<int> lista = JsonConvert.DeserializeObject<List<int>>(solicitudesLista) ?? new List<int>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };

            try
            {
                var query = "";
                var querySP = "";
                using var connection = new SqlConnection(stringConn);
                {
                    var queryAuth = @"Select * From Tes_Autorizaciones Where Clave = @clave and nombre = @usuario and estado = 'A'";
                    var autorizacion = connection.QueryFirstOrDefault<TES_AutorizacionData>(queryAuth, new
                    {
                        clave = clave,
                        usuario = usuario
                    });

                    if (autorizacion == null)
                    {
                        response.Code = -1;
                        response.Description = "Contrase&ntilde;a Incorrecta, o no Existe Nivel de Autorizaci&oacute;n";
                        return response;
                    }

                    foreach (var solicitud in lista)
                    {
                        //Valida tipo de autorizacion (Emision Documento o Firma)
                        if (tipo_autorizacion == 0)
                        {
                            //Emision
                            query = "Update Tes_Transacciones set Autoriza='N', Fecha_Autorizacion = Null, User_Autoriza = Null Where Nsolicitud = @nsolicitud ";

                            querySP = "exec spTesBitacora @nsolicitud,'03','Des-Autorización de Tipo Emisión de Documentos',@usuario";
                        }
                        else
                        {
                            //Firmas
                            query = "Update Tes_Transacciones set FIRMAS_AUTORIZA_FECHA = Null, FIRMAS_AUTORIZA_USUARIO = Null Where Nsolicitud = @nsolicitud ";

                            querySP = "exec spTesBitacora @nsolicitud,'03','Des-Autorización de Tipo Firmas Electrónicas',@usuario";
                        }

                        connection.Execute(query, new { usuario = usuario, nsolicitud = solicitud });
                        connection.Execute(querySP, new { usuario = usuario, nsolicitud = solicitud });
                    }

                    response.Description = "Des-autorizacion procesada correctamente!";
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
