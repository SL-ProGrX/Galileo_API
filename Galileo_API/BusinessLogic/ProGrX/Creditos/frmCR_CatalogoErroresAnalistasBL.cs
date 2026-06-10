namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrCatalogoErroresAnalistasBL
    {
        private readonly FrmCrCatalogoErroresAnalistasDB _db;

        public FrmCrCatalogoErroresAnalistasBL(IConfiguration config)
        {
            _db = new FrmCrCatalogoErroresAnalistasDB(config);
        }

        public ErrorDto<List<CrCatalogoErroresAnalistasModel>> CrCatalogoErroresAnalistas_Obtener(int CodEmpresa)
        {
            return _db.CrCatalogoErroresAnalistas_Obtener(CodEmpresa);
        }

        public ErrorDto CrCatalogoErroresAnalistas_Guardar(int CodEmpresa, CrCatalogoErroresAnalistasGuardarRequest request)
        {
            return _db.CrCatalogoErroresAnalistas_Guardar(CodEmpresa, request);
        }

        public ErrorDto CrCatalogoErroresAnalistas_Eliminar(int CodEmpresa, CrCatalogoErroresAnalistasEliminarRequest request)
        {
            return _db.CrCatalogoErroresAnalistas_Eliminar(CodEmpresa, request);
        }
    }
}
