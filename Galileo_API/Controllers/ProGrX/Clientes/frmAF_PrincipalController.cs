using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFPrincipalController : ControllerBase
    {
        private readonly FrmAFPrincipalBL _bl;
        public FrmAFPrincipalController(IConfiguration config)
        {
            _bl = new FrmAFPrincipalBL(config);
        }

        [Authorize]
        [HttpGet("AF_CatalogoGenerico_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CatalogosGenerales_Obtener(int CodEmpresa, string Cod_Catalogo)
        {
            return _bl.AF_CatalogosGenerales_Obtener(CodEmpresa, Cod_Catalogo);
        }

        [Authorize]
        [HttpGet("AF_Catalogos_Obtener")]
        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string? cod_institucion)
        {
            return _bl.AF_Catalogos_Obtener(CodEmpresa, cod_institucion);
        }

        [Authorize]
        [HttpGet("AF_Persona_Obtener")]
        public ErrorDto<AfPersonaDto> AF_Persona_Obtener(int CodEmpresa, string cedula)
        {
            return _bl.AF_Persona_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("AF_Persona_Guardar")]
        public ErrorDto AF_Persona_Guardar(int CodEmpresa, string request, string mov)
        {
            return _bl.AF_Persona_Guardar(CodEmpresa, request, mov);
        }

        [HttpPost("AF_Persona_Nombramientos_Add")]
        public ErrorDto AF_Persona_Nombramientos_Add(int CodEmpresa, string request, string mov)
        {
            return _bl.AF_Persona_Nombramientos_Add(CodEmpresa, request, mov);
        }

        [Authorize]
        [HttpPost("AF_Persona_Relacion_Add")]
        public ErrorDto AF_Persona_Relacion_Add(int CodEmpresa, string request, string mov)
        {
            return _bl.AF_Persona_Relacion_Add(CodEmpresa, request, mov);
        }

        [Authorize]
        [HttpPost("AF_Persona_Relacion_Del")]
        public ErrorDto AF_Persona_Relacion_Del(int CodEmpresa, int idRelacion, string usuario)
        {
            return _bl.AF_Persona_Relacion_Del(CodEmpresa, idRelacion, usuario);
        }

        [Authorize]
        [HttpPost("AF_Persona_Salarios_Add")]
        public ErrorDto AF_Persona_Salarios_Add(int CodEmpresa, string request, string mov)
        {
            return _bl.AF_Persona_Salarios_Add(CodEmpresa, request, mov);
        }

        [Authorize]
        [HttpPost("AF_Persona_Ingresos_Economicos_Add")]
        public ErrorDto AF_Persona_Ingresos_Economicos_Add(int CodEmpresa, AfPersonaIngresoEconomicoAddDto request)
        {
            return _bl.AF_Persona_Ingresos_Economicos_Add(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Direccion_Add")]
        public ErrorDto AF_Persona_Direccion_Add(int CodEmpresa, string request, string mov)
        {
            return _bl.AF_Persona_Direccion_Add(CodEmpresa, request, mov);
        }

        [Authorize]
        [HttpPost("AF_Persona_Escolaridad_Registra")]
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Escolaridad_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Preferencias_Registra")]
        public ErrorDto AF_Persona_Preferencias_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Preferencias_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Canales_Registra")]
        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Canales_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Patrimonio_Vincula")]
        public ErrorDto AF_Persona_Patrimonio_Vincula(int CodEmpresa, AfPersonaPatrimonioVinculaDto req)
        {
            return _bl.AF_Persona_Patrimonio_Vincula(CodEmpresa, req);
        }

        [Authorize]
        [HttpPost("AF_Persona_Bienes_Registra")]
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Bienes_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Productos_Registra")]
        public ErrorDto AF_Persona_Productos_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Productos_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_RegistroDefault")]
        public ErrorDto AF_RegistroDefault(int CodEmpresa, AfRegistroDefaultDto req)
        {
            return _bl.AF_RegistroDefault(CodEmpresa, req);
        }

        [Authorize]
        [HttpGet("AF_Provincias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            return _bl.AF_Provincias_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Cantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string provincia)
        {
            return _bl.AF_Cantones_Obtener(CodEmpresa, provincia);
        }

        [Authorize]
        [HttpGet("AF_Distritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Distritos_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return _bl.AF_Distritos_Obtener(CodEmpresa, provincia, canton);
        }

        [Authorize]
        [HttpGet("AF_Persona_Scroll")]
        public ErrorDto<string> TES_Transaccion_Scroll(int CodEmpresa, int scrollCode, string cedula)
        {
            return _bl.TES_Persona_Scroll(CodEmpresa, scrollCode, cedula);
        }
        [Authorize]
        [HttpGet("AF_Cumplimiento_Obtener")]
        public ErrorDto<List<AfCumplimientoDto>> AF_PersonaProductos_Consulta(int CodEmpresa, string Cedula)
        {
            return _bl.AF_PersonaProductos_Consulta(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_Persona_Consulta_Obtener")]
        public ErrorDto<AfConsultasGeneralesDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _bl.AF_Persona_Consulta_Obtener(CodEmpresa, cedula, usuario);
        }

        [Authorize]
        [HttpGet("AF_Persona_Relacion_List")]
        public ErrorDto<List<AfPersonaRelacionDto>> AF_Persona_Relacion_List(int CodEmpresa, string cedula)
        {
            return _bl.AF_Persona_Relacion_List(CodEmpresa, cedula);
        }

        [HttpPost("AF_Persona_Indicadores_Registra")]
        public ErrorDto AF_Persona_Indicadores_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Indicadores_Registra(CodEmpresa, request);
        }

        [HttpPost("AF_Persona_PrimeraDeduccion_Registra")]
        public ErrorDto AF_Persona_PrimeraDeduccion_Registra(int CodEmpresa, string cedula, string prideduc)
        {
            return _bl.AF_Persona_PrimeraDeduccion_Registra(CodEmpresa, cedula, prideduc);
        }

        [HttpPost("AF_Persona_Elimina")]
        public ErrorDto AF_Persona_Elimina(int CodEmpresa, string cedula)
        {
            return _bl.AF_Persona_Elimina(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("AF_Scroll_General")]
        public ErrorDto<string> AF_Scroll_General(int CodEmpresa, int scrollCode, string id_promotor, int tipoScroll, string? cod_Institucion, string? cod_departamento)
        {
            return _bl.AF_Scroll_General(CodEmpresa, scrollCode, id_promotor, tipoScroll, cod_Institucion ?? string.Empty, cod_departamento ?? string.Empty);
        }

        [Authorize]
        [HttpPost("AF_Persona_Validar")]
        public ErrorDto AF_Persona_Validar(int CodEmpresa, string req)
        {
            return _bl.AF_Persona_Validar(CodEmpresa, req);
        }

        [Authorize]
        [HttpGet("AF_PersonaPadron_Obtener")]
        public ErrorDto<AfPadronPersonaDto> AF_PersonaPadron_Obtener(int CodEmpresa, string cedula)
        {
            return _bl.AF_PersonaPadron_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("AF_Persona_Dimex_Add")]
        public ErrorDto AF_Persona_Dimex_Add(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Dimex_Add(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_Persona_Direccion_Elimina")]
        public ErrorDto AF_Persona_Direccion_Elimina(int CodEmpresa, string cedula, string linea, string usuario)
        {
            return _bl.AF_Persona_Direccion_Elimina(CodEmpresa, cedula, linea, usuario);
        }

        [Authorize]
        [HttpPost("AF_Persona_Motivos_Registra")]
        public ErrorDto AF_Persona_Motivos_Registra(int CodEmpresa, string request)
        {
            return _bl.AF_Persona_Motivos_Registra(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return _bl.FechaServidor_Obtener(CodEmpresa);
        }
    }
}
