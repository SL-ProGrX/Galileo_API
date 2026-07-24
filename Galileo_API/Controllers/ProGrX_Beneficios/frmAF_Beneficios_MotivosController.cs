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

        /// <summary>Lista de motivos de beneficios.</summary>
        [Authorize]
        [HttpGet("BeneficiosMotivos_Obtener")]
        public ErrorDto<BeneMotivosDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
            => _bl.BeneficiosMotivos_Obtener(CodEmpresa, pagina, paginacion, filtro);

        /// <summary>Inserta un motivo de beneficio.</summary>
        [Authorize]
        [HttpPost("BeneficiosMotivos_Agregar")]
        public ErrorDto BeneficiosMotivos_Agregar(int CodEmpresa, [FromBody] BeneMotivos request)
            => _bl.BeneficiosMotivos_Agregar(CodEmpresa, request);

        /// <summary>Actualiza un motivo de beneficio.</summary>
        [Authorize]
        [HttpPut("BeneficiosMotivos_Actualizar")]
        public ErrorDto BeneficiosMotivos_Actualizar(int CodEmpresa, [FromBody] BeneMotivos request)
            => _bl.BeneficiosMotivos_Actualizar(CodEmpresa, request);

        /// <summary>Elimina un motivo de beneficio.</summary>
        [Authorize]
        [HttpDelete("BeneficiosMotivos_Eliminar")]
        public ErrorDto BeneficiosMotivos_Eliminar(int CodEmpresa, string id)
            => _bl.BeneficiosMotivos_Eliminar(CodEmpresa, id);
    }
}
