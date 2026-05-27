namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    using Galileo.Models.ERROR;
    using Galileo_API.Models.ProGrX_Contabilidad;
    using Galileo_API.DataBaseTier.ProGrX_Contabilidad;

    public class FrmCntXConCierreBL
    {
        private readonly FrmCntXConCierreDB _db;

        public FrmCntXConCierreBL(IConfiguration config)
        {
            _db = new FrmCntXConCierreDB(config);
        }

        public ErrorDto<FrmCntXConCierreLista> AF_CntXConCierre_Obtener(int codEmpresa)
        {
            return _db.AF_CntXConCierre_Obtener(codEmpresa);
        }

        public ErrorDto<FrmCntXConCierreDefinicionLista> AF_CntXConCierre_ObtenerDefinicion(int codEmpresa, int codConsolida)
        {
            return _db.AF_CntXConCierre_ObtenerDefinicion(codEmpresa, codConsolida);
        }        

        public ErrorDto AF_CntXConCierre_ValidaPeriodoBase(int codEmpresa, int mes, int anio, int codContabilidad)
        {
            return _db.AF_CntXConCierre_ValidaPeriodoBase(codEmpresa, mes, anio, codContabilidad);
        }

        public ErrorDto AF_CntXConCierre_ValidaPeriodoLocal(int codEmpresa, int mes, int anio, int codConsolida)
        {
            return _db.AF_CntXConCierre_ValidaPeriodoLocal(codEmpresa, mes, anio, codConsolida);
        }
        public ErrorDto<FrmCntXConCierrePortalLista> AF_CntXConCierre_ObtenerPortales(int codEmpresa, int codConsolida)
        {
            return _db.AF_CntXConCierre_ObtenerPortales(codEmpresa, codConsolida);
        }

        public ErrorDto AF_CntXConCierre_ValidaPeriodo(int codEmpresa, int mes, int anio, int codContabilidad, bool soloAbierto)
        {
            return _db.AF_CntXConCierre_ValidaPeriodo(codEmpresa, mes, anio, codContabilidad, soloAbierto);
        }

        public ErrorDto AF_CntXConCierre_InsertarPeriodo(int codEmpresa, int anio, int mes, int codContabilidad)
        {
            return _db.AF_CntXConCierre_InsertarPeriodo(codEmpresa, anio, mes, codContabilidad);
        }

        public ErrorDto AF_CntXConCierre_InsertarMovimientos(int codEmpresa, int codConsolida, int codContabilidad, int anio, int mes, int nivel)
        {
            return _db.AF_CntXConCierre_InsertarMovimientos(codEmpresa, codConsolida, codContabilidad, anio, mes, nivel);
        }

        public ErrorDto AF_CntXConCierre_ActualizarMovimiento(int codEmpresa, FrmCntXConCierreActualizarMovimientoRequest req)
        {
            return _db.AF_CntXConCierre_ActualizarMovimiento(codEmpresa, req);
        }

        public ErrorDto<FrmCntXConCierreCuentaMovLista> AF_CntXConCierre_ObtenerMovimientosPortal(int codEmpresa, int mes, int anio, int nivel, string contabilidades)
        {
            return _db.AF_CntXConCierre_ObtenerMovimientosPortal(codEmpresa, mes, anio, nivel, contabilidades);
        }

        public ErrorDto<FrmCntXConCierreContabilidadPortalLista> AF_CntXConCierre_ObtenerContabilidadesPortal(int codEmpresa, int codConsolida, int codPortal)
        {
            return _db.AF_CntXConCierre_ObtenerContabilidadesPortal(codEmpresa, codConsolida, codPortal);
        }

        public ErrorDto AF_CntXConCierre_ExisteMovimientoConsolidado(int codEmpresa, FrmCntXConCierreExisteMovimientoRequest req)
        {
            return _db.AF_CntXConCierre_ExisteMovimientoConsolidado(codEmpresa, req);
        }

        public ErrorDto AF_CntXConCierre_InsertarMovimiento(int codEmpresa, FrmCntXConCierreInsertarMovimientoRequest req)
        {
            return _db.AF_CntXConCierre_InsertarMovimiento(codEmpresa, req);
        }
    }
}
