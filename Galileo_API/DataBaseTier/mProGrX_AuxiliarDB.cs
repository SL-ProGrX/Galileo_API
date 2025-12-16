using System.Data;
using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Dapper;
using Humanizer;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class MProGrXAuxiliarDB
    {
        private readonly PortalDB _portalDB;

        public string dateFormat { get; set; }
        public string controlAuth { get; set; }

        private const string _descripcion = "descripcion";

        // Identificadores SQL (tabla/columna) permitidos: letras, números, _
        private static readonly Regex IdentRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public MProGrXAuxiliarDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);

            var dateFormatValue = config.GetSection("AppSettings").GetSection("DateTimeFormat").Value;
            dateFormat = dateFormatValue ?? string.Empty;

            var controlAuthValue = config.GetSection("AppSettings").GetSection("ControlAutorizacion").Value;
            controlAuth = controlAuthValue ?? string.Empty;
        }

        #region Periodos / Inventario básicos

        public bool fxInvPeriodos(int CodEmpresa, string vfecha)
        {
            bool vPasa = false;

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlExiste = @"
                    SELECT ISNULL(COUNT(*),0) AS Existe 
                    FROM pv_periodos 
                    WHERE mes > MONTH(@Fecha) 
                      AND anio = YEAR(@Fecha) 
                      AND estado = 'C';";

                var resp = connection.QueryFirstOrDefault<int>(sqlExiste, new { Fecha = vfecha });

                vPasa = resp == 0;

                if (vPasa)
                {
                    const string sqlEstado = @"
                        SELECT estado 
                        FROM pv_periodos 
                        WHERE anio = YEAR(@Fecha) 
                          AND mes  = MONTH(@Fecha);";

                    var estado = connection.QueryFirstOrDefault<string>(sqlEstado, new { Fecha = vfecha });
                    if (estado == "C")
                    {
                        vPasa = false;
                    }
                }
            }
            catch
            {
                vPasa = false;
            }

            return vPasa;
        }

        public ErrorDto sbInvInventario(int CodEmpresa, CompraInventarioDto req)
        {
            var result = new ErrorDto();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string procedure = "spINVAfectacion";
                var parameters = new
                {
                    CodProd = req.CodProducto,
                    Cantidad = req.Cantidad,
                    Bodega = req.CodBodega,
                    CodTipo = req.CodTipo,
                    Origen = req.Origen,
                    Fecha = req.Fecha,
                    Precio = req.Precio,
                    ImpCon = req.ImpConsumo,
                    ImpVenta = req.ImpVentas,
                    TipoMov = req.TipoMov,
                    Usuario = req.Usuario
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

        #endregion

        #region Verificación de líneas / productos / bodegas

        public ErrorDto fxInvVerificaLineaDetalle(int CodEmpresa, int ColCantidad, string vMov, int ColProd, int ColBod1, int ColBod2, List<FacturaDetalleDto> vGrid)
        {
            var result = new ErrorDto { Code = 1 };

            if (ColProd > 0 && vGrid.Count > 0)
            {
                int count = 0;
                foreach (var item in vGrid)
                {
                    count++;
                    if (item.cantidad > 0)
                    {
                        VerificaProducto(CodEmpresa, item, ref result, count);

                        if (ColBod1 > 0) VerificaBodega(CodEmpresa, item, vMov, ref result, count, true);
                        if (ColBod2 > 0) VerificaBodega(CodEmpresa, item, vMov, ref result, count, false);
                    }
                }
            }
            return result;
        }

        private void VerificaProducto(int codEmpresa, FacturaDetalleDto item, ref ErrorDto result, int count)
        {
            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                const string sql = @"
                    SELECT estado 
                    FROM pv_productos 
                    WHERE cod_producto = @CodProducto;";

                var exist = connection.Query<ProductoDto>(sql, new { CodProducto = item.cod_producto }).ToList();

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

        private void VerificaBodega(int codEmpresa, FacturaDetalleDto item, string vMov, ref ErrorDto result, int count, bool isEntrada)
        {
            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);

                const string sql = @"
                    SELECT permite_entradas, permite_salidas, estado 
                    FROM pv_bodegas 
                    WHERE cod_bodega = @CodBodega;";

                var bodega = connection.Query<Models.BodegaDto>(sql, new { CodBodega = item.cod_bodega }).FirstOrDefault();

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

                VerificaPermisosEntradaSalida(bodega, vMov, ref result, count, item.cod_bodega, isEntrada);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
        }

        private static void VerificaPermisosEntradaSalida(
      Models.BodegaDto bodega,
      string vMov,
      ref ErrorDto result,
      int count,
      string cod_bodega,
      bool isEntrada)
        {
            if (bodega == null) return;

            bool requiereEntrada = isEntrada
                ? vMov == "E"
                : vMov is "E" or "T";

            bool requiereSalida = isEntrada
                ? vMov is "S" or "R" or "T"
                : vMov is "R" or "S";

            if (requiereEntrada && bodega.permite_entradas == "0")
            {
                AgregarError(ref result, count, cod_bodega, "No Permite Entradas");
                return;
            }

            if (requiereSalida && bodega.permite_salidas == "0")
            {
                AgregarError(ref result, count, cod_bodega, "No Permite Salidas");
            }
        }

        private static void AgregarError(ref ErrorDto result, int count, string codBodega, string mensaje)
        {
            result.Code = 0;
            result.Description += $"\r\nL {count} - Bodega : {codBodega} - {mensaje}";
        }


        public bool fxInvPeriodoEstado(int CodEmpresa, string vfecha)
        {
            bool vPasa = false;

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT estado 
                    FROM pv_periodos 
                    WHERE anio = YEAR(@Fecha) 
                      AND mes  = MONTH(@Fecha);";

                var estado = connection.QueryFirstOrDefault<string>(sql, new { Fecha = vfecha });

                vPasa = estado != "C";
            }
            catch
            {
                // por seguridad queda false
            }

            return vPasa;
        }

        #endregion

        #region Parámetros / Autorizaciones

        public ErrorDto<ParametroValor> fxCxPParametro(int CodEmpresa, string Cod_Parametro)
        {
            var response = new ErrorDto<ParametroValor> { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT cod_parametro, valor 
                    FROM cxp_parametros 
                    WHERE cod_parametro = @CodParametro;";

                response.Result = connection.QueryFirstOrDefault<ParametroValor>(sql, new { CodParametro = Cod_Parametro });

                if (response.Result?.Valor == null)
                {
                    response.Result = new ParametroValor { Cod_Parametro = Cod_Parametro, Valor = "GEN" };
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

        public ErrorDto fxInvTransaccionesAutoriza(int CodEmpresa, string Boleta, string TipoTran, string AutorizaUser)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlUser = @"
                    SELECT genera_user 
                    FROM pv_invTransac 
                    WHERE Tipo = @TipoTran 
                      AND Boleta = @Boleta;";

                var generaUser = connection.ExecuteScalar<string>(sqlUser, new { TipoTran, Boleta });

                if (string.IsNullOrEmpty(generaUser))
                {
                    info.Description = $"No se encontró el usuario que generó la boleta '{Boleta}', verifique que la boleta exista";
                    return info;
                }

                const string sqlValida = @"
                    SELECT ISNULL(COUNT(*),0) 
                    FROM pv_orden_autousers 
                    WHERE Usuario = @AutorizaUser 
                      AND Usuario_Asignado = @GUser 
                      AND ENTRADAS = 1;";

                int valideAutorizacion = connection.ExecuteScalar<int>(sqlValida, new { AutorizaUser, GUser = generaUser });

                info.Code = valideAutorizacion;
                info.Description = valideAutorizacion == 1
                    ? generaUser
                    : "Usted no se encuentra Registrado como Autorizado del Usuario " + generaUser + " que Generó la Transacción...(Verifique)";

                return info;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        #endregion

        #region fxSIFCCodigos (consulta códigos genéricos)

        public ConsultaDescripcion fxSIFCCodigos(int CodEmpresa, string vTipoDC, string vCodDesX, string vTabla, int Cod_Conta)
        {
            var def = GetCodigoTablaDef(vTabla);
            if (def is null) return new ConsultaDescripcion();

            var sqlInfo = BuildCodigoSql(def, vTipoDC, vCodDesX, Cod_Conta);
            if (sqlInfo is null) return new ConsultaDescripcion();

            var (sql, parameters) = sqlInfo.Value;

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                return connection.QueryFirstOrDefault<ConsultaDescripcion>(sql, parameters) ?? new ConsultaDescripcion();
            }
            catch
            {
                return new ConsultaDescripcion();
            }
        }

        private sealed record CodigoTablaDef(string Table, string CodeColumn, string DescColumn, bool UsaCodConta);

        private static CodigoTablaDef? GetCodigoTablaDef(string vTabla)
        {
            var table = vTabla.ToUpperInvariant();

            return table switch
            {
                "PROVEEDORES" => new CodigoTablaDef("cxp_proveedores", "Cod_proveedor", "Descripcion", false),
                "PRODUCTOS" => new CodigoTablaDef("pv_Productos", "cod_Producto", _descripcion, false),
                "CARGOSPROV" => new CodigoTablaDef("cxp_cargos", "Cod_cargo", _descripcion, false),
                "UNIDADES" => new CodigoTablaDef("pv_unidades", "Cod_Unidad", _descripcion, false),
                "MARCAS" => new CodigoTablaDef("pv_marcas", "Cod_Marca", _descripcion, false),
                "LINEAPRODUCTO" => new CodigoTablaDef("pv_prod_clasifica", "cod_prodclas", _descripcion, false),
                "BANCOS" => new CodigoTablaDef("Tes_Bancos", "id_banco", _descripcion, false),
                "CLIENTES" => new CodigoTablaDef("pv_clientes", "cedula", "nombre", false),
                "BODEGAS" => new CodigoTablaDef("pv_bodegas", "cod_bodega", _descripcion, false),
                "PRECIOS" => new CodigoTablaDef("pv_tipos_precios", "cod_precio", _descripcion, false),
                "AGENTES" => new CodigoTablaDef("pv_agentes", "cod_agente", "Nombre", false),
                "CAJAS" => new CodigoTablaDef("pv_cajas", "cod_caja", "Nombre", false),
                "CUENTAS" => new CodigoTablaDef("CntX_cuentas", "cod_Cuenta", _descripcion, true),
                _ => null
            };
        }

        private static (string sql, object parameters)? BuildCodigoSql(CodigoTablaDef def, string vTipoDC, string vCodDesX, int codConta)
        {
            bool porCodigo = vTipoDC == "D";

            // Quoting seguro de identificadores (tabla/col)
            var table = SafeIdent(def.Table);
            var codeCol = SafeIdent(def.CodeColumn);
            var descCol = SafeIdent(def.DescColumn);

            string where;
            object parameters;

            if (porCodigo)
            {
                where = def.UsaCodConta
                    ? $"WHERE {codeCol} = @Code AND [cod_contabilidad] = @CodConta"
                    : $"WHERE {codeCol} = @Code";

                parameters = def.UsaCodConta
                    ? new { Code = vCodDesX, CodConta = codConta }
                    : new { Code = vCodDesX };
            }
            else
            {
                where = def.UsaCodConta
                    ? $"WHERE {descCol} = @Desc AND [cod_contabilidad] = @CodConta"
                    : $"WHERE {descCol} = @Desc";

                parameters = def.UsaCodConta
                    ? new { Desc = vCodDesX, CodConta = codConta }
                    : new { Desc = vCodDesX };
            }

            var sql = $@"
SELECT {codeCol} AS CodX, {descCol} AS DescX
FROM {table}
{where};";

            return (sql, parameters);
        }

        #endregion

        #region Utilidades simples

        public static bool fxCorreoValido(string correo)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, patron);
        }

        public static string fxConvertModelToXml<T>(T model)
        {
            if (EqualityComparer<T>.Default.Equals(model, default(T)))
                throw new ArgumentNullException(nameof(model));

            var serializer = new XmlSerializer(typeof(T));
            using var writer = new StringWriter();
            serializer.Serialize(writer, model);
            var xmlOutput = writer.ToString();

            xmlOutput = xmlOutput.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            xmlOutput = xmlOutput.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
            xmlOutput = xmlOutput.Trim();
            xmlOutput = xmlOutput.Replace(" xsi:nil=\"true\" ", "");
            xmlOutput = xmlOutput.Replace("false", "0").Replace("true", "1");

            return xmlOutput;
        }

        public ErrorDto<int> ActivosSinAsignar_Obtener(int CodEmpresa, string usuario)
        {
            var result = new ErrorDto<int> { Code = 0, Result = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT COUNT(*)  
                    FROM PV_CONTROL_ACTIVOS
                    WHERE ENTREGA_USUARIO = ''
                      AND ESTADO IN ('P', 'R') 
                      AND REGISTRO_USUARIO = @Usuario;";

                result.Result = connection.QueryFirstOrDefault<int>(sql, new { Usuario = usuario });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }

            return result;
        }

        public string? validaFechaGlobal(DateTime? fecha)
        {
            try
            {
                return fecha.HasValue ? fecha.Value.ToString(dateFormat) : null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Bitácoras

        public ErrorDto BitacoraProducto(BitacoraProductoInsertarDto req)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(req.EmpresaId);

                const string sql = @"
                    INSERT INTO [dbo].[BITACORA_PRODUCTOS]
                        ([COD_PRODUCTO], [CONSEC], [MOVIMIENTO], [DETALLE], [REGISTRO_FECHA], [REGISTRO_USUARIO])
                    VALUES
                        (@CodProducto, @Consec, @Movimiento, @Detalle, GETDATE(), @RegistroUsuario);";

                resp.Code = connection.Execute(sql, new
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

        public ErrorDto BitacoraProveedor(BitacoraProveedorInsertarDto req)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(req.EmpresaId);

                const string sql = @"
                    INSERT INTO [dbo].[BITACORA_PROVEEDOR]
                        ([COD_PROVEEDOR], [CONSEC], [MOVIMIENTO], [DETALLE], [REGISTRO_FECHA], [REGISTRO_USUARIO])
                    VALUES
                        (@CodProveedor, @Consec, @Movimiento, @Detalle, GETDATE(), @RegistroUsuario);";

                resp.Code = connection.Execute(sql, new
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

        #endregion

        #region Otros helpers

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    SELECT CODIGO_SINPE AS item, RTRIM(Descripcion) AS descripcion 
                    FROM AFI_TIPOS_IDS 
                    ORDER BY Tipo_Id;";

                resp.Result = connection.Query<DropDownListaGenericaModel>(sql).ToList();
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
            var resp = new ErrorDto<string> { Code = 0 };

            long parteEntera = (long)Math.Floor(numero);
            int parteDecimal = (int)((numero - parteEntera) * 100);

            string letrasEntera = parteEntera.ToWords(new CultureInfo("es"));
            if (letrasEntera.Equals("uno", StringComparison.CurrentCultureIgnoreCase))
                letrasEntera = "Un";

            letrasEntera = char.ToUpper(letrasEntera[0]) + letrasEntera[1..];

            string letrasDecimal = parteDecimal > 0 ? $" con {parteDecimal.ToWords(new CultureInfo("es"))} " : "";

            resp.Result = letrasEntera + letrasDecimal;
            return resp;
        }

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

        #endregion

        #region FONDOS v6 - Control de Cambios

        public int FndControlAutoriza_Guardar(FndControlAutorizaData request)
        {
            if (controlAuth != "Y") return 3;

            var result = new ErrorDto { Code = 0 };

            try
            {
                if (!IsSqlInternoSeguro(request.strSQL))
                    return SetErrorResult(result, "SQL no permitido por políticas de seguridad.");

                var match = ParseUpdateSql(request.strSQL);
                if (match == null)
                    return SetErrorResult(result, "La sentencia SQL no es válida o no se puede analizar.");

                string table = match.Groups["table"].Value.Trim();
                string setClause = match.Groups["setClause"].Value.Trim();
                string whereClause = match.Groups["whereClause"].Value.Trim();

                if (!IsWhereSeguro(whereClause))
                    return SetErrorResult(result, "WHERE no permitido por políticas de seguridad.");

                using var connection = _portalDB.CreateConnection(request.CodEmpresa);

                // SONAR S2077: SQL dinámico intencional (control de cambios interno).
                // Mitigación: SQL validado (tokens), tabla validada (identificador), WHERE validado.
                var safeTable = SafeIdent(table);
                var dtTable = connection.Query($"SELECT * FROM {safeTable} WHERE {whereClause}").FirstOrDefault();

                if (dtTable == null)
                    return SetErrorResult(result, $"❌ No se encontró información en {table} con la condición: {whereClause}");

                var diferencias = ObtenerDiferencias(connection, table, setClause, whereClause, dtTable);
                var dtDiferencias = CrearDataTableDiferencias(diferencias);

                if (dtDiferencias.Rows.Count == 0)
                    return SetErrorResult(result, "No se encontraron diferencias entre los valores originales y los nuevos.", 2);

                var ctx = new ControlCambioContext(request.CodEmpresa, request.usuario);

                var payload = new ControlCambioPayload(
                    TipoCambio: request.tipoCambio,
                    Tabla: table,
                    Llave: whereClause,
                    EventoQuery: "UPDATE",
                    InsertSql: "",
                    Diferencias: dtDiferencias
                );

                result = InsertarTablaControl(ctx, payload);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result.Code ?? -1;
        }

        private static Match? ParseUpdateSql(string sql)
        {
            string pattern = @"UPDATE\s+(?<table>\w+)\s+SET\s+(?<setClause>.+?)\s+WHERE\s+(?<whereClause>.+)$";
            var match = Regex.Match(sql, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match : null;
        }

        private static int SetErrorResult(ErrorDto result, string description, int code = -1)
        {
            result.Code = code;
            result.Description = description;
            return result.Code ?? -1;
        }

        private static List<(string Campo, object ValorOriginal, object ValorNuevo)> ObtenerDiferencias(
            SqlConnection connection,
            string table,
            string setClause,
            string whereClause,
            object dtTable)
        {
            if (!IsWhereSeguro(whereClause))
                return new List<(string Campo, object ValorOriginal, object ValorNuevo)>();

            var selectParts = new List<string>();

            foreach (var assignment in SplitSetClauseSafely(setClause))
            {
                var splitIndex = assignment.IndexOf('=');
                if (splitIndex <= 0) continue;

                var column = assignment[..splitIndex].Trim();
                var expression = assignment[(splitIndex + 1)..].Trim();

                if (!IdentRegex.IsMatch(column))
                    throw new SecurityException("Columna inválida en SET.");

                selectParts.Add($"{expression} AS {SafeIdent(column)}");
            }

            var safeTable = SafeIdent(table);

            // SONAR S2077: SQL dinámico intencional (comparación valores).
            // Mitigación: tabla/columnas validadas y WHERE validado.
            string selectStatement = $"SELECT {string.Join(", ", selectParts)} FROM {safeTable} WHERE {whereClause};";
            var dtTableNew = connection.Query(selectStatement).FirstOrDefault();

            var dicOriginal = (IDictionary<string, object>)dtTable;
            var dicNuevo = dtTableNew as IDictionary<string, object>;

            if (dicNuevo == null)
                return new List<(string Campo, object ValorOriginal, object ValorNuevo)>();

            var diferencias = new List<(string Campo, object ValorOriginal, object ValorNuevo)>();

            foreach (var kvp in dicOriginal)
            {
                var key = kvp.Key;
                var valorOriginal = kvp.Value;

                if (dicNuevo.TryGetValue(key, out var valorNuevo) && !SonIguales(valorOriginal, valorNuevo))
                    diferencias.Add((key, valorOriginal, valorNuevo));
            }

            return diferencias;
        }

        private static DataTable CrearDataTableDiferencias(List<(string Campo, object ValorOriginal, object ValorNuevo)> diferencias)
        {
            var dt = new DataTable();
            dt.Columns.Add("Campo", typeof(string));
            dt.Columns.Add("ValorOriginal", typeof(object));
            dt.Columns.Add("ValorNuevo", typeof(object));

            foreach (var dif in diferencias)
                dt.Rows.Add(dif.Campo, dif.ValorOriginal, dif.ValorNuevo);

            return dt;
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
                    quotes ^= 1;

                if (quotes == 0)
                {
                    if (c == '(') parentheses++;
                    else if (c == ')') parentheses--;
                    else if (c == ',' && parentheses == 0)
                    {
                        parts.Add(input[start..i].Trim());
                        start = i + 1;
                    }
                }
            }

            if (start < input.Length)
                parts.Add(input[start..].Trim());

            return parts;
        }

        private static bool SonIguales(object valorOriginal, object valorNuevo)
        {
            if (valorOriginal == null && valorNuevo == null) return true;
            if (valorOriginal == null || valorNuevo == null) return false;

            valorOriginal = ConvertirBoolANumero(valorOriginal);
            valorNuevo = ConvertirBoolANumero(valorNuevo);

            if (valorOriginal is DateTime dtOriginal)
            {
                if (TryConvertToDateTime(valorNuevo, out var dtNuevo))
                    return dtOriginal.Date == dtNuevo.Date;
                return false;
            }

            if (decimal.TryParse(valorOriginal.ToString(), out var num1) &&
                decimal.TryParse(valorNuevo.ToString(), out var num2))
                return num1 == num2;

            var strOriginal = valorOriginal.ToString()?.Trim() ?? string.Empty;
            var strNuevo = valorNuevo.ToString()?.Trim() ?? string.Empty;
            return strOriginal.Equals(strNuevo, StringComparison.OrdinalIgnoreCase);
        }

        private static object ConvertirBoolANumero(object valor)
        {
            if (valor is bool b) return b ? 1 : 0;
            if (bool.TryParse(valor?.ToString(), out var parsedBool)) return parsedBool ? 1 : 0;
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

            string[] formatos = { "yyyyMMdd", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };

            return DateTime.TryParseExact(str, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
        }

        public record ControlCambioContext(int CodEmpresa, string Usuario);

        public record ControlCambioPayload(int TipoCambio, string Tabla, object Llave, string EventoQuery, string? InsertSql, DataTable? Diferencias);

        private ErrorDto InsertarTablaControl(ControlCambioContext ctx, ControlCambioPayload payload)
        {
            var result = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(ctx.CodEmpresa);

                var jsonLlave = JsonConvert.SerializeObject(payload.Llave);
                string valoresJsonAct;
                string? valoresJsonDif = null;

                if (payload.Diferencias != null)
                {
                    var original = payload.Diferencias.Copy();
                    valoresJsonAct = JsonConvert.SerializeObject(original, Formatting.Indented);

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

        public int FndControlCambios_Autoriza(int CodEmpresa, int idCambio, string usuario)
        {
            var result = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sqlCambio = @"
                    SELECT * 
                    FROM FND_CONTROL_CAMBIOS_APROB 
                    WHERE ID_CAMBIO = @IdCambio;";

                var dtCambio = connection.Query<FndControlCambioAprobDto>(sqlCambio, new { IdCambio = idCambio }).FirstOrDefault();
                if (dtCambio == null)
                    return SetErrorResult(result, "No se encontró el registro en la tabla de control.");

                if (!IsSqlInternoSeguro(dtCambio.valoresjsonact ?? string.Empty) && dtCambio.cod_evento == "INSERT")
                    return SetErrorResult(result, "SQL INSERT no permitido por políticas de seguridad.");

                if (!IsWhereSeguro(dtCambio.llaves?.Trim('"') ?? string.Empty))
                    return SetErrorResult(result, "WHERE no permitido por políticas de seguridad.");

                if (string.IsNullOrWhiteSpace(dtCambio.nom_tabla))
                    return SetErrorResult(result, "El nombre de la tabla es nulo o vacío.");

                string query;
                var safeTable = SafeIdent(dtCambio.nom_tabla);

                switch (dtCambio.cod_evento)
                {
                    case "UPDATE":
                        var cambios = JsonConvert.DeserializeObject<List<CampoCambio>>(dtCambio.valoresjsondif ?? string.Empty) ?? new List<CampoCambio>();

                        // columnas validadas
                        var setParts = cambios
                            .Where(c => !string.IsNullOrWhiteSpace(c.Campo))
                            .Select(c =>
                        {
                            if (!IdentRegex.IsMatch(c.Campo ?? string.Empty))
                                throw new SecurityException("Campo inválido en cambios.");

                            return $"{SafeIdent(c.Campo!)} = {FormatearValorSql(c.ValorNuevo ?? string.Empty)}";
                        });

                        var llaves = dtCambio.llaves?.Trim('"') ?? string.Empty;

                        // SONAR S2077: SQL dinámico intencional (aplicación cambios aprobados).
                        query = $"UPDATE {safeTable} SET {string.Join(", ", setParts)} WHERE {llaves};";
                        result.Code = connection.Execute(query);
                        break;

                    case "INSERT":
                        // INSERT se guarda como SQL completo (histórico).
                        // Se asume SQL interno, bloqueado por IsSqlInternoSeguro.
                        query = dtCambio.valoresjsonact?.Trim('"') ?? string.Empty;
                        if (!IsSqlInternoSeguro(query))
                            return SetErrorResult(result, "SQL INSERT no permitido por políticas de seguridad.");

                        // SONAR S2077: ejecución controlada (insert interno validado).
                        result.Code = connection.Execute(query);
                        break;

                    case "DELETE":
                        var llavesDelete = dtCambio.llaves?.Trim('"') ?? string.Empty;

                        // SONAR S2077: SQL dinámico intencional (delete aprobado).
                        query = $"DELETE FROM {safeTable} WHERE {llavesDelete};";
                        result.Code = connection.Execute(query);
                        break;

                    default:
                        return SetErrorResult(result, "Tipo de evento no soportado.");
                }

                if (result.Code != -1)
                {
                    const string sqlUpdateEstado = @"
                        UPDATE FND_CONTROL_CAMBIOS_APROB 
                        SET COD_ESTADO = 'V', 
                            USUARIO_APRUEBA = @Usuario, 
                            FECHA_APRUEBA = GETDATE()
                        WHERE ID_CAMBIO = @IdCambio;";

                    connection.Execute(sqlUpdateEstado, new { Usuario = usuario, IdCambio = idCambio });
                    result.Description = "ok";
                }
                else
                {
                    return SetErrorResult(result, "Error al actualizar la tabla de control.");
                }
            }
            catch (Exception ex)
            {
                return SetErrorResult(result, ex.Message);
            }

            return result.Code ?? -1;
        }

        private static string FormatearValorSql(object valor)
        {
            if (valor == null || valor is JValue jVal && jVal.Type == JTokenType.Null)
                return "NULL";

            if (valor is JValue jv)
            {
                return jv.Type switch
                {
                    JTokenType.Boolean => (bool)jv ? "1" : "0",
                    JTokenType.String => $"'{jv.ToString().Replace("'", "''")}'",
                    JTokenType.Integer => jv.ToString(),
                    JTokenType.Float => jv.ToString(),
                    _ => $"'{jv.ToString().Replace("'", "''")}'"
                };
            }

            if (valor is bool b) return b ? "1" : "0";
            if (valor is string s) return $"'{s.Replace("'", "''")}'";

            return valor.ToString() ?? "NULL";
        }

        public int FndControlAutoriza_Eliminar(FndControlAutorizaData request)
        {
            var result = new ErrorDto { Code = 0 };
            if (controlAuth != "Y") return 3;

            try
            {
                if (!IsSqlInternoSeguro(request.strSQL))
                    return SetErrorResult(result, "SQL no permitido por políticas de seguridad.");

                const string patternDelete = @"DELETE\s+(FROM\s+)?(?<table>\w+)\s+WHERE\s+(?<whereClause>.+)$";
                var matchDelete = Regex.Match(request.strSQL, patternDelete, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!matchDelete.Success)
                    return SetErrorResult(result, "La sentencia SQL no es válida o no se puede analizar.");

                var table = matchDelete.Groups["table"].Value;
                var whereClause = matchDelete.Groups["whereClause"].Value;

                if (!IsWhereSeguro(whereClause))
                    return SetErrorResult(result, "WHERE no permitido por políticas de seguridad.");

                _ = SafeIdent(table); // valida tabla

                var ctx = new ControlCambioContext(request.CodEmpresa, request.usuario);

                using var diferenciasTable = new DataTable();
                var payload = new ControlCambioPayload(request.tipoCambio, table, whereClause, "DELETE", "", diferenciasTable);

                result = InsertarTablaControl(ctx, payload);
            }
            catch (Exception ex)
            {
                return SetErrorResult(result, ex.Message);
            }

            return result.Code ?? -1;
        }

        public int FndControlAutoriza_Insertar(FndControlAutorizaData request)
        {
            var result = new ErrorDto { Code = 0 };
            if (controlAuth != "Y") return 3;

            try
            {
                if (!IsSqlInternoSeguro(request.strSQL))
                    return SetErrorResult(result, "SQL no permitido por políticas de seguridad.");

                const string patternInsert = @"insert\s+(?:into\s+)?(?<table>\w+)\s*\((?<columns>[^)]+)\)\s*values\s*\((?<values>.+?)\)";
                var matchInsert = Regex.Match(request.strSQL, patternInsert, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (!matchInsert.Success)
                    return SetErrorResult(result, "La sentencia SQL no es válida o no se puede analizar.");

                var table = matchInsert.Groups["table"].Value;
                _ = SafeIdent(table); // valida tabla

                var ctx = new ControlCambioContext(request.CodEmpresa, request.usuario);
                var payload = new ControlCambioPayload(request.tipoCambio, table, "", "INSERT", request.strSQL, null);

                result = InsertarTablaControl(ctx, payload);
            }
            catch (Exception ex)
            {
                return SetErrorResult(result, ex.Message);
            }

            return result.Code ?? -1;
        }

        #endregion

        // ========= Seguridad (hotspots) =========

        private static string SafeIdent(string ident)
        {
            if (string.IsNullOrWhiteSpace(ident) || !IdentRegex.IsMatch(ident))
                throw new SecurityException("Identificador SQL inválido.");

            return $"[{ident}]";
        }

        private static bool IsWhereSeguro(string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause)) return false;

            // Bloquea multi statement + comentarios
            if (whereClause.Contains(";") || whereClause.Contains("--") || whereClause.Contains("/*") || whereClause.Contains("*/"))
                return false;

            // Bloquea keywords peligrosas dentro del WHERE
            var banned = new[] { " drop ", " alter ", " create ", " truncate ", " exec", " execute ", " merge ", " grant ", " revoke " };
            var lower = " " + whereClause.ToLowerInvariant() + " ";
            if (banned.Any(b => lower.Contains(b))) return false;

            // Restricción simple: permite caracteres típicos de filtros (ajusta si necesitas)
            // letras/números/_/[]/=/</>/!/()/espacios/'/"/./,/AND/OR/IS/NULL/LIKE/IN
            // Si esto te bloquea casos válidos, lo ajustamos.
            if (!Regex.IsMatch(whereClause, @"^[A-Za-z0-9_\[\]\s\=\<\>\!\(\)'\."","",%+-]+$"))
                return false;

            return true;
        }

        private static bool IsSqlInternoSeguro(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;

            if (sql.Contains(";") || sql.Contains("--") || sql.Contains("/*") || sql.Contains("*/"))
                return false;

            string[] banned =
            {
                " xp_", " sp_", "exec", "execute",
                "drop", "alter", "create", "truncate",
                "grant", "revoke", "merge"
            };

            var lower = " " + sql.ToLowerInvariant() + " ";
            return !banned.Any(b => lower.Contains(b));
        }
    }
}