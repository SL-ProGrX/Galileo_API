using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatAdminsitradoresBl
    {
        private readonly FrmIvrCatAdminsitradoresDb _db;

        public FrmIvrCatAdminsitradoresBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrCatAdminsitradoresDb(config);
        }

        public ErrorDto<IvrCatAdministradorInicialData>
            IVR_Cat_Adminsitradores_Inicializar(
                int codEmpresa)
        {
            return _db
                .IVR_Cat_Adminsitradores_Inicializar(
                    codEmpresa);
        }

        public ErrorDto<List<IvrCatAdministradorBusquedaData>>
            IVR_Cat_Adminsitradores_Buscar(
                int codEmpresa,
                string? filtro)
        {
            return _db.IVR_Cat_Adminsitradores_Buscar(
                codEmpresa,
                filtro);
        }

        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            return _db.IVR_Cat_Adminsitradores_Obtener(
                codEmpresa,
                codAdministrador);
        }

        public ErrorDto<IvrCatAdministradorNavegacionData?>
            IVR_Cat_Adminsitradores_Navegar(
                int codEmpresa,
                string? codAdministrador,
                bool siguiente)
        {
            return _db.IVR_Cat_Adminsitradores_Navegar(
                codEmpresa,
                codAdministrador,
                siguiente);
        }

        public ErrorDto<IvrCatAdministradorCuentaContableData>
            IVR_Cat_Adminsitradores_Cuenta_Validar(
                int codEmpresa,
                string? cuenta)
        {
            return _db
                .IVR_Cat_Adminsitradores_Cuenta_Validar(
                    codEmpresa,
                    cuenta);
        }

        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Registrar(
                int codEmpresa,
                IvrCatAdministradorGuardarRequest? request)
        {
            return _db.IVR_Cat_Adminsitradores_Registrar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Actualizar(
                int codEmpresa,
                IvrCatAdministradorGuardarRequest? request)
        {
            return _db.IVR_Cat_Adminsitradores_Actualizar(
                codEmpresa,
                request);
        }

        public ErrorDto IVR_Cat_Adminsitradores_Eliminar(
            int codEmpresa,
            string? codAdministrador,
            string? usuario)
        {
            return _db.IVR_Cat_Adminsitradores_Eliminar(
                codEmpresa,
                codAdministrador,
                usuario);
        }

        public ErrorDto
            <List<IvrCatAdministradorCuentaBancariaData>>
            IVR_Cat_Adminsitradores_Cuentas_Bancarias_Obtener(
                int codEmpresa,
                string? identificacion)
        {
            return _db
                .IVR_Cat_Adminsitradores_Cuentas_Bancarias_Obtener(
                    codEmpresa,
                    identificacion);
        }

        public ErrorDto<List<IvrCatAdministradorPortafolioData>>
            IVR_Cat_Adminsitradores_Portafolios_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            return _db
                .IVR_Cat_Adminsitradores_Portafolios_Obtener(
                    codEmpresa,
                    codAdministrador);
        }

        public ErrorDto
            IVR_Cat_Adminsitradores_Portafolio_Actualizar(
                int codEmpresa,
                IvrCatAdministradorPortafolioActualizarRequest?
                    request)
        {
            return _db
                .IVR_Cat_Adminsitradores_Portafolio_Actualizar(
                    codEmpresa,
                    request);
        }

        public ErrorDto
            <List<IvrCatAdministradorInstrumentoData>>
            IVR_Cat_Adminsitradores_Instrumentos_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            return _db
                .IVR_Cat_Adminsitradores_Instrumentos_Obtener(
                    codEmpresa,
                    codAdministrador);
        }

        public ErrorDto
            IVR_Cat_Adminsitradores_Instrumento_Actualizar(
                int codEmpresa,
                IvrCatAdministradorInstrumentoActualizarRequest?
                    request)
        {
            return _db
                .IVR_Cat_Adminsitradores_Instrumento_Actualizar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<IvrCatAdministradorContactoData>>
            IVR_Cat_Adminsitradores_Contactos_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            return _db
                .IVR_Cat_Adminsitradores_Contactos_Obtener(
                    codEmpresa,
                    codAdministrador);
        }
    }
}