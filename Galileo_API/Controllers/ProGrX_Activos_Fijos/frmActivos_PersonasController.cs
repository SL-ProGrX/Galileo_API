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
    public class FrmActivosPersonasController : ControllerBase
    {
        private readonly FrmActivosPersonasBL _bl;

        public FrmActivosPersonasController(IConfiguration config)
        {
            _bl = new FrmActivosPersonasBL(config);
        }

        [Authorize]
        [HttpGet("Activos_Personas_Lista_Obtener")]
        public ErrorDto<ActivosPersonasLista> Activos_Personas_Lista_Obtener(int CodEmpresa, string filtros, string? codDepartamento = null, string? codSeccion = null)
        {
            // Ensure no nulls are passed to BL
            var safeCodDepartamento = codDepartamento ?? string.Empty;
            var safeCodSeccion = codSeccion ?? string.Empty;
            return _bl.Activos_Personas_Lista_Obtener(CodEmpresa, filtros, safeCodDepartamento, safeCodSeccion);
        }

        [Authorize]
        [HttpGet("Activos_Personas_Obtener")]
        public ErrorDto<List<ActivosPersonasData>> Activos_Personas_Obtener(int CodEmpresa, string filtros, string? codDepartamento = null, string? codSeccion = null)
        {
            var safeCodDepartamento = codDepartamento ?? string.Empty;
            var safeCodSeccion = codSeccion ?? string.Empty;
            return _bl.Activos_Personas_Obtener(CodEmpresa, filtros, safeCodDepartamento, safeCodSeccion);
        }

        [Authorize]
        [HttpPost("Activos_Personas_Guardar")]
        public ErrorDto Activos_Personas_Guardar(int CodEmpresa, string usuario,ActivosPersonasData persona)
        {
            return _bl.Activos_Personas_Guardar(CodEmpresa, usuario, persona);
        }

        [Authorize]
        [HttpDelete("Activos_Personas_Eliminar")]
        public ErrorDto Activos_Personas_Eliminar(int CodEmpresa, string identificacion, string usuario)
        {
            return _bl.Activos_Personas_Eliminar(CodEmpresa, identificacion, usuario);
        }

        [Authorize]
        [HttpGet("Activos_Personas_Valida")]
        public ErrorDto Activos_Personas_Valida(int CodEmpresa, string identificacion)
        {
            return _bl.Activos_Personas_Valida(CodEmpresa, identificacion);
        }

        [Authorize]
        [HttpPost("Activos_Personas_CambioDepto_Aplicar")]
        public ErrorDto<CambioDeptoResponse> Activos_Personas_CambioDepto_Aplicar(int CodEmpresa, string usuario, CambioDeptoRequest req)
        {
            return _bl.Activos_Personas_CambioDepto_Aplicar(CodEmpresa, usuario, req);
        }

        [Authorize]
        [HttpPost("Activos_Personas_SincronizarRH")]
        public ErrorDto Activos_Personas_SincronizarRH(int CodEmpresa, string usuario)
        {
            return _bl.Activos_Personas_SincronizarRH(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("Activos_Departamentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Departamentos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Secciones_ObtenerPorDepto")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_ObtenerPorDepto(int CodEmpresa, string cod_departamento)
        {
            return _bl.Activos_Secciones_ObtenerPorDepto(CodEmpresa, cod_departamento);
        }

        [Authorize]
        [HttpPost("Activos_Personas_BoletaActivosAsignados_Lote")]
        public ActionResult<ErrorDto<object>> BoletaActivosAsignados_Lote(int codEmpresa, ActivosPersonasReporteLoteRequest request)
        {
            return _bl.Activos_BoletaActivosAsignados_Lote(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("Activos_Personas_ContratoResponsabilidad_Lote")]
        public ActionResult<ErrorDto<object>> ContratoResponsabilidad_Lote(int codEmpresa, ActivosPersonasReporteLoteRequest request)
        {
            return _bl.Activos_ContratoResponsabilidad_Lote(codEmpresa, request);
        }
    }
}