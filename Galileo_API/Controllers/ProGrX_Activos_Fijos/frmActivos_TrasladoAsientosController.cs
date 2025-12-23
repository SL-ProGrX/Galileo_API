using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosTrasladoAsientosController : ControllerBase
    {
        private readonly FrmActivosTrasladoAsientosBL _bl;
        public FrmActivosTrasladoAsientosController(IConfiguration config)
        {
            _bl = new FrmActivosTrasladoAsientosBL(config);
        }

        [HttpGet("Activos_TrasladoAsientos_Lista_Obtener")]
        [Authorize]
        public ErrorDto<TablasListaGenericaModel> Activos_TrasladoAsientos_Lista_Obtener(int CodEmpresa,string filtros)
        {
            return _bl.Activos_TrasladoAsientos_Lista_Obtener(CodEmpresa, filtros);
        }


        [HttpPost("Activos_TrasladoAsientos_Trasladar")]
        [Authorize]
        public ErrorDto<bool> Activos_TrasladoAsientos_Trasladar(int CodEmpresa, List<ActivosTrasladoAsientoRequest> request)
        {
            return _bl.Activos_TrasladoAsientos_Trasladar(CodEmpresa, request);
        }

    }
}