using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRepMovTipoCuentaBl
    {
        private readonly FrmCntXRepMovTipoCuentaDb _db;

        public FrmCntXRepMovTipoCuentaBl(
            IConfiguration config)
        {
            _db = new FrmCntXRepMovTipoCuentaDb(config);
        }

        public ErrorDto<CntXRepMovTipoCuentaInicializarResponse>
            CntX_frmCntX_RepMovTipoCuenta_Inicializar(
                int codEmpresa,
                int codContabilidad)
        {
            return _db
                .CntX_frmCntX_RepMovTipoCuenta_Inicializar(
                    codEmpresa,
                    codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? unidad)
        {
            return _db
                .CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener(
                    codEmpresa,
                    codContabilidad,
                    unidad);
        }

        public ErrorDto<CntXRepMovTipoCuentaData?>
            CntX_frmCntX_RepMovTipoCuenta_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            return _db
                .CntX_frmCntX_RepMovTipoCuenta_Cuenta_Obtener(
                    codEmpresa,
                    codContabilidad,
                    cuenta);
        }

        public ErrorDto
            CntX_frmCntX_RepMovTipoCuenta_Reporte_Preparar(
                int codEmpresa,
                CntXRepMovTipoCuentaPrepararRequest request)
        {
            return _db
                .CntX_frmCntX_RepMovTipoCuenta_Reporte_Preparar(
                    codEmpresa,
                    request);
        }
    }
}