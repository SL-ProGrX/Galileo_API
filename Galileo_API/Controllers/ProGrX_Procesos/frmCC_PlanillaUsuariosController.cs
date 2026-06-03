using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Procesos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo.Controllers.ProGrX_Procesos
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCCPlanillaUsuariosController : ControllerBase
    {
        private readonly FrmCCPlanillaUsuariosBL _bl;

        public FrmCCPlanillaUsuariosController(IConfiguration config)
        {
            _bl = new FrmCCPlanillaUsuariosBL(config);
        }

        [Authorize]
        [HttpGet("CC_Planilla_Lista_Obtener")]
        public ErrorDto<List<CCPlanillaListaData>> CC_Planilla_Lista_Obtener(int CodEmpresa, string modo)
        {
            return _bl.CC_Planilla_Lista_Obtener(CodEmpresa, modo);
        }

        [Authorize]
        [HttpGet("CC_Planilla_Detalle_Obtener")]
        public ErrorDto<List<CCPlanillaDetalleData>> CC_Planilla_Detalle_Obtener(int CodEmpresa, string modo, string dato)
        {
            return _bl.CC_Planilla_Detalle_Obtener(CodEmpresa, modo, dato);
        }

        [Authorize]
        [HttpPost("CC_Planilla_Aplica")]
        public ErrorDto CC_Planilla_Aplica(int CodEmpresa, string usuario, CCPlanillaAplicaRequest req)
        {
            return _bl.CC_Planilla_Aplica(CodEmpresa, usuario, req);
        }

        [Authorize]
        [HttpPost("CC_Planilla_Todos_Aplica")]
        public ErrorDto CC_Planilla_Todos_Aplica(int CodEmpresa, string usuario, CCPlanillaTodosRequest req)
        {
            return _bl.CC_Planilla_Todos_Aplica(CodEmpresa, usuario, req);
        }
    }
}