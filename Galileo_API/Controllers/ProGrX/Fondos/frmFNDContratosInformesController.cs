using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFNDContratosInformesController : ControllerBase
    {
        private readonly FrmFNDContratosInformesBL _bl;

        public FrmFNDContratosInformesController(IConfiguration? config)
        {
            _bl = new FrmFNDContratosInformesBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_ContratosInformes_Contrato_Obtener")]
        public ErrorDto<FndContratosInformesContrato> Fnd_ContratosInformes_Contrato_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string usuario)
        {
            return _bl.Fnd_ContratosInformes_Contrato_Obtener(CodEmpresa, operadora, plan, contrato, usuario);
        }

        [Authorize]
        [HttpPost("Fnd_ContratosInformes_Email_Enviar")]
        public ErrorDto<string> Fnd_ContratosInformes_Email_Enviar(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string usuario)
        {
            return _bl.Fnd_ContratosInformes_Email_Enviar(CodEmpresa, operadora, plan, contrato, usuario);
        }

        [Authorize]
        [HttpGet("Fnd_ContratosInformes_Retiros_Obtener")]
        public ErrorDto<FndContratosInformesLiquidacionesLista> Fnd_ContratosInformes_Retiros_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string filtros)
        {
            return _bl.Fnd_ContratosInformes_Retiros_Obtener(CodEmpresa, operadora, plan, contrato, filtros);
        }
    }
}
