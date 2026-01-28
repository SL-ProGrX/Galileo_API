using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXTiposCuentasBl
    {
        private readonly FrmCntXTiposCuentasDb _db;

        public FrmCntXTiposCuentasBl(IConfiguration config) => _db = new FrmCntXTiposCuentasDb(config);

        public ErrorDto<List<CntXTiposCuentasData>> CntXTiposCuentas_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXTiposCuentas_Obtener(codEmpresa, codConta);
        }

        public ErrorDto CntXTiposCuentas_Guardar(int codEmpresa, int codConta, string usuario, CntXTiposCuentasData request)
        {
            return _db.CntXTiposCuentas_Guardar(codEmpresa, codConta, usuario, request);
        }

        public ErrorDto CntXTiposCuentas_Eliminar(int codEmpresa, int codConta, string usuario, string tipoCuenta)
        {
            return _db.CntXTiposCuentas_Eliminar(codEmpresa, codConta, usuario, tipoCuenta);
        }
    }
}
