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

        public ErrorDto<List<DropDownListaGenericaModel>> Ah_ConciliadorPatronal_Instituciones_Obtener(
            int codEmpresa)
        {
            return _db.Ah_ConciliadorPatronal_Instituciones_Obtener(codEmpresa);
        }

        public static ErrorDto<List<FrmAhConciliadorPatronalHistoricoDto>> Ah_ConciliadorPatronal_Cargado(
            int codEmpresa,
            FrmAhConciliadorPatronalCargadoRequest request)
        {
            return FrmAhConciliadorPatronalDB.Ah_ConciliadorPatronal_Cargado(codEmpresa, request);
        }

        public ErrorDto<FrmAhConciliadorPatronalAplicarResponse> Ah_ConciliadorPatronal_Aplicar(
            int codEmpresa,
            FrmAhConciliadorPatronalCargadoRequest request)
        {
            return _db.Ah_ConciliadorPatronal_Aplicar(codEmpresa, request);
        }

        public ErrorDto<List<FrmAhConciliadorPatronalConciliacionDto>> Ah_ConciliadorPatronal_Conciliacion_Obtener(
            int codEmpresa,
            FrmAhConciliadorPatronalConciliacionRequest request)
        {
            return _db.Ah_ConciliadorPatronal_Conciliacion_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<FrmAhConciliadorPatronalResultadoDto>> Ah_ConciliadorPatronal_Resultados_Obtener(
            int codEmpresa,
            FrmAhConciliadorPatronalResultadosRequest request)
        {
            return _db.Ah_ConciliadorPatronal_Resultados_Obtener(codEmpresa, request);
        }
    }
}
