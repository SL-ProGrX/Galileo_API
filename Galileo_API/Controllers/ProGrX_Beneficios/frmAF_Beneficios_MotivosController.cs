using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Motivos de Beneficios (frmAF_Beneficios_Motivos).
    /// </summary>
    [Route("api/frmAF_Beneficios_Motivos")]
    [ApiController]
    [Authorize]
    public class FrmAfBeneficiosMotivosController : ControllerBase
    {
        private readonly FrmAfBeneficiosMotivosBL _bl;

        public FrmAfBeneficiosMotivosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosMotivosBL(config);
        }

        /// <summary>Lista de motivos con paginación, filtro y ordenamiento.</summary>
        [HttpGet("BeneficiosMotivos_Obtener")]
        public ErrorDto<BeneMotivosDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, string? filtros)
            => _bl.BeneficiosMotivos_Obtener(CodEmpresa, filtros);

        /// <summary>Exporta la lista de motivos aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("BeneficiosMotivos_Exportar")]
        public ErrorDto<List<BeneMotivos>> BeneficiosMotivos_Exportar(int CodEmpresa, string? filtros)
            => _bl.BeneficiosMotivos_Exportar(CodEmpresa, filtros);

        /// <summary>Inserta un motivo de beneficio.</summary>
        [HttpPost("BeneficiosMotivos_Agregar")]
        public ErrorDto BeneficiosMotivos_Agregar(int CodEmpresa, [FromBody] BeneMotivos request)
            => _bl.BeneficiosMotivos_Agregar(CodEmpresa, request);

        /// <summary>Actualiza un motivo de beneficio.</summary>
        [HttpPut("BeneficiosMotivos_Actualizar")]
        public ErrorDto BeneficiosMotivos_Actualizar(int CodEmpresa, [FromBody] BeneMotivos request)
            => _bl.BeneficiosMotivos_Actualizar(CodEmpresa, request);

        /// <summary>Elimina un motivo de beneficio.</summary>
        [HttpDelete("BeneficiosMotivos_Eliminar")]
        public ErrorDto BeneficiosMotivos_Eliminar(int CodEmpresa, string id)
            => _bl.BeneficiosMotivos_Eliminar(CodEmpresa, id);
    }
}
