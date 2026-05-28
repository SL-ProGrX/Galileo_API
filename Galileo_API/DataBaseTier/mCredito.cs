using Dapper;
using Galileo.Models;
using Galileo_API.DataBaseTier;
using Microsoft.Data.SqlClient;

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

        public static bool fxCrdSaldoVerifica(SqlConnection conn, long pOperacion, decimal pSaldo)
        {
            try
            {
                const string query = @"
                    select
                      case when C.poliza = 'S' or C.retencion = 'S' then 'S' else 'N' end as retencion,
                      R.saldo,
                      R.montoapr,
                      R.plazo,
                      R.amortiza
                    from reg_creditos R
                    inner join Catalogo C on R.codigo = C.codigo
                    where R.id_solicitud = @operacion;";

                var rs = conn.QueryFirstOrDefault<dynamic>(
                    query,
                    new { operacion = pOperacion }
                );

                if (rs == null)
                    return false;

                string retencion = rs.retencion;
                decimal saldo = rs.saldo;
                int plazo = rs.plazo;

                decimal curSaldo;

                if (retencion == "S")
                {
                    if (plazo > 900)
                    {
                        curSaldo = pSaldo;
                    }
                    else
                    {
                        curSaldo = saldo;
                    }
                }
                else
                {
                    curSaldo = saldo;
                }

                return curSaldo == pSaldo;
            }
            catch
            {
                return false;
            }
        }

        public static List<DropDownListaGenericaModel> SbCrdFactorCalculo()
        {
            return new List<DropDownListaGenericaModel>
            {
                new() { item = "01", descripcion = "Comercial Nivelada (30/360)" },
                new() { item = "02", descripcion = "Actual (Actual/360)" },
                new() { item = "03", descripcion = "Actual Nivelada" },
                new() { item = "04", descripcion = "Balloon Actual" },
                new() { item = "05", descripcion = "Balloon Comercial" },
                new() { item = "06", descripcion = "Nivelada Quincenal" },
                new() { item = "07", descripcion = "Interés Diario" }
            };
        }

        public static string FxCrdFactorCalculo(string pDato)
        {
            pDato = (pDato ?? string.Empty).Trim();

            return pDato switch
            {
                "Comercial Nivelada (30/360)" => "01",
                "Actual (Actual/360)" => "02",
                "Actual Nivelada" => "03",
                "Balloon Actual" => "04",
                "Balloon Comercial" => "05",
                "Nivelada Quincenal" => "06",
                "Interés Diario" => "07",

                "01" => "Comercial Nivelada (30/360)",
                "02" => "Actual (Actual/360)",
                "03" => "Actual Nivelada",
                "04" => "Balloon Actual",
                "05" => "Balloon Comercial",
                "06" => "Nivelada Quincenal",
                "07" => "Interés Diario",

                _ => "01"
            };
        }

        public static decimal FxCrdCuotaNivelada(
            decimal pSaldo,
            int pPlazo,
            decimal pTasa,
            DateTime pFechaInicio,
            double pFactorDesv = 0.01)
        {
            decimal curCuotaI = MCobroDb.fxCalcula_Cuota(pSaldo, pPlazo, pTasa, "M");
            double dbTasaDiaria = (double)pTasa / 36000d;
            pFechaInicio = new DateTime(pFechaInicio.Year, pFechaInicio.Month, 1);

            decimal[,] vMatriz = new decimal[pPlazo + 1, 7];
            int vAproximaciones = 1;
            bool vPaso;

            for (int i = 1; i <= pPlazo; i++)
            {
                vMatriz[i, 0] = i;
                vMatriz[i, 2] = DateTime.DaysInMonth(pFechaInicio.Year, pFechaInicio.Month);
                pFechaInicio = pFechaInicio.AddMonths(1);
            }

            vMatriz[1, 1] = Redondear((decimal)((double)pSaldo * dbTasaDiaria * 30d) + 1m);

            for (int i = 1; i <= pPlazo; i++)
            {
                vPaso = true;
                vMatriz[1, 6] = pSaldo;

                if (i == 1 && vMatriz[i, 2] == 31m && vAproximaciones == 1)
                {
                    vMatriz[i, 1] = Redondear((decimal)((double)vMatriz[i, 6] * dbTasaDiaria * (double)vMatriz[i, 2]));
                }

                if (i > 1)
                {
                    vMatriz[i, 1] = vMatriz[i - 1, 1];
                    vMatriz[i, 6] = vMatriz[i - 1, 5];
                }

                if (vMatriz[i, 1] > vMatriz[i, 6] || i == pPlazo)
                {
                    vMatriz[i, 1] = Redondear(vMatriz[i, 6] + (decimal)((double)vMatriz[i, 6] * dbTasaDiaria * (double)vMatriz[i, 2]));
                }

                vMatriz[i, 3] = Redondear((decimal)((double)vMatriz[i, 6] * dbTasaDiaria * (double)vMatriz[i, 2]));
                vMatriz[i, 4] = Redondear(vMatriz[i, 1] - vMatriz[i, 3]);
                vMatriz[i, 5] = Redondear(vMatriz[i, 6] - vMatriz[i, 4]);

                if (vMatriz[i, 4] < 0m && vPaso)
                {
                    vAproximaciones++;
                    vMatriz[1, 1] = Redondear(vMatriz[1, 1] + 0.5m);
                    i = 0;
                    continue;
                }

                if (vMatriz[i, 5] <= 0m && i < pPlazo && vPaso)
                {
                    vAproximaciones++;
                    vMatriz[1, 1] = Redondear(vMatriz[1, 1] - 0.5m);
                    i = 0;
                    continue;
                }

                if (vMatriz[i, 5] <= 0m && i == pPlazo && vPaso)
                {
                    if (vMatriz[i, 6] <= vMatriz[1, 1])
                    {
                        curCuotaI = vMatriz[1, 1];
                        break;
                    }

                    vAproximaciones++;
                    vMatriz[1, 1] = Redondear(vMatriz[1, 1] + 0.5m);
                    i = 0;
                    continue;
                }

                if (vAproximaciones >= 100000)
                {
                    break;
                }
            }

            return Redondear(curCuotaI);
        }

        private static decimal Redondear(decimal valor)
        {
            return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        }
    }
}
