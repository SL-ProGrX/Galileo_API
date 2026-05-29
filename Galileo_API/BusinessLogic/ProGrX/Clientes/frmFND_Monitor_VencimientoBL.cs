using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.DataBaseTier.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmFndMonitorVencimientoBl
    {
        private readonly FrmFndMonitorVencimientoBd _db;

        public FrmFndMonitorVencimientoBl(IConfiguration config)
        {
            _db = new FrmFndMonitorVencimientoBd(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Planes_TipoPlan_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Planes_TipoPlan_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndPlanesItem>> Fnd_Planes_Obtener(int CodEmpresa, FndPlanesObtenerRequest request)
        {
            return _db.Fnd_Planes_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<FndVencimientosConsultaResult>> Fnd_Vencimientos_Consulta(int CodEmpresa, FndVencimientosConsultaRequest request)
        {
            return _db.Fnd_Vencimientos_Consulta(CodEmpresa, request);
        }
    }
}