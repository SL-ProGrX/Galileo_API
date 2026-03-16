using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXAsientosBl
    {
        private readonly FrmCntXAsientosDb _db;

        public FrmCntXAsientosBl(IConfiguration config) 
            => _db = new FrmCntXAsientosDb(config);

        public ErrorDto<CntXAsientoData?> CntXAsientos_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            return _db.CntXAsientos_Obtener(codEmpresa, codConta, tipoAsiento, numAsiento);
        }

        public ErrorDto<List<CntXAsientoDetalleData>> CntXAsientos_Detalle_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            return _db.CntXAsientos_Detalle_Obtener(codEmpresa, codConta, tipoAsiento, numAsiento);
        }

        public ErrorDto<CntXAsientoData?> CntXAsientos_Scroll_Obtener(
            int codEmpresa, int codConta, int anio, int mes, string tipoAsiento, string numAsiento, int scrollCode)
        {
            return _db.CntXAsientos_Scroll_Obtener(codEmpresa, codConta, anio, mes, tipoAsiento, numAsiento, scrollCode);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Lista_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _db.CntXAsientos_Lista_Obtener(codEmpresa, codConta, tipoAsiento);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXTiposAsientos_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXTiposAsientos_Lista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<string?> CntXTiposAsientos_Descripcion_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _db.CntXTiposAsientos_Descripcion_Obtener(codEmpresa, codConta, tipoAsiento);    
        }
    }
}
