using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Resultado del cálculo de cargas sociales del preanálisis (VB6: sbCalcula_Cargas,
        /// frmPreaEstudiov2.frm líneas 10283-10378). Fase 6 del diccionario de campos
        /// (frmPreaEstudiov2_diccionario_campos.md): cierra los últimos 5 parámetros de
        /// spCrdPreaPreanalisisModifica (TOTAL_CARGA_CCSS, CARGA_CCSS, CARGA_ASOCIACION,
        /// CARGA_FRAP, CARGA_IMPUESTO_SALARIO), que no son controles sino valores calculados.
        /// </summary>
        private sealed class CargasResult
        {
            public decimal CargaCcss { get; set; }
            public decimal CargaAsociacion { get; set; }
            public decimal CargaFrap { get; set; }
            public decimal CargaImpuestoSalario { get; set; }
            public decimal TotalCargaCcss { get; set; }
        }

        /// <summary>
        /// Lee los porcentajes globales de CRD_PREA_PARAMETROS usados por sbCalcula_Cargas
        /// (VB6: mPreAnalisis.bas, sbInicializaGlobales, líneas 540-572):
        ///   '07' -&gt; GlobalPorcCCSS, '08' -&gt; GlobalPorcAsocSolidarista, '09' -&gt; GlobalPorcFRAPFAP.
        /// Mismo patrón que ObtenerParametrosSalario (códigos '17'/'22') ya usado en este módulo.
        /// </summary>
        private static (decimal porcCcss, decimal porcAsociacion, decimal porcFrapFap) ObtenerParametrosCargas(IDbConnection connection)
        {
            try
            {
                var parametros = connection.Query(
                    "SELECT COD_PARAMETRO, VALOR FROM CRD_PREA_PARAMETROS WHERE COD_PARAMETRO IN ('07', '08', '09')"
                );

                decimal porcCcss = 0m;
                decimal porcAsociacion = 0m;
                decimal porcFrapFap = 0m;

                foreach (var p in parametros)
                {
                    var dict = new Dictionary<string, object>((IDictionary<string, object>)p, StringComparer.OrdinalIgnoreCase);
                    var codParametro = GetString(dict, "COD_PARAMETRO");
                    var valor = GetDecimal(dict, "VALOR");

                    switch (codParametro)
                    {
                        case "07": porcCcss = valor; break;
                        case "08": porcAsociacion = valor; break;
                        case "09": porcFrapFap = valor; break;
                    }
                }

                return (porcCcss, porcAsociacion, porcFrapFap);
            }
            catch
            {
                // No bloqueante, igual que el resto de lecturas de parámetros de este módulo.
                return (0m, 0m, 0m);
            }
        }

        /// <summary>
        /// VB6: fxRentaCalculo (mPreAnalisis.bas líneas 247-265) — SELECT dbo.fxCRDPreaCalculaRenta(salario),
        /// redondeado a 2 decimales. La lógica de tramos de renta vive en la función SQL, no en VB6 ni aquí.
        /// </summary>
        private static decimal CalcularRentaCargas(IDbConnection connection, decimal salario)
        {
            try
            {
                const string sql = "SELECT dbo.fxCRDPreaCalculaRenta(@Salario) AS Resultado";
                var row = connection.QueryFirstOrDefault(sql, new { Salario = salario }) as IDictionary<string, object>;
                if (row is null)
                {
                    return 0m;
                }
                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return Math.Round(GetDecimal(dict, "Resultado"), 2);
            }
            catch
            {
                return 0m;
            }
        }

        /// <summary>
        /// VB6: sbCalcula_Cargas (frmPreaEstudiov2.frm líneas 10283-10378). Fórmulas confirmadas
        /// verbatim contra el .frm (no adivinadas):
        ///   CARGA_CCSS = DevengadoMes * PorcCCSS / 100 (incondicional, si PorcCCSS es numérico).
        ///   CARGA_ASOCIACION = aplicaCargaAsociacion ? DevengadoMes * PorcAsociacion / 100 : 0.
        ///   CARGA_FRAP = aplicaCargaFrap ? DevengadoMes * (FrapPorcAdicional + PorcFrapFap) / 100 : 0.
        ///     FrapPorcAdicional = txtFrapPorc.Text en VB6 (puntos extra sobre el % global).
        ///   CARGA_IMPUESTO_SALARIO = DevengadoMes &gt; 0 ? fxRentaCalculo(DevengadoMes) : 0.
        ///   TOTAL_CARGA_CCSS = (CARGA_ASOCIACION si aplica) + (CARGA_FRAP si aplica)
        ///                      + CARGA_IMPUESTO_SALARIO + CARGA_CCSS (líneas 10343-10360; NO es
        ///                      una suma plana de las 4 — Asociación/FRAP están condicionadas).
        /// </summary>
        private static CargasResult CalcularCargas(
            IDbConnection connection,
            decimal devengadoMes,
            bool aplicaCargaAsociacion,
            bool aplicaCargaFrap,
            decimal frapPorcAdicional)
        {
            var (porcCcss, porcAsociacion, porcFrapFap) = ObtenerParametrosCargas(connection);
            var frapPorcTotal = NormalizarFrapPorc(frapPorcAdicional) + porcFrapFap;

            var result = new CargasResult
            {
                CargaCcss = devengadoMes * porcCcss / 100m,
                CargaAsociacion = aplicaCargaAsociacion ? devengadoMes * porcAsociacion / 100m : 0m,
                CargaFrap = aplicaCargaFrap ? devengadoMes * frapPorcTotal / 100m : 0m,
            };
            result.CargaImpuestoSalario = devengadoMes > 0m ? CalcularRentaCargas(connection, devengadoMes) : 0m;

            result.TotalCargaCcss = (aplicaCargaAsociacion ? result.CargaAsociacion : 0m)
                + (aplicaCargaFrap ? result.CargaFrap : 0m)
                + result.CargaImpuestoSalario
                + result.CargaCcss;

            return result;
        }

        /// <summary>
        /// VB6: txtCedula_LostFocus -> gPreAnalisis.Socio = socios.EstadoActual; el formulario
        /// solo lo trata como asociado cuando vale "S" (sbEstructuraActualiza).
        /// </summary>
        private static bool ObtenerEsSocio(IDbConnection connection, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return false;
            }

            try
            {
                var estado = connection.QueryFirstOrDefault<string>(
                    "SELECT EstadoActual FROM socios WHERE cedula = @Cedula",
                    new { Cedula = cedula.Trim() });
                return string.Equals(estado?.Trim(), "S", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// VB6: sbCalcula_Cargas. Calcula las cargas con el salario devengado del mes y los
        /// checks actuales de pantalla, sin persistir. Cada componente se redondea a 2
        /// decimales como Format(..., "Standard") y el total suma solo lo que aplica.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CargasResponse> Prea_frmPreaEstudiov2_Cargas_Calcular(
            int codEmpresa,
            decimal salarioMensual,
            bool aplicaAsociacion,
            bool aplicaFrap,
            decimal ptsExtraFrap)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CargasResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CargasResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var cargas = CalcularCargas(connection, salarioMensual, aplicaAsociacion, aplicaFrap, ptsExtraFrap);

                var ccss = Math.Round(cargas.CargaCcss, 2);
                var asociacion = Math.Round(cargas.CargaAsociacion, 2);
                var frap = Math.Round(cargas.CargaFrap, 2);
                var renta = Math.Round(cargas.CargaImpuestoSalario, 2);

                result.Result = new FrmPreaEstudiov2CargasResponse
                {
                    carga_ccss = ccss,
                    carga_asociacion = asociacion,
                    carga_frap = frap,
                    carga_impuesto_salario = renta,
                    total_cargas = (aplicaAsociacion ? asociacion : 0m) + (aplicaFrap ? frap : 0m) + renta + ccss
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// VB6: txtFrapPorc_Change fuerza el porcentaje extra de FRAP al rango 0..10.
        /// </summary>
        private static decimal NormalizarFrapPorc(decimal value)
        {
            if (value < 0m)
            {
                return 0m;
            }

            if (value > 10m)
            {
                return 10m;
            }

            return value;
        }

        /// <summary>
        /// VB6 (sbLigarDatos, frmPreaEstudiov2.frm líneas 10865-10888): al cargar un expediente
        /// existente, chkCargaAsociacion/chkCargaFrap quedan marcados si el valor ya guardado de
        /// CARGA_ASOCIACION/CARGA_FRAP es mayor a 0 — no hay checkbox propio migrado en Angular,
        /// así que se replica esa misma regla leyendo el valor actualmente guardado en BD antes
        /// de recalcular (GuardarModifica siempre opera sobre un expediente YA existente, así que
        /// esta es exactamente la situación de "registro cargado" del VB6, no la de "nuevo").
        /// </summary>
        private static (bool aplicaCargaAsociacion, bool aplicaCargaFrap) ObtenerAplicaCargas(IDbConnection connection, string codPreanalisis)
        {
            try
            {
                var row = connection.QueryFirstOrDefault(
                    "SELECT CARGA_ASOCIACION, CARGA_FRAP FROM CRD_PREA_PREANALISIS WHERE COD_PREANALISIS = @Expediente",
                    new { Expediente = codPreanalisis }
                ) as IDictionary<string, object>;

                if (row is null)
                {
                    return (false, false);
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return (GetDecimal(dict, "CARGA_ASOCIACION") > 0m, GetDecimal(dict, "CARGA_FRAP") > 0m);
            }
            catch
            {
                return (false, false);
            }
        }
    }
}
