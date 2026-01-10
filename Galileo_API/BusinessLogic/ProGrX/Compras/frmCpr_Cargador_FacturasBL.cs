using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprCargadorFacturasBL
    {
        readonly FrmCprCargadorFacturasDB _db;

        public FrmCprCargadorFacturasBL(IConfiguration config)
        {
            _db = new FrmCprCargadorFacturasDB(config);
        }

        public ErrorDto<CprFacturasXmlLista> Cargador_Facturas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            return _db.Cargador_Facturas_Obtener(CodEmpresa, proveedor, filtros);
        }

        public ErrorDto<CprFacturasXmlDto> Cargador_Factura_ObtenerPorId(int CodEmpresa, int id)
        {
            return _db.Cargador_Factura_ObtenerPorId(CodEmpresa, id);
        }

        public ErrorDto Cargador_Facturas_Guardar(int CodEmpresa, CprFacturasXmlDto request)
        {
            return _db.Cargador_Facturas_Guardar(CodEmpresa, request);
        }

        public ErrorDto Cargador_Facturas_Actualizar(int CodEmpresa, CprFacturasXmlDto request)
        {
            return _db.Cargador_Facturas_Actualizar(CodEmpresa, request);
        }

        public ErrorDto<List<CprFacturasLineasXmlData>> Cargador_FacturasDetalle_Obtener(int CodEmpresa, int id, string? cod_proveedor)
        {
            return _db.Cargador_FacturasDetalle_Obtener(CodEmpresa, id, cod_proveedor);
        }

        public ErrorDto<CprFacturasXmlLista> Cargador_FacturasActivas_Obtener(int CodEmpresa, int proveedor, string filtros)
        {
            return _db.Cargador_FacturasActivas_Obtener(CodEmpresa, proveedor, filtros);
        }
    }
}