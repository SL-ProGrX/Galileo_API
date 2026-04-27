using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCcCaRemesasController : ControllerBase
    {
        private readonly FrmCcCaRemesasBL _bl;

        public FrmCcCaRemesasController(IConfiguration config)
        {
            _bl = new FrmCcCaRemesasBL(config);
        }

        [HttpGet("CcCaRemesas_Catalogos_Obtener")]
        public ErrorDto<CcCaRemesasCatalogosResponse> CcCaRemesas_Catalogos_Obtener(int CodEmpresa)
        {
            return _bl.CcCaRemesas_Catalogos_Obtener(CodEmpresa);
        }

        [HttpPost("CcCaRemesas_Envio_Consulta")]
        public ErrorDto<List<CcCaRemesasEnvioConsultaData>> CcCaRemesas_Envio_Consulta(
            int CodEmpresa,
            [FromBody] CcCaRemesasEnvioConsultaRequest request)
        {
            return _bl.CcCaRemesas_Envio_Consulta(CodEmpresa, request);
        }

        [HttpGet("CcCaRemesas_Recibe_Pendientes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CcCaRemesas_Recibe_Pendientes_Obtener(int CodEmpresa)
        {
            return _bl.CcCaRemesas_Recibe_Pendientes_Obtener(CodEmpresa);
        }

        [HttpGet("CcCaRemesas_Recibe_Detalle_Obtener")]
        public ErrorDto<List<CcCaRemesasRecibeDetalleData>> CcCaRemesas_Recibe_Detalle_Obtener(int CodEmpresa, long remesa)
        {
            return _bl.CcCaRemesas_Recibe_Detalle_Obtener(CodEmpresa, remesa);
        }

        [HttpGet("CcCaRemesas_Envio_Pendiente_Validar")]
        public ErrorDto<CcCaRemesasEnvioPendienteData?> CcCaRemesas_Envio_Pendiente_Validar(int CodEmpresa)
        {
            return _bl.CcCaRemesas_Envio_Pendiente_Validar(CodEmpresa);
        }

        [HttpGet("CcCaRemesas_Envio_NumeroGeneracion_Obtener")]
        public ErrorDto<long> CcCaRemesas_Envio_NumeroGeneracion_Obtener(int CodEmpresa)
        {
            return _bl.CcCaRemesas_Envio_NumeroGeneracion_Obtener(CodEmpresa);
        }

        [HttpPost("CcCaRemesas_Envio_Registrar")]
        public ErrorDto CcCaRemesas_Envio_Registrar(
            int CodEmpresa,
            string usuario,
            [FromBody] CcCaRemesasEnvioRegistrarRequest request)
        {
            return _bl.CcCaRemesas_Envio_Registrar(CodEmpresa, usuario, request);
        }

        [HttpGet("CcCaRemesas_Envio_ArchivoBanco_Obtener")]
        public ErrorDto<ArchivoDto> CcCaRemesas_Envio_ArchivoBanco_Obtener(int CodEmpresa, long numeroGeneracion)
        {
            return _bl.CcCaRemesas_Envio_ArchivoBanco_Obtener(CodEmpresa, numeroGeneracion);
        }

        [HttpPost("CcCaRemesas_Recibe_Autorizaciones_Cargar")]
        public ErrorDto CcCaRemesas_Recibe_Autorizaciones_Cargar(
            int CodEmpresa,
            [FromBody] CcCaRemesasRecibeAutorizacionesRequest request)
        {
            return _bl.CcCaRemesas_Recibe_Autorizaciones_Cargar(CodEmpresa, request);
        }

        [HttpPost("CcCaRemesas_Recibe_Cierra")]
        public ErrorDto CcCaRemesas_Recibe_Cierra(
            int CodEmpresa,
            long numeroGeneracion,
            string usuario)
        {
            return _bl.CcCaRemesas_Recibe_Cierra(CodEmpresa, numeroGeneracion, usuario);
        }

        [HttpPost("CcCaRemesas_Recibe_Aplica")]
        public ErrorDto<CcCaRemesasRecibeAplicaResponse> CcCaRemesas_Recibe_Aplica(
                int CodEmpresa,
                [FromBody] CcCaRemesasRecibeAplicaRequest request)
        {
            return _bl.CcCaRemesas_Recibe_Aplica(CodEmpresa, request);
        }
    }
}
