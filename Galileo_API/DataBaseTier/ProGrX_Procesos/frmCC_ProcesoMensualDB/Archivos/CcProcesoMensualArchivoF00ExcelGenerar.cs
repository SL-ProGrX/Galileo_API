using System.Data;
using System.Globalization;
using System.Text;
using Dapper; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF00ExcelGenerar :      CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoF00ExcelGenerar.CcProcesoMensualArchivoF00RegistroDbModel>

    {

        private List<string> _movimientos = [];

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["00"];

        protected override string CodigoPlanillaEnvio => "00";
        protected override string CodigoFormato => "F00";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;
        protected override Encoding EncodingArchivo => Encoding.UTF8;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                S.nombre AS Nombre,
                P.Tipo,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
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
                  request.FechaProceso,
                Movimientos = _movimientos,
                request.CodInstitucion
            };
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF00RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return string.Join(
                ";",
                LimpiarCampo(registro.Cedula),
                LimpiarCampo(registro.Nombre),
                LimpiarCampo(registro.Tipo),
                registro.MontoActual.ToString(CultureInfo.InvariantCulture),
                LimpiarCampo(registro.Movimiento),
                LimpiarCampo(registro.InstDesc),
                LimpiarCampo(registro.IdAlterno));
        }

        protected override CcProcesoMensualArchivoGeneradoModel CrearRespuesta(
            string nombreArchivo,
            string rutaArchivo,
            string contenido)
        {
            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentType,
                ArchivoBytes = EncodingArchivo.GetBytes(contenido),
                ArchivosGenerados = [rutaArchivo]
            };
        }

        private static string LimpiarCampo(string? valor)
        {
            return (valor ?? string.Empty)
                .Trim()
                .Replace(";", " ");
        }

        public sealed class CcProcesoMensualArchivoF00RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public string InstDesc { get; set; } = string.Empty;
            public string IdAlterno { get; set; } = string.Empty;
        }

    }
}
