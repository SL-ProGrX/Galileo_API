using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de la Carga por Lote de Beneficios (frmAF_BeneficiosCargaLote).
    /// </summary>
    [Route("api/frmAF_BeneficiosCargaLote")]
    [ApiController]
    public class FrmAfBeneficiosCargaLoteController : ControllerBase
    {
        private readonly FrmAfBeneficiosCargaLoteBL _bl;

        public FrmAfBeneficiosCargaLoteController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosCargaLoteBL(config);
        }

        /// <summary>Inserta un lote de beneficios.</summary>
        [Authorize]
        [HttpPost("Beneficio_Lote_Carga_Insertar")]
        public ErrorDto Beneficio_Lote_Carga_Insertar(int CodEmpresa, [FromBody] string beneficio)
            => _bl.Beneficio_Lote_Carga_Insertar(CodEmpresa, beneficio);

        /// <summary>Obtiene la revisión de un lote cargado.</summary>
        [Authorize]
        [HttpGet("Beneficio_Lote_Revisa_Obtener")]
        public ErrorDto<List<AfiBeneCargaLoteData>> Beneficio_Lote_Revisa_Obtener(int CodEmpresa, string cod_beneficio, string usuario)
            => _bl.Beneficio_Lote_Revisa_Obtener(CodEmpresa, cod_beneficio, usuario);

        /// <summary>Procesa un lote de beneficios.</summary>
        [Authorize]
        [HttpPost("Beneficio_Lote_Procesa")]
        public ErrorDto Beneficio_Lote_Procesa(int CodEmpresa, string cod_beneficio, string usuario, string Formato)
            => _bl.Beneficio_Lote_Procesa(CodEmpresa, cod_beneficio, usuario, Formato);
    }
}
