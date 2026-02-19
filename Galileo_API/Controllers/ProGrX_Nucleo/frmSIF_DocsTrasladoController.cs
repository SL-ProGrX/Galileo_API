using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifDocsTrasladoController : ControllerBase
    {
        private readonly FrmSifDocsTrasladoBL _bl;

        public FrmSifDocsTrasladoController(IConfiguration config)
        {
            _bl = new FrmSifDocsTrasladoBL(config);
        }

        [Authorize]
        [HttpGet("Sif_DocsTraslado_Lista_Obtener")]
        public ErrorDto<SifDocsTrasladoDocumentosLista> Sif_DocsTraslado_Lista_Obtener(
            int CodEmpresa, string filtros, DateTime fechaInicio, DateTime fechaFin, bool soloBalanceados)
        {
            return _bl.Sif_DocsTraslado_Lista_Obtener(CodEmpresa, filtros, fechaInicio, fechaFin, soloBalanceados);
        }

        [Authorize]
        [HttpGet("Sif_DocsTraslado_Desbalanceados_Obtener")]
        public ErrorDto<SifDocsTrasladoDesbalanceadosLista> Sif_DocsTraslado_Desbalanceados_Obtener(
            int CodEmpresa, string filtros, DateTime fechaInicio, DateTime fechaFin)
        {
            return _bl.Sif_DocsTraslado_Desbalanceados_Obtener(CodEmpresa, filtros, fechaInicio, fechaFin);
        }

        [Authorize]
        [HttpGet("Sif_DocsTraslado_Lista_Export")]
        public ErrorDto<List<SifDocsTrasladoDocumentosData>> Sif_DocsTraslado_Lista_Export(
            int CodEmpresa, string filtros, DateTime fechaInicio, DateTime fechaFin, bool soloBalanceados)
        {
            return _bl.Sif_DocsTraslado_Lista_Export(CodEmpresa, filtros, fechaInicio, fechaFin, soloBalanceados);
        }

        [Authorize]
        [HttpGet("Sif_DocsTraslado_Desbalanceados_Export")]
        public ErrorDto<List<SifDocsTrasladoDesbalanceadoData>> Sif_DocsTraslado_Desbalanceados_Export(
            int CodEmpresa, string filtros, DateTime fechaInicio, DateTime fechaFin)
        {
            return _bl.Sif_DocsTraslado_Desbalanceados_Export(CodEmpresa, filtros, fechaInicio, fechaFin);
        }

        [Authorize]
        [HttpGet("Sif_DocsTraslado_Documento_Config_Obtener")]
        public ErrorDto<SifDocsTrasladoDocumentoConfig> Sif_DocsTraslado_Documento_Config_Obtener(
            int CodEmpresa, string tipoDocumento)
        {
            return _bl.Sif_DocsTraslado_Documento_Config_Obtener(CodEmpresa, tipoDocumento);
        }

        [Authorize]
        [HttpPost("Sif_DocsTraslado_Reactivar")]
        public ErrorDto<string> Sif_DocsTraslado_Reactivar(
            int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return _bl.Sif_DocsTraslado_Reactivar(CodEmpresa, fechaInicio, fechaFin);
        }

        [Authorize]
        [HttpPost("Sif_DocsTraslado_Aplica")]
        public ErrorDto<string> Sif_DocsTraslado_Aplica(
            int CodEmpresa, string jrequest)
        {
            return _bl.Sif_DocsTraslado_Aplica(CodEmpresa, jrequest);
        }

        [Authorize]
        [HttpPost("Sif_DocsTraslado_Aplica_Lote")]
        public ErrorDto<SifDocsTrasladoResultadoLote> Sif_DocsTraslado_Aplica_Lote(
            int CodEmpresa, string jrequest)
        {
            return _bl.Sif_DocsTraslado_Aplica_Lote(CodEmpresa, jrequest);
        }
    }
}