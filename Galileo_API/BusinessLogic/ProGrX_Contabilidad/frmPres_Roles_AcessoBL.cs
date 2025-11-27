using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresRolesAcessoBl
    {
        readonly FrmPresRolesAcessoDb _db;

        public FrmPresRolesAcessoBl(IConfiguration config)
        {
            _db = new FrmPresRolesAcessoDb(config);
        }

        public ErrorDto<RolesLista> ObtenerRoles(int CodEmpresa, int contabilidad, string usuario)
        {
            return _db.ObtenerRoles(CodEmpresa, contabilidad, usuario);
        }

        public ErrorDto Roles_Upsert(int CodCliente, string usuario, RolesDto request)
        {
            return _db.Roles_Upsert(CodCliente, usuario, request);
        }

        public ErrorDto<List<MiembrosRolDto>> Rol_Miembros_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _db.Rol_Miembros_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }
        public ErrorDto Rol_Miembros_Registro(int CodCliente, string cod_contabilidad, string rol, string usuario, MiembrosRolDto request)
        {
            return _db.Core_Miembros_Registro(CodCliente, cod_contabilidad, rol, usuario, request);
        }

        public ErrorDto<List<CuentaRolDto>> Rol_Cuentas_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _db.Rol_Cuentas_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        public ErrorDto<List<CuentaRolDto>> Rol_CuentasRegistrada_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _db.Rol_CuentasRegistrada_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        public ErrorDto Rol_Cuenta_Registra(int CodCliente, string cod_contabilidad, string rol, CuentaRolDto request)
        {
            return _db.Rol_Cuenta_Registra(CodCliente, cod_contabilidad, rol, request);
        }

        public ErrorDto Rol_Cuenta_Elimina(int CodCliente, string cod_contabilidad, string rol, CuentaRolDto request)
        {
            return _db.Rol_Cuenta_Elimina(CodCliente, cod_contabilidad, rol, request);
        }

        public ErrorDto Rol_Eliminar(int CodEmpresa, string codRol)
        {
            return _db.Rol_Eliminar(CodEmpresa, codRol);
        }

        public ErrorDto<List<UnidadesRolDto>> Rol_Unidades_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _db.Rol_Unidades_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        public ErrorDto Rol_Unidad_Registro(int CodCliente, string cod_contabilidad, string rol, int boolasingado, UnidadesRolDto request)
        {
            return _db.Rol_Unidad_Registro(CodCliente, cod_contabilidad, rol, boolasingado, request);
        }

        public ErrorDto<List<CentroCosto>> Rol_Unidad_CC_Obtener(int CodCliente, string cod_contabilidad, string rol, string unidad, string? filtro, string usuario)
        {
            return _db.Rol_Unidad_CC_Obtener(CodCliente, cod_contabilidad, rol, unidad, filtro, usuario);
        }

        public ErrorDto Rol_Unidad_CC_Registro(int CodCliente, string cod_contabilidad, string rol, string unidad, int boolasingado, CentroCosto request)
        {
            return _db.Rol_Unidad_CC_Registro(CodCliente, cod_contabilidad, rol, unidad, boolasingado, request);
        }

    }
}