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
        public ErrorDto<AfCdComiteDetalleDto> AfCdComites_Detalle(int codEmpresa, string codComite)
        {
            return _bl.AfCdComites_Detalle(codEmpresa, codComite);
        }

        [HttpGet("AfCdComites_BuscarComites")]
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarComites(int codEmpresa, string? filtro)
        {
            return _bl.AfCdComites_BuscarComites(codEmpresa, filtro);
        }

        [HttpGet("AfCdComites_DirectoresLista")]
        public ErrorDto<List<AfCdDirectorDto>> AfCdComites_DirectoresLista(int codEmpresa)
        {
            return _bl.AfCdComites_DirectoresLista(codEmpresa);
        }

        [HttpGet("AfCdComites_BuscarUnidades")]
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarUnidades(int codEmpresa, string? filtro)
        {
            return _bl.AfCdComites_BuscarUnidades(codEmpresa, filtro);
        }

        [HttpGet("AfCdComites_BuscarActividades")]
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarActividades(int codEmpresa, string? filtro)
        {
            return _bl.AfCdComites_BuscarActividades(codEmpresa, filtro);
        }

        [HttpGet("AfCdComites_BuscarEjecutivos")]
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarEjecutivos(int codEmpresa, string? filtro)
        {
            return _bl.AfCdComites_BuscarEjecutivos(codEmpresa, filtro);
        }

        [HttpGet("AfCdComites_BuscarMiembros")]
        public ErrorDto<List<AfCdComiteMiembroDto>> AfCdComites_BuscarMiembros(int codEmpresa, string? filtro)
        {
            return _bl.AfCdComites_BuscarMiembros(codEmpresa, filtro);
        }

        [HttpGet("AfCdComites_BuscarPorUnidad")]
        public ErrorDto<string?> AfCdComites_BuscarPorUnidad(int codEmpresa, string codigoUp)
        {
            return _bl.AfCdComites_BuscarPorUnidad(codEmpresa, codigoUp);
        }

        [HttpGet("AfCdComites_Scroll")]
        public ErrorDto<AfCdComiteDetalleDto?> AfCdComites_Scroll(int codEmpresa, string? codComite, int direccion)
        {
            return _bl.AfCdComites_Scroll(codEmpresa, codComite, direccion);
        }

        [HttpPost("AfCdComites_Guardar")]
        public ErrorDto<bool> AfCdComites_Guardar(int codEmpresa, AfCdComiteGuardarRequest request)
        {
            return _bl.AfCdComites_Guardar(codEmpresa, request);
        }

        [HttpPost("AfCdComites_Asociar")]
        public ErrorDto<bool> AfCdComites_Asociar(int codEmpresa, string tipo, AfCdComiteAsociacionRequest request)
        {
            return _bl.AfCdComites_Asociar(codEmpresa, tipo, request);
        }

        [HttpPost("AfCdComites_EliminarAsociacion")]
        public ErrorDto<bool> AfCdComites_EliminarAsociacion(int codEmpresa, string tipo, AfCdComiteAsociacionRequest request)
        {
            return _bl.AfCdComites_EliminarAsociacion(codEmpresa, tipo, request);
        }

        [HttpGet("AfCdComites_Miembros")]
        public ErrorDto<List<AfCdComiteMiembroDto>> AfCdComites_Miembros(int codEmpresa, string codComite, bool activos)
        {
            return _bl.AfCdComites_Miembros(codEmpresa, codComite, activos);
        }

        [HttpGet("AfCdComites_DatosMiembro")]
        public ErrorDto<AfCdComiteMiembroDto?> AfCdComites_DatosMiembro(int codEmpresa, string cedula, string? codComite)
        {
            return _bl.AfCdComites_DatosMiembro(codEmpresa, cedula, codComite);
        }

        [HttpPost("AfCdComites_GuardarMiembro")]
        public ErrorDto<bool> AfCdComites_GuardarMiembro(int codEmpresa, AfCdComiteMiembroGuardarRequest request)
        {
            return _bl.AfCdComites_GuardarMiembro(codEmpresa, request);
        }

        [HttpPost("AfCdComites_EliminarMiembro")]
        public ErrorDto<bool> AfCdComites_EliminarMiembro(int codEmpresa, string codComite, string cedula, string usuario)
        {
            return _bl.AfCdComites_EliminarMiembro(codEmpresa, codComite, cedula, usuario);
        }

        [HttpGet("AfCdComites_HistorialMiembros")]
        public ErrorDto<List<AfCdComiteMiembroHistorialDto>> AfCdComites_HistorialMiembros(int codEmpresa, string codComite)
        {
            return _bl.AfCdComites_HistorialMiembros(codEmpresa, codComite);
        }

        [HttpGet("AfCdComites_Mensajes")]
        public ErrorDto<List<AfCdComiteMensajeDto>> AfCdComites_Mensajes(int codEmpresa, string codComite)
        {
            return _bl.AfCdComites_Mensajes(codEmpresa, codComite);
        }
    }
}
