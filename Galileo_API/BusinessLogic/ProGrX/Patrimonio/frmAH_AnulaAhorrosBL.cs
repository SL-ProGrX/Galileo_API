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

        public ErrorDto<FrmAhAnulaAhorrosConsultaResponse?> Ah_AnulaAhorros_Consulta_Obtener(int codEmpresa, string cedula)
            => _db.Ah_AnulaAhorros_Consulta_Obtener(codEmpresa, cedula);

        public ErrorDto<List<FrmAhAnulaAhorrosMovimientoResponse>> Ah_AnulaAhorros_Movimientos_Obtener(int codEmpresa, string cedula, string tipoRubro)
            => _db.Ah_AnulaAhorros_Movimientos_Obtener(codEmpresa, cedula, tipoRubro);

        public ErrorDto<FrmAhAnulaAhorrosProcesarResponse> Ah_AnulaAhorros_Procesar(int codEmpresa, FrmAhAnulaAhorrosProcesarRequest request)
            => _db.Ah_AnulaAhorros_Procesar(codEmpresa, request);
    }
}
