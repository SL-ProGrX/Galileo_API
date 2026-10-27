using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatReservasBl
    {
        private readonly FrmIvrCatReservasDb _db;

        public FrmIvrCatReservasBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmIvrCatReservasDb(config);
        }

        public ErrorDto<List<IvrCatReservaDto>> IVR_Cat_Reservas_Lista_Obtener(
            int codEmpresa)
        {
            return _db.IVR_Cat_Reservas_Lista_Obtener(codEmpresa);
        }

        public ErrorDto IVR_Cat_Reservas_Registrar(
            int codEmpresa,
            IvrCatReservaGuardarRequest? request)
        {
            return _db.IVR_Cat_Reservas_Registrar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Reservas_Actualizar(
            int codEmpresa,
            IvrCatReservaGuardarRequest? request)
        {
            return _db.IVR_Cat_Reservas_Actualizar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Reservas_Eliminar(
            int codEmpresa,
            string? codReserva,
            string? usuario)
        {
            return _db.IVR_Cat_Reservas_Eliminar(codEmpresa, codReserva, usuario);
        }

        public ErrorDto<List<IvrCatReservaCuentaDto>> IVR_Cat_Reservas_Cuentas_Obtener(
            int codEmpresa,
            string? codReserva)
        {
            return _db.IVR_Cat_Reservas_Cuentas_Obtener(codEmpresa, codReserva);
        }

        public ErrorDto IVR_Cat_Reservas_Cuenta_Registrar(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request)
        {
            return _db.IVR_Cat_Reservas_Cuenta_Registrar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Reservas_Cuenta_Actualizar(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request)
        {
            return _db.IVR_Cat_Reservas_Cuenta_Actualizar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Reservas_Cuenta_Eliminar(
            int codEmpresa,
            IvrCatReservaCuentaRequest? request)
        {
            return _db.IVR_Cat_Reservas_Cuenta_Eliminar(codEmpresa, request);
        }
    }
}
