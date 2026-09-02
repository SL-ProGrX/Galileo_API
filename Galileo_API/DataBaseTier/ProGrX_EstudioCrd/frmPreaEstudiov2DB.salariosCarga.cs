using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Las tres grillas de la pestana Salarios en un solo batch. Replican
        /// sbSalarios_Load / sbExtras_Load / sbIncapacidades_Load de VB6; el SQL de cada
        /// una es identico al que ya usaban ObtenerTablaSalarios / ObtenerExtras /
        /// ObtenerIncapacidades, solo que ahora viajan juntas.
        /// </summary>
        private const string SqlDetalleSalarios = @"
            EXEC spCrdPreaTraeSalariosExpediente @CodPreanalisis;

            EXEC spCRDPreaDETALLE_EXTRAS_TxExpediente @CodPreanalisis;

            SELECT COD_PREANALISIS, DIAS, DESDE, HASTA, ORDEN
            FROM CRD_PREA_V2_INCAPACIDADES
            WHERE COD_PREANALISIS = @CodPreanalisis
            ORDER BY ORDEN;";

        /// <summary>
        /// Arma el bloque de Salarios a partir del recordset plano de
        /// spCRDPreaPREANALISIS_T mas el detalle de las tres grillas.
        /// </summary>
        private static FrmPreaEstudiov2SalariosDto ConstruirSalarios(
            IDbConnection connection,
            IDictionary<string, object> row,
            string codPreanalisis)
        {
            var extrasFijas = GetDecimal(row, "EXTRAS_FIJAS");
            var porcComponenteAdicional = GetDecimal(row, "PORCENTAJE_COMPONENTE_AD");
            var detalle = ObtenerDetalleSalarios(connection, codPreanalisis);

            return new FrmPreaEstudiov2SalariosDto
            {
                tipo_salario = GetString(row, "tipo_salario"),
                corte_colilla = GetDateTime(row, "FECHA_CORTE_COLIILA"), // nombre real en VB6 (con typo)
                salario_devengado = GetDecimal(row, "SALARIO_DEVENGADO_COLILLA"),
                salario_mensual = GetDecimal(row, "DEVENGADO_MES"),
                salario_constancia = GetDecimal(row, "SALARIO_CONSTANCIA"),
                salario_orden_patronal = GetDecimal(row, "SALARIO_ORDEN_PATRONAL"),
                ingreso_privado = GetDecimal(row, "MONTO_ACT_PRIVADAS"),
                componente_adicional_id = GetInt(row, "ID_COMPONENTE_AD"),
                componente_adicional_porc = porcComponenteAdicional,
                componente_adicional_base = extrasFijas,
                // Replica el calculo cliente de VB6: txtCompAdicional = base * porc / 100
                componentes_adicionales = Math.Round(extrasFijas * porcComponenteAdicional / 100m, 2),
                total_extras = GetDecimal(row, "REBAJO_EXTRAS"),
                tabla_salarios = detalle.TablaSalarios,
                extras = detalle.Extras,
                incapacidades = detalle.Incapacidades,
            };
        }

        private sealed record DetalleSalarios(
            List<FrmPreaEstudiov2SalarioDetalleDto> TablaSalarios,
            List<FrmPreaEstudiov2ExtraDto> Extras,
            List<FrmPreaEstudiov2IncapacidadDto> Incapacidades);

        /// <summary>
        /// Un viaje a la base en lugar de tres. Si el batch falla se cae a las lecturas
        /// individuales, que degradan por separado igual que antes (una grilla vacia no
        /// bloquea a las otras).
        /// </summary>
        private static DetalleSalarios ObtenerDetalleSalarios(IDbConnection connection, string codPreanalisis)
        {
            try
            {
                using var multi = connection.QueryMultiple(
                    SqlDetalleSalarios,
                    new { CodPreanalisis = codPreanalisis });

                return new DetalleSalarios(
                    MapearTablaSalarios(multi.Read()),
                    MapearExtras(multi.Read()),
                    MapearIncapacidades(multi.Read()));
            }
            catch (Exception)
            {
                return new DetalleSalarios(
                    ObtenerTablaSalarios(connection, codPreanalisis),
                    ObtenerExtras(connection, codPreanalisis),
                    ObtenerIncapacidades(connection, codPreanalisis));
            }
        }

        /// <summary>
        /// Detalle de la pestana Salarios sin recargar el expediente completo. Sustituye a
        /// la llamada que Angular hacia a Prea_frmPreaEstudiov2_Cargar al cambiar el Tipo
        /// de Salario, que traia catalogos, credito, resumen y encabezado para descartarlos.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SalariosDto> Prea_frmPreaEstudiov2_Salarios_Consultar(
            int codEmpresa, string codPreanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2SalariosDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2SalariosDto()
            };

            var expediente = codPreanalisis?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(expediente))
            {
                result.Code = -1;
                result.Description = "Debe indicar el codigo de expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = "EXEC spCRDPreaPREANALISIS_T @CodPreanalisis";
                var rawRow = connection.QueryFirstOrDefault(sql, new { CodPreanalisis = expediente })
                    as IDictionary<string, object>;

                if (rawRow is null)
                {
                    result.Code = -1;
                    result.Description = $"No se encontro el expediente {expediente}.";
                    return result;
                }

                IDictionary<string, object> row = new Dictionary<string, object>(rawRow, StringComparer.OrdinalIgnoreCase);
                result.Result = ConstruirSalarios(connection, row, expediente);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2SalariosDto();
            }

            return result;
        }
    }
}
