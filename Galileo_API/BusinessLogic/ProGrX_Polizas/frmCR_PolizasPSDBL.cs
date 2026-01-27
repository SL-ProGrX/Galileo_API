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

        public ErrorDto<List<CajasUserDto>> Cajas_Usuario_Obtener(int codEmpresa, string usuario)
        {
            return DbfrmCR_PolizasPSDDb.Cajas_Usuario_Obtener(codEmpresa, usuario
            );
        }

    }
}