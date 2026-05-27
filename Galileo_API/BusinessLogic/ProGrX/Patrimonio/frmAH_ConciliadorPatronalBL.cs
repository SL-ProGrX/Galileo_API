using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhConciliadorPatronalBL
    {
        private readonly FrmAhConciliadorPatronalDB _db;

        public FrmAhConciliadorPatronalBL(IConfiguration config)
        {
            _db = new FrmAhConciliadorPatronalDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ConciliadorPatronal_Instituciones_Obtener(
            int codEmpresa)
        {
            return _db.Patrimonio_frmAH_ConciliadorPatronal_Instituciones_Obtener(codEmpresa);
        }

        public static ErrorDto<List<FrmAhConciliadorPatronalHistoricoDto>> Patrimonio_frmAH_ConciliadorPatronal_Cargado(
            int codEmpresa,
            FrmAhConciliadorPatronalCargadoRequest request)
        {
            return FrmAhConciliadorPatronalDB.Patrimonio_frmAH_ConciliadorPatronal_Cargado(codEmpresa, request);
        }

        public ErrorDto<FrmAhConciliadorPatronalAplicarResponse> Patrimonio_frmAH_ConciliadorPatronal_Aplicar(
            int codEmpresa,
            FrmAhConciliadorPatronalCargadoRequest request)
        {
            return _db.Patrimonio_frmAH_ConciliadorPatronal_Aplicar(codEmpresa, request);
        }

        public ErrorDto<List<FrmAhConciliadorPatronalConciliacionDto>> Patrimonio_frmAH_ConciliadorPatronal_Conciliacion_Obtener(
            int codEmpresa,
            FrmAhConciliadorPatronalConciliacionRequest request)
        {
            return _db.Patrimonio_frmAH_ConciliadorPatronal_Conciliacion_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<FrmAhConciliadorPatronalResultadoDto>> Patrimonio_frmAH_ConciliadorPatronal_Resultados_Obtener(
            int codEmpresa,
            FrmAhConciliadorPatronalResultadosRequest request)
        {
            return _db.Patrimonio_frmAH_ConciliadorPatronal_Resultados_Obtener(codEmpresa, request);
        }
    }
}
