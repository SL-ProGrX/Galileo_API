using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Estados de Beneficios (frmAF_Beneficios_Estados).
    /// </summary>
    [Route("api/frmAF_Beneficios_Estados")]
    [ApiController]
    [Authorize]
    public class FrmAfBeneficiosEstadosController : ControllerBase
    {
        private readonly FrmAfBeneficiosEstadosBL _bl;

        public FrmAfBeneficiosEstadosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosEstadosBL(config);
        }

        /// <summary>Lista de estados de beneficios con paginación, filtro y ordenamiento.</summary>
        [HttpGet("BeneficiosEstados_Obtener")]
        public ErrorDto<BeneEstadoDataLista> BeneficiosEstados_Obtener(int CodEmpresa, string? filtros)
            => _bl.BeneficiosEstados_Obtener(CodEmpresa, filtros);

        /// <summary>Exporta la lista de estados aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("BeneficiosEstados_Exportar")]
        public ErrorDto<List<BeneEstado>> BeneficiosEstados_Exportar(int CodEmpresa, string? filtros)
            => _bl.BeneficiosEstados_Exportar(CodEmpresa, filtros);

        /// <summary>Inserta un estado de beneficio.</summary>
        [HttpPost("BeneficiosEstados_Agregar")]
        public ErrorDto BeneficiosEstados_Agregar(int CodEmpresa, [FromBody] BeneEstado request)
            => _bl.BeneficiosEstados_Agregar(CodEmpresa, request);

        /// <summary>Actualiza un estado de beneficio.</summary>
        [HttpPut("BeneficiosEstados_Actualizar")]
        public ErrorDto BeneficiosEstados_Actualizar(int CodEmpresa, [FromBody] BeneEstado request)
            => _bl.BeneficiosEstados_Actualizar(CodEmpresa, request);

        /// <summary>Elimina un estado de beneficio.</summary>
        [HttpDelete("BeneficiosEstados_Eliminar")]
        public ErrorDto BeneficiosEstados_Eliminar(int CodEmpresa, string id)
            => _bl.BeneficiosEstados_Eliminar(CodEmpresa, id);
    }
}
