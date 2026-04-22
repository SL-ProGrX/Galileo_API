using Galileo.DataBaseTier; 
using Galileo.Models;
using Galileo.Models.ERROR;
 
namespace Galileo.BusinessLogic
{
    public class FrmCajasTransacTipoCambioBL
    { 
        private readonly FrmCajasTransacTipoCambioDB _db;
        public FrmCajasTransacTipoCambioBL(IConfiguration config)
        {
            _db = new FrmCajasTransacTipoCambioDB(config);
        }
        
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TransacTipoCambio_TipoDocumento_Obtener(int CodEmpresa, string Caja)
        {
            return _db.Cajas_TransacTipoCambio_TipoDocumento_Obtener(CodEmpresa,Caja);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TransacTipoCambio_Divisas_Obtener(int CodEmpresa)
        {
            return _db.Cajas_TransacTipoCambio_Divisas_Obtener(CodEmpresa);
        }
    }
}