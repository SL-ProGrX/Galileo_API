using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTiposMarcasBL
    {
        private readonly FrmInvTiposMarcasDB _db;

        public FrmInvTiposMarcasBL(IConfiguration config)
        {
            _db = new FrmInvTiposMarcasDB(config);
        }

        public ErrorDto<MarcasDataLista> Marcas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Marcas_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto<List<MarcasDto>> Marcas_ObtenerTodos(int CodEmpresa)
        {
            return _db.Marcas_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto Marcas_Insertar(int CodEmpresa, MarcasDto request)
        {
            return _db.Marcas_Insertar(CodEmpresa, request);
        }

        public ErrorDto Marcas_Actualizar(int CodEmpresa, MarcasDto request)
        {
            return _db.Marcas_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Marcas_Eliminar(int CodEmpresa, string marca)
        {
            return _db.Marcas_Eliminar(CodEmpresa, marca);
        }
    }
}
