using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesDesAutorizacionesDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesDesAutorizacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener solicitudes autorizadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesSolicitudesLista> TES_DesAutorizaciones_Obtener(int CodEmpresa, string filtros)
        {
            TesDesAutorizacionesFiltros filtro = JsonConvert.DeserializeObject<TesDesAutorizacionesFiltros>(filtros) ?? new TesDesAutorizacionesFiltros();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TesSolicitudesLista>
            {
                Code = 0,
                Description = "",
                Result = new TesSolicitudesLista(),
            };
            var fechaInicio = filtro.fecha_inicio.Date;
            var fechaCorte = filtro.fecha_corte.Date.AddDays(1).AddTicks(-1);
            try
            {
                var Rangos = conn.Query<TesAutorizacionData>(FrmTesAutorizacionSql.SQL_TES_AUTORIZACIONES_RANGOS,
                    new { filtro.usuario }).FirstOrDefault();

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

                response.Result.solicitudes = conn.Query<TesSolicitudesData>(query,
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
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesSolicitudesLista>(ex.Message);
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var query = "";
                var querySP = "";
                var queryAuth = @"Select * From Tes_Autorizaciones Where Clave = @clave and nombre = @usuario and estado = 'A'";
                var autorizacion = conn.QueryFirstOrDefault<TesAutorizacionData>(queryAuth, new
                {
                    clave,
                    usuario
                });

                if (autorizacion == null)
                {
                    return DbHelper.ErrorResponse("Contrase&ntilde;a Incorrecta, o no Existe Nivel de Autorizaci&oacute;n");
                }

                foreach (var solicitud in lista)
                {
                    string descBitacora = "";
                    //Valida tipo de autorizacion (Emision Documento o Firma)
                    if (tipo_autorizacion == 0)
                    {
                        //Emision
                        query = "Update Tes_Transacciones set Autoriza='N', Fecha_Autorizacion = Null, User_Autoriza = Null Where Nsolicitud = @nsolicitud ";

                        descBitacora = "Des-Autorización de Tipo Emisión de Documentos";
                    }
                    else
                    {
                        //Firmas
                        query = "Update Tes_Transacciones set FIRMAS_AUTORIZA_FECHA = Null, FIRMAS_AUTORIZA_USUARIO = Null Where Nsolicitud = @nsolicitud ";

                        descBitacora = "Des-Autorización de Tipo Firmas Electrónicas";
                    }

                    querySP = "exec spTesBitacora @nsolicitud,'03',@detalle,@usuario";

                    conn.Execute(query, new { usuario, nsolicitud = solicitud });
                    conn.Execute(querySP, new { usuario, nsolicitud = solicitud , detalle = descBitacora });
                }

                return DbHelper.OkResponse("Des-autorizacion procesada correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}   

                
