using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic
{
    public class FrmCcActualizaDatosBl
    {
        private readonly FrmCcActualizaDatosDb _db;

        public FrmCcActualizaDatosBl(IConfiguration config)
        {
            _db = new FrmCcActualizaDatosDb(config);
        }

        public ErrorDto CC_ActualizaDatos_Proceso_Ejecutar(int CodEmpresa)
        {
            return _db.CC_ActualizaDatos_Proceso_Ejecutar(CodEmpresa);
        }
    }
}