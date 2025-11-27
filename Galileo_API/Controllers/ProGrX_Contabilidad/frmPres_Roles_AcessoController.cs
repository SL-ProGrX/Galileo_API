using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmPres_Roles_AcessoController : ControllerBase
    {
        readonly FrmPresRolesAcessoBl _bl;
        public frmPres_Roles_AcessoController(IConfiguration config)
        {
            _bl = new FrmPresRolesAcessoBl(config);
        }

        [Authorize]
        [HttpGet("ObtenerRoles")]
        public ErrorDto<RolesLista> ObtenerRoles(int CodEmpresa, int contabilidad, string usuario)
        {
            return _bl.ObtenerRoles(CodEmpresa, contabilidad, usuario);
        }

        [Authorize]
        [HttpPost("Roles_Upsert")]
        public ErrorDto Roles_Upsert(int CodCliente, string usuario, RolesDto request)
        {
            return _bl.Roles_Upsert(CodCliente, usuario, request);
        }

        [Authorize]
        [HttpGet("Rol_Miembros_Obtener")]
        public ErrorDto<List<MiembrosRolDto>> Rol_Miembros_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _bl.Rol_Miembros_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        [Authorize]
        [HttpPost("Rol_Miembros_Registro")]
        public ErrorDto Rol_Miembros_Registro(int CodCliente, string cod_contabilidad, string rol, string usuario, MiembrosRolDto request)
        {
            return _bl.Rol_Miembros_Registro(CodCliente, cod_contabilidad, rol, usuario, request);
        }

        [Authorize]
        [HttpGet("Rol_Cuentas_Obtener")]
        public ErrorDto<List<CuentaRolDto>> Rol_Cuentas_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _bl.Rol_Cuentas_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        [Authorize]
        [HttpGet("Rol_CuentasRegistrada_Obtener")]
        public ErrorDto<List<CuentaRolDto>> Rol_CuentasRegistrada_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _bl.Rol_CuentasRegistrada_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        [Authorize]
        [HttpPost("Rol_Cuenta_Registra")]
        public ErrorDto Rol_Cuenta_Registra(int CodCliente, string cod_contabilidad, string rol, CuentaRolDto request)
        {
            return _bl.Rol_Cuenta_Registra(CodCliente, cod_contabilidad, rol, request);
        }

        [Authorize]
        [HttpPost("Rol_Cuenta_Elimina")]
        public ErrorDto Rol_Cuenta_Elimina(int CodCliente, string cod_contabilidad, string rol, CuentaRolDto request)
        {
            return _bl.Rol_Cuenta_Elimina(CodCliente, cod_contabilidad, rol, request);
        }

        [Authorize]
        [HttpDelete("Rol_Eliminar")]
        public ErrorDto Rol_Eliminar(int CodEmpresa, string codRol)
        {
            return _bl.Rol_Eliminar(CodEmpresa, codRol);
        }

        [Authorize]
        [HttpGet("Rol_Unidades_Obtener")]
        public ErrorDto<List<UnidadesRolDto>> Rol_Unidades_Obtener(int CodCliente, string cod_contabilidad, string rol, string? filtro, string usuario)
        {
            return _bl.Rol_Unidades_Obtener(CodCliente, cod_contabilidad, rol, filtro, usuario);
        }

        [Authorize]
        [HttpPost("Rol_Unidad_Registro")]
        public ErrorDto Rol_Unidad_Registro(int CodCliente, string cod_contabilidad, string rol, int boolasingado, UnidadesRolDto request)
        {
            return _bl.Rol_Unidad_Registro(CodCliente, cod_contabilidad, rol, boolasingado, request);
        }

        [Authorize]
        [HttpGet("Rol_Unidad_CC_Obtener")]
        public ErrorDto<List<CentroCosto>> Rol_Unidad_CC_Obtener(int CodCliente, string cod_contabilidad, string rol, string unidad, string? filtro, string usuario)
        {
            return _bl.Rol_Unidad_CC_Obtener(CodCliente, cod_contabilidad, rol, unidad, filtro, usuario);
        }

        [Authorize]
        [HttpPost("Rol_Unidad_CC_Registro")]
        public ErrorDto Rol_Unidad_CC_Registro(int CodCliente, string cod_contabilidad, string rol, string unidad, int boolasingado, CentroCosto request)
        {
            return _bl.Rol_Unidad_CC_Registro(CodCliente, cod_contabilidad, rol, unidad, boolasingado, request);
        }

    }
}