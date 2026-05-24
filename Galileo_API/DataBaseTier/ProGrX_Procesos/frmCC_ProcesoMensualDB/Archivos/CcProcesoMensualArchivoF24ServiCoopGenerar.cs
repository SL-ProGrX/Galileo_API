using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF24ServiCoopGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF24ServiCoopGenerar.CcProcesoMensualArchivoF24RegistroDbModel>
        
    {
  
        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["24"];

        protected override string CodigoPlanillaEnvio => "24";
        protected override string CodigoFormato => "F24";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                P.Tipo,
                S.nombre AS Nombre,
                ISNULL(D.descripcion, '') AS Departamento
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            LEFT JOIN AFDepartamentos D
                ON S.cod_institucion = D.cod_institucion
               AND S.cod_departamento = D.cod_departamento
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";

        protected override string CrearLineaArchivo(
                CcProcesoMensualArchivoF24RegistroDbModel registro,
                CcProcesoMensualGeneraArchivoRequest request)
        {
            return registro.Cedula.Trim()
                + ","
                + ReemplazarComas(registro.Nombre)
                + ","
                + registro.MontoActual.ToString(CultureInfo.InvariantCulture)
                + ","
                + ReemplazarComas(registro.Departamento)
                + ","
                + registro.Movimiento;
        }

        private static string ReemplazarComas(string? valor)
        {
            return (valor ?? string.Empty).Replace(",", " ");
        }
        public sealed class CcProcesoMensualArchivoF24RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Departamento { get; set; } = string.Empty;
        }
    }
}
