using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los Requisitos Fosol (frmFSL_Requisitos).
    /// </summary>
    [Route("api/frmFSL_Requisitos")]
    [ApiController]
    public class FrmFslRequisitosController : ControllerBase
    {
        private readonly FrmFslRequisitosBL _bl;

        public FrmFslRequisitosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslRequisitosBL(config);
        }

        /// <summary>Lista de requisitos Fosol.</summary>
        [Authorize]
        [HttpGet("FslRequisitos_Obtener")]
        public ErrorDto<FslRequisitosDataLista> FslRequisitos_Obtener(int CodCliente, string filtros)
            => _bl.FslRequisitos_Obtener(CodCliente, filtros);

        /// <summary>Causas activas de un plan.</summary>
        [Authorize]
        [HttpGet("FslPlanesCausa_Obtener")]
        public ErrorDto<List<FslPanesCausasLista>> FslPlanesCausa_Obtener(int CodCliente, string cod_plan)
            => _bl.FslPlanesCausa_Obtener(CodCliente, cod_plan);

        /// <summary>Requisitos y su asignación a una causa/plan.</summary>
        [Authorize]
        [HttpGet("FslRequisitoCausa_Obtener")]
        public ErrorDto<List<FslRequisitoCausa>> FslRequisitoCausa_Obtener(int CodCliente, string cod_plan, string cod_causa)
            => _bl.FslRequisitoCausa_Obtener(CodCliente, cod_plan, cod_causa);

        /// <summary>Planes Fosol activos.</summary>
        [Authorize]
        [HttpGet("FslPlanes_Obtener")]
        public ErrorDto<List<FslPlanes>> FslPlanes_Obtener(int CodCliente)
            => _bl.FslPlanes_Obtener(CodCliente);

        /// <summary>Guarda un requisito (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("Requisito_Guardar")]
        public ErrorDto Requisito_Guardar(int CodCliente, [FromBody] FslRequisitosData requisito)
            => _bl.Requisito_Guardar(CodCliente, requisito);

        /// <summary>Edita la asignación de un requisito a una causa/plan.</summary>
        [Authorize]
        [HttpPut("FslAsignacion_Editar")]
        public ErrorDto FslAsignacion_Editar(int CodCliente, [FromBody] FslRequisitoEditar asignacion)
            => _bl.FslAsignacion_Editar(CodCliente, asignacion);

        /// <summary>Elimina un requisito.</summary>
        [Authorize]
        [HttpDelete("FslRequisito_Eliminar")]
        public ErrorDto FslRequisito_Eliminar(int CodCliente, string cod_requisito)
            => _bl.FslRequisito_Eliminar(CodCliente, cod_requisito);
    }
}
