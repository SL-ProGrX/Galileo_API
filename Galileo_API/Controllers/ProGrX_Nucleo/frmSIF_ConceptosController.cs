using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifConceptosController : ControllerBase
    {
        private readonly FrmSifConceptosBL _bl;
        public FrmSifConceptosController(IConfiguration config)
        {
            _bl = new FrmSifConceptosBL(config);
        }

        [Authorize]
        [HttpGet("Sif_ConceptosLista_Obtener")]
        public ErrorDto<SifConceptoLista> Sif_ConceptosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_ConceptosLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sif_Conceptos_Obtener")]
        public ErrorDto<List<SifConceptoData>> Sif_Conceptos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_Conceptos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Sif_Conceptos_Guardar")]
        public ErrorDto Sif_Conceptos_Guardar(int CodEmpresa, string usuario, SifConceptoData concepto)
        {
            return _bl.SIF_Conceptos_Guardar(CodEmpresa, usuario, concepto);
        }

        [Authorize]
        [HttpDelete("Sif_Conceptos_Eliminar")]
        public ErrorDto Sif_Conceptos_Eliminar(int CodEmpresa, string usuario, string concepto)
        {
            return _bl.SIF_Conceptos_Eliminar(CodEmpresa, usuario, concepto);
        }

        [Authorize]
        [HttpGet("Sif_Conceptos_Valida")]
        public ErrorDto Sif_Conceptos_Valida(int CodEmpresa, string cod_concepto)
        {
            return _bl.SIF_Conceptos_Valida(CodEmpresa, cod_concepto);
        }

        [Authorize]
        [HttpGet("Sif_ConceptosDocumentos_Obtener")]
        public ErrorDto<List<SifConceptoDocumentoData>> Sif_ConceptosDocumentos_Obtener(int CodEmpresa, string cod_concepto)
        {
            return _bl.SIF_ConceptosDocumentos_Obtener(CodEmpresa, cod_concepto);
        }

        [Authorize]
        [HttpPost("Sif_ConceptosDocumentos_Asociar")]
        public ErrorDto Sif_ConceptosDocumentos_Asociar(int CodEmpresa, string usuario, string cod_concepto, string tipo_documento)
        {
            return _bl.SIF_ConceptosDocumentos_Asociar(CodEmpresa, usuario, cod_concepto, tipo_documento);
        }

        [Authorize]
        [HttpDelete("Sif_ConceptosDocumentos_Desasociar")]
        public ErrorDto Sif_ConceptosDocumentos_Desasociar(int CodEmpresa, string usuario, string cod_concepto, string tipo_documento)
        {
            return _bl.SIF_ConceptosDocumentos_Desasociar(CodEmpresa, usuario, cod_concepto, tipo_documento);
        }

    }
}