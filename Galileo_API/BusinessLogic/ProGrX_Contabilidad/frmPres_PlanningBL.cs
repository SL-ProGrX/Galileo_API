using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.BusinessLogic
{
    public class FrmPresPlanningBl
    {
        readonly FrmPresPlanningDb _db;

        public FrmPresPlanningBl(IConfiguration config)
        {
            _db = new FrmPresPlanningDb(config);
        }

        public ErrorDto<List<PresVistaPresupuestoData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            return _db.PresPlanning_Obtener(CodCliente, datos);
        }

        public ErrorDto<List<PreVistaPresupuestoCuentaData>> PresPlanningCuenta_Obtener(int CodCliente, string datos)
        {
            return _db.PresPlanningCuenta_Obtener(CodCliente, datos);
        }

        public ErrorDto<List<PresVistaPresCuentaRealHistoricoData>> PresPlanningCuentaReal_Obtener(int CodCliente, string datos)
        {
            return _db.PresPlanningCuentaReal_Obtener(CodCliente, datos);
        }

        public ErrorDto PresAjustes_Guardar(int CodCliente, PresAjustesGuarda request)
        {
            return _db.PresAjustes_Guardar(CodCliente, request);
        }

        public ErrorDto<CntxCierres> Pres_Cierre_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            return _db.Pres_Cierre_Obtener(CodEmpresa, codModelo, codContab, usuario);
        }

        public ErrorDto<List<PreVistaPresupuestoCuentaData>> Pres_Ajustes_Obtener(int CodCliente, int consulta, string datos)
        {
            return _db.Pres_Ajustes_Obtener(CodCliente, consulta, datos);
        }

        public ErrorDto<List<PresModelisLista>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _db.Pres_Modelos_Obtener(CodEmpresa, CodContab, Usuario);
        }

        public ErrorDto<List<ModeloGenericList>> Pres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            return _db.Pres_Ajustes_Permitidos_Obtener(CodEmpresa, codContab, codModelo, Usuario);
        }

        public ErrorDto Pres_AjusteMasivo_Guardar(int CodEmpresa, int codContab, string codModelo, string usuario, DateTime periodo, List<PresCargaMasivaModel> datos)
        {
            return _db.Pres_AjusteMasivo_Guardar(CodEmpresa, codContab, codModelo, usuario, periodo, datos);
        }

    }
}