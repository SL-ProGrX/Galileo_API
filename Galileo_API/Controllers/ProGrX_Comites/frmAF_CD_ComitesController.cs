using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfCdComitesController : ControllerBase
    {
        private readonly FrmAfCdComitesBL _bl;

        public FrmAfCdComitesController(IConfiguration config)
        {
            _bl = new FrmAfCdComitesBL(config);
        }

        [HttpGet("AfCdComites_Detalle")]
        public ActionResult<ErrorDto<AfCdComiteDetalleDto>> AfCdComites_Detalle([FromQuery] int codEmpresa, [FromQuery] string codComite)
            => _bl.AfCdComites_Detalle(codEmpresa, codComite);

        [HttpGet("AfCdComites_BuscarComites")]
        public ActionResult<ErrorDto<List<AfCdComiteListaDto>>> AfCdComites_BuscarComites([FromQuery] int codEmpresa, [FromQuery] string? filtro)
            => _bl.AfCdComites_BuscarComites(codEmpresa, filtro);

        [HttpGet("AfCdComites_DirectoresLista")]
        public ActionResult<ErrorDto<List<AfCdDirectorDto>>> AfCdComites_DirectoresLista([FromQuery] int codEmpresa)
            => _bl.AfCdComites_DirectoresLista(codEmpresa);

        [HttpGet("AfCdComites_BuscarUnidades")]
        public ActionResult<ErrorDto<List<AfCdComiteListaDto>>> AfCdComites_BuscarUnidades([FromQuery] int codEmpresa, [FromQuery] string? filtro)
            => _bl.AfCdComites_BuscarUnidades(codEmpresa, filtro);

        [HttpGet("AfCdComites_BuscarActividades")]
        public ActionResult<ErrorDto<List<AfCdComiteListaDto>>> AfCdComites_BuscarActividades([FromQuery] int codEmpresa, [FromQuery] string? filtro)
            => _bl.AfCdComites_BuscarActividades(codEmpresa, filtro);

        [HttpGet("AfCdComites_BuscarEjecutivos")]
        public ActionResult<ErrorDto<List<AfCdComiteListaDto>>> AfCdComites_BuscarEjecutivos([FromQuery] int codEmpresa, [FromQuery] string? filtro)
            => _bl.AfCdComites_BuscarEjecutivos(codEmpresa, filtro);

        [HttpGet("AfCdComites_BuscarMiembros")]
        public ActionResult<ErrorDto<List<AfCdComiteMiembroDto>>> AfCdComites_BuscarMiembros([FromQuery] int codEmpresa, [FromQuery] string? filtro)
            => _bl.AfCdComites_BuscarMiembros(codEmpresa, filtro);

        [HttpGet("AfCdComites_BuscarPorUnidad")]
        public ActionResult<ErrorDto<string?>> AfCdComites_BuscarPorUnidad([FromQuery] int codEmpresa, [FromQuery] string codigoUp)
            => _bl.AfCdComites_BuscarPorUnidad(codEmpresa, codigoUp);

        [HttpGet("AfCdComites_Scroll")]
        public ActionResult<ErrorDto<AfCdComiteDetalleDto?>> AfCdComites_Scroll([FromQuery] int codEmpresa, [FromQuery] string? codComite, [FromQuery] int direccion)
            => _bl.AfCdComites_Scroll(codEmpresa, codComite, direccion);

        [HttpPost("AfCdComites_Guardar")]
        public ActionResult<ErrorDto<bool>> AfCdComites_Guardar([FromQuery] int codEmpresa, [FromBody] AfCdComiteGuardarRequest request)
            => _bl.AfCdComites_Guardar(codEmpresa, request);

        [HttpPost("AfCdComites_Asociar")]
        public ActionResult<ErrorDto<bool>> AfCdComites_Asociar([FromQuery] int codEmpresa, [FromQuery] string tipo, [FromBody] AfCdComiteAsociacionRequest request)
            => _bl.AfCdComites_Asociar(codEmpresa, tipo, request);

        [HttpPost("AfCdComites_EliminarAsociacion")]
        public ActionResult<ErrorDto<bool>> AfCdComites_EliminarAsociacion([FromQuery] int codEmpresa, [FromQuery] string tipo, [FromBody] AfCdComiteAsociacionRequest request)
            => _bl.AfCdComites_EliminarAsociacion(codEmpresa, tipo, request);

        [HttpGet("AfCdComites_Miembros")]
        public ActionResult<ErrorDto<List<AfCdComiteMiembroDto>>> AfCdComites_Miembros([FromQuery] int codEmpresa, [FromQuery] string codComite, [FromQuery] bool activos)
            => _bl.AfCdComites_Miembros(codEmpresa, codComite, activos);

        [HttpGet("AfCdComites_DatosMiembro")]
        public ActionResult<ErrorDto<AfCdComiteMiembroDto?>> AfCdComites_DatosMiembro([FromQuery] int codEmpresa, [FromQuery] string cedula, [FromQuery] string? codComite)
            => _bl.AfCdComites_DatosMiembro(codEmpresa, cedula, codComite);

        [HttpPost("AfCdComites_GuardarMiembro")]
        public ActionResult<ErrorDto<bool>> AfCdComites_GuardarMiembro([FromQuery] int codEmpresa, [FromBody] AfCdComiteMiembroGuardarRequest request)
            => _bl.AfCdComites_GuardarMiembro(codEmpresa, request);

        [HttpPost("AfCdComites_EliminarMiembro")]
        public ActionResult<ErrorDto<bool>> AfCdComites_EliminarMiembro([FromQuery] int codEmpresa, [FromQuery] string codComite, [FromQuery] string cedula, [FromQuery] string usuario)
            => _bl.AfCdComites_EliminarMiembro(codEmpresa, codComite, cedula, usuario);

        [HttpGet("AfCdComites_HistorialMiembros")]
        public ActionResult<ErrorDto<List<AfCdComiteMiembroHistorialDto>>> AfCdComites_HistorialMiembros([FromQuery] int codEmpresa, [FromQuery] string codComite)
            => _bl.AfCdComites_HistorialMiembros(codEmpresa, codComite);

        [HttpGet("AfCdComites_Mensajes")]
        public ActionResult<ErrorDto<List<AfCdComiteMensajeDto>>> AfCdComites_Mensajes([FromQuery] int codEmpresa, [FromQuery] string codComite)
            => _bl.AfCdComites_Mensajes(codEmpresa, codComite);
    }
}
