using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFProfesionesBL
    {
        private readonly FrmAFProfesionesDB _db;

        public FrmAFProfesionesBL(IConfiguration config)
        {
            _db = new FrmAFProfesionesDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> AF_Profesiones_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_Profesiones_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_Profesiones_Guardar(int CodEmpresa, string Usuario, string Codigo, string Descripcion)
        {
            return _db.AF_Profesiones_Guardar(CodEmpresa, Usuario, Codigo, Descripcion);
        }

        public ErrorDto AF_Profesiones_Eliminar(int CodEmpresa, string Usuario, int Codigo, string Descripcion)
        {
            return _db.AF_Profesiones_Eliminar(CodEmpresa, Usuario, Codigo, Descripcion);
        }
    }
}