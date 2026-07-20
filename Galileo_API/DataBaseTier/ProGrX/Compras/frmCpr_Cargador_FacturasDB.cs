using Dapper;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCprCargadorFacturasDB
    {
        private readonly PortalDB _portalDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly string _sendEmail;
        private readonly string _notificaciones;

        public FrmCprCargadorFacturasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _envioCorreoDB = new EnvioCorreoDB(config);

            _sendEmail = config.GetSection("AppSettings").GetSection("EnviaEmail").Value?.ToString() ?? string.Empty;
            _notificaciones = config.GetSection("AppSettings").GetSection("Notificaciones").Value?.ToString() ?? string.Empty;
        }


        // ===========================
        //  HELPERS (anti-duplication)
        // ===========================

        /// <summary>
        /// Limpia cédula jurídica (quita guiones y espacios)
        /// </summary>
        /// <param name="cedJur"></param>
        /// <returns></returns>
        private static string CleanCedJur(string cedJur)
        {
            return (cedJur ?? string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
        }


        /// <summary>
        /// Envía correo si está habilitado en configuración
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="registroUsuario"></param>
        /// <param name="bitacoraDetalle"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task SendEmailAndLogIfEnabled(int codEmpresa, string to, string subject, string body, string registroUsuario, string bitacoraDetalle)
        {
            if (!string.Equals(_sendEmail, "Y", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(to))
                return;

            var correoConfigResult = _envioCorreoDB.CorreoConfig(codEmpresa, _notificaciones);
            if (correoConfigResult == null || correoConfigResult.Code != 0 || correoConfigResult.Result == null)
                throw new InvalidOperationException($"No se pudo obtener la configuración de correo: {correoConfigResult?.Description}");

            var eConfig = correoConfigResult.Result;

            var emailRequest = new EmailRequest
            {
                To = to,
                From = eConfig.User,
                Subject = subject,
                Body = body,
                Attachments = new List<IFormFile>()
            };

            var resp = new ErrorDto();
            await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);

            BitacoraEnvioCorreo(new BitacoraComprasInsertarDto
            {
                EmpresaId = codEmpresa,
                consec = 0,
                movimiento = "Registra",
                detalle = bitacoraDetalle,
                registro_usuario = registroUsuario
            });
        }


        /// <summary>
        /// Obtiene la lista de facturas XML
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="proveedor"></param>
        /// <param name="filtros"></param>
        /// <param name="soloActivas"></param>
        /// <returns></returns>
        private ErrorDto<CprFacturasXmlLista> Cargador_Facturas_ObtenerCore(int codEmpresa, int proveedor, string filtros, bool soloActivas)
        {
            var filtro = JsonConvert.DeserializeObject<CprFacturasXmlFiltros>(filtros) ?? new CprFacturasXmlFiltros();

            var response = new ErrorDto<CprFacturasXmlLista>
            {
                Code = 0,
                Result = new CprFacturasXmlLista { total = 0, lista = new List<CprFacturasXmlDto>() }
            };

            try
            {
                string? cedJurLimpia = null;

                if (proveedor != 0)
                {
                    var cedJurProveedorResp = DbHelper.ExecuteSingleQuery<string>(
                        _portalDB,
                        codEmpresa,
                        "SELECT cedjur FROM cxp_proveedores WHERE cod_proveedor = @CodProveedor",
                        defaultValue: null,
                        parameters: new { CodProveedor = proveedor }
                    );

                    var cedJurProveedor = cedJurProveedorResp.Result;

                    if (string.IsNullOrWhiteSpace(cedJurProveedor))
                    {
                        response.Code = -2;
                        response.Description = "No se encontró Cédula Jurídica para el proveedor seleccionado. Verifique el registro de proveedor";
                        response.Result = null;
                        return response;
                    }

                    cedJurLimpia = CleanCedJur(cedJurProveedor);
                }

                var q = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro.Trim()}%";

                var offset = filtro.pagina.GetValueOrDefault(0);
                if (offset < 0) offset = 0;

                var fetch = filtro.paginacion.GetValueOrDefault(int.MaxValue);
                if (fetch <= 0) fetch = int.MaxValue;

                var sortField = (filtro.sortField ?? string.Empty)
                    .Trim()
                    .ToLowerInvariant() switch
                {
                    "cod_documento" => "cod_documento",
                    "nombre_prov" => "nombre_prov",
                    "estado" => "estado",
                    "monto_total" => "monto_total",
                    _ => "id"
                };
                var sortOrder = filtro.sortOrder == 1 ? 1 : -1;

                const string sql = @"SELECT *, COUNT(*) OVER() AS TotalRows
        FROM CPR_FACTURAS_XML
        WHERE (@SoloActivas = 0 OR ESTADO IN ('P','A'))
        AND (@CedJur IS NULL OR REPLACE(REPLACE(ced_jur_prov, ' ', ''), '-', '') = @CedJur)
        AND (@Q IS NULL OR (cod_documento LIKE @Q OR nombre_prov LIKE @Q OR ced_jur_prov LIKE @Q))
        ORDER BY
            CASE WHEN @SortField = 'cod_documento' AND @SortOrder = 1 THEN cod_documento END ASC,
            CASE WHEN @SortField = 'cod_documento' AND @SortOrder = -1 THEN cod_documento END DESC,
            CASE WHEN @SortField = 'nombre_prov' AND @SortOrder = 1 THEN nombre_prov END ASC,
            CASE WHEN @SortField = 'nombre_prov' AND @SortOrder = -1 THEN nombre_prov END DESC,
            CASE WHEN @SortField = 'estado' AND @SortOrder = 1 THEN estado END ASC,
            CASE WHEN @SortField = 'estado' AND @SortOrder = -1 THEN estado END DESC,
            CASE WHEN @SortField = 'monto_total' AND @SortOrder = 1 THEN monto_total END ASC,
            CASE WHEN @SortField = 'monto_total' AND @SortOrder = -1 THEN monto_total END DESC,
            id DESC
        OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var listResp = DbHelper.WithConn(_portalDB, codEmpresa, conn =>
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();

                    var total = 0;

                    var list = conn.Query<CprFacturasXmlDto, int, CprFacturasXmlDto>(
                        sql,
                        (dto, t) =>
                        {
                            total = t;
                            return dto;
                        },
                        new
                        {
                            SoloActivas = soloActivas ? 1 : 0,
                            CedJur = cedJurLimpia,
                            Q = q,
                            Offset = offset,
                            Fetch = fetch,
                            SortField = sortField,
                            SortOrder = sortOrder
                        },
                        splitOn: "TotalRows"
                    ).ToList();

                    return new CprFacturasXmlLista
                    {
                        total = total,
                        lista = list
                    };
                });

                if (listResp.Code != 0)
                {
                    response.Code = -1;
                    response.Description = listResp.Description;
                    response.Result = null;
                    return response;
                }

                response.Result = listResp.Result ?? new CprFacturasXmlLista { total = 0, lista = new List<CprFacturasXmlDto>() };
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
        /// Obtiene la lista de facturas XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proveedor"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CprFacturasXmlLista> Cargador_Facturas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            // Mantiene comportamiento original: solo activas (P, A)
            return Cargador_Facturas_ObtenerCore(CodEmpresa, proveedor, filtros, soloActivas: true);
        }


        /// <summary>
        /// Obtiene una factura XML por ID
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="id">ID de la factura</param>
        /// <returns>Factura encontrada o error</returns>
        public ErrorDto<CprFacturasXmlDto> Cargador_Factura_ObtenerPorId(int CodEmpresa, int id)
        {
            var response = new ErrorDto<CprFacturasXmlDto> { Code = 0 };

            try
            {
                // Factura
                var facturaResp = DbHelper.ExecuteSingleQuery<CprFacturasXmlDto>(
                    _portalDB,
                    CodEmpresa,
                    "SELECT * FROM CPR_FACTURAS_XML WHERE id = @id",
                    defaultValue: null,
                    parameters: new { id }
                );

                var factura = facturaResp.Result;
                if (factura == null)
                {
                    response.Code = -2;
                    response.Description = "Factura no encontrada";
                    return response;
                }

                // Proveedor por cédula jurídica (normalizada)
                var cedulaLimpia = factura.ced_jur_prov?.Replace("-", "").Replace(" ", "");

                var proveedorResp = DbHelper.ExecuteSingleQuery<CxpProveedorData>(
                    _portalDB,
                    CodEmpresa,
                    @"SELECT TOP 1 cod_proveedor, descripcion
                      FROM CXP_PROVEEDORES
                      WHERE REPLACE(REPLACE(cedjur, ' ', ''), '-', '') = @Cedula",
                    defaultValue: null,
                    parameters: new { Cedula = cedulaLimpia }
                );

                var proveedor = proveedorResp.Result;

                if (proveedor != null)
                {
                    factura.cod_proveedor = proveedor.Cod_Proveedor;
                    factura.descripcion = proveedor.Descripcion;
                }
                else
                {
                    factura.cod_proveedor = string.Empty;
                    factura.descripcion = string.Empty;
                }

                // Líneas
                var lineasResp = Cargador_FacturasDetalle_Obtener(CodEmpresa, id, proveedor?.Cod_Proveedor?.ToString());
                factura.lineas = lineasResp.Result;

                response.Result = factura;
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
        public ErrorDto Cargador_Facturas_Guardar(int CodEmpresa, CprFacturasXmlDto request)
        {
            var result = DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                conn.Open();
                using var tx = conn.BeginTransaction();

                // Verifica si la factura ya existe
                if (FacturaExiste(conn, tx, request))
                    return new ErrorDto { Code = -2, Description = "Factura ya registrada." };

                // Inserta encabezado y obtiene ID
                int id = InsertarFactura(conn, tx, request);

                // Inserta detalle (si aplica)
                InsertarLineas(conn, tx, id, request);

                tx.Commit();
                return DbHelper.OkResponse("Ok");
            });

            // Si falló la conexión / ejecución, devolvemos el error del helper
            if (result.Code != 0)
                return DbHelper.ErrorResponse(result.Description ?? "Error desconocido.", -1);

            // Unwrap: la acción devolvió un ErrorDto (éxito o -2)
            var resp = result.Result ?? DbHelper.ErrorResponse("Error desconocido.", -1);

            // Envía correo fuera de la transacción solo si salió OK
            if (resp.Code == 0)
            {
                try
                {
                    CorreoNotificaRegistroFactura_Enviar(
                        CodEmpresa,
                        request.nombre_prov,
                        request.cod_documento,
                        request.ced_jur_prov,
                        request.registro_usuario
                    ).GetAwaiter().GetResult();
                }
                catch
                {
                    // No rompe el guardado si el correo falla
                }
            }

            return resp;
        }


        /// <summary>
        /// Verifica si la factura ya existe
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static bool FacturaExiste(SqlConnection conn, IDbTransaction tx, CprFacturasXmlDto request)
        {
            const string sql = @"SELECT COUNT(*)
                                FROM CPR_FACTURAS_XML
                                WHERE COD_DOCUMENTO = @Cod_Documento
                                AND CLAVE = @Clave
                                AND CED_JUR_PROV = @Ced_Jur_Prov";

            int count = conn.ExecuteScalar<int>(
                sql,
                new
                {
                    Cod_Documento = request.cod_documento,
                    Clave = request.clave,
                    Ced_Jur_Prov = request.ced_jur_prov
                },
                tx
            );

            return count > 0;
        }


        /// <summary>
        /// Inserta la factura XML y devuelve el ID generado
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static int InsertarFactura(SqlConnection conn, IDbTransaction tx, CprFacturasXmlDto request)
        {
            const string sql = @"INSERT INTO CPR_FACTURAS_XML
                                (COD_UEN, COD_DOCUMENTO, CLAVE, CED_JUR_PROV, NOMBRE_PROV, MONTO_TOTAL,
                                COD_DIVISA, FECHA, ESTADO, REGISTRO_USUARIO, REGISTRO_FECHA)
                                VALUES
                                (@Cod_UEN, @Cod_Documento, @Clave, @Ced_Jur_Prov, @Nombre_Prov, @Monto_Total,
                                @Cod_Divisa, @Fecha, @Estado, @Registro_Usuario, @Registro_Fecha);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return conn.QuerySingle<int>(
                sql,
                new
                {
                    Cod_UEN = request.cod_uen,
                    Cod_Documento = request.cod_documento,
                    Clave = request.clave,
                    Ced_Jur_Prov = request.ced_jur_prov,
                    Nombre_Prov = request.nombre_prov,
                    Monto_Total = request.monto_total,
                    Cod_Divisa = request.cod_divisa,
                    Fecha = request.fecha,
                    Estado = request.estado,
                    Registro_Usuario = request.registro_usuario,
                    Registro_Fecha = request.registro_fecha
                },
                tx
            );
        }


        /// <summary>
        /// Inserta las líneas de la factura XML
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tx"></param>
        /// <param name="facId"></param>
        /// <param name="request"></param>
        private static void InsertarLineas(SqlConnection conn, IDbTransaction tx, int facId, CprFacturasXmlDto request)
        {
            if (request.lineas == null || request.lineas.Count == 0)
                return;

            const string sql = @"INSERT INTO CPR_FACTURAS_XML_DETALLE
                                (FAC_ID, NUMERO_LINEA, CODIGO, CODIGO_COMERCIAL, CANTIDAD, UNIDAD_MEDIDA,
                                UNIDAD_MED_COMERCIAL, DETALLE, PRECIO, MONTO, SUB_TOTAL, IMPUESTO,
                                IMP_PORCENTAJE, TOTAL_LINEA, COD_DOCUMENTO)
                                VALUES
                                (@FAC_ID, @NUMERO_LINEA, @CODIGO, @CODIGO_COMERCIAL, @CANTIDAD, @UNIDAD_MEDIDA,
                                @UNIDAD_MED_COMERCIAL, @DETALLE, @PRECIO, @MONTO, @SUB_TOTAL, @IMPUESTO,
                                @IMP_PORCENTAJE, @TOTAL_LINEA, @COD_DOCUMENTO);";

            foreach (var l in request.lineas)
            {
                var impPorcentaje = (l.precioUnitario == 0) ? 0 : (l.impuesto / l.precioUnitario) * 100;

                conn.Execute(
                    sql,
                    new
                    {
                        FAC_ID = facId,
                        NUMERO_LINEA = l.numeroLinea,
                        CODIGO = l.codigo,
                        CODIGO_COMERCIAL = l.codigoComercial,
                        CANTIDAD = l.cantidad,
                        UNIDAD_MEDIDA = l.unidadMedida,
                        UNIDAD_MED_COMERCIAL = l.unidadMedidaComercial,
                        DETALLE = l.detalle,
                        PRECIO = l.precioUnitario,
                        MONTO = l.montoTotal,
                        SUB_TOTAL = l.subTotal,
                        IMPUESTO = l.impuesto,
                        IMP_PORCENTAJE = impPorcentaje,
                        TOTAL_LINEA = l.montoTotalLinea,
                        COD_DOCUMENTO = request.cod_documento
                    },
                    tx
                );
            }
        }


        /// <summary>
        /// Actualiza la factura XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cargador_Facturas_Actualizar(int CodEmpresa, CprFacturasXmlDto request)
        {
            try
            {
                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    @"UPDATE CPR_FACTURAS_XML
                      SET COD_UEN = @Cod_UEN
                      WHERE COD_DOCUMENTO = @Cod_Documento
                        AND CLAVE = @Clave
                        AND CED_JUR_PROV = @Ced_Jur_Prov",
                    new
                    {
                        Cod_UEN = request.cod_uen,
                        Cod_Documento = request.cod_documento,
                        Clave = request.clave,
                        Ced_Jur_Prov = request.ced_jur_prov
                    }
                );
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


        /// <summary>
        /// Obtiene los detalles de una factura XML
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <param name="cod_proveedor"></param>
        /// <returns></returns>
        public ErrorDto<List<CprFacturasLineasXmlData>> Cargador_FacturasDetalle_Obtener(int CodEmpresa, int id, string? cod_proveedor)
        {
            var response = new ErrorDto<List<CprFacturasLineasXmlData>> { Code = 0 };

            try
            {
                var r = DbHelper.WithConn<List<CprFacturasLineasXmlData>>(_portalDB, CodEmpresa, conn =>
                {
                    // Detalle (parametrizado)
                    var lineas = conn.Query<CprFacturasLineasXmlData>(
                        @"SELECT FAC_ID,
                                 NUMERO_LINEA as numeroLinea,
                                 CODIGO,
                                 CODIGO_COMERCIAL as codigoComercial,
                                 CANTIDAD,
                                 UNIDAD_MEDIDA as unidadMedida,
                                 UNIDAD_MED_COMERCIAL as unidadMedidaComercial,
                                 DETALLE,
                                 PRECIO as precioUnitario,
                                 MONTO as montoTotal,
                                 SUB_TOTAL as subTotal,
                                 IMPUESTO,
                                 IMP_PORCENTAJE,
                                 TOTAL_LINEA as montoTotalLinea,
                                 COD_DOCUMENTO
                          FROM CPR_FACTURAS_XML_DETALLE
                          WHERE FAC_ID = @id",
                        new { id }
                    ).ToList();

                    // Obtiene cod proveedor asociado a la cédula de la factura (parametrizado)
                    var provCod = conn.QueryFirstOrDefault<string>(
                        @"SELECT TOP 1 COD_PROVEEDOR
                          FROM CXP_PROVEEDORES cp
                          WHERE REPLACE(REPLACE(cp.CEDJUR, ' ', ''), '-', '') IN
                          (
                              SELECT REPLACE(REPLACE(cfx.CED_JUR_PROV, ' ', ''), '-', '')
                              FROM CPR_FACTURAS_XML cfx
                              WHERE cfx.ID = @id
                          )",
                        new { id }
                    );

                    // Verifica códigos en inventario (parametrizado)
                    foreach (var linea in lineas)
                    {
                        var valida = conn.QueryFirstOrDefault<CprValidaProducto>(
                            @"SELECT TOP 1 cspcl.COD_PRODUCTO, pp.DESCRIPCION
                              FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS cspcl
                              LEFT JOIN PV_PRODUCTOS pp ON pp.COD_PRODUCTO = cspcl.COD_PRODUCTO
                              LEFT JOIN CPR_SOLICITUD_PROV_COTIZA cspc ON cspc.ID_COTIZACION = cspcl.ID_COTIZACION
                              WHERE cspcl.CODIGO = @codgo
                                AND cspc.PROVEEDOR_CODIGO = @cod_proveedor",
                            new
                            {
                                codgo = linea.codigoComercial,
                                cod_proveedor = provCod
                            }
                        );

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

                    return lineas;
                });

                if (r.Code != 0)
                {
                    response.Code = -1;
                    response.Description = r.Description;
                    response.Result = null;
                    return response;
                }

                response.Result = r.Result;
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
        /// Obtiene la lista de facturas XML activas (P, A)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proveedor"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CprFacturasXmlLista> Cargador_FacturasActivas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            // Activas (P, A)
            return Cargador_Facturas_ObtenerCore(CodEmpresa, proveedor, filtros, soloActivas: true);
        }


        /// <summary>
        /// Envía correo de notificación de registro de factura
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proveedor"></param>
        /// <param name="factura"></param>
        /// <param name="ced_jur"></param>
        /// <param name="registro_usuario"></param>
        /// <returns></returns>
        private async Task CorreoNotificaRegistroFactura_Enviar(int CodEmpresa, string proveedor, string factura, string ced_jur, string registro_usuario)
        {
            try
            {
                var emailResp = DbHelper.ExecuteSingleQuery<string>(
                    _portalDB,
                    CodEmpresa,
                    "SELECT TOP 1 EMAIL FROM CXP_PROVEEDORES WHERE cedjur = @CedJur",
                    defaultValue: string.Empty,
                    parameters: new { CedJur = ced_jur }
                );

                var emailProveedor = emailResp.Result ?? string.Empty;

                var body = @$"<html lang=""es"">
        <head>
        <meta charset=""UTF-8"">
        <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Notificación de carga de factura</title>
        </head>
        <body>
        <h2><strong>Notificación de carga de factura</strong></h2>
        <p>Estimado/a {proveedor} La factura #{factura} se ha cargado.</p>
        </body>
        </html>";

                await SendEmailAndLogIfEnabled(
                    CodEmpresa,
                    emailProveedor,
                    "Notificación de carga de factura",
                    body,
                    registro_usuario,
                    $@"Envío de correo de registro de factura #{factura} a {proveedor}"
                );
            }
            catch
            {
                // no rompe el flujo si falla el correo
            }
        }


        /// <summary>
        /// Bitacora de envio de correo
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto BitacoraEnvioCorreo(BitacoraComprasInsertarDto req)
        {
            try
            {
                return DbHelper.ExecuteNonQuery(
                    _portalDB,
                    req.EmpresaId,
                    @"INSERT INTO [dbo].[BITACORA_COMPRAS]
                      ([ID_COMPRA],[CONSEC],[MOVIMIENTO],[DETALLE],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                      VALUES
                      (@id_bitacora,@consec,@movimiento,@detalle,GETDATE(),@registro_usuario)",
                    req
                );
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
