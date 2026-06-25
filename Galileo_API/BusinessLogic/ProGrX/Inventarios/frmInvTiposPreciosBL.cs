using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTiposPreciosBL
    {
        private readonly FrmInvTiposPreciosDB _db;
        public FrmInvTiposPreciosBL(IConfiguration config)
        {
            _db = new FrmInvTiposPreciosDB(config);
        }

        public ErrorDto<PreciosDataLista> Precios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Precios_Obtener(CodCliente, pagina, paginacion, filtro);
        }
        public ErrorDto<List<Precio>> Precios_ObtenerTodos(int CodEmpresa)
        {
            return _db.Precios_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto Precios_Insertar(int CodEmpresa, Precio request)
        {
            return _db.Precios_Insertar(CodEmpresa, request);
        }

        public ErrorDto Precios_Actualizar(int CodEmpresa, Precio request)
        {
            return _db.Precios_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Precios_Eliminar(int CodEmpresa, string precio)
        {
            return _db.Precios_Eliminar(CodEmpresa, precio);
        }
    }
}
