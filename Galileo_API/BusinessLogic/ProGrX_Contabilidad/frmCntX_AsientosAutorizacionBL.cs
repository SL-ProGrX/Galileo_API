using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXAsientosAutorizacionBl
    {
        private readonly FrmCntXAsientosAutorizacionDb _db;

        public FrmCntXAsientosAutorizacionBl(IConfiguration config) => 
            _db = new FrmCntXAsientosAutorizacionDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Tipos_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXAsientos_Tipos_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<CntXAsientoAutorizacionData>> CntXAsientos_ListaPendientes_Obtener(int codEmpresa, int codConta, string tipoAsiento, int anio, int mes)
        {
            return _db.CntXAsientos_ListaPendientes_Obtener(codEmpresa, codConta, tipoAsiento, anio, mes);
        }

        public ErrorDto CntXAsientos_Autorizar(int codEmpresa, int codConta, string usuario, List<CntXAsientoAutorizacionData> lista)
        {
            return _db.CntXAsientos_Autorizar(codEmpresa, codConta, usuario, lista);
        }
    }
}
