using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCoGruposController : ControllerBase
    {
        private readonly FrmCoGruposBl _bl;

        public FrmCoGruposController(IConfiguration config)
        {
            _bl = new FrmCoGruposBl(config);
        }

        [Authorize]
        [HttpGet("CO_Grupos_Obtener")]
        public ErrorDto<List<CoGruposData>> CO_Grupos_Obtener(int CodEmpresa)
        {
            return _bl.CO_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CO_Grupos_Guardar")]
        public ErrorDto CO_Grupos_Guardar(int CodEmpresa, CoGruposData data)
        {
            return _bl.CO_Grupos_Guardar(CodEmpresa, data);
        }

        [Authorize]
        [HttpDelete("CO_Grupos_Eliminar")]
        public ErrorDto CO_Grupos_Eliminar(int CodEmpresa, int IdGrupo, string Usuario)
        {
            return _bl.CO_Grupos_Eliminar(CodEmpresa, IdGrupo, Usuario);
        }

        [Authorize]
        [HttpGet("CO_Grupos_Asignacion_Obtener")]
        public ErrorDto<List<CoGruposAsignacionData>> CO_Grupos_Asignacion_Obtener(int CodEmpresa, string GrupoId, string Filtro, int Tipo)
        {
            return _bl.CO_Grupos_Asignacion_Obtener(CodEmpresa, GrupoId, Filtro, Tipo);
        }

        [Authorize]
        [HttpPost("CO_Grupos_Asignar")]
        public ErrorDto CO_Grupos_Asignar(int CodEmpresa, string GrupoId, int Tipo, string Codigo, bool IsChecked, string Usuario)
        {
            return _bl.CO_Grupos_Asignar(CodEmpresa, GrupoId, Tipo, Codigo, IsChecked, Usuario);
        }
    }
}


