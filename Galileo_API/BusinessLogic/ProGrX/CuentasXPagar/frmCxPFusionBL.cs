using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPFusionBL
    {
        private readonly FrmCxPFusionDB _db;

        public FrmCxPFusionBL(IConfiguration config)
        {
            _db = new FrmCxPFusionDB(config);
        }
        public ErrorDto<CxpProveedoresDataLista> Proveedores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Proveedores_Obtener(CodCliente, pagina, paginacion, filtro);
        }
        public ErrorDto Fusion_Aplicar(int CodCliente, int proveedor, List<CxpProveedorData> proveedores)
        {
            return _db.Fusion_Aplicar(CodCliente, proveedor, proveedores);
        }
    }
}