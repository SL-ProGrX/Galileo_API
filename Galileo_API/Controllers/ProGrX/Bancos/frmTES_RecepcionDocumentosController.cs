using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesRecepcionDocumentosController : ControllerBase
    {
        private readonly FrmTesRecepcionDocumentosBL RecepcionDocumentosBL;

        public FrmTesRecepcionDocumentosController(IConfiguration config)
        {
            RecepcionDocumentosBL = new FrmTesRecepcionDocumentosBL(config);
        }

        [HttpGet("TES_RecepcionDoc_Ubicaciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_RecepcionDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario)
        {
            return RecepcionDocumentosBL.TES_RecepcionDoc_Ubicaciones_Obtener(CodEmpresa, Usuario);
        }

        [HttpGet("TES_RecepcionDoc_Remesa_Scroll_Obtener")]
        public ErrorDto<TesUbiRemesaDto> TES_RecepcionDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
        {
            return RecepcionDocumentosBL.TES_RecepcionDoc_Remesa_Scroll_Obtener(CodEmpresa, scrollCode, Remesa);
        }

        [HttpGet("TES_RecepcionDoc_Remesa_Obtener")]
        public ErrorDto<TesUbiRemesaDto> TES_RecepcionDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            return RecepcionDocumentosBL.TES_RecepcionDoc_Remesa_Obtener(CodEmpresa, Remesa);
        }

        [HttpGet("TES_RecepcionDocumentos_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_RecepcionDocumentos_Obtener(int CodEmpresa, int Remesa, string filtros)
        {
            return RecepcionDocumentosBL.TES_RecepcionDocumentos_Obtener(CodEmpresa, Remesa, filtros);
        }

        [HttpPost("TES_RecepcionDocumentos_Aplicar")]
        public ErrorDto TES_RecepcionDocumentos_Aplicar(int CodEmpresa, string parametros)
        {
            return RecepcionDocumentosBL.TES_RecepcionDocumentos_Aplicar(CodEmpresa, parametros);
        }
    }
}