using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOControlUsuariosRolBL
    {
        private readonly FrmCOControlUsuariosRolDB Db;

        public FrmCOControlUsuariosRolBL(IConfiguration config)
        {
            Db = new FrmCOControlUsuariosRolDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CO_UsuariosRol_Usuarios_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return Db.CO_UsuariosRol_Usuarios_Dropdown_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolAntiguedadItem>> CO_UsuariosRol_Antiguedad_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_UsuariosRol_Antiguedad_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolGarantiaItem>> CO_UsuariosRol_Garantias_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_UsuariosRol_Garantias_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolOficinaItem>> CO_UsuariosRol_Oficinas_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_UsuariosRol_Oficinas_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoControlUsuariosListaResult<CoControlUsuariosRolInstitucionItem>> CO_UsuariosRol_Instituciones_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return Db.CO_UsuariosRol_Instituciones_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto CO_UsuariosRol_Antiguedad_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarAntiguedadRequest req)
        {
            return Db.CO_UsuariosRol_Antiguedad_Asignar(CodEmpresa, req);
        }

        public ErrorDto CO_UsuariosRol_Garantia_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarGarantiaRequest req)
        {
            return Db.CO_UsuariosRol_Garantia_Asignar(CodEmpresa, req);
        }

        public ErrorDto CO_UsuariosRol_Oficina_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarOficinaRequest req)
        {
            return Db.CO_UsuariosRol_Oficina_Asignar(CodEmpresa, req);
        }

        public ErrorDto CO_UsuariosRol_Institucion_Asignar(int CodEmpresa, CoControlUsuariosRolAsignarInstitucionRequest req)
        {
            return Db.CO_UsuariosRol_Institucion_Asignar(CodEmpresa, req);
        }

        public ErrorDto CO_UsuariosRol_Copia(int CodEmpresa, CoControlUsuariosRolCopiaRequest req)
        {
            return Db.CO_UsuariosRol_Copia(CodEmpresa, req);
        }

        public ErrorDto CO_UsuariosRol_Limpia(int CodEmpresa, CoControlUsuariosRolLimpiaRequest req)
        {
            return Db.CO_UsuariosRol_Limpia(CodEmpresa, req);
        }
    }
}
