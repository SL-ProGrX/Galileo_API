using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrx_Personas
{
    public class FrmAfUnidadesBl
    {
        private readonly FrmAfUnidadesDb DbAfUnidades;

        public FrmAfUnidadesBl(IConfiguration config)
        {
            DbAfUnidades = new FrmAfUnidadesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Unidades_Provincias_Obtener(int CodEmpresa)
        {
            return DbAfUnidades.AF_Unidades_Provincias_Obtener(CodEmpresa);
        }

        public ErrorDto<TablasListaGenericaModel> AF_Unidades_Lista_Obtener(int CodEmpresa, int rbTipo, string filtro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();
            return DbAfUnidades.AF_Unidades_Lista_Obtener(CodEmpresa, rbTipo, filtros);
        }

        public ErrorDto<AfUnidadesDto> AF_Unidades_BuscarPorCodigo_Obtener(int CodEmpresa, int rbTipo, string Codigo)
        {
            return DbAfUnidades.AF_Unidades_BuscarPorCodigo_Obtener(CodEmpresa, rbTipo, Codigo);
        }

        public ErrorDto AF_Unidades_Guardar(int CodEmpresa, int rbTipo, bool Editar, AfUnidadesDto Info, string Usuario)
        {
            return DbAfUnidades.AF_Unidades_Guardar(CodEmpresa, rbTipo, Editar, Info, Usuario);
        }

        public ErrorDto AF_Unidades_Eliminar(int CodEmpresa, int rbTipo, string Codigo, string Usuario)
        {
            return DbAfUnidades.AF_Unidades_Eliminar(CodEmpresa, rbTipo, Codigo, Usuario);
        }
    }
}
