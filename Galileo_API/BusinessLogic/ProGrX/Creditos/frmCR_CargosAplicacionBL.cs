using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCargosAplicacionBl
    {
        private readonly FrmCrCargosAplicacionDb _db;

        public FrmCrCargosAplicacionBl(IConfiguration config)
        {
            _db = new FrmCrCargosAplicacionDb(config);
        }

        public ErrorDto<List<CrCargosAplicacionCargoData>> Cr_CargosAplicacion_Cargos_Obtener(int codEmpresa)
            => _db.Cr_CargosAplicacion_Cargos_Obtener(codEmpresa);

        public ErrorDto<CrCargosAplicacionOperacionData?> Cr_CargosAplicacion_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _db.Cr_CargosAplicacion_Operacion_Obtener(codEmpresa, operacion);

        public ErrorDto Cr_CargosAplicacion_Aplicar(
            int codEmpresa,
            CrCargosAplicacionAplicarRequest request)
            => _db.Cr_CargosAplicacion_Aplicar(codEmpresa, request);
    }
}