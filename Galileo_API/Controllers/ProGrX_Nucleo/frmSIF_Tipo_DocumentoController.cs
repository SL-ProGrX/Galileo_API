using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmSifTipoDocumentoController : ControllerBase
    {
        private readonly FrmSifTipoDocumentoBL _bl;
        public FrmSifTipoDocumentoController(IConfiguration config)
        {
            _bl = new FrmSifTipoDocumentoBL(config);
        }


        [Authorize]
        [HttpGet("SIF_tipoDocumento_Consultar")]
        public ErrorDto<string> SIF_tipoDocumento_Consultar(int CodEmpresa, int orden, string tipoDocumento = "")
        {
            return _bl.SIF_tipoDocumento_Consultar(CodEmpresa, tipoDocumento, orden);
        }
         
        [Authorize]
        [HttpGet("SIF_tipoDocumentoData_Consultar")]
        public ErrorDto<SifTipoDocumentoData> SIF_tipoDocumentoData_Consultar(int CodEmpresa, string tipoDocumento)
        {
            return _bl.SIF_tipoDocumentoData_Consultar(CodEmpresa, tipoDocumento);
        }

        [Authorize]
        [HttpGet("SIF_tipoDocumento_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> SIF_tipoDocumento_Obtener(int CodEmpresa)
        {
            return _bl.SIF_tipoDocumento_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("SIF_tipoDocumento_Guardar")]
        public ErrorDto SIF_tipoDocumento_Guardar(int CodEmpresa, string usuario, SifTipoDocumentoData tipoDoc, int accion)
        {
            return _bl.SIF_tipoDocumento_Guardar(CodEmpresa, usuario, tipoDoc, accion);
        }

        [Authorize]
        [HttpGet("SIF_TipoDocumentosConceptosRelacionados_Obtener")]
        public ErrorDto<List<SifTipoDocConceptoData>> SIF_TipoDocumentosConceptosRelacionados_Obtener(int CodEmpresa, string tipoDoc)
        {
            return _bl.SIF_TipoDocumentosConceptosRelacionados_Obtener(CodEmpresa, tipoDoc);
        }

        [Authorize]
        [HttpPost("SIF_TipoDocumentosConceptosRelacionados_Guardar")]
        public ErrorDto SIF_TipoDocumentosConceptosRelacionados_Guardar(int CodEmpresa, string usuario, string cod_concepto, string tipoDoc, string accion)
        {
            return _bl.SIF_TipoDocumentosConceptosRelacionados_Guardar(CodEmpresa, usuario, cod_concepto, tipoDoc, accion);
        }

    }
}