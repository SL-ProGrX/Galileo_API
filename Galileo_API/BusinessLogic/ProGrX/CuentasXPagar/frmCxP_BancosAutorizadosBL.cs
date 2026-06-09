using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPBancosAutorizadosBL
    {
        private readonly FrmCxPBancosAutorizadosDB _db;

        public FrmCxPBancosAutorizadosBL(IConfiguration config)
        {
            _db = new FrmCxPBancosAutorizadosDB(config);
        }

        public ErrorDto<List<BancosAutorizadosDto>> ObtenerBancosAutorizados(int CodCliente)
        {
            return _db.ObtenerBancosAutorizados(CodCliente);
        }

        public ErrorDto IngresarTesBancosNuevos(string Usuario, int CodCliente)
        {
            return _db.IngresarTesBancosNuevos(Usuario, CodCliente);
        }

        public ErrorDto ActualizarCheque(int BancoId, bool Valor, int CodCliente)
        {
            return _db.ActualizarCheque(BancoId, Valor, CodCliente);
        }

        public ErrorDto ActualizarTransferencia(int BancoId, bool Valor, int CodCliente)
        {
            return _db.ActualizarTransferencia(BancoId, Valor, CodCliente);
        }
    }
}