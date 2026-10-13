using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatParametrosBl
    {
        private readonly FrmIvrCatParametrosDb _db;

        public FrmIvrCatParametrosBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrCatParametrosDb(config);
        }

        public ErrorDto IVR_Cat_Parametros_Inicializar(
            int codEmpresa)
        {
            return _db.IVR_Cat_Parametros_Inicializar(
                codEmpresa);
        }

        public ErrorDto<List<IvrCatParametroDto>>
            IVR_Cat_Parametros_Lista_Obtener(
                int codEmpresa)
        {
            return _db.IVR_Cat_Parametros_Lista_Obtener(
                codEmpresa);
        }

        public ErrorDto IVR_Cat_Parametros_Actualizar(
            int codEmpresa,
            IvrCatParametroActualizarRequest? request)
        {
            return _db.IVR_Cat_Parametros_Actualizar(
                codEmpresa,
                request);
        }
    }
}