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
    public class FrmCRVerificaDatosPersonalesController : ControllerBase
    {
        private readonly FrmCRVerificaDatosPersonalesBL BL;

        public FrmCRVerificaDatosPersonalesController(IConfiguration config)
        {
            BL = new FrmCRVerificaDatosPersonalesBL(config);
        }

        [Authorize]
        [HttpGet("CR_VerificaDatos_Completo_Obtener")]
        public ErrorDto<CrVerificaDatosCompletoDto> CR_VerificaDatos_Completo_Obtener(int CodEmpresa, string identificacion)
        {
            return BL.CR_VerificaDatos_Completo_Obtener(CodEmpresa, identificacion);
        }

        [Authorize]
        [HttpGet("CR_VerificaDatos_Persona_F4_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_VerificaDatos_Persona_F4_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.CR_VerificaDatos_Persona_F4_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_VerificaDatos_Nombramientos_Lista_Obtener")]
        public ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Lista_Obtener(int CodEmpresa,string identificacion,string parametros)
        {
            return BL.CR_VerificaDatos_Nombramientos_Lista_Obtener(CodEmpresa, identificacion, parametros);
        }

        [Authorize]
        [HttpGet("CR_VerificaDatos_Nombramientos_Lista_Export")]
        public ErrorDto<CrVerificaDatosListaResult<CrVerificaDatosNombramientoItem>> CR_VerificaDatos_Nombramientos_Lista_Export(int CodEmpresa,string identificacion,string parametros)
        {
            return BL.CR_VerificaDatos_Nombramientos_Lista_Export(CodEmpresa, identificacion, parametros);
        }

        [Authorize]
        [HttpPost("CR_VerificaDatos_Nombramiento_Agregar")]
        public ErrorDto CR_VerificaDatos_Nombramiento_Agregar(int CodEmpresa, CrVerificaDatosNombramientoAgregarRequest req)
        {
            return BL.CR_VerificaDatos_Nombramiento_Agregar(CodEmpresa, req);
        }
        [Authorize]
        [HttpPost("CR_VerificaDatos_Guardar")]
        public ErrorDto CR_VerificaDatos_Guardar(int CodEmpresa, CrVerificaDatosGuardarRequest req)
        {
            return BL.CR_VerificaDatos_Guardar(CodEmpresa, req);
        }
        [Authorize]
        [HttpGet("CR_EstadoLaboral_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EstadoLaboral_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_EstadoLaboral_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CR_EstadoCivil_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_EstadoCivil_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_EstadoCivil_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Nacionalidades_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Nacionalidades_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_Nacionalidades_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Provincias_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Provincias_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_Provincias_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_Cantones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Cantones_Dropdown_Obtener(int CodEmpresa, string provincia)
        {
            return BL.CR_Cantones_Dropdown_Obtener(CodEmpresa, provincia);
        }

        [Authorize]
        [HttpGet("CR_Distritos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Distritos_Dropdown_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return BL.CR_Distritos_Dropdown_Obtener(CodEmpresa, provincia, canton);
        }
        [Authorize]
        [HttpPost("CR_VerificaDatos_Catalogo_Asignar")]
        public ErrorDto CR_VerificaDatos_Catalogo_Asignar(int CodEmpresa, CrVerificaDatosAsignarCatalogoRequest req)
        {
            return BL.CR_VerificaDatos_Catalogo_Asignar(CodEmpresa, req);
        }
    }
}