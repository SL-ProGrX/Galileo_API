using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRCambioInfoEstadisticaModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class FrmCRCambioInfoEstadisticaController : ControllerBase
    {
        private readonly FrmCRCambioInfoEstadisticaBL _bl;

        public FrmCRCambioInfoEstadisticaController(IConfiguration config)
        {
            _bl = new FrmCRCambioInfoEstadisticaBL(config);
        }


        [HttpGet("CR_CambioInfoEstadistica_DatosTipo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioInfoEstadistica_DatosTipo_Obtener(int codEmpresa, string tipo)
           => _bl.CR_CambioInfoEstadistica_DatosTipo_Obtener(codEmpresa, tipo);

        [HttpPost("CR_CambioInfoEstadistica_Procesar")]
        public ErrorDto<bool> CR_CambioInfoEstadistica_Procesar(int codEmpresa, [FromBody] CrCambioInfoEstadisticaProcesarRequest request)
               => _bl.CR_CambioInfoEstadistica_Procesar(codEmpresa, request);

        [HttpPost("CR_CambioInfoEstadistica_CargarListado")]
        public ErrorDto<CrCambioInfoEstadisticaCargaListadoResponse> CR_CambioInfoEstadistica_CargarListado(int codEmpresa, [FromBody] CrCambioInfoEstadisticaCargaListadoRequest request)
              => _bl.CR_CambioInfoEstadistica_CargarListado(codEmpresa, request);
    }
}
