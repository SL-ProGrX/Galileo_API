using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.General;
using Galileo_API.Models.ProGrX.General;

namespace Galileo_API.BusinessLogic.ProGrX.General
{
    public sealed class FrmCcEstadoCuentaMailBl
    {
        private readonly FrmCcEstadoCuentaMailDb _db;

        public FrmCcEstadoCuentaMailBl(
            IConfiguration config)
        {
            _db = new FrmCcEstadoCuentaMailDb(config);
        }

        public ErrorDto<CcEstadoCuentaMailInicialData>
            CC_Estado_Cuenta_Mail_Inicializar(
                int codEmpresa,
                string cedula)
        {
            return _db.CC_Estado_Cuenta_Mail_Inicializar(
                codEmpresa,
                cedula);
        }

        public ErrorDto CC_Estado_Cuenta_Mail_Enviar(
            int codEmpresa,
            CcEstadoCuentaMailEnviarRequest? request)
        {
            return _db.CC_Estado_Cuenta_Mail_Enviar(
                codEmpresa,
                request);
        }
    }
}