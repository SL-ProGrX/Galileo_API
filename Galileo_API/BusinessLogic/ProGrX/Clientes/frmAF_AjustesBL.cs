using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic
{
    public class FrmAFAjustesBL
    {
        private readonly FrmAfAjustesDB _db;

        public FrmAFAjustesBL(IConfiguration config)
        {
            _db = new FrmAfAjustesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposId_Obtener(int CodEmpresa)
        {
            return _db.AF_TiposId_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_EstadosPersona_ObtenerActivos(int CodEmpresa)
        {
            return _db.AF_EstadosPersona_ObtenerActivos(CodEmpresa);
        }

        public ErrorDto AF_Ajustes_CambiarIdentificacion(int CodEmpresa, string cedula, int nuevoTipoId)
        {
            return _db.AF_Ajustes_CambiarIdentificacion(CodEmpresa, cedula, nuevoTipoId);
        }

        public ErrorDto AF_Ajustes_CambiarEstado(int CodEmpresa, string cedula, string nuevoEstado)
        {
            return _db.AF_Ajustes_CambiarEstado(CodEmpresa, cedula, nuevoEstado);
        }

        public ErrorDto AF_Ajustes_CambiarInstitucion_ASECCSS(int CodEmpresa, string cedula, string cambiosJson)
        {
            AjustesInstitucionAseccssDto cambios = JsonConvert.DeserializeObject<AjustesInstitucionAseccssDto>(cambiosJson)
                                                   ?? new AjustesInstitucionAseccssDto();

            return _db.AF_Ajustes_CambiarInstitucion_ASECCSS(
                CodEmpresa,
                cedula,
                cambios.codInstitucion,
                cambios.up,
                cambios.ut,
                cambios.ct
            );
        }

        public ErrorDto AF_Ajustes_CambiarInstitucion(int CodEmpresa, string cedula, string cambiosJson)
        {
            AjustesInstitucionDto cambios = JsonConvert.DeserializeObject<AjustesInstitucionDto>(cambiosJson)
                                            ?? new AjustesInstitucionDto();

            return _db.AF_Ajustes_CambiarInstitucion(
                CodEmpresa,
                cedula,
                cambios.codInstitucion,
                cambios.codDepartamento ?? string.Empty,
                cambios.codSeccion ?? string.Empty
            );
        }

        public ErrorDto<AfAjustePersonaDetalle> AF_Ajustes_CargarDatos(int CodEmpresa, string cedula)
        {
            return _db.AF_Ajustes_CargarDatos(CodEmpresa, cedula);
        }

        public ErrorDto AF_Ajustes_Cambiar(int CodEmpresa, string ajuste, int codigo)
        {
            return _db.AF_Ajustes_Cambiar(CodEmpresa, ajuste, codigo);
        }

        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string cod_institucion)
        {
            return _db.AF_Catalogos_Obtener(CodEmpresa, cod_institucion);
        }
    }
}