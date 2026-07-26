using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los Parámetros de Beneficios Fosol (frmFSL_Parametros).
    /// </summary>
    [Route("api/frmFSL_Parametros")]
    [ApiController]
    public class FrmFslParametrosController : ControllerBase
    {
        private readonly FrmFslParametrosBL _bl;

        public FrmFslParametrosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslParametrosBL(config);
        }

        /// <summary>Lista de parámetros Fosol.</summary>
        [Authorize]
        [HttpGet("FslParametros_Obtener")]
        public ErrorDto<FdlParametrosListaDto> FslParametros_Obtener(int CodCliente, string filtros)
            => _bl.FslParametros_Obtener(CodCliente, filtros);

        /// <summary>Actualiza el valor de un parámetro Fosol.</summary>
        [Authorize]
        [HttpPut("FslParametros_Actualizar")]
        public ErrorDto FslParametros_Actualizar(int CodCliente, [FromBody] FdlParametrosDto parametro)
            => _bl.FslParametros_Actualizar(CodCliente, parametro);
    }
}
