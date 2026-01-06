using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndSeguridadNivelesController : ControllerBase
    {
        private readonly FrmFndSeguridadNivelesBl _bl;

        public FrmFndSeguridadNivelesController(IConfiguration config) => _bl = new FrmFndSeguridadNivelesBl(config);

        [Authorize]
        [HttpGet("Fnd_SegNiveles_Grupos_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_SegNiveles_Grupos_Obtener(int CodEmpresa, bool Exporta, string Filtros)
        {
            return _bl.Fnd_SegNiveles_Grupos_Obtener(CodEmpresa, Exporta, Filtros);
        }

        [Authorize]
        [HttpGet("Fnd_SegNiveles_Planes_Obtener")]
        public ErrorDto<List<FndSegNivelesPlanesData>> Fnd_SegNiveles_Planes_Obtener(int CodEmpresa, string CodGrupo, string? Filtro)
        {
            return _bl.Fnd_SegNiveles_Planes_Obtener(CodEmpresa, CodGrupo, Filtro);
        }

        [Authorize]
        [HttpGet("Fnd_SegNiveles_Usuarios_Obtener")]
        public ErrorDto<List<FndSegNivelesUsuariosData>> Fnd_SegNiveles_Usuarios_Obtener(int CodEmpresa, string CodGrupo, string? Filtro)
        {
            return _bl.Fnd_SegNiveles_Usuarios_Obtener(CodEmpresa, CodGrupo, Filtro);
        }

        [Authorize]
        [HttpPost("Fnd_SegNiveles_Grupos_Guardar")]
        public ErrorDto Fnd_SegNiveles_Grupos_Guardar(int CodEmpresa, FndSegNivelesGrupoDto Data)
        {
            return _bl.Fnd_SegNiveles_Grupos_Guardar(CodEmpresa, Data);
        }

        [Authorize]
        [HttpDelete("Fnd_SegNiveles_Grupos_Eliminar")]
        public ErrorDto Fnd_SegNiveles_Grupos_Eliminar(int CodEmpresa, string CodGrupo, string Usuario)
        {
            return _bl.Fnd_SegNiveles_Grupos_Eliminar(CodEmpresa, CodGrupo, Usuario);
        }

        [Authorize]
        [HttpPost("Fnd_SegNiveles_Planes_Actualizar")]
        public ErrorDto Fnd_SegNiveles_Planes_Actualizar(int CodEmpresa, FndSegNivelesPlanesDto Data)
        {
            return _bl.Fnd_SegNiveles_Planes_Actualizar(CodEmpresa, Data);
        }

        [Authorize]
        [HttpPost("Fnd_SegNiveles_Usuarios_Actualizar")]
        public ErrorDto Fnd_SegNiveles_Usuarios_Actualizar(int CodEmpresa, FndSegNivelesUsuariosDto Data)
        {
            return _bl.Fnd_SegNiveles_Usuarios_Actualizar(CodEmpresa, Data);
        }
    }
}


