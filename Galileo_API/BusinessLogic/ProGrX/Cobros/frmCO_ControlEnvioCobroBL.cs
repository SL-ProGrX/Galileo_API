using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogicTier.ProGrX.Cobros
{
    public sealed class FrmCOControlEnvioCobroBL
    {
        private readonly FrmCOControlEnvioCobroDB _db;

        public FrmCOControlEnvioCobroBL(IConfiguration config)
        {
            _db = new FrmCOControlEnvioCobroDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            Co_ControlEnvioCobro_Gestiones_Obtener(int codEmpresa)
        {
            return _db.Co_ControlEnvioCobro_Gestiones_Obtener(codEmpresa);
        }

        public ErrorDto<List<CoControlEnvioCobroPendienteData>>
            Co_ControlEnvioCobro_Pendientes_Obtener(
                int codEmpresa,
                bool todos,
                string? codGestion)
        {
            return _db.Co_ControlEnvioCobro_Pendientes_Obtener(
                codEmpresa,
                todos,
                codGestion);
        }
    }
}