using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysIvaParametrosController : ControllerBase
    {
        private readonly FrmSysIvaParametrosBL _bl;

        public FrmSysIvaParametrosController(IConfiguration config)
        {
            _bl = new FrmSysIvaParametrosBL(config);
        }

        [Authorize]
        [HttpGet("Sys_Iva_Parametros_Lista_Obtener")]
        public ErrorDto<SysIvaParametrosLista> Sys_Iva_Parametros_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sys_Iva_Parametros_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_Iva_Parametros_Obtener")]
        public ErrorDto<List<SysIvaParametrosData>> Sys_Iva_Parametros_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sys_Iva_Parametros_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPut("Sys_Iva_Parametro_Actualizar")]
        public ErrorDto<SysIvaParametrosUpdateResponse> Sys_Iva_Parametro_Actualizar(int CodEmpresa, string codParametro, string usuario, string jrequest)
        {
            return _bl.Sys_Iva_Parametro_Actualizar(CodEmpresa, codParametro, usuario, jrequest);
        }

        [Authorize]
        [HttpGet("Sys_Iva_Cuentas_Buscar")]
        public ErrorDto<SysIvaCuentasResumenLista> Sys_Iva_Cuentas_Buscar(int CodEmpresa, int codContabilidad, string filtros)
        {
            return _bl.Sys_Iva_Cuentas_Buscar(CodEmpresa, codContabilidad, filtros);
        }   
        [Authorize]
        [HttpGet("Sys_Iva_CuentaPorCodigo_Obtener")]
        public ErrorDto<SysIvaCuentasResumenData> Sys_Iva_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codigoSinMask)
        {
            return _bl.Sys_Iva_CuentaPorCodigo_Obtener(CodEmpresa, codContabilidad, codigoSinMask);
        }
        [Authorize]
        [HttpGet("Sys_Iva_Cuentas_Todas_Obtener")]
        public ErrorDto<SysIvaCuentasResumenLista> Sys_Iva_Cuentas_Todas_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _bl.Sys_Iva_Cuentas_Todas_Obtener(CodEmpresa, codContabilidad);
        }

    }
}