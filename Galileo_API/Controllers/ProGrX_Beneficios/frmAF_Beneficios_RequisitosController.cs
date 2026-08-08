using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Requisitos para Beneficios (frmAF_Beneficios_Requisitos).
    /// </summary>
    [Route("api/frmAF_Beneficios_Requisitos")]
    [ApiController]
    [Authorize]
    public class FrmAfBeneficiosRequisitosController : ControllerBase
    {
        private readonly FrmAfBeneficiosRequisitosBL _bl;

        public FrmAfBeneficiosRequisitosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosRequisitosBL(config);
        }

        /// <summary>Lista de requisitos con paginación, filtro y ordenamiento.</summary>
        [HttpGet("AfBeneRequisitos_Obtener")]
        public ErrorDto<BeneRequisitosDataLista> AfBeneRequisitos_Obtener(int CodCliente, string? filtros)
            => _bl.AfBeneRequisitos_Obtener(CodCliente, filtros);

        /// <summary>Exporta la lista de requisitos aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("AfBeneRequisitos_Exportar")]
        public ErrorDto<List<BeneRequisitosData>> AfBeneRequisitos_Exportar(int CodCliente, string? filtros)
            => _bl.AfBeneRequisitos_Exportar(CodCliente, filtros);

        /// <summary>Inserta un requisito (o actualiza si existe).</summary>
        [HttpPost("AfBeneRequisitos_Insertar")]
        public ErrorDto AfBeneRequisitos_Insertar(int CodCliente, [FromBody] BeneRequisitosData requisito)
            => _bl.AfBeneRequisitos_Insertar(CodCliente, requisito);

        /// <summary>Actualiza un requisito.</summary>
        [HttpPut("AfBeneRequisitos_Actualizar")]
        public ErrorDto AfBeneRequisitos_Actualizar(int CodCliente, [FromBody] BeneRequisitosData requisito)
            => _bl.AfBeneRequisitos_Actualizar(CodCliente, requisito);

        /// <summary>Elimina un requisito.</summary>
        [HttpDelete("AfBeneRequisitos_Eliminar")]
        public ErrorDto AfBeneRequisitos_Eliminar(int CodCliente, string cod_requisito)
            => _bl.AfBeneRequisitos_Eliminar(CodCliente, cod_requisito);
    }
}
