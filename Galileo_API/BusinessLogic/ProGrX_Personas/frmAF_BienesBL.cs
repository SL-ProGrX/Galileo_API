using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFBienesBL
    {
        private readonly FrmAFBienesDB _db;

        public FrmAFBienesBL(IConfiguration config)
        {
            _db = new FrmAFBienesDB(config);
        }

        public ErrorDto<BienesTipoLista> AF_BienesTipos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_BienesTipos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_BienesTipos_Guardar(int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            return _db.AF_BienesTipos_Guardar(CodEmpresa, usuario, bienTipo);
        }

        public ErrorDto AF_BienesTipos_Eliminar(int CodEmpresa, string usuario, string bienTipo)
        {
            return _db.AF_BienesTipos_Eliminar(CodEmpresa, usuario, bienTipo);
        }

        public ErrorDto AF_BienesTipos_Valida(int CodEmpresa, string bienTipo)
        {
            return _db.AF_BienesTipos_Valida(CodEmpresa, bienTipo);
        }
        public ErrorDto<List<BienesTipoData>> AF_BienesTipos_Exportar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_BienesTipos_Exportar(CodEmpresa, filtros);
        }
    }
}
