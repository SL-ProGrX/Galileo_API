using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Tipos de Sanciones de Beneficios (frmAF_Bene_Sanciones_Tipos).
    /// </summary>
    [Route("api/frmAF_Bene_Sanciones_Tipos")]
    [ApiController]
    public class FrmAfBeneSancionesTiposController : ControllerBase
    {
        private readonly FrmAfBeneSancionesTiposBL _bl;

        public FrmAfBeneSancionesTiposController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneSancionesTiposBL(config);
        }

        /// <summary>Lista de tipos de sanciones.</summary>
        [Authorize]
        [HttpGet("afBeneTipoSancionObtener")]
        public ErrorDto<AfTipoSancionesDtoLista> afBeneTipoSancionObtener(int CodCliente, string filtros)
            => _bl.afBeneTipoSancionObtener(CodCliente, filtros);

        /// <summary>Catálogo de retenciones disponibles.</summary>
        [Authorize]
        [HttpGet("BeneRetenciones_Obtener")]
        public ErrorDto<List<BeneListaRetencion>> BeneRetenciones_Obtener(int CodCliente)
            => _bl.BeneRetenciones_Obtener(CodCliente);

        /// <summary>Inserta un tipo de sanción (o actualiza si existe).</summary>
        [Authorize]
        [HttpPost("AfBeneTipoSancion_Insertar")]
        public ErrorDto AfBeneTipoSancion_Insertar(int CodCliente, [FromBody] AfTipoSancionesDto tipo_sancion)
            => _bl.AfBeneTipoSancion_Insertar(CodCliente, tipo_sancion);

        /// <summary>Actualiza un tipo de sanción.</summary>
        [Authorize]
        [HttpPut("AfBeneTipoSancion_Actualizar")]
        public ErrorDto AfBeneTipoSancion_Actualizar(int CodCliente, [FromBody] AfTipoSancionesDto tipo_sancion)
            => _bl.AfBeneTipoSancion_Actualizar(CodCliente, tipo_sancion);

        /// <summary>Elimina un tipo de sanción.</summary>
        [Authorize]
        [HttpDelete("AfBeneTipoSancion_Eliminar")]
        public ErrorDto AfBeneTipoSancion_Eliminar(int CodCliente, int tipo_sancion)
            => _bl.AfBeneTipoSancion_Eliminar(CodCliente, tipo_sancion);
    }
}
