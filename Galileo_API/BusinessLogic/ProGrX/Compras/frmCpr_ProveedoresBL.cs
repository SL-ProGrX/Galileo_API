using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprProveedoresBL
    {
        readonly FrmCprProveedoresDB _db;

        public FrmCprProveedoresBL(IConfiguration config)
        {
            _db = new FrmCprProveedoresDB(config);
        }

        public ErrorDto CprProveedores_Importar(int CodEmpresa)
        {
            return _db.CprProveedores_Importar(CodEmpresa);
        }

        public ErrorDto<CprProveedoresDto> CprProveedor_Scroll(int CodEmpresa, int scroll, string? codigo)
        {
            return _db.CprProveedor_Scroll(CodEmpresa, scroll, codigo);
        }

        public ErrorDto<CprProveedoresLista> CprProveedoresLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CprProveedoresLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CprProveedoresDto> CprProveedores_Obtener(int CodEmpresa, string codigo)
        {
            return _db.CprProveedores_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto CprProveedores_Guardar(int CodEmpresa, bool vEdita, CprProveedoresDto proveedor)
        {
            return _db.CprProveedores_Guardar(CodEmpresa, vEdita, proveedor);
        }

        public ErrorDto CprProveedores_Eliminar(int CodEmpresa, string codigo)
        {
            return _db.CprProveedores_Eliminar(CodEmpresa, codigo);
        }

        public ErrorDto<float> CprProveedorPuntaje_Obtener(int CodEmpresa, string codigo)
        {
            return _db.CprProveedorPuntaje_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<CprProveedorBitacoraData>> CprProveedoreBitacoraPuntaje(int CodEmpresa, string codigo)
        {
            return _db.CprProveedoreBitacoraPuntaje(CodEmpresa, codigo);
        }
    }
}