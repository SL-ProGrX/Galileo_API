using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX.Bancos;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesDocumentosController : ControllerBase
    {
        private readonly FrmTesDocumentosBL _Bl;

        public FrmTesDocumentosController(IConfiguration config)
        {
            _Bl = new FrmTesDocumentosBL(config);
        }

        [HttpGet("Tes_DocumentosLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_DocumentosLista_Obtener(int CodEmpresa)
        {
            return _Bl.TES_DocumentosLista_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_Documentos_Scroll")]
        public ErrorDto<TesTiposDocDto> Tes_Documentos_Scroll(int CodEmpresa, string tipo, int? scroll)
        {
            return _Bl.Tes_Documentos_Scroll(CodEmpresa, tipo, scroll);
        }

        [HttpGet("Tes_Documentos_Obtener")]
        public ErrorDto<TesTiposDocDto> Tes_Documentos_Obtener(int CodEmpresa, string tipo)
        {
            return _Bl.Tes_Documentos_Obtener(CodEmpresa, tipo);
        }

        [HttpGet("Tes_DocumentosTiposAsientos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_DocumentosTiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _Bl.TES_DocumentosTiposAsientos_Obtener(CodEmpresa, contabilidad);
        }

        [HttpGet("Tes_DocAnulaConceptos_Obtener")]
        public ErrorDto<List<TesDocAnulaConceptosData>> Tes_DocAnulaConceptos_Obtener(int CodEmpresa, string tipo)
        {
            return _Bl.TES_DocAnulaConceptos_Obtener(CodEmpresa, tipo);
        }

        [HttpGet("Tes_DocAnulaConceptos_Scroll")]
        public ErrorDto<DropDownListaGenericaModel> Tes_DocAnulaConceptos_Scroll(int CodEmpresa, string concepto, int? scroll)
        {
            return _Bl.Tes_DocAnulaConceptos_Scroll(CodEmpresa, concepto, scroll);
        }

        [HttpPost("Tes_Documentos_Guardar")]
        public ErrorDto Tes_Documentos_Guardar(int CodEmpresa, string usuario, TesTiposDocDto documento)
        {
            return _Bl.TES_Documentos_Guardar(CodEmpresa, usuario, documento);
        }

        [HttpDelete("Tes_Documentos_Eliminar")]
        public ErrorDto Tes_Documentos_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _Bl.TES_Documentos_Eliminar(CodEmpresa, tipo, usuario);
        }

        [HttpPost("TES_DocAnulaConcepto_Guardar")]
        public ErrorDto TES_DocAnulaConcepto_Guardar(int CodEmpresa, string usuario, string tipo, TesDocAnulaConceptosData concepto)
        {
            return _Bl.TES_DocAnulaConcepto_Guardar(CodEmpresa, usuario, tipo, concepto);
        }

        [HttpDelete("TES_DocAnulaConcepto_Eliminar")]
        public ErrorDto TES_DocAnulaConcepto_Eliminar(int CodEmpresa, int id_conceptos, string usuario)
        {
            return _Bl.TES_DocAnulaConcepto_Eliminar(CodEmpresa, id_conceptos, usuario);
        }
    }
}
