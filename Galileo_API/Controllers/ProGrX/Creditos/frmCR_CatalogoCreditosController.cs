using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoCreditosController : ControllerBase
    {
        private readonly FrmCrCatalogoCreditosBl _bl;

        public FrmCrCatalogoCreditosController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoCreditosBl(config);
        }

        [HttpGet("CrCatalogoCreditos_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoData>> CrCatalogoCreditos_Obtener(int codEmpresa, bool soloAutoGestion = false)
        {
            return _bl.CrCatalogoCreditos_Obtener(codEmpresa, soloAutoGestion);
        }

        [HttpGet("CrCatalogoCreditos_Consultar")]
        public ErrorDto<CrCatalogoCreditoData?> CrCatalogoCreditos_Consultar(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_Consultar(codEmpresa, codigo);
        }

        [HttpGet("CrCatalogoCreditos_PermiteCambioRetencionPoliza")]
        public ErrorDto<bool> CrCatalogoCreditos_PermiteCambioRetencionPoliza(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_PermiteCambioRetencionPoliza(codEmpresa, codigo);
        }

        [HttpGet("CrCatalogoCreditos_Cuentas_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoCuentaData>> CrCatalogoCreditos_Cuentas_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_Cuentas_Obtener(codEmpresa, codigo);
        }

        [HttpGet("CrCatalogoCreditos_Asignaciones_Obtener")]
        public ErrorDto<CrCatalogoCreditoAsignacionesData> CrCatalogoCreditos_Asignaciones_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_Asignaciones_Obtener(codEmpresa, codigo);
        }

        [HttpGet("CrCatalogoCreditos_BloqueUno_Obtener")]
        public ErrorDto<CrCatalogoCreditoBloqueUnoData> CrCatalogoCreditos_BloqueUno_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_BloqueUno_Obtener(codEmpresa, codigo);
        }

        [HttpGet("CrCatalogoCreditos_Prioridad_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoPrioridadData>> CrCatalogoCreditos_Prioridad_Obtener(int codEmpresa)
        {
            return _bl.CrCatalogoCreditos_Prioridad_Obtener(codEmpresa);
        }

        [HttpGet("CrCatalogoCreditos_Adjuntos_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoAdjuntoData>> CrCatalogoCreditos_Adjuntos_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_Adjuntos_Obtener(codEmpresa, codigo);
        }

        [HttpGet("CrCatalogoCreditos_Oficinas_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Oficinas_Obtener(int codEmpresa)
        {
            return _bl.CrCatalogoCreditos_Oficinas_Obtener(codEmpresa);
        }

        [HttpGet("CrCatalogoCreditos_Planes_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Planes_Obtener(int codEmpresa)
        {
            return _bl.CrCatalogoCreditos_Planes_Obtener(codEmpresa);
        }

        [HttpGet("CrCatalogoCreditos_Divisas_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Divisas_Obtener(int codEmpresa)
        {
            return _bl.CrCatalogoCreditos_Divisas_Obtener(codEmpresa);
        }

        [HttpPost("CrCatalogoCreditos_Asignacion_Guardar")]
        public ErrorDto CrCatalogoCreditos_Asignacion_Guardar(int codEmpresa, CrCatalogoCreditoAsignacionGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_Asignacion_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_BloqueUno_Guardar")]
        public ErrorDto CrCatalogoCreditos_BloqueUno_Guardar(int codEmpresa, CrCatalogoCreditoBloqueUnoGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_BloqueUno_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_Prioridad_Guardar")]
        public ErrorDto CrCatalogoCreditos_Prioridad_Guardar(int codEmpresa, CrCatalogoCreditoPrioridadGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_Prioridad_Guardar(codEmpresa, request);
        }

        [HttpGet("CrCatalogoCreditos_RangosBase_Obtener")]
        public ErrorDto<CrCatalogoCreditoRangosBaseData> CrCatalogoCreditos_RangosBase_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_RangosBase_Obtener(codEmpresa, codigo);
        }

        [HttpPost("CrCatalogoCreditos_RangoBase_Guardar")]
        public ErrorDto<int> CrCatalogoCreditos_RangoBase_Guardar(int codEmpresa, CrCatalogoCreditoRangoBaseGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_RangoBase_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_RangoPlazo_Guardar")]
        public ErrorDto<int> CrCatalogoCreditos_RangoPlazo_Guardar(int codEmpresa, CrCatalogoCreditoRangoPlazoGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_RangoPlazo_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_RangoGarantia_Guardar")]
        public ErrorDto CrCatalogoCreditos_RangoGarantia_Guardar(int codEmpresa, CrCatalogoCreditoRangoGarantiaGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_RangoGarantia_Guardar(codEmpresa, request);
        }

        [HttpGet("CrCatalogoCreditos_RangosLiquidez_Obtener")]
        public ErrorDto<CrCatalogoCreditoRangosLiquidezData> CrCatalogoCreditos_RangosLiquidez_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_RangosLiquidez_Obtener(codEmpresa, codigo);
        }

        [HttpPost("CrCatalogoCreditos_LiquidezBono_Guardar")]
        public ErrorDto CrCatalogoCreditos_LiquidezBono_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezBonoGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_LiquidezBono_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_LiquidezCapacidad_Guardar")]
        public ErrorDto CrCatalogoCreditos_LiquidezCapacidad_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezCapacidadGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_LiquidezCapacidad_Guardar(codEmpresa, request);
        }

        [HttpGet("CrCatalogoCreditos_ComitesEstudio_Obtener")]
        public ErrorDto<List<CrCatalogoCreditoComiteEstudioData>> CrCatalogoCreditos_ComitesEstudio_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoCreditos_ComitesEstudio_Obtener(codEmpresa, codigo);
        }

        [HttpPost("CrCatalogoCreditos_ComiteEstudio_Guardar")]
        public ErrorDto<int> CrCatalogoCreditos_ComiteEstudio_Guardar(int codEmpresa, CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_ComiteEstudio_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_Guardar")]
        public ErrorDto CrCatalogoCreditos_Guardar(int codEmpresa, CrCatalogoCreditoGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_Guardar(codEmpresa, request);
        }

        [HttpPost("CrCatalogoCreditos_PeL_Guardar")]
        public ErrorDto CrCatalogoCreditos_PeL_Guardar(int codEmpresa, CrCatalogoCreditoPeLGuardarRequest request)
        {
            return _bl.CrCatalogoCreditos_PeL_Guardar(codEmpresa, request);
        }

        [HttpDelete("CrCatalogoCreditos_Eliminar")]
        public ErrorDto CrCatalogoCreditos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            return _bl.CrCatalogoCreditos_Eliminar(codEmpresa, codigo, usuario);
        }
    }
}
