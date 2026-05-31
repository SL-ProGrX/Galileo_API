using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrAutorizacionesController : ControllerBase
    {
        private readonly FrmAfCrAutorizacionesBL _bl;

        public FrmAFCrAutorizacionesController(IConfiguration config)
        {
            _bl = new FrmAfCrAutorizacionesBL(config);
        }

        [Authorize]
        [HttpPost("AF_CRAutorizaciones_Obtener")]
        public ErrorDto<List<AfCrAutorizacion>> AF_CRAutorizaciones_Obtener(int CodEmpresa, [FromBody] AfCrAutorizacionFiltros filtros)
        {
            return _bl.AF_CRAutorizaciones_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_CRAutorizaciones_Autorizar")]
        public ErrorDto AF_CRAutorizaciones_Autorizar(int CodEmpresa, int CodRenuncia, string Observaciones, int pAutoriza, string Usuario)
        {
            return _bl.AF_CRAutorizaciones_Autorizar(CodEmpresa, CodRenuncia, Observaciones, pAutoriza, Usuario);
        }
    }
}