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
            _db = new FrmCrComitesAprobacionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Comites_Dropdown_Obtener(int CodEmpresa)
            => _db.CR_ComitesAprobaciones_Comites_Dropdown_Obtener(CodEmpresa);

        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Obtener(int CodEmpresa, int id_comite)
            => _db.CR_ComitesAprobaciones_Comite_Obtener(CodEmpresa, id_comite);

        public ErrorDto<CrComitesAprobacionesComite> CR_ComitesAprobaciones_Comite_Scroll(int CodEmpresa, int id_comite, int direccion)
            => _db.CR_ComitesAprobaciones_Comite_Scroll(CodEmpresa, id_comite, direccion);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Actas_Dropdown_Obtener(int CodEmpresa, int id_comite)
            => _db.CR_ComitesAprobaciones_Actas_Dropdown_Obtener(CodEmpresa, id_comite);

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(int CodEmpresa, string filtro)
            => _db.CR_ComitesAprobaciones_Usuarios_Dropdown_Obtener(CodEmpresa, filtro);

        public ErrorDto<CrComitesAprobacionesSolicitudesLista> CR_ComitesAprobaciones_Solicitudes_Obtener(int CodEmpresa, CrComitesAprobacionesSolicitudRequest request)
            => _db.CR_ComitesAprobaciones_Solicitudes_Obtener(CodEmpresa, request);

        public ErrorDto<CrComitesAprobacionesDetalle> CR_ComitesAprobaciones_Detalle_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _db.CR_ComitesAprobaciones_Detalle_Obtener(CodEmpresa, tipo_caso, operacion);

        public ErrorDto<CrComitesAprobacionesPatrimonio> CR_ComitesAprobaciones_Patrimonio_Obtener(int CodEmpresa, string cedula)
            => _db.CR_ComitesAprobaciones_Patrimonio_Obtener(CodEmpresa, cedula);

        public ErrorDto<CrComitesAprobacionesDeudasResponse> CR_ComitesAprobaciones_Deudas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string cedula)
            => _db.CR_ComitesAprobaciones_Deudas_Obtener(CodEmpresa, tipo_caso, operacion, cedula);

        public ErrorDto<CrComitesAprobacionesFianzasResponse> CR_ComitesAprobaciones_Fianzas_Obtener(int CodEmpresa, string cedula)
            => _db.CR_ComitesAprobaciones_Fianzas_Obtener(CodEmpresa, cedula);

        public ErrorDto<List<CrComitesAprobacionesRefundicion>> CR_ComitesAprobaciones_Refundiciones_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _db.CR_ComitesAprobaciones_Refundiciones_Obtener(CodEmpresa, tipo_caso, operacion);

        public ErrorDto<List<CrComitesAprobacionesDesembolso>> CR_ComitesAprobaciones_Desembolsos_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _db.CR_ComitesAprobaciones_Desembolsos_Obtener(CodEmpresa, tipo_caso, operacion);

        public ErrorDto<List<CrComitesAprobacionesSeguimiento>> CR_ComitesAprobaciones_Seguimiento_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _db.CR_ComitesAprobaciones_Seguimiento_Obtener(CodEmpresa, tipo_caso, operacion);

        public ErrorDto<List<CrComitesAprobacionesFiador>> CR_ComitesAprobaciones_Fiadores_Obtener(int CodEmpresa, string tipo_caso, string operacion)
            => _db.CR_ComitesAprobaciones_Fiadores_Obtener(CodEmpresa, tipo_caso, operacion);

        public ErrorDto<CrComitesAprobacionesFiadorDetalle> CR_ComitesAprobaciones_FiadorDetalle_Obtener(int CodEmpresa, string cedula, string estudioCredito)
            => _db.CR_ComitesAprobaciones_FiadorDetalle_Obtener(CodEmpresa, cedula, estudioCredito);

        public ErrorDto<List<CrComitesAprobacionesCausa>> CR_ComitesAprobaciones_Causas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string tipo)
            => _db.CR_ComitesAprobaciones_Causas_Obtener(CodEmpresa, tipo_caso, operacion, tipo);

        public ErrorDto CR_ComitesAprobaciones_Resolucion_Guardar(int CodEmpresa, CrComitesAprobacionesResolucionRequest request)
            => _db.CR_ComitesAprobaciones_Resolucion_Guardar(CodEmpresa, request);

        public ErrorDto CR_ComitesAprobaciones_Causas_Guardar(int CodEmpresa, CrComitesAprobacionesCausasGuardarRequest request)
            => _db.CR_ComitesAprobaciones_Causas_Guardar(CodEmpresa, request);

        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaActual_Obtener(int CodEmpresa, int id_comite, string acta)
            => _db.CR_ComitesAprobaciones_ActaActual_Obtener(CodEmpresa, id_comite, acta);

        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaNueva_Crear(int CodEmpresa, int id_comite, string usuario)
            => _db.CR_ComitesAprobaciones_ActaNueva_Crear(CodEmpresa, id_comite, usuario);

        public ErrorDto CR_ComitesAprobaciones_Acta_Guardar(int CodEmpresa, CrComitesAprobacionesActaGuardarRequest request)
            => _db.CR_ComitesAprobaciones_Acta_Guardar(CodEmpresa, request);

        public ErrorDto CR_ComitesAprobaciones_Acta_Cerrar(int CodEmpresa, int id_comite, string acta, string usuario)
            => _db.CR_ComitesAprobaciones_Acta_Cerrar(CodEmpresa, id_comite, acta, usuario);

        public ErrorDto<List<CrComitesAprobacionesActaAsistencia>> CR_ComitesAprobaciones_ActaAsistencia_Obtener(int CodEmpresa, int id_comite, string acta)
            => _db.CR_ComitesAprobaciones_ActaAsistencia_Obtener(CodEmpresa, id_comite, acta);

        public ErrorDto<List<CrComitesAprobacionesActaHistorico>> CR_ComitesAprobaciones_ActasHistorico_Obtener(int CodEmpresa, int id_comite, DateTime fecha_inicio, DateTime fecha_corte, string identificacion)
            => _db.CR_ComitesAprobaciones_ActasHistorico_Obtener(CodEmpresa, id_comite, fecha_inicio, fecha_corte, identificacion);

        public ErrorDto<List<CrComitesAprobacionesActaResolucion>> CR_ComitesAprobaciones_ActaResoluciones_Obtener(int CodEmpresa, int id_comite, string acta)
            => _db.CR_ComitesAprobaciones_ActaResoluciones_Obtener(CodEmpresa, id_comite, acta);
    }
}
