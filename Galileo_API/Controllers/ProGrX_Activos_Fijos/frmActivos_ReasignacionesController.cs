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
    public class FrmActivosReasignacionesController : ControllerBase
    {
        private readonly FrmActivosReasignacionesBL _bl;

        public FrmActivosReasignacionesController(IConfiguration config)
        {
            _bl = new FrmActivosReasignacionesBL(config);
        }

        [HttpGet("Activos_Reasignacion_SiguienteBoleta_Obtener")]
        [Authorize]
        public ErrorDto<string> Activos_Reasignacion_SiguienteBoleta_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Reasignacion_SiguienteBoleta_Obtener(CodEmpresa);
        }
        
        [HttpGet("Activos_Reasignacion_Activos_Lista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosReasignacionesActivosLista> Activos_Reasignacion_Activos_Lista_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.Activos_Reasignacion_Activos_Lista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_Reasignacion_Activo_Obtener")]
        [Authorize]
        public ActionResult<ErrorDto<ActivosReasignacionesActivo>> Activos_Reasignacion_Activo_Obtener(int CodEmpresa, string numPlaca)
        {
            return _bl.Activos_Reasignacion_Activo_Obtener(CodEmpresa, numPlaca);
        }

        [HttpGet("Activos_Reasignacion_Personas_Buscar")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Personas_Buscar(
            int CodEmpresa,
            string filtros)
        {
            return _bl.Activos_Reasignacion_Personas_Buscar(CodEmpresa, filtros);
        }

        [HttpGet("Activos_Reasignacion_Persona_Obtener")]
        [Authorize]
        public ErrorDto<ActivosReasignacionesPersona> Activos_Reasignacion_Persona_Obtener(
            int CodEmpresa,
            string identificacion)
        {
            return _bl.Activos_Reasignacion_Persona_Obtener(CodEmpresa, identificacion);
        }

        [HttpGet("Activos_Reasignacion_Motivos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reasignacion_Motivos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Reasignacion_Motivos_Obtener(CodEmpresa);
        }

        [HttpPost("Activos_Reasignacion_CambioResponsable")]
        [Authorize]
        public ErrorDto<ActivosReasignacionesBoletaResult> Activos_Reasignacion_CambioResponsable(
            int CodEmpresa,
            [FromBody] ActivosReasignacionesCambioRequest data)
        {
            return _bl.Activos_Reasignacion_CambioResponsable(CodEmpresa, data);
        }

        [HttpGet("Activos_Reasignacion_BoletasLista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosReasignacionesBoletaHistorialLista> Activos_Reasignacion_BoletasLista_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.Activos_Reasignacion_BoletasLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_Reasignacion_Boletas_Export")]
        [Authorize]
        public ErrorDto<List<ActivosReasignacionesBoletaHistorialItem>> Activos_Reasignacion_Boletas_Export(
            int CodEmpresa,
            string filtros)
        {
            return _bl.Activos_Reasignacion_Boletas_Export(CodEmpresa, filtros);
        }

        [HttpGet("Activos_Reasignacion_Obtener")]
        [Authorize]
        public ErrorDto<ActivosReasignacionesBoleta> Activos_Reasignacion_Obtener(
            int CodEmpresa,
            string cod_traslado)
        {
            return _bl.Activos_Reasignacion_Obtener(CodEmpresa, cod_traslado);
        }
        
        [Authorize]
        [HttpPost("Activos_Reasignacion_Boletas_Lote")]
        public ActionResult<ErrorDto<object>> Reasignacion_Boletas_Lote(int codEmpresa, ActivosReasignacionesBoletasLoteRequest request)
        {
            return _bl.Activos_Reasignacion_Boletas_Lote(codEmpresa, request);
        }

    }
}
