using System.Security.Cryptography;

namespace Galileo.DataBaseTier
{
    public static class mHaciendaDB
    {
        public static int Aleatorio(int minimo, int maximoInclusive)
        {
            if (minimo > maximoInclusive) (minimo, maximoInclusive) = (maximoInclusive, minimo);

            return RandomNumberGenerator.GetInt32(minimo, maximoInclusive + 1);
        }

        public static string fxStringRelleno(string valor, string tipo, string relleno, int largo)
        {
            valor = (valor ?? "").Trim();
            var fillChar = string.IsNullOrEmpty(relleno) ? ' ' : relleno[0];

            if (largo <= 0) return "";

            if (valor.Length > largo)
                return valor.Substring(valor.Length - largo, largo);

            tipo = (tipo ?? "").Trim().ToUpperInvariant();
            return (tipo == "D")
                ? valor.PadRight(largo, fillChar)
                : valor.PadLeft(largo, fillChar);
        }

        public static string fxHacienda_Clave50(
            string codPais,
            DateTime fechaTransac,
            string idEmpresa,
            string codSucursal,
            string terminalPOS,
            string comprobanteInterno,
            string situacionComprobante,
            string tipoComprobante)
        {
            int rndClave = Aleatorio(0, 99999999);

            string yy = (fechaTransac.Year % 100).ToString("00");

            return (codPais ?? "").Trim()
                 + fechaTransac.Day.ToString("00")
                 + fechaTransac.Month.ToString("00")
                 + yy
                 + fxStringRelleno(idEmpresa, "I", "0", 12)
                 + fxStringRelleno(codSucursal, "I", "0", 3)
                 + fxStringRelleno(terminalPOS, "I", "0", 5)
                 + (tipoComprobante ?? "").Trim()
                 + fxStringRelleno(comprobanteInterno, "I", "0", 10)
                 + (situacionComprobante ?? "").Trim()
                 + fxStringRelleno(rndClave.ToString(), "D", "9", 8);
        }

        public static string fxHacienda_Clave20(
            string codSucursal,
            string terminalPOS,
            string comprobanteInterno,
            string tipoComprobante)
        {
            return fxStringRelleno(codSucursal, "I", "0", 3)
                 + fxStringRelleno(terminalPOS, "I", "0", 5)
                 + (tipoComprobante ?? "").Trim()
                 + fxStringRelleno(comprobanteInterno, "I", "0", 10);
        }
    }
}
