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
    [Authorize]
    public class FrmCRComitesAprobacionesController : ControllerBase
    {
        private readonly FrmCrComitesAprobacionesBL _bl;
        private const string DatosRequeridos = "Datos requeridos.";

        public FrmCRComitesAprobacionesController(IConfiguration config)
        {
            _bl = new FrmCrComitesAprobacionesBL(config);
        }

        [HttpGet("CR_ComitesAprobaciones_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Comites_Dropdown_Obtener(int CodEmpresa)
            => _bl.CR_ComitesAprobaciones_Comites_Dropdown_Obtener(CodEmpresa);

        [HttpGet("CR_ComitesAprobaciones_Comite_Obtener")]
        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Obtener(int CodEmpresa, int id_comite)
            => _bl.CR_ComitesAprobaciones_Comite_Obtener(CodEmpresa, id_comite);

        [HttpGet("CR_ComitesAprobaciones_Comite_Scroll")]
        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Scroll(int CodEmpresa, int id_comite, int direccion)
            => _bl.CR_ComitesAprobaciones_Comite_Scroll(CodEmpresa, id_comite, direccion);

        [HttpGet("CR_ComitesAprobaciones_Actas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Actas_Dropdown_Obtener(int CodEmpresa, int id_comite)
            => _bl.CR_ComitesAprobaciones_Actas_Dropdown_Obtener(CodEmpresa, id_comite);

        [HttpGet("CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(int CodEmpresa, string filtro = "")
            => _bl.CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(CodEmpresa, filtro ?? string.Empty);

        [HttpPost("CR_ComitesAprobaciones_Solicitudes_Obtener")]
        public ErrorDto<CrComitesAprobacionesSolicitudesLista> CR_ComitesAprobaciones_Solicitudes_Obtener(int CodEmpresa, [FromBody] CrComitesAprobacionesSolicitudRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesSolicitudesLista>(DatosRequeridos, -2, new CrComitesAprobacionesSolicitudesLista());
            }

            return _bl.CR_ComitesAprobaciones_Solicitudes_Obtener(CodEmpresa, request);
        }

        [HttpGet("CR_ComitesAprobaciones_Detalle_Obtener")]
        public ErrorDto<CrComitesAprobacionesDetalle> CR_ComitesAprobaciones_Detalle_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _bl.CR_ComitesAprobaciones_Detalle_Obtener(CodEmpresa, tipo_caso, operacion);

        [HttpGet("CR_ComitesAprobaciones_Patrimonio_Obtener")]
        public ErrorDto<CrComitesAprobacionesPatrimonio> CR_ComitesAprobaciones_Patrimonio_Obtener(int CodEmpresa, string cedula)
            => _bl.CR_ComitesAprobaciones_Patrimonio_Obtener(CodEmpresa, cedula ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_Deudas_Obtener")]
        public ErrorDto<CrComitesAprobacionesDeudasResponse> CR_ComitesAprobaciones_Deudas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
            => _bl.CR_ComitesAprobaciones_Deudas_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, cedula ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_Fianzas_Obtener")]
        public ErrorDto<CrComitesAprobacionesFianzasResponse> CR_ComitesAprobaciones_Fianzas_Obtener(int CodEmpresa, string cedula)
            => _bl.CR_ComitesAprobaciones_Fianzas_Obtener(CodEmpresa, cedula ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_Refundiciones_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesRefundicion>> CR_ComitesAprobaciones_Refundiciones_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _bl.CR_ComitesAprobaciones_Refundiciones_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_Desembolsos_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesDesembolso>> CR_ComitesAprobaciones_Desembolsos_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _bl.CR_ComitesAprobaciones_Desembolsos_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_Seguimiento_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesSeguimiento>> CR_ComitesAprobaciones_Seguimiento_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _bl.CR_ComitesAprobaciones_Seguimiento_Obtener(CodEmpresa, tipo_caso, operacion);

        [HttpGet("CR_ComitesAprobaciones_Fiadores_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesFiador>> CR_ComitesAprobaciones_Fiadores_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _bl.CR_ComitesAprobaciones_Fiadores_Obtener(CodEmpresa, tipo_caso, operacion);

        [HttpGet("CR_ComitesAprobaciones_FiadorDetalle_Obtener")]
        public ErrorDto<CrComitesAprobacionesFiadorDetalle> CR_ComitesAprobaciones_FiadorDetalle_Obtener(int CodEmpresa, string cedula, string estudioCredito)
            => _bl.CR_ComitesAprobaciones_FiadorDetalle_Obtener(CodEmpresa, cedula, estudioCredito);

        [HttpGet("CR_ComitesAprobaciones_Causas_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesCausa>> CR_ComitesAprobaciones_Causas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string tipo)
            => _bl.CR_ComitesAprobaciones_Causas_Obtener(CodEmpresa, tipo_caso, operacion, tipo);

        [HttpPost("CR_ComitesAprobaciones_Resolucion_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_Resolucion_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesResolucionRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return _bl.CR_ComitesAprobaciones_Resolucion_Guardar(CodEmpresa, request);
        }

        [HttpPost("CR_ComitesAprobaciones_Causas_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_Causas_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesCausasGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return _bl.CR_ComitesAprobaciones_Causas_Guardar(CodEmpresa, request);
        }

        [HttpGet("CR_ComitesAprobaciones_ActaActual_Obtener")]
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaActual_Obtener(int CodEmpresa, int id_comite, string acta)
            => _bl.CR_ComitesAprobaciones_ActaActual_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);

        [HttpPost("CR_ComitesAprobaciones_ActaNueva_Crear")]
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaNueva_Crear(int CodEmpresa, int id_comite, string usuario)
            => _bl.CR_ComitesAprobaciones_ActaNueva_Crear(CodEmpresa, id_comite, usuario ?? string.Empty);

        [HttpPost("CR_ComitesAprobaciones_Acta_Guardar")]
        public ErrorDto CR_ComitesAprobaciones_Acta_Guardar(int CodEmpresa, [FromBody] CrComitesAprobacionesActaGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(DatosRequeridos, -2);
            }

            return _bl.CR_ComitesAprobaciones_Acta_Guardar(CodEmpresa, request);
        }

        [HttpPost("CR_ComitesAprobaciones_Acta_Cerrar")]
        public ErrorDto CR_ComitesAprobaciones_Acta_Cerrar(int CodEmpresa, int id_comite, string acta, string usuario)
            => _bl.CR_ComitesAprobaciones_Acta_Cerrar(CodEmpresa, id_comite, acta ?? string.Empty, usuario ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_ActaAsistencia_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesActaAsistencia>> CR_ComitesAprobaciones_ActaAsistencia_Obtener(int CodEmpresa, int id_comite, string acta)
            => _bl.CR_ComitesAprobaciones_ActaAsistencia_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_ActasHistorico_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesActaHistorico>> CR_ComitesAprobaciones_ActasHistorico_Obtener(int CodEmpresa, int id_comite, DateTime fecha_inicio, DateTime fecha_corte, string identificacion = "")
            => _bl.CR_ComitesAprobaciones_ActasHistorico_Obtener(CodEmpresa, id_comite, fecha_inicio, fecha_corte, identificacion ?? string.Empty);

        [HttpGet("CR_ComitesAprobaciones_ActaResoluciones_Obtener")]
        public ErrorDto<List<CrComitesAprobacionesActaResolucion>> CR_ComitesAprobaciones_ActaResoluciones_Obtener(int CodEmpresa, int id_comite, string acta)
            => _bl.CR_ComitesAprobaciones_ActaResoluciones_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
    }
}
