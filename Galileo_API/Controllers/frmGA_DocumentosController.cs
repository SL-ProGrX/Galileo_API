using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GA;
using System.Reflection;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmGA_DocumentosController : Controller
    {
        private readonly IConfiguration _config;
        frmGA_DocumentosBL BL_GA_Documentos;


        public frmGA_DocumentosController(IConfiguration config)
        {
            _config = config;
            BL_GA_Documentos = new frmGA_DocumentosBL(_config);
        }


        [HttpGet("TiposDocumentos_Obtener")]
        public ErrorDto<List<TiposDocumentosArchivosDto>> TiposDocumentos_Obtener(int CodEmpresa, string Usuario, string Modulo)
        {
            return new frmGA_DocumentosBL(_config).TiposDocumentos_Obtener(CodEmpresa, Usuario, Modulo);
        }


        [HttpPost("Documentos_Insertar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Documentos_Insertar(
           [FromQuery] int CodEmpresa,
           [FromForm] DocumentoFormData formData)
        {

            var file = formData.File;
            var info = formData.Info;

            // Convertir el JSON a DTO
            var documentInfo = JsonConvert.DeserializeObject<DocumentosArchivoDto>(info)
                               ?? new DocumentosArchivoDto();

            // Leer el archivo en un byte array
            byte[] fileContent = null;
            if (file != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                }
            }

            documentInfo.filecontent = fileContent;

            var result = BL_GA_Documentos.Documentos_Insertar(CodEmpresa, documentInfo);

            return Ok(result);
        }


        [HttpPost("Documentos_Obtener")]
        public List<DocumentosArchivoDto> Documentos_Obtener(GaDocumento filtros)
        {
            return new frmGA_DocumentosBL(_config).Documentos_Obtener(filtros);
        }

        [HttpDelete("Documentos_Eliminar")]
        public ErrorDto Documentos_Eliminar(int CodCliente, string llave01, string llave02, string llave03, string usuario)
        {
            return new frmGA_DocumentosBL(_config).Documentos_Eliminar(CodCliente, llave01, llave02, llave03, usuario);
        }
    }
}
