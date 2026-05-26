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
    public class FrmCrComitesAutorizadoresController : ControllerBase
    {
        private readonly FrmCrComitesAutorizadoresBL BL;

        public FrmCrComitesAutorizadoresController(IConfiguration config)
        {
            BL = new FrmCrComitesAutorizadoresBL(config);
        }

        [Authorize]
        [HttpGet("CR_Puestos_Lista_Obtener")]
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>> CR_Puestos_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_Puestos_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_Puestos_Lista_Export")]
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>> CR_Puestos_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_Puestos_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_Puestos_Guardar")]
        public ErrorDto CR_Puestos_Guardar(int CodEmpresa, string usuario, [FromBody] CrComitesPuestoDto data)
        {
            return BL.CR_Puestos_Guardar(CodEmpresa, data, usuario);
        }

        [Authorize]
        [HttpDelete("CR_Puestos_Eliminar")]
        public ErrorDto CR_Puestos_Eliminar(int CodEmpresa, string id_puesto, string usuario)
        {
            return BL.CR_Puestos_Eliminar(CodEmpresa, id_puesto, usuario);
        }

        [Authorize]
        [HttpGet("CR_Personas_Lista_Obtener")]
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>> CR_Personas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_Personas_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_Personas_Lista_Export")]
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>> CR_Personas_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_Personas_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_Personas_Guardar")]
        public ErrorDto CR_Personas_Guardar(int CodEmpresa, string usuario, [FromBody] CrComitesPersonaDto data)
        {
            return BL.CR_Personas_Guardar(CodEmpresa, data, usuario);
        }

        [Authorize]
        [HttpDelete("CR_Personas_Eliminar")]
        public ErrorDto CR_Personas_Eliminar(int CodEmpresa, string cedula, string usuario)
        {
            return BL.CR_Personas_Eliminar(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpGet("CR_Puestos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Puestos_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_Puestos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_Comites_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Asignacion_Miembros_Lista_Obtener")]
        public ErrorDto<List<CrComitesAsignacionDto>> CR_Asignacion_Miembros_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            return BL.CR_Asignacion_Miembros_Lista_Obtener(CodEmpresa, id_comite);
        }

        [Authorize]
        [HttpPost("CR_Asignacion_Miembros_Asignar")]
        public ErrorDto CR_Asignacion_Miembros_Asignar(int CodEmpresa, [FromBody] CrComitesAsignacionRequest request)
        {
            return BL.CR_Asignacion_Miembros_Asignar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_Asignacion_Autorizadores_Lista_Obtener")]
        public ErrorDto<List<CrComitesAsignacionDto>> CR_Asignacion_Autorizadores_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            return BL.CR_Asignacion_Autorizadores_Lista_Obtener(CodEmpresa, id_comite);
        }

        [Authorize]
        [HttpPost("CR_Asignacion_Autorizadores_Asignar")]
        public ErrorDto CR_Asignacion_Autorizadores_Asignar(int CodEmpresa, [FromBody] CrComitesAsignacionRequest request)
        {
            return BL.CR_Asignacion_Autorizadores_Asignar(CodEmpresa, request);
        }
    }
}