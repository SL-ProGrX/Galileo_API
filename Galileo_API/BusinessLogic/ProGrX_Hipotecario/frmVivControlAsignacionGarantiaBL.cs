using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivControlAsignacionGarantiaBl
    {
        private readonly FrmVivControlAsignacionGarantiaDb _db;

        public FrmVivControlAsignacionGarantiaBl(IConfiguration config)
            => _db = new FrmVivControlAsignacionGarantiaDb(config);

        public ErrorDto<List<VivControlAsignacionGarantiaPendienteData>> VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes(int codEmpresa, string tipoProfesional)
        {
            return _db.VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes(codEmpresa, tipoProfesional);
        }

        public ErrorDto<List<VivControlAsignacionProfesionalData>> VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales(int codEmpresa, int idZona, string tipoProfesional, long idGarantia)
        {
            return _db.VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales(codEmpresa, idZona, tipoProfesional, idGarantia);
        }

        public ErrorDto VivControlAsignacionGarantia_Asignacion_Aplicar(int codEmpresa, string usuario, VivControlAsignacionGarantiaAsignarRequest request)
        {
            return _db.VivControlAsignacionGarantia_Asignacion_Aplicar(codEmpresa, usuario, request);
        }

        public ErrorDto VivControlAsignacionGarantia_Asignacion_Borrar(int codEmpresa, long idGarantia, int idContacto, string usuario)
        {
            return _db.VivControlAsignacionGarantia_Asignacion_Borrar(codEmpresa, idGarantia, idContacto, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> VivControlAsignacionGarantia_ObtenerProfesionales(
            int codEmpresa, string tipoLista, string tipoProfesional)
        {
            return _db.VivControlAsignacionGarantia_ObtenerProfesionales(codEmpresa, tipoLista, tipoProfesional);
        }

        public ErrorDto<List<VivControlEntregaGarantiaData>> VivControlAsignacionGarantia_Entrega_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            return _db.VivControlAsignacionGarantia_Entrega_ObtenerGarantias(codEmpresa, idContacto, tipoProfesional);
        }

        public ErrorDto VivControlAsignacionGarantia_Entrega_Aplicar(int codEmpresa, string usuario, VivControlEntregaGarantiaRequest request)
        {
            return _db.VivControlAsignacionGarantia_Entrega_Aplicar(codEmpresa, usuario, request);
        }

        public ErrorDto<VivControlAsignacionGarantiaNotaData?> VivControlAsignacionGarantia_ObtenerUltimaNota(int codEmpresa, long idGarantia, string tipoProfesional)
        {
            return _db.VivControlAsignacionGarantia_ObtenerUltimaNota(codEmpresa, idGarantia, tipoProfesional);
        }

        public ErrorDto<List<VivControlRecibeGarantiaData>> VivControlAsignacionGarantia_Recepcion_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            return _db.VivControlAsignacionGarantia_Recepcion_ObtenerGarantias(codEmpresa, idContacto, tipoProfesional);
        }

        public ErrorDto VivControlAsignacionGarantia_Recepcion_Aplicar(int codEmpresa, string usuario, VivControlRecibeGarantiaRequest request)
        {
            return _db.VivControlAsignacionGarantia_Recepcion_Aplicar(codEmpresa, usuario, request);
        }

        public ErrorDto<List<VivControlRegistroGarantiaData>> VivControlAsignacionGarantia_Registro_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            return _db.VivControlAsignacionGarantia_Registro_ObtenerGarantias(codEmpresa, idContacto, tipoProfesional);
        }

        public ErrorDto VivControlAsignacionGarantia_Registro_Aplicar(int codEmpresa, string usuario, VivControlRegistroGarantiaRequest request)
        {
            return _db.VivControlAsignacionGarantia_Registro_Aplicar(codEmpresa, usuario, request);
        }

        public ErrorDto<VivControlTiemposSeguimientoData> VivControlAsignacionGarantia_ObtenerTiemposSeguimiento(int codEmpresa, string profesional)
        {
            return _db.VivControlAsignacionGarantia_ObtenerTiemposSeguimiento(codEmpresa, profesional);
        }

        public ErrorDto<VivControlHonorariosRegistraData> VivControlAsignacionGarantia_Asignacion_ValidaHonorariosRegistra(int codEmpresa, int idGarantia)
        {
            return _db.VivControlAsignacionGarantia_Asignacion_ValidaHonorariosRegistra(codEmpresa, idGarantia);
        }
    }
}