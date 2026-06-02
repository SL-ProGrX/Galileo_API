using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCRDeteccionFraudesController : ControllerBase
    {
        private readonly FrmCRDeteccionFraudesBL BL;

        public FrmCRDeteccionFraudesController(IConfiguration config)
        {
            BL = new FrmCRDeteccionFraudesBL(config);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Operaciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Operaciones_Dropdown_Obtener(int CodEmpresa)
        {
            return FrmCRDeteccionFraudesBL.CR_DeteccionFraudes_Operaciones_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Personas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Personas_Dropdown_Obtener(int CodEmpresa)
        {
            return FrmCRDeteccionFraudesBL.CR_DeteccionFraudes_Personas_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Garantias_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return FrmCRDeteccionFraudesBL.CR_DeteccionFraudes_Garantias_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_DeteccionFraudes_Usuarios_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_DeteccionFraudes_Comites_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Recursos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Recursos_Dropdown_Obtener(int CodEmpresa,string? codigo,bool todasLineas)
        {
            return BL.CR_DeteccionFraudes_Recursos_Dropdown_Obtener(CodEmpresa,codigo,todasLineas);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Destinos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Destinos_Dropdown_Obtener(int CodEmpresa,string? codigo,bool todasLineas)
        {
            return BL.CR_DeteccionFraudes_Destinos_Dropdown_Obtener(CodEmpresa,codigo,todasLineas);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Linea_Descripcion_Obtener")]
        public ErrorDto<CrDeteccionFraudesLineaDescripcionDto> CR_DeteccionFraudes_Linea_Descripcion_Obtener(int CodEmpresa,string? codigo)
        {
            return BL.CR_DeteccionFraudes_Linea_Descripcion_Obtener(CodEmpresa,codigo);
        }
        [Authorize]
        [HttpGet("CR_DeteccionFraudes_Lineas_F4_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Lineas_F4_Obtener(int CodEmpresa,string? filtro)
        {
            return BL.CR_DeteccionFraudes_Lineas_F4_Obtener(CodEmpresa,filtro);
        }
        [Authorize]
        [HttpPost("CR_DeteccionFraudes_PrepararReporte")]
        public ErrorDto CR_DeteccionFraudes_PrepararReporte(int CodEmpresa,[FromBody] CrDeteccionFraudesReporteRequest request)
        {
            return BL.CR_DeteccionFraudes_PrepararReporte(CodEmpresa,request);
        }
    }
}