using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFTiposIdsBL
    {
        private readonly FrmAFTiposIdsDB _db;

        public FrmAFTiposIdsBL(IConfiguration config)
        {
            _db = new FrmAFTiposIdsDB(config);
        }

        public ErrorDto<AfTiposIdsLista> AF_TiposIds_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_TiposIds_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_TiposIds_Guardar(int CodEmpresa, string Usuario, AfTiposIdsDto Info)
        {
            return _db.AF_TiposIds_Guardar(CodEmpresa, Usuario, Info);
        }

        public ErrorDto AF_TiposIds_Eliminar(int CodEmpresa, string Usuario, int TipoId)
        {
            return _db.AF_TiposIds_Eliminar(CodEmpresa, Usuario, TipoId);
        }
    }
}