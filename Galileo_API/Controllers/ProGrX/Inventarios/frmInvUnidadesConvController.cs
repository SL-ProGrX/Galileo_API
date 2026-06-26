using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmInvUnidadesConvController : ControllerBase
    {
        private readonly FrmInvUnidadesConvBL _bl;

        public FrmInvUnidadesConvController(IConfiguration config)
        {
            _bl = new FrmInvUnidadesConvBL(config);
        }

        [HttpGet("UnidadMedicion_Obtener")]
        public ErrorDto<List<UnidadMedicionConv>> UnidadMedicion_Obtener(int CodCliente)
        {
            return _bl.UnidadMedicion_Obtener(CodCliente);
        }

        [HttpGet("UnidadConvLista_Obtener")]
        public ErrorDto<UnidadesConvLista> UnidadConvLista_Obtener(int CodCliente, string cod_unidad)
        {
            return _bl.UnidadConvLista_Obtener(CodCliente, cod_unidad);
        }

        [HttpPost("UnidadConv_Guardar")]
        public ErrorDto UnidadConv_Guardar(int CodCliente, UnidadMedicionConvData equivalencia)
        {
            return _bl.UnidadConv_Guardar(CodCliente, equivalencia);
        }

        [HttpDelete("UnidadConv_Eliminar")]
        public ErrorDto UnidadConv_Eliminar(int CodCliente, string cod_unidad, string cod_unidad_d)
        {
            return _bl.UnidadConv_Eliminar(CodCliente, cod_unidad, cod_unidad_d);
        }
    }
}