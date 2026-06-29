using Galileo.DataBaseTier;
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
    public class FrmCRPlanPagosController : ControllerBase
    {
        private readonly FrmCRPlanPagosBL BL;

        public FrmCRPlanPagosController(IConfiguration config)
        {
            BL = new FrmCRPlanPagosBL(config);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Obtener")]
        public ErrorDto<CrPlanPagosObtenerDto> CR_PlanPagos_Obtener(int CodEmpresa,int operacion,string? usuario)
        {
            return BL.CR_PlanPagos_Obtener(CodEmpresa, operacion, usuario);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Cargos_Lista_Obtener")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosCargosData>> CR_PlanPagos_Cargos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Cargos_Lista_Obtener(CodEmpresa, operacion, idSeq, filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Cargos_Lista_Export")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosCargosData>> CR_PlanPagos_Cargos_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Cargos_Lista_Export(CodEmpresa, operacion, idSeq, filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Polizas_Lista_Obtener")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosPolizasData>> CR_PlanPagos_Polizas_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Polizas_Lista_Obtener(CodEmpresa, operacion, idSeq, filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Polizas_Lista_Export")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosPolizasData>> CR_PlanPagos_Polizas_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Polizas_Lista_Export(CodEmpresa, operacion, idSeq, filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Documentos_Lista_Obtener")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosDocumentosData>> CR_PlanPagos_Documentos_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            bool todos,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Documentos_Lista_Obtener(
                CodEmpresa,
                operacion,
                idSeq,
                todos,
                filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Documentos_Lista_Export")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosDocumentosData>> CR_PlanPagos_Documentos_Lista_Export(
            int CodEmpresa,
            int operacion,
            decimal idSeq,
            bool todos,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Documentos_Lista_Export(
                CodEmpresa,
                operacion,
                idSeq,
                todos,
                filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_DocumentoValores_Obtener")]
        public ErrorDto<List<CrPlanPagosValoresData>> CR_PlanPagos_DocumentoValores_Obtener(
            int CodEmpresa,
            string tipoDocumento,
            string transaccion)
        {
            return BL.CR_PlanPagos_DocumentoValores_Obtener(
                CodEmpresa,
                tipoDocumento,
                transaccion);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Ajustes_Lista_Obtener")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosAjustesData>> CR_PlanPagos_Ajustes_Lista_Obtener(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Ajustes_Lista_Obtener(CodEmpresa, operacion, filtros);
        }

        [Authorize]
        [HttpGet("CR_PlanPagos_Ajustes_Lista_Export")]
        public ErrorDto<CrPlanPagosListaResult<CrPlanPagosAjustesData>> CR_PlanPagos_Ajustes_Lista_Export(
            int CodEmpresa,
            int operacion,
            string parametros)
        {
            var filtros = string.IsNullOrWhiteSpace(parametros)
                ? new FiltrosLazyLoadData()
                : Newtonsoft.Json.JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();

            return BL.CR_PlanPagos_Ajustes_Lista_Export(CodEmpresa, operacion, filtros);
        }

        [Authorize]
        [HttpPost("CR_PlanPagos_Activar")]
        public ErrorDto CR_PlanPagos_Activar(
            int CodEmpresa,
            [FromBody] CrPlanPagosActivarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.");
            }

            return BL.CR_PlanPagos_Activar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_PlanPagos_Revisar")]
        public ErrorDto CR_PlanPagos_Revisar(
            int CodEmpresa,
            [FromBody] CrPlanPagosRevisarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.");
            }

            return BL.CR_PlanPagos_Revisar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_PlanPagos_Email_Enviar")]
        public ErrorDto CR_PlanPagos_Email_Enviar(
            int CodEmpresa,
            [FromBody] CrPlanPagosEmailRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.");
            }

            return BL.CR_PlanPagos_Email_Enviar(CodEmpresa, request);
        }
    }
}