using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.ControlTramites;
using Galileo_API.DataBaseTier.ProGrX.ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX.ControlTramites
{
    public class FrmSifTagsModulosBl
    {
        private readonly FrmSifTagsModulosDb _db;

        public FrmSifTagsModulosBl(IConfiguration config)
        {
            _db = new FrmSifTagsModulosDb(config);
        }

        public ErrorDto<List<SifTagsModulosProcesoData>> SIF_TagsModulos_Procesos_Lista_Obtener(int CodEmpresa)
        {
            return _db.SIF_TagsModulos_Procesos_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SifTagsModulosProcesoData>> SIF_TagsModulos_Procesos_Lista_Export(int CodEmpresa)
        {
            return _db.SIF_TagsModulos_Procesos_Lista_Export(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SIF_TagsModulos_Procesos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.SIF_TagsModulos_Procesos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto SIF_TagsModulos_Proceso_Guardar(int CodEmpresa, string? usuario, SifTagsModulosProcesoGuardarRequest? request)
        {
            return _db.SIF_TagsModulos_Proceso_Guardar(CodEmpresa, usuario, request);
        }

        public ErrorDto<List<SifTagsModulosEtiquetaData>> SIF_TagsModulos_Etiquetas_Lista_Obtener(int CodEmpresa, string? codModulo)
        {
            return _db.SIF_TagsModulos_Etiquetas_Lista_Obtener(CodEmpresa, codModulo);
        }

        public ErrorDto SIF_TagsModulos_Etiqueta_Guardar(int CodEmpresa, SifTagsModulosEtiquetaGuardarRequest? request)
        {
            return _db.SIF_TagsModulos_Etiqueta_Guardar(CodEmpresa, request);
        }
    }
}