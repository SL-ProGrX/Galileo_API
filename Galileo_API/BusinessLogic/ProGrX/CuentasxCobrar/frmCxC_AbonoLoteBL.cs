using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public sealed class FrmCxCAbonoLoteBl
    {
        private readonly FrmCxCAbonoLoteDb _db;

        public FrmCxCAbonoLoteBl(IConfiguration config)
        {
            _db = new FrmCxCAbonoLoteDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_Clientes_Obtener(int codEmpresa)
        {
            return _db.CxC_AbonoLote_Clientes_Obtener(codEmpresa);
        }

        public int CxC_AbonoLote_Derecho_Aplicar_Obtener(
            int codEmpresa,
            string usuario)
        {
            return _db.CxC_AbonoLote_Derecho_Aplicar_Obtener(
                codEmpresa,
                usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_Divisas_Obtener(
                int codEmpresa,
                string cedula)
        {
            return _db.CxC_AbonoLote_Divisas_Obtener(
                codEmpresa,
                cedula);
        }

        public ErrorDto<List<CxCFactPendienteCancelacionData>>
            CxC_AbonoLote_Facturas_Obtener(
                int codEmpresa,
                CxCAbonoLoteFacturasRequest request)
        {
            return _db.CxC_AbonoLote_Facturas_Obtener(
                codEmpresa,
                request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_TiposDocumento_Obtener(
                int codEmpresa,
                string caja)
        {
            return _db.CxC_AbonoLote_TiposDocumento_Obtener(
                codEmpresa,
                caja);
        }

        public ErrorDto<CxCAbonoLoteAplicarData>
            CxC_AbonoLote_Aplicar(
                int codEmpresa,
                CxCAbonoLoteAplicarRequest request)
        {
            return _db.CxC_AbonoLote_Aplicar(
                codEmpresa,
                request);
        }
    }
}
