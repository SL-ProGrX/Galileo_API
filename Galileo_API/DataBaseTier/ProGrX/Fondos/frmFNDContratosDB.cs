using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MFndFuncionesDb _mFNDFunciones;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;

        private string pCuponFrecuencia = "";
        private string pCuponPaga = "";
        private string pCuponFrecuenciaId = "";

        public FrmFndContratosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
            _mFNDFunciones = new MFndFuncionesDb(_config);
            _mProGrxMain = new MProGrxMain(_config);
            _AuxiliarDB = new MProGrXAuxiliarDB(_config);
        }
    }
}