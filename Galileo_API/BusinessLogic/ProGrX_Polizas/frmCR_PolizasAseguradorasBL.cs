using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Galileo_API.DataBaseTier.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCRPolizasAseguradorasBl(FrmCRPolizasAseguradorasDb dbfrmCR_PolizasAseguradoras)
    {
        private readonly FrmCRPolizasAseguradorasDb DbfrmCR_PolizasAseguradorasDb = dbfrmCR_PolizasAseguradoras;

        public FrmCRPolizasAseguradorasBl(IConfiguration config)
            : this(new FrmCRPolizasAseguradorasDb(config))
        {
        }

        public ErrorDto<List<PolizaAseguradoraDto>> Poliza_PSD_Consulta(int codEmpresa,DateTime fechaCorte, string usuario,string tipo)
        {
            return DbfrmCR_PolizasAseguradorasDb.Poliza_PSD_Consulta(codEmpresa,fechaCorte,usuario,tipo);
        }


    }
}