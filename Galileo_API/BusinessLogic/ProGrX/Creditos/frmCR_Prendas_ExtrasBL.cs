using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPrendasExtrasBl
    {
        private readonly FrmCrPrendasExtrasDb _db;

        public FrmCrPrendasExtrasBl(IConfiguration config)
        {
            _db = new FrmCrPrendasExtrasDb(config);
        }

        public ErrorDto<CrPrendasExtrasConsultaData> CR_Prendas_Extras_Consulta(int codEmpresa, long prendaId)
            => _db.CR_Prendas_Extras_Consulta(codEmpresa, prendaId);

        public ErrorDto<CrPrendasExtrasGuardarData> CR_Prendas_Extras_Guardar(
            int codEmpresa,
            CrPrendasExtrasGuardarRequest request)
            => _db.CR_Prendas_Extras_Guardar(codEmpresa, request);
    }
}
