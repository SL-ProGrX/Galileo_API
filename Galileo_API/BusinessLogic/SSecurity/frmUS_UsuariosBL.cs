using Galileo.DataBaseTier;
using Galileo.Models;
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

        public ErrorDto<int> UsuarioExiste(string usuario)
        {
            return UsuariosDB.UsuarioExiste(usuario);
        }

        public ErrorDto<List<UsuarioModel>> UsuariosEmpresaObtener(int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosDB.UsuariosEmpresaObtener(codEmpresa, AdminView, DirGlobal);
        }

        public ErrorDto<List<UsuarioModel>> UsuariosExplorerObtener(int codEmpresa)
        {
            return UsuariosDB.UsuariosExplorerObtener(codEmpresa);
        }

        public ErrorDto<ExplorerRootInfoDto> ExplorerRootInfoObtener(int codEmpresa)
        {
            return UsuariosDB.ExplorerRootInfoObtener(codEmpresa);
        }

        public ErrorDto<UsuarioModel?> UsuarioConsultar(string paramUsuario, int codEmpresa, bool AdminView, bool DirGlobal)
        {
            return UsuariosDB.UsuarioConsultar(paramUsuario, codEmpresa, AdminView, DirGlobal);
        }

        public ErrorDto UsuarioGuardarActualizar(UsuarioModel usuarioDto)
        {
            return UsuariosDB.UsuarioGuardarActualizar(usuarioDto);
        }

        public ErrorDto<List<UsuarioClienteDto>> UsuarioClientesConsultar(string nombreUsuario)
        {
            var respuesta = UsuariosDB.UsuarioClientesConsultar(nombreUsuario);
            if ((respuesta.Code ?? 0) < 0 || respuesta.Result is null)
                return respuesta;

            List<UsuarioClienteDto> clientes = respuesta.Result;

            foreach (UsuarioClienteDto cli in clientes)
            {
                string strUsuario = cli.Usuario.Trim();

                if (!string.IsNullOrEmpty(strUsuario) && strUsuario == nombreUsuario)
                {
                    cli.Seleccionado = true;
                }
                else
                {
                    cli.Seleccionado = false;
                }
            }
            respuesta.Result = clientes;
            return respuesta;
        }

        public ErrorDto UsuarioClienteAsignar(UsuarioClienteAsignaDto usuarioClienteAsignaDto)
        {
            return UsuariosDB.UsuarioClienteAsignar(usuarioClienteAsignaDto);
        }

        public ErrorDto<List<TipoTransaccionBitacora>> UsuarioCuentaTiposTransaccionObtener()
        {
            return UsuariosDB.UsuarioCuentaTiposTransaccionObtener();
        }

        public ErrorDto<List<UsuarioCuentaBitacora>> UsuarioBitacoraConsultar(UsuarioBitacoraRequest request)
        {
            return UsuariosDB.UsuarioBitacoraConsultar(request);
        }

        public ErrorDto<List<UsuarioClienteRolDto>> UsuarioClienteRolesConsultar(string nombreUsuario, string codEmpresa)
        {
            return UsuariosDB.UsuarioClienteRolesConsultar(nombreUsuario, codEmpresa);
        }

        public ErrorDto<List<UsuarioClienteRolDto>> UsuarioClienteRolesExplorerObtener(string nombreUsuario, int codEmpresa)
        {
            return UsuariosDB.UsuarioClienteRolesExplorerObtener(nombreUsuario, codEmpresa);
        }

        public ErrorDto<List<RolMiembroExplorerDto>> RolMiembrosExplorerObtener(string rolId, int codEmpresa)
        {
            return UsuariosDB.RolMiembrosExplorerObtener(rolId, codEmpresa);
        }

        public ErrorDto UsuarioClienteRolAsignar(UsuarioClienteRolAsignaDto usuarioClienteRolAsignaDto)
        {
            return UsuariosDB.UsuarioClienteRolAsignar(usuarioClienteRolAsignaDto);
        }
    }
}
