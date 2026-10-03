using Galileo.DataBaseTier.ProGrX;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;

namespace Galileo.BusinessLogic.ProGrX
{
    public class FrmDsbColaboradorBL
    {
        private readonly FrmDsbColaboradorDB _db;

        public FrmDsbColaboradorBL(IConfiguration config)
        {
            _db = new FrmDsbColaboradorDB(config);
        }

        public ErrorDto<ColaboradorVinculoData> Colaborador_Vinculado_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return _db.Colaborador_Vinculado_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<ColaboradorPerfilData> Colaborador_Perfil_Obtener(
            int CodEmpresa,
            string usuario,
            string AppVersion)
        {
            return _db.Colaborador_Perfil_Obtener(
                CodEmpresa,
                usuario,
                AppVersion);
        }

        public ErrorDto<bool> Colaborador_Foto_Cambia(
            int CodEmpresa,
            string usuario,
            ColaboradorFotoCambiaRequest request)
        {
            return _db.Colaborador_Foto_Cambia(CodEmpresa, usuario, request);
        }

        public ErrorDto<List<ColaboradorEmpleadoOpcionData>> Colaborador_Consulta_Id(
            int CodEmpresa,
            string Identificacion,
            string? EmpleadoId)
        {
            return _db.Colaborador_Consulta_Id(
                CodEmpresa,
                Identificacion,
                EmpleadoId);
        }

        public ErrorDto<ColaboradorAccesoData> Colaborador_Acceso(
            int CodEmpresa,
            string usuario,
            ColaboradorAccesoRequest request)
        {
            return _db.Colaborador_Acceso(CodEmpresa, usuario, request);
        }

        public ErrorDto<ColaboradorClaveReestableceData> Colaborador_Clave_Reestablece(
            int CodEmpresa,
            string usuario,
            ColaboradorClaveReestableceRequest request)
        {
            return _db.Colaborador_Clave_Reestablece(
                CodEmpresa,
                usuario,
                request);
        }

        public ErrorDto<bool> Colaborador_Clave_Cambia(
            int CodEmpresa,
            string usuario,
            ColaboradorClaveCambiaRequest request)
        {
            return _db.Colaborador_Clave_Cambia(CodEmpresa, usuario, request);
        }

        public ErrorDto<List<Dictionary<string, object?>>> Colaborador_Menu_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorMenuRequest request)
        {
            return _db.Colaborador_Menu_Obtener(
                CodEmpresa,
                usuario,
                request);
        }

        public ErrorDto<bool> Colaborador_Autorizacion_Registrar(
            int CodEmpresa,
            string usuario,
            ColaboradorAutorizacionRequest request)
        {
            return _db.Colaborador_Autorizacion_Registrar(CodEmpresa, usuario, request);
        }

        public ErrorDto<bool> Colaborador_Traslado_Gestionar(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoGestionRequest request)
        {
            return _db.Colaborador_Traslado_Gestionar(CodEmpresa, usuario, request);
        }

        public ErrorDto<ColaboradorTrasladoConfiguracionData> Colaborador_Traslado_Configuracion_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoAccesoRequest request)
        {
            return _db.Colaborador_Traslado_Configuracion_Obtener(CodEmpresa, usuario, request);
        }

        public ErrorDto<ColaboradorTrasladoRegistroData> Colaborador_Traslado_Registrar(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoRegistrarRequest request)
        {
            return _db.Colaborador_Traslado_Registrar(CodEmpresa, usuario, request);
        }

        public ErrorDto<List<ColaboradorTrasladoPlacaData>> Colaborador_Traslado_Detalle_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoDetalleRequest request)
        {
            return _db.Colaborador_Traslado_Detalle_Obtener(CodEmpresa, usuario, request);
        }

        public ErrorDto<bool> Colaborador_Reporte_Validar(
            int CodEmpresa,
            string usuario,
            ColaboradorReporteRequest request)
        {
            return _db.Colaborador_Reporte_Validar(CodEmpresa, usuario, request);
        }

        public ErrorDto<bool> Colaborador_Reporte_Bitacora(
            int CodEmpresa,
            string usuario,
            ColaboradorReporteRequest request)
        {
            return _db.Colaborador_Reporte_Bitacora(CodEmpresa, usuario, request);
        }

        public ErrorDto<ColaboradorSolicitudConfiguracionData> Colaborador_Solicitud_Configuracion_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorSolicitudConfiguracionRequest request)
        {
            return _db.Colaborador_Solicitud_Configuracion_Obtener(
                CodEmpresa,
                usuario,
                request);
        }

        public ErrorDto<int> Colaborador_Solicitud_Dias_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorSolicitudDiasRequest request)
        {
            return _db.Colaborador_Solicitud_Dias_Obtener(
                CodEmpresa,
                usuario,
                request);
        }

        public ErrorDto<ColaboradorSolicitudRegistroData> Colaborador_Solicitud_Registrar(
            int CodEmpresa,
            string usuario,
            ColaboradorSolicitudRegistrarRequest request)
        {
            return _db.Colaborador_Solicitud_Registrar(
                CodEmpresa,
                usuario,
                request);
        }
    }
}
