using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprAnulacionCompraBL
    {
        private readonly FrmCprAnulacionCompraDB _db;

        public FrmCprAnulacionCompraBL(IConfiguration config)
        {
            _db = new FrmCprAnulacionCompraDB(config);
        }

        public ErrorDto<List<CompraDto>> Compras_Obtener(int CodEmpresa, string filtro)
        {
            return _db.Compras_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<CompraAnulacionDatosDto> Compra_Datos_Obtener(int CodEmpresa, string Cod_Compra)
        {
            return _db.Compra_Datos_Obtener(CodEmpresa, Cod_Compra);
        }


        public ErrorDto<List<CompraDetalleDto>> CompraDetalles_Obtener(int CodEmpresa, string Cod_Factura)
        {
            return _db.CompraDetalles_Obtener(CodEmpresa, Cod_Factura);
        }

        public ErrorDto<CompraAnulacionDto> Compra_Obtener(int CodEmpresa, string codCompra)
        {
            return _db.Compra_Obtener(CodEmpresa, codCompra);
        }

        public ErrorDto Compra_Anular(int CodEmpresa, CompraAnulacionDto compraDto)
        {
            return _db.Compra_Anular(CodEmpresa, compraDto);
        }

        public ErrorDto<CompraAnulacionDatosDto> Compra_Anulacion_Datos_Obtener(int CodEmpresa, CompraAnulacionDatosRequestDto compraAnulacionDatosRequestDto)
        {
            return _db.Compra_Anulacion_Datos_Obtener(CodEmpresa, compraAnulacionDatosRequestDto);
        }
    }
}
