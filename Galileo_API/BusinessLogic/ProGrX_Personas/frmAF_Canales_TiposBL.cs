using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrx_Personas
{
    public class FrmAFCanalesTiposBL
    {
        private readonly FrmAFCanalesTiposDB _db;

        public FrmAFCanalesTiposBL(IConfiguration config)
        {
            _db = new FrmAFCanalesTiposDB(config);
        }

        public ErrorDto<CanalTipoLista> AF_CanalesTipos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_CanalesTipos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_CanalesTipos_Guardar(int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            return _db.AF_CanalesTipos_Guardar(CodEmpresa, usuario, canalTipo);
        }

        public ErrorDto AF_CanalesTipos_Eliminar(int CodEmpresa, string usuario, string canalTipo)
        {
            return _db.AF_CanalesTipos_Eliminar(CodEmpresa, usuario, canalTipo);
        }

        public ErrorDto AF_CanalesTipos_Valida(int CodEmpresa, string canalTipo)
        {
            return _db.AF_CanalesTipos_Valida(CodEmpresa, canalTipo);
        }

        public ErrorDto<List<CanalTipoData>> AF_CanalesTipos_Exportar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_CanalesTipos_Exportar(CodEmpresa, filtros);
        }
    }
}
