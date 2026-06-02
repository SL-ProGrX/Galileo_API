using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfListadoIngresoBL
    {
        private readonly FrmAfListadoIngresoDB _db;
        public FrmAfListadoIngresoBL(IConfiguration config)
        {
            _db = new FrmAfListadoIngresoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_ListadoIngreso_Estados_Obtener(int CodEmpresa)
        {
            return _db.AF_ListadoIngreso_Estados_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_ListadoIngreso_Instituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_ListadoIngreso_Instituciones_Obtener(CodEmpresa);
        }
    }
}