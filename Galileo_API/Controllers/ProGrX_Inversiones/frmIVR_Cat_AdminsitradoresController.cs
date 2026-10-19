using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Adminsitradores")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatAdminsitradoresController
        : ControllerBase
    {
        private readonly FrmIvrCatAdminsitradoresBl _bl;

        public FrmIvrCatAdminsitradoresController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrCatAdminsitradoresBl(config);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Inicializar")]
        public ErrorDto<IvrCatAdministradorInicialData>
            IVR_Cat_Adminsitradores_Inicializar(
                [FromQuery] int codEmpresa)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Inicializar(
                    codEmpresa);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Buscar")]
        public ErrorDto<List<IvrCatAdministradorBusquedaData>>
            IVR_Cat_Adminsitradores_Buscar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? filtro)
        {
            return _bl.IVR_Cat_Adminsitradores_Buscar(
                codEmpresa,
                filtro);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Obtener")]
        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador)
        {
            return _bl.IVR_Cat_Adminsitradores_Obtener(
                codEmpresa,
                codAdministrador);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Navegar")]
        public ErrorDto<IvrCatAdministradorNavegacionData?>
            IVR_Cat_Adminsitradores_Navegar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador,
                [FromQuery] bool siguiente)
        {
            return _bl.IVR_Cat_Adminsitradores_Navegar(
                codEmpresa,
                codAdministrador,
                siguiente);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Cuenta_Validar")]
        public ErrorDto<IvrCatAdministradorCuentaContableData>
            IVR_Cat_Adminsitradores_Cuenta_Validar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? cuenta)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Cuenta_Validar(
                    codEmpresa,
                    cuenta);
        }

        [ValidateAntiForgeryToken]
        [HttpPost(
            "IVR_Cat_Adminsitradores_Registrar")]
        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Registrar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatAdministradorGuardarRequest? request)
        {
            return _bl.IVR_Cat_Adminsitradores_Registrar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Cat_Adminsitradores_Actualizar")]
        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Actualizar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatAdministradorGuardarRequest? request)
        {
            return _bl.IVR_Cat_Adminsitradores_Actualizar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "IVR_Cat_Adminsitradores_Eliminar")]
        public ErrorDto IVR_Cat_Adminsitradores_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codAdministrador,
            [FromQuery] string? usuario)
        {
            return _bl.IVR_Cat_Adminsitradores_Eliminar(
                codEmpresa,
                codAdministrador,
                usuario);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Cuentas_Bancarias_Obtener")]
        public ErrorDto
            <List<IvrCatAdministradorCuentaBancariaData>>
            IVR_Cat_Adminsitradores_Cuentas_Bancarias_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? identificacion)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Cuentas_Bancarias_Obtener(
                    codEmpresa,
                    identificacion);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Portafolios_Obtener")]
        public ErrorDto<List<IvrCatAdministradorPortafolioData>>
            IVR_Cat_Adminsitradores_Portafolios_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Portafolios_Obtener(
                    codEmpresa,
                    codAdministrador);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Cat_Adminsitradores_Portafolio_Actualizar")]
        public ErrorDto
            IVR_Cat_Adminsitradores_Portafolio_Actualizar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatAdministradorPortafolioActualizarRequest?
                    request)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Portafolio_Actualizar(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Instrumentos_Obtener")]
        public ErrorDto
            <List<IvrCatAdministradorInstrumentoData>>
            IVR_Cat_Adminsitradores_Instrumentos_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Instrumentos_Obtener(
                    codEmpresa,
                    codAdministrador);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Cat_Adminsitradores_Instrumento_Actualizar")]
        public ErrorDto
            IVR_Cat_Adminsitradores_Instrumento_Actualizar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatAdministradorInstrumentoActualizarRequest?
                    request)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Instrumento_Actualizar(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "IVR_Cat_Adminsitradores_Contactos_Obtener")]
        public ErrorDto<List<IvrCatAdministradorContactoData>>
            IVR_Cat_Adminsitradores_Contactos_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador)
        {
            return _bl
                .IVR_Cat_Adminsitradores_Contactos_Obtener(
                    codEmpresa,
                    codAdministrador);
        }
    }
}