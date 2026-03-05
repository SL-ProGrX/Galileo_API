using System.Globalization;
using Dapper;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier
{
    public class MSeguimientoDB
    {
        private readonly PortalDB _portalDB;

        public MSeguimientoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public sealed class SgtOpcionDto
        {
            public string code { get; set; } = string.Empty;
            public string label { get; set; } = string.Empty;
        }

        public List<SgtOpcionDto> SGT_Opciones_Obtener()
        {
            return new List<SgtOpcionDto>
            {
                new() { code = "CRD", label = "Refunde Créditos" },
                new() { code = "DES", label = "Desembolsos y Rebajos" },
                new() { code = "RET", label = "Refunde Retenciones" },
                new() { code = "FIR", label = "Registro de Firmas" },
                new() { code = "REQ", label = "Requisitos" },
                new() { code = "CAR", label = "Cargos Adicionales" },
                new() { code = "FIA", label = "Registro Fiadores" },
                new() { code = "ILI", label = "Impacto Liquidez" },
                new() { code = "RSM", label = "Resumen" }
            };
        }
        public DateTime fxFechaCalculo(int codEmpresa, string? pLinea = "", decimal pPriDeduc = 0, int pDiaPago = 32)
        {
            const string sql = "select dbo.fxCrdFormalizaIntCorte(@linea, @priDeduc, @diaPago) as 'Result'";
            var parametros = new
            {
                linea = (pLinea ?? string.Empty).Trim(),
                priDeduc = pPriDeduc,
                diaPago = pDiaPago
            };
            return DbHelper.ExecuteSingleQuery<DateTime>(
                _portalDB,
                codEmpresa,
                sql,
                DateTime.MinValue,
                parametros
            ).Result;
        }

        public decimal fxPrimerDeduccion(int codEmpresa, string? pCodigo = "", long pDeductora = 0, decimal pPriDeduc = 0, int pDiaPago = 32)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            conn.Open();

            int iMes;
            int lngAnio;
            DateTime vFecha;

            int pQuincena = 0;

            if (pDeductora > 0)
            {
                const string sqlFecha = "select dbo.fxCrd_Primer_Deduccion(@Deductora) as FechaCorte;";
                vFecha = conn.QueryFirstOrDefault<DateTime>(sqlFecha, new { Deductora = pDeductora });
                const string sqlFrecuencia = @"
                    select isnull(frecuencia,'M') as FrecuenciaId
                    from instituciones
                    where cod_institucion = @Deductora;";
                var frecuenciaId = (conn.QueryFirstOrDefault<string>(sqlFrecuencia, new { Deductora = pDeductora }) ?? "M")
                    .Trim().ToUpperInvariant();

                if (frecuenciaId == "Q")
                    pQuincena = 1;

                iMes = vFecha.Month;
                lngAnio = vFecha.Year;
                if (pQuincena == 1)
                    pQuincena = (vFecha.Day == 15) ? 1 : 2;

                return BuildPrimerDeduccionDecimal(lngAnio, iMes, pQuincena);
            }
            vFecha = fxFechaCalculo(codEmpresa, pCodigo ?? string.Empty, pPriDeduc, pDiaPago);

            iMes = vFecha.Month;
            lngAnio = vFecha.Year;
            if (iMes == 12)
            {
                iMes = 1;
                lngAnio++;
            }
            else
            {
                iMes++;
            }
            return BuildPrimerDeduccionDecimal(lngAnio, iMes, pQuincena);
        }

        private static decimal BuildPrimerDeduccionDecimal(int anio, int mes, int quincena)
        {
            var s = $"{anio}{mes:00}.{quincena}";
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                return val;

            return 0m;
        }

        public long fxPrimerDeduccionCuota(int codEmpresa, DateTime? vFechaI = null)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            conn.Open();
            var fechaServidor = conn.QueryFirst<DateTime>("select dbo.MyGetdate();");
            var baseDate = (vFechaI ?? fechaServidor).AddMonths(1);
            var primerDiaMes = new DateTime(baseDate.Year, baseDate.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var vFecha = primerDiaMes.AddDays(-1);

            int iMes = vFecha.Month;
            int lngAnio = vFecha.Year;
            for (int x = 1; x <= 2; x++)
            {
                if (iMes == 12)
                {
                    iMes = 1;
                    lngAnio++;
                }
                else
                {
                    iMes++;
                }
            }

            var s = $"{lngAnio}{iMes:00}";
            if (long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var outVal))
                return outVal;

            return 0;
        }
    }
}