using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresModeloCuentasBl
    {
        readonly FrmPresModeloCuentasDb _db;

        public FrmPresModeloCuentasBl(IConfiguration config)
        {
            _db = new FrmPresModeloCuentasDb(config);
        }

        public ErrorDto<List<CuentasCatalogoData>> spPres_CuentasCatalogo_Obtener(int CodEmpresa, int CodContab, string CodModelo, string CodUnidad, string CodCentroCosto)
        {
            return _db.spPres_CuentasCatalogo_Obtener(CodEmpresa, CodContab, CodModelo, CodUnidad, CodCentroCosto);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _db.Pres_Modelos_Obtener(CodEmpresa, CodContab, Usuario);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Unidades_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _db.Pres_Unidades_Obtener(CodEmpresa, CodContab, Usuario);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_CentroCosto_Obtener(int CodEmpresa, int CodContab, string CodUnidad)
        {
            return _db.Pres_CentroCosto_Obtener(CodEmpresa, CodContab, CodUnidad);
        }

        public ErrorDto spPres_Modelo_Cuentas_CargaDatos(int CodEmpresa, List<PresModeloCuentasImportData> request)
        {
            return _db.spPres_Modelo_Cuentas_CargaDatos(CodEmpresa, request);
        }

        public ErrorDto<List<PresModeloCuentasImportData>> spPres_Modelo_Cuentas_RevisaImport(int CodEmpresa, int CodContab, string CodModelo, string Usuario)
        {
            return _db.spPres_Modelo_Cuentas_RevisaImport(CodEmpresa, CodContab, CodModelo, Usuario);
        }

        public ErrorDto spPres_Modelo_Cuentas_Import(int CodEmpresa, int CodContab, string CodModelo, string Usuario)
        {
            return _db.spPres_Modelo_Cuentas_Import(CodEmpresa, CodContab, CodModelo, Usuario);
        }

        public ErrorDto<List<PresModeloCuentasImportData>> spCntX_Periodo_Fiscal_Meses(int CodEmpresa, int CodContab, string CodModelo, string Usuario, List<PresModeloCuentasHorizontal> request)
        {
            return _db.spCntX_Periodo_Fiscal_Meses(CodEmpresa, CodContab, CodModelo, Usuario, request);
        }
    }
}