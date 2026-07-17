using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Personas de Beneficios Integrales (FrmAfBeneficiosIntegralPer).
    /// </summary>
    [Route("api/frmAF_Beneficios_Integral_Per")]
    [ApiController]
    public class FrmAfBeneficiosIntegralPerController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralPerBL _bl;

        public FrmAfBeneficiosIntegralPerController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralPerBL(config);
        }

        /// <summary>Lista de estados civiles.</summary>
        [Authorize]
        [HttpGet("EstadoCivilLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoCivilLista_Obtener(int CodCliente)
            => _bl.EstadoCivilLista_Obtener(CodCliente);

        /// <summary>Lista de niveles académicos.</summary>
        [Authorize]
        [HttpGet("NivelAcademicoLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NivelAcademicoLista_Obtener(int CodCliente)
            => _bl.NivelAcademicoLista_Obtener(CodCliente);

        /// <summary>Lista de nacionalidades.</summary>
        [Authorize]
        [HttpGet("NacionalidadLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NacionalidadLista_Obtener(int CodCliente)
            => _bl.NacionalidadLista_Obtener(CodCliente);

        /// <summary>Lista de países.</summary>
        [Authorize]
        [HttpGet("PaisLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> PaisLista_Obtener(int CodCliente)
            => _bl.PaisLista_Obtener(CodCliente);

        /// <summary>Lista de provincias.</summary>
        [Authorize]
        [HttpGet("ProvinciaLista_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProvinciaLista_Obtener(int CodCliente)
            => _bl.ProvinciaLista_Obtener(CodCliente);

        /// <summary>Lista de estados laborales.</summary>
        [Authorize]
        [HttpGet("EstadoLaboral_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoLaboral_Obtener(int CodCliente)
            => _bl.EstadoLaboral_Obtener(CodCliente);

        /// <summary>Cuentas bancarias del socio.</summary>
        [Authorize]
        [HttpGet("Cuentas_Obtener")]
        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Usuario)
            => _bl.Cuentas_Obtener(CodCliente, Usuario);

        /// <summary>Datos de la persona (socio).</summary>
        [Authorize]
        [HttpGet("DatosPersona_Obtener")]
        public ErrorDto<AfiBeneficioIntegralPersonaData>? DatosPersona_Obtener(int CodCliente, string? cedula)
            => _bl.DatosPersona_Obtener(CodCliente, cedula);

        /// <summary>Valida si el socio existe.</summary>
        [Authorize]
        [HttpGet("ValidaSocioExiste_Obtener")]
        public ErrorDto ValidaSocioExiste_Obtener(int CodCliente, string cedula)
            => _bl.validaSocioExiste(CodCliente, cedula);

        /// <summary>Actualiza los datos de la persona.</summary>
        [Authorize]
        [HttpPost("Persona_Actualizar")]
        public ErrorDto Persona_Actualizar(int CodCliente, string cedula, string persona)
            => _bl.Persona_Actualizar(CodCliente, cedula, persona);

        /// <summary>Guarda (inserta o actualiza) un teléfono del socio.</summary>
        [Authorize]
        [HttpPost("Telefono_Guardar")]
        public ErrorDto Telefono_Guardar(int CodCliente, [FromBody] AfiBeneTelefonoGuardar telefono)
            => _bl.Telefono_Guardar(CodCliente, telefono);

        /// <summary>Teléfonos del socio.</summary>
        [Authorize]
        [HttpGet("Telefonos_Obtener")]
        public ErrorDto<List<AfiBeneTelefono>> Telefonos_Obtener(int CodCliente, string cedula)
            => _bl.Telefonos_Obtener(CodCliente, cedula);

        /// <summary>Elimina un teléfono del socio.</summary>
        [Authorize]
        [HttpDelete("Telefono_Eliminar")]
        public ErrorDto Telefono_Eliminar(int CodCliente, int id, string usuario)
            => _bl.Telefono_Eliminar(CodCliente, id, usuario);

        /// <summary>Ejecuta las validaciones de la persona.</summary>
        [Authorize]
        [HttpGet("ValidarPersona")]
        public ErrorDto ValidarPersona(int CodCliente, string cedula)
            => _bl.ValidarPersona(CodCliente, cedula);
    }
}