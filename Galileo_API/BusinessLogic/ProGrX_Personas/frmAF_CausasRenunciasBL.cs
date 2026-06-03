using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCausasRenunciasBL
    {
        private readonly FrmAFCausasRenunciasDB _db;

        public FrmAFCausasRenunciasBL(IConfiguration config)
        {
            _db = new FrmAFCausasRenunciasDB(config);
        }

        public ErrorDto<List<AfCausasRenunciasData>> AF_CausasRenuncias_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_CausasRenuncias_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_CausasRenuncias_Guardar(int CodEmpresa, string usuario, AfCausasRenunciasData causa)
        {
            return _db.AF_CausasRenuncias_Guardar(CodEmpresa, causa, usuario);
        }

        public ErrorDto AF_CausasRenuncias_Eliminar(int CodEmpresa, int id_causa, string usuario)
        {
            return _db.AF_CausasRenuncias_Eliminar(CodEmpresa, id_causa, usuario);
        }
    }
}
