using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public abstract class CcProcesoMensualArchivoConMovimientosGeneratorBase<TRegistro>
        : CcProcesoMensualArchivoPlanoGenerarBase<TRegistro>
    {
        private List<string> _movimientos = [];
        protected string CodigoInstitucionArchivo { get; private set; } = string.Empty;
        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(connection, request.CodInstitucion);

            CodigoInstitucionArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerCodigoInstitucionArchivo(request.CodInstitucion, configuracion.CodigoInstDeduc);

            _movimientos = ObtenerMovimientos(configuracion);

            return base.GenerarArchivo(connection, request);
        }
        protected virtual List<string> ObtenerMovimientos(CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerMovimientosPorComparador(
                configuracion);
        }
        protected override object CrearParametrosRegistros(CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
                request.FechaProceso,
                Movimientos = _movimientos,
                request.CodInstitucion
            };
        }
        protected virtual void PrepararConfiguracion(IDbConnection connection, CcProcesoMensualArchivoConfiguracionModel configuracion, CcProcesoMensualGeneraArchivoRequest request)
        {
        }
    }
}
