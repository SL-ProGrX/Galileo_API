using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrReporteDiferidosBl
    {
        private readonly FrmCrReporteDiferidosDb _db;

        public FrmCrReporteDiferidosBl(IConfiguration config)
        {
            _db = new FrmCrReporteDiferidosDb(config);
        }

        public ErrorDto<CrReporteDiferidosPantallaData> CrReporteDiferidos_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _db.CrReporteDiferidos_Pantalla_Obtener(codEmpresa, usuario);

        public ErrorDto<List<DropDownListaGenericaModel>> CrReporteDiferidos_Catalogo_Obtener(
            int codEmpresa)
            => _db.CrReporteDiferidos_Catalogo_Obtener(codEmpresa);

        public ErrorDto<DropDownListaGenericaModel> CrReporteDiferidos_Codigo_Descripcion_Obtener(
            int codEmpresa,
            string codigo)
            => _db.CrReporteDiferidos_Codigo_Descripcion_Obtener(codEmpresa, codigo);

        public ErrorDto<List<CrReporteDiferidosItem>> CrReporteDiferidos_Consulta_Obtener(
            int codEmpresa,
            string request)
        {
            CrReporteDiferidosConsultaRequest filtros = 
                JsonConvert.DeserializeObject<CrReporteDiferidosConsultaRequest>(request) ?? new CrReporteDiferidosConsultaRequest();
            return _db.CrReporteDiferidos_Consulta_Obtener(codEmpresa, filtros);
        }
    }
}