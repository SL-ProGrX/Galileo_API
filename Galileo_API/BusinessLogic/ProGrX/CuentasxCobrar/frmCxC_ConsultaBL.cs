using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCConsultaBL
    {

        private readonly FrmCxCConsultaDB _db;

        public FrmCxCConsultaBL(IConfiguration config) => _db = new FrmCxCConsultaDB(config);

        public ErrorDto<CxCPersonaDto?> ConsultarPersona(int codEmpresa, string cedula)
        {
            return _db.ConsultarPersona(codEmpresa, cedula);
        }

        public ErrorDto<List<CxCCuentaDto>> ConsultarCuentas(int codEmpresa, string cedula, string estado)
        {
            return _db.ConsultarCuentas(codEmpresa, cedula, estado);
        }

        public ErrorDto<List<CxCSolicitudDto>> ConsultarSolicitudes(int codEmpresa, string cedula)
        {
            return _db.ConsultarSolicitudes(codEmpresa, cedula);
        }

        public ErrorDto<List<CxCPreAnalisisDto>> ConsultarPreAnalisis(int codEmpresa, string cedula)
        {
            return _db.ConsultarPreAnalisis(codEmpresa, cedula);
        }

        public ErrorDto<List<CxCIncobrableDto>> ConsultarIncobrables(int codEmpresa, string cedula)
        {
            return _db.ConsultarIncobrables(codEmpresa, cedula);
        }

        public ErrorDto<List<CxCFacturaDto>> ConsultarFacturas(int codEmpresa, CxCFacturaFiltroDto filtro)
        {
            return _db.ConsultarFacturas(codEmpresa, filtro);
        }

        public ErrorDto<List<CxCDesembolsoDto>> ConsultarDesembolsos(int codEmpresa, string cedula)
        {
            return _db.ConsultarDesembolsos(codEmpresa, cedula);
        }

        public ErrorDto<List<CxCMensajeDto>> ConsultarMensajes(int codEmpresa, string cedula)
        {
            return _db.ConsultarMensajes(codEmpresa, cedula);
        }

        public ErrorDto<bool> GuardarMensaje(int codEmpresa, CxCMensajeAddDto dto)
        {
            return _db.GuardarMensaje(codEmpresa, dto);
        }

        public ErrorDto<bool> EliminarMensaje(int codEmpresa, CxCMensajeDeleteDto dto)
        {
            return _db.EliminarMensaje(codEmpresa, dto);
        }

        public ErrorDto<List<CxCDesembolsoFacturaDto>> ConsultarFacturasPorGiro(int codEmpresa, int operacion, int idGiro)
        {
            return _db.ConsultarFacturasPorGiro(codEmpresa, operacion, idGiro);
        }
    }



}

