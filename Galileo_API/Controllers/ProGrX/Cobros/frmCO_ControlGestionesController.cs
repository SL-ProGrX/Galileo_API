using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOControlGestionesController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCOControlGestionesBL _bl;

        public FrmCOControlGestionesController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCOControlGestionesBL(_config);
        }

        [Authorize]
        [HttpGet("Co_GestionesLista_Obtener")]
        public ErrorDto<CoControlGestionesLista> Co_GestionesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_GestionesLista_Obtener(CodEmpresa, filtros);
        }
        [Authorize]
        [HttpGet("Co_Gestiones_Export")]
        public ErrorDto<CoControlGestionesLista> Co_Gestiones_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_Gestiones_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Co_Gestiones_Guardar")]
        public ErrorDto Co_Gestiones_Guardar(int CodEmpresa, string usuario, CoControlGestionesData gestion)
        {
            return _bl.Co_Gestiones_Guardar(CodEmpresa, usuario, gestion);
        }

        [Authorize]
        [HttpDelete("Co_Gestiones_Eliminar")]
        public ErrorDto Co_Gestiones_Eliminar(int CodEmpresa, string usuario, string cod_gestion)
        {
            return _bl.Co_Gestiones_Eliminar(CodEmpresa, usuario, cod_gestion);
        }

        [Authorize]
        [HttpGet("Co_NivelGestion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_NivelGestion_Obtener(int CodEmpresa)
        {
            return _bl.Co_NivelGestion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Co_Seguridad_Gestiones_Obtener")]
        public ErrorDto<List<CoControlGestionesSeguridadGestionData>> Co_Seguridad_Gestiones_Obtener(int CodEmpresa)
        {
            return _bl.Co_Seguridad_Gestiones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Co_Seguridad_Usuarios_Obtener")]
        public ErrorDto<List<CoControlGestionesSeguridadUsuarioData>> Co_Seguridad_Usuarios_Obtener(int CodEmpresa, string cod_gestion)
        {
            return _bl.Co_Seguridad_Usuarios_Obtener(CodEmpresa, cod_gestion);
        }

        [Authorize]
        [HttpPost("Co_Seguridad_Asignacion_Guardar")]
        public ErrorDto Co_Seguridad_Asignacion_Guardar(int CodEmpresa, string usuario, CoControlGestionesSeguridadAsignacionDto dto)
        {
            return _bl.Co_Seguridad_Asignacion_Guardar(CodEmpresa, usuario, dto);
        }
    }
}
