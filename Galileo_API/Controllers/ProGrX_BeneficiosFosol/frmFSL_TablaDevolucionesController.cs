using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de la Tabla de Devoluciones Fosol (frmFSL_TablaDevoluciones).
    /// </summary>
    [Route("api/frmFSL_TablaDevoluciones")]
    [ApiController]
    public class FrmFslTablaDevolucionesController : ControllerBase
    {
        private readonly FrmFslTablaDevolucionesBL _bl;

        public FrmFslTablaDevolucionesController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslTablaDevolucionesBL(config);
        }

        /// <summary>Catálogo de tipos de garantía.</summary>
        [Authorize]
        [HttpGet("FslGarantias_Obtener")]
        public ErrorDto<List<FslGarantiasData>> FslGarantias_Obtener(int CodCliente)
            => _bl.FslGarantias_Obtener(CodCliente);

        /// <summary>Lista de devoluciones.</summary>
        [Authorize]
        [HttpGet("FslDevoluciones_Obtener")]
        public ErrorDto<FslDevolucionesDataLista> FslDevoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _bl.FslDevoluciones_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Guarda una devolución (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("ParametroDevolucion_Guardar")]
        public ErrorDto ParametroDevolucion_Guardar(int CodCliente, [FromBody] FslDevolucionesData devolucion)
            => _bl.ParametroDevolucion_Guardar(CodCliente, devolucion);

        /// <summary>Elimina una devolución.</summary>
        [Authorize]
        [HttpDelete("FslDevolucion_Eliminar")]
        public ErrorDto FslDevolucion_Eliminar(int CodCliente, int cod_devolucion)
            => _bl.FslDevolucion_Eliminar(CodCliente, cod_devolucion);
    }
}
