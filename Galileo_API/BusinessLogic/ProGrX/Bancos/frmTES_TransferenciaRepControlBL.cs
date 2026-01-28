using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTransferenciaRepControlBL
    {
        private readonly FrmTesTransferenciaRepControlDB _transferenciaRepControlDB;
        private readonly MTesoreria mTesoreria;

        public FrmTesTransferenciaRepControlBL(IConfiguration config)
        {
            _transferenciaRepControlDB = new FrmTesTransferenciaRepControlDB(config);
            mTesoreria = new MTesoreria(config);
        }

        public ErrorDto<TransferenciaRepControlCatalogoDto> TES_TransferenciaRepControl_Catalogos_Obtener(int CodEmpresa, int Banco)
        {
            return _transferenciaRepControlDB.TES_TransferenciaRepControl_Catalogos_Obtener(CodEmpresa, Banco);
        }

        public ErrorDto<long> TES_TransferenciaRepControl_NTran_Obtener(int CodEmpresa, int Banco, string TipoDoc, string Plan)
        {
            ErrorDto<long> txtNTran = mTesoreria.fxTesTipoDocConsec(CodEmpresa, Banco, TipoDoc, "/", Plan);
            return txtNTran;
        }

        public ErrorDto<TesReporteTransferenciaDto> TES_TransferenciaRepControl_Carta_Obtener(int CodEmpresa, int Banco, long NTransac, string TipoDoc, string Plan)
        {
            return _transferenciaRepControlDB.sbTesReporteTransferencia(CodEmpresa, Banco, NTransac, TipoDoc, Plan);
        }

        public ErrorDto<object> TES_TransferenciaRepControl_Archivo_Generar(int CodEmpresa, int Banco, int NTransac, string TipoDoc, string Formato, string Plan)
        {
            return _transferenciaRepControlDB.TES_TransferenciaRepControl_Archivo_Generar(CodEmpresa, Banco, NTransac, TipoDoc, Formato, Plan);
        }
    }
}
