using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrCatalogoCopiaController : ControllerBase
    {
        private readonly FrmCrCatalogoCopiaBL BL;

        public FrmCrCatalogoCopiaController(IConfiguration config)
        {
            BL = new FrmCrCatalogoCopiaBL(config);
        }

        [Authorize]
        [HttpGet("CR_CatalogoCopia_Lineas_Obtener")]
        public ErrorDto<List<CrCatalogoCopiaLineaDto>> CR_CatalogoCopia_Lineas_Obtener(int CodEmpresa)
        {
            return BL.CR_CatalogoCopia_Lineas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_CatalogoCopia_Lineas_F4_Obtener")]
        public ErrorDto<List<CrCatalogoCopiaLineaDto>> CR_CatalogoCopia_Lineas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CR_CatalogoCopia_Lineas_F4_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpGet("CR_CatalogoCopia_Linea_Descripcion_Obtener")]
        public ErrorDto<CrCatalogoCopiaDescripcionDto> CR_CatalogoCopia_Linea_Descripcion_Obtener(int CodEmpresa, string codigo)
        {
            return BL.CR_CatalogoCopia_Linea_Descripcion_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CR_CatalogoCopia_Linea_Scroll_Obtener")]
        public ErrorDto<CrCatalogoCopiaScrollDto> CR_CatalogoCopia_Linea_Scroll_Obtener(int CodEmpresa, int scroll, string? codigo)
        {
            return BL.CR_CatalogoCopia_Linea_Scroll_Obtener(CodEmpresa, scroll, codigo);
        }

        [Authorize]
        [HttpPost("CR_CatalogoCopia_Copiar")]
        public ErrorDto<CrCatalogoCopiaResultadoDto> CR_CatalogoCopia_Copiar(int CodEmpresa, [FromBody] CrCatalogoCopiaRequest request)
        {
            return BL.CR_CatalogoCopia_Copiar(CodEmpresa, request);
        }
    }
}