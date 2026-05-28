using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhConfiguracionBL
    {
        private readonly FrmAhConfiguracionDB _db;

        public FrmAhConfiguracionBL(IConfiguration config)
        {
            _db = new FrmAhConfiguracionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AH_Configuracion_Divisas_Obtener(int codEmpresa)
            => _db.AH_Configuracion_Divisas_Obtener(codEmpresa);

        public ErrorDto<ParametrosPatrimonioDto> AH_Configuracion_Parametros_Obtener(int codEmpresa, string codDivisa)
            => _db.AH_Configuracion_Parametros_Obtener(codEmpresa, codDivisa);

        public ErrorDto AH_Configuracion_Parametros_Guardar(int codEmpresa, AhConfiguracionGuardarRequest request, string usuario)
            => _db.AH_Configuracion_Parametros_Guardar(codEmpresa, request, usuario);

        public ErrorDto<AhConfiguracionCuentaValidarResponse> AH_Configuracion_Cuenta_Validar(int codEmpresa, string cuenta,int contabilidad)
            => _db.AH_Configuracion_Cuenta_Validar(codEmpresa, cuenta, contabilidad);
    }
}
