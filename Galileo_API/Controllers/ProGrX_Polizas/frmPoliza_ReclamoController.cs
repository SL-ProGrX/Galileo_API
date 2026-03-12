using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPolizaReclamoController : ControllerBase
    {
       private readonly FrmPolizaReclamoBL _bl;
    
       public FrmPolizaReclamoController(IConfiguration config)
       {
            _bl = new FrmPolizaReclamoBL(config);
       }

        [HttpGet("Poliza_Reclamo_Motivos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Motivos_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _bl.Poliza_Reclamo_Motivos_Lista(codEmpresa, codPoliza);
        }

        [HttpGet("Poliza_Reclamo_Causas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Causas_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _bl.Poliza_Reclamo_Causas_Lista(codEmpresa, codPoliza);
        }

        [HttpGet("Poliza_Reclamo_Estados_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Estados_Lista(int codEmpresa)
        {
            return _bl.Poliza_Reclamo_Estados_Lista(codEmpresa);
        }

        [HttpGet("Poliza_Reclamo_Bancos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Bancos_Lista(
            int codEmpresa,
            string usuario)
        {
            return _bl.Poliza_Reclamo_Bancos_Lista(codEmpresa, usuario);
        }

        [HttpGet("Poliza_Reclamo_Cuentas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Cuentas_Lista(
            int codEmpresa,
            string cedula,
            int bancoId)
        {
            return _bl.Poliza_Reclamo_Cuentas_Lista(codEmpresa, cedula, bancoId);
        }

        [HttpGet("Poliza_Reclamo_Load")]
        public ErrorDto<PolizaReclamoFormularioResponse> Poliza_Reclamo_Load(
           int codEmpresa,
           int reclamoId)
        {
            return _bl.Poliza_Reclamo_Load(codEmpresa, reclamoId);
        }

        [HttpPost("Poliza_Reclamo_Nuevo")]
        public ErrorDto<PolizaReclamoFormularioResponse> Poliza_Reclamo_Nuevo(
            int codEmpresa,
            [FromBody] PolizaReclamoRequestNuevo request)
        {
            return _bl.Poliza_Reclamo_Nuevo(codEmpresa, request);
        }

        [HttpGet("Poliza_Reclamo_Seguimiento_Lista")]
        public ErrorDto<List<PolizaReclamoSeguimientoItemResponse>> Poliza_Reclamo_Seguimiento_Lista(
            int codEmpresa,
            int reclamoId)
        {
            return _bl.Poliza_Reclamo_Seguimiento_Lista(codEmpresa, reclamoId);
        }

        [HttpGet("Poliza_Reclamo_Fondo_Movimientos")]
        public ErrorDto<List<PolizaReclamoFondoItemResponse>> Poliza_Reclamo_Fondo_Movimientos(
            int codEmpresa,
            string plan,
            int contrato)
        {
            return _bl.Poliza_Reclamo_Fondo_Movimientos(codEmpresa, plan, contrato);
        }

        [HttpGet("Poliza_Reclamo_Desembolsos_Consulta")]
        public ErrorDto<List<PolizaReclamoDesembolsoItemResponse>> Poliza_Reclamo_Desembolsos_Consulta(
            int codEmpresa,
            int reclamoId,
            string plan,
            int contrato)
        {
            return _bl.Poliza_Reclamo_Desembolsos_Consulta(codEmpresa, reclamoId, plan, contrato);
        }

        [HttpGet("Poliza_Reclamo_Etiquetas_Lista")]
        public ErrorDto<List<PolizaReclamoEtiquetaItemResponse>> Poliza_Reclamo_Etiquetas_Lista(
            int codEmpresa,
            int reclamoId)
        {
            return _bl.Poliza_Reclamo_Etiquetas_Lista(codEmpresa, reclamoId);
        }

        [HttpPost("Poliza_Reclamo_Actualiza_Datos_Vida")]
        public ErrorDto Poliza_Reclamo_Actualiza_Datos_Vida(
            int codEmpresa,
            [FromBody] PolizaReclamoActualizarVidaRequest request)
        {
            return _bl.Poliza_Reclamo_Actualiza_Datos_Vida(codEmpresa, request);
        }

        [HttpPost("Poliza_Reclamo_Actualiza_Datos_Incendio")]
        public ErrorDto Poliza_Reclamo_Actualiza_Datos_Incendio(
            int codEmpresa,
            [FromBody] PolizaReclamoActualizarIncendioRequest request)
        {
            return _bl.Poliza_Reclamo_Actualiza_Datos_Incendio(codEmpresa, request);
        }

        [HttpPost("Poliza_Reclamo_Actualiza_Recepcion")]
        public ErrorDto Poliza_Reclamo_Actualiza_Recepcion(
            int codEmpresa,
            [FromBody] PolizaReclamoActualizarRecepcionRequest request)
        {
            return _bl.Poliza_Reclamo_Actualiza_Recepcion(codEmpresa, request);
        }

        [HttpPost("Poliza_Reclamo_Seguimiento_Manual_Add")]
        public ErrorDto Poliza_Reclamo_Seguimiento_Manual_Add(
            int codEmpresa,
            [FromBody] PolizaReclamoSeguimientoManualAddRequest request)
        {
            return _bl.Poliza_Reclamo_Seguimiento_Manual_Add(codEmpresa, request);
        }

        [HttpPost("Poliza_Reclamo_Fondo_Creacion")]
        public ErrorDto<PolizaReclamoFondoCrearResponse> Poliza_Reclamo_Fondo_Creacion(
            int codEmpresa,
            [FromBody] PolizaReclamoFondoCrearRequest request)
        {
            return _bl.Poliza_Reclamo_Fondo_Creacion(codEmpresa, request);
        }

        [HttpPost("Poliza_Reclamo_Fondo_Aportacion")]
        public ErrorDto<PolizaReclamoFondoAportacionResponse> Poliza_Reclamo_Fondo_Aportacion(
            int codEmpresa,
            [FromBody] PolizaReclamoFondoAportacionRequest request)
        {
            return _bl.Poliza_Reclamo_Fondo_Aportacion(codEmpresa, request);
        }
        [HttpPost("Poliza_Reclamo_Desembolsos_Aplica")]
        public ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_Aplica(
            int codEmpresa,
            [FromBody] PolizaReclamoDesembolsoAplicaRequest request)
        {
            return _bl.Poliza_Reclamo_Desembolsos_Aplica(codEmpresa, request);
        }

        [HttpPost("Poliza_Reclamo_Etiqueta_Manual_Add")]
        public ErrorDto Poliza_Reclamo_Etiqueta_Manual_Add(
            int codEmpresa,
            [FromBody] PolizaReclamoEtiquetaManualAddRequest request)
        {
            return _bl.Poliza_Reclamo_Etiqueta_Manual_Add(codEmpresa, request);
        }
    }
}
