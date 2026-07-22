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
        public ErrorDto Cpr_TiposOrden_Guardar(
            int CodEmpresa,
            string usuario,
            TiposOrdenDto tipoOrden)
        {
            return _db.Cpr_TiposOrden_Guardar(CodEmpresa, usuario, tipoOrden);
        }

        public ErrorDto Cpr_TiposOrden_Eliminar(
            int CodEmpresa,
            string usuario,
            string tipoOrden)
        {
            return _db.Cpr_TiposOrden_Eliminar(CodEmpresa, usuario, tipoOrden);
        }

        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int CodEmpresa, string usuario)
        {
            return _db.rangosMontos_Obtener(CodEmpresa, usuario);
        }

    }
}
