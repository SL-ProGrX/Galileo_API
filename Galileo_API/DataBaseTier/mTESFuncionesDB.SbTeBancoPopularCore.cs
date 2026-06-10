using Galileo.Models.ERROR;
using Galileo.Models.TES;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class MTesFuncionesDb
    {
        public static ErrorDto<object> SbTeBancoPopularCore(
               int codEmpresa,
               int bancoId,
               string tipoDoc,
               List<TesTransaccionDto> transaccionesList,
               long resolveConsecutivo
           )
        {
            DateTime fecha = DateTime.Now;

            try
            {
                long bancoConsec = resolveConsecutivo;

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
    }
}
