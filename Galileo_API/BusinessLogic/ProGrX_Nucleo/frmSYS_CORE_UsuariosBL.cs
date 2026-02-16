using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.BusinessLogic
{
    public class FrmSysCoreUsuariosBL(IConfiguration config)
    {
        private readonly FrmSysCoreUsuariosDB _db = new FrmSysCoreUsuariosDB(config);

        public ErrorDto<CoreUsuariosLista> CoreUsuariosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CoreUsuariosLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto CoreUsuariosExiste_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CoreUsuariosExiste_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoreUsuariosData> CoreUsuarios_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CoreUsuarios_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto CoreUsuarios_Importar(int CodEmpresa)
        {
            return _db.CoreUsuarios_Importar(CodEmpresa);
        }

        public ErrorDto<CoreUsuariosData> CoreUsuario_Scroll(int CodEmpresa, int scroll, string? usuario)
        {
            return _db.CoreUsuario_Scroll(CodEmpresa, scroll, usuario);
        }

        public ErrorDto CoreUsuarios_Guardar(int CodEmpresa, CoreUsuariosData usuariosData)
        {
            return _db.CoreUsuarios_Guardar(CodEmpresa, usuariosData);
        }

        public ErrorDto CoreUsuarios_Eliminar(int CodEmpresa, string usuario)
        {
            return _db.CoreUsuarios_Eliminar(CodEmpresa, usuario);
        }

        public ErrorDto<List<CoreMiembrosData>> CoreUsuariosMiembros_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CoreUsuariosMiembros_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<CoreMiembrosRolData>> CoreUsuariosUENs_Roles_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CoreUsuariosUENs_Roles_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto CoreUsuariosMiembro_Actualiza(string miembro)
        {
            return _db.CoreUsuariosMiembro_Actualiza(miembro);
        }

        public ErrorDto CoreUsuariosMiembroRol_Actualiza(string miembroRol)
        {
            return _db.CoreUsuariosMiembroRol_Actualiza(miembroRol);
        }
    }
}