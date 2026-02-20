using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCFacturasCancelaPagadorBl
    {
        private readonly FrmCxCFacturasCancelaPagadorDb _db;

        public FrmCxCFacturasCancelaPagadorBl(IConfiguration config) => _db = new FrmCxCFacturasCancelaPagadorDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            return _db.CxCFactCancPag_TipoDoc_Obtener(codEmpresa, caja);    
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_Pagadores_Obtener(int codEmpresa)
        {
            return _db.CxCFactCancPag_Pagadores_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_Divisas_Obtener(int codEmpresa, string codPagador)
        {
            return _db.CxCFactCancPag_Divisas_Obtener(codEmpresa, codPagador);
        }

        public ErrorDto<List<CxCFactPendienteCancelacionData>> CxCFactCancPag_FacturasPendientes_Obtener(int codEmpresa, CxCFactCancPagFacturasRequest filtro)
        {
            return _db.CxCFactCancPag_FacturasPendientes_Obtener(codEmpresa, filtro);
        }

        public ErrorDto CxCFactCancPag_Abono_Registrar(int codEmpresa, CxCFactCancPagRegistrarAbonoRequest request)
        {
            return _db.CxCFactCancPag_Abono_Registrar(codEmpresa, request);
        }
    }
}
