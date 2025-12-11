using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosJustificacionesController : ControllerBase
    {
        private readonly FrmActivosJustificacionesBl _bl;

        public FrmActivosJustificacionesController(IConfiguration config)
        {
            _bl = new FrmActivosJustificacionesBl(config);
        }


        [HttpGet("Activos_JustificacionesLista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosJustificacionesLista> Activos_JustificacionesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_JustificacionesLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_JustificacionesExiste_Obtener")]
        [Authorize]
        public ErrorDto Activos_JustificacionesExiste_Obtener(int CodEmpresa, string cod_justificacion)
        {
            return _bl.Activos_JustificacionesExiste_Obtener(CodEmpresa, cod_justificacion);
        }

        [HttpGet("Activos_Justificaciones_Obtener")]
        [Authorize]
        public ErrorDto<ActivosJustificacionesData> Activos_Justificaciones_Obtener(int CodEmpresa, string cod_justificacion)
        {
            return _bl.Activos_Justificaciones_Obtener(CodEmpresa, cod_justificacion);
        }

        [HttpGet("Activos_Justificacion_Scroll")]
        [Authorize]
        public ErrorDto<ActivosJustificacionesData> Activos_Justificacion_Scroll(int CodEmpresa, int scroll, string? cod_justificacion)
        {
            return _bl.Activos_Justificacion_Scroll(CodEmpresa, scroll, cod_justificacion);
        }

        [HttpPost("Activos_Justificaciones_Guardar")]
        [Authorize]
        public ErrorDto Activos_Justificaciones_Guardar(int CodEmpresa, ActivosJustificacionesData justificacionesData)
        {
            return _bl.Activos_Justificaciones_Guardar(CodEmpresa, justificacionesData);
        }

        [HttpDelete("Activos_Justificaciones_Eliminar")]
        [Authorize]
        public ErrorDto Activos_Justificaciones_Eliminar(int CodEmpresa, string usuario, string cod_justificacion)
        {
            return _bl.Activos_Justificaciones_Eliminar(CodEmpresa, usuario, cod_justificacion);
        }

        [HttpGet("Activos_JustificacionesTipos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTipos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_JustificacionesTipos_Obtener(CodEmpresa);
        }

        [HttpGet("Activos_JustificacionesTiposAsientos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTiposAsientos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_JustificacionesTiposAsientos_Obtener(CodEmpresa, contabilidad);
        }
    }
}