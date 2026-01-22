using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprSolicitudAutorizaController : ControllerBase
    {
        private readonly FrmCprSolicitudAutorizaBL _bl;
        public FrmCprSolicitudAutorizaController(IConfiguration config)
        {
            _bl = new FrmCprSolicitudAutorizaBL(config);
        }

        [HttpGet("CprSolicitudAdjudica_Consultar")]
        public ErrorDto<List<CprSolicitudAdjudicaConsulta>> CprSolicitudAdjudica_Consultar(int CodEmpresa, int cpr_id)
         {
            return _bl.CprSolicitudAdjudica_Consultar(CodEmpresa, cpr_id);
        }

        [HttpGet("CprSolicitudAdjudicaProductos_Consultar")]
        public ErrorDto<List<CprSolicitudAdjudicaProductosDto>> CprSolicitudAdjudicaProductos_Consultar(int CodEmpresa, int cpr_id, int proveedor, string? cotizacion)
        {
            return _bl.CprSolicitudAdjudicaProductos_Consultar(CodEmpresa, cpr_id, proveedor, cotizacion);
        }

        [HttpPost("CprSolicitudAdjudicaProv_Upsert")]
        public ErrorDto CprSolicitudAdjudicaProv_Upsert(int CodEmpresa, string adjudica)
        {
            return _bl.CprSolicitudAdjudicaProv_Upsert(CodEmpresa, adjudica);
        }

        [HttpGet("CprSolicitudRecomendacion_Obtener")]
        public ErrorDto<string> CprSolicitudRecomendacion_Obtener(int CodEmpresa, int cpr_id)
        {
            return _bl.CprSolicitudRecomendacion_Obtener(CodEmpresa, cpr_id);
        }

        [HttpGet("CprSolicitudNumContrato_Obtener")]
        public ErrorDto<string> CprSolicitudNumContrato_Obtener(int CodEmpresa, int cpr_id)
        {
            return _bl.CprSolicitudNumContrato_Obtener(CodEmpresa, cpr_id);
        }

        [HttpPost("CprSolicitudRecomendacion_Guardar")]
        public ErrorDto CprSolicitudRecomendacion_Guardar(int CodEmpresa, int cpr_id, string recomendacion, string? cod_contrato, bool requiereContrato)
        {
            return _bl.CprSolicitudRecomendacion_Guardar(CodEmpresa, cpr_id, recomendacion, cod_contrato, requiereContrato);
        }

        [HttpPost("CprSolicitudAdjudicacion_Cerrar")]
        public ErrorDto CprSolicitudAdjudicacion_Cerrar(int CodEmpresa, int cpr_id, string usuario)
        {
            return _bl.CprSolicitudAdjudicacion_Cerrar(CodEmpresa, cpr_id, usuario);
        }
    }
}