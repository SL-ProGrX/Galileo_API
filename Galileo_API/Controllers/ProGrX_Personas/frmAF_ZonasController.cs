using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Galileo_API.Models.ProGrX_Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
        
namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfZonasController : ControllerBase
    {
        private readonly FrmAfZonasBL _bl;

        public FrmAfZonasController(IConfiguration config)
        {
            _bl = new FrmAfZonasBL(config);
        }


        [HttpGet("AF_ZonasLista_Obtener")]
        public ErrorDto<ZonasLista> AF_ZonasLista_Obtener(int codEmpresa, string filtros)
        {
            return _bl.AF_ZonasLista_Obtener(codEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("AF_Zonas_Obtener")]
        public ErrorDto <List<ZonasData>> AF_Zonas_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_Zonas_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("AF_Zonas_Guardar")]
        public ErrorDto AF_Zonas_Guardar(int codEmpresa, string usuario, [FromBody] ZonasData zona)
        {
            return _bl.AF_Zonas_Guardar(codEmpresa, usuario, zona);
        }

        [HttpDelete("AF_Zonas_Eliminar")]
        public ErrorDto AF_Zonas_Eliminar(int codEmpresa, string usuario, string codZona)
        {
            return _bl.AF_Zonas_Eliminar(codEmpresa, usuario, codZona);
        }

        [HttpGet("AF_Zonas_Valida")]
        public ErrorDto<int> AF_Zonas_Valida(int codEmpresa, string codZona)
        {
            return _bl.AF_Zonas_Valida(codEmpresa, codZona);
        }

        [HttpGet("AF_Zonas_UsuariosAsignados_Obtener")]
        public ErrorDto<List<ZonaUsuarioAsignadoData>> AF_Zonas_UsuariosAsignados_Obtener(int codEmpresa, string codZona)
        {
            return _bl.AF_Zonas_UsuariosAsignados_Obtener(codEmpresa, codZona);
        }

        [HttpGet("AF_Zonas_InstitucionesAsignadas_Obtener")]
        public ErrorDto<List<ZonaInstitucionAsignadaData>> AF_Zonas_InstitucionesAsignadas_Obtener(int codEmpresa, string codZona)
        {
            return _bl.AF_Zonas_InstitucionesAsignadas_Obtener(codEmpresa, codZona);
        }

        [HttpPost("AF_Zonas_InstitucionAsignar_Registrar")]
        public ErrorDto AF_Zonas_InstitucionAsignar_Registrar(int codEmpresa, string codZona, int codInstitucion, bool asignar, string usuario)
        {
            return _bl.AF_Zonas_InstitucionAsignar_Registrar(codEmpresa, codZona, codInstitucion, asignar, usuario);
        }

        [HttpPost("AF_Zonas_UsuarioAsignar_Registrar")]
        public ErrorDto AF_Zonas_UsuarioAsignar_Registrar(int codEmpresa, string codZona, string codUsuario, bool asignar, string usuario)
        {
            return _bl.AF_Zonas_UsuarioAsignar_Registrar(codEmpresa, codZona, codUsuario, asignar, usuario);
        }
    }
}
