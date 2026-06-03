using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrx_Personas
{
    public class FrmAfSuspendidosConsultaBl
    {
        private readonly FrmAfSuspendidosConsultaDb DbAfSuspendidosConsulta;

        public FrmAfSuspendidosConsultaBl(IConfiguration config)
        {
            DbAfSuspendidosConsulta = new FrmAfSuspendidosConsultaDb(config);
        }

        public ErrorDto<List<AfSuspendidosConsultaDto>> AF_Suspendidos_Consulta_Obtener(int CodEmpresa, string filtro)
        {
            AfSuspendidosConsultaFiltros filtros = JsonConvert.DeserializeObject<AfSuspendidosConsultaFiltros>(filtro) ?? new AfSuspendidosConsultaFiltros();
            return DbAfSuspendidosConsulta.AF_Suspendidos_Consulta_Obtener(CodEmpresa, filtros);
        }
    }
}
