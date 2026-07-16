using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCtaCatalogoBl
    {
        private readonly FrmCrCtaCatalogoDb _db;

        public FrmCrCtaCatalogoBl(IConfiguration config)
        {
            _db = new FrmCrCtaCatalogoDb(config);
        }

        public ErrorDto<CrCtaCatalogoCuenta?> CrCtaCatalogo_Cuentas_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCtaCatalogo_Cuentas_Obtener(codEmpresa, codigo);
        }

        public ErrorDto CrCtaCatalogo_Cuentas_Guardar(int codEmpresa, CrCtaCatalogoCuentasGuardarRequest request)
        {
            return _db.CrCtaCatalogo_Cuentas_Guardar(codEmpresa, request);
        }
    }
}
