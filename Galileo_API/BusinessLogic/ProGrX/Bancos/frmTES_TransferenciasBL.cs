using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTransferenciasBl
    {
        private readonly IConfiguration? _config;
        private FrmTesTransferenciasDb _db;

        public FrmTesTransferenciasBl(IConfiguration config)
        {
            _config = config;
            _db = new FrmTesTransferenciasDb(_config);
        }

        public ErrorDto TES_Transferencia_Aceptar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            return _db.TES_Transferencia_Aceptar(CodEmpresa, transferencia);
        }

        public ErrorDto TES_Transferencia_Reversar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            return _db.TES_Transferencia_Reversar(CodEmpresa, transferencia);
        }
    }
}
