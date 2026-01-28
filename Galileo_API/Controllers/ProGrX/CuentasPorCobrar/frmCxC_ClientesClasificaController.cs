 
using Galileo.Models.ERROR; 
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Cobros;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCClientesClasificaController : ControllerBase
    {
        private readonly FrmCxCClientesClasificaBL _bl;

        public FrmCxCClientesClasificaController(IConfiguration config)
            => _bl = new FrmCxCClientesClasificaBL(config);

        [Authorize]
        [HttpGet("CxCClientesClasificaLista_Obtener")]
        public ErrorDto<CxCClientesClasificaLista> CxCClientesClasificaLista_Obtener(int CodEmpresa, string filtros, bool esExportar)
        {
            return _bl.CxCClientesClasificaLista_Obtener(CodEmpresa, filtros, esExportar);
        }

        [Authorize]
        [HttpPost("CxCClientesClasifica_Guardar")]
        public ErrorDto CxCCargosTCxCClientesClasifica_Guardaripos_Guardar(int CodEmpresa, string usuario, [FromBody] CxCClientesClasificaData datos)
        {
            return _bl.CxCClientesClasifica_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("CxCClientesClasifica_Eliminar")]
        public ErrorDto CxCClientesClasifica_Eliminar(int CodEmpresa, string usuario, string CodCategoria)
        {
            return _bl.CxCClientesClasifica_Eliminar(CodEmpresa, usuario, CodCategoria);
        }
    }
}