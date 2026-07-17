using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del proceso Generales de Beneficios Integrales (FrmAfBeneficiosIntegralGen).
    /// </summary>
    [Route("api/frmAF_Beneficios_Integral_Gen")]
    [ApiController]
    public class FrmAfBeneficiosIntegralGenController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralGenBL _bl;

        public FrmAfBeneficiosIntegralGenController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralGenBL(config);
        }

        /// <summary>Lista de beneficios de una categoría.</summary>
        [Authorize]
        [HttpGet("BeneficiosLista_Obtener")]
        public ErrorDto<List<BeneficiosLista>> BeneficiosLista_Obtener(int CodCliente, string categoria)
            => _bl.BeneficiosLista_Obtener(CodCliente, categoria);

        /// <summary>Productos asignados al beneficio.</summary>
        [Authorize]
        [HttpGet("BeneIntegralGenProductos_Obtener")]
        public ErrorDto<List<AfiBenProductoDto>> BeneIntegralGenProductos_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _bl.BeneIntegralGenProductos_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Datos generales del beneficio.</summary>
        [Authorize]
        [HttpGet("BeneficioIntegralGeneral_Obtener")]
        public ErrorDto<BeneficioGeneral> BeneficioIntegralGeneral_Obtener(int CodCliente, int? id_beneficio)
            => _bl.BeneficioIntegralGeneral_Obtener(CodCliente, id_beneficio);

        /// <summary>[PENDIENTE] Guardado central del beneficio (requiere dependencias no migradas).</summary>
        [Authorize]
        [HttpPost("BeneficioIntegralGeneral_Guardar")]
        public Task<ErrorDto<BeneficioGeneralDatos>> BeneficioIntegralGeneral_Guardar(int CodCliente, string fuente, [FromBody] BeneficioGeneralDatos beneficioGeneral)
            => _bl.BeneficioIntegralGeneral_Guardar(CodCliente, fuente, beneficioGeneral);

        /// <summary>Valida si el socio ya está en el programa Crece.</summary>
        [Authorize]
        [HttpGet("ValidaProgramaCrece_Obtener")]
        public ErrorDto ValidaProgramaCrece_Obtener(int CodCliente, string cedula)
            => _bl.ValidaProgramaCrece(CodCliente, cedula);

        /// <summary>[PENDIENTE] Notificación de resolución por correo (requiere servidor de reportes).</summary>
        [Authorize]
        [HttpPost("BeneficioNotificaResolucion_Enviar")]
        public Task<ErrorDto> BeneficioNotificaResolucion_Enviar([FromBody] List<DocArchivoBeneIntegralDto> info)
            => _bl.BeneficioNotificaResolucion_Enviar(info);

        /// <summary>Valida si el estado es de resolución del expediente.</summary>
        [Authorize]
        [HttpGet("ValidaEstadoExpediente_Obtener")]
        public ErrorDto ValidaEstadoExpediente_Obtener(int CodCliente, string estado, string categoria)
            => _bl.ValidaEstadoExpediente(CodCliente, estado, categoria);

        /// <summary>Lista de profesionales APT.</summary>
        [Authorize]
        [HttpGet("AfiBeneProfesionales_Obtener")]
        public ErrorDto<List<BeneApreLista>> AfiBeneProfesionales_Obtener(int CodCliente)
            => _bl.AfiBeneProfesionales_Obtener(CodCliente);

        /// <summary>Lista de categorías APT.</summary>
        [Authorize]
        [HttpGet("AfiBeneCategorias_Obtener")]
        public ErrorDto<List<BeneApreLista>> AfiBeneCategorias_Obtener(int CodCliente)
            => _bl.AfiBeneCategorias_Obtener(CodCliente);

        /// <summary>Valida si el beneficio requiere justificación.</summary>
        [Authorize]
        [HttpGet("ValidaRequiereJustificacion_Obtener")]
        public ErrorDto ValidaRequiereJustificacion(int CodCliente, string cedula, string beneficio)
            => _bl.ValidaRequiereJustificacion(CodCliente, cedula, beneficio);

        /// <summary>Tipo de beneficio.</summary>
        [Authorize]
        [HttpGet("ValidaTipoBeneficio_Obtener")]
        public ErrorDto<string> ValidaTipoBeneficio_Obtener(int CodCliente, string? cod_beneficio)
            => _bl.ValidaTipoBeneficio(CodCliente, cod_beneficio);

        /// <summary>Registro de mora del beneficio.</summary>
        [Authorize]
        [HttpGet("BeneRegistroMora_Obtener")]
        public ErrorDto<BeneRegistroMoraDto> BeneRegistroMora_Obtener(int CodCliente, int consec, string beneficio)
            => _bl.BeneRegistroMora_Obtener(CodCliente, consec, beneficio);

        /// <summary>Guarda el registro de mora del beneficio.</summary>
        [Authorize]
        [HttpPost("BeneRegistroMora_Guardar")]
        public ErrorDto BeneRegistroMora_Guardar(int CodCliente, [FromBody] BeneRegistroMoraGuardar cobroMora)
            => _bl.BeneRegistroMora_Guardar(CodCliente, cobroMora);

        /// <summary>[PENDIENTE] Envío de boleta de cobro de mora por correo (requiere servidor de reportes).</summary>
        [Authorize]
        [HttpPost("BeneRegistroMora_Enviar")]
        public Task<ErrorDto> BeneRegistroMora_Enviar([FromBody] DocArchivoBeneIntegralDto info)
            => _bl.BeneRegistroMora_Enviar(Convert.ToInt32(info.codCliente), info);

        /// <summary>Valida si la persona figura como fallecida.</summary>
        [Authorize]
        [HttpGet("ValidaFallecido_Obtener")]
        public ErrorDto ValidaFallecido_Obtener(int CodCliente, string cedula)
            => _bl.ValidaFallecido(CodCliente, cedula);
    }
}