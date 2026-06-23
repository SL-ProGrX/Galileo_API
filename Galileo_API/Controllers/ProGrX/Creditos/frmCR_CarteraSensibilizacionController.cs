using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrCarteraSensibilizacionController : ControllerBase
    {
        private readonly FrmCrCarteraSensibilizacionBl _bl;

        public FrmCrCarteraSensibilizacionController(IConfiguration config)
        {
            _bl = new FrmCrCarteraSensibilizacionBl(config);
        }

        [HttpGet("CrCarteraSensibilizacion_Pantalla_Obtener")]
        public ErrorDto<CrCarteraSensibilizacionPantallaData> CrCarteraSensibilizacion_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _bl.CrCarteraSensibilizacion_Pantalla_Obtener(codEmpresa, usuario);

        [HttpGet("CrCarteraSensibilizacion_Linea_Combos_Obtener")]
        public ErrorDto<CrCarteraSensibilizacionPantallaData> CrCarteraSensibilizacion_Linea_Combos_Obtener(
            int codEmpresa,
            string codigo,
            bool todasLineas)
            => _bl.CrCarteraSensibilizacion_Linea_Combos_Obtener(codEmpresa, codigo, todasLineas);

        [HttpGet("CrCarteraSensibilizacion_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCarteraSensibilizacion_Catalogo_Obtener(
            int codEmpresa)
            => _bl.CrCarteraSensibilizacion_Catalogo_Obtener(codEmpresa);

        [HttpPost("CrCarteraSensibilizacion_Buscar")]
        public ErrorDto<CrCarteraSensibilizacionResultadoData> CrCarteraSensibilizacion_Buscar(
            int codEmpresa,
            CrCarteraSensibilizacionRequest request)
            => _bl.CrCarteraSensibilizacion_Buscar(codEmpresa, request);

        [HttpPost("CrCarteraSensibilizacion_Generar")]
        public ErrorDto<CrCarteraSensibilizacionGenerarData> CrCarteraSensibilizacion_Generar(
            int codEmpresa,
            CrCarteraSensibilizacionResultadoData request)
            => _bl.CrCarteraSensibilizacion_Generar(codEmpresa, request);

        [HttpGet("CrCarteraSensibilizacion_Liquidez_Obtener")]
        public ErrorDto<List<CrCarteraSensibilizacionLiquidezItem>> CrCarteraSensibilizacion_Liquidez_Obtener(
            int codEmpresa)
            => _bl.CrCarteraSensibilizacion_Liquidez_Obtener(codEmpresa);
    }
}