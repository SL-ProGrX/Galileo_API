 
using System.Data;
using System.Globalization; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF34AsoeCorrGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF34AsoeCorrGenerar.CcProcesoMensualArchivoF34RegistroDbModel>

    {
        private const string Encabezado = "Identificacion;concepto;valor;nombre";

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["34"];

        protected override string CodigoPlanillaEnvio => "34";
        protected override string CodigoFormato => "F34";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                S.CedulaR AS CedulaColilla,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";
        public CcProcesoMensualArchivoF34AsoeCorrGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }
        protected override string CrearEncabezado()
        {
            return Encabezado;
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF34RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return (registro.CedulaColilla ?? string.Empty).Trim()
                + ";"
                + registro.CodDeduccion
                + ";"
                + registro.MontoActual.ToString("0")
                + ";"
                + registro.Nombre;
        }

        public sealed class CcProcesoMensualArchivoF34RegistroDbModel
        {
            public string CedulaColilla { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;   
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
