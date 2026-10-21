using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Cat_Administrador_Contactos")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrCatAdministradorContactosController
        : ControllerBase
    {
        private readonly FrmIvrCatAdministradorContactosBl _bl;

        public FrmIvrCatAdministradorContactosController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmIvrCatAdministradorContactosBl(config);
        }

        [HttpGet(
            "IVR_Cat_Administrador_Contactos_Buscar")]
        public ErrorDto<List<IvrCatAdministradorContactoData>>
            IVR_Cat_Administrador_Contactos_Buscar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador,
                [FromQuery] string? filtro)
        {
            return _bl.IVR_Cat_Administrador_Contactos_Buscar(
                codEmpresa,
                codAdministrador,
                filtro);
        }

        [HttpGet(
            "IVR_Cat_Administrador_Contactos_Obtener")]
        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador,
                [FromQuery] int codContacto)
        {
            return _bl.IVR_Cat_Administrador_Contactos_Obtener(
                codEmpresa,
                codAdministrador,
                codContacto);
        }

        [HttpGet(
            "IVR_Cat_Administrador_Contactos_Navegar")]
        public ErrorDto
            <IvrCatAdministradorContactoNavegacionData?>
            IVR_Cat_Administrador_Contactos_Navegar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codAdministrador,
                [FromQuery] int codContacto,
                [FromQuery] bool siguiente)
        {
            return _bl.IVR_Cat_Administrador_Contactos_Navegar(
                codEmpresa,
                codAdministrador,
                codContacto,
                siguiente);
        }

        [ValidateAntiForgeryToken]
        [HttpPost(
            "IVR_Cat_Administrador_Contactos_Registrar")]
        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Registrar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatAdministradorContactoGuardarRequest? request)
        {
            return _bl.IVR_Cat_Administrador_Contactos_Registrar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "IVR_Cat_Administrador_Contactos_Actualizar")]
        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Actualizar(
                [FromQuery] int codEmpresa,
                [FromBody]
                IvrCatAdministradorContactoGuardarRequest? request)
        {
            return _bl.IVR_Cat_Administrador_Contactos_Actualizar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "IVR_Cat_Administrador_Contactos_Eliminar")]
        public ErrorDto IVR_Cat_Administrador_Contactos_Eliminar(
            [FromQuery] int codEmpresa,
            [FromBody]
            IvrCatAdministradorContactoEliminarRequest? request)
        {
            return _bl.IVR_Cat_Administrador_Contactos_Eliminar(
                codEmpresa,
                request);
        }
    }
}
