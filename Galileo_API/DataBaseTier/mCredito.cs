namespace Galileo.DataBaseTier
{
    public static class MCredito
    {

        public static string fxMembresia(DateTime vFecha)
        {
            DateTime fechaServidor = DateTime.Now;

            // Diferencia en días entre vFecha y la "fecha del servidor"
            int iDias = (int)(fechaServidor.Date - vFecha.Date).TotalDays;

            int iAnio = 0;
            int iMes = 0;
            string vResultado = string.Empty;

            // Misma lógica que en VB: restar 365 y 30 sucesivamente
            while (iDias > 365)
            {
                iAnio++;
                iDias -= 365;
            }

            while (iDias > 30)
            {
                iMes++;
                iDias -= 30;
            }

            if (iAnio > 0)
                vResultado += $"{iAnio} año(s)";

            if (iMes > 0)
            {
                if (vResultado.Length > 0) vResultado += ", ";
                vResultado += $"{iMes} mes(es)";
            }

            if (iDias > 0)
            {
                if (vResultado.Length > 0) vResultado += " con ";
                vResultado += $"{iDias} dia(s) ";
            }

            return vResultado;

        }

    }
}
