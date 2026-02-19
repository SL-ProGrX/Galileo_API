using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifEmpresaController : ControllerBase
    {
        private readonly FrmSifEmpresaBL _bl;

        private const long MaxLogoBytes = 1_000_000; // 1 MB
        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        private static bool IsAllowedImage(IFormFile file)
        {
            if (!AllowedImageContentTypes.Contains(file.ContentType))
                return false;

            var ext = Path.GetExtension(file.FileName);
            return !string.IsNullOrWhiteSpace(ext) && AllowedImageExtensions.Contains(ext);
        }

        private static ErrorDto? ValidateImageRequest(SifEmpresaArchivoRequest request)
        {
            if (request.file == null || request.file.Length == 0)
                return new ErrorDto { Code = -1, Description = "Archivo vacío o cuerpo no recibido." };

            if (request.file.Length > MaxLogoBytes)
                return new ErrorDto { Code = -1, Description = "El archivo supera el tamaño permitido (máx. 1 MB)." };

            if (!IsAllowedImage(request.file))
                return new ErrorDto { Code = -1, Description = "Tipo de archivo no permitido. Solo JPG/JPEG o PNG." };

            return null;
        }

        private static async Task<byte[]> ReadFileAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }

        private static async Task<ErrorDto> GuardarImagenAsync(
            SifEmpresaArchivoRequest request,
            Func<int, int, byte[], string, ErrorDto> guardarFunc)
        {
            var validationError = ValidateImageRequest(request);
            if (validationError != null)
                return validationError;

            var contenido = await ReadFileAsync(request.file);
            return guardarFunc(request.CodEmpresa, request.idEmpresa, contenido, request.usuario);
        }

        public FrmSifEmpresaController(IConfiguration config)
        {
            _bl = new FrmSifEmpresaBL(config);
        }


        public class SifEmpresaArchivoRequest
        {
            [BindRequired]
            [Required]
            public int CodEmpresa { get; set; }

            [BindRequired]
            [Required]
            public int idEmpresa { get; set; }

            [BindRequired]
            [Required]
            public string usuario { get; set; } = string.Empty;

            [BindRequired]
            [Required]
            public IFormFile file { get; set; } = default!;
        }

        
        [Authorize]
        [HttpGet("Sif_Empresa_Obtener")]
        public ErrorDto<FrmSifEmpresaModel> Sif_Empresa_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _bl.Sif_Empresa_Obtener(CodEmpresa, idEmpresa);
        }

        [Authorize]
        [HttpPost("Sif_Empresa_Guardar")]
        public ErrorDto Sif_Empresa_Guardar(int CodEmpresa, [FromBody] FrmSifEmpresaModel dto, string usuario)
        {
            return _bl.Sif_Empresa_Guardar(CodEmpresa, dto, usuario);
        }

        [Authorize]
        [HttpGet("Sif_Empresa_Logo_Obtener")]
        public ErrorDto<byte[]> Sif_Empresa_Logo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _bl.Sif_Empresa_Logo_Obtener(CodEmpresa, idEmpresa);
        }

        [Authorize]
        [HttpPost("Sif_Empresa_Logo_Guardar")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1_200_000)]
        public async Task<ErrorDto> Sif_Empresa_Logo_Guardar([FromForm] SifEmpresaArchivoRequest request)
        {
            return await GuardarImagenAsync(request, _bl.Sif_Empresa_Logo_Guardar);
        }

        [Authorize]
        [HttpGet("Sif_Empresa_Fondo_Obtener")]
        public ErrorDto<byte[]> Sif_Empresa_Fondo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _bl.Sif_Empresa_Fondo_Obtener(CodEmpresa, idEmpresa);
        }

        [Authorize]
        [HttpPost("Sif_Empresa_Fondo_Guardar")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1_200_000)]
        public async Task<ErrorDto> Sif_Empresa_Fondo_Guardar([FromForm] SifEmpresaArchivoRequest request)
        {
            return await GuardarImagenAsync(request, _bl.Sif_Empresa_Fondo_Guardar);
        }

        [Authorize]
        [HttpGet("Sif_Empresa_Contabilidades_Obtener")]
        public ErrorDto<List<ComboContabilidadDto>> Sif_Empresa_Contabilidades_Obtener(int CodEmpresa)
        {
            return _bl.Sif_Empresa_Contabilidades_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Sif_Empresa_CuentaPorCodigo_Obtener")]
        public ErrorDto<CuentaLookupDto> Sif_Empresa_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codCuenta)
        {
            return _bl.Sif_Empresa_CuentaPorCodigo_Obtener(CodEmpresa, codContabilidad, codCuenta);
        }

        [Authorize]
        [HttpGet("Sif_Empresa_Cuentas_Buscar")]
        public ErrorDto<List<CuentaLookupDto>> Sif_Empresa_Cuentas_Buscar(int CodEmpresa, int codContabilidad, string? search = null)
        {
            return _bl.Sif_Empresa_Cuentas_Buscar(CodEmpresa, codContabilidad, search ?? string.Empty);
        }

        [Authorize]
        [HttpPost("Sif_Empresa_BloqueoFecha_Aplicar")]
        public ErrorDto Sif_Empresa_BloqueoFecha_Aplicar(int CodEmpresa, DateTime fecha, char accion, string usuario)
        {
            return _bl.Sif_Empresa_BloqueoFecha_Aplicar(CodEmpresa, fecha, accion, usuario);
        }

        [Authorize]
        [HttpGet("Sif_Empresa_BloqueoFecha_Obtener")]
        public ErrorDto<DateTime?> Sif_Empresa_BloqueoFecha_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _bl.Sif_Empresa_BloqueoFecha_Obtener(CodEmpresa, idEmpresa);
        }
    }
}