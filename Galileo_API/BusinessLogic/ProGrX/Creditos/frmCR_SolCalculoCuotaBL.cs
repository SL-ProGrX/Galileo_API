using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSolCalculoCuotaBl
    {
        private readonly FrmCrSolCalculoCuotaDb _db;

        public FrmCrSolCalculoCuotaBl(IConfiguration config)
        {
            _db = new FrmCrSolCalculoCuotaDb(config);
        }

        public ErrorDto<CrSolCalculoCuotaPantallaData> CrSolCalculoCuota_Pantalla_Obtener(int codEmpresa)
            => _db.CrSolCalculoCuota_Pantalla_Obtener(codEmpresa);

        public ErrorDto<CrSolCalculoCuotaCalcularCuotaData> CrSolCalculoCuota_Cuota_Calcular(
            int codEmpresa,
            CrSolCalculoCuotaCalcularCuotaRequest request)
            => _db.CrSolCalculoCuota_Cuota_Calcular(codEmpresa, request);

        public ErrorDto<CrSolCalculoCuotaNiveladaData> CrSolCalculoCuota_Nivelada_Calcular(
            int codEmpresa,
            CrSolCalculoCuotaNiveladaRequest request)
            => _db.CrSolCalculoCuota_Nivelada_Calcular(codEmpresa, request);

        public ErrorDto<CrSolCalculoCuotaDiasMesData> CrSolCalculoCuota_DiasMes_Obtener(
            int codEmpresa,
            CrSolCalculoCuotaDiasMesRequest request)
            => _db.CrSolCalculoCuota_DiasMes_Obtener(codEmpresa, request);
    }
}