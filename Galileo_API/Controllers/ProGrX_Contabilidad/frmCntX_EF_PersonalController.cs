using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXEfPersonalController : ControllerBase
    {
        private readonly FrmCntXEfPersonalBL _bl;

        public FrmCntXEfPersonalController(IConfiguration config)
        {
            _bl = new FrmCntXEfPersonalBL(config);
        }

        [Authorize]
        [HttpGet("CntXEfPersonal_Lista")]
        public ErrorDto<List<CntXEfPersonalDto>> CntXEfPersonal_Lista(int codEmpresa, int codContabilidad)
        {
            return _bl.CntXEfPersonal_Lista(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpPost("CntXEfPersonal_Guardar")]
        public ErrorDto<bool> CntXEfPersonal_Guardar(int codEmpresa, string registroUsuario, [FromBody] CntXEfPersonalSaveParams param)
        {
            if (string.IsNullOrWhiteSpace(registroUsuario))
                return new ErrorDto<bool> { Code = -1, Description = "Usuario requerido", Result = false };

            return _bl.CntXEfPersonal_Guardar(codEmpresa, registroUsuario, param);
        }

        [Authorize]
        [HttpPost("CntXEfPersonal_Eliminar")]
        public ErrorDto<bool> CntXEfPersonal_Eliminar(int codEmpresa, string registroUsuario, [FromBody] CntXEfPersonalDeleteParams param)
        {
            if (string.IsNullOrWhiteSpace(registroUsuario))
                return new ErrorDto<bool> { Code = -1, Description = "Usuario requerido", Result = false };

            return _bl.CntXEfPersonal_Eliminar(codEmpresa, registroUsuario, param);
        }

        [Authorize]
        [HttpGet("CntXEfSecciones_Lista")]
        public ErrorDto<List<CntXEfSeccionDto>> CntXEfSecciones_Lista(int codEmpresa, int codContabilidad, string codEf)
        {
            return _bl.CntXEfSecciones_Lista(codEmpresa, codContabilidad, codEf);
        }

        [Authorize]
        [HttpPost("CntXEfSeccion_Guardar")]
        public ErrorDto<bool> CntXEfSeccion_Guardar(int codEmpresa, [FromBody] CntXEfSeccionSaveParams param)
        {
            return _bl.CntXEfSeccion_Guardar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CntXEfSeccion_Eliminar")]
        public ErrorDto<bool> CntXEfSeccion_Eliminar(int codEmpresa, [FromBody] CntXEfSeccionDeleteParams param)
        {            
            return _bl.CntXEfSeccion_Eliminar(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CntXEfSeccionesItems_Lista")]
        public ErrorDto<List<CntXEfSeccionSimpleDto>> CntXEfSeccionesItems_Lista(int codEmpresa, int codContabilidad, string codEf)
        {
            return _bl.CntXEfSeccionesItems_Lista(codEmpresa, codContabilidad, codEf);
        }

        [Authorize]
        [HttpPost("CntXEfCuentasDisponibles_Lista")]
        public ErrorDto<List<CntXCuentaDto>> CntXEfCuentasDisponibles_Lista(int codEmpresa, [FromBody] CntXCuentaFiltroParams param)
        {
            return _bl.CntXEfCuentasDisponibles_Lista(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CntXEfCuentasAsignadas_Lista")]
        public ErrorDto<List<CntXCuentaAsignadaDto>> CntXEfCuentasAsignadas_Lista(int codEmpresa, int codContabilidad, string codEf, string itemId)
        {
            return _bl.CntXEfCuentasAsignadas_Lista(codEmpresa, codContabilidad, codEf, itemId);
        }

        [Authorize]
        [HttpGet("CntXEfFuncionesAsignadas_Lista")]
        public ErrorDto<List<CntXFxAsignadaDto>> CntXEfFuncionesAsignadas_Lista(int codEmpresa, int codContabilidad, string codEf, string itemId)
        {
            return _bl.CntXEfFuncionesAsignadas_Lista(codEmpresa, codContabilidad, codEf, itemId);
        }

        [Authorize]
        [HttpPost("CntXEfCuenta_Proc")]
        public ErrorDto<bool> CntXEfCuenta_Proc(int codEmpresa, [FromBody] CntXEfCuentaProcParams param)
        {
            return _bl.CntXEfCuenta_Proc(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CntXEfFx_Proc")]
        public ErrorDto<bool> CntXEfFx_Proc(int codEmpresa, [FromBody] CntXEfFxProcParams param)
        {
            return _bl.CntXEfFx_Proc(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CntXEfProcesa")]
        public ErrorDto<bool> CntXEfProcesa(int codEmpresa, [FromBody] CntXEfProcesaParams param)
        {
            return _bl.CntXEfProcesa(codEmpresa, param);
        }
    }
}
