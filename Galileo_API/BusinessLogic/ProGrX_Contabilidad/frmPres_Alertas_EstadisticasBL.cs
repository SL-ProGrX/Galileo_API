

using Galileo.DataBaseTier.ProGrX_Contabilidad;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using PgxAPI.Models.ProGrX_Contabilidad;

namespace Galileo.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmPresAlertasEstadisticasBL
    {
        private readonly FrmPresAlertasEstadisticasDB _DB;
        public FrmPresAlertasEstadisticasBL(IConfiguration config)
        {
            _DB = new FrmPresAlertasEstadisticasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PresAlertasEstadisticasTipos_Obtener(int CodEmpresa)
        {
            return _DB.PresAlertasEstadisticasTipos_Obtener(CodEmpresa);
        }

        public ErrorDto<PresVistaPresupuestoAlertasResponse> PresPlanning_Obtener(int CodCliente, string datos)
        {
            return _DB.PresPlanning_Obtener(CodCliente, datos);
        }

        public ErrorDto PresAlertaJustificacion_Guardar(int codEmpresa, PresAlertaJustificacionGuardarRequest data)
        {
            return _DB.PresAlertaJustificacion_Guardar(codEmpresa, data);
        }

        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBitacora_Obtener(
          PresAlertaJustificacionBitRequest resquest )
        {
            return _DB.PresAlertaJustificacionBitacora_Obtener(resquest);
        }

        public ErrorDto<List<PresAlertaTipoJustificacionData>> PresAlertaTipoJustificacion_Obtener(int codEmpresa, string tipoAlerta)
        {
            return _DB.PresAlertaTipoJustificacion_Obtener(codEmpresa, tipoAlerta);
        }

        #region Control de Justificaciones

        public ErrorDto PresAlertasControlExclusion_Guardar(int codEmpresa, PresAlertasControlExclusionGuardarRequest request)
        {
            return _DB.PresAlertasControlExclusion_Guardar(codEmpresa, request);
        }

        public ErrorDto<List<PresAlertasControlExclusionData>> PresAlertasControlExclusion_Obtener(int codEmpresa, PresAlertasControlExclusionFiltroRequest request)
        {
            return _DB.PresAlertasControlExclusion_Obtener(codEmpresa, request);
        }

        public ErrorDto PresAlertasControlExclusion_Eliminar(int codEmpresa, PresAlertasControlExclusionEliminarRequest request)
        {
            return _DB.PresAlertasControlExclusion_Eliminar(codEmpresa, request);
        }

        public ErrorDto<PresAlertasJustificaPeriodoData> PresAlertasJustificaPeriodo_Validar(int codEmpresa, PresAlertasJustificaPeriodoRequest request)
        {
            return _DB.PresAlertasJustificaPeriodo_Validar(codEmpresa, request);
        }

        public ErrorDto PresAlertasJustificaPeriodo_Abrir(int codEmpresa, PresAlertasJustificaPeriodoRequest request)
        {
            return _DB.PresAlertasJustificaPeriodo_Abrir(codEmpresa, request);
        }

        #endregion

        public ErrorDto<PresAlertasControlPeriodoEstadoData> PresAlertasControlPeriodo_Validar(int codEmpresa, PresAlertasControlPeriodoConfigRequest request)
        {
            return _DB.PresAlertasControlPeriodo_Validar(codEmpresa, request);
        }

        public ErrorDto PresAlertasControlPeriodo_Registrar(int codEmpresa, PresAlertasControlPeriodoConfigRequest request)
        {
            return _DB.PresAlertasControlPeriodo_Registrar(codEmpresa, request);
        }

        public ErrorDto<List<PresAlertasJustificaPeriodoConsultaData>> PresAlertasJustificaPeriodo_Obtener(
    int codEmpresa,
    PresAlertasJustificaPeriodoConsultaRequest request)
        {
            return _DB.PresAlertasJustificaPeriodo_Obtener(codEmpresa, request);
        }

        #region DashBoard

        public ErrorDto<PresAlertasDashboardResumenData> PresAlertasDashboardResumen_Obtener(int codEmpresa, PresAlertasDashboardFiltroRequest request)
        {
            return _DB.PresAlertasDashboardResumen_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<PresAlertasDashboardUnidadData>> PresAlertasDashboardUnidad_Obtener(int codEmpresa, PresAlertasDashboardFiltroRequest request)
        {
            return _DB.PresAlertasDashboardUnidad_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<PresAlertasDashboardJustificacionData>> PresAlertasDashboardJustificacion_Obtener(int codEmpresa, PresAlertasDashboardFiltroRequest request)
        {
            return _DB.PresAlertasDashboardJustificacion_Obtener(codEmpresa, request);
        }

        #endregion

    }

}
