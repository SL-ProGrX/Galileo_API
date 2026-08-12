using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndTraspasoTesoreriaController : ControllerBase
    {
        private readonly FrmFndTraspasoTesoreriaBl _bl;

        public FrmFndTraspasoTesoreriaController(IConfiguration config)
        {
            _bl = new FrmFndTraspasoTesoreriaBl(config);
        }

        [Authorize]
        [HttpGet("TraspasoTesoreria_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_Bancos_Obtener(int codEmpresa)
        {
            return _bl.TraspasoTesoreria_Bancos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("TraspasoTesoreria_ConceptosRetencion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_ConceptosRetencion_Obtener(int codEmpresa)
        {
            return _bl.TraspasoTesoreria_ConceptosRetencion_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("Tes_Token_Consulta")]
        public ErrorDto<List<TesTokenConsultaResult>> Tes_Token_Consulta([FromBody] TesTokenConsultaParams param)
        {
            return _bl.Tes_Token_Consulta(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionBancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionBancos_Obtener([FromBody] FndTraspasoTesoreriaFiltroParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionBancos_Obtener(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionUsuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionUsuarios_Obtener([FromBody] FndTraspasoTesoreriaFiltroParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionUsuarios_Obtener(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionSistemas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionSistemas_Obtener([FromBody] FndTraspasoTesoreriaFiltroParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionSistemas_Obtener(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionTokens_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionTokens_Obtener([FromBody] FndTraspasoTesoreriaFiltroParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionTokens_Obtener(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionOficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionOficinas_Obtener([FromBody] FndTraspasoTesoreriaFiltroParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionOficinas_Obtener(param);
        }

        [Authorize]
        [HttpPost("Tes_Token_New")]
        public ErrorDto<TesTokenNewResult> Tes_Token_New([FromBody] TesTokenNewParams param)
        {
            return _bl.Tes_Token_New(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_Fix")]
        public ErrorDto<FndTraspasoTesoreriaFixResult> TraspasoTesoreria_Fix([FromQuery] int codEmpresa)
        {
            return _bl.TraspasoTesoreria_Fix(codEmpresa);
        }

        [Authorize]
        [HttpGet("TraspasoTesoreria_ParametroValor_Obtener")]
        public ErrorDto<string> TraspasoTesoreria_ParametroValor_Obtener(int codEmpresa, string codigo)
        {
            return _bl.TraspasoTesoreria_ParametroValor_Obtener(codEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionConsulta")]
        public ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>> TraspasoTesoreria_LiquidacionConsulta([FromBody] FndTraspasoTesoreriaLiquidacionConsultaParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionConsulta(param);
        }

        [Authorize]
        [HttpPost("RevisaDuplicadosEnLaRemesa")]
        public ErrorDto<List<FndTraspasoTesoreriaDuplicadosResult>> RevisaDuplicadosEnLaRemesa([FromBody] FndTraspasoTesoreriaDuplicadosParams param)
        {
            return _bl.RevisaDuplicadosEnLaRemesa(param);
        }

        [Authorize]
        [HttpPost("RetLiqTesoreria")]
        public ErrorDto<bool> RetLiqTesoreria([FromBody] FndRetLiqTesoreriaParams param)
        {
            return _bl.RetLiqTesoreria(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_Update")]
        public ErrorDto<bool> TraspasoTesoreria_Update([FromBody] FndTraspasoTesoreriaUpdateParams param)
        {
            return _bl.TraspasoTesoreria_Update(param);
        }

        [Authorize]
        [HttpPost("RetLiqTesoreria_Unificado")]
        public ErrorDto<bool> RetLiqTesoreria_Unificado([FromBody] FndRetLiqTesoreriaUnificadoParams param)
        {
            return _bl.RetLiqTesoreria_Unificado(param);
        }

        [Authorize]
        [HttpPost("TraspasoTesoreria_LiquidacionDetalle")]
        public ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>> TraspasoTesoreria_LiquidacionDetalle([FromBody] FndTraspasoTesoreriaDetalleParams param)
        {
            return _bl.TraspasoTesoreria_LiquidacionDetalle(param);
        }

        [Authorize]
        [HttpPost("FND_TraspasoTesoreria_ProcesarLote")]
        public ErrorDto<FndTraspasoTesoreriaProcesarLoteResult> FND_TraspasoTesoreria_ProcesarLote(
            [FromBody] FndTraspasoTesoreriaProcesarLoteRequest request)
        {
            return _bl.FND_TraspasoTesoreria_ProcesarLote(request);
        }

        /// <summary>
        /// Inicializa el proceso persistente de traspaso de tesorería.
        /// </summary>
        [Authorize]
        [HttpPost("FND_TraspasoTesoreria_Proceso_Iniciar")]
        public ErrorDto<FndTraspasoTesoreriaProcesoResult> FND_TraspasoTesoreria_Proceso_Iniciar(
            [FromQuery] int codEmpresa,
            [FromBody] FndTraspasoTesoreriaProcesoIniciarRequest request)
        {
            request.Usuario = User.Identity?.Name ?? string.Empty;
            return _bl.FND_TraspasoTesoreria_Proceso_Iniciar(codEmpresa, request);
        }

        /// <summary>
        /// Ejecuta el siguiente lote pendiente del traspaso de tesorería.
        /// </summary>
        [Authorize]
        [HttpPost("FND_TraspasoTesoreria_Proceso_Continuar")]
        public ErrorDto<FndTraspasoTesoreriaProcesoResult> FND_TraspasoTesoreria_Proceso_Continuar(
            [FromQuery] int codEmpresa,
            [FromBody] FndTraspasoTesoreriaProcesoContinuarRequest request)
        {
            request.Usuario = User.Identity?.Name ?? string.Empty;
            return _bl.FND_TraspasoTesoreria_Proceso_Continuar(codEmpresa, request);
        }

        /// <summary>
        /// Obtiene la lista de remesas de traspaso a tesorería para el tab de Informes.
        /// </summary>
        [Authorize]
        [HttpPost("TraspasoTesoreria_Remesas_Obtener")]
        public ErrorDto<List<FndTraspasoTesoreriaRemesaResult>> TraspasoTesoreria_Remesas_Obtener(
            [FromBody] FndTraspasoTesoreriaRemesaParams param)
        {
            return _bl.TraspasoTesoreria_Remesas_Obtener(param);
        }
    }
}
