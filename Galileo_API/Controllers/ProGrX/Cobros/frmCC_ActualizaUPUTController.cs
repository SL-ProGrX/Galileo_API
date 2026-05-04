using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCCActualizaUpUtController : ControllerBase
    {
        private readonly FrmCCActualizaUpUtBl _bl;

        public FrmCCActualizaUpUtController(IConfiguration config)
        {
            _bl = new FrmCCActualizaUpUtBl(config);
        }

        [HttpPost("CC_ActualizaUpUt_ProcesarArchivo")]
        [Consumes("multipart/form-data")]
        public async Task<ErrorDto> CC_ActualizaUpUt_ProcesarArchivo(
    int CodEmpresa,
    string usuario,
    IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Archivo requerido."
                };
            }

            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Usuario requerido."
                };
            }

            var fileName = Path.GetFileName(file.FileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Nombre de archivo inválido."
                };
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(ext) && ext != ".txt")
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Extensión inválida. Solo se permiten archivos .txt."
                };
            }

            if (fileName.Contains("..", StringComparison.Ordinal) ||
                fileName.Contains('/', StringComparison.Ordinal) ||
                fileName.Contains('\\', StringComparison.Ordinal) ||
                Path.IsPathRooted(fileName))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Nombre de archivo inválido."
                };
            }

            if (!Regex.IsMatch(fileName, @"^[A-Za-z0-9._\-\s]+$"))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El nombre del archivo contiene caracteres no permitidos."
                };
            }

            return await _bl.CC_ActualizaUpUt_ProcesarArchivo(CodEmpresa, usuario, file);
        }
    }
}
