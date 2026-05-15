using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.ProGrX.Fondos;
using Galileo.BusinessLogic.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndRetencionConceptosController : ControllerBase
    {
        private readonly FrmFndRetencionConceptosBL _bl;

        public FrmFndRetencionConceptosController(IConfiguration config)
        {
            _bl = new FrmFndRetencionConceptosBL(config);
        }

        [Authorize]
        [HttpGet("FND_RetencionConceptosLista_Obtener")]
        public ErrorDto<FndRetencionConceptoLista> FND_RetencionConceptosLista_Obtener(int CodEmpresa, string enlace, string filtros)
        {
            return _bl.FND_RetencionConceptosLista_Obtener(CodEmpresa, enlace, filtros);
        }

        [Authorize]
        [HttpGet("FND_RetencionConceptos_Obtener")]
        public ErrorDto<List<FndRetencionConceptoData>> FND_RetencionConceptos_Obtener(int CodEmpresa, string enlace, string filtros)
        {
            return _bl.FND_RetencionConceptos_Obtener(CodEmpresa, enlace, filtros);
        }

        [Authorize]
        [HttpPost("FND_RetencionConceptos_Guardar")]
        public ErrorDto FND_RetencionConceptos_Guardar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            return _bl.FND_RetencionConceptos_Guardar(CodEmpresa, usuario, concepto);
        }

        [Authorize]
        [HttpDelete("FND_RetencionConceptos_Eliminar")]
        public ErrorDto FND_RetencionConceptos_Eliminar(int CodEmpresa, string usuario, string retencionCodigo)
        {
            return _bl.FND_RetencionConceptos_Eliminar(CodEmpresa, usuario, retencionCodigo);
        }

        [Authorize]
        [HttpGet("FND_RetencionConceptos_Valida")]
        public ErrorDto FND_RetencionConceptos_Valida(int CodEmpresa, string retencionCodigo)
        {
            return _bl.FND_RetencionConceptos_Valida(CodEmpresa, retencionCodigo);
        }
    }
}