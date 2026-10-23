using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatComisionesBl
    {
        private readonly FrmIvrCatComisionesDb _db;

        public FrmIvrCatComisionesBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmIvrCatComisionesDb(config);
        }

        public ErrorDto<List<IvrCatComisionDto>> IVR_Cat_Comisiones_Lista_Obtener(
            int codEmpresa)
        {
            return _db.IVR_Cat_Comisiones_Lista_Obtener(codEmpresa);
        }

        public ErrorDto IVR_Cat_Comisiones_Registrar(
            int codEmpresa,
            IvrCatComisionGuardarRequest? request)
        {
            return _db.IVR_Cat_Comisiones_Registrar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Comisiones_Actualizar(
            int codEmpresa,
            IvrCatComisionGuardarRequest? request)
        {
            return _db.IVR_Cat_Comisiones_Actualizar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Comisiones_Eliminar(
            int codEmpresa,
            string? codComision,
            string? usuario)
        {
            return _db.IVR_Cat_Comisiones_Eliminar(codEmpresa, codComision, usuario);
        }
    }
}
