
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Cobros;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCFacturaEstadosController : ControllerBase
    {
        private readonly FrmCxCFacturaEstadosBL _bl;

        public FrmCxCFacturaEstadosController(IConfiguration config)
            => _bl = new FrmCxCFacturaEstadosBL(config);

        [Authorize]
        [HttpGet("CxCFacturaEstadosLista_Obtener")]
        public ErrorDto<CxCFacturaEstadosLista> CxCFacturaEstadosLista_Obtener(int CodEmpresa, string filtros, bool esExportar)
        {
            return _bl.CxCFacturaEstadosLista_Obtener(CodEmpresa, filtros, esExportar);
        }

        [Authorize]
        [HttpPost("CxCFacturaEstados_Guardar")]
        public ErrorDto CxCFacturaEstados_Guardar(int CodEmpresa, string usuario, [FromBody] CxCFacturaEstadosData datos)
        {
            return _bl.CxCFacturaEstados_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("CxCFacturaEstados_Eliminar")]
        public ErrorDto CxCFacturaEstados_Eliminar(int CodEmpresa, string usuario, string codFactura)
        {
            return _bl.CxCFacturaEstados_Eliminar(CodEmpresa, usuario, codFactura);
        }
    }
}