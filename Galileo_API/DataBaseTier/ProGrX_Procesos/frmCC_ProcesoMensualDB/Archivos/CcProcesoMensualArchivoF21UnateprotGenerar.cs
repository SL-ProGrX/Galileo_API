 
using System.Data;
using System.Globalization; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF21UnateprotGenerar : CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoF21UnateprotGenerar.CcProcesoMensualArchivoF21RegistroDbModel>

    {
        private List<string> _movimientos = [];
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["21"];

        protected override string CodigoPlanillaEnvio => "21";
        protected override string CodigoFormato => "F21";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.cedula AS Cedula,
                P.Tipo,
                P.cod_deduccion AS CodDeduccion,
                P.Movimiento,
                P.Monto_Actual AS MontoActual,
                ISNULL(Dept.Descripcion, '') AS Departamento,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            LEFT JOIN AFDepartamentos Dept
                ON S.cod_Institucion = Dept.Cod_Institucion
               AND S.cod_Departamento = Dept.Cod_Departamento
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.cod_deduccion, P.movimiento";

        public CcProcesoMensualArchivoF21UnateprotGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }

        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            _movimientos = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerMovimientosPorIndicadores(
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
            CcProcesoMensualArchivoF21RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return registro.Cedula
                + ","
                + registro.Nombre
                + ","
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture)
                + ","
                + registro.Departamento;
        }

        public sealed class CcProcesoMensualArchivoF21RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public string Departamento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
