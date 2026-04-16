using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic
{
    public class FrmCajasRoeAnularBL
    { 
        private readonly FrmCajasRoeAnularDB _db;
        public FrmCajasRoeAnularBL(IConfiguration config)
        {
            _db = new FrmCajasRoeAnularDB(config);
        }
        
        public ErrorDto<CajasRoeAnularLista> CajasRoeAnular_Obtener(int CodEmpresa, FiltrosCajasRoeAnularData filtros)
        {
            return _db.CajasRoeAnular_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto CajasRoeAnular_Anular(int CodEmpresa, string usuario, string roe, string notas)
        {
            return _db.CajasRoeAnular_Anular(CodEmpresa, usuario, roe, notas);
        }
    }
}