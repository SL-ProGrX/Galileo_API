using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComitesAprobacionesBL
    {
        private readonly FrmCrComitesAprobacionesDB _db;

        public FrmCrComitesAprobacionesBL(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _db = new FrmCrComitesAprobacionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CR_ComitesAprobaciones_Comites_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Obtener(int CodEmpresa, int id_comite)
        {
            return _db.CR_ComitesAprobaciones_Comite_Obtener(CodEmpresa, id_comite);
        }

        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Scroll(int CodEmpresa, int id_comite, int direccion)
        {
            return _db.CR_ComitesAprobaciones_Comite_Scroll(CodEmpresa, id_comite, direccion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Actas_Dropdown_Obtener(int CodEmpresa, int id_comite)
        {
            return _db.CR_ComitesAprobaciones_Actas_Dropdown_Obtener(CodEmpresa, id_comite);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(int CodEmpresa, string filtro)
        {
            return _db.CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(CodEmpresa, filtro ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesSocio>> CR_ComitesAprobaciones_Socios_Dropdown_Obtener(int CodEmpresa, string filtro)
        {
            return _db.CR_ComitesAprobaciones_Socios_Dropdown_Obtener(CodEmpresa, filtro ?? string.Empty);
        }

        public ErrorDto<CrComitesAprobacionesSolicitudesLista> CR_ComitesAprobaciones_Solicitudes_Obtener(int CodEmpresa, CrComitesAprobacionesSolicitudRequest request)
        {
            return _db.CR_ComitesAprobaciones_Solicitudes_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrComitesAprobacionesDetalle> CR_ComitesAprobaciones_Detalle_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return _db.CR_ComitesAprobaciones_Detalle_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        public ErrorDto<CrComitesAprobacionesPatrimonio> CR_ComitesAprobaciones_Patrimonio_Obtener(int CodEmpresa, string cedula)
        {
            return _db.CR_ComitesAprobaciones_Patrimonio_Obtener(CodEmpresa, cedula ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesClasificacion>> CR_ComitesAprobaciones_Clasificacion_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
        {
            return _db.CR_ComitesAprobaciones_Clasificacion_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, cedula ?? string.Empty);
        }

        public ErrorDto<CrComitesAprobacionesDeudasResponse> CR_ComitesAprobaciones_Deudas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
        {
            return _db.CR_ComitesAprobaciones_Deudas_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, cedula ?? string.Empty);
        }

        public ErrorDto<CrComitesAprobacionesFianzasResponse> CR_ComitesAprobaciones_Fianzas_Obtener(int CodEmpresa, string cedula)
        {
            return _db.CR_ComitesAprobaciones_Fianzas_Obtener(CodEmpresa, cedula ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesRefundicion>> CR_ComitesAprobaciones_Refundiciones_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return _db.CR_ComitesAprobaciones_Refundiciones_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesDesembolso>> CR_ComitesAprobaciones_Desembolsos_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return _db.CR_ComitesAprobaciones_Desembolsos_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesSeguimiento>> CR_ComitesAprobaciones_Seguimiento_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return _db.CR_ComitesAprobaciones_Seguimiento_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesFiador>> CR_ComitesAprobaciones_Fiadores_Obtener(int CodEmpresa, string tipo_caso, string operacion)
        {
            return _db.CR_ComitesAprobaciones_Fiadores_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty);
        }

        public ErrorDto<CrComitesAprobacionesFiadorDetalle> CR_ComitesAprobaciones_FiadorDetalle_Obtener(int CodEmpresa, string cedula, string estudioCredito)
        {
            return _db.CR_ComitesAprobaciones_FiadorDetalle_Obtener(CodEmpresa, cedula ?? string.Empty, estudioCredito ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesCausa>> CR_ComitesAprobaciones_Causas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string tipo)
        {
            return _db.CR_ComitesAprobaciones_Causas_Obtener(CodEmpresa, tipo_caso ?? string.Empty, operacion ?? string.Empty, tipo ?? string.Empty);
        }

        public ErrorDto CR_ComitesAprobaciones_Resolucion_Guardar(int CodEmpresa, CrComitesAprobacionesResolucionRequest request)
        {
            return _db.CR_ComitesAprobaciones_Resolucion_Guardar(CodEmpresa, request);
        }

        public ErrorDto CR_ComitesAprobaciones_Causas_Guardar(int CodEmpresa, CrComitesAprobacionesCausasGuardarRequest request)
        {
            return _db.CR_ComitesAprobaciones_Causas_Guardar(CodEmpresa, request);
        }

        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaActual_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            return _db.CR_ComitesAprobaciones_ActaActual_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
        }

        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaNueva_Crear(int CodEmpresa, int id_comite, string usuario)
        {
            return _db.CR_ComitesAprobaciones_ActaNueva_Crear(CodEmpresa, id_comite, usuario ?? string.Empty);
        }

        public ErrorDto CR_ComitesAprobaciones_Acta_Guardar(int CodEmpresa, CrComitesAprobacionesActaGuardarRequest request)
        {

            return _db.CR_ComitesAprobaciones_Acta_Guardar(CodEmpresa, request);
        }

        public ErrorDto CR_ComitesAprobaciones_Acta_Cerrar(int CodEmpresa, int id_comite, string acta, string usuario)
        {
            return _db.CR_ComitesAprobaciones_Acta_Cerrar(CodEmpresa, id_comite, acta ?? string.Empty, usuario ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesActaAsistencia>> CR_ComitesAprobaciones_ActaAsistencia_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            return _db.CR_ComitesAprobaciones_ActaAsistencia_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
        }

        public ErrorDto CR_ComitesAprobaciones_ActaAsistencia_Guardar(int CodEmpresa, CrComitesAprobacionesActaAsistenciaGuardarRequest request)
        {
            return _db.CR_ComitesAprobaciones_ActaAsistencia_Guardar(CodEmpresa, request);
        }

        public ErrorDto<List<CrComitesAprobacionesActaHistorico>> CR_ComitesAprobaciones_ActasHistorico_Obtener(int CodEmpresa, int id_comite, DateTime fecha_inicio, DateTime fecha_corte, string identificacion)
        {
            return _db.CR_ComitesAprobaciones_ActasHistorico_Obtener(CodEmpresa, id_comite, fecha_inicio, fecha_corte, identificacion ?? string.Empty);
        }

        public ErrorDto<List<CrComitesAprobacionesActaResolucion>> CR_ComitesAprobaciones_ActaResoluciones_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            return _db.CR_ComitesAprobaciones_ActaResoluciones_Obtener(CodEmpresa, id_comite, acta ?? string.Empty);
        }
    }
}
