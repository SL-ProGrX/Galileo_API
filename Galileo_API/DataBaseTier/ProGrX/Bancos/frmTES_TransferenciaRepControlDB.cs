using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransferenciaRepControlDB
    {
        private readonly PortalDB _portalDB;
        private readonly SeguridadPortalDb _seguridadPortal;
        private readonly MTesoreria mTesoreria;

        // Defaults (si esto debería venir de parámetros/tabla, se puede mover después)
        private const string NumNegocio = "003002185187";
        private const string CedulaReg = "000000155810";
        private const string Razon = "DEPOSITO GENERAL";
        private const string zero6Append = "000000";
        private const string zero12Append = "000000000000";
        private const string fechaFormat2 = "ddMMyyyy";

        public FrmTesTransferenciaRepControlDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _seguridadPortal = new SeguridadPortalDb(config);
            mTesoreria = new MTesoreria(config);
        }

        #region ===== Helpers (anti-duplicidad / Sonar-friendly) =====

        private static void AppendIfNotEmpty(StringBuilder sb, string? line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);
        }

        private static ErrorDto<object> OkJson(object payload)
            => DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(payload, Formatting.Indented));

        private static ErrorDto<object> Err(string msg, int code = -1)
            => DbHelper.CreateErrorResponse<object>(msg, code, default!);

        /// <summary>
        /// Respuesta estándar { bancoConsec, extension, contenido }
        /// </summary>
        private static ErrorDto<object> ArchivoResponse(long bancoConsec, string extension, StringBuilder sb)
            => OkJson(new
            {
                bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
                extension,
                contenido = sb.ToString()
            });

        /// <summary>
        /// Ejecuta 3 “líneas” (numLinea 1..3) de un Stored Procedure con parámetros base.
        /// NOTA: usa DynamicParameters para “aplanar” correctamente.
        /// </summary>
        private static IEnumerable<string> ExecSP3Lineas(SqlConnection conn, string spName, object parametrosBase)
        {
            for (int numLinea = 1; numLinea <= 3; numLinea++)
            {
                var dp = new DynamicParameters(parametrosBase);
                dp.Add("numLinea", numLinea);

                var linea = conn.QueryFirstOrDefault<string>(
                    spName,
                    dp,
                    commandType: System.Data.CommandType.StoredProcedure);

                if (!string.IsNullOrWhiteSpace(linea))
                    yield return linea;
            }
        }

      

        /// <summary>
        /// Obtiene numNegocio/cedulaReg desde SIF_EMPRESA (siempre igual en tu lógica).
        /// </summary>
        private static (string numNegocio, string cedulaReg) GetEmpresa(SqlConnection conn)
        {
            const string sql = "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre from SIF_EMPRESA";
            var empresa = conn.QueryFirstOrDefault(sql);

            string cedula = empresa?.cedula_juridica?.ToString()?.Trim() ?? string.Empty;

            return (cedula, cedula);
        }

        /// <summary>
        /// Whitelist para evitar ejecutar cualquier cosa por “procedimiento” (S2077 hardening).
        /// Ajusta esta lista a los procedimientos reales que existen para formatos estándar.
        /// </summary>
        private static bool IsFormatoProcPermitido(string procedimientoBase)
        {
            if (string.IsNullOrWhiteSpace(procedimientoBase))
                return false;

            // OJO: aquí debes meter los nombres base reales que vienen en vTes_Formatos.Procedimiento
            // Ejemplos ficticios: "spTES_DV1", "spTES_DV2"
            // Si tu vTes_Formatos guarda "spTES_DV1" entonces el SP final sería "spTES_DV1_Archivo".
            return procedimientoBase is
                "spTES_DV1" or
                "spTES_DV2" or
                "spTES_FormatoEstandar"; // agrega lo que aplique
        }

        #endregion

        #region ===== Catálogos =====

        public ErrorDto<TransferenciaRepControlCatalogoDto> TES_TransferenciaRepControl_Catalogos_Obtener(int CodEmpresa, int Banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var response = new TransferenciaRepControlCatalogoDto();

                const string query1 = "exec spTes_Formatos_Bancos @Banco";

                const string query2 = @"
select rtrim(T.tipo) as IdX, rtrim(T.descripcion) as ItmX 
from tes_banco_docs D
inner join tes_tipos_doc T on D.tipo = T.tipo 
where D.comprobante = '04' and D.id_Banco = @Banco";

                const string query3 = @"
select Bp.COD_PLAN as IdX, Bp.COD_PLAN as ItmX
from TES_BANCOS B
inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
Where B.ID_BANCO = @Banco And B.UTILIZA_PLAN = 1
order by Bp.COD_PLAN asc";

                response.Formatos = conn.Query<DropDownCatalogoBancos>(query1, new { Banco }).ToList();
                response.Tipos = conn.Query<DropDownCatalogoBancos>(query2, new { Banco }).ToList();
                response.Planes = conn.Query<DropDownCatalogoBancos>(query3, new { Banco }).ToList();

                if (response.Planes == null || response.Planes.Count == 0)
                {
                    response.Planes = new List<DropDownCatalogoBancos>
                    {
                        new DropDownCatalogoBancos { idx = "-sp-", itmx = "Sin Plan" }
                    };
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TransferenciaRepControlCatalogoDto>(ex.Message);
            }
        }

        #endregion

        #region ===== Generación Archivo =====

        public ErrorDto<object> TES_TransferenciaRepControl_Archivo_Generar(
            int CodEmpresa,
            int Banco,
            int NTransac,
            string TipoDoc,
            string Formato,
            string Plan)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string queryTransac = @"
Select *
From Tes_Transacciones
Where Estado = 'T'
  And Tipo = @TipoDoc 
  And ID_Banco= @Banco
  And Autoriza='S'
  And documento_base = @NTransac
Order by Nsolicitud";

                var parametros = new { Banco, TipoDoc, NTransac };

                // Cargamos una vez por default cuando haga falta
                List<TesTransaccionDto> LoadTransacciones()
                    => conn.Query<TesTransaccionDto>(queryTransac, parametros).ToList();

                return Formato switch
                {
                    "A" => // Banco Nacional
                        ProcesarFormatoA(CodEmpresa, Banco, TipoDoc, NTransac, conn, parametros, LoadTransacciones()),
                    "B" => // Banco Popular
                        sbTeBancoPopular(CodEmpresa, Banco, TipoDoc, NTransac, LoadTransacciones()),
                    "C" => // BCR Planilla
                        ProcesarFormatoC(CodEmpresa, Banco, TipoDoc, NTransac, conn, parametros, LoadTransacciones()),
                    "D" => sbTeBCR_Empresarial(CodEmpresa, Banco, TipoDoc, NTransac),
                    "E" => sbTeBCT_Enlace(CodEmpresa, Banco, TipoDoc, NTransac),
                    "F" => sbTeBCR_Comercial(CodEmpresa, Banco, TipoDoc, NTransac),
                    "G" => sbTeBNCR_Sinpe(CodEmpresa, Banco, TipoDoc, NTransac),
                    "DV1" or "DV2" => sbTeFormatoEstandar(CodEmpresa, Banco, TipoDoc, NTransac, Formato, Plan),
                    "S" => Err("SINPE está en espera / no implementado."),
                    _ => sbTeFormatoEstandar(CodEmpresa, Banco, TipoDoc, NTransac, Formato, Plan)
                };
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> ProcesarFormatoA(
            int CodEmpresa,
            int Banco,
            string TipoDoc,
            int NTransac,
            SqlConnection conn,
            object parametros,
            List<TesTransaccionDto> transacciones)
        {
            const string queryA = @"
Select sum(monto) as Monto
From Tes_Transacciones
Where Estado = 'T'
  And Tipo = @TipoDoc
  And ID_Banco= @Banco
  And Autoriza='S'
  And documento_base = @NTransac";

            int vMonto = conn.QueryFirstOrDefault<int?>(queryA, parametros) ?? 0;

            return sbTeBancoNacional(CodEmpresa, Banco, TipoDoc, NTransac, transacciones, vMonto);
        }

        private ErrorDto<object> ProcesarFormatoC(
            int CodEmpresa,
            int Banco,
            string TipoDoc,
            int NTransac,
            SqlConnection conn,
            object parametros,
            List<TesTransaccionDto> transacciones)
        {
            const string queryC = @"
select sum(dbo.fxTESBCRTestkey(cta_ahorros,monto)) as TestKeyX,
       sum(Monto) as Monto
From Tes_Transacciones 
Where Estado = 'T'
  And Tipo = @TipoDoc
  And ID_Banco= @Banco 
  And Autoriza='S'
  And documento_base = @NTransac";

            var resultC = conn.QueryFirstOrDefault(queryC, parametros);

            long xTestKey = 0;
            decimal totalMonto = 0;

            if (resultC != null)
            {
                long testKeyX = (long?)resultC.TestKeyX ?? 0;
                xTestKey = testKeyX > 2147483468 ? 2147483468 : testKeyX;
                totalMonto = (decimal?)resultC.Monto ?? 0m;
            }

            return sbTeBCR(CodEmpresa, Banco, TipoDoc, NTransac, transacciones, xTestKey, totalMonto);
        }

        #endregion

        #region ===== Implementaciones (mismas, pero usando helpers) =====

        public ErrorDto<object> sbTeBancoNacional(
            int CodEmpresa,
            int vBanco,
            string vTipoDoc,
            int vNTransac,
            List<TesTransaccionDto> transaccionesList,
            int? curPlanilla)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int bancoId = vBanco;
            DateTime vFecha = DateTime.Now;

            decimal curMonto1 = curPlanilla ?? 0m;
            string strMonto = curMonto1.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");

            string vCuentaEmpresa = "";
            string vNumCliente = "";
            decimal curMonto2 = 0m;
            long curCuentas = 0;

            try
            {
                string empresaName = "TF " + _seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa).PGX_CORE_DB;
                string vConcepto = empresaName.PadRight(30, ' ');

                const string qBanco = "select Cta,codigo_Cliente from tes_Bancos Where id_Banco = @banco";
                var bancoData = conn.QueryFirstOrDefault(qBanco, new { banco = bancoId });

                if (bancoData != null)
                {
                    vCuentaEmpresa = (bancoData.Cta ?? "").ToString().Trim().Replace("-", "");
                    vNumCliente = (bancoData.codigo_Cliente ?? "").ToString().PadLeft(6, '0');
                }

                long bancoConsec = vNTransac;

                var sb = new StringBuilder();

                // Header
                var header = new StringBuilder(120);
                header.Append('1');
                header.Append(vNumCliente);
                header.Append(vFecha.Day.ToString("00", CultureInfo.InvariantCulture));
                header.Append(vFecha.Month.ToString("00", CultureInfo.InvariantCulture));
                header.Append(vFecha.Year.ToString("0000", CultureInfo.InvariantCulture));
                header.Append(bancoId.ToString("D12", CultureInfo.InvariantCulture));
                header.Append("10000");
                header.Append(strMonto);
                header.Append("000000000000000000000000");
                sb.AppendLine(header.ToString());

                int i = 0;

                foreach (var item in transaccionesList)
                {
                    i++;
                    string cuenta = (item.cta_ahorros ?? "").Replace("-", "").Trim();
                    if (cuenta.Length < 12)
                        return Err($"Cuenta inválida en solicitud {item.nsolicitud}.");

                    decimal monto = item.monto ?? 0m;
                    curMonto2 += monto;

                    // sumatoria cuentas (sin dígito verificador)
                    if (cuenta.Length >= 7)
                        curCuentas += long.Parse(cuenta.Substring(cuenta.Length - 7, 6), CultureInfo.InvariantCulture);

                    var linea = new StringBuilder(160);
                    linea.Append('3');
                    linea.Append(cuenta.Substring(5, 3));
                    linea.Append(cuenta.Substring(0, 3));
                    linea.Append("01");
                    linea.Append(cuenta.Substring(cuenta.Length - 7));
                    linea.Append(i.ToString("D8", CultureInfo.InvariantCulture));

                    string strMontoDet = monto.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                    linea.Append(strMontoDet);
                    linea.Append(vConcepto);
                    linea.Append("00");

                    sb.AppendLine(linea.ToString());
                }

                // Débito empresa
                if (string.IsNullOrWhiteSpace(vCuentaEmpresa) || vCuentaEmpresa.Length < 8)
                    return Err("Cuenta empresa inválida o no configurada.");

                var deb = new StringBuilder(160);
                deb.Append('2');
                deb.Append(vCuentaEmpresa.Substring(0, 3));
                deb.Append("10001");
                deb.Append(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7));
                deb.Append((i + 1).ToString("D8", CultureInfo.InvariantCulture));

                string strMontoEmpresa = curMonto2.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                deb.Append(strMontoEmpresa);
                deb.Append(vConcepto);
                deb.Append("00");
                sb.AppendLine(deb.ToString());

                curCuentas += long.Parse(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7, 6), CultureInfo.InvariantCulture);

                // Registro control
                var linea4 = new StringBuilder(200);
                linea4.Append('4');
                decimal montoControl = curMonto1 + curMonto2;
                string strMontoControl = montoControl.ToString("0000000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                linea4.Append(strMontoControl);
                linea4.Append(curCuentas.ToString("D10", CultureInfo.InvariantCulture));
                linea4.Append("0000000000");
                linea4.Append(zero12Append);
                linea4.Append(zero12Append);
                linea4.Append("00000000");
                sb.AppendLine(linea4.ToString());

                return ArchivoResponse(bancoConsec, "ENV", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        public static ErrorDto<object> sbTeBancoPopular(
            int CodEmpresa,
            int vBanco,
            string vTipoDoc,
            int vNTransac,
            List<TesTransaccionDto> transaccionesList)
        {
            DateTime vFecha = DateTime.Now;

            try
            {
                long bancoConsec = vNTransac;
                var sb = new StringBuilder();

                foreach (var item in transaccionesList)
                {
                    string codigoTrim = item.codigo?.Trim() ?? string.Empty;

                    string codigo10 = codigoTrim.Length switch
                    {
                        8 => "0" + codigoTrim.Substring(0, 1) + "0" + codigoTrim.Substring(1, 7),
                        9 => "0" + codigoTrim,
                        < 8 => Convert.ToInt64(string.IsNullOrWhiteSpace(codigoTrim) ? "0" : codigoTrim, CultureInfo.InvariantCulture)
                                    .ToString("D10", CultureInfo.InvariantCulture),
                        > 10 when codigoTrim.Length >= 10 => codigoTrim.Substring(0, 4) + "0" + codigoTrim.Substring(5, 5),
                        _ => codigoTrim.PadLeft(10, '0').Substring(0, 10)
                    };

                    string nombre = (item.beneficiario ?? string.Empty).Trim();
                    nombre = nombre.Length > 30 ? nombre.Substring(0, 30) : nombre.PadRight(30, ' ');

                    string cuenta = (item.cta_ahorros ?? "0").Trim();
                    cuenta = cuenta.Length > 13 ? cuenta.Substring(0, 13) : cuenta.PadLeft(13, '0');

                    decimal monto = item.monto ?? 0m;
                    string strMonto = monto.ToString("000000000.00", CultureInfo.InvariantCulture).Replace(".", "");

                    string strFecha = vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture);

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
                return Err(ex.Message);
            }
        }

        public ErrorDto<object> sbTeBCR(
            int CodEmpresa,
            int vBanco,
            string vTipoDoc,
            int vNTransac,
            List<TesTransaccionDto> transaccionesList,
            long vTestKey,
            decimal vMontoTotal)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            DateTime vFecha = DateTime.Now;

            try
            {
                string vRazon = Razon.PadRight(30, ' ');
                string vNumNegocio = NumNegocio;
                string vCedulaReg = CedulaReg;

                // consecutivo diario (ojo: tu query original estaba rara; aquí simplifico a COUNT)
                const string qCount = @"
select count(distinct documento_base)
from Tes_Transacciones
where id_banco = @banco and fecha_emision = @fecha and estado = 'T'";
                int i = conn.QueryFirstOrDefault<int>(qCount, new { banco = vBanco, fecha = vFecha }) + 1;
                string vConArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                // cuenta banco
                const string qCuenta = "select Cta from Tes_Bancos where id_Banco = @banco";
                string vCuentaBancoRaw = conn.QueryFirstOrDefault<string>(qCuenta, new { banco = vBanco }) ?? "0";
                _ = int.TryParse(vCuentaBancoRaw, out var cuentaN);
                string vCuentaBanco = "001" + cuentaN.ToString("D8", CultureInfo.InvariantCulture);

                // testkey complementario
                const string qTestKey = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                int xTestKey = conn.QueryFirstOrDefault<int>(qTestKey, new { cuentaBanco = vCuentaBanco, montoTotal = vMontoTotal });
                vTestKey = Math.Min(vTestKey + xTestKey, 2147483468);

                string vTesKeyCh = vTestKey.ToString(CultureInfo.InvariantCulture).Trim();
                if (vTesKeyCh.Length > 12)
                    vTestKey = long.Parse(vTesKeyCh[^12..], CultureInfo.InvariantCulture);

                long bancoConsec = vNTransac;

                var sb = new StringBuilder();

                // Encabezado
                var header = new StringBuilder(220);
                header.Append("000");
                header.Append(vNumNegocio);
                header.Append(vConArchivo);
                header.Append(zero6Append);
                header.Append(vCedulaReg);
                header.Append(Convert.ToInt64(vTestKey).ToString("D12", CultureInfo.InvariantCulture));
                header.Append(zero6Append);
                header.Append(vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                header.Append(new string(' ', 21));
                header.Append('Y');
                sb.AppendLine(header.ToString());

                // Línea 1 débito
                int lineaIndex = 1;

                var debito = new StringBuilder(220);
                debito.Append("000");
                debito.Append('1');
                debito.Append("00000");
                debito.Append(vCuentaBanco.Trim().PadRight(11).Substring(0, 11));
                debito.Append('1');
                debito.Append('4');
                debito.Append("0000");
                debito.Append(bancoConsec.ToString("D4", CultureInfo.InvariantCulture));
                debito.Append(lineaIndex.ToString("D4", CultureInfo.InvariantCulture));
                debito.Append(((long)(vMontoTotal * 100m)).ToString("D12", CultureInfo.InvariantCulture));
                debito.Append(vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                debito.Append('0');
                debito.Append(vRazon);
                sb.AppendLine(debito.ToString());

                foreach (var item in transaccionesList)
                {
                    lineaIndex++;

                    string cuenta = (item.cta_ahorros ?? string.Empty).PadRight(11).Substring(0, 11).Trim();
                    long montoCents = (long)Math.Round(((item.monto ?? 0m) * 100m), 0, MidpointRounding.AwayFromZero);

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
                    credito.Append(vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                    credito.Append('0');
                    credito.Append(vRazon);

                    sb.AppendLine(credito.ToString());
                }

                return ArchivoResponse(bancoConsec, "BCR", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCR_Empresarial(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var (numNegocio, cedulaReg) = GetEmpresa(conn);

                int bancoId = vBanco;
                string bancoTDoc = vTipoDoc;
                long bancoConsec = vNTransac;
                DateTime fecha = DateTime.Now;

                // consecutivo diario
                const string qCount = @"
select count(distinct documento_base)
from Tes_Transacciones
where id_banco = @banco and fecha_emision = @fecha and estado = 'T'";
                int i = conn.QueryFirstOrDefault<int>(qCount, new { banco = bancoId, fecha }) + 1;
                string conArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                // control
                var sb = new StringBuilder();
                var control = new StringBuilder(220);
                control.Append("000");
                control.Append((cedulaReg ?? string.Empty).Trim().PadLeft(12, '0'));
                control.Append(conArchivo);
                control.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                control.Append(zero12Append);
                control.Append(zero12Append);
                control.Append(zero6Append);
                control.Append(new string(' ', 6));
                control.Append("TLB");
                control.Append(new string(' ', 128));
                control.Append('D');
                sb.AppendLine(control.ToString());

                // líneas 2 y 3
                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(
                    "exec spTES_BCR_Empresarial_Archivo 2, @banco, @bancoTDoc, @numNegocio, @bancoConsec, 100000",
                    new { banco = bancoId, bancoTDoc, numNegocio, bancoConsec }));

                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(
                    "exec spTES_BCR_Empresarial_Archivo 3, @banco, @bancoTDoc, @numNegocio, @bancoConsec, 100000",
                    new { banco = bancoId, bancoTDoc, numNegocio, bancoConsec }));

                return ArchivoResponse(bancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCR_Comercial(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var (numNegocio, cedulaReg) = GetEmpresa(conn);

                int bancoId = vBanco;
                string bancoTDoc = vTipoDoc;
                long bancoConsec = vNTransac;
                DateTime fecha = DateTime.Now;

                // consecutivo diario
                const string qCount = @"
select count(distinct documento_base)
from Tes_Transacciones
where id_banco = @banco and fecha_emision = @fecha and estado = 'T'";
                int i = conn.QueryFirstOrDefault<int>(qCount, new { banco = bancoId, fecha }) + 1;
                string conArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                var sb = new StringBuilder();

                var control = new StringBuilder(220);
                control.Append("000");
                control.Append((cedulaReg ?? string.Empty).Trim().PadLeft(12, '0'));
                control.Append(conArchivo);
                control.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                control.Append(zero12Append);
                control.Append(zero12Append);
                control.Append(zero6Append);
                control.Append(new string('0', 138));
                sb.AppendLine(control.ToString());

                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(
                    "exec spTES_BCR_Comercial_Archivo 2, @banco, @bancoTDoc, @numNegocio, @bancoConsec, 100000",
                    new { banco = bancoId, bancoTDoc, numNegocio, bancoConsec }));

                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(
                    "exec spTES_BCR_Comercial_Archivo 3, @banco, @bancoTDoc, @numNegocio, @bancoConsec, 100000",
                    new { banco = bancoId, bancoTDoc, numNegocio, bancoConsec }));

                return ArchivoResponse(bancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCT_Enlace(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                long bancoConsec = vNTransac;
                var sb = new StringBuilder();

                const string q = @"exec spTES_BCT_Enlace_ArchivoLog @banco, @bancoTDoc, @bancoConsec";
                var r = conn.QueryFirstOrDefault(q, new { banco = vBanco, bancoTDoc = vTipoDoc, bancoConsec });

                AppendIfNotEmpty(sb, r?.Linea?.ToString());
                return ArchivoResponse(bancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBNCR_Sinpe(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                long bancoConsec = vNTransac;
                var sb = new StringBuilder();

                const string l1 = @"exec spTES_BNCR_SINPE_Archivo 1, @banco, @bancoTDoc, @bancoConsec, 0";
                const string l2 = @"exec spTES_BNCR_SINPE_Archivo 2, @banco, @bancoTDoc, @bancoConsec, 0";
                const string l3 = @"exec spTES_BNCR_SINPE_Archivo 3, @banco, @bancoTDoc, @bancoConsec, 0";

                var p = new { banco = vBanco, bancoTDoc = vTipoDoc, bancoConsec };

                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(l1, p));
                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(l2, p));
                AppendIfNotEmpty(sb, conn.QueryFirstOrDefault<string>(l3, p));

                return ArchivoResponse(bancoConsec, "tef", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        /// <summary>
        /// Formatos estándar: mueve ejecución dinámica a un SP wrapper (spTES_EjecutarFormatoArchivo),
        /// y evita concatenar "EXEC {vProcedimiento}_Archivo" en C#.
        /// </summary>
        private ErrorDto<object> sbTeFormatoEstandar(
            int CodEmpresa,
            int vBanco,
            string vTipoDoc,
            int vNTransac,
            string vFormato,
            string vPlan)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                int bancoId = vBanco;
                string pFormato = vFormato;

                var (numNegocio, _) = GetEmpresa(conn);

                const string qFormato = "select Procedimiento,Extension from vTes_Formatos where cod_formato = @formato";
                var formatoData = conn.QueryFirstOrDefault(qFormato, new { formato = pFormato });

                string vExtension = formatoData?.Extension?.ToString() ?? "txt";
                string vProcedimientoBase = formatoData?.Procedimiento?.ToString() ?? string.Empty;

                if (!IsFormatoProcPermitido(vProcedimientoBase))
                    return Err($"Procedimiento no permitido o no configurado: '{vProcedimientoBase}'");

                // OJO: aquí asumo que el wrapper SP se encarga de llamar: {vProcedimientoBase}_Archivo
                // y recibir @numLinea (1..3) + el resto de parámetros.
                const string wrapperSp = "spTES_EjecutarFormatoArchivo";

                long bancoConsec = vNTransac;
                string bancoPlan = vPlan;

                var sb = new StringBuilder();

                // param base para wrapper (aplanado)
                var parametrosBase = new
                {
                    procedimiento = vProcedimientoBase, // base, NO incluye "_Archivo" para que el SP lo construya
                    bancoID = bancoId,
                    bancoTDoc = vTipoDoc,
                    numNegocio,
                    bancoConsec,
                    bancoPlan
                };

                foreach (var linea in ExecSP3Lineas(conn, wrapperSp, parametrosBase))
                    AppendIfNotEmpty(sb, linea);

                return ArchivoResponse(bancoConsec, vExtension, sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        #endregion

        public ErrorDto<TesReporteTransferenciaDto> sbTesReporteTransferencia(int CodEmpresa, int vBanco, long vTransac, string? vTipo = "C", string? vDocumento = "TE", string? vPlan = "-sp-")
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            return mTesoreria.sbTesReporteTransferencia(conn, CodEmpresa, vBanco, vTransac, vTipo, vDocumento, vPlan);
        }
    }
}
