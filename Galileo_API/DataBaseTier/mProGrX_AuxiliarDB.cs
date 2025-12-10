using Dapper;
using Humanizer;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Galileo.DataBaseTier
{
    public class MProGrXAuxiliarDB
    {
        private readonly IConfiguration _config;

        public string dateFormat { get; set; }
        public string controlAuth { get; set; }

        private const string DescripcionColumn = "@descripcion";
        private static readonly Regex CorreoRegex = new Regex(
               @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
               RegexOptions.CultureInvariant | RegexOptions.Compiled,
               matchTimeout: TimeSpan.FromMilliseconds(200)
           );

        private static readonly Regex UpdateSqlRegex = new Regex(
            @"UPDATE\s+(?<table>\w+)\s+SET\s+(?<setClause>.+?)\s+WHERE\s+(?<whereClause>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant | RegexOptions.Compiled,
            matchTimeout: TimeSpan.FromMilliseconds(200)
        );

        private static readonly Regex DeleteSqlRegex = new Regex(
            @"DELETE\s+(FROM\s+)?(?<table>\w+)\s+WHERE\s+(?<whereClause>.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant | RegexOptions.Compiled,
            matchTimeout: TimeSpan.FromMilliseconds(200)
        );

        // Regex precompilada + timeout para evitar ReDoS (S6444)
        private static readonly Regex InsertSqlRegex = new Regex(
            @"insert\s+(?:into\s+)?(?<table>\w+)\s*\((?<columns>[^)]+)\)\s*values\s*\((?<values>.+?)\)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline |
            RegexOptions.CultureInvariant | RegexOptions.Compiled,
            matchTimeout: TimeSpan.FromMilliseconds(200)
        );

        public MProGrXAuxiliarDB(IConfiguration config)
        {
            _config = config;
            var dateFormatValue = _config.GetSection("AppSettings").GetSection("DateTimeFormat").Value;
            dateFormat = dateFormatValue != null ? dateFormatValue.ToString() : string.Empty;
            var controlAuthValue = _config.GetSection("AppSettings").GetSection("ControlAutorizacion").Value;
            controlAuth = controlAuthValue != null ? controlAuthValue.ToString() : string.Empty;
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

                var query = $@"SELECT ISNULL(COUNT(*),0) AS Existe FROM pv_periodos WHERE mes > MONTH(@vfecha)  AND anio = YEAR(@vfecha) AND estado = 'C'";
                var resp = connection.Query<int>(query, new { vfecha }).FirstOrDefault();
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
                    query = $@"Select estado from pv_periodos where anio = YEAR(@vfecha) AND mes = MONTH(@vfecha) ";
                    var estado = connection.Query<string>(query, new { vfecha }).FirstOrDefault();
                    if (estado == "C")
                    {
                        vPasa = false;
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
        public ErrorDto sbInvInventario(int CodEmpresa, CompraInventarioDto req)
        {
            ErrorDto result = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

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
        public ErrorDto fxInvVerificaLineaDetalle(int CodEmpresa, int ColCantidad, string vMov, int ColProd, int ColBod1, int ColBod2, List<FacturaDetalleDto> vGrid)
        {
            ErrorDto result = new ErrorDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            result.Code = 1;

            if (ColProd > 0 && vGrid.Count > 0)
            {
                int count = 0;
                foreach (FacturaDetalleDto item in vGrid)
                {
                    count++;
                    if (item.cantidad > 0)
                    {
                        VerificaProducto(item, stringConn, ref result, count);
                        if (ColBod1 > 0)
                        {
                            VerificaBodega(item, stringConn, vMov, ref result, count, true);
                        }
                        if (ColBod2 > 0)
                        {
                            VerificaBodega(item, stringConn, vMov, ref result, count, false);
                        }
                    }
                }
            }
            return result;
        }

        private static void VerificaProducto(FacturaDetalleDto item, string stringConn, ref ErrorDto result, int count)
        {
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = @"
                        SELECT estado
                        FROM pv_productos
                        WHERE cod_producto = @CodProducto;
                    ";

                List<ProductoDto> exist = connection.Query<ProductoDto>(
                    query,
                    new { CodProducto = item.cod_producto }
                ).ToList();

                if (exist.Count == 0)
                {
                    result.Code = 0;
                    result.Description += $"\nL {count} - Producto : {item.cod_producto} - No Existe";
                }
                else if (exist[0].Estado == "I")
                {
                    result.Code = 0;
                    result.Description += $"\nL {count} - Producto : {item.cod_producto} - Se encuentra Inactivo";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
        }

        private static void VerificaBodega(FacturaDetalleDto item, string stringConn, string vMov, ref ErrorDto result, int count, bool isEntrada)
        {
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = @cod_bodega ";
                var bodega = connection.Query<Models.BodegaDto>(query, new { cod_bodega = item.cod_bodega }).FirstOrDefault();

                if (bodega == null)
                {
                    result.Code = 0;
                    result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - No Existe";
                    return;
                }
                if (bodega.estado == "I")
                {
                    result.Code = 0;
                    result.Description += $"\r\nL {count} - Bodega : {item.cod_bodega} - Se encuentra Inactiva";
                    return;
                }

                if (isEntrada)
                {
                    VerificaPermisosEntradaSalida(bodega, vMov, ref result, count, item.cod_bodega, true);
                }
                else
                {
                    VerificaPermisosEntradaSalida(bodega, vMov, ref result, count, item.cod_bodega, false);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
        }

        private static void VerificaPermisosEntradaSalida(Models.BodegaDto bodega, string vMov, ref ErrorDto result, int count, string cod_bodega, bool isEntrada)
        {
            if (isEntrada)
            {
                if (vMov == "E" && bodega.permite_entradas == "0")
                {
                    result.Code = 0;
                    result.Description += $"\r\nL {count} - Bodega : {cod_bodega} - No Permite Entradas";
                }
                else if ((vMov == "S" || vMov == "R" || vMov == "T") && bodega.permite_salidas == "0")
                {
                    result.Code = 0;
                    result.Description += $"\r\nL {count} - Bodega : {cod_bodega} - No Permite Salidas";
                }
            }
            else
            {
                if ((vMov == "E" || vMov == "T") && bodega.permite_entradas == "0")
                {
                    result.Code = 0;
                    result.Description += $"\r\nL {count} - Bodega : {cod_bodega} - No Permite Entradas";
                }
                else if ((vMov == "R" || vMov == "S") && bodega.permite_salidas == "0")
                {
                    result.Code = 0;
                    result.Description += $"\r\nL {count} - Bodega : {cod_bodega} - No Permite Salidas";
                }
            }
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
            string? vNum = "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            //Verificar si existen posteriores Cerrados
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"select estado from pv_periodos where anio = YEAR(@vfecha)  and mes = MONTH(@vfecha) ";
                vNum = connection.Query<string>(query , new { vfecha }).FirstOrDefault();
                if (vNum == "C")
                {
                    vPasa = false;
                }
                else
                {
                    vPasa = true;
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
        public ErrorDto<ParametroValor> fxCxPParametro(int CodEmpresa, string Cod_Parametro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<ParametroValor>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                var query = @"
                    SELECT cod_parametro AS Cod_Parametro,
                           valor        AS Valor
                    FROM cxp_parametros
                    WHERE cod_parametro = @CodParametro;
                ";

                response.Result = connection.QueryFirstOrDefault<ParametroValor>(
                    query,
                    new { CodParametro = Cod_Parametro }
                );

                if (response.Result == null || response.Result.Valor == null)
                {
                    response.Result = new ParametroValor
                    {
                        Cod_Parametro = Cod_Parametro,
                        Valor = "GEN"
                    };
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
        public ErrorDto fxInvTransaccionesAutoriza(int CodEmpresa, string Boleta, string TipoTran, string AutorizaUser)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto info = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var query1 = @"select genera_user from pv_invTransac where Tipo = @TipoTran and Boleta = @Boleta";
                var generaUser = connection.ExecuteScalar<string>(query1, new { TipoTran, Boleta });

                if (string.IsNullOrEmpty(generaUser))
                {
                    info.Code = 0;
                    info.Description = $"No se encontró el usuario que generó la boleta '{Boleta}', verifique que la boleta exista";
                    return info;
                }

                var query2 = @"select isnull(count(*),0) as Existe from pv_orden_autousers where Usuario = @AutorizaUser and Usuario_Asignado = @GUser and ENTRADAS = 1";
                int valideAutorizacion = connection.ExecuteScalar<int>(query2, new { AutorizaUser, GUser = generaUser });

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

            string tableName = vTabla.ToUpper();
            string strSQL = GetSqlForTable(tableName, vTipoDC, vCodDesX, Cod_Conta);

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                info = connection.Query<ConsultaDescripcion>(strSQL).FirstOrDefault() ?? new ConsultaDescripcion();

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        private static string GetSqlForTable(string tableName, string vTipoDC, string vCodDesX, int Cod_Conta)
        {
            string codeFilter = vCodDesX.ToString();
            string descFilter = vCodDesX.ToString();

            switch (tableName)
            {
                case "PROVEEDORES":
                    return BuildSql(new BuildSqlParams { CodeColumn = "Cod_proveedor", DescColumn = DescripcionColumn, Table = "cxp_proveedores", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = false, CodConta = null });
                case "PRODUCTOS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_Producto", DescColumn = DescripcionColumn, Table = "pv_Productos", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "CARGOSPROV":
                    return BuildSql(new BuildSqlParams { CodeColumn = "Cod_cargo", DescColumn = DescripcionColumn, Table = "cxp_cargos", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "UNIDADES":
                    return BuildSql(new BuildSqlParams { CodeColumn = "Cod_Unidad", DescColumn = DescripcionColumn, Table = "pv_unidades", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "MARCAS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "Cod_Marca", DescColumn = DescripcionColumn, Table = "pv_marcas", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "LINEAPRODUCTO":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_prodclas", DescColumn = DescripcionColumn, Table = "pv_prod_clasifica", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = false, CodConta = null });
                case "BANCOS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "id_banco", DescColumn = DescripcionColumn, Table = "Tes_Bancos", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = false, CodConta = null });
                case "CLIENTES":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cedula", DescColumn = "nombre", Table = "pv_clientes", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "BODEGAS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_bodega", DescColumn = DescripcionColumn, Table = "pv_bodegas", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "PRECIOS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_precio", DescColumn = DescripcionColumn, Table = "pv_tipos_precios", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "AGENTES":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_agente", DescColumn = "Nombre", Table = "pv_agentes", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "CAJAS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_caja", DescColumn = "Nombre", Table = "pv_cajas", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = null });
                case "CUENTAS":
                    return BuildSql(new BuildSqlParams { CodeColumn = "cod_Cuenta", DescColumn = DescripcionColumn, Table = "CntX_cuentas", VTipoDC = vTipoDC, CodeFilter = codeFilter, DescFilter = descFilter, QuoteCode = true, CodConta = Cod_Conta });
                default:
                    return string.Empty;
            }
        }

        private sealed class BuildSqlParams
        {
            public string? CodeColumn { get; set; }
            public string? DescColumn { get; set; }
            public string? Table { get; set; }
            public string? VTipoDC { get; set; }
            public string? CodeFilter { get; set; }
            public string? DescFilter { get; set; }
            public bool QuoteCode { get; set; }
            public int? CodConta { get; set; }
        }

        private static string BuildSql(BuildSqlParams p)
        {
            string sql = $"select {p.CodeColumn} as CodX, {p.DescColumn} as DescX from {p.Table}";
            string where = "";

            if (p.VTipoDC == "D")
            {
                where = p.QuoteCode ? $" where {p.CodeColumn} = '{p.CodeFilter}'" : $" where {p.CodeColumn} = {p.CodeFilter}";
            }
            else
            {
                where = $" where {p.DescColumn} = '{p.DescFilter}'";
            }

            sql += where;

            if (p.CodConta.HasValue)
            {
                sql += $" and cod_contabilidad = {p.CodConta.Value}";
            }

            return sql;
        }

        public static bool fxCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                return CorreoRegex.IsMatch(correo);
            }
            catch (RegexMatchTimeoutException)
            {
                // Si excede el timeout, lo tratamos como inválido por seguridad
                return false;
            }
        }

        /// <summary>
        /// Convierte un modelo a XML para enviarlo a SP de BD
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string fxConvertModelToXml<T>(T model)
        {
            if (EqualityComparer<T>.Default.Equals(model, default(T))) throw new ArgumentNullException(nameof(model));

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
        /// Consulta la cantidad de activos sin asignar a un usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<int> ActivosSinAsignar_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<int> result = new ErrorDto<int>();
            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    var query = $@"select COUNT(*)  FROM PV_CONTROL_ACTIVOS
                                    WHERE ENTREGA_USUARIO = ''
                                    AND ESTADO IN ('P', 'R') AND REGISTRO_USUARIO = @usuario";
                    result.Result = connection.QueryFirstOrDefault<int>(query, new { usuario });
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
        public string? validaFechaGlobal(DateTime? fecha)
        {
            string? fechaValdiada = "";
            try
            {
                if (fecha != null)
                {
                    DateTime fechaActual = (DateTime)fecha;
                    fechaValdiada = fechaActual.ToString(dateFormat);
                }
                else
                {
                    fechaValdiada = null;
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
        public ErrorDto BitacoraProducto(BitacoraProductoInsertarDto req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);


                var strSQL = @"
                        INSERT INTO [dbo].[BITACORA_PRODUCTOS]
                            ([COD_PRODUCTO],
                             [CONSEC],
                             [MOVIMIENTO],
                             [DETALLE],
                             [REGISTRO_FECHA],
                             [REGISTRO_USUARIO])
                        VALUES
                            (@CodProducto,
                             @Consec,
                             @Movimiento,
                             @Detalle,
                             GETDATE(),
                             @RegistroUsuario);
                    ";

                resp.Code = connection.Execute(strSQL, new
                {
                    CodProducto = req.cod_producto,
                    Consec = req.consec,
                    Movimiento = req.movimiento,
                    Detalle = req.detalle,
                    RegistroUsuario = req.registro_usuario
                });
                resp.Description = "Ok";

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
        public ErrorDto BitacoraProveedor(BitacoraProveedorInsertarDto req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);



                var strSQL = @"
                        INSERT INTO [dbo].[BITACORA_PROVEEDOR]
                            ([COD_PROVEEDOR],
                             [CONSEC],
                             [MOVIMIENTO],
                             [DETALLE],
                             [REGISTRO_FECHA],
                             [REGISTRO_USUARIO])
                        VALUES
                            (@CodProveedor,
                             @Consec,
                             @Movimiento,
                             @Detalle,
                             GETDATE(),
                             @RegistroUsuario);
                    ";

                resp.Code = connection.Execute(strSQL, new
                {
                    CodProveedor = req.cod_proveedor,
                    Consec = req.consec,
                    Movimiento = req.movimiento,
                    Detalle = req.detalle,
                    RegistroUsuario = req.registro_usuario
                });
                resp.Description = "Ok";

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>();
            resp.Code = 0;
            resp.Result = new List<DropDownListaGenericaModel>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = "select CODIGO_SINPE as item, rtrim(Descripcion) as descripcion from AFI_TIPOS_IDS " +
                    " order by Tipo_Id ";
                resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        public static ErrorDto<string> NumeroALetras(decimal numero)
        {
            var resp = new ErrorDto<string>();
            resp.Code = 0;
            long parteEntera = (long)Math.Floor(numero);
            int parteDecimal = (int)((numero - parteEntera) * 100);

            string letrasEntera = parteEntera.ToWords(new System.Globalization.CultureInfo("es"));
            if (letrasEntera.Equals("uno", StringComparison.CurrentCultureIgnoreCase))
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
        public static byte[] CombinarBytesPdfSharp(params byte[][] pdfs)
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
            if (controlAuth != "Y")
                return 3;

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(request.CodEmpresa);
            ErrorDto result = new ErrorDto { Code = 0 };

            try
            {
                var match = ParseUpdateSql(request.strSQL);
                if (match == null)
                    return SetErrorResult(result, "La sentencia SQL no es válida o no se puede analizar.");

                string table = match.Groups["table"].Value.Trim();
                string setClause = match.Groups["setClause"].Value.Trim();
                string whereClause = match.Groups["whereClause"].Value.Trim();

                using var connection = new SqlConnection(stringConn);
                var dtTable = connection.Query($"SELECT * FROM {table} WHERE {whereClause}").FirstOrDefault();
                if (dtTable == null)
                    return SetErrorResult(result, $"❌ No se encontró información en {table} con la condición: {whereClause}");

                var diferencias = ObtenerDiferencias(connection, table, setClause, whereClause, dtTable);
                if (diferencias == null)
                    return SetErrorResult(result, "No se pudo obtener los valores nuevos para comparar.");

                var dtDiferencias = CrearDataTableDiferencias(diferencias);
                if (dtDiferencias.Rows.Count == 0)
                    return SetErrorResult(result, "No se encontraron diferencias entre los valores originales y los nuevos.", 2);

                // Crear el contexto general
                var ctx = new ControlCambioContext(
                    CodEmpresa: request.CodEmpresa,
                    Usuario: request.usuario
                );

                // Crear el payload con la información específica del cambio
                var payload = new ControlCambioPayload(
                    TipoCambio: request.tipoCambio,
                    Tabla: table,
                    Llave: whereClause,      // la llave o condición
                    EventoQuery: "UPDATE",   // tipo de operación
                    InsertSql: "",           // en este caso no hay un INSERT literal
                    Diferencias: dtDiferencias
                );

                // Llamada al método refactorizado
                result = InsertarTablaControl(ctx, payload);

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result.Code.HasValue ? result.Code.Value : -1;
        }

        private static Match? ParseUpdateSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return null;

            try
            {
                var match = UpdateSqlRegex.Match(sql);
                return match.Success ? match : null;
            }
            catch (RegexMatchTimeoutException)
            {
                // Si la regex tarda demasiado, lo tratamos como no parseable
                return null;
            }
        }

        private static int SetErrorResult(ErrorDto result, string description, int code = -1)
        {
            result.Code = code;
            result.Description = description;
            return result.Code ?? -1;
        }

        private static List<(string Campo, object ValorOriginal, object ValorNuevo)> ObtenerDiferencias(SqlConnection connection, string table, string setClause, string whereClause, object dtTable)
        {
            var selectParts = new List<string>();
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

            var dicOriginal = ((IDictionary<string, object>)dtTable);
            IDictionary<string, object>? dicNuevo = dtTableNew as IDictionary<string, object>;
            if (dicNuevo == null)
                return new List<(string Campo, object ValorOriginal, object ValorNuevo)>();

            var diferencias = new List<(string Campo, object ValorOriginal, object ValorNuevo)>();
            foreach (var kvp in dicOriginal)
            {
                var key = kvp.Key;
                var valorOriginal = kvp.Value;
                if (dicNuevo.TryGetValue(key, out var valorNuevo) && !SonIguales(valorOriginal, valorNuevo))
                {
                    diferencias.Add((key, valorOriginal, valorNuevo));
                }
            }
            return diferencias;
        }

        private static DataTable CrearDataTableDiferencias(List<(string Campo, object ValorOriginal, object ValorNuevo)> diferencias)
        {
            var dtDiferencias = new DataTable();
            dtDiferencias.Columns.Add("Campo", typeof(string));
            dtDiferencias.Columns.Add("ValorOriginal", typeof(object));
            dtDiferencias.Columns.Add("ValorNuevo", typeof(object));

            foreach (var dif in diferencias)
            {
                dtDiferencias.Rows.Add(dif.Campo, dif.ValorOriginal, dif.ValorNuevo);
            }
            return dtDiferencias;
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
            var strOriginal = valorOriginal.ToString()?.Trim() ?? string.Empty;
            var strNuevo = valorNuevo?.ToString()?.Trim() ?? string.Empty;
            return strOriginal.Equals(strNuevo, StringComparison.OrdinalIgnoreCase);
        }

        private static object ConvertirBoolANumero(object valor)
        {
            if (valor is bool b)
                return b ? 1 : 0;

            if (bool.TryParse(valor?.ToString(), out var parsedBool))
                return parsedBool ? 1 : 0;

            return valor ?? string.Empty;
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


        public record ControlCambioContext(
            int CodEmpresa,
            string Usuario
        );

        public record ControlCambioPayload(
            int TipoCambio,
            string Tabla,
            object Llave,              // mejor que string si luego lo serializas a JSON
            string EventoQuery,
            string? InsertSql,         // nullable: solo aplica cuando no hay diferencias
            DataTable? Diferencias     // nullable: si viene null usamos InsertSql
        );

        private ErrorDto InsertarTablaControl(ControlCambioContext ctx, ControlCambioPayload payload)
        {
            var result = new ErrorDto { Code = 0 };
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(ctx.CodEmpresa);

            try
            {
                using var connection = new SqlConnection(connStr);

                // --- Preparación de JSON ---
                var jsonLlave = JsonConvert.SerializeObject(payload.Llave);
                string valoresJsonAct;
                string? valoresJsonDif = null;

                if (payload.Diferencias != null)
                {
                    // snapshot original
                    var original = payload.Diferencias.Copy();
                    valoresJsonAct = JsonConvert.SerializeObject(original, Formatting.Indented);

                    // diferencias sin "ValorOriginal"
                    var dif = payload.Diferencias.Copy();
                    if (dif.Columns.Contains("ValorOriginal"))
                    {
                        dif.Columns.Remove("ValorOriginal");
                        dif.AcceptChanges();
                    }
                    valoresJsonDif = JsonConvert.SerializeObject(dif, Formatting.Indented);
                }
                else
                {
                    valoresJsonAct = JsonConvert.SerializeObject(payload.InsertSql, Formatting.Indented);
                }

                // --- SQL parametrizado (Dapper) ---
                const string sql = @"
INSERT INTO FND_CONTROL_CAMBIOS_APROB (
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
    @TipoCambio, 
    @Tabla,
    @Llaves, 
    @Evento, 
    @Usuario, 
    @ValoresJsonAct, 
    @ValoresJsonDif, 
    'P' ,
    GETDATE()
);";

                connection.Execute(sql, new
                {
                    TipoCambio = payload.TipoCambio,
                    Tabla = payload.Tabla,
                    Llaves = jsonLlave,
                    Evento = payload.EventoQuery,
                    Usuario = ctx.Usuario,
                    ValoresJsonAct = valoresJsonAct,
                    ValoresJsonDif = (object?)valoresJsonDif ?? DBNull.Value
                });

                result.Code = 1;
                result.Description = "ok";
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
            ErrorDto result = new ErrorDto();
            result.Code = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                // busco el registro en la tabla de control
                var query = @"
                    SELECT *
                    FROM FND_CONTROL_CAMBIOS_APROB
                    WHERE ID_CAMBIO = @IdCambio;
                ";

                var dtCambio = connection.QueryFirstOrDefault<FndControlCambioAprobDto>(
                    query,
                    new { IdCambio = idCambio }
                );
                if (dtCambio == null)
                {
                    result.Code = -1;
                    result.Description = "No se encontró el registro en la tabla de control.";
                    return result.Code ?? -1;
                }

                switch (dtCambio.cod_evento)
                {
                    case "UPDATE":
                        // Parsear el JSON
                        var cambios = JsonConvert.DeserializeObject<List<CampoCambio>>(dtCambio.valoresjsondif ?? string.Empty);

                        //Armo el query de update 
                        var setParts = (cambios ?? new List<CampoCambio>()).Select(c => $"{c.Campo} = {FormatearValorSql(c.ValorNuevo ?? string.Empty)}");

                        var llaves = dtCambio.llaves != null ? dtCambio.llaves.Trim('"') : string.Empty;
                        query = $"UPDATE {dtCambio.nom_tabla} SET {string.Join(", ", setParts)} WHERE {llaves};";
                        result.Code = connection.Execute(query);

                        break;
                    case "INSERT":
                        query = dtCambio.valoresjsonact != null ? dtCambio.valoresjsonact.Trim('"') : string.Empty;
                        result.Code = connection.Execute(query);
                        break;
                    case "DELETE":
                        var llavesDelete = dtCambio.llaves != null ? dtCambio.llaves.Trim('"') : string.Empty;
                        query = $"DELETE {dtCambio.nom_tabla} WHERE {llavesDelete};";
                        result.Code = connection.Execute(query);
                        break;
                    default:
                        result.Code = -1;
                        result.Description = "Tipo de evento no soportado.";
                        return result.Code ?? -1;
                }

                if (result.Code != -1)
                {
                    //Actualizo el estado de la tabla de control
                    query = $@"UPDATE FND_CONTROL_CAMBIOS_APROB SET COD_ESTADO = 'V', USUARIO_APRUEBA = @usuario , FECHA_APRUEBA = getDate()
                                    WHERE ID_CAMBIO = @idCambio";
                    connection.Execute(query, new { idCambio = idCambio, usuario = usuario });
                    result.Description = "ok";
                }
                else
                {
                    result.Code = -1;
                    result.Description = "Error al actualizar la tabla de control.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result.Code ?? -1;
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

            return valor.ToString() ?? string.Empty;
        }

        public int FndControlAutoriza_Eliminar(FndControlAutorizaData request)
        {
            ErrorDto result = new ErrorDto();
            result.Code = 0;


            if (controlAuth == "Y")
            {


                try
                {
                    Match matchDelete;
                    try
                    {
                        matchDelete = DeleteSqlRegex.Match(request.strSQL ?? string.Empty);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o excede el tiempo de análisis.";
                        return result.Code ?? -1;
                    }

                    if (!matchDelete.Success)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o no se puede analizar.";
                        return result.Code ?? -1;
                    }

                    var table = matchDelete.Groups["table"].Value;
                    var whereClause = matchDelete.Groups["whereClause"].Value;

                    // Crear el contexto (empresa + usuario)
                    var ctx = new ControlCambioContext(
                        CodEmpresa: request.CodEmpresa,
                        Usuario: request.usuario
                    );

                    // Crear el payload (datos del cambio)
                    var payload = new ControlCambioPayload(
                        TipoCambio: request.tipoCambio,
                        Tabla: table,
                        Llave: whereClause,     // condición o llave del registro
                        EventoQuery: "DELETE",  // tipo de operación
                        InsertSql: "",          // sin SQL de inserción
                        Diferencias: new DataTable() // tabla vacía
                    );

                    // Ejecutar
                    result = InsertarTablaControl(ctx, payload);

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

            return result.Code ?? -1;
        }

        public int FndControlAutoriza_Insertar(FndControlAutorizaData request)
        {
            ErrorDto result = new ErrorDto();
            result.Code = 0;


            if (controlAuth == "Y")
            {

                try
                {
                    //Obtengo el where de la consulta
                    Match matchInsert;
                    try
                    {
                        matchInsert = InsertSqlRegex.Match(request.strSQL ?? string.Empty);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o excede el tiempo de análisis.";
                        return result.Code ?? -1;
                    }

                    if (!matchInsert.Success)
                    {
                        result.Code = -1;
                        result.Description = "La sentencia SQL no es válida o no se puede analizar.";
                        return result.Code ?? -1;
                    }

                    var table = matchInsert.Groups["table"].Value;


                    // Crear el contexto
                    var ctx = new ControlCambioContext(
                        CodEmpresa: request.CodEmpresa,
                        Usuario: request.usuario
                    );

                    // Crear el payload
                    var payload = new ControlCambioPayload(
                        TipoCambio: request.tipoCambio,
                        Tabla: table,
                        Llave: "",               // no hay llave previa (nuevo registro)
                        EventoQuery: "INSERT",   // tipo de operación
                        InsertSql: request.strSQL, // los valores o SQL que se insertan
                        Diferencias: null        // sin diferencias, ya que es un alta
                    );

                    // Llamada al método
                    result = InsertarTablaControl(ctx, payload);

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

            return result.Code ?? -1;
        }
        #endregion
    }
}
