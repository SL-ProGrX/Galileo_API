using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresModeloBl
    {
        readonly FrmPresModeloDb _db;

        public FrmPresModeloBl(IConfiguration config)
        {
            _db = new FrmPresModeloDb(config);
        }

        public ErrorDto<List<CntxCData>> CntxContabilidades_Obtener(int CodEmpresa)
        {
            return _db.CntxContabilidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CntxCData>> CntxCierres_Obtener(int CodEmpresa, int CodContab)
        {
            return _db.CntxCierres_Obtener(CodEmpresa, CodContab);
        }

        public ErrorDto<PresModeloData> Pres_Modelo_Obtener(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _db.Pres_Modelo_Obtener(CodEmpresa, CodModelo, CodContab);
        }

        public ErrorDto<PresModeloData> Pres_Modelo_scroll(int CodEmpresa, int scrollValue, string? CodModelo, int CodContab)
        {
            return _db.Pres_Modelo_scroll(CodEmpresa, scrollValue, CodModelo, CodContab);
        }

        public ErrorDto Pres_Modelo_Insertar(int CodEmpresa, PresModeloInsert request)
        {
            return _db.Pres_Modelo_Insertar(CodEmpresa, request);
        }

        public ErrorDto Pres_MapeaCuentasSinCentroCosto_SP(int CodEmpresa, string CodModelo, int CodContab, string Usuario)
        {
            return _db.Pres_MapeaCuentasSinCentroCosto_SP(CodEmpresa, CodModelo, CodContab, Usuario);
        }

        public ErrorDto Pres_Model_Reiniciar(int CodEmpresa, string CodModelo)
        {
            return _db.Pres_Model_Reiniciar(CodEmpresa, CodModelo);
        }

        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _db.Pres_Modelo_Usuarios_SP(CodEmpresa, CodModelo, CodContab);
        }

        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _db.Pres_Modelo_Ajustes_SP(CodEmpresa, CodModelo, CodContab);
        }

        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_Ajustes_Autorizados_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _db.Pres_Modelo_Ajustes_Autorizados_SP(CodEmpresa, CodModelo, CodContab);
        }

        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_Usuarios_Autorizados_SP(int CodEmpresa, string CodModelo, int CodContab)
        {
            return _db.Pres_Modelo_Usuarios_Autorizados_SP(CodEmpresa, CodModelo, CodContab);
        }

        public ErrorDto<List<PressModeloAjustes>> Pres_Modelo_AjUs_Ajustes_SP(int CodEmpresa, string CodModelo, int CodContab, string Usuario)
        {
            return _db.Pres_Modelo_AjUs_Ajustes_SP(CodEmpresa, CodModelo, CodContab, Usuario);
        }

        public ErrorDto<List<PressModeloUsuarios>> Pres_Modelo_AjUs_Usuarios_SP(int CodEmpresa, string CodModelo, int CodContab, string CodAjuste)
        {
            return _db.Pres_Modelo_AjUs_Usuarios_SP(CodEmpresa, CodModelo, CodContab, CodAjuste);
        }

        public ErrorDto Pres_Modelo_AjUs_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            return _db.Pres_Modelo_AjUs_Registro_SP(CodEmpresa, request);
        }

        public ErrorDto Pres_Modelo_Ajustes_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            return _db.Pres_Modelo_Ajustes_Registro_SP(CodEmpresa, request);
        }

        public ErrorDto Pres_Modelo_Usuarios_Registro_SP(int CodEmpresa, PressModeloAjUsRegistro request)
        {
            return _db.Pres_Modelo_Usuarios_Registro_SP(CodEmpresa, request);
        }

        public ErrorDto<List<PresModeloData>> Pres_Modelos_Lista(int CodEmpresa, int CodContab)
        {
            return _db.Pres_Modelos_Lista(CodEmpresa, CodContab);
        }

        public ErrorDto Pres_Model_Eliminar(int CodEmpresa, string CodModelo)
        {
            return _db.Pres_Model_Eliminar(CodEmpresa, CodModelo);
        }
    }
}