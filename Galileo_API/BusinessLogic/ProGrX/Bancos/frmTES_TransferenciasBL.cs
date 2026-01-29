using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier;

namespace PgxAPI.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTransferenciasBL
    {
        private readonly FrmTesTransferenciasDB _transferenciasDB;

        public FrmTesTransferenciasBL(IConfiguration config)
        {
            _transferenciasDB = new FrmTesTransferenciasDB(config);
        }

        public ErrorDto TES_Transferencia_Aceptar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            return _transferenciasDB.TES_Transferencia_Aceptar(CodEmpresa, transferencia);
        }

        public ErrorDto TES_Transferencia_Reversar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            return _transferenciasDB.TES_Transferencia_Reversar(CodEmpresa,transferencia);
        }

    }
}
