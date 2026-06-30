using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF23CorreosGenerar : CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoF23CorreosGenerar.CcProcesoMensualArchivoRegistroDbModel>

    {
        private const string MovimientoExclusion = "E";

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["23"];

        protected override string CodigoPlanillaEnvio => "23";
        protected override string CodigoFormato => "F23";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                P.Tipo,
                S.nombre AS Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento <> @MovimientoExclusion
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";

        public CcProcesoMensualArchivoF23CorreosGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }
        protected override object CrearParametrosRegistros(
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
                request.FechaProceso,
                request.CodInstitucion,
                MovimientoExclusion
            };
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return registro.Cedula.Trim()
                + ","
                + ReemplazarComas(registro.Nombre)
                + ","
                + registro.MontoActual.ToString("0");
        }

        private static string ReemplazarComas(string? valor)
        {
            return (valor ?? string.Empty).Replace(",", " ");
        }

        public sealed class CcProcesoMensualArchivoRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
