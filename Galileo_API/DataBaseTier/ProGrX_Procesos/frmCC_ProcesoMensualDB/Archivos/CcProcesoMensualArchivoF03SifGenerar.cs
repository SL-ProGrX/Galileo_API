using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF03SifGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF03SifGenerar.CcProcesoMensualArchivoF03SifRegistroDbModel>

    {
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["03_S"];

        protected override string CodigoPlanillaEnvio => "03_S";
        protected override string CodigoFormato => "F03";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;
       protected override Encoding EncodingArchivo => CcProcesoMensualEncodingHelper.Utf8SinBom;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                S.nombre AS Nombre,
                P.Tipo,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                I.Descripcion AS InstDesc
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            INNER JOIN instituciones I
                ON S.cod_institucion = I.cod_institucion
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF03SifRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return string.Join(
                "\t",
                LimpiarCampo(registro.Cedula),
                LimpiarCampo(registro.Nombre),
                LimpiarCampo(registro.Tipo),
                registro.MontoActual.ToString(CultureInfo.InvariantCulture),
                LimpiarCampo(registro.Movimiento),
                LimpiarCampo(registro.InstDesc));
        }

        private static string LimpiarCampo(string? valor)
        {
            return (valor ?? string.Empty)
                .Trim()
                .Replace("\t", " ");
        }

        public sealed class CcProcesoMensualArchivoF03SifRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string InstDesc { get; set; } = string.Empty;
        }
    }
 
}
