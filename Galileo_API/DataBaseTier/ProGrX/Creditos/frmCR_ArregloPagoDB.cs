using Galileo.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;
        private readonly MCajas _mCajas;
        private readonly MAfilicacionDB _mAfilicacion;
        private readonly MSecurityMainDb _securityMainDb;

        private const int VModulo = 3;
        private const string usuarioInvalido = "Debe indicar el usuario.";
        private const string paramGlobalesNulos = "No fue posible obtener los parametros globales.";

        private const string LineaSaldoAnterior = "Saldo Anterior";
        private const string LineaSaldoActual = "Saldo Actual";
        private const string LineaInteresCorriente = "Interes Corriente";
        private const string LineaInteresAtrasado = "Interes Atrasado";
        private const string LineaCargosTotales = "Cargos Totales";
        private const string LineaPolizas = "Polizas";
        private const string LineaAmortizacion = "Amortizacion";
        private const string LineaCapitalizacion = "Capitalizacion";

        public FrmCrArregloPagoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mRecibos = new MRecibos(config);
            _mCajas = new MCajas(config);
            _mAfilicacion = new MAfilicacionDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
    }
}
