using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprCompraDirectaBL
    {
        private readonly FrmCprCompraDirectaDB _db;

        public FrmCprCompraDirectaBL(IConfiguration config)
        {
            _db = new FrmCprCompraDirectaDB(config);
        }

        public ErrorDto<CompraDirectaData?> CompraDirecta_Obtener(int CodEmpresa, string CodCompra, string CodOrden, int Codproveedor)
        {
            return _db.CompraDirecta_Obtener(CodEmpresa, CodCompra, CodOrden, Codproveedor);
        }

        public ErrorDto<CompraDirectaListaData> CompraDirectaDetalle_Obtener(int CodEmpresa, string jfiltros, string? CodFactura, int? Codproveedor)
        {
            return _db.CompraDirectaDetalle_Obtener(CodEmpresa, jfiltros, CodFactura, Codproveedor);
        }

        public ErrorDto CostoArticulos_Actualiza(int CodEmpresa, string Usuario, string CodCompra)
        {
            return _db.CostoArticulos_Actualiza(CodEmpresa, Usuario, CodCompra);
        }

        public ErrorDto CompraDirecta_Insertar(int CodEmpresa, CompraDirectaInsert orden)
        {
            return _db.CompraDirecta_Insertar(CodEmpresa, orden);
        }

        public ErrorDto<List<CompraDirectaResumenData>> CprCompraDirecta_Lista_Obtener(int codEmpresa)
        {
            return _db.CprCompraDirecta_Lista_Obtener(codEmpresa);
        }

    }
}
