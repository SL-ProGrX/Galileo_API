using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrDesemConceptosBl
    {
        private readonly FrmCrDesemConceptosDb _db;

        public FrmCrDesemConceptosBl(IConfiguration config)
            => _db = new FrmCrDesemConceptosDb(config);

        public ErrorDto<List<CrConceptoDesembData>> CrDesembConceptos_Obtener(int codEmpresa)
        {
            return _db.CrDesembConceptos_Obtener(codEmpresa);
        }

        public ErrorDto CrDesembConcepto_Guardar(int codEmpresa, string usuario, int codConta, CrConceptoDesembData request)
        {
            return _db.CrDesembConcepto_Guardar(codEmpresa, usuario, codConta, request);
        }

        public ErrorDto CrDesembConcepto_Eliminar(int codEmpresa, int codCondeb, string usuario)
        {
            return _db.CrDesembConcepto_Eliminar(codEmpresa, codCondeb, usuario);
        }
    }
}
