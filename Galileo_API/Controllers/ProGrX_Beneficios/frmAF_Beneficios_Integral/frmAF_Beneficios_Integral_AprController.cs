using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Apremiantes de Beneficios Integrales (FrmAfBeneficiosIntegralApr).
    /// </summary>
    [Route("api/frmAF_Beneficios_Integral_Apr")]
    [ApiController]
    public class FrmAfBeneficiosIntegralAprController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralAprBL _bl;

        public FrmAfBeneficiosIntegralAprController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralAprBL(config);
        }

        /// <summary>Categorías de apremiantes.</summary>
        [Authorize]
        [HttpGet("CategoriaAPT_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> CategoriaAPT_Obtener(int CodCliente)
            => _bl.CategoriaAPT_Obtener(CodCliente);

        /// <summary>Profesionales de apremiantes.</summary>
        [Authorize]
        [HttpGet("ProfesionalAPT_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProfesionalAPT_Obtener(int CodCliente)
            => _bl.ProfesionalAPT_Obtener(CodCliente);

        /// <summary>Guarda un miembro del núcleo familiar.</summary>
        [Authorize]
        [HttpPost("MiembroFamiliar_Guardar")]
        public ErrorDto MiembroFamiliar_Guardar(int CodCliente, [FromBody] BeneIntNucleoFamDto miembro)
            => _bl.MiembroFamiliar_Guardar(CodCliente, miembro);

        /// <summary>Miembros del núcleo familiar del socio.</summary>
        [Authorize]
        [HttpGet("MiembrosFamiliar_Obtener")]
        public ErrorDto<List<BeneIntNucleoFamLista>> MiembrosFamiliar_Obtener(int CodCliente, string? cedula)
            => _bl.MiembrosFamiliar_Obtener(CodCliente, cedula);

        /// <summary>Elimina un miembro del núcleo familiar.</summary>
        [Authorize]
        [HttpDelete("MiembroFamiliar_Eliminar")]
        public ErrorDto MiembroFamiliar_Eliminar(int CodCliente, long id, string usuario)
            => _bl.MiembroFamiliar_Eliminar(CodCliente, id, usuario);

        /// <summary>Situación financiera del socio por tipo.</summary>
        [Authorize]
        [HttpGet("SituacionFinSocio_Obtener")]
        public ErrorDto<List<AfiBeneSocioFinanzas>> SituacionFinSocio_Obtener(int CodCliente, string? cedula, string tipo)
            => _bl.SituacionFinSocio_Obtener(CodCliente, cedula, tipo);

        /// <summary>Guarda la situación financiera del socio.</summary>
        [Authorize]
        [HttpPost("SituacionFinanciera_Guardar")]
        public ErrorDto SituacionFinanciera_Guardar(int CodCliente, [FromBody] AfiBeneSocioFinanzasGuardar finanza)
            => _bl.SituacionFinanciera_Guardar(CodCliente, finanza);

        /// <summary>Elimina un registro de situación financiera.</summary>
        [Authorize]
        [HttpDelete("SituacionFinanciera_Eliminar")]
        public ErrorDto SituacionFinanciera_Eliminar(int CodCliente, int id, string usuario)
            => _bl.SituacionFinanciera_Eliminar(CodCliente, id, usuario);

        /// <summary>Síntesis financiera del socio.</summary>
        [Authorize]
        [HttpGet("SintecisFinanciera_Obtener")]
        public ErrorDto<AfiBeneSintesisFinanzas> SintecisFinanciera_Obtener(int CodCliente, string? cedula)
            => _bl.SintecisFinanciera_Obtener(CodCliente, cedula);

        /// <summary>Comportamiento financiero del socio.</summary>
        [Authorize]
        [HttpGet("ComportamientoFinanciero_Obtener")]
        public ErrorDto<AfiBeneCompFinanciero> ComportamientoFinanciero_Obtener(int CodCliente, string cedula)
            => _bl.ComportamientoFinanciero_Obtener(CodCliente, cedula);

        /// <summary>Lista de motivos de justificación de la categoría.</summary>
        [Authorize]
        [HttpGet("BeneMotivoLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneMotivoLista_Obtener(int CodCliente, string? categoria)
            => _bl.BeneMotivoLista_Obtener(CodCliente, categoria);

        /// <summary>Justificaciones del expediente del socio.</summary>
        [Authorize]
        [HttpGet("BeneJustificaciones_Obtener")]
        public ErrorDto<List<AfiBeneApreJustificacion>> BeneJustificaciones_Obtener(int CodCliente, string cedula, int expediente)
            => _bl.BeneJustificaciones_Obtener(CodCliente, cedula, expediente);

        /// <summary>Guarda una justificación.</summary>
        [Authorize]
        [HttpPost("BeneJustificacion_Guardar")]
        public ErrorDto BeneJustificacion_Guardar(int CodCliente, [FromBody] AfiBeneApreJustificacionGuardar justificacion)
            => _bl.BeneJustificacion_Guardar(CodCliente, justificacion);

        /// <summary>Elimina una justificación.</summary>
        [Authorize]
        [HttpDelete("BeneJustificacion_Eliminar")]
        public ErrorDto BeneJustificacion_Eliminar(int CodCliente, int id_justificacion, string usuario)
            => _bl.BeneJustificacion_Eliminar(CodCliente, id_justificacion, usuario);

        /// <summary>Costo de manutención configurado.</summary>
        [Authorize]
        [HttpGet("CostoManutencion_Obtener")]
        public ErrorDto<float> CostoManutencion_Obtener(int CodCliente)
            => _bl.CostoManutencion_Obtener(CodCliente);

        /// <summary>Costo de deducción configurado.</summary>
        [Authorize]
        [HttpGet("CostoDeduccion_Obtener")]
        public ErrorDto<float> CostoDeduccion_Obtener(int CodCliente)
            => _bl.CostoDeduccion_Obtener(CodCliente);
    }
}
