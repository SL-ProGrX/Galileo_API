using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFPrincipalBL
    {
        private readonly FrmAFPrincipalDB _db;

        public FrmAFPrincipalBL(IConfiguration config)
        {
            _db = new FrmAFPrincipalDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CatalogosGenerales_Obtener(int CodEmpresa, string Cod_Catalogo)
        {
            return _db.AF_CatalogosGenerales_Obtener(CodEmpresa, Cod_Catalogo);
        }

        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string? cod_institucion)
        {
            return _db.AF_Catalogos_Obtener(CodEmpresa, cod_institucion);
        }

        public ErrorDto<AfPersonaDto> AF_Persona_Obtener(int CodEmpresa, string cedula)
        {
            return _db.AF_Persona_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto AF_Persona_Guardar(int CodEmpresa, string request, string mov)
        {
            return _db.AF_Persona_Guardar(CodEmpresa, request, mov);

        }

        public ErrorDto AF_Persona_Nombramientos_Add(int CodEmpresa, string req, string mov)
        {
            return _db.AF_Persona_Nombramientos_Add(CodEmpresa, req, mov);
        }

        public ErrorDto AF_Persona_Relacion_Add(int CodEmpresa, string request, string mov)
        {
            return _db.AF_Persona_Relacion_Add(CodEmpresa, request, mov);
        }

        public ErrorDto AF_Persona_Relacion_Del(int CodEmpresa, int idRelacion, string usuario)
        {
            return _db.AF_Persona_Relacion_Del(CodEmpresa, idRelacion, usuario);
        }

        public ErrorDto AF_Persona_Salarios_Add(int CodEmpresa, string request, string mov)
        {
            return _db.AF_Persona_Salarios_Add(CodEmpresa, request, mov);
        }

        public ErrorDto AF_Persona_Ingresos_Economicos_Add(int CodEmpresa, AfPersonaIngresoEconomicoAddDto request)
        {
            return _db.AF_Persona_Ingresos_Economicos_Add(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Direccion_Add(int CodEmpresa, string req, string mov)
        {
            return _db.AF_Persona_Direccion_Add(CodEmpresa, req, mov);
        }

        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Escolaridad_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Preferencias_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Preferencias_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Canales_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Patrimonio_Vincula(int CodEmpresa, AfPersonaPatrimonioVinculaDto request)
        {
            return _db.AF_Persona_Patrimonio_Vincula(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Bienes_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Productos_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Productos_Registra(CodEmpresa, request);
        }
        public ErrorDto AF_RegistroDefault(int CodEmpresa, AfRegistroDefaultDto request)
        {
            return _db.AF_RegistroDefault(CodEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            return _db.AF_Provincias_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string provincia)
        {
            return _db.AF_Cantones_Obtener(CodEmpresa, provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Distritos_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return _db.AF_Distritos_Obtener(CodEmpresa, provincia, canton);
        }

        public ErrorDto<string> TES_Persona_Scroll(int CodEmpresa, int scrollCode, string cedula)
        {
            return _db.TES_Persona_Scroll(CodEmpresa, scrollCode, cedula);
        }

        public ErrorDto<List<AfCumplimientoDto>> AF_PersonaProductos_Consulta(int CodEmpresa, string Cedula)
        {
            return _db.AF_PersonaProductos_Consulta(CodEmpresa, Cedula);
        }

        public ErrorDto<AfConsultasGeneralesDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _db.AF_Persona_Consulta_Obtener(CodEmpresa, cedula, usuario);
        }

        public ErrorDto<List<AfPersonaRelacionDto>> AF_Persona_Relacion_List(int CodEmpresa, string cedula)
        {
            return _db.AF_Persona_Relacion_List(CodEmpresa, cedula);
        }

        public ErrorDto AF_Persona_Indicadores_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Indicadores_Registra(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_PrimeraDeduccion_Registra(int CodEmpresa, string cedula, string prideduc)
        {
            return _db.AF_Persona_PrimeraDeduccion_Registra(CodEmpresa, cedula, prideduc);
        }

        public ErrorDto AF_Persona_Elimina(int CodEmpresa, string cedula)
        {
            return _db.AF_Persona_Elimina(CodEmpresa, cedula);
        }

        public ErrorDto<string> AF_Scroll_General(int CodEmpresa, int scrollCode, string id_promotor, int tipoScroll, string cod_Institucion, string cod_departamento)
        {
            return _db.AF_Scroll_General(CodEmpresa, scrollCode, id_promotor, tipoScroll, cod_Institucion, cod_departamento);
        }

        public ErrorDto AF_Persona_Validar(int CodEmpresa, string req)
        {
            return _db.AF_Persona_Validar(CodEmpresa, req);
        }

        public ErrorDto<AfPadronPersonaDto> AF_PersonaPadron_Obtener(int CodEmpresa, string cedula)
        {
            return _db.AF_PersonaPadron_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto AF_Persona_Dimex_Add(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Dimex_Add(CodEmpresa, request);
        }

        public ErrorDto AF_Persona_Direccion_Elimina(int CodEmpresa, string cedula, string linea, string usuario)
        {
            return _db.AF_Persona_Direccion_Elimina(CodEmpresa, cedula, linea, usuario);
        }

        public ErrorDto AF_Persona_Motivos_Registra(int CodEmpresa, string request)
        {
            return _db.AF_Persona_Motivos_Registra(CodEmpresa, request);
        }

        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return _db.FechaServidor_Obtener(CodEmpresa);
        }
    }
}
