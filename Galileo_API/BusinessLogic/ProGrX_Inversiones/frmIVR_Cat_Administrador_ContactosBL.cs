using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatAdministradorContactosBl
    {
        private readonly FrmIvrCatAdministradorContactosDb _db;

        public FrmIvrCatAdministradorContactosBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrCatAdministradorContactosDb(config);
        }

        public ErrorDto<List<IvrCatAdministradorContactoData>>
            IVR_Cat_Administrador_Contactos_Buscar(
                int codEmpresa,
                string? codAdministrador,
                string? filtro)
        {
            return _db.IVR_Cat_Administrador_Contactos_Buscar(
                codEmpresa,
                codAdministrador,
                filtro);
        }

        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Obtener(
                int codEmpresa,
                string? codAdministrador,
                int codContacto)
        {
            return _db.IVR_Cat_Administrador_Contactos_Obtener(
                codEmpresa,
                codAdministrador,
                codContacto);
        }

        public ErrorDto
            <IvrCatAdministradorContactoNavegacionData?>
            IVR_Cat_Administrador_Contactos_Navegar(
                int codEmpresa,
                string? codAdministrador,
                int codContacto,
                bool siguiente)
        {
            return _db.IVR_Cat_Administrador_Contactos_Navegar(
                codEmpresa,
                codAdministrador,
                codContacto,
                siguiente);
        }

        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Registrar(
                int codEmpresa,
                IvrCatAdministradorContactoGuardarRequest? request)
        {
            return _db.IVR_Cat_Administrador_Contactos_Registrar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Actualizar(
                int codEmpresa,
                IvrCatAdministradorContactoGuardarRequest? request)
        {
            return _db.IVR_Cat_Administrador_Contactos_Actualizar(
                codEmpresa,
                request);
        }

        public ErrorDto IVR_Cat_Administrador_Contactos_Eliminar(
            int codEmpresa,
            IvrCatAdministradorContactoEliminarRequest? request)
        {
            return _db.IVR_Cat_Administrador_Contactos_Eliminar(
                codEmpresa,
                request);
        }
    }
}
