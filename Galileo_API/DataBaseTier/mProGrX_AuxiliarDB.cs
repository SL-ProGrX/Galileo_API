using Dapper;
using Humanizer;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using ServicioDePruebaWCF;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PgxAPI.DataBaseTier
{
    public class mProGrX_AuxiliarDB
    {
        private readonly IConfiguration _config;

        private IService1 client = new Service1Client();
        public string dateFormat = "";
        private string controlAuth = "";

        public mProGrX_AuxiliarDB(IConfiguration config)
        {
            _config = config;
            dateFormat = _config.GetSection("AppSettings").GetSection("DateTimeFormat").Value.ToString();
            controlAuth = _config.GetSection("AppSettings").GetSection("ControlAutorizacion").Value.ToString();
        }

        /// <summary>
        /// Busca en la tabla de periodos si existe un periodo cerrado posterior al periodo que se desea cerrar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vfecha"></param>
        /// <returns></returns>
        public bool fxInvPeriodos(int CodEmpresa, string vfecha)
        {
            bool vPasa = false;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            //Verificar si existen posteriores Cerrados
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT ISNULL(COUNT(*),0) AS Existe FROM pv_periodos WHERE mes > MONTH('{vfecha}')  AND anio = YEAR('{vfecha}') AND estado = 'C'";
                    var resp = connection.Query<int>(query).FirstOrDefault();
                    if (resp > 0)
                    {
                        vPasa = false;
                    }
                    else
                    {
                        vPasa = true;
                    }

                    if (vPasa)
                    {
                        query = $@"Select estado from pv_periodos where anio = YEAR('{vfecha}') AND mes = MONTH('{vfecha}') ";
                        var estado = connection.Query<string>(query).FirstOrDefault();
                        if (estado == "C")
                        {
                            vPasa = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                vPasa = false;
                _ = ex.Message;
            }
            return vPasa;
        }

        /// <summary>
        /// Registra un movimiento de inventario en la tabla de afectaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDTO sbInvInventario(int CodEmpresa, CompraInventarioDTO req)
        {
            ErrorDTO result = new ErrorDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "spINVAfectacion";
                    var parameters = new
                    {
                        @CodProd = req.CodProducto,
                        @Cantidad = req.Cantidad,
                        @Bodega = req.CodBodega,
                        @CodTipo = req.CodTipo,
                        @Origen = req.Origen,
                        @Fecha = req.Fecha,
                        @Precio = req.Precio,
                        @ImpCon = req.ImpConsumo,
                        @ImpVenta = req.ImpVentas,
                        @TipoMov = req.TipoMov,
                        @Usuario = req.Usuario
                    };

                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);
                    result.Code = 0;
                    result.Description = "ok";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Verifica la existencia de productos y bodegas en la tabla de inventario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ColCantidad"></param>
        /// <param name="vMov"></param>
        /// <param name="ColProd"></param>
        /// <param name="ColBod1"></param>
        /// <param name="ColBod2"></param>
        /// <param name="vGrid"></param>
        /// <returns></returns>
        public ErrorDTO fxInvVerificaLineaDetalle(int CodEmpresa, int ColCantidad, string vMov, int ColProd, int ColBod1, int ColBod2, List<FacturaDetalleDto> vGrid)
        {
            ErrorDTO result = new ErrorDTO();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            int count = 0;
            result.Code = 1;

            if (ColProd > 0 && vGrid.Count > 0)
            {
                foreach (FacturaDetalleDto item in vGrid)
                {
                    count++;
                    //Verifica la Cantidad de Articulos
                    if (item.cantidad > 0)
                    {
                        // Verifica que el Producto este Activo, y Que Existe
                        try
                        {
                            using var connection = new SqlConnection(stringConn);
                            {
                                var query = $@"select estado from pv_productos where cod_producto = '{item.cod_producto}' ";
                                List<ProductoDTO> exist = connection.Query<ProductoDTO>(query).ToList();

                                if (exist.Count == 0)
                                {
                                    result.Code = 0;
                                    result.Description += $"\nL {count} - Producto : {item.cod_producto} - No Existe";
                                }
                                else
                                {
                                    if (exist[0].Estado == "I")
                                    {
                                        result.Code = 0;
                                        result.Description += $"\nL {count} - Producto : {item.cod_producto} - Se encuentra Inactivo";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Code = -1;
                            result.Description = ex.Message;
                        }

                        if (ColBod1 > 0)
                        {
                            //Verifica que la Bodega Exista y que Permita Registrar el Tipo de Movimiento
                            try
                            {
                                using var connection = new SqlConnection(stringConn);
                                {
                                    var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{item.cod_bodega}' ";
                                    List<BodegaDTO> exist = connection.Query<BodegaDTO>(query).ToList();

                                    if (exist.Count == 0)
                                    {
                                        result.Code = 0;
                                        result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Existe";
                                    }
                                    else
                                    {
                                        if (exist[0].estado == "I")
                                        {
                                            result.Code = 0;
                                            result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - Se encuentra Inactiva";
                                        }
                                        else
                                        {
                                            switch (vMov)
                                            {
                                                case "E":
                                                    if (exist[0].permite_entradas == "0")
                                                    {
                                                        result.Code = 0;
                                                        result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Permite Entradas";
                                                    }
                                                    break;
                                                case "S":
                                                case "R":
                                                case "T":
                                                    if (exist[0].permite_salidas == "0")
                                                    {
                                                        result.Code = 0;
                                                        result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Permite Salidas";
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Code = -1;
                                result.Description = ex.Message;
                            }
                        }

                        //Verifica que el Producto Exista en la Bodega
                        if (ColBod2 > 0)
                        {
                            try
                            {
                                using var connection = new SqlConnection(stringConn);
                                {
                                    var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{item.cod_bodega}' ";
                                    List<BodegaDTO> exist = connection.Query<BodegaDTO>(query).ToList();

                                    if (exist.Count == 0)
                                    {
                                        result.Code = 0;
                                        result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Existe";
                                    }
                                    else
                                    {
                                        if (exist[0].estado == "I")
                                        {
                                            result.Code = 0;
                                            result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - Se encuentra Inactiva";
                                        }
                                        else
                                        {
                                            switch (vMov)
                                            {
                                                case "E":
                                                case "T":

                                                    if (exist[0].permite_entradas == "0")
                                                    {
                                                        result.Code = 0;
                                                        result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Permite Entradas";
                                                    }
                                                    break;

                                                case "R":
                                                case "S":
                                                    if (exist[0].permite_salidas == "0")
                                                    {
                                                        result.Code = 0;
                                                        result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Permite Salidas";
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Verifica si el periodo de inventario está cerrado o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vfecha"></param>
        /// <returns></returns>
        public bool fxInvPeriodoEstado(int CodEmpresa, string vfecha)
        {
            bool vPasa = false;
            string vNum = "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            //Verificar si existen posteriores Cerrados
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select estado from pv_periodos where anio = YEAR('{vfecha}')  and mes = MONTH('{vfecha}') ";
                    vNum = connection.Query<string>(query).FirstOrDefault();
                    if (vNum == "C")
                    {
                        vPasa = false;
                    }
                    else
                    {
                        vPasa = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return vPasa;
        }

        /// <summary>
        /// Consulta los parámetros de la tabla cxp_parametros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Parametro"></param>
        /// <returns></returns>
        public ErrorDTO<ParametroValor> fxCxPParametro(int CodEmpresa, string Cod_Parametro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<ParametroValor>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT cod_parametro, valor FROM cxp_parametros WHERE cod_parametro = '{Cod_Parametro}'";

                    response.Result = connection.Query<ParametroValor>(query).FirstOrDefault();

                    if (response.Result.Valor == null)
                    {
                        response.Result.Valor = "GEN";
                        response.Result.Cod_Parametro = Cod_Parametro;
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
        /// Valida si el usuario que genera la transacción tiene autorización para autorizarla
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Boleta"></param>
        /// <param name="TipoTran"></param>
        /// <param name="AutorizaUser"></param>
        /// <returns></returns>
        public ErrorDTO fxInvTransaccionesAutoriza(int CodEmpresa, string Boleta, string TipoTran, string AutorizaUser)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO info = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query1 = $@"select genera_user from pv_invTransac where Tipo = '{TipoTran}' and Boleta = '{Boleta}'";
                    var generaUser = connection.ExecuteScalar<string>(query1);

                    if (string.IsNullOrEmpty(generaUser))
                    {
                        info.Code = 0;
                        info.Description = $"No se encontró el usuario que generó la boleta '{Boleta}', verifique que la boleta exista";
                        return info;
                    }

                    var query2 = $@"select isnull(count(*),0) as Existe from pv_orden_autousers where Usuario = '{AutorizaUser}' and Usuario_Asignado = '{generaUser}' and ENTRADAS = 1";
                    int valideAutorizacion = connection.ExecuteScalar<int>(query2);

                    if (valideAutorizacion == 1)
                    {
                        info.Code = valideAutorizacion;
                        info.Description = generaUser;
                        return info;
                    }
                    else
                    {
                        info.Code = valideAutorizacion;
                        info.Description = "Usted no se encuentra Registrado como Autorizado del Usuario " + generaUser + " que Generó la Transacción...(Verifique)";
                        return info;
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Consulta la descripción de un código en una tabla específica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vTipoDC"></param>
        /// <param name="vCodDesX"></param>
        /// <param name="vTabla"></param>
        /// <param name="Cod_Conta"></param>
        /// <returns></returns>
        public ConsultaDescripcion fxSIFCCodigos(int CodEmpresa, string vTipoDC, string vCodDesX, string vTabla, int Cod_Conta)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ConsultaDescripcion info = new ConsultaDescripcion();

            string strSQL = "";
            string tableName = vTabla.ToUpper();
            string codeFilter = vCodDesX.ToString();
            string descFilter = vCodDesX.ToString();

            switch (tableName)
            {
                case "PROVEEDORES":
                    strSQL = "select Cod_proveedor as CodX, Descripcion as DescX from cxp_proveedores";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_proveedor = " + codeFilter;
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "PRODUCTOS":
                    strSQL = "select cod_Producto as CodX, Descripcion as DescX from pv_Productos";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_producto = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "CARGOSPROV":
                    strSQL = "select Cod_cargo as CodX, Descripcion as DescX from cxp_cargos";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_cargo = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "UNIDADES":
                    strSQL = "select Cod_Unidad as CodX, Descripcion as DescX from pv_unidades";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_unidad = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "MARCAS":
                    strSQL = "select Cod_Marca as CodX, Descripcion as DescX from pv_marcas";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_marca = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "LINEAPRODUCTO":
                    strSQL = "select cod_prodclas as CodX, Descripcion as DescX from pv_prod_clasifica";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_prodclas = " + codeFilter;
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "BANCOS":
                    strSQL = "select id_banco as CodX, Descripcion as DescX from Tes_Bancos";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where id_banco = " + codeFilter;
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "CLIENTES":
                    strSQL = "select cedula as CodX, nombre as DescX from pv_clientes";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cedula = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where nombre = '" + descFilter + "'";
                    }
                    break;

                case "BODEGAS":
                    strSQL = "select cod_bodega as CodX, Descripcion as DescX from pv_bodegas";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_bodega = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "PRECIOS":
                    strSQL = "select cod_precio as CodX, Descripcion as DescX from pv_tipos_precios";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_precio = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    break;

                case "AGENTES":
                    strSQL = "select cod_agente as CodX, Nombre as DescX from pv_agentes";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_agente = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where nombre = '" + descFilter + "'";
                    }
                    break;

                case "CAJAS":
                    strSQL = "select cod_caja as CodX, Nombre as DescX from pv_cajas";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_caja = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where nombre = '" + descFilter + "'";
                    }
                    break;

                case "CUENTAS":
                    strSQL = "select cod_Cuenta as CodX, Descripcion as DescX from CntX_cuentas";
                    if (vTipoDC == "D")
                    {
                        strSQL += " where cod_cuenta = '" + codeFilter + "'";
                    }
                    else
                    {
                        strSQL += " where descripcion = '" + descFilter + "'";
                    }
                    strSQL += " and cod_contabilidad = " + Cod_Conta;
                    break;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    info = connection.Query<ConsultaDescripcion>(strSQL).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public bool fxCorreoValido(string correo)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, patron);
        }

        /// <summary>
        /// Convierte un modelo a XML para enviarlo a SP de BD
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string fxConvertModelToXml<T>(T model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            string xmlOutput;
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, model);
                xmlOutput = writer.ToString();
            }

            // Limpieza del XML
            xmlOutput = xmlOutput.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            xmlOutput = xmlOutput.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
            xmlOutput = xmlOutput.Trim();
            xmlOutput = xmlOutput.Replace(" xsi:nil=\"true\" ", "");
            xmlOutput = xmlOutput.Replace("false", "0").Replace("true", "1");

            return xmlOutput;
        }

        /// <summary>
        /// Prueba de conexión al servicio WCF
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="numVal"></param>
        /// <returns></returns>
        public string WCF_ApiTest(int CodEmpresa, int numVal)
        {
            ErrorDTO info = new ErrorDTO();
            try
            {
                var response = client.GetDataAsync(numVal);
                return response.Result;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                return ex.Message;
            }

        }

        /// <summary>
        /// Consulta la cantidad de activos sin asignar a un usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<int> ActivosSinAsignar_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<int> result = new ErrorDTO<int>();
            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    var query = $@"select COUNT(*)  FROM PV_CONTROL_ACTIVOS
                                    WHERE ENTREGA_USUARIO = ''
                                    AND ESTADO IN ('P', 'R') AND REGISTRO_USUARIO = '{usuario}'";
                    result.Result = connection.QueryFirstOrDefault<int>(query);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }
            return result;
        }

        /// <summary>
        /// Valida la fecha de un campo DateTime, si es nulo devuelve null de lo contrario devuelve la fecha en el formato definido en el appsettings
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        public string validaFechaGlobal(DateTime? fecha)
        {
            string fechaValdiada = "";
            try
            {
                if (fecha != null)
                {
                    DateTime fechaActual = (DateTime)fecha;
                    fechaValdiada = fechaActual.ToString(dateFormat);
                }
            }
            catch (Exception)
            {
                fechaValdiada = null;
            }
            return fechaValdiada;
        }

        /// <summary>
        /// Inserta un registro en la tabla de bitácora de productos
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDTO BitacoraProducto(BitacoraProductoInsertarDTO req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var strSQL = $@"INSERT INTO [dbo].[BITACORA_PRODUCTOS]
                                           ([COD_PRODUCTO]
                                           ,[CONSEC]
                                           ,[MOVIMIENTO]
                                           ,[DETALLE]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{req.cod_producto}'
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

        /// <summary>
        /// Inserta un registro en la tabla de bitácora de proveedores
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDTO BitacoraProveedor(BitacoraProveedorInsertarDTO req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    

                    var strSQL = $@"INSERT INTO [dbo].[BITACORA_PROVEEDOR]
                                           ([COD_PROVEEDOR]
                                           ,[CONSEC]
                                           ,[MOVIMIENTO]
                                           ,[DETALLE]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{req.cod_proveedor}'
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

        public ErrorDTO<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select CODIGO_SINPE as item, rtrim(Descripcion) as descripcion from AFI_TIPOS_IDS " +
                        " order by Tipo_Id ";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public ErrorDTO<string> NumeroALetras(decimal numero)
        {
            var resp = new ErrorDTO<string>();
            resp.Code = 0;
            long parteEntera = (long)Math.Floor(numero);
            int parteDecimal = (int)((numero - parteEntera) * 100);

            string letrasEntera = parteEntera.ToWords(new System.Globalization.CultureInfo("es"));
            if (letrasEntera.ToLower() == "uno")
            {
                letrasEntera = "Un";
            }
            // Asegurarse de que la primera letra esté en mayúscula
            letrasEntera = char.ToUpper(letrasEntera[0]) + letrasEntera.Substring(1);

            string letrasDecimal = parteDecimal > 0 ? $" con {parteDecimal.ToWords(new System.Globalization.CultureInfo("es"))} " : "";

            resp.Result = letrasEntera + letrasDecimal;
            return resp;
        }

        /// <summary>
        /// Combina información de varios pdf en uno solo
        /// El parametro que se debe pasar es el array de bytes 
        /// </summary>
        /// <param name="pdfs"></param>
        /// <returns></returns>
        public byte[] CombinarBytesPdfSharp(params byte[][] pdfs)
        {
            using var outDoc = new PdfDocument();
            foreach (var pdf in pdfs)
            {
                if (pdf == null || pdf.Length == 0) continue;
                using var msIn = new MemoryStream(pdf);
                using var src = PdfReader.Open(msIn, PdfDocumentOpenMode.Import);
                for (int i = 0; i < src.PageCount; i++)
                    outDoc.AddPage(src.Pages[i]);
            }
            using var msOut = new MemoryStream();
            outDoc.Save(msOut, false);
            return msOut.ToArray();
        }

        #region Metodo FONDOS v6 para migrar

        /// <summary>
        /// Valida string de control de autorizacion para guardar cambios en la tabla de control
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public int FndControlAutoriza_Guardar(FndControlAutorizaData request)
        {
            ErrorDTO result = new ErrorDTO();
            result.Code = 0;
            

            if (controlAuth == "Y")
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);
                /*
                 COD_TIPO_CAMBIO:
                 1 = Insertar
                 2 = Modificar
                 3 = Eliminar
                 */

                try
                {
                    //Obtengo el where de la consulta
                    string pattern = @"UPDATE\s+(?<table>\w+)\s+SET\s+(?<setClause>.+?)\s+WHERE\s+(?<whereClause>.+)$";
                    var match = Regex.Match(request.strSQL, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (!match.Success)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o no se puede analizar.";
                        return result.Code.Value;
                    }

                    string table = match.Groups["table"].Value.Trim();
                    string setClause = match.Groups["setClause"].Value.Trim();
                    string whereClause = match.Groups["whereClause"].Value.Trim();

                    // Construir query para campos actuales
                    string query = $"SELECT * FROM {table} WHERE {whereClause}";

                    using (var connection = new SqlConnection(stringConn))
                    {
                        //valores originales
                        var dtTable = connection.Query(query).FirstOrDefault();
                        if (dtTable == null)
                        {
                            result.Code = -1;
                            result.Description = $"❌ No se encontró información en {table} con la condición: {whereClause}";
                            return result.Code.Value;
                        }

                        //procesa Update para realizar comparacion de valores

                        
                        List<string> selectParts = new List<string>();
                        foreach (var assignment in SplitSetClauseSafely(setClause))
                        {
                            var splitIndex = assignment.IndexOf('=');
                            if (splitIndex > 0)
                            {
                                var column = assignment.Substring(0, splitIndex).Trim();
                                var expression = assignment.Substring(splitIndex + 1).Trim();
                                selectParts.Add($"{expression} AS {column.ToUpper()}");
                            }
                        }

                        string selectStatement = $"SELECT {string.Join(", ", selectParts)} FROM {table} WHERE {whereClause};";
                        var dtTableNew = connection.Query(selectStatement).FirstOrDefault();

                        var diferencias = new List<(string Campo, object ValorOriginal, object ValorNuevo)>();

                        // Convertir los resultados a diccionarios para facilitar la comparación
                        var dicOriginal = ((IDictionary<string, object>)dtTable);
                        var dicNuevo = ((IDictionary<string, object>)dtTableNew);

                        foreach (var kvp in dicOriginal)
                        {
                            var key = kvp.Key;
                            var valorOriginal = kvp.Value;

                            if (dicNuevo.TryGetValue(key, out var valorNuevo))
                            {
                                if (!SonIguales(valorOriginal, valorNuevo))
                                {
                                    diferencias.Add((key, valorOriginal, valorNuevo));
                                }
                            }
                            
                        }

                        var dtDiferencias = new DataTable();

                        dtDiferencias.Columns.Add("Campo", typeof(string));
                        dtDiferencias.Columns.Add("ValorOriginal", typeof(object));
                        dtDiferencias.Columns.Add("ValorNuevo", typeof(object));

                        foreach (var dif in diferencias)
                        {
                            dtDiferencias.Rows.Add(dif.Campo, dif.ValorOriginal, dif.ValorNuevo);
                        }

                        if(dtDiferencias.Rows.Count == 0)
                        {
                            result.Code = 2;
                            result.Description = "No se encontraron diferencias entre los valores originales y los nuevos.";
                            return result.Code.Value;
                        }
                        

                        result = InsertarTablaControl(
                            request.CodEmpresa, 
                            request.tipoCambio, 
                            table, 
                            whereClause, 
                            request.usuario, 
                            "UPDATE",
                            null,
                            dtDiferencias) ;
                        

                    }

                }
                catch (Exception ex)
                {
                    result.Code = -1;
                    result.Description = ex.Message;
                }
            }
            else
            {
                result.Code = 3;
            }

            return result.Code.Value;
        }

        private static List<string> SplitSetClauseSafely(string input)
        {
            var parts = new List<string>();
            int parentheses = 0;
            int quotes = 0;
            int start = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\'' && (i == 0 || input[i - 1] != '\\'))
                    quotes ^= 1; // Alterna entre dentro/fuera de comillas

                if (quotes == 0)
                {
                    if (c == '(') parentheses++;
                    else if (c == ')') parentheses--;
                    else if (c == ',' && parentheses == 0)
                    {
                        parts.Add(input.Substring(start, i - start).Trim());
                        start = i + 1;
                    }
                }
            }

            if (start < input.Length)
                parts.Add(input.Substring(start).Trim());

            return parts;
        }

        private static bool SonIguales(object valorOriginal, object valorNuevo)
        {
            if (valorOriginal == null && valorNuevo == null) return true;
            if (valorOriginal == null || valorNuevo == null) return false;

            // Convertir booleanos a 0/1
            valorOriginal = ConvertirBoolANumero(valorOriginal);
            valorNuevo = ConvertirBoolANumero(valorNuevo);

            // Si el original es fecha, intentar interpretar el nuevo como fecha
            if (valorOriginal is DateTime dtOriginal)
            {
                if (TryConvertToDateTime(valorNuevo, out DateTime dtNuevo))
                    return dtOriginal.Date == dtNuevo.Date; // comparar solo fecha (sin hora)
                return false;
            }

            // Comparar como números
            if (decimal.TryParse(valorOriginal.ToString(), out var num1) &&
                decimal.TryParse(valorNuevo.ToString(), out var num2))
                return num1 == num2;

            // Comparar como string ignorando espacios y mayúsculas
            return valorOriginal.ToString().Trim().Equals(
                   valorNuevo.ToString().Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static object ConvertirBoolANumero(object valor)
        {
            if (valor is bool b)
                return b ? 1 : 0;

            if (bool.TryParse(valor?.ToString(), out var parsedBool))
                return parsedBool ? 1 : 0;

            return valor;
        }

        private static bool TryConvertToDateTime(object valor, out DateTime fecha)
        {
            fecha = default;

            if (valor is DateTime dt)
            {
                fecha = dt;
                return true;
            }

            var str = valor?.ToString()?.Trim();
            if (string.IsNullOrEmpty(str)) return false;

            // Formatos conocidos (puedes agregar más si ocupás)
            string[] formatos = { "yyyyMMdd", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

            return DateTime.TryParseExact(str, formatos, CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out fecha);
        }

        /// <summary>
        /// Inserta un registro en la tabla de control de cambios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoCambio"></param>
        /// <param name="tabla"></param>
        /// <param name="llave"></param>
        /// <param name="usuario"></param>
        /// <param name="jsonOrg"></param>
        /// <param name="jsonNew"></param>
        /// <returns></returns>
        private ErrorDTO InsertarTablaControl(
            int CodEmpresa, int tipoCambio,  
            string tabla, string llave, 
            string usuario, string vQuery,
            string vInsert, DataTable dtDiferencias)
        {
            ErrorDTO result = new ErrorDTO();
            result.Code = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Convierto llave a Json
                    var jsonLlave = JsonConvert.SerializeObject(llave);
                    string jsonOriginal = string.Empty;
                    string jsonNew = string.Empty;
                    if (dtDiferencias != null)
                    {
                        jsonOriginal = JsonConvert.SerializeObject(dtDiferencias, Formatting.Indented);
                        //Obtengo solo valores neuvos.
                        dtDiferencias.Columns.Remove("ValorOriginal");
                        dtDiferencias.AcceptChanges();
                        DataTable dtEjecuta = dtDiferencias;
                        jsonNew = JsonConvert.SerializeObject(dtEjecuta, Formatting.Indented);
                    }
                    else
                    {
                        jsonOriginal = JsonConvert.SerializeObject(vInsert, Formatting.Indented);
                    }
                   

                    var query = $@"INSERT INTO FND_CONTROL_CAMBIOS_APROB (
                                        COD_TIPO_CAMBIO,
                                        NOM_TABLA,
                                        LLAVES,
                                        COD_EVENTO,
                                        USUARIO_CAMBIO, 
                                        VALORESJSONACT, 
                                        VALORESJSONDIF, 
                                        COD_ESTADO , 
                                        FECHA_CAMBIO)
                                  VALUES (
                                        {tipoCambio}, 
                                        '{tabla}',
                                        '{jsonLlave.Replace("'", "''")}', 
                                        '{vQuery}', 
                                        '{usuario}', 
                                        '{jsonOriginal.Replace("'", "''")}', 
                                        '{jsonNew.Replace("'", "''")}', 
                                        'P' ,GETDATE())";
                    connection.Execute(query);
                    result.Code = 1;
                    result.Description = "ok";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el update en la tabla de control de cambios y actualiza el estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idCambio"></param>
        /// <returns></returns>
        public int FndControlCambios_Autoriza(int CodEmpresa, int idCambio, string usuario)
        {
            ErrorDTO result = new ErrorDTO();
            result.Code = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //busco el registro en la tabla de control
                    var query = $@"SELECT * FROM FND_CONTROL_CAMBIOS_APROB WHERE ID_CAMBIO = {idCambio}";
                    var dtCambio = connection.Query<FndControlCambioAprobDTO>(query).FirstOrDefault();
                    if (dtCambio == null)
                    {
                        result.Code = -1;
                        result.Description = "No se encontró el registro en la tabla de control.";
                        return result.Code.Value;
                    }

                    switch (dtCambio.cod_evento)
                    {
                        case "UPDATE":
                            // Parsear el JSON
                            var cambios = JsonConvert.DeserializeObject<List<CampoCambio>>(dtCambio.valoresjsondif);

                            //Armo el query de update 
                            var setParts = cambios.Select(c => $"{c.Campo} = {FormatearValorSql(c.ValorNuevo)}");

                            query = $"UPDATE {dtCambio.nom_tabla} SET {string.Join(", ", setParts)} WHERE {dtCambio.llaves.Trim('"')};";
                            result.Code = connection.Execute(query);

                            break;
                        case "INSERT":
                            query = dtCambio.valoresjsonact.Trim('"'); 
                            result.Code = connection.Execute(query);
                            break;
                        case "DELETE":
                            query = $"DELETE {dtCambio.nom_tabla} WHERE {dtCambio.llaves.Trim('"')};";
                            result.Code = connection.Execute(query);
                            break;
                        default:
                            result.Code = -1;
                            result.Description = "Tipo de evento no soportado.";
                            return result.Code.Value;
                    }

                    if (result.Code != -1)
                    {
                        //Actualizo el estado de la tabla de control
                        query = $@"UPDATE FND_CONTROL_CAMBIOS_APROB SET COD_ESTADO = 'V', USUARIO_APRUEBA = '{usuario}' , FECHA_APRUEBA = getDate()
                                    WHERE ID_CAMBIO = {idCambio}";
                        connection.Execute(query);
                        result.Description = "ok";
                    }
                    else
                    {
                        result.Code = -1;
                        result.Description = "Error al actualizar la tabla de control.";
                    }

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result.Code.Value;
        }

        private static string FormatearValorSql(object valor)
        {
            if (valor == null || valor is JValue jVal && jVal.Type == JTokenType.Null)
                return "NULL";

            if (valor is JValue jv)
            {
                switch (jv.Type)
                {
                    case JTokenType.Boolean:
                        return (bool)jv ? "1" : "0";
                    case JTokenType.String:
                        return $"'{jv.ToString().Replace("'", "''")}'";
                    case JTokenType.Integer:
                    case JTokenType.Float:
                        return jv.ToString();
                    default:
                        return $"'{jv.ToString().Replace("'", "''")}'";
                }
            }

            if (valor is bool b)
                return b ? "1" : "0";
            if (valor is string s)
                return $"'{s.Replace("'", "''")}'";

            return valor.ToString();
        }

        public int FndControlAutoriza_Eliminar(FndControlAutorizaData request)
        {
            ErrorDTO result = new ErrorDTO();
            result.Code = 0;


            if (controlAuth == "Y")
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);
                /*
                 COD_TIPO_CAMBIO:
                 1 = Insertar
                 2 = Modificar
                 3 = Eliminar
                 */

                try
                {
                    //Obtengo el where de la consulta
                    string patternDelete = @"DELETE\s+(FROM\s+)?(?<table>\w+)\s+WHERE\s+(?<whereClause>.+)$";
                    var matchDelete = Regex.Match(request.strSQL, patternDelete, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (!matchDelete.Success)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o no se puede analizar.";
                        return result.Code.Value;
                    }

                    var table = matchDelete.Groups["table"].Value;
                    var whereClause = matchDelete.Groups["whereClause"].Value;


                    result = InsertarTablaControl(
                          request.CodEmpresa,
                          request.tipoCambio,
                          table,
                          whereClause,
                          request.usuario,
                          "DELETE",
                          null,
                          null);
                }
                catch (Exception ex)
                {
                    result.Code = -1;
                    result.Description = ex.Message;
                }
            }
            else
            {
                result.Code = 3;
            }

            return result.Code.Value;
        }

        public int FndControlAutoriza_Insertar(FndControlAutorizaData request)
        {
            ErrorDTO result = new ErrorDTO();
            result.Code = 0;


            if (controlAuth == "Y")
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);
                /*
                 COD_TIPO_CAMBIO:
                 1 = Insertar
                 2 = Modificar
                 3 = Eliminar
                 */

                try
                {
                    //Obtengo el where de la consulta
                    string patternInsert = @"insert\s+(?:into\s+)?(?<table>\w+)\s*\((?<columns>[^)]+)\)\s*values\s*\((?<values>.+?)\)";
                    var matchInsert = Regex.Match(request.strSQL, patternInsert, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (!matchInsert.Success)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o no se puede analizar.";
                        return result.Code.Value;
                    }

                    var table = matchInsert.Groups["table"].Value;
                    var columns = matchInsert.Groups["columns"].Value;
                    var values = matchInsert.Groups["values"].Value;


                    result = InsertarTablaControl(
                          request.CodEmpresa,
                          request.tipoCambio,
                          table,
                          null,
                          request.usuario,
                          "INSERT",
                          request.strSQL,
                          null);
                }
                catch (Exception ex)
                {
                    result.Code = -1;
                    result.Description = ex.Message;
                }
            }
            else
            {
                result.Code = 3;
            }

            return result.Code.Value;
        }
        #endregion
    }
}
