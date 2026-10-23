using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo_API.BusinessLogic.ProGrX_Inversiones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Inversiones
{
    [Route("api/frmIVR_Rec_Titulos")]
    [Authorize]
    [ApiController]
    public sealed class FrmIvrRecTitulosController : ControllerBase
    {
        private readonly FrmIvrRecTitulosBl _bl;

        public FrmIvrRecTitulosController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmIvrRecTitulosBl(config);
        }

        [HttpGet("IVR_Rec_Titulos_Inicializar")]
        public ErrorDto<IvrRecTitulosInicialData> IVR_Rec_Titulos_Inicializar(
            [FromQuery] int codEmpresa) =>
            _bl.IVR_Rec_Titulos_Inicializar(codEmpresa);

        [HttpGet("IVR_Rec_Titulos_Buscar")]
        public ErrorDto<List<IvrRecTitulosBusquedaData>> IVR_Rec_Titulos_Buscar(
            [FromQuery] int codEmpresa,
            [FromQuery] string? filtro) =>
            _bl.IVR_Rec_Titulos_Buscar(codEmpresa, filtro);

        [HttpGet("IVR_Rec_Titulos_Navegar")]
        public ErrorDto<IvrRecTitulosNavegacionData?> IVR_Rec_Titulos_Navegar(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId,
            [FromQuery] bool siguiente) =>
            _bl.IVR_Rec_Titulos_Navegar(codEmpresa, tituloId, siguiente);

        [HttpGet("IVR_Rec_Titulos_Obtener")]
        public ErrorDto<IvrRecTitulosData?> IVR_Rec_Titulos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId) =>
            _bl.IVR_Rec_Titulos_Obtener(codEmpresa, tituloId);

        [HttpGet("IVR_Rec_Titulos_Adquisiciones_Obtener")]
        public ErrorDto<List<IvrRecTitulosTransaccionData>> IVR_Rec_Titulos_Adquisiciones_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId) =>
            _bl.IVR_Rec_Titulos_Adquisiciones_Obtener(codEmpresa, tituloId);

        [HttpGet("IVR_Rec_Titulos_Comisiones_Obtener")]
        public ErrorDto<List<IvrRecTitulosComisionData>> IVR_Rec_Titulos_Comisiones_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId) =>
            _bl.IVR_Rec_Titulos_Comisiones_Obtener(codEmpresa, tituloId);

        [HttpGet("IVR_Rec_Titulos_Flujos_Obtener")]
        public ErrorDto<List<IvrRecTitulosFlujoData>> IVR_Rec_Titulos_Flujos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId,
            [FromQuery] string? tipo) =>
            _bl.IVR_Rec_Titulos_Flujos_Obtener(codEmpresa, tituloId, tipo);

        [HttpGet("IVR_Rec_Titulos_Cierres_Obtener")]
        public ErrorDto<List<IvrRecTitulosCierreData>> IVR_Rec_Titulos_Cierres_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId) =>
            _bl.IVR_Rec_Titulos_Cierres_Obtener(codEmpresa, tituloId);

        [HttpGet("IVR_Rec_Titulos_Fondos_Obtener")]
        public ErrorDto<List<IvrRecTitulosFondoData>> IVR_Rec_Titulos_Fondos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId,
            [FromQuery] string? estado,
            [FromQuery] string? tipo) =>
            _bl.IVR_Rec_Titulos_Fondos_Obtener(codEmpresa, tituloId, estado, tipo);

        [HttpGet("IVR_Rec_Titulos_Cupones_Obtener")]
        public ErrorDto<List<IvrRecTitulosCuponData>> IVR_Rec_Titulos_Cupones_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId,
            [FromQuery] bool proyectados) =>
            _bl.IVR_Rec_Titulos_Cupones_Obtener(codEmpresa, tituloId, proyectados);

        [HttpGet("IVR_Rec_Titulos_Asientos_Obtener")]
        public ErrorDto<List<IvrRecTitulosAsientoData>> IVR_Rec_Titulos_Asientos_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId) =>
            _bl.IVR_Rec_Titulos_Asientos_Obtener(codEmpresa, tituloId);

        [HttpGet("IVR_Rec_Titulos_Asiento_Titulo_Obtener")]
        public ErrorDto<List<IvrRecTitulosAsientoDetalleData>> IVR_Rec_Titulos_Asiento_Titulo_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] long tituloId) =>
            _bl.IVR_Rec_Titulos_Asiento_Titulo_Obtener(codEmpresa, tituloId);

        [HttpGet("IVR_Rec_Titulos_Asiento_Detalle_Obtener")]
        public ErrorDto<List<IvrRecTitulosAsientoDetalleData>> IVR_Rec_Titulos_Asiento_Detalle_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] string? tipoAsiento,
            [FromQuery] string? numeroAsiento) =>
            _bl.IVR_Rec_Titulos_Asiento_Detalle_Obtener(codEmpresa, tipoAsiento, numeroAsiento);

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Rec_Titulos_Registrar")]
        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Registrar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrRecTitulosGuardarRequest? request) =>
            _bl.IVR_Rec_Titulos_Registrar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Rec_Titulos_Actualizar")]
        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrRecTitulosGuardarRequest? request) =>
            _bl.IVR_Rec_Titulos_Actualizar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpPut("IVR_Rec_Titulos_Activar")]
        public ErrorDto<IvrRecTitulosGuardarData> IVR_Rec_Titulos_Activar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrRecTitulosAccionRequest? request) =>
            _bl.IVR_Rec_Titulos_Activar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpDelete("IVR_Rec_Titulos_Adquisicion_Eliminar")]
        public ErrorDto IVR_Rec_Titulos_Adquisicion_Eliminar(
            [FromQuery] int codEmpresa,
            [FromBody] IvrRecTitulosTransaccionEliminarRequest? request) =>
            _bl.IVR_Rec_Titulos_Adquisicion_Eliminar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpPost("IVR_Rec_Titulos_Pyd_Recalcular")]
        public ErrorDto IVR_Rec_Titulos_Pyd_Recalcular(
            [FromQuery] int codEmpresa,
            [FromBody] IvrRecTitulosAccionRequest? request) =>
            _bl.IVR_Rec_Titulos_Pyd_Recalcular(codEmpresa, request);
    }
}
