using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndBeneficiariosContratosController : ControllerBase
    {
        private readonly FrmFndBeneficiariosContratosBL _BL;

        public FrmFndBeneficiariosContratosController(IConfiguration? config)
        {
            _BL = new FrmFndBeneficiariosContratosBL(config);
        }

        [Authorize]
        [HttpGet("FND_Beneficiarios_Contratos_Lista_Obtener")]
        public ErrorDto<List<FndBeneficiariosContratosData>> FND_Beneficiarios_Contratos_Lista_Obtener(int CodEmpresa, string cedula, int operadora, string plan, long contrato)
        {
            return _BL.FND_Beneficiarios_Contratos_Lista_Obtener(CodEmpresa, cedula, operadora, plan, contrato);
        }

        [Authorize]
        [HttpGet("FND_Beneficiarios_Contratos_Parentescos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Beneficiarios_Contratos_Parentescos_Obtener(int CodEmpresa)
        {
            return _BL.FND_Beneficiarios_Contratos_Parentescos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpDelete("FNDBeneficiarios_Contratos_Eliminar")]
        public ErrorDto FNDBeneficiarios_Contratos_Eliminar(int CodEmpresa, int consec, string usuario)
        {
            return _BL.FNDBeneficiarios_Contratos_Borrar(CodEmpresa, consec, usuario);
        }

        [Authorize]
        [HttpPost("FND_Beneficiarios_Contratos_Guardar")]
        public ErrorDto FND_Beneficiarios_Contratos_Guardar(int CodEmpresa, string usuario, FndBeneficiariosContratosData beneficiario)
        {
            return _BL.FND_Beneficiarios_Contratos_Guardar(CodEmpresa, usuario, beneficiario);
        }

        [Authorize]
        [HttpGet("FNDBene_Cnt_CedulaBN_Obtener")]
        public ErrorDto<string> FNDBene_Cnt_CedulaBN_Obtener(int CodEmpresa, string cedula, string plan, long contrato, int operadora)
        {
            return _BL.FNDBene_Cnt_CedulaBN_Obtener(CodEmpresa, cedula, plan, contrato, operadora);
        }
    }
}