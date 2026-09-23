using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvExistenciaProductoController : ControllerBase
    {
        private readonly FrmInvExistenciaProductoBL _bl;

        public FrmInvExistenciaProductoController(IConfiguration config)
        {
            _bl = new FrmInvExistenciaProductoBL(config);
        }

        [HttpGet("INV_ExistenciaProducto_FechaServidor_Obtener")]
        public ErrorDto INV_ExistenciaProducto_FechaServidor_Obtener(int CodEmpresa)
        {
            return _bl.INV_ExistenciaProducto_FechaServidor_Obtener(CodEmpresa);
        }

        [HttpGet("INV_ExistenciaProducto_Consultar")]
        public ErrorDto<InvExistenciaProductoResultadoDto> INV_ExistenciaProducto_Consultar(
            int CodEmpresa,
            string CodProducto,
            string FechaCorte,
            string Usuario)
        {
            return _bl.INV_ExistenciaProducto_Consultar(
                CodEmpresa,
                new InvExistenciaProductoConsulta
                {
                    cod_producto = CodProducto,
                    fecha_corte = FechaCorte,
                    usuario = Usuario
                });
        }
    }
}
