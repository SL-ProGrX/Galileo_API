using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCambioInfoEstadisticaController : ControllerBase
    {
        private readonly FrmAfCambioInfoEstadisticaBL _bl;
        public FrmAfCambioInfoEstadisticaController(IConfiguration config)
        {
            _bl = new FrmAfCambioInfoEstadisticaBL(config);
        }

        [Authorize]
        [HttpGet("AF_CambioInfoEstadistica_Listas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CambioInfoEstadistica_Listas_Obtener(int CodEmpresa, string vTipo)
        {
            return _bl.AF_CambioInfoEstadistica_Listas_Obtener(CodEmpresa, vTipo);
        }

        [Authorize]
        [HttpPost("AF_CambioInfoEstadistica_Procesar")]
        public ErrorDto AF_CambioInfoEstadistica_Procesar(int CodEmpresa, string usuario, string vTipo, int vCodigo, List<AfCambioInfoEstadisticaDatos> cedulas)
        {
            return _bl.AF_CambioInfoEstadistica_Procesar(CodEmpresa, usuario, vTipo, vCodigo, cedulas);
        }
    }
}