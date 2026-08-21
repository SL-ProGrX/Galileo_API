using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmRastreoMovDocController :
        ControllerBase
    {
        private readonly FrmRastreoMovDocBl _bl;

        public FrmRastreoMovDocController(
            IConfiguration config)
        {
            _bl = new FrmRastreoMovDocBl(config);
        }

        [HttpGet(
            "Conciliacion_RastreoMovDoc_Inicializar")]
        public ErrorDto<RastreoMovDocInicializaData>
            Conciliacion_RastreoMovDoc_Inicializar(
                int codEmpresa)
        {
            return _bl
                .Conciliacion_RastreoMovDoc_Inicializar(
                    codEmpresa);
        }

        [HttpGet(
            "Conciliacion_RastreoMovDoc_Resumen_Obtener")]
        public ErrorDto<List<RastreoMovDocResumenData>>
            Conciliacion_RastreoMovDoc_Resumen_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .Conciliacion_RastreoMovDoc_Resumen_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "Conciliacion_RastreoMovDoc_Detalle_Obtener")]
        public ErrorDto<List<RastreoMovDocDetalleData>>
            Conciliacion_RastreoMovDoc_Detalle_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .Conciliacion_RastreoMovDoc_Detalle_Obtener(
                    codEmpresa,
                    request);
        }
    }
}