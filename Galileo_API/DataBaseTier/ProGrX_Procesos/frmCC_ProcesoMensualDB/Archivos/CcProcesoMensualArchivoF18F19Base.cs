using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public abstract class CcProcesoMensualArchivoF18F19Base<TRegistro>
    : CcProcesoMensualArchivoConMovimientosGeneratorBase<TRegistro>
    where TRegistro : ICcProcesoMensualArchivoTipoMontoRegistro
    {
        private const string TipoAhorro = "A";
        private const string TipoExtraordinario = "E";
        private const string TipoCredito = "C";

        private decimal _porcAhorro;

        protected CcProcesoMensualArchivoF18F19Base(
            IOptions<ArchivosGeneradosOptions> archivosOptions)
            : base(archivosOptions)
        {
        }

        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;
        protected decimal PorcentajeAhorro => _porcAhorro;

        protected override void PrepararConfiguracion(
            IDbConnection connection,
            CcProcesoMensualArchivoConfiguracionModel configuracion,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            _porcAhorro = configuracion.PorcAhorro;
        }

        protected string FormatearMontoPorTipo(TRegistro registro)
        {
            return registro.Tipo?.Trim().ToUpperInvariant() switch
            {
                TipoAhorro => registro.MontoActual.ToString("######0.00", CultureInfo.InvariantCulture),
                TipoExtraordinario => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture),
                TipoCredito => registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture),
                _ => string.Empty
            };
        }

        protected string FormatearMontoPorTipoSinPunto(TRegistro registro)
        {
            return FormatearMontoPorTipo(registro)
                .Replace(".", string.Empty, StringComparison.Ordinal);
        }
    }
}
