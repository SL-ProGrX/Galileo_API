using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhRegistraAhorroBL
    {
        private readonly FrmAhRegistraAhorroDB _db;

        public FrmAhRegistraAhorroBL(IConfiguration config)
        {
            _db = new FrmAhRegistraAhorroDB(config);
        }

        public ErrorDto<FrmAhRegistraAhorroCargarResponse> AH_RegistraAhorro_Cargar(
            int codEmpresa,
            FrmAhRegistraAhorroCargarRequest request)
            => _db.AH_RegistraAhorro_Cargar(codEmpresa, request);

        public ErrorDto<FrmAhRegistraAhorroGestionResponse> AH_RegistraAhorro_Gestion_Registrar(
            int codEmpresa,
            FrmAhRegistraAhorroGestionRegistrarRequest request)
            => _db.AH_RegistraAhorro_Gestion_Registrar(codEmpresa, request);

        public ErrorDto<FrmAhRegistraAhorroGestionResponse> AH_RegistraAhorro_Gestion_Estado(
            int codEmpresa,
            int gestionId)
            => _db.AH_RegistraAhorro_Gestion_Estado(codEmpresa, gestionId);

        public ErrorDto<FrmAhRegistraAhorroAplicarResponse> AH_RegistraAhorro_Aplicar(
            int codEmpresa,
            FrmAhRegistraAhorroAplicarRequest request)
            => _db.AH_RegistraAhorro_Aplicar(codEmpresa, request);
    }
}
