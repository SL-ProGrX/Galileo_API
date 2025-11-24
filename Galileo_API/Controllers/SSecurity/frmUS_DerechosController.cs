using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmUs_DerechosController : ControllerBase
    {
        readonly FrmUsDerechosBl DerechosBL;
        public FrmUs_DerechosController(IConfiguration config)
        {
            DerechosBL = new FrmUsDerechosBl(config);
        }

        [HttpGet("ObtenerUsDerechosNewDTOs")]
        public List<UsDerechosNewDto> ObtenerUsDerechosNewDTOs(string Rol, string Estado)
        {
            return DerechosBL.ObtenerUsDerechosNewDTOs(Rol, Estado);
        }

        [HttpGet("ObtenerArbolDerechosNew")]
        public List<UsModuloDto> ObtenerArbolDerechosNew(string Rol, string Estado)
        {
            return DerechosBL.ObtenerArbolDerechosNew(Rol, Estado);
        }

        [HttpGet("ObtenerArbolDerechosNewPrime")]
        public List<PrimeTreeDto> ObtenerArbolDerechosNewPrime(string Rol, string Estado)
        {
            return DerechosBL.ObtenerArbolDerechosNewPrime(Rol, Estado);
        }

        [HttpGet("ObtenerUsRoles")]
        public List<UsRolDto> ObtenerUsRoles()
        {
            return DerechosBL.ObtenerUsRoles();
        }


        [HttpDelete("EliminarUsDerechosNewDTO")]
        public ErrorDto EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            return DerechosBL.EliminarUsDerechosNewDTO(COD_OPCION, ESTADO, COD_ROL);
        }

        [HttpPost("CrearUsDerechosNewDTO")]
        public ErrorDto CrearUsDerechosNewDTO(List<CrearUsDerechosNewDto> info)
        {
            return DerechosBL.CrearUsDerechosNewDTO(info);
        }

        [HttpPatch("EditarUsDerechosNew")]
        public ErrorDto EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {
            return DerechosBL.EditarUsDerechosNew(COD_OPCION, ESTADO, COD_ROL, NUEVO_ESTADO);
        }

        [HttpPost("GuardarUsDerecho")]
        public ErrorDto GuardarUsDerecho(List<CrearUsDerechosNewDto> info)
        {
            return DerechosBL.CrearUsDerechosNewDTO(info);
        }

    }
}