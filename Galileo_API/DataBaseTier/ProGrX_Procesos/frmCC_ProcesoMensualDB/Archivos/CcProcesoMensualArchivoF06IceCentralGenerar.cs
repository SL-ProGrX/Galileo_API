using Dapper;
using System.Data;
using System.Globalization; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF06IceCentralGenerar : CcProcesoMensualArchivoCreditoCondicionalGeneratorBase
    {
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["06"];

        protected override string CodigoPlanillaEnvio => "06";
        protected override string CodigoFormato => "F06";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        public CcProcesoMensualArchivoF06IceCentralGenerar( IOptions<ArchivosGeneradosOptions> archivosOptions)
        : base(archivosOptions)
        {
        }

        protected override IEnumerable<CcProcesoMensualArchivoRegistroDbModel> FiltrarRegistros(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros)
        {
            return registros.Where(registro => ObtenerTipoMovimiento(registro.Movimiento) != 4);
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            if (Connection is null)
            {
                return string.Empty;
            }

            var tipoMovimiento = ObtenerTipoMovimiento(registro.Movimiento);

            var cedula = Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                registro.Cedula,
                "I",
                "0",
                9);

            if (tipoMovimiento == 1)
            {
                return Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    cedula.Trim(),
                    "D",
                    "0",
                    30);
            }

            var montoTotal = ObtenerMontoTotalCredito(
                Connection,
                registro.Cedula,
                request.FechaProceso);

            var montoMensual = Convert.ToInt64(registro.MontoActual * 100);

            return cedula
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    montoTotal.ToString(CultureInfo.InvariantCulture),
                    "I",
                    "0",
                    11)
                + Helpers.CcProcesoMensualArchivoRutaHelperDb.FxStringRelleno(
                    montoMensual.ToString(CultureInfo.InvariantCulture),
                    "I",
                    "0",
                    10);
        }

        private static long ObtenerMontoTotalCredito(
            IDbConnection connection,
            string cedula,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    ISNULL(SUM(montoapr), 0) AS Monto
                FROM reg_creditos
                WHERE prideduc <= @FechaProceso
                  AND estado = 'A'
                  AND cedula = @Cedula";

            var monto = connection.QueryFirstOrDefault<decimal>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Cedula = cedula
                });

            return Convert.ToInt64(monto * 100);
        }
    }
}
