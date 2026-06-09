using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPCargosAdicionalesBL
    {
        private readonly FrmCxPCargosAdicionalesDB _db;

        public FrmCxPCargosAdicionalesBL(IConfiguration config)
        {
            _db = new FrmCxPCargosAdicionalesDB(config);
        }

        public ErrorDto<List<CargosAdicionalDto>> ObtenerCargosAdicionales(int CodEmpresa)
        {
            return _db.ObtenerCargosAdicionales(CodEmpresa);
        }

        public ErrorDto ExisteCargoAdicional(int CodEmpresa, string CodCargo)
        {
            return _db.ExisteCargoAdicional(CodEmpresa, CodCargo);
        }

        public ErrorDto EliminarCargoAdicional(int CodEmpresa, string CodCargo)
        {
            return _db.EliminarCargoAdicional(CodEmpresa, CodCargo);
        }

        public ErrorDto InsertarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            return _db.InsertarCargoAdicional(CodEmpresa, Info);
        }

        public ErrorDto ActualizarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            return _db.ActualizarCargoAdicional(CodEmpresa, Info);
        }
    }
}