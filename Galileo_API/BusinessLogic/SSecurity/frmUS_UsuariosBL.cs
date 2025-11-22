using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsUsuariosBl
    {

        readonly FrmUsUsuariosDb UsuariosDB;

        public FrmUsUsuariosBl(IConfiguration config)
        {
            UsuariosDB = new FrmUsUsuariosDb(config);
        }

        public int UsuarioExiste(string usuario)
        {
            return UsuariosDB.UsuarioExiste(usuario);
        }

        public List<UsuarioModel> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosDB.UsuariosEmpresaObtener(codEmpresa, AdminView, DirGlobal);
        }

        public UsuarioModel UsuarioConsultar(string paramUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosDB.UsuarioConsultar(paramUsuario, codEmpresa, AdminView, DirGlobal);
        }

        public ErrorDto UsuarioGuardarActualizar(UsuarioModel usuarioDto)
        {
            return UsuariosDB.UsuarioGuardarActualizar(usuarioDto);
        }

        public List<UsuarioClienteDto> UsuarioClientesConsultar(string nombreUsuario)
        {
            List<UsuarioClienteDto> clientes = UsuariosDB.UsuarioClientesConsultar(nombreUsuario);

            foreach (UsuarioClienteDto cli in clientes)
            {
                string strUsuario = cli.Usuario!.Trim();

                if (!string.IsNullOrEmpty(strUsuario) && strUsuario == nombreUsuario)
                {
                    cli.Seleccionado = true;
                }
                else
                {
                    cli.Seleccionado = false;
                }
            }
            return clientes;
        }

        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDto usuarioClienteAsignaDto)
        {
            return UsuariosDB.UsuarioClienteAsignar(usuarioClienteAsignaDto);
        }

        public List<TipoTransaccionBitacora> UsuarioCuentaTiposTransaccionObtener()
        {
            return UsuariosDB.UsuarioCuentaTiposTransaccionObtener();
        }

        public List<UsuarioCuentaBitacora> UsuarioBitacoraConsultar(UsuarioBitacoraRequest request)
        {
            return UsuariosDB.UsuarioBitacoraConsultar(request);
        }

        public List<UsuarioClienteRolDto> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            return UsuariosDB.UsuarioClienteRolesConsultar(nombreUsuario, codEmpresa);
        }

        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            return UsuariosDB.UsuarioClienteRolAsignar(usuarioClienteRolAsignaDto);
        }
    }
}