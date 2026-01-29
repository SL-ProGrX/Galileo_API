using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Galileo_API.Models.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesConsultaCuentaSinpeBL
    {
        private readonly FrmTesConsultaCuentaSinpeDB _db;

        public FrmTesConsultaCuentaSinpeBL(IConfiguration config)
        {
            _db = new FrmTesConsultaCuentaSinpeDB(config);
        }

        public ErrorDto Tes_ConsultaCuentasSinpe_Aplicar(int CodEmpresa, int aplica, TesConsultaCuentaSinpeModels cuenta)
        {
            return _db.Tes_ConsultaCuentasSinpe_Aplicar(CodEmpresa, aplica, cuenta);
        }
    }
}
