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
    public class FrmCrPolizaProcEnvioController : ControllerBase
    {
        private readonly FrmCrPolizaProcEnvioBl _bl;

        public FrmCrPolizaProcEnvioController(IConfiguration config)
        {
            _bl = new FrmCrPolizaProcEnvioBl(config);
        }

        [HttpGet("Crd_PolizasProcEnvio_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasProcEnvio_Catalogo_Obtener(int CodEmpresa)
        {
            return _bl.Crd_PolizasProcEnvio_Catalogo_Obtener(CodEmpresa);
        }

        [HttpPost("Crd_PolizasProcEnvio_GridMeta_Obtener")]
        public ErrorDto<CrdPolizaGridMetaResponseDto> Crd_PolizasProcEnvio_GridMeta_Obtener(int CodEmpresa, CrdPolizaGridMetaRequestDto req)
        {
            return _bl.Crd_PolizasProcEnvio_GridMeta_Obtener(CodEmpresa, req);
        }

        [HttpPost("Crd_PolizasProcEnvio_Consultar")]
        public ErrorDto<CrdPolizaConsultaResponseDto> Crd_PolizasProcEnvio_Consultar(
          int CodEmpresa,
          CrdPolizaConsultaRequestDto req)
        {
            return _bl.Crd_PolizasProcEnvio_Consultar(CodEmpresa, req);
        }

    }
}
