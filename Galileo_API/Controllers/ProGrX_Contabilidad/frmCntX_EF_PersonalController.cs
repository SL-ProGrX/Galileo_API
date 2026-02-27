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
    }
}
