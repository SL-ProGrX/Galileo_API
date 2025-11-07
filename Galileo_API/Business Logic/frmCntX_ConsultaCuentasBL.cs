using PgxAPI.DataBaseTier;
using PgxAPI.Models;

namespace PgxAPI.BusinessLogic
{
    public class frmCntX_ConsultaCuentasBL
    {
        private readonly IConfiguration _config;
        FrmCntXConsultaCuentasDb DbProveedores;

        public frmCntX_ConsultaCuentasBL(IConfiguration config)
        {
            _config = config;
            DbProveedores = new FrmCntXConsultaCuentasDb(_config);
        }

        public List<CtnxCuentasDto> ObtenerCuentas(int CodEmpresa, CuentaVarModel cuenta)
        {
            return DbProveedores.ObtenerCuentas(CodEmpresa, cuenta);
        }
        public List<CtnxCuentasArbolModel> ObtenerCuentasArbol(int CodEmpresa, CuentaVarModel cuenta)
        {
            return DbProveedores.ObtenerCuentasArbol(CodEmpresa, cuenta);
        }


        public List<DropDownListaGenericaModel> ObtenerDivisas(int CodEmpresa, int Contavilidad)
        {
            return DbProveedores.ObtenerDivisas(CodEmpresa, Contavilidad);
        }
        public List<DropDownListaGenericaModel> ObtenerTiposCuentas(int CodEmpresa, int Contavilidad)
        {
            return DbProveedores.ObtenerTiposCuentas(CodEmpresa, Contavilidad);
        }


    }
}
