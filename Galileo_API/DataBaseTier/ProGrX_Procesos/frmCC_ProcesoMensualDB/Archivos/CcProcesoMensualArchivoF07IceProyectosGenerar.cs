using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF07IceProyectosGenerar : CcProcesoMensualArchivoCreditoCondicionalGeneratorBase

    {
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["07"];

        protected override string CodigoPlanillaEnvio => "07";
        protected override string CodigoFormato => "F07";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override IEnumerable<CcProcesoMensualArchivoRegistroDbModel> FiltrarRegistros(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros)
        { 
            return registros.Where(registro => ObtenerTipoMovimiento(registro.Movimiento) == 2);
        }
        protected override string CrearLineaArchivo(
               CcProcesoMensualArchivoRegistroDbModel registro,
               CcProcesoMensualGeneraArchivoRequest request)
        {
            if (Connection is null)
            {
                return string.Empty;
            }

            var nombre = Helpers.CcProcesoMensualArchivoRutaHelperDb.SepararNombre(
                registro.Nombre);

            var credito = ObtenerDatosCredito(
                Connection,
                registro.Cedula,
                request.FechaProceso);

            var monto = credito.Cantidad > 0
                ? credito.Monto
                : registro.MontoActual;

            var plazo = credito.Cantidad > 0
                ? credito.Plazo
                : 1m;

            return registro.Cedula.Trim()
                + "\t"
                + nombre.Apellido1
                + "\t"
                + nombre.Apellido2
                + "\t"
                + nombre.Nombre1
                + " "
                + nombre.Nombre2
                + "\t"
                + monto.ToString(CultureInfo.InvariantCulture)
                + "\t"
                + plazo.ToString(CultureInfo.InvariantCulture)
                + "\t"
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture);
        }

        private static CcProcesoMensualArchivoF07CreditoDbModel ObtenerDatosCredito(
            IDbConnection connection,
            string cedula,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    COUNT(*) AS Cantidad,
                    ISNULL(SUM(montoapr), 0) AS Monto,
                    ISNULL(SUM(Saldo), 0) AS Saldo,
                    ISNULL(AVG(plazo), 1) AS Plazo
                FROM reg_creditos
                WHERE prideduc <= @FechaProceso
                  AND estado = 'A'
                  AND cedula = @Cedula";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF07CreditoDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    Cedula = cedula
                }) ?? new CcProcesoMensualArchivoF07CreditoDbModel
                {
                    Cantidad = 0,
                    Monto = 0,
                    Plazo = 1
                };
        }

        private sealed class CcProcesoMensualArchivoF07CreditoDbModel
        {
            public int Cantidad { get; set; }
            public decimal Monto { get; set; }
            public decimal Saldo { get; set; }
            public decimal Plazo { get; set; }
        }
    }
}
