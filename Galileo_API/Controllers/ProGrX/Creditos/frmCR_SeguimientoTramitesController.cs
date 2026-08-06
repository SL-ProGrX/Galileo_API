using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoTramitesController : ControllerBase
    {
        private readonly FrmCrSeguimientoTramitesBl _bl;

        public FrmCrSeguimientoTramitesController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoTramitesBl(config);
        }

        [HttpGet("Cr_SeguimientoTramites_Inicializar")]
        public ErrorDto<CrSeguimientoTramitesInicializarData> Cr_SeguimientoTramites_Inicializar(
            int codEmpresa,
            string usuario)
            => _bl.Cr_SeguimientoTramites_Inicializar(codEmpresa, usuario);

        [HttpGet("Cr_SeguimientoTramites_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesBusquedaItem>> Cr_SeguimientoTramites_Buscar(
            int codEmpresa,
            string? cedula,
            string? nombre)
            => _bl.Cr_SeguimientoTramites_Buscar(codEmpresa, cedula, nombre);

        [HttpGet("Cr_SeguimientoTramites_Operacion_Obtener")]
        public ErrorDto<CrSeguimientoTramitesOperacionData> Cr_SeguimientoTramites_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.Cr_SeguimientoTramites_Operacion_Obtener(codEmpresa, operacion);

        [HttpPost("Cr_SeguimientoTramites_Operaciones_Buscar")]
        public ErrorDto<CrSeguimientoTramitesOperacionBusquedaLista>
            Cr_SeguimientoTramites_Operaciones_Buscar(
                int codEmpresa,
                FiltrosLazyLoadData filtros)
            => _bl.Cr_SeguimientoTramites_Operaciones_Buscar(codEmpresa, filtros);

        [HttpGet("Cr_SeguimientoTramites_Operacion_Navegar")]
        public ErrorDto<int> Cr_SeguimientoTramites_Operacion_Navegar(
            int codEmpresa,
            int operacion,
            string direccion)
            => _bl.Cr_SeguimientoTramites_Operacion_Navegar(codEmpresa, operacion, direccion);

        [HttpPost("Cr_SeguimientoTramites_Recepcion_Guardar")]
        public ErrorDto<CrSeguimientoTramitesRecepcionGuardarResult>
            Cr_SeguimientoTramites_Recepcion_Guardar(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGuardarRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Guardar(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Socios_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionSocioItem>>
            Cr_SeguimientoTramites_Recepcion_Socios_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Socios_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Lineas_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionLineaItem>>
            Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Promotores_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionPromotorItem>>
            Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar")]
        public ErrorDto<List<CrSeguimientoTramitesRecepcionProveedorItem>>
            Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(
                int codEmpresa,
                string? filtro)
            => _bl.Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(codEmpresa, filtro);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesRecepcionLineaContextoData>
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionLineaContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesRecepcionGarantiaContextoData>
            Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener")]
        public ErrorDto<List<CrSeguimientoTramitesOpcionItem>>
            Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionBancoCuentasRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesRecepcionFondoContextoData>
            Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionFondoContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Deductora_Contexto_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionDeductoraContextoData>
            Cr_SeguimientoTramites_Formalizacion_Deductora_Contexto_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesFormalizacionDeductoraContextoRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Deductora_Contexto_Obtener(
                codEmpresa,
                request);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Recurso_Disponible_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionRecursoDisponibleData>
            Cr_SeguimientoTramites_Formalizacion_Recurso_Disponible_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesFormalizacionRecursoDisponibleRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Recurso_Disponible_Obtener(
                codEmpresa,
                request);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionPrevalidacionData>
            Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesFormalizacionPrevalidacionRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Obtener(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Recepcion_Monto_Calcular")]
        public ErrorDto<CrSeguimientoTramitesRecepcionMontoCalculadoData>
            Cr_SeguimientoTramites_Recepcion_Monto_Calcular(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesRecepcionMontoCalcularRequest request)
            => _bl.Cr_SeguimientoTramites_Recepcion_Monto_Calcular(codEmpresa, request);

        [HttpPut("Cr_SeguimientoTramites_MontoNoGravable_Actualizar")]
        public ErrorDto Cr_SeguimientoTramites_MontoNoGravable_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesMontoNoGravableRequest request)
            => _bl.Cr_SeguimientoTramites_MontoNoGravable_Actualizar(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Refundiciones_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionRefundicionItem>>
            Cr_SeguimientoTramites_Formalizacion_Refundiciones_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Refundiciones_Obtener(
                codEmpresa,
                operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Desembolsos_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionDesembolsoItem>>
            Cr_SeguimientoTramites_Formalizacion_Desembolsos_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Desembolsos_Obtener(codEmpresa, operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_RefundeRetenciones_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionRefundeRetencionItem>>
            Cr_SeguimientoTramites_Formalizacion_RefundeRetenciones_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_RefundeRetenciones_Obtener(
                codEmpresa,
                operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Firmas_Obtener")]
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionFirmaItem>>
            Cr_SeguimientoTramites_Formalizacion_Firmas_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Firmas_Obtener(codEmpresa, operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Requisitos_Obtener")]
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionRequisitoItem>>
            Cr_SeguimientoTramites_Formalizacion_Requisitos_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Requisitos_Obtener(codEmpresa, operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Cargos_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionCargoItem>>
            Cr_SeguimientoTramites_Formalizacion_Cargos_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Cargos_Obtener(codEmpresa, operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Fiadores_Obtener")]
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionFiadorItem>>
            Cr_SeguimientoTramites_Formalizacion_Fiadores_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Fiadores_Obtener(codEmpresa, operacion);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_ImpactoLiquidez_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionImpactoLiquidezData>
            Cr_SeguimientoTramites_Formalizacion_ImpactoLiquidez_Obtener(
                int codEmpresa,
                int operacion)
            => _bl.Cr_SeguimientoTramites_Formalizacion_ImpactoLiquidez_Obtener(
                codEmpresa,
                operacion);

        [HttpPut("Cr_SeguimientoTramites_Formalizacion_Firma_Actualizar")]
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Firma_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionFirmaActualizarRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Firma_Actualizar(codEmpresa, request);

        [HttpGet("Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionResumenData>
            Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener(
                int codEmpresa,
                [FromQuery] CrSeguimientoTramitesFormalizacionResumenRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener(codEmpresa, request);

        [HttpPost("Cr_SeguimientoTramites_Formalizacion_Aplicar")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionResult>
            Cr_SeguimientoTramites_Formalizacion_Aplicar(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionAplicarRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Aplicar(codEmpresa, request);

        [HttpPost("Cr_SeguimientoTramites_Formalizacion_Anular")]
        public ErrorDto<CrSeguimientoTramitesFormalizacionResult>
            Cr_SeguimientoTramites_Formalizacion_Anular(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionAnularRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Anular(codEmpresa, request);

        [HttpPut("Cr_SeguimientoTramites_Formalizacion_Fechas_Actualizar")]
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Fechas_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionFechasRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Fechas_Actualizar(codEmpresa, request);

        [HttpPut("Cr_SeguimientoTramites_Formalizacion_Indicadores_Actualizar")]
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Indicadores_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionIndicadoresRequest request)
            => _bl.Cr_SeguimientoTramites_Formalizacion_Indicadores_Actualizar(codEmpresa, request);
    }
}
