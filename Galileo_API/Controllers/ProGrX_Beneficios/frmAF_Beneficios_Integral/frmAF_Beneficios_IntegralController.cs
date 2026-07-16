using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del formulario principal de Beneficios Integrales (frmAF_Beneficios_Integral).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfBeneficiosIntegralController : ControllerBase
    {
        private readonly FrmAfBeneficiosIntegralBL _bl;

        public FrmAfBeneficiosIntegralController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficiosIntegralBL(config);
        }

        /// <summary>Catálogos de tablas SYS/BENE.</summary>
        [Authorize]
        [HttpGet("Catalogo_Obtener")]
        public ErrorDto<List<CatalogosLista>> Catalogo_Obtener(int CodEmpresa, int tipo, int modulo)
            => _bl.Catalogo_Obtener(CodEmpresa, tipo, modulo);

        /// <summary>Lista de categorías de beneficios.</summary>
        [Authorize]
        [HttpGet("BeneIntegralCategorias_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneIntegralCategorias_Obtener(int CodCliente)
            => _bl.BeneIntegralCategorias_Obtener(CodCliente);

        /// <summary>Observaciones del beneficio.</summary>
        [Authorize]
        [HttpGet("BeneIntegralObservaciones_Obtener")]
        public ErrorDto<List<AfiBeneObservaciones>> BeneIntegralObservaciones_Obtener(int CodCliente, int consec, string cod_beneficio)
            => _bl.BeneIntegralObservaciones_Obtener(CodCliente, consec, cod_beneficio);

        /// <summary>Guarda una observación del beneficio.</summary>
        [Authorize]
        [HttpPost("BeneIntegralObservaciones_Guardar")]
        public ErrorDto BeneIntegralObservaciones_Guardar(int CodCliente, [FromBody] AfiBeneObservaciones observacion)
            => _bl.BeneIntegralObservaciones_Guardar(CodCliente, observacion);

        /// <summary>Elimina una observación del beneficio.</summary>
        [Authorize]
        [HttpDelete("BeneIntegralObservaciones_Eliminar")]
        public ErrorDto BeneIntegralObservaciones_Eliminar(int CodCliente, int id_observacion, string usuario)
            => _bl.BeneIntegralObservaciones_Eliminar(CodCliente, id_observacion, usuario);

        /// <summary>Bitácora del beneficio.</summary>
        [Authorize]
        [HttpGet("BitacoraBeneficioIntegral_Obtener")]
        public ErrorDto<List<BitacoraBeneficioIntegralDto>> BitacoraBeneficioIntegral_Obtener(int CodCliente, string Cod_Beneficio, int Consec)
            => _bl.BitacoraBeneficioIntegral_Obtener(CodCliente, Cod_Beneficio, Consec);

        /// <summary>Expediente del beneficio (tablas serializadas en JSON).</summary>
        [Authorize]
        [HttpGet("BeneIntegralRepExpediente_Obtener")]
        public ErrorDto<object> BeneIntegralRepExpediente_Obtener(int CodEmpresa, string cedula, int id_beneficio, string categoria)
            => _bl.BeneIntegralRepExpediente_Obtener(CodEmpresa, cedula, id_beneficio, categoria);

        /// <summary>Beneficios para aprobación masiva.</summary>
        [Authorize]
        [HttpGet("BeneficiosParaAprobacionMasiva_Obtener")]
        public ErrorDto<BeneConsultaDatosLista> BeneficiosParaAprobacionMasiva_Obtener(int CodEmpresa, string Categoria, string filtroString)
            => _bl.BeneficiosParaAprobacionMasiva_Obtener(CodEmpresa, Categoria, filtroString);

        /// <summary>Aprueba de forma masiva los beneficios seleccionados.</summary>
        [Authorize]
        [HttpPost("BeneIntegral_AprobacionMasiva")]
        public ErrorDto BeneIntegral_AprobacionMasiva(int CodEmpresa, string lista)
            => _bl.BeneIntegral_AprobacionMasiva(CodEmpresa, lista);

        /// <summary>Beneficios para control mensual.</summary>
        [Authorize]
        [HttpGet("BeneficiosControMensual_Obtener")]
        public ErrorDto<BeneConsultaDatosLista> BeneficiosControMensual_Obtener(int CodEmpresa, string Categoria, string filtroString)
            => _bl.BeneficiosControMensual_Obtener(CodEmpresa, Categoria, filtroString);

        /// <summary>Genera las solicitudes de depósito.</summary>
        [Authorize]
        [HttpPost("BeneSolicitudDeposito_Generar")]
        public ErrorDto BeneSolicitudDeposito_Generar(int CodEmpresa, string lista, int mes)
            => _bl.BeneSolicitudDeposito_Generar(CodEmpresa, lista, mes);

        /// <summary>Devuelve las solicitudes de depósito.</summary>
        [Authorize]
        [HttpPost("BeneSolicitudDeposito_Devolver")]
        public ErrorDto BeneSolicitudDeposito_Devolver(int CodEmpresa, string lista)
            => _bl.BeneSolicitudDeposito_Devolver(CodEmpresa, lista);

        /// <summary>Reporte de control mensual.</summary>
        [Authorize]
        [HttpGet("BeneficiosControMensual_Reporte")]
        public ErrorDto<BeneConsultaDatosLista> BeneficiosControMensual_Reporte(int CodEmpresa, string Categoria, string filtroString)
            => _bl.BeneficiosControMensual_Reporte(CodEmpresa, Categoria, filtroString);

        /// <summary>Grupos de beneficios de una categoría.</summary>
        [Authorize]
        [HttpGet("BeneficioGrupos_Obtener")]
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneficioGrupos_Obtener(int CodEmpresa, string Categoria)
            => _bl.BeneficioGrupos_Obtener(CodEmpresa, Categoria);

        /// <summary>Permisos del usuario para la categoría.</summary>
        [Authorize]
        [HttpGet("ValidaUsuarioBeneficios_Obtener")]
        public ErrorDto<BeneCategoriaPermisos> ValidaUsuarioBeneficios_Obtener(int CodEmpresa, string usuario, string cod_categoria)
            => _bl.ValidaUsuarioBeneficios_Obtener(CodEmpresa, usuario, cod_categoria);

        /// <summary>Envía la solicitud de bloqueo del asociado al Departamento de Cobros.</summary>
        [Authorize]
        [HttpPost("BeneSolicitudBloqueo_Enviar")]
        public async Task<ErrorDto> BeneSolicitudBloqueo_Enviar([FromBody] DocArchivoBeneIntegralDto info)
            => await _bl.BeneSolicitudBloqueo_Enviar(Convert.ToInt32(info.codCliente), info);
    }
}
