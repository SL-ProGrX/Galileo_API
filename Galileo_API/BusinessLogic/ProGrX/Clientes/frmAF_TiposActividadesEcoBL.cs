using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFTiposActividadesEcoBL
    {
        private readonly FrmAFTiposActividadesEcoDB _db;

        public FrmAFTiposActividadesEcoBL(IConfiguration config)
        {
            _db = new FrmAFTiposActividadesEcoDB(config);
        }

        public ErrorDto<AfTiposActividadesEcoLista> AF_TiposActividadesEco_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_TiposActividadesEco_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_TiposActividadesEco_Guardar(int CodEmpresa, string Usuario, AfTiposActividadesEcoDto Info)
        {
            return _db.AF_TiposActividadesEco_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_TiposActividadesEco_Eliminar(int CodEmpresa, string Usuario, string CodActividad)
        {
            return _db.AF_TiposActividadesEco_Eliminar(CodEmpresa, Usuario, CodActividad);
        }

        public ErrorDto<AfTiposActividadesEcoLista> AF_TiposActividadesEco_SubActividad_Obtener(int CodEmpresa, string CodActividad, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_TiposActividadesEco_SubActividad_Obtener(CodEmpresa, CodActividad, filtros);
        }

        public ErrorDto AF_TiposActividadesEco_SubActividad_Guardar(int CodEmpresa, string Usuario, AfTiposActividadesEcoDto Info)
        {
            return _db.AF_TiposActividadesEco_SubActividad_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_TiposActividadesEco_SubActividad_Eliminar(int CodEmpresa, string Usuario, string CodActividad, string CodSubAct)
        {
            return _db.AF_TiposActividadesEco_SubActividad_Eliminar(CodEmpresa, Usuario, CodActividad, CodSubAct);
        }
    }
}