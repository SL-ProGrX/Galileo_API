using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF34AsoeCorrGenerar :  CcProcesoMensualArchivoPlanoGeneratorBase<CcProcesoMensualArchivoF34AsoeCorrGenerar.CcProcesoMensualArchivoF34RegistroDbModel>

    {
        private const string Encabezado = "Identificacion;concepto;valor;nombre";

        private List<string> _movimientos = [];

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
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture)
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
