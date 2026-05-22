using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class MTesFuncionesDb
    {
        public ErrorDto<object> SbTeBancoNacionalCore(
               SqlConnection conn,
               int codEmpresa,
               int bancoId,
               string tipoDoc,
               List<TesTransaccionDto> transaccionesList,
               int? curPlanilla,
               Func<long> resolveConsecutivo
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
                string empresaName = "TF " + _seguridadPortal.SeleccionarPgxClientePorCodEmpresa(codEmpresa).PGX_CORE_DB;
                string concepto = empresaName.PadRight(30, ' ');

                const string qBanco = "select Cta, codigo_Cliente from tes_Bancos Where id_Banco = @banco";
                var bancoData = conn.QueryFirstOrDefault(qBanco, new { banco = bancoId });

                if (bancoData != null)
                {
                    cuentaEmpresa = (bancoData.Cta ?? "").ToString().Trim().Replace("-", "");
                    numCliente = (bancoData.codigo_Cliente ?? "").ToString().PadLeft(6, '0');
                }

                long bancoConsec = resolveConsecutivo();

                var sb = new StringBuilder();

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

                int i = 0;
                foreach (var item in transaccionesList)
                {
                    i++;

                    string cuenta = (item.cta_ahorros ?? "").Replace("-", "").Trim();
                    if (cuenta.Length < 12)
                        return DbHelper.CreateErrorResponse<object>($"Cuenta inválida en solicitud {item.nsolicitud}.");

                    decimal monto = item.monto ?? 0m;
                    montoDetalles += monto;

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

                sumaCuentas += long.Parse(cuentaEmpresa.Substring(cuentaEmpresa.Length - 7, 6), CultureInfo.InvariantCulture);

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

    }
}
