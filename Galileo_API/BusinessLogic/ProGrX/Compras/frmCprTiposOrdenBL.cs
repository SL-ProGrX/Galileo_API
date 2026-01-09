using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprTiposOrdenBL
    {
         private readonly FrmCprTiposOrdenDB _db;

        public FrmCprTiposOrdenBL(IConfiguration config)
        {
            _db = new FrmCprTiposOrdenDB(config);
        }
        public ErrorDto<TiposOrdenLista> ObtenerTiposOrdenes(int CodCliente, string filtros)
        {
            return _db.ObtenerTiposOrdenes(CodCliente, filtros);
        }
        public ErrorDto TipoOrden_Actualizar(int CodEmpresa, TiposOrdenDto tiposOrden)
        {
            return _db.TipoOrden_Actualizar(CodEmpresa, tiposOrden);
        }
        public ErrorDto TipoOrden_Eliminar(int CodEmpresa, string tiposOrden)
        {
            return _db.TipoOrden_Eliminar(CodEmpresa, tiposOrden);
        }

        public ErrorDto TipoOrden_Insertar(int CodEmpresa, TiposOrdenDto tiposOrden)
        {
            return _db.TipoOrden_Insertar(CodEmpresa, tiposOrden);
        }

        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int CodEmpresa, string usuario)
        {
            return _db.rangosMontos_Obtener(CodEmpresa, usuario);
        }

    }
}
