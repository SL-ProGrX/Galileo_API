using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public sealed class FrmPreaEstudiov2ScoreCrediticioBL
    {
        private readonly FrmPreaEstudiov2ScoreCrediticioDB _db;

        public FrmPreaEstudiov2ScoreCrediticioBL(IConfiguration config)
        {
            _db = new FrmPreaEstudiov2ScoreCrediticioDB(config);
        }

        public ErrorDto<PreaScoreCrediticioHistorialDto> Consultar(
            int codEmpresa, string codPreanalisis, string cedula)
        {
            if (codEmpresa <= 0 || string.IsNullOrWhiteSpace(codPreanalisis)
                || codPreanalisis.Trim().Length > 20 || string.IsNullOrWhiteSpace(cedula))
            {
                return new ErrorDto<PreaScoreCrediticioHistorialDto>
                {
                    Code = -1,
                    Description = "Debe indicar la empresa, el expediente y la identificación válidos.",
                    Result = new PreaScoreCrediticioHistorialDto()
                };
            }

            return _db.Consultar(codEmpresa, codPreanalisis.Trim(), cedula.Trim());
        }
    }
}
