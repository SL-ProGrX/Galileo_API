using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPTiposProvBL
    {
        private readonly FrmCxPTiposProvDB _db;

        public FrmCxPTiposProvBL(IConfiguration config)
        {
            _db = new FrmCxPTiposProvDB(config);
        }

        public ErrorDto<List<TiposProveedorDto>> ObtenerClasificacionProveedores(int CodCliente)
        {
            return _db.ObtenerClasificacionProveedores(CodCliente);
        }

        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodCliente)
        {
            return _db.ObtenerProveedores(CodCliente);
        }

        public ErrorDto TipoProveedor_Actualizar(TiposProveedorDto request)
        {
            return _db.TipoProveedor_Actualizar(request);
        }

        public ErrorDto TipoProveedor_Eliminar(TiposProveedorDto request)
        {
            return _db.TipoProveedor_Eliminar(request);
        }

        public ErrorDto TipoProveedor_Insertar(TiposProveedorDto request)
        {
            return _db.TipoProveedor_Insertar(request);
        }
    }//end class
}//end namespace