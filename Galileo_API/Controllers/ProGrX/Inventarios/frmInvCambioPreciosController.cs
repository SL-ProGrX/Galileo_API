using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvCambioPreciosController
        : ControllerBase
    {
        private readonly FrmInvCambioPreciosBL _bl;

        public FrmInvCambioPreciosController(
            IConfiguration config)
        {
            _bl = new FrmInvCambioPreciosBL(
                config);
        }

        [HttpGet(
            "INV_CambioPrecios_TiposPrecio_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            INV_CambioPrecios_TiposPrecio_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_CambioPrecios_TiposPrecio_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_CambioPrecios_Proveedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_CambioPrecios_Proveedores_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_CambioPrecios_Proveedores_Obtener(
                    CodEmpresa);
        }

        [HttpPost(
            "INV_CambioPrecios_Archivo_Cargar")]
        public ErrorDto<
            CambioPrecioArchivoCargaResponse>
            INV_CambioPrecios_Archivo_Cargar(
                int CodEmpresa,
                CambioPrecioArchivoCargaRequest request)
        {
            return _bl
                .INV_CambioPrecios_Archivo_Cargar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "INV_CambioPrecios_Archivo_Procesar")]
        public ErrorDto
            INV_CambioPrecios_Archivo_Procesar(
                int CodEmpresa,
                CambioPrecioArchivoProcesarRequest request)
        {
            return _bl
                .INV_CambioPrecios_Archivo_Procesar(
                    CodEmpresa,
                    request);
        }

        [HttpGet(
            "INV_CambioPrecios_Factura_Detalle_Obtener")]
        public ErrorDto<
            List<FacturaPrecioDetalleDto>>
            INV_CambioPrecios_Factura_Detalle_Obtener(
                int CodEmpresa,
                string codFactura,
                int codProveedor)
        {
            return _bl
                .INV_CambioPrecios_Factura_Detalle_Obtener(
                    CodEmpresa,
                    codFactura,
                    codProveedor);
        }

        [HttpPut(
            "INV_CambioPrecios_Factura_Precios_Actualizar")]
        public ErrorDto
            INV_CambioPrecios_Factura_Precios_Actualizar(
                int CodEmpresa,
                CambioPrecioFacturaActualizarRequest request)
        {
            return _bl
                .INV_CambioPrecios_Factura_Precios_Actualizar(
                    CodEmpresa,
                    request);
        }
    }
}