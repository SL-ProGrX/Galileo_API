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
    public class FrmCrPolizasSicamaController : ControllerBase
    {
        private readonly FrmCrPolizasSicamaBl _bl;

        public FrmCrPolizasSicamaController(IConfiguration config)
        {
            _bl = new FrmCrPolizasSicamaBl(config);
        }

        [HttpGet("Cr_PolizasSicama_Polizas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizasSicama_Polizas_Lista(int CodEmpresa)
        {
            return _bl.Cr_PolizasSicama_Polizas_Lista(CodEmpresa);
        }

        [HttpGet("fxFechaServidor")]
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _bl.fxFechaServidor(codEmpresa);
        }

        [HttpPost("Cr_PolizasSicama_Envio_Consulta")]
        public ErrorDto<List<CrPolizasSicamaEnvioRow>> Cr_PolizasSicama_Envio_Consulta(
         int CodEmpresa,
         string Usuario,
         CrPolizasSicamaEnvioConsultaRequest request)
        {
            return _bl.Cr_PolizasSicama_Envio_Consulta(CodEmpresa, Usuario, request);
        }

        [HttpPost("Cr_PolizasSicama_Consulta_Obtener")]
        public ErrorDto<List<CrPolizasSicamaEnvioRow>> Cr_PolizasSicama_Consulta_Obtener(
           int CodEmpresa,
           string Usuario,
           CrPolizasSicamaEnvioConsultaRequest request)
        {
            return _bl.Cr_PolizasSicama_Consulta_Obtener(CodEmpresa, Usuario, request);
        }

        [HttpGet("Cr_PolizasSicama_Beneficiarios_Lista")]
        public ErrorDto<List<CrPolizasSicamaBeneficiariosRowDto>>
              Cr_PolizasSicama_Beneficiarios_Lista(
              int CodEmpresa,
              string Usuario,
              string poliza)
        {
            return _bl.Cr_PolizasSicama_Beneficiarios_Lista(CodEmpresa, Usuario, poliza);
        }

        [HttpPost("Cr_PolizasSicama_PlanillaDirecta_Sube")]
        public ErrorDto Cr_FndPlanillaDirecta_Sube(
          int CodEmpresa,
          string Usuario,
          CrFndPlanillaDirectaSubeRequest request)
        {
            return _bl.Cr_FndPlanillaDirecta_Sube(CodEmpresa, Usuario, request);
        }

        [HttpPost("Cr_PolizasSicama_PlanillaDirecta_Consulta")]
        public ErrorDto<List<CrFndPlanillaDirectaConsultaRowDto>>
            Cr_FndPlanillaDirecta_Consulta(
            int CodEmpresa,
            string Usuario,
            CrFndPlanillaDirectaConsultaRequest request)
        {
            return _bl.Cr_FndPlanillaDirecta_Consulta(CodEmpresa, Usuario, request);
        }
    }
}
