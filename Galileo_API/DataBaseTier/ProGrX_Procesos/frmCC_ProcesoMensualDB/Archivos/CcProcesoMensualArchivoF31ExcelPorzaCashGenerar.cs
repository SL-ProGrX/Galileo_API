using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF31ExcelPorzaCashGenerar :   CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoF31ExcelPorzaCashGenerar.CcProcesoMensualArchivoF31RegistroDbModel>
    {
        private List<string> _movimientos = [];

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["31"];

        protected override string CodigoPlanillaEnvio => "31";
        protected override string CodigoFormato => "F31";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Tipo,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                S.nombre AS Nombre,
                I.Descripcion AS InstDesc,
                ISNULL(S.CedulaR, S.cedula) AS IdAlterno
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            INNER JOIN instituciones I
                ON S.cod_institucion = I.cod_institucion
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";

        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            _movimientos = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerMovimientosPorComparador(
                configuracion);

            return base.GenerarArchivo(connection, request);
        }

        protected override object CrearParametrosRegistros(
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
                FechaProceso = request.FechaProceso,
                Movimientos = _movimientos,
                CodInstitucion = request.CodInstitucion
            };
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF31RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return registro.Cedula.Trim()
                + ";"
                + ReemplazarPuntoYComa(registro.Nombre)
                + ";"
                + registro.Tipo
                + ";"
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture)
                + ";"
                + registro.Movimiento
                + ";"
                + registro.InstDesc
                + ";"
                + registro.IdAlterno.Trim();
        }

        private static string ReemplazarPuntoYComa(string? valor)
        {
            return (valor ?? string.Empty).Replace(";", " ");
        }

        public sealed class CcProcesoMensualArchivoF31RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string InstDesc { get; set; } = string.Empty;
            public string IdAlterno { get; set; } = string.Empty;
        }
    }


}
 
