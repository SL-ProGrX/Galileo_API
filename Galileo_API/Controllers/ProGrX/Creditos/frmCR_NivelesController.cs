using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrNivelesController : ControllerBase
    {
        private readonly FrmCrNivelesBL _bl;

        public FrmCrNivelesController(IConfiguration configuration)
        {
            _bl = new FrmCrNivelesBL(configuration);
        }

        [Authorize]
        [HttpGet("CR_Niveles_Procesos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Niveles_Procesos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CR_Niveles_Procesos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Niveles_Grupos_F4_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Niveles_Grupos_F4_Obtener(int CodEmpresa, string tipo, string? texto = "")
        {
            return _bl.CR_Niveles_Grupos_F4_Obtener(CodEmpresa, tipo, texto);
        }

        [Authorize]
        [HttpGet("CR_Niveles_Grupo_Scroll_Obtener")]
        public ErrorDto<CrNivelesGrupoDto> CR_Niveles_Grupo_Scroll_Obtener(int CodEmpresa, int grupoActual, string tipoProceso, int tipo)
        {
            return _bl.CR_Niveles_Grupo_Scroll_Obtener(CodEmpresa, grupoActual, tipoProceso, tipo);
        }

        [Authorize]
        [HttpGet("CR_Niveles_Grupo_Obtener")]
        public ErrorDto<CrNivelesGrupoDetalleDto> CR_Niveles_Grupo_Obtener(int CodEmpresa, int grupoId)
        {
            return _bl.CR_Niveles_Grupo_Obtener(CodEmpresa, grupoId);
        }

        [Authorize]
        [HttpPost("CR_Niveles_Grupo_Guardar")]
        public ErrorDto<CrNivelesGrupoDto> CR_Niveles_Grupo_Guardar(int CodEmpresa, string usuario, [FromBody] CrNivelesGrupoGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>("Solicitud inválida.");
            }

            return _bl.CR_Niveles_Grupo_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_Niveles_Grupo_Eliminar")]
        public ErrorDto CR_Niveles_Grupo_Eliminar(int CodEmpresa, int grupoId, string usuario)
        {
            return _bl.CR_Niveles_Grupo_Eliminar(CodEmpresa, grupoId, usuario);
        }

        [Authorize]
        [HttpGet("CR_Niveles_Miembros_Lista_Obtener")]
        public ErrorDto<CrNivelesMiembroLista> CR_Niveles_Miembros_Lista_Obtener(int CodEmpresa, int grupoId, string? texto = "")
        {
            return _bl.CR_Niveles_Miembros_Lista_Obtener(CodEmpresa, grupoId, texto);
        }

        [Authorize]
        [HttpGet("CR_Niveles_Lineas_Lista_Obtener")]
        public ErrorDto<CrNivelesLineaLista> CR_Niveles_Lineas_Lista_Obtener(int CodEmpresa, int grupoId, string? texto = "")
        {
            return _bl.CR_Niveles_Lineas_Lista_Obtener(CodEmpresa, grupoId, texto);
        }

        [Authorize]
        [HttpPost("CR_Niveles_Miembro_Asignar")]
        public ErrorDto CR_Niveles_Miembro_Asignar(int CodEmpresa, string usuario, [FromBody] CrNivelesAsignacionMiembroRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Solicitud inválida.");
            }

            return _bl.CR_Niveles_Miembro_Asignar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_Niveles_Linea_Asignar")]
        public ErrorDto CR_Niveles_Linea_Asignar(int CodEmpresa, string usuario, [FromBody] CrNivelesAsignacionLineaRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Solicitud inválida.");
            }

            return _bl.CR_Niveles_Linea_Asignar(CodEmpresa, request, usuario);
        }
    }
}