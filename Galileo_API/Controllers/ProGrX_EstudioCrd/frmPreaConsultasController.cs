
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using static Galileo_API.Models.ProGrX_EstudioCrd.FrmPreaConsultasModels;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class FrmPreaConsultasController : ControllerBase
    {
        private readonly FrmPreaConsultasBL _bl;

        public FrmPreaConsultasController(IConfiguration config) =>
            _bl = new FrmPreaConsultasBL(config);

        [HttpGet("PreaConsultas_Catalogos_Obtener")]
        public ErrorDto<PreaConsultasCatalogosResponse> PreaConsultas_Catalogos_Obtener(int codEmpresa)
        {
            return _bl.PreaConsultas_Catalogos_Obtener(codEmpresa);
        }
        
        [HttpPost("PreaConsultas_Grid_Obtener")]
        public ErrorDto<ConsultaLista> PreaConsultas_Grid_Obtener(int codEmpresa, bool esExportar, [FromBody] PreaConsultasFiltroRequest request)
        {
            return _bl.PreaConsultas_Grid_Obtener(codEmpresa, esExportar, request);
        }
        [HttpPost("PreaConsultas_Resumen_Obtener")]
        public ErrorDto<List<PreaConsultasResumenModel>> PreaConsultas_Resumen_Obtener(int codEmpresa, [FromBody] PreaConsultasFiltroRequest request)
        {
            return _bl.PreaConsultas_Resumen_Obtener(codEmpresa, request);
        }
        [HttpGet("PreaConsultas_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Usuarios_Obtener(int codEmpresa)
        {
            return _bl.PreaConsultas_Usuarios_Obtener(codEmpresa);
        }
        [HttpGet("PreaConsultas_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Lineas_Obtener(int codEmpresa)
        {
            return _bl.PreaConsultas_Lineas_Obtener(codEmpresa);
        }
        [HttpGet("PreaConsultas_Destinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Destinos_Obtener(int codEmpresa)
        {
            return _bl.PreaConsultas_Destinos_Obtener(codEmpresa);
        }
        [HttpGet("PreaConsultas_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Instituciones_Obtener(int codEmpresa)
        {
            return _bl.PreaConsultas_Instituciones_Obtener(codEmpresa);
        }
        [HttpGet("PreaConsultas_Comites_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Comites_Obtener(int codEmpresa)
        {
            return _bl.PreaConsultas_Comites_Obtener(codEmpresa);
        }

    }
}
