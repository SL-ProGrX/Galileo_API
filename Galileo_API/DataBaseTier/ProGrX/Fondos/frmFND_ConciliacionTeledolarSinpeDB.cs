using System.Security;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndConciliacionTeledolarSinpeDb
    {
        private readonly IConfiguration _config;

        public FrmFndConciliacionTeledolarSinpeDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Ejecuta el reporte de conciliación Teledolar Aseccss.
        /// </summary>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns>ErrorDto con la lista de resultados.</returns>
        public ErrorDto<List<FndConciliacionTeledolarSinpeResult>> ConciliacionTeledolarSinpe_Obtener(FndConciliacionTeledolarSinpeParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de conciliación son requeridos.",
                    -2,
                    new List<FndConciliacionTeledolarSinpeResult>());
            }

            var codEmpresa = NormalizarCodEmpresa(param.CodEmpresa);
            var connectionString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            return DbHelper.ExecuteStoredProcedureList<FndConciliacionTeledolarSinpeResult>(
                connectionString,
                "spFndReporteConciliacionTeledolarAseccss",
                CrearParametrosConciliacion(param));
        }

        /// <summary>
        /// Crea los parámetros seguros para el procedimiento de conciliación.
        /// </summary>
        /// <param name="param">Parámetros recibidos desde la capa superior.</param>
        /// <returns>Objeto anónimo con parámetros del procedimiento.</returns>
        private static object CrearParametrosConciliacion(FndConciliacionTeledolarSinpeParams param)
        {
            return new
            {
                FechaInicial = param.FechaInicio.Date,
                FechaFinal = param.FechaCorte.Date.AddDays(1).AddTicks(-1),
                SoloDiferencias = param.SoloDiferencias ? 1 : 0
            };
        }

        /// <summary>
        /// Valida el código de empresa antes de resolver la cadena de conexión.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa recibido.</param>
        /// <returns>Código de empresa validado.</returns>
        private static int NormalizarCodEmpresa(int codEmpresa)
        {
            if (codEmpresa <= 0 || codEmpresa > 999999)
            {
                throw new SecurityException("El código de empresa no es válido.");
            }

            return codEmpresa;
        }
    }
}
