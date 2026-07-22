using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRepBalanceSituacionBl
    {
        private readonly FrmCntXRepBalanceSituacionDb _db;

        public FrmCntXRepBalanceSituacionBl(
            IConfiguration config)
        {
            _db = new FrmCntXRepBalanceSituacionDb(config);
        }

        public ErrorDto<CntXRepBalanceSituacionInicializarResponse>
            CntX_frmCntX_RepBalanceSituacion_Inicializar(
                int codEmpresa,
                int codContabilidad)
        {
            return _db
                .CntX_frmCntX_RepBalanceSituacion_Inicializar(
                    codEmpresa,
                    codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? unidad)
        {
            return _db
                .CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener(
                    codEmpresa,
                    codContabilidad,
                    unidad);
        }

        public ErrorDto<CntXRepBalanceSituacionCuentaData?>
            CntX_frmCntX_RepBalanceSituacion_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            return _db
                .CntX_frmCntX_RepBalanceSituacion_Cuenta_Obtener(
                    codEmpresa,
                    codContabilidad,
                    cuenta);
        }

        public ErrorDto<CntXRepBalanceSituacionPrepararResponse>
            CntX_frmCntX_RepBalanceSituacion_Reporte_Preparar(
                int codEmpresa,
                CntXRepBalanceSituacionPrepararRequest request)
        {
            return _db
                .CntX_frmCntX_RepBalanceSituacion_Reporte_Preparar(
                    codEmpresa,
                    request);
        }
    }
}