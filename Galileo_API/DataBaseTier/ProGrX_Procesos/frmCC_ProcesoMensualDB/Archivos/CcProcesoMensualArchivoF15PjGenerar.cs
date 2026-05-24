using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF15PjGenerar : CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoF15PjGenerar.CcProcesoMensualArchivoF15RegistroDbModel>

    {
        private const string TipoAhorro = "A";

        private List<string> _movimientos = [];

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["15"];

        protected override string CodigoPlanillaEnvio => "15";
        protected override string CodigoFormato => "F15";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;
        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Tipo,
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                ISNULL(S.cod_sector, 0) AS Sector,
                S.nombre AS Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
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

            _movimientos = ObtenerMovimientos(configuracion);

            return base.GenerarArchivo(connection, request);
        }

        protected override string CrearNombreArchivo(
           IDbConnection connection,
           CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);
            var codigo = configuracion.CodigoInstDeduc?.Trim() ?? string.Empty;
            var fechaTexto = fechaServidor.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            return $"E-{codigo}-{fechaTexto}-01{ExtensionArchivo}";
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
          CcProcesoMensualArchivoF15RegistroDbModel registro,
          CcProcesoMensualGeneraArchivoRequest request)
        {
            return FormatearCedula(registro.Cedula)
                + "\t"
                + registro.CodDeduccion.Trim()
                + "\t"
                + FormatearMontoPorTipo(registro)
                + "\t"
                + ObtenerSectorArchivo(registro.Sector);
        }


        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            var movimientos = new List<string>();

            AgregarMovimientoSiAplica(movimientos, configuracion.IncInclusiones, "I");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncExclusiones, "E");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncModificaciones, "C");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncMantienen, "M");

            movimientos.Add("P");

            return movimientos;
        }

        private static void AgregarMovimientoSiAplica( List<string> movimientos, int indicador, string movimiento)
        {
            if (indicador == 1)
            {
                movimientos.Add(movimiento);
            }
        }



        private static string FormatearCedula(string cedula)
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

        private static string FormatearMontoPorTipo(
            CcProcesoMensualArchivoF15RegistroDbModel registro)
        {
            if (string.Equals(
                registro.Tipo?.Trim(),
                TipoAhorro,
                StringComparison.OrdinalIgnoreCase))
            {
                return registro.MontoActual.ToString("######0.00", CultureInfo.InvariantCulture);
            }

            return registro.MontoActual.ToString("############0.00", CultureInfo.InvariantCulture);
        }

        private static string ObtenerSectorArchivo(int sector)
        {
            return sector == 2 ? "1" : "0";
        }

        public sealed class CcProcesoMensualArchivoF15RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public int Sector { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
