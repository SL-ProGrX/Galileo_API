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
    public class FrmCprContratosController : ControllerBase
    {
        private readonly FrmCprContratosBL _bl;
        
        public FrmCprContratosController(IConfiguration config)
        {
            _bl = new FrmCprContratosBL(config);
        }

        [HttpGet("CprContrato_Obtener")]
        public ErrorDto<CprContratosDto> CprContrato_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _bl.CprContrato_Obtener(CodEmpresa, cod_contrato);
        }

        [HttpGet("CprContratosLista_Obtener")]
        public ErrorDto<CprContratosLista> CprContratosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CprContratosLista_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("CprContrato_Insertar")]
        public ErrorDto CprContrato_Insertar(int CodEmpresa, CprContratosDto contrato)
        {
            return _bl.CprContrato_Insertar(CodEmpresa, contrato);
        }

        [HttpPost("CprContrato_Actualizar")]
        public ErrorDto CprContrato_Actualizar(int CodEmpresa, CprContratosDto contrato)
        {
            return _bl.CprContrato_Actualizar(CodEmpresa, contrato);
        }

        [HttpDelete("CprContrato_Eliminar")]
        public ErrorDto CprContrato_Eliminar(int CodEmpresa, string cod_contrato, string usuario)
        {
            return _bl.CprContrato_Eliminar(CodEmpresa, cod_contrato, usuario);
        }

        [HttpGet("CprContrato_Adendums_Obtener")]
        public ErrorDto<List<CprContratosAdendumsDto>> CprContrato_Adendums_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _bl.CprContrato_Adendums_Obtener(CodEmpresa, cod_contrato);
        }

        [HttpPost("CprContrato_Adendum_Guardar")]
        public ErrorDto CprContrato_Adendum_Guardar(int CodEmpresa, CprContratosAdendumsDto adendum)
        {
            return _bl.CprContrato_Adendum_Guardar(CodEmpresa, adendum);
        }

        [HttpDelete("CprContrato_Adendum_Eliminar")]
        public ErrorDto CprContrato_Adendum_Eliminar(int CodEmpresa, int id_adendum, string usuario)
        {
            return _bl.CprContrato_Adendum_Eliminar(CodEmpresa, id_adendum, usuario);
        }

        [HttpGet("CprContrato_Estados_Obtener")]
        public ErrorDto<List<CprContratosEstadosDto>> CprContrato_Estados_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _bl.CprContrato_Estados_Obtener(CodEmpresa, cod_contrato);
        }

        [HttpPost("CprContrato_Estados_Guardar")]
        public ErrorDto CprContrato_Estados_Guardar(int CodEmpresa, CprContratosEstadosDto estado)
        {
            return _bl.CprContrato_Estados_Guardar(CodEmpresa, estado);
        }

        [HttpDelete("CprContrato_Estados_Eliminar")]
        public ErrorDto CprContrato_Estados_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            return _bl.CprContrato_Estados_Eliminar(CodEmpresa, linea_id, usuario);
        }

        [HttpGet("CprContrato_Productos_Obtener")]
        public ErrorDto<List<CprContratosProductosDto>> CprContrato_Productos_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _bl.CprContrato_Productos_Obtener(CodEmpresa, cod_contrato);
        }

        [HttpPost("CprContrato_Producto_Guardar")]
        public ErrorDto CprContrato_Producto_Guardar(int CodEmpresa, CprContratosProductosDto producto)
        {
            return _bl.CprContrato_Producto_Guardar(CodEmpresa, producto);
        }

        [HttpDelete("CprContrato_Producto_Eliminar")]
        public ErrorDto CprContrato_Producto_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            return _bl.CprContrato_Producto_Eliminar(CodEmpresa, linea_id, usuario);
        }

        [HttpGet("CprContrato_Prorroga_Obtener")]
        public ErrorDto<List<CprContratosProrrogasDto>> CprContrato_Prorroga_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _bl.CprContrato_Prorroga_Obtener(CodEmpresa, cod_contrato);
        }

        [HttpPost("CprContrato_Prorroga_Guardar")]
        public ErrorDto CprContrato_Prorroga_Guardar(int CodEmpresa, CprContratosProrrogasDto prorroga)
        {
            return _bl.CprContrato_Prorroga_Guardar(CodEmpresa, prorroga);
        }

        [HttpDelete("CprContrato_Prorroga_Eliminar")]
        public ErrorDto CprContrato_Prorroga_Eliminar(int CodEmpresa, int id_prorroga, string usuario)
        {
            return _bl.CprContrato_Prorroga_Eliminar(CodEmpresa, id_prorroga, usuario);
        }

        [HttpGet("CprContrato_Bitacora_Obtener")]
        public ErrorDto<List<CprContratosBitacoraDto>> CprContrato_Bitacora_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _bl.CprContrato_Bitacora_Obtener(CodEmpresa, cod_contrato);
        }

        [HttpPost("CprContratoNotificacion_Enviar")]
        public Task<ErrorDto> CprContratoNotificacion_Enviar(int CodEmpresa, string cod_contrato, string mensaje, string usuario)
        {
            return _bl.CprContratoNotificacion_Enviar(CodEmpresa, cod_contrato, mensaje, usuario);
        }

        [HttpGet("CprContratosPorSolicitud_Obtener")]
        public ErrorDto<List<CprContratosDto>> CprContratosPorSolicitud_Obtener(int CodEmpresa, int cpr_id)
        {
            return _bl.CprContratosPorSolicitud_Obtener(CodEmpresa, cpr_id);
        }
    }
}