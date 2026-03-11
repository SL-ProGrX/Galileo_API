using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPolizaReclamoController : ControllerBase
    {
       private readonly FrmPolizaReclamoBL _bl;
    
       public FrmPolizaReclamoController(IConfiguration config)
       {
            _bl = new FrmPolizaReclamoBL(config);
       }

        [HttpGet("Poliza_Reclamo_Motivos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Motivos_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _bl.Poliza_Reclamo_Motivos_Lista(codEmpresa, codPoliza);
        }

        [HttpGet("Poliza_Reclamo_Causas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Causas_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _bl.Poliza_Reclamo_Causas_Lista(codEmpresa, codPoliza);
        }

        [HttpGet("Poliza_Reclamo_Estados_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Estados_Lista(int codEmpresa)
        {
            return _bl.Poliza_Reclamo_Estados_Lista(codEmpresa);
        }

        [HttpGet("Poliza_Reclamo_Bancos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Bancos_Lista(
            int codEmpresa,
            string usuario)
        {
            return _bl.Poliza_Reclamo_Bancos_Lista(codEmpresa, usuario);
        }

        [HttpGet("Poliza_Reclamo_Cuentas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Cuentas_Lista(
            int codEmpresa,
            string cedula,
            int bancoId)
        {
            return _bl.Poliza_Reclamo_Cuentas_Lista(codEmpresa, cedula, bancoId);
        }

    }
}
