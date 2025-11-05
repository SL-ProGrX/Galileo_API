using PgxAPI.Models.TES;
using Microsoft.Data.SqlClient;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_AutorizacionDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;

        public frmTES_AutorizacionDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(config);
        }

        /// <summary>
        /// Obtener solicitudes pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TES_SolicitudesLista> TES_SolicitudesPendientes_Obtener(int CodEmpresa, string filtros)
        {
            TES_AutorizacionFiltros filtro = JsonConvert.DeserializeObject<TES_AutorizacionFiltros>(filtros) ?? new TES_AutorizacionFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_SolicitudesLista>
            {
                Code = 0,
                Result = new TES_SolicitudesLista()
            };
            response.Result.total = 0;
            var fechaInicio = filtro.fecha_inicio.Date;
            var fechaCorte = filtro.fecha_corte.Date.AddDays(1).AddTicks(-1);
            try
            {
                var query = "";
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

                    //Verifica si el banco requiere supervisi�n de movimientos
                    var querySupervision = "Select SUPERVISION from tes_bancos where id_banco = @Banco";
                    int iSupervision = connection.Query<int?>(querySupervision, new { Banco = filtro.id_banco }).FirstOrDefault() ?? 0;

                    //Obtener cta Interbancaria
                    var queryInterbancaria = @"select Bg.LCTA_INTERBANCARIA 
                        from TES_BANCOS Tb inner join TES_BANCOS_GRUPOS Bg on Tb.COD_GRUPO = Bg.COD_GRUPO
                        Where Tb.ID_BANCO = @Banco";
                    int? mLInterBanca = connection.Query<int?>(queryInterbancaria, new { Banco = filtro.id_banco }).FirstOrDefault() ?? 0;

                    //Revision con Ajuste Automatico
                    var querySP = "exec spTes_Cuentas_Revision_Automatica @Banco";
                    connection.Execute(querySP, new { Banco = filtro.id_banco });

                    var queryConteo = @"select COUNT(T.nsolicitud) from Tes_Transacciones T inner join Tes_Bancos B on T.id_banco = B.id_banco
                        where T.estado = 'P' and B.id_banco = @Banco and T.Tipo = @TipoDoc";
                    response.Result.total = connection.Query<int>(queryConteo,
                        new
                        {
                            Banco = filtro.id_banco,
                            TipoDoc = filtro.tipo_doc
                        }).FirstOrDefault();

                    //Consulta
                    if (filtro.duplicados)
                    {
                        query = @"select T.nsolicitud,T.codigo,T.beneficiario,T.monto,T.fecha_solicitud,T.cta_Ahorros, 
                        dbo.fxTesSupervisa(CODIGO,BENEFICIARIO,monto,0,'T') as 'duplicado', 
                        dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) as 'Cta_Verifica'
                        , T.Detalle1 + T.detalle2 as 'Detalle', isnull(T.cod_App,'') as 'AppId',
                        IIF(T.user_hold IS NULL, 0, 1) AS Bloqueo, S.ESTADOACTUAL
                        from Tes_Transacciones T inner join Tes_Bancos B on T.id_banco = B.id_banco
                        inner join Socios S on T.CODIGO = S.CEDULA 
                        where T.estado = 'P' and B.id_banco = @Banco and T.Tipo = @TipoDoc";
                    }
                    else
                    {
                        query = @"select T.nsolicitud,T.codigo,T.beneficiario,T.monto,T.fecha_solicitud,T.cta_Ahorros
                        ,0 as 'duplicado'
                        , dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) as 'Cta_Verifica'
                        , T.Detalle1 + T.detalle2 as 'Detalle', isnull(T.cod_App,'') as 'AppId',
                        IIF(T.user_hold IS NULL, 0, 1) AS Bloqueo, S.ESTADOACTUAL
                        from Tes_Transacciones T inner join Tes_Bancos B on T.id_banco = B.id_banco
                        inner join Socios S on T.CODIGO = S.CEDULA 
                        where T.estado = 'P' and B.id_banco = @Banco and T.Tipo = @TipoDoc";
                    }

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
                        query += " and T.fecha_hold is null ";
                    }

                    //Transferencias
                    if (filtro.tipo_doc == "TE")
                    {
                        switch (filtro.tipo_cuenta?.ToUpperInvariant())
                        {
                            case "L": // Locales
                                query += $" and len(rtrim(T.cta_Ahorros)) <> {mLInterBanca} ";
                                break;
                            case "I": // Interbancarias
                                query += $" and len(rtrim(T.cta_Ahorros)) = {mLInterBanca} ";
                                break;
                            default:
                                // "Todas" o null 
                                break;
                        }

                        //Grupo Bancario
                        var queryGrupoBanco = "select dbo.fxTes_BancoSFN(@Banco) as Codigo";
                        var mGrupoBancario = connection.Query<int?>(queryGrupoBanco, new { Banco = filtro.id_banco }).FirstOrDefault() ?? 0;

                        //Filtra Cuentas del mismo Banco
                        if (filtro.mismo_banco)
                        {
                            query += $" and (SUBSTRING(rtrim(T.cta_Ahorros), 1, 10) like '%{mGrupoBancario}%' and len(rtrim(T.cta_Ahorros)) = {mLInterBanca}) ";
                        }
                    }

                    //Valida tipo de autorizaci�n (Emisi�n Documento o Firma)
                    if (filtro.tipo_autorizacion == 0)
                    {
                        query += " and T.fecha_autorizacion is null and T.monto between @MontoInicio and @MontoFin ";

                        if (!string.IsNullOrWhiteSpace(filtro.token))
                        {
                            query += " and T.id_token = @Token ";
                        }
                    }
                    else
                    {
                        query += @" and T.FIRMAS_AUTORIZA_FECHA is null and T.monto > B.firmas_hasta 
                            and dbo.fxTesAutorizaFirmaAcceso(@Usuario, @Banco, T.monto) = 1 ";
                    }

                    if (!string.IsNullOrWhiteSpace(filtro.detalle))
                    {
                        query += " and (T.DETALLE1 + T.DETALLE2) like @Detalle ";
                    }

                    if (!string.IsNullOrWhiteSpace(filtro.appid))
                    {
                        query += " and isnull(T.COD_APP,'') like @CodigoApp ";
                    }

                    query += " order by T.nsolicitud asc, T.fecha_solicitud asc";
                    response.Result.solicitudes = connection.Query<TES_SolicitudesData>(query,
                        new
                        {
                            Banco = filtro.id_banco,
                            TipoDoc = filtro.tipo_doc,
                            Usuario = filtro.usuario,
                            FechaInicio = fechaInicio,
                            FechaFin = fechaCorte,
                            SolicitudInicio = filtro.solicitud_inicio,
                            SolicitudCorte = filtro.solicitud_corte,
                            MontoInicio = filtro.monto_inicio,
                            MontoFin = filtro.monto_fin,
                            Token = filtro.token,
                            Detalle = $"%{filtro.detalle}%",
                            CodigoApp = $"%{filtro.appid}%"
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.solicitudes = null;
                response.Result.total = 0;
            }
            return response;
        }

        /// <summary>
        /// Aplicar autorizaci�n de solicitudes pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="clave"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_autorizacion"></param>
        /// <param name="solicitudesLista"></param>
        /// <returns></returns>
        public ErrorDto TES_Autorizacion_Aplicar(TES_AutorizaParametros nsolicitud)
        {
            List<int> lista = JsonConvert.DeserializeObject<List<int>>(nsolicitud.solicitudesLista) ?? new List<int>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(nsolicitud.codEmpresa);
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
                        clave = nsolicitud.clave,
                        usuario = nsolicitud.usuario
                    });

                    if (autorizacion == null)
                    {
                        response.Code = -2;
                        response.Description = "Contrase&ntilde;a Incorrecta, o no Existe Nivel de Autorizaci&oacute;n";
                        return response;
                    }

                    string vMensaje = "";
                    bool permite = true;
                    foreach (var solicitud in lista)
                    {

                        if (mTesoreria.fxTesParametro(nsolicitud.codEmpresa, "12") == "S")
                        {
                            //reviso si el código de id es igual al código del usuario
                            query = "SELECT USER_SOLICITA FROM TES_TRANSACCIONES WHERE NSOLICITUD = @nsolicitud";
                            var userSolicita = connection.QueryFirstOrDefault<string>(query, new { nsolicitud = solicitud });

                            if (userSolicita.ToUpper() == nsolicitud.usuario.ToUpper())
                            {
                                response.Code = -1;
                                vMensaje += solicitud + ",";
                                permite = false;
                            }
                        }

                        if (permite)
                        {
                            //Valida tipo de autorizacion (Emision Documento o Firma)
                            if (nsolicitud.tipo_autorizacion == 0)
                            {
                                //Emision
                                query = $@"Update Tes_Transacciones set Autoriza='S', Fecha_Autorizacion = dbo.MyGetdate(), User_Autoriza = @usuario, 
                                                     ESTADO_SINPE = @estadoSinpe, TIPO_GIROSINPE = @tipoGiroSinpe Where Nsolicitud = @nsolicitud ";

                                querySP = "exec spTesBitacora @nsolicitud,'02','',@usuario";
                            }
                            else
                            {
                                //Firmas
                                query = "Update Tes_Transacciones set FIRMAS_AUTORIZA_FECHA = dbo.MyGetdate(), FIRMAS_AUTORIZA_USUARIO = @usuario Where Nsolicitud = @nsolicitud ";

                                querySP = "exec spTesBitacora @nsolicitud,'04','',@usuario";
                            }

                            int? stadoSinpeInt = nsolicitud.estadoSinpe == true ? 1 : 0;
                            if(nsolicitud.tipoDocumento != "TS")
                            {
                                nsolicitud.tipoGiroSinpe = "NA";
                                stadoSinpeInt = null;
                            }


                            connection.Execute(query, new { usuario = nsolicitud.usuario, nsolicitud = solicitud, estadoSinpe = nsolicitud.estadoSinpe, tipoGiroSinpe = nsolicitud.tipoGiroSinpe });
                            connection.Execute(querySP, new { usuario = nsolicitud.usuario, nsolicitud = solicitud });
                        } 
                    }

                    if(response.Code == 0)
                    {
                        response.Description = "Autorización procesada correctamente!";
                    }
                    else
                    {
                        response.Description += $@" - Solicitud(es): {vMensaje} no puede(n) ser autorizada(s) por el mismo usuario";  
                    }
                    
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
        /// Obtener rangos de montos de autorizaci�n de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<TES_AutorizacionData> TES_AutorizacionDoc_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_AutorizacionData>
            {
                Code = 0,
                Result = new TES_AutorizacionData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rango_gen_Inicio, rango_gen_corte, firmas_gen_inicio, firmas_gen_corte 
                    from TES_AUTORIZACIONES where NOMBRE = @usuario";
                    response.Result = connection.Query<TES_AutorizacionData>(query,
                        new { usuario = usuario }).FirstOrDefault();
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
        /// Obtener rango de montos de autorizaci�n de firmas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<TES_FirmasAutData> TES_AutorizacionFirma_Obtener(int CodEmpresa, string usuario, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_FirmasAutData>
            {
                Code = 0,
                Result = new TES_FirmasAutData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select firmas_autoriza_inicio,firmas_autoriza_corte from TES_BANCO_FIRMASAUT 
                        where USUARIO = @usuario and ID_BANCO = @banco and aplica_rango_autorizacion = 1";
                    response.Result = connection.Query<TES_FirmasAutData>(query, 
                        new { usuario = usuario, banco = banco }).FirstOrDefault();
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
    }
}