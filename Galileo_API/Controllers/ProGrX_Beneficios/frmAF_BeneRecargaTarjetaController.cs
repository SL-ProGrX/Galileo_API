using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints de Recarga de Tarjetas de Beneficios (frmAF_BeneRecargaTarjeta).
    /// </summary>
    [Route("api/frmAF_BeneRecargaTarjeta")]
    [ApiController]
    public class FrmAfBeneRecargaTarjetaController : ControllerBase
    {
        private readonly FrmAfBeneRecargaTarjetaBL _bl;

        public FrmAfBeneRecargaTarjetaController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneRecargaTarjetaBL(config);
        }

        /// <summary>Lista de remesas de tarjetas.</summary>
        [Authorize]
        [HttpGet("AfiTajertasRemesas_Obtener")]
        public ErrorDto<AfiBeneTarjetasRemesasDataLista> AfiTajertasRemesas_Obtener(int CodCliente, string? filtro, int? pagina, int? paginacion)
            => _bl.AfiTajertasRemesas_Obtener(CodCliente, filtro, pagina, paginacion);

        /// <summary>Remesa de tarjetas por código.</summary>
        [Authorize]
        [HttpGet("AfiTarjetasRemesa_Obtener")]
        public ErrorDto<AfiBeneTarjetasRemesasData> AfiTarjetasRemesa_Obtener(int CodCliente, int cod_remesa)
            => _bl.AfiTarjetasRemesa_Obtener(CodCliente, cod_remesa);

        /// <summary>Remesas de tarjetas abiertas.</summary>
        [Authorize]
        [HttpGet("AfiTarjetasRemesasAbiertas_Obtener")]
        public ErrorDto<List<AfiBeneTarjetasRemesasData>> AfiTarjetasRemesasAbiertas_Obtener(int CodCliente)
            => _bl.AfiTarjetasRemesasAbiertas_Obtener(CodCliente);

        /// <summary>Tarjetas de regalo por estado.</summary>
        [Authorize]
        [HttpGet("AfiTarjetasRegalo_Obtener")]
        public ErrorDto<AfiBeneTarjetasDataLista> AfiTarjetasRegalo_Obtener(int CodCliente, string filtros, string estado, bool? sinAsignar)
            => _bl.AfiTarjetasRegalo_Obtener(CodCliente, filtros, estado, sinAsignar);

        /// <summary>Productos habilitados como tarjeta de regalo.</summary>
        [Authorize]
        [HttpGet("AfiTarjetasProductos_Obtener")]
        public ErrorDto<List<ProductoData>> AfiTarjetasProductos_Obtener(int CodCliente)
            => _bl.AfiTarjetasProductos_Obtener(CodCliente);

        /// <summary>Remesas de tarjetas con datos de proveedor.</summary>
        [Authorize]
        [HttpGet("AfiRecargaTarjProveedor_ObtenerRemesas")]
        public ErrorDto<AfiBeneTarjetasRemesasDataLista> AfiRecargaTarjProveedor_ObtenerRemesas(int CodCliente, string? filtro, int? pagina, int? paginacion)
            => _bl.AfiRecargaTarjProveedor_ObtenerRemesas(CodCliente, filtro, pagina, paginacion);

        /// <summary>Tarjetas de regalo recargadas de una remesa.</summary>
        [Authorize]
        [HttpGet("AfiTarjetasRegaloRecargadas_Obtener")]
        public ErrorDto<List<AfiBeneTarjetasData>> AfiTarjetasRegaloRecargadas_Obtener(int CodCliente, int cod_remesa)
            => _bl.AfiTarjetasRegaloRecargadas_Obtener(CodCliente, cod_remesa);

        /// <summary>Inserta una remesa de tarjetas.</summary>
        [Authorize]
        [HttpPost("AfiTarjetasRemesa_Insertar")]
        public ErrorDto AfiTarjetasRemesa_Insertar(int CodCliente, [FromBody] AfiBeneTarjetasRemesasData remesa)
            => _bl.AfiTarjetasRemesa_Insertar(CodCliente, remesa);

        /// <summary>Actualiza una remesa de tarjetas.</summary>
        [Authorize]
        [HttpPut("AfiTarjetasRemesa_Actualizar")]
        public ErrorDto AfiTarjetasRemesa_Actualizar(int CodCliente, [FromBody] AfiBeneTarjetasRemesasData remesa)
            => _bl.AfiTarjetasRemesa_Actualizar(CodCliente, remesa);

        /// <summary>Elimina una remesa de tarjetas.</summary>
        [Authorize]
        [HttpDelete("AfiTarjetasRemesa_Eliminar")]
        public ErrorDto AfiTarjetasRemesa_Eliminar(int CodCliente, long cod_remesa)
            => _bl.AfiTarjetasRemesa_Eliminar(CodCliente, cod_remesa);

        /// <summary>Inserta una tarjeta de regalo.</summary>
        [Authorize]
        [HttpPost("AfiTarjetasRegalo_Insertar")]
        public ErrorDto AfiTarjetasRegalo_Insertar(int CodCliente, [FromBody] string tarjetas)
            => _bl.AfiTarjetasRegalo_Insertar(CodCliente, tarjetas);

        /// <summary>Actualiza una tarjeta de regalo.</summary>
        [Authorize]
        [HttpPut("AfiTarjetasRegalo_Actualizar")]
        public ErrorDto AfiTarjetasRegalo_Actualizar(int CodCliente, [FromBody] string tarjetas)
            => _bl.AfiTarjetasRegalo_Actualizar(CodCliente, tarjetas);

        /// <summary>Elimina una tarjeta de regalo.</summary>
        [Authorize]
        [HttpDelete("AfiTarjetasRegalo_Eliminar")]
        public ErrorDto AfiTarjetasRegalo_Eliminar(int CodCliente, int id_tr)
            => _bl.AfiTarjetasRegalo_Eliminar(CodCliente, id_tr);

        /// <summary>Recarga las tarjetas de regalo.</summary>
        [Authorize]
        [HttpPost("AfiTarjetasRegalo_Recargar")]
        public ErrorDto AfiTarjetasRegalo_Recargar(int CodCliente, [FromBody] string tarjetas)
            => _bl.AfiTarjetasRegalo_Recargar(CodCliente, tarjetas);

        /// <summary>Envía por correo la solicitud de pago de recarga de tarjetas.</summary>
        [Authorize]
        [HttpPost("AfiTarjetasRegaloRecargadas_Enviar")]
        public Task<ErrorDto> AfiTarjetasRegaloRecargadas_Enviar(int CodCliente, [FromBody] DocArchivoBeneRecargaTarjetaDto parametros)
            => _bl.AfiTarjetasRegaloRecargadas_Enviar(CodCliente, parametros);
    }
}
