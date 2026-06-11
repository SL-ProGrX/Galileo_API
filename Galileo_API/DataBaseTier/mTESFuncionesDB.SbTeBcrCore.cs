using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class MTesFuncionesDb
    {
        public ErrorDto<object> SbTeBcrCore(FormatoBcrRequest request)
        {
            DateTime fecha = DateTime.Now;

            try
            {
                string vRazon = GetParametro(request.codEmpresa, "14");
                vRazon = vRazon.Length > 30 ? vRazon.Substring(0, 30) : vRazon.PadRight(30, ' ');
                string vNumNegocio = GetParametro(request.codEmpresa, "15");
                string vCedulaReg = GetParametro(request.codEmpresa, "13");

                int i = request.resolveConsecutivoArchivoDelDia(request.conn, request.bancoId, fecha);
                string vConArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                const string qCuenta = "select Cta from Tes_Bancos where id_Banco = @banco";
                string vCuentaBancoRaw = request.conn.QueryFirstOrDefault<string>(qCuenta, new { banco = request.bancoId }) ?? "0";
                string cuentaNormalizada = vCuentaBancoRaw.Replace("-", "").Trim();

                if (!long.TryParse(cuentaNormalizada, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cuentaN))
                    cuentaN = 0;

                string vCuentaBanco = "001" + cuentaN.ToString("D8", CultureInfo.InvariantCulture);

                const string qTestKey = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                int xTestKey = request.conn.QueryFirstOrDefault<int>(
                    qTestKey,
                    new { cuentaBanco = vCuentaBanco, montoTotal = request.vMontoTotal });

                request.vTestKey = Math.Min(request.vTestKey + xTestKey, 2147483468);

                string vTesKeyCh = request.vTestKey.ToString(CultureInfo.InvariantCulture).Trim();
                if (vTesKeyCh.Length > 12)
                    request.vTestKey = long.Parse(vTesKeyCh[^12..], CultureInfo.InvariantCulture);

                long bancoConsec = request.resolveBancoConsec;

                var sb = new StringBuilder();

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

                foreach (var item in request.transaccionesList)
                {
                    lineaIndex++;

                    string cuenta = (item.cta_ahorros ?? string.Empty).Trim();
                    cuenta = cuenta.Length >= 11
                        ? cuenta.Substring(0, 11)
                        : cuenta.PadRight(11, ' ');

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
    }
}
