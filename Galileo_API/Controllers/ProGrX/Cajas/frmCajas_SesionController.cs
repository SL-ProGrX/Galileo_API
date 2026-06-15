using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers.ProGrX.Cajas
{
    namespace Galileo.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class FrmCajasSesionController : ControllerBase
        {
            readonly FrmCajasSesionBl BL_Cajas_Sesion;
            public FrmCajasSesionController(IConfiguration config)
            {
                BL_Cajas_Sesion = new FrmCajasSesionBl(config);
            }

            [Authorize]
            [HttpGet("Cajas_Sesion_Obtener")]
            public ErrorDto<CajasSesionDto> Cajas_Sesion_ObtenerActiva(
                int CodEmpresa,
                int sesionId,
                string caja,
                string usuario,
                int apertura,
                string identificacion)
            {
                return BL_Cajas_Sesion.Cajas_Sesion_Obtener(CodEmpresa, sesionId, caja, usuario, apertura, identificacion);
            }

            [Authorize]
            [HttpPost("Cajas_Sesion_Inicia")]
            public ErrorDto Cajas_Sesion_Inicia(int codEmpresa, string caja, string usuario, int apertura, int tipoId, string cedula, string nombre)
            {
                return BL_Cajas_Sesion.Cajas_Sesion_Inicia(codEmpresa, caja, usuario, apertura, tipoId, cedula, nombre);
            }

            [Authorize]
            [HttpPost("Cajas_Sesion_Finaliza")]
            public ErrorDto<CajasSesionFinalizaResultDto> Cajas_Sesion_Finaliza(int codEmpresa, int sesionId, string usuario)
            {
                return BL_Cajas_Sesion.Cajas_Sesion_Finaliza(codEmpresa, sesionId, usuario);
            }

            [Authorize]
            [HttpGet("Cajas_Sesion_Movimientos")]
            public ErrorDto<List<CajasSesionMovimientosDto>> Cajas_Sesion_Movimientos(int CodEmpresa, int sesionId)
            {
                return BL_Cajas_Sesion.Cajas_Sesion_Movimientos(CodEmpresa, sesionId);
            }

            [HttpGet("TiposIdentificacion_Obtener")]
            public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
            {
                return BL_Cajas_Sesion.TiposIdentificacion_Obtener(CodCliente);
            }
        }
    }
}
