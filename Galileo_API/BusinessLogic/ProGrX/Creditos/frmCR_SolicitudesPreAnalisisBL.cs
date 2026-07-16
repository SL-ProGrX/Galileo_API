using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSolicitudesPreAnalisisBl
    {
        private readonly FrmCrSolicitudesPreAnalisisDb _db;

        public FrmCrSolicitudesPreAnalisisBl(IConfiguration config)
        {
            _db = new FrmCrSolicitudesPreAnalisisDb(config);
        }

        public ErrorDto<CrSolicitudesPreAnalisisPantallaData> CrSolicitudesPreAnalisis_Pantalla_Obtener(
            int codEmpresa)
            => _db.CrSolicitudesPreAnalisis_Pantalla_Obtener(codEmpresa);

        public ErrorDto<CrSolicitudesPreAnalisisConsultaData> CrSolicitudesPreAnalisis_Consulta_Obtener(
            int codEmpresa,
            string request)
        {
            CrSolicitudesPreAnalisisConsultaRequest filtros =
                JsonConvert.DeserializeObject<CrSolicitudesPreAnalisisConsultaRequest>(request)
                ?? new CrSolicitudesPreAnalisisConsultaRequest();

            return _db.CrSolicitudesPreAnalisis_Consulta_Obtener(codEmpresa, filtros);
        }
    }
}