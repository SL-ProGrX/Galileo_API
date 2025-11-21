using Galileo.DataBaseTier;
using Galileo.Models;

namespace Galileo.BusinessLogic
{
    public class FrmCntXConsultaCuentasBl
    {
        readonly FrmCntXConsultaCuentasDb DbProveedores;

        public FrmCntXConsultaCuentasBl(IConfiguration config)
        {
            DbProveedores = new FrmCntXConsultaCuentasDb(config);
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