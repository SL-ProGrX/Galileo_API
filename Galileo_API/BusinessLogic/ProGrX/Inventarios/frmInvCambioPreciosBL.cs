using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvCambioPreciosBL
    {
        private readonly FrmInvCambioPreciosDB _db;

        public FrmInvCambioPreciosBL(
            IConfiguration config)
        {
            _db = new FrmInvCambioPreciosDB(
                config);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            INV_CambioPrecios_TiposPrecio_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_CambioPrecios_TiposPrecio_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_CambioPrecios_Proveedores_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_CambioPrecios_Proveedores_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            CambioPrecioArchivoCargaResponse>
            INV_CambioPrecios_Archivo_Cargar(
                int CodEmpresa,
                CambioPrecioArchivoCargaRequest request)
        {
            return _db
                .INV_CambioPrecios_Archivo_Cargar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            INV_CambioPrecios_Archivo_Procesar(
                int CodEmpresa,
                CambioPrecioArchivoProcesarRequest request)
        {
            return _db
                .INV_CambioPrecios_Archivo_Procesar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto<
            List<FacturaPrecioDetalleDto>>
            INV_CambioPrecios_Factura_Detalle_Obtener(
                int CodEmpresa,
                string codFactura,
                int codProveedor)
        {
            return _db
                .INV_CambioPrecios_Factura_Detalle_Obtener(
                    CodEmpresa,
                    codFactura,
                    codProveedor);
        }

        public ErrorDto
            INV_CambioPrecios_Factura_Precios_Actualizar(
                int CodEmpresa,
                CambioPrecioFacturaActualizarRequest request)
        {
            return _db
                .INV_CambioPrecios_Factura_Precios_Actualizar(
                    CodEmpresa,
                    request);
        }
    }
}