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
    public class FrmPolizaReclamoInformesController : ControllerBase
    {
        private readonly FrmPolizaReclamoInformesBL _bl;

        public FrmPolizaReclamoInformesController(IConfiguration config)
        {
            _bl = new FrmPolizaReclamoInformesBL(config);
        }

        [HttpGet("fxFechaServidor")]
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _bl.fxFechaServidor(codEmpresa);
        }

        [HttpGet("Poliza_Reclamo_Informes_Polizas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Polizas_Lista(int CodEmpresa)
        {
            return _bl.Poliza_Reclamo_Informes_Polizas_Lista(CodEmpresa);
        }

        [HttpGet("Poliza_Reclamo_Informes_Estados_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Estados_Lista(int CodEmpresa)
        {
            return _bl.Poliza_Reclamo_Informes_Estados_Lista(CodEmpresa);
        }

        [HttpGet("Poliza_Reclamo_Informes_Motivos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Motivos_Lista(int CodEmpresa, string codPoliza)
        {
            return _bl.Poliza_Reclamo_Informes_Motivos_Lista(CodEmpresa, codPoliza);
        }

        [HttpGet("Poliza_Reclamo_Informes_Causas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>>
        Poliza_Reclamo_Informes_Causas_Lista(int CodEmpresa, string codPoliza)
        {
            return _bl.Poliza_Reclamo_Informes_Causas_Lista(CodEmpresa, codPoliza);
        }

        [HttpPost("Poliza_Reclamo_Informes_Preparar_Filtros")]
        public ErrorDto Poliza_Reclamo_Informes_Preparar_Filtros(
          int CodEmpresa, string usuario, PolizaReclamoInformesPrepararFiltrosRequest request)
        {
            return _bl.Poliza_Reclamo_Informes_Preparar_Filtros(CodEmpresa, usuario, request);
        }
    }
}
