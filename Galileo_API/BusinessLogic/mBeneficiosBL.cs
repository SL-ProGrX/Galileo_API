using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class mBeneficiosBL
    {
        private readonly IConfiguration _config;
        MBeneficiosDB mBeneficioDB;

        public mBeneficiosBL(IConfiguration config)
        {
            _config = config;
            mBeneficioDB = new MBeneficiosDB(_config);
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
