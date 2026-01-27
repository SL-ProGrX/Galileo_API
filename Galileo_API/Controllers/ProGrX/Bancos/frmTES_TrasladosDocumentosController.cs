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
    public class FrmTesTrasladosDocumentosController : ControllerBase
    {
        private readonly FrmTesTrasladosDocumentosBL TrasladosDocumentosBL;

        public FrmTesTrasladosDocumentosController(IConfiguration config)
        {
            TrasladosDocumentosBL = new FrmTesTrasladosDocumentosBL(config);
        }

        [HttpGet("TES_TrasladosDoc_Ubicaciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TrasladosDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario, string Tipo)
        {
            return TrasladosDocumentosBL.TES_TrasladosDoc_Ubicaciones_Obtener(CodEmpresa, Usuario, Tipo);
        }

        [HttpGet("TES_TrasladosDoc_Remesa_Scroll_Obtener")]
        public ErrorDto<TesUbiRemesaDto> TES_TrasladosDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
        {
            return TrasladosDocumentosBL.TES_TrasladosDoc_Remesa_Scroll_Obtener(CodEmpresa, scrollCode, Remesa);
        }

        [HttpGet("TES_TrasladosDoc_Remesa_Obtener")]
        public ErrorDto<TesUbiRemesaDto> TES_TrasladosDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            return TrasladosDocumentosBL.TES_TrasladosDoc_Remesa_Obtener(CodEmpresa, Remesa);
        }

        [HttpGet("TES_TrasladosDocumentos_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_TrasladosDocumentos_Obtener(int CodEmpresa, int Remesa, string filtros)
        {
            return TrasladosDocumentosBL.TES_TrasladosDocumentos_Obtener(CodEmpresa, Remesa, filtros);
        }

        [HttpGet("TES_TrasladosDoc_Solicitud_Obtener")]
        public ErrorDto<TesTrasladoDocumentoDto> TES_TrasladosDoc_Solicitud_Obtener(int CodEmpresa, int Solicitud)
        {
            return TrasladosDocumentosBL.TES_TrasladosDoc_Solicitud_Obtener(CodEmpresa, Solicitud);
        }

        [HttpPost("TES_TrasladosDocumentos_Guardar")]
        public ErrorDto TES_TrasladosDocumentos_Guardar(int CodEmpresa, bool vEdita, string Remesa)
        {
            return TrasladosDocumentosBL.TES_TrasladosDocumentos_Guardar(CodEmpresa, vEdita, Remesa);
        }

        [HttpDelete("TES_TrasladosDocumentos_Eliminar")]
        public ErrorDto TES_TrasladosDocumentos_Eliminar(int CodEmpresa, int Remesa, string Usuario)
        {
            return TrasladosDocumentosBL.TES_TrasladosDocumentos_Eliminar(CodEmpresa, Remesa, Usuario);
        }

        [HttpPost("TES_TrasladosDocumentos_Linea_Guardar")]
        public ErrorDto TES_TrasladosDocumentos_Linea_Guardar(int CodEmpresa, string Remesa, string Linea)
        {
            return TrasladosDocumentosBL.TES_TrasladosDocumentos_Linea_Guardar(CodEmpresa, Remesa, Linea);
        }

        [HttpDelete("TES_TrasladosDocumentos_Linea_Eliminar")]
        public ErrorDto TES_TrasladosDocumentos_Linea_Eliminar(int CodEmpresa, int Remesa, int Solicitud)
        {
            return TrasladosDocumentosBL.TES_TrasladosDocumentos_Linea_Eliminar(CodEmpresa, Remesa, Solicitud);
        }
    }
}