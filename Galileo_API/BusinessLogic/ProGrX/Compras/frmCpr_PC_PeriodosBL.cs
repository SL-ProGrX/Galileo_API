using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprPCPeriodosBL
    {
        readonly FrmCprPCPeriodosDB _db;

        public FrmCprPCPeriodosBL(IConfiguration config)
        {
            _db = new FrmCprPCPeriodosDB(config);
        }

        public ErrorDto<List<CatalogosLista>> CprPeriodosContabilidades_Obtener(int CodEmpresa)
        {
            return _db.CprPeriodosContabilidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CatalogosLista>> CprPeriodosModelos_Obtener(int CodEmpresa, string usuario, int cod_contabilidad)
        {
            return _db.CprPeriodosModelos_Obtener(CodEmpresa, usuario, cod_contabilidad);
        }

        public ErrorDto<CprPlanPeriodosDto> CprPeriodosPlan_Obtener(int CodEmpresa, int id_periodo)
        {
            return _db.CprPeriodosPlan_Obtener(CodEmpresa, id_periodo);
        }

        public ErrorDto<CprPeriodosPlanLista> CprPeriodosPlanLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CprPeriodosPlanLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<CprPlanPeriodosDto> CprPeriodoPlan_Scroll(int CodEmpresa, int scroll, int? id_periodo)
        {
            return _db.CprPeriodoPlan_Scroll(CodEmpresa, scroll, id_periodo);
        }

        public ErrorDto CprPeriodoPlan_Guardar(int CodEmpresa, CprPlanPeriodosDto periodo)
        {
            return _db.CprPeriodoPlan_Guardar(CodEmpresa, periodo);
        }

        public ErrorDto CprPeriodoPlan_Eliminar(int CodEmpresa, int id_periodo)
        {
            return _db.CprPeriodoPlan_Eliminar(CodEmpresa, id_periodo);
        }

        public ErrorDto CprPeriodoPlan_Aprobar(int CodEmpresa, int id_periodo, string usuario)
        {
            return _db.CprPeriodoPlan_Aprobar(CodEmpresa, id_periodo, usuario);
        }

        public ErrorDto<CprModeloDateDatos> CprPeriodoPlanMeses_Obtener(string modelo)
        {
            return _db.CprPeriodoPlanMeses_Obtener(modelo);
        }
    }
}