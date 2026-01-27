using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class MTesFuncionesDb
    {
        private readonly IConfiguration _config;
        private readonly SeguridadPortalDb _seguridadPortal;
        private readonly MTesoreria mTesoreria;
        public const string zero6Append = "000000";
        public const string zero12Append = "000000000000";
        public const string fechaFormat = "yyyy/MM/dd";
        public const string fechaFormat2 = "ddMMyyyy";

        public MTesFuncionesDb(IConfiguration config)
        {
            _config = config;
            _seguridadPortal = new SeguridadPortalDb(config);
            mTesoreria = new MTesoreria(config);
        }

        private string GetEmpresaConn(int codEmpresa) =>
            new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

        private static string Trunc(string? value, int maxLen)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length > maxLen ? value.Substring(0, maxLen) : value;
        }

        public long fxgTesoreriaMaestro(int CodEmpresa, string usuario, TesoreriaMaestroModel tesoreria)
        {
            try
            {
                var stringConn = GetEmpresaConn(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var detalle1 = Trunc(tesoreria.vDetalle1, 26);
                var detalle2 = Trunc(tesoreria.vDetalle2, 26);

                // Insert + retorno del ID insertado (evita MAX(nsolicitud))
                const string sqlInsertCk = @"
INSERT INTO Tes_Transacciones (
    id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud,
    estado, estadoi, modulo, submodulo, cta_ahorros, detalle1, detalle2,
    referencia, op, genera, actualiza, cod_unidad, cod_concepto,
    user_solicita, autoriza, fecha_autorizacion, user_autoriza,
    ref_01, ref_02, ref_03, cod_app, ID_TOKEN, REMESA_TIPO, REMESA_ID
) VALUES (
    @Banco, @TipoDocumento, @Codigo, @Beneficiario, @Monto, @Fecha,
    'P', 'P', 'CC', 'C', @Cuenta, @Detalle1, @Detalle2,
    @Referencia, @OP, 'S', 'S', @Unidad, @Concepto,
    @Usuario, 'S', GETDATE(), @Usuario,
    @Ref01, @Ref02, @Ref03, @CodApp, @Token, @RemesaTipo, @RemesaId
);
SELECT CAST(SCOPE_IDENTITY() as bigint);";

                const string sqlInsertNoCk = @"
INSERT INTO Tes_Transacciones (
    id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud,
    estado, estadoi, modulo, submodulo, cta_ahorros, detalle1, detalle2,
    referencia, op, genera, actualiza, cod_unidad, cod_concepto,
    ref_01, ref_02, ref_03, cod_app, ID_TOKEN, REMESA_TIPO, REMESA_ID,
    user_solicita
) VALUES (
    @Banco, @TipoDocumento, @Codigo, @Beneficiario, @Monto, @Fecha,
    'P', 'P', 'CC', 'C', @Cuenta, @Detalle1, @Detalle2,
    @Referencia, @OP, 'S', 'S', @Unidad, @Concepto,
    @Ref01, @Ref02, @Ref03, @CodApp, @Token, @RemesaTipo, @RemesaId,
    @Usuario
);
SELECT CAST(SCOPE_IDENTITY() as bigint);";

                var args = new
                {
                    Banco = tesoreria.vBanco,
                    TipoDocumento = tesoreria.vTipoDocumento,
                    Codigo = tesoreria.vCodigo,
                    Beneficiario = tesoreria.vBeneficiario,
                    Monto = tesoreria.vMonto,
                    Fecha = tesoreria.vFecha,
                    Cuenta = tesoreria.vCuenta,
                    Detalle1 = detalle1,
                    Detalle2 = detalle2,
                    Referencia = tesoreria.vReferencia,
                    OP = tesoreria.vOP,
                    Unidad = tesoreria.vUnidad,
                    Concepto = tesoreria.vConcepto,
                    Usuario = usuario,
                    Ref01 = tesoreria.vRef_01,
                    Ref02 = tesoreria.vRef_02,
                    Ref03 = tesoreria.vRef_03,
                    CodApp = tesoreria.vCodApp,
                    Token = tesoreria.vToken,
                    RemesaTipo = tesoreria.vRemesaTipo,
                    RemesaId = tesoreria.vRemesa
                };

                var isCk = tesoreria.vTipoDocumento.Equals("CK", StringComparison.OrdinalIgnoreCase);
                var nsolicitud = connection.QuerySingle<long>(isCk ? sqlInsertCk : sqlInsertNoCk, args);

                // Validación de consistencia (opcional, pero mantiene tu lógica)
                const string sqlCheck = @"SELECT TOP 1 * FROM Tes_Transacciones WHERE nsolicitud = @Nsolicitud;";
                var row = connection.QueryFirstOrDefault<Models.TesTransaccionesDto>(sqlCheck, new { Nsolicitud = nsolicitud });

                if (row != null && string.Equals(row.CODIGO?.Trim(), tesoreria.vCodigo?.Trim(), StringComparison.Ordinal))
                    return nsolicitud;

                // Fallback (si por alguna razón no coincidiera)
                const string sqlFallback = @"
SELECT TOP 1 CAST(nsolicitud as bigint)
FROM Tes_Transacciones
WHERE codigo = @Codigo AND op = @OP
ORDER BY nsolicitud DESC;";

                return connection.QueryFirstOrDefault<long>(sqlFallback, new { Codigo = tesoreria.vCodigo, OP = tesoreria.vOP });
            }
            catch
            {
                return 0;
            }
        }

        public void sbgTesoreriaDetalle(int CodEmpresa, TesoreriaDetalleModel detalle)
        {
            try
            {
                var stringConn = GetEmpresaConn(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
INSERT INTO Tes_Trans_Asiento (
    nsolicitud, cuenta_contable, monto, debehaber, linea, cod_unidad, cod_cc
) VALUES (
    @Solicitud, @CtaConta, @Monto, @DH, @Linea, @Unidad, @CC
);";

                connection.Execute(sql, new
                {
                    Solicitud = detalle.vSolicitud,
                    CtaConta = detalle.vCtaConta,
                    Monto = detalle.vMonto,
                    DH = detalle.vDH,
                    Linea = detalle.vLinea,
                    Unidad = detalle.vUnidad,
                    CC = detalle.vCC
                });
            }
            catch
            {
                // ideal: log
            }
        }

        public static string fxTipoDocumento(string tipo)
        {
            return tipo switch
            {
                "CK" => "Cheque",
                "TE" => "Transferencia",
                "EF" or "RE" => "Efectivo",
                "ND" => "Nota Debito",
                "NC" => "Nota Credito",
                "OT" => "Otro...",
                "CD" => "Ctrl Desembolsos",
                "CP" => "Proveedor",
                "RC" => "Retiro en Caja",
                "FD" => "Fondo Transitorio",
                "TS" => "Transferencia SINPE",

                "Cheque" => "CK",
                "Transferencia" => "TE",
                "Efectivo" => "EF",
                "Nota Debito" => "ND",
                "Nota Credito" => "NC",
                "Otro..." => "OT",
                "Ctrl Desembolsos" => "CD",
                "Proveedor" => "CP",
                "Retiro en Caja" => "RC",
                "Fondo Transitorio" => "FD",
                "Transferencia SINPE" => "TS",

                _ => string.Empty
            };
        }

        public string fxTesToken(int CodEmpresa, string usuario)
        {
            try
            {
                var stringConn = GetEmpresaConn(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // OJO: esto sigue pudiendo colisionar en concurrencia.
                // Ideal: manejar con constraint + reintento, o generar token con GUID.
                var prefix = DateTime.Now.ToString("yyyy.MM.dd");

                const string sqlConsec = @"
                                        SELECT ISNULL(COUNT(id_token),0) + 1
                                        FROM tes_tokens
                                        WHERE id_token LIKE @PrefixLike;";

                var consec = connection.QuerySingle<int>(sqlConsec, new { PrefixLike = prefix + "%" });
                var token = $"{prefix}{consec}";

                const string sqlInsert = @"
                                        INSERT tes_tokens (id_token, registro_fecha, registro_usuario, estado)
                                        VALUES (@Token, GETDATE(), @Usuario, 'A');";

                connection.Execute(sqlInsert, new { Token = token, Usuario = usuario });

                return token;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool fxgTESValidaDatos(int CodEmpresa, int Contabilidad, string vTipo, string vCodigo, string vFiltro = "")
        {
            try
            {
                var stringConn = GetEmpresaConn(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string sql = vTipo.ToUpperInvariant() switch
                {
                    "CONCEPTO" => @"
SELECT ISNULL(COUNT(*),0)
FROM tes_conceptos
WHERE cod_concepto = @Codigo AND Estado = 'A';",

                    "UNIDAD" => @"
SELECT ISNULL(COUNT(*),0)
FROM CntX_unidades
WHERE cod_unidad = @Codigo AND Activa = 1 AND cod_Contabilidad = @Contabilidad;",

                    "CC" => @"
SELECT ISNULL(COUNT(*),0)
FROM CNTX_CENTRO_COSTOS
WHERE COD_CENTRO_COSTO = @Codigo
  AND Activo = 1
  AND cod_contabilidad = @Contabilidad
  AND (
        @Filtro = '' OR COD_CENTRO_COSTO IN (
            SELECT COD_CENTRO_COSTO
            FROM CNTX_UNIDADES_CC
            WHERE cod_unidad = @Filtro AND cod_contabilidad = @Contabilidad
        )
  );",

                    _ => ""
                };

                if (string.IsNullOrWhiteSpace(sql)) return false;

                var existe = connection.QuerySingle<int>(sql, new
                {
                    Codigo = vCodigo,
                    Contabilidad,
                    Filtro = vFiltro ?? ""
                });

                return existe > 0;
            }
            catch
            {
                return false;
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbgTESBusqueda(int CodEmpresa, int Contabilidad, string vTipo, string vFiltro = "")
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                var stringConn = GetEmpresaConn(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string sql = vTipo.ToUpperInvariant() switch
                {
                    "CONCEPTO" => @"
SELECT cod_concepto as item, descripcion
FROM tes_conceptos
WHERE Estado = 'A'
ORDER BY cod_concepto;",

                    "UNIDAD" => @"
SELECT cod_unidad as item, descripcion
FROM CntX_unidades
WHERE Activa = 1 AND cod_Contabilidad = @Contabilidad
ORDER BY cod_unidad;",

                    "CC" => @"
SELECT COD_CENTRO_COSTO as item, descripcion
FROM CNTX_CENTRO_COSTOS
WHERE Activo = 1
  AND cod_contabilidad = @Contabilidad
  AND (
        @Filtro = '' OR COD_CENTRO_COSTO IN (
            SELECT COD_CENTRO_COSTO
            FROM CNTX_UNIDADES_CC
            WHERE cod_unidad = @Filtro AND cod_contabilidad = @Contabilidad
        )
  )
ORDER BY COD_CENTRO_COSTO;",

                    _ => ""
                };

                if (string.IsNullOrWhiteSpace(sql))
                {
                    response.Code = -1;
                    response.Description = $"Tipo no soportado: {vTipo}";
                    response.Result = null;
                    return response;
                }

                response.Result = connection.Query<DropDownListaGenericaModel>(sql, new
                {
                    Contabilidad,
                    Filtro = vFiltro ?? ""
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
                return response;
            }
        }

        #region Manejo de Archivos Bancos

        /// <summary>
        /// { bancoConsec, extension, contenido } (evita duplicación en returns)
        /// </summary>
        private static ErrorDto<object> ArchivoResponse(long bancoConsec, string extension, StringBuilder sb)
            => OkJson(new
            {
                bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
                extension,
                contenido = sb.ToString()
            });

        private static ErrorDto<object> OkJson(object payload)
            => DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(payload, Formatting.Indented));

        private string GetParametro(int codEmpresa, string codigo)
           => mTesoreria.fxTesParametro(codEmpresa, codigo);

        public ErrorDto<object> SbTeBancoNacionalCore(
                SqlConnection conn,
                int codEmpresa,
                int bancoId,
                string tipoDoc, // puede venir null/"" si no aplica para el escenario de consecutivo provisto
                List<TesTransaccionDto> transaccionesList,
                int? curPlanilla,
                Func<long> resolveConsecutivo // define si se calcula o viene dado
            )
            {

                DateTime fecha = DateTime.Now;

                decimal montoPlanilla = curPlanilla ?? 0m;
                string strMontoPlanilla = montoPlanilla
                    .ToString("0000000000.00", CultureInfo.InvariantCulture)
                    .Replace(".", "");

                string cuentaEmpresa = "";
                string numCliente = "";

                decimal montoDetalles = 0m;
                long sumaCuentas = 0;

                try
                {
                    // Concepto
                    string empresaName = "TF " + _seguridadPortal.SeleccionarPgxClientePorCodEmpresa(codEmpresa).PGX_CORE_DB;
                    string concepto = empresaName.PadRight(30, ' ');

                    // Banco data
                    const string qBanco = "select Cta, codigo_Cliente from tes_Bancos Where id_Banco = @banco";
                    var bancoData = conn.QueryFirstOrDefault(qBanco, new { banco = bancoId });

                    if (bancoData != null)
                    {
                        cuentaEmpresa = (bancoData.Cta ?? "").ToString().Trim().Replace("-", "");
                        numCliente = (bancoData.codigo_Cliente ?? "").ToString().PadLeft(6, '0');
                    }

                    // Consecutivo (calculado o provisto)
                    long bancoConsec = resolveConsecutivo();

                    var sb = new StringBuilder();

                    // Header
                    var header = new StringBuilder(120);
                    header.Append('1');
                    header.Append(numCliente);
                    header.Append(fecha.Day.ToString("00", CultureInfo.InvariantCulture));
                    header.Append(fecha.Month.ToString("00", CultureInfo.InvariantCulture));
                    header.Append(fecha.Year.ToString("0000", CultureInfo.InvariantCulture));
                    header.Append(bancoId.ToString("D12", CultureInfo.InvariantCulture));
                    header.Append("10000");
                    header.Append(strMontoPlanilla);
                    header.Append("000000000000000000000000");
                    sb.AppendLine(header.ToString());

                    // Detalles
                    int i = 0;
                    foreach (var item in transaccionesList)
                    {
                        i++;

                        string cuenta = (item.cta_ahorros ?? "").Replace("-", "").Trim();
                        if (cuenta.Length < 12)
                            return DbHelper.CreateErrorResponse<object>($"Cuenta inválida en solicitud {item.nsolicitud}.");

                        decimal monto = item.monto ?? 0m;
                        montoDetalles += monto;

                        // Sumatoria de cuentas (sin dígito verificador): últimos 7, tomar 6
                        if (cuenta.Length >= 7)
                            sumaCuentas += long.Parse(cuenta.Substring(cuenta.Length - 7, 6), CultureInfo.InvariantCulture);

                        var linea = new StringBuilder(160);
                        linea.Append('3');
                        linea.Append(cuenta.Substring(5, 3));
                        linea.Append(cuenta.Substring(0, 3));
                        linea.Append("01");
                        linea.Append(cuenta.Substring(cuenta.Length - 7));
                        linea.Append(i.ToString("D8", CultureInfo.InvariantCulture));

                        string strMontoDet = monto.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                        linea.Append(strMontoDet);
                        linea.Append(concepto);
                        linea.Append("00");

                        sb.AppendLine(linea.ToString());
                    }

                    // Débito empresa
                    if (string.IsNullOrWhiteSpace(cuentaEmpresa) || cuentaEmpresa.Length < 8)
                        return DbHelper.CreateErrorResponse<object>("Cuenta empresa inválida o no configurada.");

                    var deb = new StringBuilder(160);
                    deb.Append('2');
                    deb.Append(cuentaEmpresa.Substring(0, 3));
                    deb.Append("10001");
                    deb.Append(cuentaEmpresa.Substring(cuentaEmpresa.Length - 7));
                    deb.Append((i + 1).ToString("D8", CultureInfo.InvariantCulture));

                    string strMontoEmpresa = montoDetalles.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                    deb.Append(strMontoEmpresa);
                    deb.Append(concepto);
                    deb.Append("00");
                    sb.AppendLine(deb.ToString());

                    // sumar cuenta empresa a control
                    sumaCuentas += long.Parse(cuentaEmpresa.Substring(cuentaEmpresa.Length - 7, 6), CultureInfo.InvariantCulture);

                    // Registro control
                    var linea4 = new StringBuilder(200);
                    linea4.Append('4');

                    decimal montoControl = montoPlanilla + montoDetalles;
                    string strMontoControl = montoControl
                        .ToString("0000000000000.00", CultureInfo.InvariantCulture)
                        .Replace(".", "");

                    linea4.Append(strMontoControl);
                    linea4.Append(sumaCuentas.ToString("D10", CultureInfo.InvariantCulture));
                    linea4.Append("0000000000");
                    linea4.Append(zero12Append);
                    linea4.Append(zero12Append);
                    linea4.Append("00000000");
                    sb.AppendLine(linea4.ToString());

                    return ArchivoResponse(bancoConsec, "ENV", sb);
                }
                catch (Exception ex)
                {
                    return DbHelper.CreateErrorResponse<object>(ex.Message);
                }
            }

        public static ErrorDto<object> SbTeBancoPopularCore(
                int codEmpresa,
                int bancoId,
                string tipoDoc,
                List<TesTransaccionDto> transaccionesList,
                Func<long> resolveConsecutivo
            )
        {
            DateTime fecha = DateTime.Now;

            try
            {
                long bancoConsec = resolveConsecutivo();
                var sb = new StringBuilder();

                foreach (var item in transaccionesList)
                {
                    string codigoTrim = item.codigo?.Trim() ?? string.Empty;

                    string codigo10 = codigoTrim.Length switch
                    {
                        8 => "0" + codigoTrim.Substring(0, 1) + "0" + codigoTrim.Substring(1, 7),
                        9 => "0" + codigoTrim,
                        < 8 => Convert.ToInt64(
                                    string.IsNullOrWhiteSpace(codigoTrim) ? "0" : codigoTrim,
                                    CultureInfo.InvariantCulture
                                ).ToString("D10", CultureInfo.InvariantCulture),
                        > 10 when codigoTrim.Length >= 10 => codigoTrim.Substring(0, 4) + "0" + codigoTrim.Substring(5, 5),
                        _ => codigoTrim.PadLeft(10, '0').Substring(0, 10)
                    };

                    string nombre = (item.beneficiario ?? string.Empty).Trim();
                    nombre = nombre.Length > 30 ? nombre.Substring(0, 30) : nombre.PadRight(30, ' ');

                    string cuenta = (item.cta_ahorros ?? "0").Trim();
                    cuenta = cuenta.Length > 13 ? cuenta.Substring(0, 13) : cuenta.PadLeft(13, '0');

                    decimal monto = item.monto ?? 0m;
                    string strMonto = monto.ToString("000000000.00", CultureInfo.InvariantCulture).Replace(".", "");

                    string strFecha = fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture);

                    var line = new StringBuilder(140);
                    line.Append(codigo10);
                    line.Append(nombre);
                    line.Append(cuenta);
                    line.Append(' ');
                    line.Append(strMonto);
                    line.Append(strFecha);
                    line.Append('A');
                    line.Append("06");
                    line.Append('P');
                    line.Append(strFecha);
                    line.Append(strMonto);

                    sb.AppendLine(line.ToString());
                }

                return ArchivoResponse(bancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }


        public ErrorDto<object> SbTeBcrCore(
             FormatoBcrRequest request
        )
                {
               
                    DateTime fecha = DateTime.Now;

                    try
                    {
                        // 🔒 Siempre desde parámetros (como planilla)
                        string vRazon = GetParametro(request.codEmpresa, "BCRFormat3").PadRight(30, ' ');
                        string vNumNegocio = GetParametro(request.codEmpresa, "BCRFormat1");
                        string vCedulaReg = GetParametro(request.codEmpresa, "BCRFormat2");

                        // Consecutivo diario del archivo
                        int i = request.resolveConsecutivoArchivoDelDia(request.conn, request.bancoId, fecha);
                        string vConArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                        // Cuenta banco
                        const string qCuenta = "select Cta from Tes_Bancos where id_Banco = @banco";
                        string vCuentaBancoRaw = request.conn.QueryFirstOrDefault<string>(qCuenta, new { banco = request.bancoId }) ?? "0";

                        if (!int.TryParse(vCuentaBancoRaw, out var cuentaN))
                            cuentaN = 0;

                        string vCuentaBanco = "001" + cuentaN.ToString("D8", CultureInfo.InvariantCulture);

                        // TestKey complementario
                        const string qTestKey = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                        int xTestKey = request.conn.QueryFirstOrDefault<int>(
                            qTestKey,
                            new { cuentaBanco = vCuentaBanco, montoTotal = request.vMontoTotal });

                         request.vTestKey = Math.Min(request.vTestKey + xTestKey, 2147483468);

                        string vTesKeyCh = request.vTestKey.ToString(CultureInfo.InvariantCulture).Trim();
                        if (vTesKeyCh.Length! > 12)
                              request.vTestKey = long.Parse(vTesKeyCh[^12..], CultureInfo.InvariantCulture);

                        // Consecutivo documento (calculado o provisto)
                        long bancoConsec = request.resolveBancoConsec();

                        var sb = new StringBuilder();

                        // Header
                        var header = new StringBuilder(220);
                        header.Append("000");
                        header.Append(vNumNegocio);
                        header.Append(vConArchivo);
                        header.Append(zero6Append);
                        header.Append(vCedulaReg);
                        header.Append(request.vTestKey.ToString("D12", CultureInfo.InvariantCulture));
                        header.Append(zero6Append);
                        header.Append(fecha.Day.ToString("D2", CultureInfo.InvariantCulture));
                        header.Append(fecha.Month.ToString("D2", CultureInfo.InvariantCulture));
                        header.Append(fecha.Year.ToString("D4", CultureInfo.InvariantCulture));
                        header.Append(new string(' ', 21));
                        header.Append('Y');
                        sb.AppendLine(header.ToString());

                        // Débito
                        int lineaIndex = 1;

                        var debito = new StringBuilder(220);
                        debito.Append("000");
                        debito.Append('1');
                        debito.Append("00000");
                        debito.Append(vCuentaBanco.PadRight(11).Substring(0, 11));
                        debito.Append('1');
                        debito.Append('4');
                        debito.Append("0000");
                        debito.Append(bancoConsec.ToString("D4", CultureInfo.InvariantCulture));
                        debito.Append(lineaIndex.ToString("D4", CultureInfo.InvariantCulture));
                        debito.Append(((long)(request.vMontoTotal * 100m)).ToString("D12", CultureInfo.InvariantCulture));
                        debito.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                        debito.Append('0');
                        debito.Append(vRazon);
                        sb.AppendLine(debito.ToString());

                        // Créditos
                        foreach (var item in request.transaccionesList)
                        {
                            lineaIndex++;

                            string cuenta = (item.cta_ahorros ?? "")
                                .PadRight(11)
                                .Substring(0, 11)
                                .Trim();

                            long montoCents = (long)Math.Round(
                                (item.monto ?? 0m) * 100m,
                                0,
                                MidpointRounding.AwayFromZero);

                            var credito = new StringBuilder(220);
                            credito.Append("000");
                            credito.Append('2');
                            credito.Append("00000");
                            credito.Append(cuenta);
                            credito.Append('1');
                            credito.Append('2');
                            credito.Append("0000");
                            credito.Append(bancoConsec.ToString("D4", CultureInfo.InvariantCulture));
                            credito.Append(lineaIndex.ToString("D4", CultureInfo.InvariantCulture));
                            credito.Append(montoCents.ToString("D12", CultureInfo.InvariantCulture));
                            credito.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                            credito.Append('0');
                            credito.Append(vRazon);

                            sb.AppendLine(credito.ToString());
                        }

                        return ArchivoResponse(bancoConsec, "BCR", sb);
                    }
                    catch (Exception ex)
                    {
                        return DbHelper.CreateErrorResponse<object>(ex.Message);
                    }
        }
        
        
        
        
        
        
        
        #endregion

    }
}