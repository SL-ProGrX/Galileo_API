using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesConceptosController : ControllerBase
    {
        private readonly FrmTesConceptosBL _conceptosBL;

        public FrmTesConceptosController(IConfiguration config)
        {
            _conceptosBL = new FrmTesConceptosBL(config);
        }

        [HttpGet("Tes_ConceptosLista_Obtener")]
        public ErrorDto<TesConceptosLista> Tes_ConceptosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _conceptosBL.Tes_ConceptosLista_Obtener(CodEmpresa, filtros);
        }
        [HttpPost("Tes_Conceptos_Guardar")]
        public ErrorDto Tes_Conceptos_Guardar(int CodEmpresa, string usuario, TesConceptosData concepto)
        {
            return _conceptosBL.Tes_Conceptos_Guardar(CodEmpresa, usuario, concepto);
        }

        [HttpDelete("Tes_Conceptos_Eliminar")]
        public ErrorDto Tes_Conceptos_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _conceptosBL.Tes_Conceptos_Eliminar(CodEmpresa, tipo, usuario);
        }

        [HttpGet("Tes_Conceptos_Obtener")]
        public ErrorDto<List<TesConceptosData>> Tes_Conceptos_Obtener(int CodEmpresa, string filtros)
        {
            return _conceptosBL.Tes_Conceptos_Obtener(CodEmpresa, filtros);
        }

        
        [HttpGet("Tes_Concepto_Valida")]
        public ErrorDto Tes_Concepto_Valida(int CodEmpresa, string codigo)
        {
            return _conceptosBL.Tes_Concepto_Valida(CodEmpresa, codigo);
        }

    }
}
