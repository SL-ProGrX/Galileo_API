using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConsultaOperacionesController : ControllerBase
    {
        private readonly FrmCrConsultaOperacionesBl _bl;

        public FrmCrConsultaOperacionesController(IConfiguration config)
        {
            _bl = new FrmCrConsultaOperacionesBl(config);
        }

        [HttpGet("CrConsultaOperaciones_BusquedaOperaciones_Obtener")]
        public ErrorDto<List<CrConsultaOperacionesBusquedaOperacionDto>> CrConsultaOperaciones_BusquedaOperaciones_Obtener(
            int codEmpresa)
            => _bl.CrConsultaOperaciones_BusquedaOperaciones_Obtener(codEmpresa);

        [HttpGet("CrConsultaOperaciones_BusquedaSocios_Obtener")]
        public ErrorDto<List<CrConsultaOperacionesBusquedaSocioDto>> CrConsultaOperaciones_BusquedaSocios_Obtener(
            int codEmpresa)
            => _bl.CrConsultaOperaciones_BusquedaSocios_Obtener(codEmpresa);

        [HttpGet("CrConsultaOperaciones_Cedula_Obtener")]
        public ErrorDto<List<CrConsultaOperacionesListaDto>> CrConsultaOperaciones_Cedula_Obtener(
            int codEmpresa,
            string cedula)
            => _bl.CrConsultaOperaciones_Cedula_Obtener(codEmpresa, cedula);

        [HttpGet("CrConsultaOperaciones_Detalle_Obtener")]
        public ErrorDto<CrConsultaOperacionesDetalleDto> CrConsultaOperaciones_Detalle_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.CrConsultaOperaciones_Detalle_Obtener(codEmpresa, operacion);
    }
}