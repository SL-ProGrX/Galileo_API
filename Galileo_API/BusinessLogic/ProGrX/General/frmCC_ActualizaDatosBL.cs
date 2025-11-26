using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCcActualizaDatosBl
    {
        private readonly FrmCcActualizaDatosDb DbfrmCC_ActualizaDatos;

        public FrmCcActualizaDatosBl(IConfiguration config)
        {
            DbfrmCC_ActualizaDatos = new FrmCcActualizaDatosDb(config);
        }

        public ErrorDto CC_ActualizaDatos_SP(int CodEmpresa)
        {
            return DbfrmCC_ActualizaDatos.CC_ActualizaDatos_SP(CodEmpresa);
        }
    }
}