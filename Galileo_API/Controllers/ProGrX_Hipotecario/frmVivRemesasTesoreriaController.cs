using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmVivRemesasTesoreriaController : ControllerBase
    {
        private readonly FrmVivRemesasTesoreriaBL _bl;

        public FrmVivRemesasTesoreriaController(IConfiguration config)
        {
            _bl = new FrmVivRemesasTesoreriaBL(config);
        }

        [HttpGet("RemesasTesoreria_Obtener")]
        public ActionResult<ErrorDto<List<RemesasTesoreriaObtenerDto>>> RemesasTesoreria_Obtener([FromQuery] int codEmpresa)
            => _bl.RemesasTesoreria_Obtener(codEmpresa);

        [HttpPost("RemesasTesoreria_Insertar")]
        public ActionResult<ErrorDto<int>> RemesasTesoreria_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] RemesaTesoreriaUpsertDto dto)
            => _bl.RemesasTesoreria_Insertar(codEmpresa, dto);

        [HttpPut("RemesasTesoreria_Actualizar")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreria_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] RemesaTesoreriaUpsertDto dto)
            => _bl.RemesasTesoreria_Actualizar(codEmpresa, dto);

        [HttpDelete("RemesasTesoreriaDetalle_Eliminar")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreriaDetalle_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesa)
            => _bl.RemesasTesoreriaDetalle_Eliminar(codEmpresa, remesa);

        [HttpGet("RemesasTesoreria_Filtrar")]
        public ActionResult<ErrorDto<List<RemesasTesoreriaObtenerDto>>> RemesasTesoreria_Filtrar(
            [FromQuery] int codEmpresa,
            [FromQuery] string tipo)
            => _bl.RemesasTesoreria_Filtrar(codEmpresa, tipo);

        [HttpGet("RemesasTesoreria_DesembolsosDisponibles")]
        public ActionResult<ErrorDto<List<RemesaTesoreriaDesembolsoDisponibleDto>>> RemesasTesoreria_DesembolsosDisponibles(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada)
            => _bl.RemesasTesoreria_DesembolsosDisponibles(codEmpresa, remesaSeleccionada);

        [HttpGet("RemesasTesoreria_ValidarAbierta")]
        public ActionResult<ErrorDto<RemesaTesoreriaExisteDto>> RemesasTesoreria_ValidarAbierta(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada)
            => _bl.RemesasTesoreria_ValidarAbierta(codEmpresa, remesaSeleccionada);

        [HttpPut("RemesasTesoreria_CargarDesembolso")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreria_CargarDesembolso(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada,
            [FromQuery] int codigoDesembolso)
            => _bl.RemesasTesoreria_CargarDesembolso(codEmpresa, remesaSeleccionada, codigoDesembolso);

        [HttpPut("RemesasTesoreria_Cerrar")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreria_Cerrar(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada,
            [FromQuery] string usuario)
            => _bl.RemesasTesoreria_Cerrar(codEmpresa, remesaSeleccionada, usuario);

        [HttpGet("RemesasTesoreria_DesembolsosAsignados")]
        public ActionResult<ErrorDto<List<RemesaTesoreriaDesembolsoAsignadoDto>>> RemesasTesoreria_DesembolsosAsignados(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada)
            => _bl.RemesasTesoreria_DesembolsosAsignados(codEmpresa, remesaSeleccionada);

        [HttpGet("RemesasTesoreria_ValidarCerrada")]
        public ActionResult<ErrorDto<RemesaTesoreriaExisteDto>> RemesasTesoreria_ValidarCerrada(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada)
            => _bl.RemesasTesoreria_ValidarCerrada(codEmpresa, remesaSeleccionada);

        [HttpPut("RemesasTesoreria_ActualizarProceso")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreria_ActualizarProceso(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesaSeleccionada,
            [FromQuery] string usuario,
            [FromQuery] int idDesem)
            => _bl.RemesasTesoreria_ActualizarProceso(codEmpresa, remesaSeleccionada, usuario, idDesem);
    }
}
