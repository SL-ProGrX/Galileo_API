using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrx_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfNoticaNoCotizantesController : ControllerBase
    {
        private readonly FrmAfNoticaNoCotizantesBl BlAfNoticaNoCotizantes;

        public FrmAfNoticaNoCotizantesController(IConfiguration config)
        {
            BlAfNoticaNoCotizantes = new FrmAfNoticaNoCotizantesBl(config);
        }

        [Authorize]
        [HttpGet("AF_NoticaNoCotizantes_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_NoticaNoCotizantes_Instituciones_Obtener(int CodEmpresa)
        {
            return BlAfNoticaNoCotizantes.AF_NoticaNoCotizantes_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_NoticaNoCotizantes_Rangos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_NoticaNoCotizantes_Rangos_Obtener(int CodEmpresa)
        {
            return BlAfNoticaNoCotizantes.AF_NoticaNoCotizantes_Rangos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_NoticaNoCotizantes_Consulta_Obtener")]
        public ErrorDto<List<AfAsociadosSinAportesDto>> AF_NoticaNoCotizantes_Consulta_Obtener(int CodEmpresa, string Filtros)
        {
            return BlAfNoticaNoCotizantes.AF_NoticaNoCotizantes_Consulta_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpPost("AF_NoticaNoCotizantes_Estadistica_Actualizar")]
        public ErrorDto AF_NoticaNoCotizantes_Estadistica_Actualizar(int CodEmpresa)
        {
            return BlAfNoticaNoCotizantes.AF_NoticaNoCotizantes_Estadistica_Actualizar(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_NoticaNoCotizantes_Asociados_Notificar")]
        public ErrorDto AF_NoticaNoCotizantes_Asociados_Notificar(int CodEmpresa, List<AfAsociadosSinAportesDto> Lista, int Aviso, string Usuario)
        {
            return BlAfNoticaNoCotizantes.AF_NoticaNoCotizantes_Asociados_Notificar(CodEmpresa, Lista, Aviso, Usuario);
        }
    }
}
