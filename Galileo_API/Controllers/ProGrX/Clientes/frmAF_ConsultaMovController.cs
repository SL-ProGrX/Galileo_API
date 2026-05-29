using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfConsultaMovController : ControllerBase
    {
        private readonly FrmAfConsultaMovBl BL_AF_ConsultaMov;
        public FrmAfConsultaMovController(IConfiguration config)
        {
            BL_AF_ConsultaMov = new FrmAfConsultaMovBl(config);
        }

        [Authorize]
        [HttpGet("ConsultaMovIngresos_Obtener")]
        public ErrorDto<List<AfiConsultaMovIngresos>> ConsultaMovIngresos_Obtener(int CodCliente, string cedula)
        {
            return BL_AF_ConsultaMov.ConsultaMovIngresos_Obtener(CodCliente, cedula);
        }

        [Authorize]
        [HttpGet("ConsultaMovRenuncias_Obtener")]
        public ErrorDto<List<AfiConsultaMovRenuncias>> ConsultaMovRenuncias_Obtener(int CodCliente, string cedula)
        {
            return BL_AF_ConsultaMov.ConsultaMovRenuncias_Obtener(CodCliente, cedula);
        }

        [Authorize]
        [HttpGet("ConsultaMovLiquidaciones_Obtener")]
        public ErrorDto<List<AfiConsultaMovLiquidaciones>> ConsultaMovLiquidaciones_Obtener(int CodCliente, string cedula)
        {
            return BL_AF_ConsultaMov.ConsultaMovLiquidaciones_Obtener(CodCliente, cedula);
        }

        [Authorize]
        [HttpPost("AF_MovLiquidaciones_Reversion")]
        public ErrorDto AF_MovLiquidaciones_Reversion(int CodEmpresa, string usuario, string idLiquidacion)
        {
            return BL_AF_ConsultaMov.AF_MovLiquidaciones_Reversion(CodEmpresa, usuario, idLiquidacion);
        }

        [Authorize]
        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return BL_AF_ConsultaMov.FechaServidor_Obtener(CodEmpresa);
        }
    }
}