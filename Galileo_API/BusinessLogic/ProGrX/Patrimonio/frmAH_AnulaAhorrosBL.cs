using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHAnulaAhorrosBL
    {
        private readonly FrmAHAnulaAhorrosDB _db;

        public FrmAHAnulaAhorrosBL(IConfiguration config)
        {
            _db = new FrmAHAnulaAhorrosDB(config);
        }

        public ErrorDto<FrmAhAnulaAhorrosConsultaResponse?> Patrimonio_frmAH_AnulaAhorros_Consulta_Obtener(int codEmpresa, string cedula)
            => _db.Patrimonio_frmAH_AnulaAhorros_Consulta_Obtener(codEmpresa, cedula);

        public ErrorDto<List<FrmAhAnulaAhorrosMovimientoResponse>> Patrimonio_frmAH_AnulaAhorros_Movimientos_Obtener(int codEmpresa, string cedula, string tipoRubro)
            => _db.Patrimonio_frmAH_AnulaAhorros_Movimientos_Obtener(codEmpresa, cedula, tipoRubro);

        public ErrorDto<FrmAhAnulaAhorrosProcesarResponse> Patrimonio_frmAH_AnulaAhorros_Procesar(int codEmpresa, FrmAhAnulaAhorrosProcesarRequest request)
            => _db.Patrimonio_frmAH_AnulaAhorros_Procesar(codEmpresa, request);
    }
}
