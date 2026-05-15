using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndRastreoMovDocBl
    {
        private readonly FrmFndRastreoMovDocDb DbFndRastreoMovDoc;

        public FrmFndRastreoMovDocBl(IConfiguration config)
        {
            DbFndRastreoMovDoc = new FrmFndRastreoMovDocDb(config);
        }

        public ErrorDto<List<FndRastreoMovDocResumenData>> FND_RastreoMovDoc_Resumen_Obtener(int CodEmpresa, string Filtros)
        {
            FndRastreoMovDocFiltros filtros = JsonConvert.DeserializeObject<FndRastreoMovDocFiltros>(Filtros) ?? new FndRastreoMovDocFiltros();
            return DbFndRastreoMovDoc.FND_RastreoMovDoc_Resumen_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<FndRastreoMovDocDetalleData>> FND_RastreoMovDoc_Detalle_Obtener(int CodEmpresa, string Filtros)
        {
            FndRastreoMovDocFiltros filtros = JsonConvert.DeserializeObject<FndRastreoMovDocFiltros>(Filtros) ?? new FndRastreoMovDocFiltros();
            return DbFndRastreoMovDoc.FND_RastreoMovDoc_Detalle_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<FndRastreoMovDocArchivosData>> FND_RastreoMovDoc_Archivo_Obtener(int CodEmpresa, string Filtros)
        {
            FndRastreoMovDocFiltros filtros = JsonConvert.DeserializeObject<FndRastreoMovDocFiltros>(Filtros) ?? new FndRastreoMovDocFiltros();
            return DbFndRastreoMovDoc.FND_RastreoMovDoc_Archivo_Obtener(CodEmpresa, filtros);
        }
    }
}