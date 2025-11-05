using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCPR_Solicitud_CotizaValoraDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly frmCpr_SolicitudDB frmCpr_Solicitud;
        public string sendEmail = "";
        public string Notificaciones = "";

        public frmCPR_Solicitud_CotizaValoraDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            frmCpr_Solicitud = new frmCpr_SolicitudDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }

        public ErrorDto<List<CprValoracionLista>> CprSolicitudProveedoresLista_Obtener(int CodEmpresa, int consulta, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprValoracionLista>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCPR_SolicitudProveedores_Obtener {consulta}, {cpr_id}";
                    response.Result = connection.Query<CprValoracionLista>(query).ToList();
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

        public ErrorDto CprSolicitudProveedor_Invitar(int CodEmpresa, CprSolicitudProvDTO proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Obtengo el tipo de la solicitud
                    var qrySolicitud = @$"SELECT * FROM CPR_SOLICITUD WHERE CPR_ID = {proveedor.cpr_id}";
                    CprSolicitudDTO solicitud = connection.Query<CprSolicitudDTO>(qrySolicitud).FirstOrDefault();

                    
                    if(solicitud.tipo_orden == frmCpr_Solicitud.CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    {

                        var qryProveedores = $@"SELECT COUNT(*) FROM CPR_SOLICITUD_PROV WHERE CPR_ID = {proveedor.cpr_id} ";
                        int proveedores = connection.QueryFirstOrDefault<int>(qryProveedores);

                        if(proveedores == 1)
                        {
                            error.Code = -1;
                            error.Description = "Una compra directa no permite mas de un proveedor";
                            return error;
                        }

                        var query = $@"exec spCPR_SolicitudProv_Invitar '{proveedor.proveedor_codigo}', '{proveedor.cpr_id}',  '{proveedor.registro_usuario}' ";
                        var respuesta = connection.Query(query);

                        error = frmCpr_Solicitud.CompraDirectaProv_Agregar(CodEmpresa, proveedor.cpr_id, solicitud);
                    }
                    else
                    {
                        var query = $@"exec spCPR_SolicitudProv_Invitar '{proveedor.proveedor_codigo}', '{proveedor.cpr_id}',  '{proveedor.registro_usuario}' ";
                        var respuesta = connection.Query(query);


                    }

                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        public ErrorDto CprSolicitudProveedor_Eliminar(int CodEmpresa, int proveedor_codigo, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new ErrorDto();
            error.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete from CPR_SOLICITUD_PROV where PROVEEDOR_CODIGO = {proveedor_codigo} and CPR_ID = {cpr_id}";
                    var respuesta = connection.Query(query);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        public ErrorDto<List<CprSolicitudProvDTO>> CprSolicitudProvInvitados_Obtener(int CodEmpresa, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprSolicitudProvDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCPR_SolicitudProvInvitados_Obtener {cpr_id}";
                    response.Result = connection.Query<CprSolicitudProvDTO>(query).ToList();

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

        public ErrorDto<List<CprSolicitudPrvBs>> CprSolicitudProvContizacionLista_Obtener(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprSolicitudPrvBs>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCPR_SolicitudProvCotiLista_Obtener {cpr_id}, '{cod_proveedor}'";
                    response.Result = connection.Query<CprSolicitudPrvBs>(query).ToList();
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

        public ErrorDto<List<CprSolicitudProvValItemData>> CprSolicitudProvValItemData_Obtener(int CodEmpresa, string parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprSolicitudProvValItemData>>();
            var parametro = JsonConvert.DeserializeObject<CprParametrosVal_Busqueda >(parametros);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ISNULL(P.ID_VALORACION, 0) AS ID_VALORACION ,
                                        I.val_item, I.DESCRIPCION, I.PESO,ISNULL(P.NOTA, 0) AS NOTA, ISNULL(P.PUNTAJE, 0) AS PUNTAJE
                                        FROM CPR_VALORA_ITEMS I LEFT JOIN CPR_SOLICITUD_PROV_VALORA P 
                                        ON P.VAL_ITEM = I.VAL_ITEM AND P.CPR_ID = {parametro.crp_id} 
                                        AND PROVEEDOR_CODIGO = {parametro.proveedor}
                                        WHERE I.VAL_ID = '{parametro.val_id}' ";
                    response.Result = connection.Query<CprSolicitudProvValItemData>(query).ToList();
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

        public async Task<ErrorDto> CprSolicitudProvCotizacion_Enviar(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            List<CprSolicitudBsDTO> infoProducto = new List<CprSolicitudBsDTO>();
            CprProveedorDTO proveedor = new CprProveedorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"SELECT DESCRIPCION, CEDJUR, EMAIL FROM CPR_PROVEEDORES_TEMPO WHERE COD_PROVEEDOR = {cod_proveedor}";
                    proveedor = connection.Query<CprProveedorDTO>(query).FirstOrDefault();


                    query = @$"SELECT B.CPR_ID, B.COD_PRODUCTO, P.DESCRIPCION, B.CANTIDAD, B.MONTO, (B.CANTIDAD * B.MONTO) AS TOTAL, P.COD_UNIDAD
                    FROM CPR_SOLICITUD_BS B left JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = B.COD_PRODUCTO
                    WHERE B.CPR_ID = {cpr_id}";

                    infoProducto = connection.Query<CprSolicitudBsDTO>(query).ToList();


                    string queryRecepcion = $"SELECT recepcion_ofertas FROM CPR_SOLICITUD WHERE cpr_id = {cpr_id}";
                    DateTime? horario_recepcion = connection.QueryFirstOrDefault<DateTime?>(queryRecepcion, new { CprId = cpr_id });
                    string recepcion = horario_recepcion?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Sin fecha";



                    await CorreoSolicitaCotizacion_Enviar(CodEmpresa, proveedor, infoProducto, recepcion);

                    query = @$"UPDATE CPR_SOLICITUD_PROV SET ESTADO = 'E' WHERE CPR_ID = {cpr_id} AND PROVEEDOR_CODIGO = '{cod_proveedor}'";
                    connection.Query(query);

                    resp.Description = "Solicitud de cotización enviada";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private async Task CorreoSolicitaCotizacion_Enviar(int CodEmpresa, CprProveedorDTO proveedor, List<CprSolicitudBsDTO> info, string recepcion)
        {

            ErrorDto resp = new ErrorDto();
            try
            {


                EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, Notificaciones);
                string body = @$"<html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Solicitud de Cotización</title>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                    }}
                                    .container {{
                                        width: 600px;
                                        margin: 0 auto;
                                        border: 1px solid #eaeaea;
                                        padding: 20px;
                                    }}
                                    .header {{
                                        background-color: #e8f3ff;
                                        padding: 10px;
                                    }}
                                    .header img {{
                                        width: auto;
                                        height: 50px;
                                    }}
                                    .content {{
                                        margin-top: 20px;
                                    }}
                                    .content h2 {{
                                        font-size: 16px;
                                        color: #0072ce;
                                    }}
                                    .table {{
                                        width: 100%;
                                        margin-top: 20px;
                                        border-collapse: collapse;
                                    }}
                                    .table th, .table td {{
                                        padding: 10px;
                                        border: 1px solid #dcdcdc;
                                        text-align: left;
                                    }}
                                    .table th {{
                                        background-color: #0072ce;
                                        color: white;
                                    }}
                                    
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                    </div>
                                    <div class=""content"">
                                        <h2><strong>Solicitud de Cotización</strong> </h2>
                                        <p>No. Solicitud <strong>{info[0].cpr_id}</strong> </p>
                                        <p>Proveedor: {proveedor.descripcion}, Cédula Jurídica: {proveedor.cedjur}</p>
                                        <p>Mediante la presente se le solicita una cotización de los productos detallados, a continuación.</p>
                                            <p>Fecha límite de recepción de ofertas: {recepcion}</p>
                                        <table class=""table"">
                                            <tr>
                                                <th>Cantidad</th>
                                                <th>U/M</th>
                                                <th>Código</th>
                                                <th>Descripción</th>
                                                <th>Monto</th>
                                                <th>Total</th>
                                            </tr>
                ";

                foreach (var producto in info)
                {
                    body += "<tr>";
                    body += $"<td>{producto.cantidad}</td>";
                    body += $"<td>{producto.cod_unidad.ToUpper()}</td>";
                    body += $"<td>{producto.cod_producto}</td>";
                    body += $"<td>{producto.descripcion}</td>";
                    body += $"<td>{producto.monto}</td>";
                    body += $"<td>{producto.total}</td>";
                    body += "</tr>";
                }

                body += @"      </table>
                            </div>
                        </div>
                    </body>
                </html>";

                List<IFormFile> Attachments = new List<IFormFile>();

                if(sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = proveedor.email;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Solicitud de Cotización";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
        }

        public ErrorDto CprSolicitudValoracion_Guardar(int CodEmpresa, CprSolicitusValoracionGuardar datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new() 
            { 
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string xmlOutput = "";

                    //Guardo valoracion de proveedores.
                    foreach (var proveedor in datos.valoracion)
                    {
                        xmlOutput = _AuxiliarDB.fxConvertModelToXml<CprSolicitudProvValItemData>(proveedor);
                        var values = new
                        {
                            datos = xmlOutput,
                            cpr_id = datos.cotizacion.cpr_id,
                            proveedor = datos.cotizacion.proveedor_codigo,
                            usuario = datos.cotizacion.valora_usuario
                        };

                        var id_val = connection.Execute("spCPR_CprSolicitudProvValora_Guardar", values, commandType: CommandType.StoredProcedure);

                    }

                    //Guardo valoracion de productos.
                    foreach (var producto in datos.productos)
                    {

                        var update = $@"UPDATE [dbo].[CPR_SOLICITUD_PROV_BS]
                                       SET [VALORA_PUNTAJE] = {producto.valora_puntaje}
                                          ,[VALORA_FECHA] = getdate()
                                          ,[VALORA_USUARIO] = '{datos.cotizacion.valora_usuario}'
                                          ,[VALORA_NOTAS] = '{datos.cotizacion.valora_notas}'
                                          ,ESTADO = 'V'
                                     WHERE [CPR_ID] = {datos.cotizacion.cpr_id}
                                      AND [COD_PRODUCTO] = '{producto.cod_producto}'
                                      AND [PROVEEDOR_CODIGO] = {datos.cotizacion.proveedor_codigo} " ;
                                  //  AND [CODIGO] = '{datos.cotizacion.codigo}'  ";

                        response.Code =  connection.Execute(update);
                    }

                    //actualiza Proveedor estado y puntaje
                    var updateProv = $@"UPDATE CPR_SOLICITUD_PROV
                                   SET ESTADO = 'V'
                                      ,VALORA_PUNTAJE = {datos.cotizacion.valora_puntaje}
                                      ,VALORA_FECHA = getdate()
                                      ,VALORA_USUARIO = '{datos.cotizacion.valora_usuario}'
                                      ,NOTAS = '{datos.cotizacion.valora_notas}'
                                 WHERE CPR_ID = {datos.cotizacion.cpr_id}
                                   AND PROVEEDOR_CODIGO = {datos.cotizacion.proveedor_codigo} ";
                    response.Code = connection.Execute(updateProv);

                    //Actualiza cotizacion estado
                    var estadoCotiza = $@"UPDATE CPR_SOLICITUD_PROV_COTIZA SET ESTADO = 'V' WHERE CPR_ID = {datos.cotizacion.cpr_id}
                                           AND  PROVEEDOR_CODIGO = {datos.cotizacion.proveedor_codigo} ";
                    response.Code = connection.Execute(estadoCotiza);


                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto CprSolicitudProv_GastoMenor(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {

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