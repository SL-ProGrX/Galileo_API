using Galileo.DataBaseTier; 
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic
{
    public class FrmCajasDepositosTransitoBL
    { 
        private readonly FrmCajasDepositosTransitoDB _db;
        public FrmCajasDepositosTransitoBL(IConfiguration config)
        {
            _db = new FrmCajasDepositosTransitoDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_DepositosTransito_Cuentas_Obtener(int CodEmpresa)
        {
            return _db.Cajas_DepositosTransito_Cuentas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CajasDepositosTransitoData>> Cajas_Depositos_Transito_Consultar(int CodEmpresa, FiltrosData filtros)
        {
            return _db.Cajas_Depositos_Transito_Consultar(CodEmpresa,  filtros);
        }
    }
}