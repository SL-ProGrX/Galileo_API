using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosConsultasController : ControllerBase
    {
        private readonly FrmCrConveniosConsultasBl _bl;

        public FrmCrConveniosConsultasController(IConfiguration config)
        {
            _bl = new FrmCrConveniosConsultasBl(config);
        }

        [HttpGet("CrConveniosConsultas_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrConveniosConsultas_Tipos_Obtener(
            [FromQuery] int codEmpresa)
            => _bl.CrConveniosConsultas_Tipos_Obtener(codEmpresa);

        [HttpGet("CrConveniosConsultas_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrConveniosConsultas_Usuarios_Obtener(
            [FromQuery] int codEmpresa)
            => _bl.CrConveniosConsultas_Usuarios_Obtener(codEmpresa);

        [HttpGet("CrConveniosConsultas_Convenios_Obtener")]
        public ErrorDto<List<CrConvenioConsultaData>> CrConveniosConsultas_Convenios_Obtener(
            [FromQuery] int codEmpresa)
            => _bl.CrConveniosConsultas_Convenios_Obtener(codEmpresa);

        [HttpGet("CrConveniosConsultas_Convenio_Obtener")]
        public ErrorDto<CrConvenioConsultaData?> CrConveniosConsultas_Convenio_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string? codConvenio,
            [FromQuery] string? descripcion)
            => _bl.CrConveniosConsultas_Convenio_Obtener(codEmpresa, codConvenio, descripcion);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosConsultas_Ordenes_Obtener")]
        public ErrorDto<CrConvenioOrdenesConsulta> CrConveniosConsultas_Ordenes_Obtener(
            [FromQuery] int codEmpresa,
            [FromBody] CrConvenioConsultaFiltros filtros)
            => _bl.CrConveniosConsultas_Ordenes_Obtener(codEmpresa, filtros);

        [HttpGet("CrConveniosConsultas_DetallePago_Obtener")]
        public ErrorDto<CrConvenioDetallePagoData?> CrConveniosConsultas_DetallePago_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string codFactura)
            => _bl.CrConveniosConsultas_DetallePago_Obtener(codEmpresa, codFactura);
    }
}
