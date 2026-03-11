using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using static Galileo_API.Models.ProGrX_Polizas.FrmPolizaFacturaVerModels;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizaFacturaVerBL
    {
        private readonly FrmPolizaFacturaVerDB _db;

        public FrmPolizaFacturaVerBL(IConfiguration config)
        {
            _db = new FrmPolizaFacturaVerDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizaFacturaVer_Divisas_Obtener(int codEmpresa, int codContabilidad)
                 => _db.CrdPolizaFacturaVer_Divisas_Obtener(codEmpresa, codContabilidad);
        public ErrorDto<CrdPolizaFacturaVerDivisaLocalModel> CrdPolizaFacturaVer_DivisaLocal_Obtener(int codEmpresa, int codContabilidad)
                => _db.CrdPolizaFacturaVer_DivisaLocal_Obtener(codEmpresa, codContabilidad);
        public ErrorDto<CrdPolizaFacturaVerFacturaResponse> CrdPolizaFacturaVer_Factura_Obtener(
            int codEmpresa,int proveedor, string factura)
                => _db.CrdPolizaFacturaVer_Factura_Obtener(codEmpresa, proveedor, factura);
        public ErrorDto<CrdPolizaFacturaVerAsientosResponse> CrdPolizaFacturaVer_Asientos_Obtener(
          int codEmpresa,int proveedor,string factura)
                 => _db.CrdPolizaFacturaVer_Asientos_Obtener(codEmpresa, proveedor, factura);
    }
}
