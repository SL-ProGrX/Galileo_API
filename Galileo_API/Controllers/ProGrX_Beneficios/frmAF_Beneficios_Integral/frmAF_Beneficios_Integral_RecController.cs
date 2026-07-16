using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Reconocimientos de Beneficios Integrales (FrmAfBeneficiosIntegralRec).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfBeneficiosIntegralRecController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralRecBL _bl;

        public FrmAfBeneficiosIntegralRecController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralRecBL(config);
        }

        /// <summary>Datos del reconocimiento asociado a un beneficio.</summary>
        [Authorize]
        [HttpGet("BeneReconocimiento_Obtener")]
        public ErrorDto<AfiBeneReconocimientosDatos> BeneReconocimiento_Obtener(int CodCliente, int id_beneficio)
            => _bl.BeneReconocimiento_Obtener(CodCliente, id_beneficio);

        /// <summary>Guarda (inserta o actualiza) el reconocimiento.</summary>
        [Authorize]
        [HttpPost("BeneReconocimiento_Guardar")]
        public ErrorDto BeneReconocimiento_Guardar(int CodCliente, [FromBody] AfiBeneReconocimientos reconocimiento)
            => _bl.BeneReconocimiento_Guardar(CodCliente, reconocimiento);

        /// <summary>Rechaza el expediente del reconocimiento.</summary>
        [Authorize]
        [HttpPost("BeneReconocimiento_Rechazar")]
        public ErrorDto BeneReconocimiento_Rechazar(int CodCliente, int id_beneficio, string usuario)
            => _bl.BeneReconocimiento_Rechazar(CodCliente, id_beneficio, usuario);

        /// <summary>Valida si el estudiante ya está registrado en otro reconocimiento.</summary>
        [Authorize]
        [HttpGet("ValidaEstudiante_Obtener")]
        public ErrorDto ValidaEstudiante_Obtener(int CodCliente, string cedula, string id_beneficio)
            => _bl.ValidaEstudiante_Obtener(CodCliente, cedula, id_beneficio);

        /// <summary>Nota mínima configurada para el beneficio.</summary>
        [Authorize]
        [HttpGet("ValidaNotaMinima_Obtener")]
        public ErrorDto ValidaNotaMinima_Obtener(int CodCliente)
            => _bl.ValidaNotaMinima(CodCliente);

        /// <summary>Nota mínima para pasar la materia.</summary>
        [Authorize]
        [HttpGet("ValidaNotaPasaMateria_Obtener")]
        public ErrorDto ValidaNotaPasaMateria_Obtener(int CodCliente)
            => _bl.ValidaNotaPasaMateria(CodCliente);
    }
}
