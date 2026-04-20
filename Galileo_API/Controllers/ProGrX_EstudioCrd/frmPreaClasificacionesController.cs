using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaClasificacionesController : ControllerBase
    {
        private readonly FrmPreaClasificacionesBl _bl;

        public FrmPreaClasificacionesController(IConfiguration config) =>
            _bl = new FrmPreaClasificacionesBl(config);

        [HttpGet("PreaClasificacion_Razones_Obtener")]
        public ErrorDto<List<PreaClasificacionRazonData>> PreaClasificacion_Razones_Obtener(int codEmpresa)
        {
            return _bl.PreaClasificacion_Razones_Obtener(codEmpresa);
        }

        [HttpGet("PreaClasificacion_Catalogo_Obtener")]
        public ErrorDto<List<PreaClasificacionData>> PreaClasificacion_Catalogo_Obtener(int codEmpresa, string catalogo)
        {
            return _bl.PreaClasificacion_Catalogo_Obtener(codEmpresa, catalogo);
        }

        [HttpPost("PreaClasificacion_Razon_Guardar")]
        public ErrorDto PreaClasificacion_Razon_Guardar(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            return _bl.PreaClasificacion_Razon_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaClasificacion_Razon_Eliminar")]
        public ErrorDto PreaClasificacion_Razon_Eliminar(int codEmpresa, string codRazon, string usuario)
        {
            return _bl.PreaClasificacion_Razon_Eliminar(codEmpresa, codRazon, usuario);
        }


    }
}