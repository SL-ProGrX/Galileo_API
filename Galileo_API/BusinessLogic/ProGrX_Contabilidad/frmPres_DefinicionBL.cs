using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresDefinicionBl
    {
        readonly FrmPresDefinicionDb _db;

        public FrmPresDefinicionBl(IConfiguration config)
        {
            _db = new FrmPresDefinicionDb(config);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int CodEmpresa, string usuario, int codContab)
        {
            return _db.Pres_Modelos_Obtener(CodEmpresa, usuario, codContab);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            return _db.Pres_Modelo_Unidades_Obtener(CodEmpresa, codModelo, codContab, usuario);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_CC_Obtener(int CodEmpresa, string codModelo, int codContab, string codUnidad)
        {
            return _db.Pres_Modelo_Unidades_CC_Obtener(CodEmpresa, codModelo, codContab, codUnidad);
        }

        public ErrorDto<CntxCuentasData> Pres_Definicion_scroll(int CodEmpresa, int scrollValue, string? CodCtaMask, int CodContab)
        {
            return _db.Pres_Definicion_scroll(CodEmpresa, scrollValue, CodCtaMask, CodContab);
        }

        public ErrorDto<List<VistaPresCuentaData>> Pres_VistaPresupuesto_Cuenta_SP(int CodEmpresa, PresCuenta request)
        {
            return _db.Pres_VistaPresupuesto_Cuenta_SP(CodEmpresa, request);
        }

        public ErrorDto<CuentasLista> Pres_Cuentas_Obtener(int CodEmpresa, string cod_contabilidad, int? pagina, int? paginacion, string? filtro)
        {
            return _db.Pres_Cuentas_Obtener(CodEmpresa, cod_contabilidad, pagina, paginacion, filtro);
        }
       
    }
}