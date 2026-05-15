using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndGruposOperativosController : ControllerBase
    {
        private readonly FrmFndGruposOperativosBl _bl;

        public FrmFndGruposOperativosController(IConfiguration config)
        {
            _bl = new FrmFndGruposOperativosBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_GruposOperativos_Lista_Obtener")]
        public ErrorDto<FndGruposOperativosLista> Fnd_GruposOperativos_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Fnd_GruposOperativos_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Fnd_GruposOperativos_Obtener")]
        public ErrorDto<List<FndGrupoOperativoModel>> Fnd_GruposOperativos_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_GruposOperativos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Fnd_GruposOperativos_Valida")]
        public ErrorDto<FndGrupoOperativoValidaResult> Fnd_GruposOperativos_Valida(int CodEmpresa, string grupoCodigo)
        {
            return _bl.Fnd_GruposOperativos_Valida(CodEmpresa, grupoCodigo);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_Guardar")]
        public ErrorDto Fnd_GruposOperativos_Guardar(int CodEmpresa, [FromBody] FndGrupoOperativoModel grupo)
        {
            return _bl.Fnd_GruposOperativos_Guardar(CodEmpresa, grupo);
        }

        [Authorize]
        [HttpDelete("Fnd_GruposOperativos_Eliminar")]
        public ErrorDto Fnd_GruposOperativos_Eliminar(int CodEmpresa, string grupoCodigo, string usuario)
        {
            return _bl.Fnd_GruposOperativos_Eliminar(CodEmpresa, grupoCodigo, usuario);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_Planes_Obtener")]
        public ErrorDto<List<FndGrupoOperativoPlanResult>> Fnd_GruposOperativos_Planes_Obtener(
           int CodEmpresa, [FromBody] FndGrupoOperativoFiltroRequest request)
        {
            return _bl.Fnd_GruposOperativos_Planes_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_Usuarios_Obtener")]
        public ErrorDto<List<FndGrupoOperativoUsuarioResult>> Fnd_GruposOperativos_Usuarios_Obtener(
            int CodEmpresa, [FromBody] FndGrupoOperativoFiltroRequest request)
        {
            return _bl.Fnd_GruposOperativos_Usuarios_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_Conceptos_Obtener")]
        public ErrorDto<List<FndGrupoOperativoConceptoResult>> Fnd_GruposOperativos_Conceptos_Obtener(
            int CodEmpresa, [FromBody] FndGrupoOperativoFiltroRequest request)
        {
            return _bl.Fnd_GruposOperativos_Conceptos_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_AsignarPlan")]
        public ErrorDto Fnd_GruposOperativos_AsignarPlan(int CodEmpresa, [FromBody] FndGrupoOperativoAsignarPlanRequest request)
        {
            return _bl.Fnd_GruposOperativos_AsignarPlan(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_AsignarUsuario")]
        public ErrorDto Fnd_GruposOperativos_AsignarUsuario(int CodEmpresa, [FromBody] FndGrupoOperativoAsignarUsuarioRequest request)
        {
            return _bl.Fnd_GruposOperativos_AsignarUsuario(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_GruposOperativos_AsignarConcepto")]
        public ErrorDto Fnd_GruposOperativos_AsignarConcepto(int CodEmpresa, [FromBody] FndGrupoOperativoAsignarConceptoRequest request)
        {
            return _bl.Fnd_GruposOperativos_AsignarConcepto(CodEmpresa, request);
        }
    }
}