using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmUsDerechosController : ControllerBase
    {
        readonly FrmUsDerechosBl DerechosBL;
        public FrmUsDerechosController(IConfiguration config)
        {
            DerechosBL = new FrmUsDerechosBl(config);
        }

        [HttpGet("ObtenerUsDerechosNewDTOs")]
        public ErrorDto<List<UsDerechosNewDto>> ObtenerUsDerechosNewDTOs(string Rol, string Estado)
        {
            return DerechosBL.ObtenerUsDerechosNewDTOs(Rol, Estado);
        }

        [HttpGet("ObtenerArbolDerechosNew")]
        public ErrorDto<List<UsModuloDto>> ObtenerArbolDerechosNew(string Rol, string Estado)
        {
            return DerechosBL.ObtenerArbolDerechosNew(Rol, Estado);
        }

        [HttpGet("ObtenerArbolDerechosNewPrime")]
        public ErrorDto<List<PrimeTreeDto>> ObtenerArbolDerechosNewPrime(string Rol, string Estado)
        {
            return DerechosBL.ObtenerArbolDerechosNewPrime(Rol, Estado);
        }

        [HttpGet("ObtenerUsRoles")]
        public ErrorDto<List<UsRolDto>> ObtenerUsRoles()
        {
            return DerechosBL.ObtenerUsRoles();
        }


        [HttpDelete("EliminarUsDerechosNewDTO")]
        public ErrorDto EliminarUsDerechosNewDTO(int COD_OPCION, string ESTADO, string COD_ROL)
        {
            return DerechosBL.EliminarUsDerechosNewDTO(COD_OPCION, ESTADO, COD_ROL);
        }

        [HttpPost("CrearUsDerechosNewDTO")]
        public ErrorDto CrearUsDerechosNewDTO([FromBody] List<CrearUsDerechosNewDto> info)
        {
            return DerechosBL.CrearUsDerechosNewDTO(info);
        }

        [HttpPatch("EditarUsDerechosNew")]
        public ErrorDto EditarUsDerechosNew(int COD_OPCION, string ESTADO, string COD_ROL, string NUEVO_ESTADO)
        {
            return DerechosBL.EditarUsDerechosNew(COD_OPCION, ESTADO, COD_ROL, NUEVO_ESTADO);
        }

        [HttpPost("GuardarUsDerecho")]
        public ErrorDto GuardarUsDerecho([FromBody] List<CrearUsDerechosNewDto> info)
        {
            return DerechosBL.CrearUsDerechosNewDTO(info);
        }

        [HttpPost("RegistrarBitacora")]
        public ErrorDto RegistrarBitacora([FromBody] SegLogInsertarDto request)
        {
            return DerechosBL.RegistrarBitacora(request);
        }

    }
}
