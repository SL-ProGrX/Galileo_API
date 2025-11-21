using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class MBeneficiosBl
    {
        readonly MBeneficiosDB mBeneficioDB;

        public MBeneficiosBl(IConfiguration config)
        {
            mBeneficioDB = new MBeneficiosDB(config);
        }

        public ErrorDto fxNombre(int CodEmpresa, string cedula)
        {
            return mBeneficioDB.fxNombre(CodEmpresa, cedula);
        }

        public ErrorDto fxDescribeBanco(int CodEmpresa, int codBanco)
        {
            return mBeneficioDB.fxDescribeBanco(CodEmpresa, codBanco);
        }
    }
}
