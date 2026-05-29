using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfPadronEmpleadosBL
    {
        private readonly FrmAfPadronEmpleadosDB _db;

        public FrmAfPadronEmpleadosBL(IConfiguration config)
        {
            _db = new FrmAfPadronEmpleadosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosInstituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_PadronEmpleadosInstituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosEstados_Obtener(int CodEmpresa)
        {
            return _db.AF_PadronEmpleadosEstados_Obtener(CodEmpresa);
        }

        public ErrorDto AF_PadronEmpleados_Eliminar(int CodEmpresa, string cedula)
        {
            return _db.AF_PadronEmpleados_Eliminar(CodEmpresa, cedula);
        }

        public ErrorDto<TablasListaGenericaModel> AF_PadronEmpleados_Obtener(int CodEmpresa, bool exporta, string jFiltros, string jTblFiltros)
        {
            AfPadronEmpleadosFiltro filtros = JsonConvert.DeserializeObject<AfPadronEmpleadosFiltro>(jFiltros) ?? new AfPadronEmpleadosFiltro();
            FiltrosLazyLoadData tblFiltros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jTblFiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_PadronEmpleados_Obtener(CodEmpresa, exporta, filtros, tblFiltros);
        }
    }
}