using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GA;

namespace Galileo.Controllers
{
    [Route("api/FrmGaDocumentos")]
    [Route("api/frmGA_Documentos")]
    [ApiController]
    public class FrmGaDocumentosController : ControllerBase
    {
        private readonly IConfiguration _config;
        readonly FrmGaDocumentosBl BL_GA_Documentos;


        public FrmGaDocumentosController(IConfiguration config)
        {
            _config = config;
            BL_GA_Documentos = new FrmGaDocumentosBl(_config);
        }


        [HttpGet("TiposDocumentos_Obtener")]
        public ErrorDto<List<TiposDocumentosArchivosDto>> TiposDocumentos_Obtener(int CodEmpresa, string Usuario, string Modulo)
        {
            return new FrmGaDocumentosBl(_config).TiposDocumentos_Obtener(CodEmpresa, Usuario, Modulo);
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
            var documentInfo = !string.IsNullOrEmpty(info)
                ? JsonConvert.DeserializeObject<DocumentosArchivoDto>(info) ?? new DocumentosArchivoDto()
                : new DocumentosArchivoDto();

            // Leer el archivo en un byte array
            byte[] fileContent = Array.Empty<byte>();
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
            return new FrmGaDocumentosBl(_config).Documentos_Obtener(filtros);
        }


        [HttpDelete("Documentos_Eliminar")]
        public ErrorDto Documentos_Eliminar(int CodCliente, string llave01, string llave02, string llave03, string usuario)
        {
            return new FrmGaDocumentosBl(_config).Documentos_Eliminar(CodCliente, llave01, llave02, llave03, usuario);
        }
    }
}
