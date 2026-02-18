using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOControlUsuariosBL
    {
        private readonly FrmCOControlUsuariosDB Db;

        public FrmCOControlUsuariosBL(IConfiguration config)
        {
            Db = new FrmCOControlUsuariosDB(config);
        }

        public ErrorDto<CoControlUsuariosData> CO_Usuarios_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_Usuarios_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoControlUsuariosData> CO_Usuarios_Scroll_Obtener(int CodEmpresa, int scrollCode, string? usuarioActual)
        {
            return Db.CO_Usuarios_Scroll_Obtener(CodEmpresa, scrollCode, usuarioActual);
        }
        public ErrorDto CO_Usuarios_Existe_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_Usuarios_Existe_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosF4Item>> CO_Usuarios_F4_Obtener(int CodEmpresa, string? filtro)
        {
            return Db.CO_Usuarios_F4_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_Bancos_Dropdown_Obtener(int CodEmpresa, string usuario_sesion)
        {
            return Db.CO_Bancos_Dropdown_Obtener(CodEmpresa, usuario_sesion);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Lista_Obtener(
            int CodEmpresa, string cedula, string? filtro)
        {
            return Db.CO_Usuarios_Cuentas_Lista_Obtener(CodEmpresa, cedula, filtro);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCuentasData>> CO_Usuarios_Cuentas_Lista_Export(
            int CodEmpresa, string cedula, string? filtro)
        {
            return Db.CO_Usuarios_Cuentas_Lista_Export(CodEmpresa, cedula, filtro);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosGrupoItem>> CO_Usuarios_Grupos_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_Usuarios_Grupos_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosCarteraItem>> CO_Usuarios_Carteras_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_Usuarios_Carteras_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto CO_Usuarios_Guardar(int CodEmpresa, CoControlUsuariosGuardarRequest req)
        {
            return Db.CO_Usuarios_Guardar(CodEmpresa, req);
        }

        public ErrorDto CO_Usuarios_Grupos_Asignar(int CodEmpresa, CoControlUsuariosAsignacionRequest req)
        {
            return Db.CO_Usuarios_Grupos_Asignar(CodEmpresa, req);
        }

        public ErrorDto CO_Usuarios_Carteras_Asignar(int CodEmpresa, CoControlUsuariosAsignacionRequest req)
        {
            return Db.CO_Usuarios_Carteras_Asignar(CodEmpresa, req);
        }

        public ErrorDto CO_Usuarios_Eliminar(int CodEmpresa, string usuario, string usuario_sesion)
        {
            return Db.CO_Usuarios_Eliminar(CodEmpresa, usuario, usuario_sesion);
        }
    }
}
