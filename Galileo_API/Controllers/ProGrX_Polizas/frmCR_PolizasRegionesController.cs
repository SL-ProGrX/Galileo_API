using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizasRegionesController : ControllerBase
    {
        private readonly FrmCrPolizasRegionesBL _dl;

        public FrmCrPolizasRegionesController(IConfiguration config)
        {
            _dl = new FrmCrPolizasRegionesBL(config);
        }

        [HttpGet("Crd_Polizas_Region_Obtener")]
        public ErrorDto<List<CrdPolizasRegionDto>> Crd_Polizas_Region_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _dl.Crd_Polizas_Region_Obtener(CodEmpresa, cod_poliza);
        }

        [HttpPost("Crd_Polizas_Region_Guardar")]
        public ErrorDto Crd_Polizas_Region_Guardar(int CodEmpresa, string usuario, CrdPolizasRegionGuardarDto dto)
        {
            return _dl.Crd_Polizas_Region_Guardar(CodEmpresa, usuario, dto);
        }

        [HttpDelete("Crd_Polizas_Region_Eliminar")]
        public ErrorDto Crd_Polizas_Region_Eliminar(int CodEmpresa, string cod_poliza, int cod_region)
        {
            return _dl.Crd_Polizas_Region_Eliminar(CodEmpresa, cod_poliza, cod_region);
        }

        [HttpGet("Crd_Polizas_RegionLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Polizas_RegionLista_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _dl.Crd_Polizas_RegionLista_Obtener(CodEmpresa, cod_poliza);
        }

        [HttpGet("Crd_Provincias_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Provincias_Listar(int CodEmpresa)
        {
            return _dl.Crd_Provincias_Listar(CodEmpresa);
        }

        [HttpGet("Crd_Polizas_Region_Cantones_Listar")]
        public ErrorDto<List<CrdPolizasRegionCantonDto>> Crd_Polizas_Region_Cantones_Listar(
                int CodEmpresa,
                string cod_poliza,
                int cod_region,
                string? provincia,
                CrdCantonesModo modo)
        {
            return _dl.Crd_Polizas_Region_Cantones_Listar(CodEmpresa, cod_poliza, cod_region, provincia, modo);
        }

        [HttpPost("Crd_Polizas_Region_Canton_Asignar")]
        public ErrorDto Crd_Polizas_Region_Canton_Asignar(int CodEmpresa, string usuario, CrdPolizasRegionAsignarCantonDto dto)
        {
            return _dl.Crd_Polizas_Region_Canton_Asignar(CodEmpresa, usuario, dto);
        }
    }
}
