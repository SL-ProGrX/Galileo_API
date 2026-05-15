using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndParametrosController : ControllerBase
    {

        private readonly FrmFndParametrosBL _BL;

        public FrmFndParametrosController(IConfiguration? config)
        {
            _BL = new FrmFndParametrosBL(config);
        }

        [Authorize]
        [HttpGet("FND_Parametros_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_Parametros_Obtener(int CodEmpresa, bool exporta, int cod_contabilidad, string filtro)
        {
            return _BL.Fnd_Parametros_Obtener(CodEmpresa, exporta, cod_contabilidad, filtro);
        }

        [Authorize]
        [HttpPost("FND_Parametros_Guardar")]
        public ErrorDto Fnd_Parametros_Guardar(int CodEmpresa, string usuario, FndParametrosDto data)
        {
            return _BL.Fnd_Parametros_Guardar(CodEmpresa, usuario, data);
        }
    }
}