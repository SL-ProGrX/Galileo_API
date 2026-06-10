namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrComitesParametrosBL
    {
        private readonly FrmCrComitesParametrosDB _db;

        public FrmCrComitesParametrosBL(IConfiguration config)
        {
            _db = new FrmCrComitesParametrosDB(config);
        }

        public ErrorDto<List<CrComitesParametroModel>> CrComitesParametros_Obtener(int CodEmpresa)
        {
            return _db.CrComitesParametros_Obtener(CodEmpresa);
        }

        public ErrorDto CrComitesParametros_Actualizar(int CodEmpresa, CrComitesParametroActualizarRequest request)
        {
            return _db.CrComitesParametros_Actualizar(CodEmpresa, request);
        }
    }
}
