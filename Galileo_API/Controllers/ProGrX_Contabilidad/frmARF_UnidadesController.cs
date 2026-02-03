using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmArfUnidadesController : ControllerBase
    {
        private readonly FrmArfUnidadesBl _bl;

        public FrmArfUnidadesController(IConfiguration config) => _bl = new FrmArfUnidadesBl(config);
        
        [HttpGet("ArfUnidades_Provincias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Provincias_Obtener(int codEmpresa)
        {
            return _bl.ArfUnidades_Provincias_Obtener(codEmpresa);
        }

        [HttpGet("ArfUnidades_Cantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Cantones_Obtener(int codEmpresa, string codProvincia)
        {
            return _bl.ArfUnidades_Cantones_Obtener(codEmpresa, codProvincia);
        }

        [HttpGet("ArfUnidades_Distritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Distritos_Obtener(int codEmpresa, string codProvincia, string codCanton)
        {
            return _bl.ArfUnidades_Distritos_Obtener(codEmpresa, codProvincia, codCanton);
        }

        [HttpGet("ArfUnidades_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Unidades_Obtener(int codEmpresa)
        {
            return _bl.ArfUnidades_Unidades_Obtener(codEmpresa);
        }

        [HttpGet("ArfUnidades_Scroll_Obtener")]
        public ErrorDto<ArfUnidadesData> ArfUnidades_Scroll_Obtener(int codEmpresa, string codUnidad, int scrollCode)
        {
            return _bl.ArfUnidades_Scroll_Obtener(codEmpresa, codUnidad, scrollCode);
        }

        [HttpGet("ArfUnidades_ConsultaUnidad_Obtener")]
        public ErrorDto<ArfUnidadesData> ArfUnidades_ConsultaUnidad_Obtener(int codEmpresa, string codUnidad)
        {
            return _bl.ArfUnidades_ConsultaUnidad_Obtener(codEmpresa, codUnidad);
        }
    }
}