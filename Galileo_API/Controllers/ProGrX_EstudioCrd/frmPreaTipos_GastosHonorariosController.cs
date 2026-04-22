using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrPreaConfigController : ControllerBase
    {
        private readonly FrmCrPreaConfigBL _bl;
        public FrmCrPreaConfigController(IConfiguration config)
        {
            _bl = new FrmCrPreaConfigBL(config);
        }
        [Authorize]
        [HttpGet("CR_Prea_Config_Lista_Obtener")]
        public ErrorDto<CrPreaConfigListaResult> CR_Prea_Config_Lista_Obtener(int CodEmpresa,string tipo,string filtros)
        {
            return _bl.CR_Prea_Config_Lista_Obtener(CodEmpresa, tipo, filtros);
        }
        [Authorize]
        [HttpGet("CR_Prea_Config_Lista_Export")]
        public ErrorDto<CrPreaConfigListaResult> CR_Prea_Config_Lista_Export(int CodEmpresa,string tipo,string filtros)
        {
            return _bl.CR_Prea_Config_Lista_Export(CodEmpresa, tipo, filtros);
        }
        [Authorize]
        [HttpPost("CR_Prea_Config_Guardar")]
        public ErrorDto CR_Prea_Config_Guardar(int CodEmpresa,string usuario,string tipo,[FromBody] CrPreaConfigGuardarRequest request)
        {
            return _bl.CR_Prea_Config_Guardar(CodEmpresa, usuario, tipo, request);
        }
        [Authorize]
        [HttpDelete("CR_Prea_Config_Eliminar")]
        public ErrorDto CR_Prea_Config_Eliminar(int CodEmpresa,string usuario,string tipo,int id)
        {
            return _bl.CR_Prea_Config_Eliminar(CodEmpresa, usuario, tipo, id);
        }
        [Authorize]
        [HttpGet("CR_Prea_AvaluoCFIA_Obtener")]
        public ErrorDto<CrPreaAvaluoCfiaDto> CR_Prea_AvaluoCFIA_Obtener(int CodEmpresa)
        {
            return _bl.CR_Prea_AvaluoCFIA_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpPost("CR_Prea_AvaluoCFIA_Guardar")]
        public ErrorDto CR_Prea_AvaluoCFIA_Guardar(int CodEmpresa,string usuario,[FromBody] CrPreaAvaluoCfiaGuardarRequest request)
        {
            return _bl.CR_Prea_AvaluoCFIA_Guardar(CodEmpresa, usuario, request);
        }
    }
}