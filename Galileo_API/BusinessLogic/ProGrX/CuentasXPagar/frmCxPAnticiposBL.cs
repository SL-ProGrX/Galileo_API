using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPAnticiposBL
    {
        private readonly FrmCxPAnticiposDB _db;

        public FrmCxPAnticiposBL(IConfiguration config)
        {
            _db = new FrmCxPAnticiposDB(config);
        }

        public ErrorDto ExeAnticipos(int CodCliente, string filtros)
        {
            return _db.ExeAnticipos(CodCliente, filtros);
        }

        public ErrorDto<List<CargoDto>> ObtenerCargos(int CodCliente)
        {
            return _db.ObtenerCargos(CodCliente);
        }

        public ErrorDto<List<AdelantoRegistradoDto>> ObtenerAdelantosRegistrados(int CodCliente, int Proveedor)
        {
            return _db.ObtenerAdelantosRegistrados(CodCliente, Proveedor);
        }

        public ErrorDto<List<HistorialPagoDto>> ObtenerHistorialDePagos(int CodCliente, int Proveedor, string Anticipos)
        {
            return _db.ObtenerHistorialDePagos(CodCliente, Proveedor, Anticipos);
        }

        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodEmpresa)
        {
            return _db.ObtenerProveedores(CodEmpresa);
        }

        public ErrorDto ConsecutivoAdelanto(int CodEmpresa, int Proveedor)
        {
            return _db.ConsecutivoAdelanto(CodEmpresa, Proveedor);
        }
    }//end class
}//end namespace