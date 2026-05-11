using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXConPortalesController : ControllerBase
    {
        private readonly FrmCntXConPortalesBl _bl;

        public FrmCntXConPortalesController(IConfiguration config) =>
            _bl = new FrmCntXConPortalesBl(config);

        [HttpGet("CntXConPortales_Consulta_Obtener")]
        public ErrorDto<CntXConPortalesDefinicionData?> CntXConPortales_Consulta_Obtener(
            int codEmpresa,
            int codPortal)
        {
            return _bl.CntXConPortales_Consulta_Obtener(codEmpresa, codPortal);
        }

        [HttpGet("CntXConPortales_Scroll_Obtener")]
        public ErrorDto<CntXConPortalesDefinicionData?> CntXConPortales_Scroll_Obtener(
            int codEmpresa,
            int codPortalActual,
            string direccion)
        {
            return _bl.CntXConPortales_Scroll_Obtener(codEmpresa, codPortalActual, direccion);
        }

        [HttpGet("CntXConPortales_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXConPortales_Lista_Obtener(int codEmpresa)
        {
            return _bl.CntXConPortales_Lista_Obtener(codEmpresa);
        }

        [HttpPost("CntXConPortales_ProbarConexion")]
        public ErrorDto CntXConPortales_ProbarConexion(
            int codEmpresa,
            CntXConPortalesConexionRequest request)
        {
            return _bl.CntXConPortales_ProbarConexion(codEmpresa, request);
        }

        [HttpPost("CntXConPortales_Contabilidades_Obtener")]
        public ErrorDto<List<CntXConPortalesContabilidadData>> CntXConPortales_Contabilidades_Obtener(
            int codEmpresa,
            CntXConPortalesConexionRequest request)
        {
            return _bl.CntXConPortales_Contabilidades_Obtener(codEmpresa, request);
        }

        [HttpPost("CntXConPortales_Guardar")]
        public ErrorDto CntXConPortales_Guardar(
            int codEmpresa,
            string usuario,
            CntXConPortalesGuardarRequest request)
        {
            return _bl.CntXConPortales_Guardar(codEmpresa, usuario, request);
        }

        [HttpPost("CntXConPortales_Borrar")]
        public ErrorDto CntXConPortales_Borrar(
            int codEmpresa,
            int codPortal,
            string usuario)
        {
            return _bl.CntXConPortales_Borrar(codEmpresa, codPortal, usuario);
        }
    }
}
