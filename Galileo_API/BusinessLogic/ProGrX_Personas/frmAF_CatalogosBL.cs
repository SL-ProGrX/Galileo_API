using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCatalogosBL
    {
        private readonly FrmAFCatalogosDB _db;

        public FrmAFCatalogosBL(IConfiguration config)
        {
            _db = new FrmAFCatalogosDB(config);
        }

        public ErrorDto<CatalogoLista> AF_Catalogos_Obtener(int CodEmpresa, int tipoId, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_Catalogos_Obtener(CodEmpresa, tipoId, filtros);
        }

        public ErrorDto<CatalogoValidate> AF_Catalogos_Valida(int CodEmpresa, string catalogoId, int tipoId)
        {
            return _db.AF_Catalogos_Valida(CodEmpresa, catalogoId, tipoId);
        }

        public ErrorDto AF_Catalogos_Guardar(int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            return _db.AF_Catalogos_Guardar(CodEmpresa, usuario, catalogo);
        }

        public ErrorDto AF_Catalogos_Eliminar(int CodEmpresa, string usuario, int lineaId)
        {
            return _db.AF_Catalogos_Eliminar(CodEmpresa, usuario, lineaId);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Catalogos_Tipos_Obtener(int CodEmpresa)
        {
            return _db.AF_Catalogos_Tipos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CatalogoTipoData>> AF_Catalogos_Tipos_ObtenerTodos(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_Catalogos_Tipos_ObtenerTodos(CodEmpresa, filtros);
        }
    }
}
