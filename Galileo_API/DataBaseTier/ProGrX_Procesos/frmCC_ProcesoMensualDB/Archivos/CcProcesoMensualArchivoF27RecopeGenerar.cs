
using System.Data;
using System.Globalization; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF27RecopeGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF27RecopeGenerar.CcProcesoMensualArchivoF27RegistroDbModel>

    {
        public CcProcesoMensualArchivoF27RecopeGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        { }

        private const string TipoDeduccionMonto = "M";
        readonly DateTime _fechaArchivo = DateTime.MinValue;

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["27"];

        protected override string CodigoPlanillaEnvio => "27";
        protected override string CodigoFormato => "F27";
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
                S.CedulaR AS CedulaColilla,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";

     

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF27RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var cedula = FormatearCedula(registro.Cedula);
            
            var monto = registro.MontoActual.ToString("0");

            DateTime vFecha = DateTime.ParseExact(
                    request.FechaProceso + "14",
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture
                );

            var fechaTexto = vFecha.ToString("dd.MM.yyyy");


            if (string.Equals(
                registro.TipoDeduc?.Trim(),
                TipoDeduccionMonto,
                StringComparison.OrdinalIgnoreCase))
            {
                return cedula
                    + "\t"
                    + fechaTexto
                    + "\t"
                    + registro.CodDeduccion
                    + "\t"
                    + monto;
            }

            return cedula
                + "\t"
                + fechaTexto
                + "\t"
                + registro.CodDeduccion
                + "\t"
                + "\t"
                + monto;
        }

        private static string FormatearCedula(string? cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            return decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numero)
                    ? numero.ToString("0000000000", CultureInfo.InvariantCulture)
                    : texto;
        }

       

        public sealed class CcProcesoMensualArchivoF27RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public decimal PorcDeduc { get; set; } = 0;
            public string TipoDeduc { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public string CedulaColilla { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
