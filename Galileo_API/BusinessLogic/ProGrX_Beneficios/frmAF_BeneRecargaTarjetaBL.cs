using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio de Recarga de Tarjetas de Beneficios (frmAF_BeneRecargaTarjeta).
    /// </summary>
    public class FrmAfBeneRecargaTarjetaBL
    {
        private readonly FrmAfBeneRecargaTarjetaDB _db;

        public FrmAfBeneRecargaTarjetaBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneRecargaTarjetaDB(config);
        }

        /// <summary>Lista de remesas de tarjetas.</summary>
        public ErrorDto<AfiBeneTarjetasRemesasDataLista> AfiTajertasRemesas_Obtener(int CodCliente, string? filtro, int? pagina, int? paginacion)
            => _db.AfiTajertasRemesas_Obtener(CodCliente, filtro, pagina, paginacion);

        /// <summary>Remesa de tarjetas por código.</summary>
        public ErrorDto<AfiBeneTarjetasRemesasData> AfiTarjetasRemesa_Obtener(int CodCliente, int cod_remesa)
            => _db.AfiTarjetasRemesa_Obtener(CodCliente, cod_remesa);

        /// <summary>Remesas de tarjetas abiertas.</summary>
        public ErrorDto<List<AfiBeneTarjetasRemesasData>> AfiTarjetasRemesasAbiertas_Obtener(int CodCliente)
            => _db.AfiTarjetasRemesasAbiertas_Obtener(CodCliente);

        /// <summary>Tarjetas de regalo por estado.</summary>
        public ErrorDto<AfiBeneTarjetasDataLista> AfiTarjetasRegalo_Obtener(int CodCliente, string filtros, string estado, bool? sinAsignar)
            => _db.AfiTarjetasRegalo_Obtener(CodCliente, filtros, estado, sinAsignar);

        /// <summary>Productos habilitados como tarjeta de regalo.</summary>
        public ErrorDto<List<ProductoData>> AfiTarjetasProductos_Obtener(int CodCliente)
            => _db.AfiTarjetasProductos_Obtener(CodCliente);

        /// <summary>Remesas de tarjetas con datos de proveedor.</summary>
        public ErrorDto<AfiBeneTarjetasRemesasDataLista> AfiRecargaTarjProveedor_ObtenerRemesas(int CodCliente, string? filtro, int? pagina, int? paginacion)
            => _db.AfiRecargaTarjProveedor_ObtenerRemesas(CodCliente, filtro, pagina, paginacion);

        /// <summary>Tarjetas de regalo recargadas de una remesa.</summary>
        public ErrorDto<List<AfiBeneTarjetasData>> AfiTarjetasRegaloRecargadas_Obtener(int CodCliente, int cod_remesa)
            => _db.AfiTarjetasRegaloRecargadas_Obtener(CodCliente, cod_remesa);

        /// <summary>Inserta una remesa de tarjetas.</summary>
        public ErrorDto AfiTarjetasRemesa_Insertar(int CodCliente, AfiBeneTarjetasRemesasData remesa)
            => _db.AfiTarjetasRemesa_Insertar(CodCliente, remesa);

        /// <summary>Actualiza una remesa de tarjetas.</summary>
        public ErrorDto AfiTarjetasRemesa_Actualizar(int CodCliente, AfiBeneTarjetasRemesasData remesa)
            => _db.AfiTarjetasRemesa_Actualizar(CodCliente, remesa);

        /// <summary>Elimina una remesa de tarjetas.</summary>
        public ErrorDto AfiTarjetasRemesa_Eliminar(int CodCliente, long cod_remesa)
            => _db.AfiTarjetasRemesa_Eliminar(CodCliente, cod_remesa);

        /// <summary>Inserta una tarjeta de regalo.</summary>
        public ErrorDto AfiTarjetasRegalo_Insertar(int CodCliente, string tarjetas)
            => _db.AfiTarjetasRegalo_Insertar(CodCliente, tarjetas);

        /// <summary>Actualiza una tarjeta de regalo.</summary>
        public ErrorDto AfiTarjetasRegalo_Actualizar(int CodCliente, string tarjetas)
            => _db.AfiTarjetasRegalo_Actualizar(CodCliente, tarjetas);

        /// <summary>Elimina una tarjeta de regalo.</summary>
        public ErrorDto AfiTarjetasRegalo_Eliminar(int CodCliente, int id_tr)
            => _db.AfiTarjetasRegalo_Eliminar(CodCliente, id_tr);

        /// <summary>Recarga las tarjetas de regalo.</summary>
        public ErrorDto AfiTarjetasRegalo_Recargar(int CodCliente, string tarjetas)
            => _db.AfiTarjetasRegalo_Recargar(CodCliente, tarjetas);

        /// <summary>Envía por correo la solicitud de pago de recarga de tarjetas.</summary>
        public Task<ErrorDto> AfiTarjetasRegaloRecargadas_Enviar(int CodCliente, DocArchivoBeneRecargaTarjetaDto parametros)
            => _db.AfiTarjetasRegaloRecargadas_Enviar(CodCliente, parametros);
    }
}
