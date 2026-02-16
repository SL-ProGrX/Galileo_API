using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPolizaAsociadosController : ControllerBase
    {
        private readonly FrmPolizaAsociadosBL _bl;

        public FrmPolizaAsociadosController(IConfiguration config)
        {
            _bl = new FrmPolizaAsociadosBL(config);
        }

        [HttpGet("Poliza_AsociadoCatalogo_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_AsociadoCatalogo_Listar(int CodEmpresa, string tipo = "PA")
        {
            return _bl.Poliza_AsociadoCatalogo_Listar(CodEmpresa, tipo);
        }

        [HttpGet("Poliza_Asociados_Corte_Listar")]
        public ErrorDto<List<PolizaAsociadoCorteSpDto>> Poliza_Asociados_Corte_Listar(
        int CodEmpresa,
        string Usuario,
        DateTime FechaCorte,
        string? Tipo)
        {
            return _bl.Poliza_Asociados_Corte_Listar(CodEmpresa, Usuario, FechaCorte, Tipo);
        }

        [HttpGet("Poliza_Beneficiarios_Listar")]
        public ErrorDto<List<PolizaBeneficiariosSpDto>> Poliza_Beneficiarios_Listar(
          int CodEmpresa,
          string CodPoliza)
        {
            return _bl.Poliza_Beneficiarios_Listar(CodEmpresa, CodPoliza);
        }

    }
}
