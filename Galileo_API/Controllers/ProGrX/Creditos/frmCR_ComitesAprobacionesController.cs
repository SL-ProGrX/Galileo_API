using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRComitesAprobacionesController : ControllerBase
    {
        private readonly FrmCrComitesAprobacionesBL BL;
        private const string DatosRequeridos = "Datos requeridos.";

        public FrmCRComitesAprobacionesController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            BL = new FrmCrComitesAprobacionesBL(config);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_ComitesAprobaciones_Comites_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Comite_Obtener")]
        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Obtener(int CodEmpresa, int id_comite)
        {
            return BL.CR_ComitesAprobaciones_Comite_Obtener(CodEmpresa, id_comite);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Comite_Scroll")]
        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Scroll(int CodEmpresa, int id_comite, int direccion)
        {
            return BL.CR_ComitesAprobaciones_Comite_Scroll(CodEmpresa, id_comite, direccion);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Actas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Actas_Dropdown_Obtener(int CodEmpresa, int id_comite)
        {
            return BL.CR_ComitesAprobaciones_Actas_Dropdown_Obtener(CodEmpresa, id_comite);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(int CodEmpresa, string filtro = "")
        {
            return BL.CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(CodEmpresa, filtro ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Socios_Dropdown_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesSocio>> CR_ComitesAprobaciones_Socios_Dropdown_Obtener(int CodEmpresa, string filtro = "")
        {
            return BL.CR_ComitesAprobaciones_Socios_Dropdown_Obtener(CodEmpresa, filtro ?? string.Empty);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_Solicitudes_Obtener")]
        public ErrorDto<CrComitesAprobacionesSolicitudesLista> CR_ComitesAprobaciones_Solicitudes_Obtener(int CodEmpresa, [FromBody] CrComitesAprobacionesSolicitudRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesSolicitudesLista>(DatosRequeridos, -2, new CrComitesAprobacionesSolicitudesLista());
            }

            return BL.CR_ComitesAprobaciones_Solicitudes_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Detalle_Obtener")]
        public ErrorDto<CrComitesAprobacionesDetalle> CR_ComitesAprobaciones_Detalle_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return BL.CR_ComitesAprobaciones_Detalle_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Patrimonio_Obtener")]
        public ErrorDto<CrComitesAprobacionesPatrimonio> CR_ComitesAprobaciones_Patrimonio_Obtener(int CodEmpresa, string cedula)
        {
            return BL.CR_ComitesAprobaciones_Patrimonio_Obtener(CodEmpresa, cedula ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Clasificacion_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesClasificacion>> CR_ComitesAprobaciones_Clasificacion_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
        {
            return BL.CR_ComitesAprobaciones_Clasificacion_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, cedula ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Deudas_Obtener")]
        public ErrorDto<CrComitesAprobacionesDeudasResponse> CR_ComitesAprobaciones_Deudas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
        {
            return BL.CR_ComitesAprobaciones_Deudas_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, cedula ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Fianzas_Obtener")]
        public ErrorDto<CrComitesAprobacionesFianzasResponse> CR_ComitesAprobaciones_Fianzas_Obtener(int CodEmpresa, string cedula)
        {
            return BL.CR_ComitesAprobaciones_Fianzas_Obtener(CodEmpresa, cedula ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Refundiciones_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesRefundicion>> CR_ComitesAprobaciones_Refundiciones_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return BL.CR_ComitesAprobaciones_Refundiciones_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Desembolsos_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesDesembolso>> CR_ComitesAprobaciones_Desembolsos_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return BL.CR_ComitesAprobaciones_Desembolsos_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Seguimiento_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesSeguimiento>> CR_ComitesAprobaciones_Seguimiento_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return BL.CR_ComitesAprobaciones_Seguimiento_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Fiadores_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesFiador>> CR_ComitesAprobaciones_Fiadores_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return BL.CR_ComitesAprobaciones_Fiadores_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_FiadorDetalle_Obtener")]
        public ErrorDto<CrComitesAprobacionesFiadorDetalle> CR_ComitesAprobaciones_FiadorDetalle_Obtener(int CodEmpresa, string cedula, string estudioCredito)
        {
            return BL.CR_ComitesAprobaciones_FiadorDetalle_Obtener(CodEmpresa, cedula ?? string.Empty, estudioCredito ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_Causas_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesCausa>> CR_ComitesAprobaciones_Causas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string tipo)
        {
            return BL.CR_ComitesAprobaciones_Causas_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, tipo ?? string.Empty);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_Resolucion_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_Resolucion_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesResolucionRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return BL.CR_ComitesAprobaciones_Resolucion_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_Causas_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_Causas_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesCausasGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return BL.CR_ComitesAprobaciones_Causas_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_ActaActual_Obtener")]
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaActual_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            return BL.CR_ComitesAprobaciones_ActaActual_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_ActaNueva_Crear")]
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaNueva_Crear(int CodEmpresa, int id_comite, string usuario)
        {
            return BL.CR_ComitesAprobaciones_ActaNueva_Crear(CodEmpresa, id_comite, usuario ?? string.Empty);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_Acta_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_Acta_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesActaGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return BL.CR_ComitesAprobaciones_Acta_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_Acta_Cerrar")]
        public ErrorDto CR_ComitesAprobaciones_Acta_Cerrar(int CodEmpresa, int id_comite, string acta, string usuario)
        {
            return BL.CR_ComitesAprobaciones_Acta_Cerrar(CodEmpresa, id_comite, acta ?? string.Empty, usuario ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_ActaAsistencia_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesActaAsistencia>> CR_ComitesAprobaciones_ActaAsistencia_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            return BL.CR_ComitesAprobaciones_ActaAsistencia_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
        }

        [Authorize]
        [HttpPost("CR_ComitesAprobaciones_ActaAsistencia_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_ActaAsistencia_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesActaAsistenciaGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return BL.CR_ComitesAprobaciones_ActaAsistencia_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_ActasHistorico_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesActaHistorico>> CR_ComitesAprobaciones_ActasHistorico_Obtener(int CodEmpresa, int id_comite, DateTime fecha_inicio, DateTime fecha_corte, string identificacion = "")
        {
            return BL.CR_ComitesAprobaciones_ActasHistorico_Obtener(CodEmpresa, id_comite, fecha_inicio, fecha_corte, identificacion ?? string.Empty);
        }

        [Authorize]
        [HttpGet("CR_ComitesAprobaciones_ActaResoluciones_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesActaResolucion>> CR_ComitesAprobaciones_ActaResoluciones_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            return BL.CR_ComitesAprobaciones_ActaResoluciones_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
        }
    }
}
