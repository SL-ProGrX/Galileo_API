namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrCatalogoExcBL
    {
        private readonly FrmCrCatalogoExcDB _db;

        public FrmCrCatalogoExcBL(IConfiguration config)
        {
            _db = new FrmCrCatalogoExcDB(config);
        }

        public ErrorDto<List<CrCatalogoExcDisponibleModel>> CrCatalogoExc_Disponible_Obtener(int CodEmpresa)
        {
            return _db.CrCatalogoExc_Disponible_Obtener(CodEmpresa);
        }

        public ErrorDto CrCatalogoExc_Disponible_Guardar(int CodEmpresa, CrCatalogoExcDisponibleGuardarRequest request)
        {
            return _db.CrCatalogoExc_Disponible_Guardar(CodEmpresa, request);
        }

        public ErrorDto CrCatalogoExc_Disponible_Eliminar(int CodEmpresa, CrCatalogoExcDisponibleEliminarRequest request)
        {
            return _db.CrCatalogoExc_Disponible_Eliminar(CodEmpresa, request);
        }
    }
}
