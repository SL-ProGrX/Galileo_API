
using System.Globalization; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF26JupemaGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF26JupemaGenerar.CcProcesoMensualArchivoF26RegistroDbModel>
    {
        public CcProcesoMensualArchivoF26JupemaGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }

        private const string TipoDeduccionMonto = "M"; 

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["26"];

        protected override string CodigoPlanillaEnvio => "26";
        protected override string CodigoFormato => "F26";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Porc_Deduc AS PorcDeduc,
                P.Tipo_Deduc AS TipoDeduc,
                P.Movimiento,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";

        protected override string CrearLineaArchivo(
             CcProcesoMensualArchivoF26RegistroDbModel registro,
             CcProcesoMensualGeneraArchivoRequest request)
        {
            var valor = string.Equals(
                registro.TipoDeduc?.Trim(),
                TipoDeduccionMonto,
                StringComparison.OrdinalIgnoreCase)
                    ? registro.MontoActual
                    : registro.PorcDeduc;

            return registro.Cedula.Trim()
                + ","
                + registro.Nombre
                + ","
                + request.FechaProceso.ToString(CultureInfo.InvariantCulture)
                + ","
                + registro.CodDeduccion
                + ",F,"
                + valor.ToString(CultureInfo.InvariantCulture);
        }
        public sealed class CcProcesoMensualArchivoF26RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public decimal PorcDeduc { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}