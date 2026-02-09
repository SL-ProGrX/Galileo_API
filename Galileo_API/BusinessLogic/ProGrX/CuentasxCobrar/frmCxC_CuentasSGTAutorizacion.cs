 
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCCuentasSgtAutorizacionModels;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtAutorizacionBL
    {

        private readonly FrmCxCCuentasSgtAutorizacionDb _db;

        public FrmCxCCuentasSgtAutorizacionBL(IConfiguration config) => _db = new FrmCxCCuentasSgtAutorizacionDb(config);

        public ErrorDto<CuentasSgtAutorizacionDto?> CxCCuentasSGTAutorizacion_Consulta(int codEmpresa, int operacion)
        {
             
            return _db.CxCCuentasSGTAutorizacion_Consulta(codEmpresa, operacion);
        }

        public ErrorDto CxCCuentasSGTAutorizacion_Actualizar(int codEmpresa, string usuario, string estado, int operacion, string notas)
        {
            return _db.CxCCuentasSGTAutorizacion_Actualizar(codEmpresa, usuario, estado, operacion, notas);
        }
    }
}
