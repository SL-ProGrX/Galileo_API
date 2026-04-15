using Galileo.BusinessLogic.ProGrX_Contabilidad;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.Models.ProGrX_Contabilidad;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPresAlertasEstadisticasController : ControllerBase
    {
        
        private readonly FrmPresAlertasEstadisticasBL _BL;
        public FrmPresAlertasEstadisticasController(IConfiguration config)
        {
            _BL = new FrmPresAlertasEstadisticasBL(config);
        }

        [Authorize]
        [HttpGet("PresAlertasEstadisticasTipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PresAlertasEstadisticasTipos_Obtener(int CodEmpresa)
        {
            return _BL.PresAlertasEstadisticasTipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("PresPlanning_Obtener")]
        public ErrorDto<PresVistaPresupuestoAlertasResponse> PresPlanning_Obtener(int CodCliente, string datos)
        {
            return _BL.PresPlanning_Obtener(CodCliente, datos);
        }

        [Authorize]
        [HttpPost("PresAlertaJustificacion_Guardar")]
        public ErrorDto PresAlertaJustificacion_Guardar(int CodEmpresa, [FromBody] PresAlertaJustificacionGuardarRequest data)
        {
            return _BL.PresAlertaJustificacion_Guardar(CodEmpresa, data);
        }

        [Authorize]
        [HttpGet("PresAlertaJustificacionBitacora_Obtener")]
        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBitacora_Obtener(
          [FromQuery] PresAlertaJustificacionBitRequest resquest)
        {
            return _BL.PresAlertaJustificacionBitacora_Obtener(resquest);
        }

        [Authorize]
        [HttpGet("PresAlertaTipoJustificacion_Obtener")]
        public ErrorDto<List<PresAlertaTipoJustificacionData>> PresAlertaTipoJustificacion_Obtener(int CodEmpresa, string tipoAlerta)
        {
            return _BL.PresAlertaTipoJustificacion_Obtener(CodEmpresa, tipoAlerta);
        }

        #region Control de Justificaciones

        [Authorize]
        [HttpPost("PresAlertasControlExclusion_Guardar")]
        public ErrorDto PresAlertasControlExclusion_Guardar(int CodEmpresa, [FromBody] PresAlertasControlExclusionGuardarRequest request)
        {
            return _BL.PresAlertasControlExclusion_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasControlExclusion_Obtener")]
        public ErrorDto<List<PresAlertasControlExclusionData>> PresAlertasControlExclusion_Obtener(int CodEmpresa, [FromBody] PresAlertasControlExclusionFiltroRequest request)
        {
            return _BL.PresAlertasControlExclusion_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasControlExclusion_Eliminar")]
        public ErrorDto PresAlertasControlExclusion_Eliminar(int CodEmpresa, [FromBody] PresAlertasControlExclusionEliminarRequest request)
        {
            return _BL.PresAlertasControlExclusion_Eliminar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasJustificaPeriodo_Validar")]
        public ErrorDto<PresAlertasJustificaPeriodoData> PresAlertasJustificaPeriodo_Validar(int CodEmpresa, [FromBody] PresAlertasJustificaPeriodoRequest request)
        {
            return _BL.PresAlertasJustificaPeriodo_Validar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasJustificaPeriodo_Abrir")]
        public ErrorDto PresAlertasJustificaPeriodo_Abrir(int CodEmpresa, [FromBody] PresAlertasJustificaPeriodoRequest request)
        {
            return _BL.PresAlertasJustificaPeriodo_Abrir(CodEmpresa, request);
        }

        #endregion

        [Authorize]
        [HttpPost("PresAlertasControlPeriodo_Validar")]
        public ErrorDto<PresAlertasControlPeriodoEstadoData> PresAlertasControlPeriodo_Validar(int CodEmpresa, [FromBody] PresAlertasControlPeriodoConfigRequest request)
        {
            return _BL.PresAlertasControlPeriodo_Validar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasControlPeriodo_Registrar")]
        public ErrorDto PresAlertasControlPeriodo_Registrar(int CodEmpresa, [FromBody] PresAlertasControlPeriodoConfigRequest request)
        {
            return _BL.PresAlertasControlPeriodo_Registrar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasJustificaPeriodo_Obtener")]
        public ErrorDto<List<PresAlertasJustificaPeriodoConsultaData>> PresAlertasJustificaPeriodo_Obtener(
    int CodEmpresa,
    [FromBody] PresAlertasJustificaPeriodoConsultaRequest request)
        {
            return _BL.PresAlertasJustificaPeriodo_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasDashboardResumen_Obtener")]
        public ErrorDto<PresAlertasDashboardResumenData> PresAlertasDashboardResumen_Obtener(int CodEmpresa, [FromBody] PresAlertasDashboardFiltroRequest request)
        {
            return _BL.PresAlertasDashboardResumen_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasDashboardUnidad_Obtener")]
        public ErrorDto<List<PresAlertasDashboardUnidadData>> PresAlertasDashboardUnidad_Obtener(int CodEmpresa, [FromBody] PresAlertasDashboardFiltroRequest request)
        {
            return _BL.PresAlertasDashboardUnidad_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasDashboardJustificacion_Obtener")]
        public ErrorDto<List<PresAlertasDashboardJustificacionData>> PresAlertasDashboardJustificacion_Obtener(int CodEmpresa, [FromBody] PresAlertasDashboardFiltroRequest request)
        {
            return _BL.PresAlertasDashboardJustificacion_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("PresAlertasControlPeriodo_ActualizarBloqueo")]
        public ErrorDto PresAlertasControlPeriodo_ActualizarBloqueo(int CodEmpresa, [FromBody] PresAlertasControlPeriodoBloqueoActualizarRequest request)
        {
            return _BL.PresAlertasControlPeriodo_ActualizarBloqueo(CodEmpresa, request);
        }

    }
}
