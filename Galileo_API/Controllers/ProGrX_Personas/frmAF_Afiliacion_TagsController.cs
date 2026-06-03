using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfAfiliacionTagsController : ControllerBase
    {
        private readonly FrmAFAfiliacionTagsBL _bl;

        public FrmAfAfiliacionTagsController(IConfiguration config)
        {
            _bl = new FrmAFAfiliacionTagsBL(config);
        }

        [Authorize]
        [HttpGet("AFI_Afiliaciones_Consulta_Recepcion")]
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recepcion(int CodEmpresa, string estado, string filtro)
        {
            return _bl.AFI_Afiliaciones_Consulta_Recepcion(CodEmpresa, estado, filtro);
        }

        [Authorize]
        [HttpGet("AFI_Afiliaciones_Consulta_Recibidas")]
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recibidas(int CodEmpresa, string estado, string filtro)
        {
            return _bl.AFI_Afiliaciones_Consulta_Recibidas(CodEmpresa, estado, filtro);
        }

        [Authorize]
        [HttpGet("AFI_Afiliaciones_Consulta_Pendientes")]
        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Pendientes(int CodEmpresa, string estado, string filtro)
        {
            return _bl.AFI_Afiliaciones_Consulta_Pendientes(CodEmpresa, estado, filtro);
        }

        [Authorize]
        [HttpGet("AF_CR_BoletasAfiliacion_Obtener")]
        public ErrorDto<List<AfBoletasAfiliacion>> AF_CR_BoletasAfiliacion_Obtener(int CodEmpresa)
        {
            return _bl.AF_CR_BoletasAfiliacion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AFI_Afiliacion_Recepcion_Aplica")]
        public ErrorDto AFI_Afiliacion_Recepcion_Aplica(int CodEmpresa, int boleta, string usuario)
        {
            return _bl.AFI_Afiliacion_Recepcion_Aplica(CodEmpresa, boleta, usuario);
        }


        [Authorize]
        [HttpPost("AFI_Afiliacion_Revision_Aplica")]
        public ErrorDto AFI_Afiliacion_Revision_Aplica(int codEmpresa, int consec, string estado, string usuario, string? nota)
        {
            return _bl.AFI_Afiliacion_Revision_Aplica(codEmpresa, consec, estado, usuario, nota ?? string.Empty);
        }

        [Authorize]
        [HttpGet("AFI_Afiliaciones_Etiquetas_Consulta")]
        public ErrorDto<List<AfiEtiquetaDto>> AFI_Afiliaciones_Etiquetas_Consulta(int CodEmpresa, int boleta)
        {
            return _bl.AFI_Afiliaciones_Etiquetas_Consulta(CodEmpresa, boleta);
        }

        [Authorize]
        [HttpPost("AFI_Afiliacion_Revision_Reversar")]
        public ErrorDto AFI_Afiliacion_Revision_Reversar(int CodEmpresa, int boleta, string usuario, string nota)
        {
            return _bl.AFI_Afiliacion_Revision_Reversar(CodEmpresa, boleta, usuario, nota);
        }

        [Authorize]
        [HttpPost("AFI_Afiliacion_Recepcion_Agregar")]
        public ErrorDto AFI_Afiliacion_Recepcion_Agregar(int CodEmpresa, int boleta, string usuario)
        {
            return _bl.AFI_Afiliacion_Recepcion_Agregar(CodEmpresa, boleta, usuario);
        }

        [Authorize]
        [HttpGet("AF_BoletasAfiliacionLista_Obtener")]
        public ErrorDto<List<AfBoletasAfiliacion>> AF_BoletasAfiliacionLista_Obtener(int CodEmpresa)
        {
            return _bl.AF_BoletasAfiliacionLista_Obtener(CodEmpresa);
        }
    }
}
