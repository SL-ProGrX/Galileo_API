
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxPPlantillasController : ControllerBase
    {
        private readonly FrmCxPPlantillasBL _bl;

        public FrmCxPPlantillasController(IConfiguration config)
        {
            _bl = new FrmCxPPlantillasBL(config);
        }

        [HttpGet("Plantillas_Obtener")]
        public ErrorDto<List<PlantillaDto>> Plantillas_Obtener(int CodEmpresa)
        {
            return _bl.Plantillas_Obtener(CodEmpresa);
        }

        [HttpGet("Unidades_Obtener")]
        public ErrorDto<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            return _bl.Unidades_Obtener(CodEmpresa);
        }

        [HttpGet("CentrosCosto_Obtener")]
        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            return _bl.CentrosCosto_Obtener(CodEmpresa, Cod_Unidad);
        }

        [HttpGet("PlantillaDetalle_Obtener")]
        public ErrorDto<PlantillaDto> PlantillaDetalle_Obtener(int CodEmpresa, string Cod_Plantilla)
        {
            return _bl.PlantillaDetalle_Obtener(CodEmpresa, Cod_Plantilla);
        }

        [HttpGet("PlantillaDetalle_Scroll")]
        public ErrorDto<PlantillaDto> PlantillaDetalle_Scroll(int CodEmpresa, int scroll, string Cod_Plantilla)
        {
            return _bl.PlantillaDetalle_Scroll(CodEmpresa, scroll, Cod_Plantilla);
        }

        [HttpGet("PlantillaAsientos_Obtener")]
        public ErrorDto<List<PlantillaAsientoDto>> PlantillaAsientos_Obtener(int CodEmpresa, string Cod_Plantilla)
        {
            return _bl.PlantillaAsientos_Obtener(CodEmpresa, Cod_Plantilla);
        }

        [HttpPost("Plantilla_Actualizar")]
        public ErrorDto Plantilla_Actualizar(int CodEmpresa, PlantillaDto data)
        {
            return _bl.Plantilla_Actualizar(CodEmpresa, data);
        }

        [HttpPost("Plantilla_Insertar")]
        public ErrorDto Plantilla_Insertar(int CodEmpresa, PlantillaDto data)
        {
            return _bl.Plantilla_Insertar(CodEmpresa, data);
        }

        [HttpPost("PlantillaAsiento_Insertar")]
        public ErrorDto PlantillaAsiento_Insertar(int CodEmpresa, PlantillaAsientoDto data)
        {
            return _bl.PlantillaAsiento_Insertar(CodEmpresa, data);
        }

        [HttpPost("PlantillaAsiento_Actualizar")]
        public ErrorDto PlantillaAsiento_Actualizar(int CodEmpresa, PlantillaAsientoDto data)
        {
            return _bl.PlantillaAsiento_Actualizar(CodEmpresa, data);
        }

        [HttpPost("PlantillaAsiento_Borrar")]
        public ErrorDto PlantillaAsiento_Borrar(int CodEmpresa, PlantillaAsientoDto data)
        {
            return _bl.PlantillaAsiento_Borrar(CodEmpresa, data);
        }

        [HttpPost("Plantilla_Borrar")]
        public ErrorDto Plantilla_Borrar(int CodEmpresa, string Cod_Plantilla)
        {
            return _bl.Plantilla_Borrar(CodEmpresa, Cod_Plantilla);
        }
    }
}