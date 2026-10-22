using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatPortafoliosBl
    {
        private readonly FrmIvrCatPortafoliosDb _db;

        public FrmIvrCatPortafoliosBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmIvrCatPortafoliosDb(config);
        }

        public ErrorDto<List<IvrCatPortafolioDto>> IVR_Cat_Portafolios_Lista_Obtener(
            int codEmpresa)
        {
            return _db.IVR_Cat_Portafolios_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<IvrCatPortafolioAdministradorDto>> IVR_Cat_Portafolios_Administradores_Obtener(
            int codEmpresa,
            string? codPortafolio)
        {
            return _db.IVR_Cat_Portafolios_Administradores_Obtener(codEmpresa, codPortafolio);
        }

        public ErrorDto IVR_Cat_Portafolios_Registrar(
            int codEmpresa,
            IvrCatPortafolioGuardarRequest? request)
        {
            return _db.IVR_Cat_Portafolios_Registrar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Portafolios_Actualizar(
            int codEmpresa,
            IvrCatPortafolioGuardarRequest? request)
        {
            return _db.IVR_Cat_Portafolios_Actualizar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Portafolios_Eliminar(
            int codEmpresa,
            string? codPortafolio,
            string? usuario)
        {
            return _db.IVR_Cat_Portafolios_Eliminar(codEmpresa, codPortafolio, usuario);
        }
    }
}
