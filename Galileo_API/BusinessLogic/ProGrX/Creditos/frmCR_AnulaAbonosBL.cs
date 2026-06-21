using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAnulaAbonosBL
    {
        private readonly FrmCrAnulaAbonosDB _db;

        public FrmCrAnulaAbonosBL(IConfiguration config)
        {
            _db = new FrmCrAnulaAbonosDB(config);
        }

        public ErrorDto<CrAnulaAbonosConsultaResponse> CR_AnulaAbonos_ConsultarOperacion(int codEmpresa, int idSolicitud)
        {
            return _db.CR_AnulaAbonos_ConsultarOperacion(codEmpresa, idSolicitud);
        }

        public ErrorDto<string> CR_AnulaAbonos_CuentaRecomendada(int codEmpresa, CrAnulaAbonosCuentaRecomendadaRequest request)
        {
            return _db.CR_AnulaAbonos_CuentaRecomendada(codEmpresa, request);
        }

        public ErrorDto<CrAnulaAbonosProcesarResponse> CR_AnulaAbonos_Procesar(int codEmpresa, CrAnulaAbonosProcesarRequest request)
        {
            return _db.CR_AnulaAbonos_Procesar(codEmpresa, request);
        }
    }
}
