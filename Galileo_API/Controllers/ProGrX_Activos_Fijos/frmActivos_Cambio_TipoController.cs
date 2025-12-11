using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosCambioTipoController : ControllerBase
    {
        private readonly FrmActivosCambioTipoBL _bl;
        public FrmActivosCambioTipoController(IConfiguration config)
        {
            _bl = new FrmActivosCambioTipoBL(config);
        }

        [Authorize]
        [HttpGet("Activos_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Tipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_DatosActivo_Consultar")]
        public ErrorDto<ActivosPrincipalesData?> Activos_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            return _bl.Activos_DatosActivo_Consultar(CodEmpresa, placa);
        }

        [Authorize]
        [HttpGet("Activos_Obtener")]
        public ErrorDto<List<ActivosData>> Activos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Obtener(CodEmpresa);
        }

    }
}