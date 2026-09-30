using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_Parametros")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslParametrosController : ControllerBase
    {
        private readonly FrmFslParametrosBl _bl;

        public FrmFslParametrosController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmFslParametrosBl(config);
        }

        [HttpPost("FSL_Parametros_Inicializar")]
        public ErrorDto FSL_Parametros_Inicializar(
            int CodCliente)
        {
            return _bl.FSL_Parametros_Inicializar(
                CodCliente);
        }

        [HttpGet("FSL_Parametros_Lista_Obtener")]
        public ErrorDto<FslParametrosListaDto>
            FSL_Parametros_Lista_Obtener(
                int CodCliente,
                string filtros)
        {
            return _bl.FSL_Parametros_Lista_Obtener(
                CodCliente,
                filtros);
        }

        [HttpPut("FSL_Parametros_Actualizar")]
        public ErrorDto FSL_Parametros_Actualizar(
            int CodCliente,
            FslParametroActualizarRequest request)
        {
            return _bl.FSL_Parametros_Actualizar(
                CodCliente,
                request);
        }
    }
}