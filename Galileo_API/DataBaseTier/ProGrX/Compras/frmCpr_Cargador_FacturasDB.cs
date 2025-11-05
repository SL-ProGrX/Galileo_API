
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_Cargador_FacturasDB
    {

        private readonly IConfiguration? _config;
        public string sendEmail = "";
        public string TestMail = "";
        public string Notificaciones = "";
        public string EmailProveedor = "";
        private readonly EnvioCorreoDB _envioCorreoDB;


        public frmCpr_Cargador_FacturasDB(IConfiguration config)
        {
            _config = config;
            _envioCorreoDB = new EnvioCorreoDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();
        }


        /// <summary>
        /// Obtiene la lista de facturas XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proveedor"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<CprFacturasXMLLista> Cargador_Facturas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CprFacturasXMLFiltros filtro = JsonConvert.DeserializeObject<CprFacturasXMLFiltros>(filtros) ?? new CprFacturasXMLFiltros();
            var response = new ErrorDTO<CprFacturasXMLLista>
            {
                Code = 0,
                Result = new CprFacturasXMLLista
                {
                    total = 0
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string where = "";
                string paginaActual = "";
                string paginacionActual = "";
                string? cedJurProveedor = null;

                if (proveedor != 0)
                {
                    cedJurProveedor = connection.QueryFirstOrDefault<string>(
                        "SELECT cedjur FROM cxp_proveedores WHERE cod_proveedor = @CodProveedor",
                        new { CodProveedor = proveedor });

                    if (string.IsNullOrEmpty(cedJurProveedor))
                    {
                        response.Code = -2;
                        response.Description = "No se encontró Cédula Jurídica para el proveedor seleccionado. Verifique el registro de proveedor";
                        response.Result = null;
                        return response;
                    }

                    where += $"WHERE REPLACE(REPLACE(ced_jur_prov, ' ', ''), '-', '')  = '{cedJurProveedor.Replace("-", "")}' ";
                }

                // Filtro de b�squeda general
                if (filtro.filtro != null)
                {
                    var busqueda = $"( cod_documento LIKE '%{filtro.filtro}%' OR nombre_prov LIKE '%{filtro.filtro}%' OR ced_jur_prov LIKE '%{filtro.filtro}%' )";

                    if (string.IsNullOrEmpty(where))
                        where = $"WHERE {busqueda} ";
                    else
                        where += $"AND {busqueda} ";
                }

                // Paginaci�n
                if (filtro.pagina != null)
                {
                    paginaActual = $" OFFSET {filtro.pagina} ROWS ";
                    paginacionActual = $" FETCH NEXT {filtro.paginacion} ROWS ONLY ";
                }

                // Consulta total y lista de facturas
                string countQuery = $"SELECT COUNT(*) FROM CPR_FACTURAS_XML {where}";
                response.Result.total = connection.Query<int>(countQuery).First();

                string dataQuery = $"SELECT * FROM CPR_FACTURAS_XML {where} ORDER BY id DESC {paginaActual} {paginacionActual}";
                response.Result.lista = connection.Query<CprFacturasXML_DTO>(dataQuery).ToList();
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
        /// Obtiene una factura XML por ID
        /// </summary>
        /// <param name="CodEmpresa">C�digo de la empresa</param>
        /// <param name="id">ID de la factura</param>
        /// <returns>Factura encontrada o error</returns>
        public ErrorDTO<CprFacturasXML_DTO> Cargador_Factura_ObtenerPorId(int CodEmpresa, int id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<CprFacturasXML_DTO>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string query = "SELECT * FROM CPR_FACTURAS_XML WHERE id = @id";
                var factura = connection.QueryFirstOrDefault<CprFacturasXML_DTO>(query, new { id });

                if (factura == null)
                {
                    response.Code = -2;
                    response.Description = "Factura no encontrada";
                }
                else
                {
                    // Buscar proveedor por c�dula jur�dica en CXP_PROVEEDORES
                    string proveedorQuery = @"
                    SELECT TOP 1 cod_proveedor, descripcion 
                    FROM CXP_PROVEEDORES 
                    WHERE REPLACE(REPLACE(cedjur, ' ', ''), '-', '') = @Cedula";

                    var cedulaLimpia = factura.ced_jur_prov?.Replace("-", "").Replace(" ", "");

                    var proveedor = connection.QueryFirstOrDefault<CxpProveedorData>(
                        proveedorQuery, new { Cedula = cedulaLimpia });

                    if (proveedor != null)
                    {
                        // Asignar datos del proveedor a la factura
                        factura.cod_proveedor = proveedor.Cod_Proveedor;
                        factura.descripcion = proveedor.Descripcion;
                    }
                    else
                    {
                        factura.cod_proveedor = null;
                        factura.descripcion = null;
                    }

                    response.Result = factura;

                    // Obtener l�neas de la factura
                    response.Result.lineas = Cargador_FacturasDetalle_Obtener(CodEmpresa, id, proveedor.Cod_Proveedor).Result;

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
        /// Inserta la factura XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Cargador_Facturas_Guardar(int CodEmpresa, CprFacturasXML_DTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    // Verifica si la factura ya existe
                    var existsQuery = @"SELECT COUNT(*) FROM CPR_FACTURAS_XML WHERE COD_DOCUMENTO = @Cod_Documento AND CLAVE = @Clave AND CED_JUR_PROV = @Ced_Jur_Prov";
                    int count = connection.ExecuteScalarAsync<int>(existsQuery, new { request.clave, request.cod_documento, request.ced_jur_prov }).Result;

                    if (count > 0)
                    {
                        resp.Code = -2;
                        resp.Description = "Factura ya registrada.";
                        return resp;
                    }

                    var query = @"INSERT INTO CPR_FACTURAS_XML 
                            (COD_UEN, COD_DOCUMENTO, CLAVE, CED_JUR_PROV, NOMBRE_PROV, MONTO_TOTAL, 
                             COD_DIVISA, FECHA, ESTADO, REGISTRO_USUARIO, REGISTRO_FECHA)
                          VALUES
                            (@Cod_UEN, @Cod_Documento, @Clave, @Ced_Jur_Prov, @Nombre_Prov, @Monto_Total, 
                             @Cod_Divisa, @Fecha, @Estado, @Registro_Usuario, @Registro_Fecha)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_UEN", request.cod_uen, DbType.String);
                    parameters.Add("Cod_Documento", request.cod_documento, DbType.String);
                    parameters.Add("Clave", request.clave, DbType.String);
                    parameters.Add("Ced_Jur_Prov", request.ced_jur_prov, DbType.String);
                    parameters.Add("Nombre_Prov", request.nombre_prov, DbType.String);
                    parameters.Add("Monto_Total", request.monto_total, DbType.Decimal);
                    parameters.Add("Cod_Divisa", request.cod_divisa, DbType.String);
                    parameters.Add("Fecha", request.fecha, DbType.DateTime);
                    parameters.Add("Estado", request.estado, DbType.String);
                    parameters.Add("Registro_Usuario", request.registro_usuario, DbType.String);
                    parameters.Add("Registro_Fecha", request.registro_fecha, DbType.DateTime);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";



                    // Guardar l�neas de la factura
                    if (request.lineas != null && request.lineas.Count > 0)
                    {
                        // Guardar el ID de la factura insertada
                        //devuelve el id del beneficio
                        query = $"SELECT id FROM CPR_FACTURAS_XML  WHERE CLAVE = '{request.clave}' ";
                        var id = connection.Query<int>(query).FirstOrDefault();

                        foreach (var linea in request.lineas)
                        {
                            //obtengo el % de impuesto por linea
                            var porcentajeImpuesto = (linea.impuesto / linea.precioUnitario) * 100;

                            var lineaQuery = @"INSERT INTO CPR_FACTURAS_XML_DETALLE (
                                                        FAC_ID, 
                                                        NUMERO_LINEA, 
                                                        CODIGO, 
                                                        CODIGO_COMERCIAL, 
                                                        CANTIDAD, 
                                                        UNIDAD_MEDIDA, 
                                                        UNIDAD_MED_COMERCIAL, 
                                                        DETALLE, 
                                                        PRECIO, 
                                                        MONTO, 
                                                        SUB_TOTAL, 
                                                        IMPUESTO, 
                                                        IMP_PORCENTAJE, 
                                                        TOTAL_LINEA, 
                                                        COD_DOCUMENTO
                                                        ) VALUES (
                                                        @FAC_ID, 
                                                        @NUMERO_LINEA, 
                                                        @CODIGO, 
                                                        @CODIGO_COMERCIAL, 
                                                        @CANTIDAD, 
                                                        @UNIDAD_MEDIDA, 
                                                        @UNIDAD_MED_COMERCIAL, 
                                                        @DETALLE, 
                                                        @PRECIO, 
                                                        @MONTO, 
                                                        @SUB_TOTAL, 
                                                        @IMPUESTO, 
                                                        @IMP_PORCENTAJE, 
                                                        @TOTAL_LINEA, 
                                                        @COD_DOCUMENTO)
                                                        ";

                            var lineaParameters = new DynamicParameters();
                            lineaParameters.Add("FAC_ID", id);
                            lineaParameters.Add("NUMERO_LINEA", linea.numeroLinea, DbType.Int32);
                            lineaParameters.Add("CODIGO", linea.codigo, DbType.String);
                            lineaParameters.Add("CODIGO_COMERCIAL", linea.codigoComercial, DbType.String);
                            lineaParameters.Add("CANTIDAD", linea.cantidad, DbType.Decimal);
                            lineaParameters.Add("UNIDAD_MEDIDA", linea.unidadMedida, DbType.String);
                            lineaParameters.Add("UNIDAD_MED_COMERCIAL", linea.unidadMedidaComercial, DbType.String);
                            lineaParameters.Add("DETALLE", linea.detalle, DbType.String);
                            lineaParameters.Add("PRECIO", linea.precioUnitario, DbType.Decimal);
                            lineaParameters.Add("MONTO", linea.montoTotal, DbType.Decimal);
                            lineaParameters.Add("SUB_TOTAL", linea.subTotal, DbType.Decimal);
                            lineaParameters.Add("IMPUESTO", linea.impuesto, DbType.Decimal);
                            lineaParameters.Add("IMP_PORCENTAJE", porcentajeImpuesto, DbType.Decimal);
                            lineaParameters.Add("TOTAL_LINEA", linea.montoTotalLinea, DbType.Decimal);
                            lineaParameters.Add("COD_DOCUMENTO", request.cod_documento, DbType.String);

                            connection.ExecuteAsync(lineaQuery, lineaParameters).Wait();
                        }
                    }

                    // Envia correo al proveedor del registro de la factura

                    CorreoNotificaRegistroFactura_Enviar(CodEmpresa,request.nombre_prov, request.cod_documento, request.ced_jur_prov, request.registro_usuario);



                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Actualiza la factura XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Cargador_Facturas_Actualizar(int CodEmpresa, CprFacturasXML_DTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = @"UPDATE CPR_FACTURAS_XML 
                           SET COD_UEN = @Cod_UEN
                           WHERE COD_DOCUMENTO = @Cod_Documento AND CLAVE = @Clave AND CED_JUR_PROV = @Ced_Jur_Prov";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_UEN", request.cod_uen, DbType.String);
                    parameters.Add("Cod_Documento", request.cod_documento, DbType.String);
                    parameters.Add("Clave", request.clave, DbType.String);
                    parameters.Add("Ced_Jur_Prov", request.ced_jur_prov, DbType.String);


                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los detalles de una factura XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDTO<List<CprFacturasLineasXML_Data>> Cargador_FacturasDetalle_Obtener(int CodEmpresa, int id, string? cod_proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CprFacturasLineasXML_Data>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string query = "SELECT FAC_ID, NUMERO_LINEA as numeroLinea, CODIGO, " +
                    "CODIGO_COMERCIAL as codigoComercial, CANTIDAD, UNIDAD_MEDIDA as unidadMedida, " +
                    "UNIDAD_MED_COMERCIAL as unidadMedidaComercial, DETALLE, PRECIO as precioUnitario, " +
                    "MONTO as montoTotal, SUB_TOTAL as subTotal, IMPUESTO, IMP_PORCENTAJE," +
                    " TOTAL_LINEA as montoTotalLinea, COD_DOCUMENTO " +
                    "FROM CPR_FACTURAS_XML_DETALLE WHERE FAC_ID = @id";
                response.Result = connection.Query<CprFacturasLineasXML_Data>(query, new { id }).ToList();

                //verifica si el proveedor es diferente de 0
                query = $@"SELECT TOP 1 COD_PROVEEDOR  FROM CXP_PROVEEDORES cp 
                                WHERE REPLACE(REPLACE(cp.CEDJUR, ' ', ''), '-', '')  IN (
	                                SELECT REPLACE(REPLACE(cfx.CED_JUR_PROV, ' ', ''), '-', '') FROM CPR_FACTURAS_XML cfx 
	                                WHERE cfx.ID = @id
                                )";
                var cedJur = connection.QueryFirstOrDefault<string>(query, new { id });

                //verifica si los c�digos existen en inventario
                foreach (var linea in response.Result)
                {


                    string queryCodigo = $@"SELECT TOP 1 cspcl.COD_PRODUCTO, pp.DESCRIPCION FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS cspcl 
                                            LEFT JOIN PV_PRODUCTOS pp ON pp.COD_PRODUCTO = cspcl.COD_PRODUCTO
                                            left JOIN CPR_SOLICITUD_PROV_COTIZA cspc ON cspc.ID_COTIZACION = cspcl.ID_COTIZACION 
                                            WHERE cspcl.CODIGO = @codgo AND cspc.PROVEEDOR_CODIGO = @cod_proveedor ";
                    CprValidaProducto valida = connection.QueryAsync<CprValidaProducto>(queryCodigo, new { codgo = linea.codigoComercial, cod_proveedor = cedJur }).Result.FirstOrDefault();

                    if (valida == null)
                    {
                        linea.inv_existe = false;
                    }
                    else
                    {
                        linea.codigo = valida.COD_PRODUCTO;
                        linea.detalle = valida.DESCRIPCION;
                        linea.inv_existe = true;
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

        public ErrorDTO<CprFacturasXMLLista> Cargador_FacturasActivas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CprFacturasXMLFiltros filtro = JsonConvert.DeserializeObject<CprFacturasXMLFiltros>(filtros) ?? new CprFacturasXMLFiltros();
            var response = new ErrorDTO<CprFacturasXMLLista>
            {
                Code = 0,
                Result = new CprFacturasXMLLista
                {
                    total = 0
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                string where = "";
                string paginaActual = "";
                string paginacionActual = "";
                string? cedJurProveedor = null;

                if (proveedor != 0)
                {
                    cedJurProveedor = connection.QueryFirstOrDefault<string>(
                        "SELECT cedjur FROM cxp_proveedores WHERE cod_proveedor = @CodProveedor",
                        new { CodProveedor = proveedor });

                    if (string.IsNullOrEmpty(cedJurProveedor))
                    {
                        response.Code = -2;
                        response.Description = "No se encontr� C�dula Jur�dica para el proveedor seleccionado. Verifique el registro de proveedor";
                        response.Result = null;
                        return response;
                    }

                    where += $"WHERE REPLACE(REPLACE(ced_jur_prov, ' ', ''), '-', '')  = '{cedJurProveedor.Replace("-", "")}' ";
                }

                // Filtro de b�squeda general
                if (filtro.filtro != null)
                {
                    var busqueda = $"( cod_documento LIKE '%{filtro.filtro}%' OR nombre_prov LIKE '%{filtro.filtro}%' OR ced_jur_prov LIKE '%{filtro.filtro}%' )";

                    if (string.IsNullOrEmpty(where))
                        where = $"WHERE {busqueda} ";
                    else
                        where += $"AND {busqueda} ";
                }

                // Paginaci�n
                if (filtro.pagina != null)
                {
                    paginaActual = $" OFFSET {filtro.pagina} ROWS ";
                    paginacionActual = $" FETCH NEXT {filtro.paginacion} ROWS ONLY ";
                }

                // Consulta total y lista de facturas
                string countQuery = $"SELECT COUNT(*) FROM CPR_FACTURAS_XML {where} AND ESTADO IN ('P', 'A')";
                response.Result.total = connection.Query<int>(countQuery).First();

                string dataQuery = $"SELECT * FROM CPR_FACTURAS_XML {where} AND ESTADO IN ('P', 'A') ORDER BY id DESC {paginaActual} {paginacionActual}";
                response.Result.lista = connection.Query<CprFacturasXML_DTO>(dataQuery).ToList();
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
        /// Notifica Asignacion Solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private async Task CorreoNotificaRegistroFactura_Enviar(int CodEmpresa, string proveedor, string factura, string ced_jur, string registro_usuario)
        {

            ErrorDTO resp = new ErrorDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<OrdenLineas> info = new List<OrdenLineas>();

            int CodCliente = CodEmpresa;

            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var queryEmail = @$"SELECT EMAIL FROM CXP_PROVEEDORES WHERE cedjur = '{ced_jur}'";
                    string EmailProveedor = connection.Query<string>(queryEmail).FirstOrDefault();

                }


                EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, Notificaciones);
                string body = @$"<html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                             <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Notificaci�n de carga de factura</title>
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
                                    <h2><strong>Notificaci�n de carga de factura</strong></h2>
                                    <p>Estimado/a {proveedor} La factura #{factura} se ha cargado.</p>";



                List<IFormFile> Attachments = new List<IFormFile>();


                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = EmailProveedor;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Notificaci�n de carga de factura";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                    }

                    BitacoraEnvioCorreo(new BitacoraComprasInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        consec = 0,
                        movimiento = "Registra",
                        detalle = $@"Env�o de correo de registro de factura #{factura} a {proveedor}",
                        registro_usuario = registro_usuario
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
        }


        /// <summary>
        /// Bitacora de envio de correo
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDTO BitacoraEnvioCorreo(BitacoraComprasInsertarDTO req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {


                    var strSQL = $@"INSERT INTO [dbo].[BITACORA_COMPRAS]
                                           ([ID_COMPRA]
                                           ,[CONSEC]
                                           ,[MOVIMIENTO]
                                           ,[DETALLE]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{req.id_bitacora}'
                                           ,{req.consec}
                                           ,'{req.movimiento}' 
                                           , '{req.detalle}'
                                           , getdate()
                                           , '{req.registro_usuario}' )";

                    resp.Code = connection.Execute(strSQL);
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

    }
}