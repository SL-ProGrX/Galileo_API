using Dapper;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Reflection;

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

        public static string fxCrdParametro(SqlConnection conn, string pParametro)
        {
            try
            {
                var query = $@"select valor from crd_parametros where cod_parametro = @parametro ";
                var resultado = conn.QueryFirstOrDefault<string>(query, new { parametro = pParametro });
                if(string.IsNullOrEmpty(resultado))
                {
                    return "3";
                }

                return resultado;
            }
            catch (Exception)
            {
                return "3";
            }
        }

        public static int fxMesDias(int pMes, int pAnio)
        {
            return DateTime.DaysInMonth(pAnio, pMes);
        }

    }
}
