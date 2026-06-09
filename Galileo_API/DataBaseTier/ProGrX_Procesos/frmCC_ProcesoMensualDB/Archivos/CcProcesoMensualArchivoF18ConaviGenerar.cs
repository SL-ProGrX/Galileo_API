
using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF18ConaviGenerar : CcProcesoMensualArchivoF18F19Base<CcProcesoMensualArchivoF18ConaviGenerar.CcProcesoMensualArchivoF18RegistroDbModel>

    {
        public CcProcesoMensualArchivoF18ConaviGenerar(
         IOptions<ArchivosGeneradosOptions> archivosOptions)
         : base(archivosOptions)
        {
        }
         

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["18"];

        protected override string CodigoPlanillaEnvio => "18";
        protected override string CodigoFormato => "F18";

        protected override string QueryRegistros => @"
            SELECT
                P.cedula AS Cedula,
                P.Tipo,
                P.cod_deduccion AS CodDeduccion,
                P.Movimiento,
                P.Monto_Actual AS MontoActual,
                ISNULL(S.cod_sector, 0) AS Sector
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.cod_deduccion, P.movimiento";

        protected override string CrearLineaArchivo(  CcProcesoMensualArchivoF18RegistroDbModel registro,  CcProcesoMensualGeneraArchivoRequest request)
        {
            return string.Join(
                "\t",
                FormatearCedula(registro.Cedula),
                registro.CodDeduccion.Trim(),
                FormatearMontoPorTipo(registro),
                "0",
                "0");
        }

         

        private static string FormatearCedula(string? cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            if (decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numero))
            {
                texto = numero.ToString("0000000000", CultureInfo.InvariantCulture);
            }

            return texto.Length > 10
                ? texto[..10]
                : texto;
        }

        public sealed class CcProcesoMensualArchivoF18RegistroDbModel : ICcProcesoMensualArchivoTipoMontoRegistro
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public int Sector { get; set; }
        }
    }
}
