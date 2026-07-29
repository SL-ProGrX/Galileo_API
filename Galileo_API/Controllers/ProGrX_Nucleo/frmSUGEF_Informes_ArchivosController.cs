using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSugefInformesArchivosController : ControllerBase
    {
        private readonly FrmSugefInformesArchivosBL _bl;
        public FrmSugefInformesArchivosController(IConfiguration config)
        {
            _bl = new FrmSugefInformesArchivosBL(config);
        }

        [Authorize]
        [HttpGet("SUGEFInformesArchivos_Cortes_Obtener")]
        public ErrorDto<List<SugefInformesArchivosData>> SUGEFInformesArchivos_Cortes_Obtener(int CodEmpresa)
        {
            return _bl.SUGEFInformesArchivos_Cortes_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SUGEFInformesArchivos_Obtener")]
        public ErrorDto<List<SugefFacilidadesCrediticiasData>> SUGEFInformesArchivos_Obtener(int CodEmpresa, DateTime Corte)
        {
            return _bl.SUGEFInformesArchivos_Obtener(CodEmpresa, Corte);
        }

        [Authorize]
        [HttpPost("SUGEFInformesArchivos_Corte_Procesar")]
        public ErrorDto SUGEFInformesArchivos_Corte_Procesar(int CodEmpresa, string Usuario, DateTime Corte, string Descripcion, DateTime RngInicio, DateTime RngCorte)
        {
            return _bl.SUGEFInformesArchivos_Corte_Procesar(CodEmpresa, Usuario, Corte, Descripcion, RngInicio, RngCorte);
        }

        [Authorize]
        [HttpPost("SUGEFInformesArchivos_Archivo")]
        public IActionResult SUGEFInformesArchivos_Archivo(int CodEmpresa, string Usuario, DateTime Corte)
        {
            var resultado = _bl.SUGEFInformesArchivos_Archivo(CodEmpresa, Usuario, Corte);

            ArchivoDescargaDto archivo = resultado.Result;

            return File(
                archivo.Contenido,
                archivo.ContentType,
                archivo.NombreArchivo);
        }
    }
}