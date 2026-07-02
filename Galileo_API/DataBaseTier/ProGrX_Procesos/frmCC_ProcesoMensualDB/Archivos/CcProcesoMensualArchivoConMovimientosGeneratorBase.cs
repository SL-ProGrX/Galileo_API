using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


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
            PrepararConfiguracion(connection, configuracion, request);
            return base.GenerarArchivo(connection, request);
        }
        protected override string ObtenerCodigoInstDeduc()
        {
            return CodigoInstitucionArchivo;
        }
        protected CcProcesoMensualArchivoConMovimientosGeneratorBase(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
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
