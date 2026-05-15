using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndRastreoMovDocController : ControllerBase
    {
        private readonly FrmFndRastreoMovDocBl BlFndRastreoMovDoc;

        public FrmFndRastreoMovDocController(IConfiguration config)
        {
            BlFndRastreoMovDoc = new FrmFndRastreoMovDocBl(config);
        }

        [Authorize]
        [HttpGet("FND_RastreoMovDoc_Resumen_Obtener")]
        public ErrorDto<List<FndRastreoMovDocResumenData>> FND_RastreoMovDoc_Resumen_Obtener(int CodEmpresa, string Filtros)
        {
            return BlFndRastreoMovDoc.FND_RastreoMovDoc_Resumen_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpGet("FND_RastreoMovDoc_Detalle_Obtener")]
        public ErrorDto<List<FndRastreoMovDocDetalleData>> FND_RastreoMovDoc_Detalle_Obtener(int CodEmpresa, string Filtros)
        {
            return BlFndRastreoMovDoc.FND_RastreoMovDoc_Detalle_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpGet("FND_RastreoMovDoc_Archivo_Obtener")]
        public ErrorDto<List<FndRastreoMovDocArchivosData>> FND_RastreoMovDoc_Archivo_Obtener(int CodEmpresa, string Filtros)
        {
            return BlFndRastreoMovDoc.FND_RastreoMovDoc_Archivo_Obtener(CodEmpresa, Filtros);
        }
    }
}