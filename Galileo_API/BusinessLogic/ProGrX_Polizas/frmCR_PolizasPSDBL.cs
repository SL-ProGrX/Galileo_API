using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCRPolizasPsdBl(FrmCRPolizasPsdDb dbfrmCR_PolizasPSD)
    {
        private readonly FrmCRPolizasPsdDb DbfrmCR_PolizasPSDDb = dbfrmCR_PolizasPSD;

        public FrmCRPolizasPsdBl(IConfiguration config)
            : this(new FrmCRPolizasPsdDb(config))
        {
        }

        public ErrorDto<List<PolizaPsdDto>> Poliza_PSD_Consulta(int codEmpresa,DateTime fechaCorte, string usuario,string tipo)
        {
            return DbfrmCR_PolizasPSDDb.Poliza_PSD_Consulta(codEmpresa,fechaCorte,usuario,tipo);
        }

        public ErrorDto<bool> Poliza_PSD_Genera(int codEmpresa,DateTime fechaCorte,string usuario)
        {
            return DbfrmCR_PolizasPSDDb.Poliza_PSD_Genera(codEmpresa,fechaCorte,usuario);
        }

    }
}