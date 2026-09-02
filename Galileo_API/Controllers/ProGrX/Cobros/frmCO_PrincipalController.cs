using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOPrincipalController : ControllerBase
    {
        private readonly FrmCOPrincipalBL _bl;

        public FrmCOPrincipalController(IConfiguration config)
            => _bl = new FrmCOPrincipalBL(config);

        [Authorize]
        [HttpGet("Operaciones_Listar")]
        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _bl.Operaciones_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Operaciones_Obtener")]
        public ErrorDto<OperacionBusquedaListaDto> Operaciones_Obtener(int codEmpresa, string filtros)
        {
            return _bl.Operaciones_Obtener(codEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Operacion_Consultar")]
        public ErrorDto<OperacionConsultarDto> Operacion_Consultar(int codEmpresa, int operacion)
        {
            return _bl.Operacion_Consultar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Deductoras_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Deductoras_Listar(int CodEmpresa, int codInstitucion)
        {
            return _bl.Deductoras_Listar(CodEmpresa, codInstitucion);
        }

        [Authorize]
        [HttpGet("Estado_Consultar")]
        public ErrorDto<CoEstadoDto> Estado_Consultar(int codEmpresa, int operacion, DateTime? fechaCorte)
        {
            return _bl.Estado_Consultar(codEmpresa, operacion, fechaCorte);
        }

        [Authorize]
        [HttpGet("Historial_Listar")]
        public ErrorDto<List<CoHistorialDto>> Historial_Listar(int codEmpresa, int operacion)
        {
            return _bl.Historial_Listar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Gestiones_Listar")]
        public ErrorDto<List<COGestionDto>> Gestiones_Listar(int codEmpresa, string cedula)
        {
            return _bl.Gestiones_Listar(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("CobroFiadores_Listar")]
        public ErrorDto<List<COCobroFiadorRowDto>> CobroFiadores_Listar(int codEmpresa, int operacion)
        {
            return _bl.CobroFiadores_Listar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("CobroFiador_Cancelar")]
        public ErrorDto<string> CobroFiador_Cancelar(int codEmpresa, int operacion, string usuario)
        {
            return _bl.CobroFiador_Cancelar(codEmpresa, operacion, usuario);
        }

        [Authorize]
        [HttpGet("TrasladoDeuda_Listar")]
        public ErrorDto<List<COTrasladoDeudaRowDto>> TrasladoDeuda_Listar(int codEmpresa, int operacion)
        {
            return _bl.TrasladoDeuda_Listar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("TrasladoDeuda_Revertir")]
        public ErrorDto<string> TrasladoDeuda_Revertir(int codEmpresa, [FromBody] COTrasladoDeudaRevertirRequestDto request)
        {
            return _bl.TrasladoDeuda_Revertir(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("Contacto_Consultar")]
        public ErrorDto<COContactoDto> Contacto_Consultar(int codEmpresa, int operacion)
        {
            return _bl.Contacto_Consultar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Mora_Listar")]
        public ErrorDto<List<COMoraDto>> Mora_Listar(int codEmpresa, int operacion, string tipo)
        {
            return _bl.Mora_Listar(codEmpresa, operacion, tipo);
        }

        [Authorize]
        [HttpGet("Ejecutivos_Listar")]
        public ErrorDto<List<COEjecutivoDto>> Ejecutivos_Listar(int codEmpresa, int operacion)
        {
            return _bl.Ejecutivos_Listar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Lineas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            return _bl.Lineas_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Personas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Personas_Listar(int codEmpresa)
        {
            return _bl.Personas_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("LineasPorPersona_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> LineasPorPersona_Listar(int codEmpresa, string cedula)
        {
            return _bl.LineasPorPersona_Listar(codEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("OperacionesPorPersonaLinea_Listar")]
        public ErrorDto<List<OperacionBusquedaDto>> OperacionesPorPersonaLinea_Listar(int codEmpresa, string cedula, string linea)
        {
            return _bl.OperacionesPorPersonaLinea_Listar(codEmpresa, cedula, linea);
        }

        [Authorize]
        [HttpGet("OperacionesPorPersona_Listar")]
        public ErrorDto<List<OperacionBusquedaDto>> OperacionesPorPersona_Listar(int codEmpresa, string cedula)
        {
            return _bl.OperacionesPorPersona_Listar(codEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("CambiarDeductora")]
        public ErrorDto<string> CambiarDeductora(int codEmpresa, COCambiarDeductoraRequestDto request)
        {
            return _bl.CambiarDeductora(
                codEmpresa,
                request.operacion,
                request.deductora
            );
        }

        [Authorize]
        [HttpPost("ValidarCongelamiento")]
        public ErrorDto<bool> ValidarCongelamiento(int codEmpresa, COValidarCongelamientoRequestDto request)
        {
            return _bl.ValidarCongelamiento(codEmpresa, request.cedula, request.tipo
            );
        }

        [Authorize]
        [HttpPost("ValidarPasoCobroJudicial")]
        public ErrorDto<bool> ValidarPasoCobroJudicial(int codEmpresa, int operacion)
        {
            return _bl.ValidarPasoCobroJudicial(codEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("CobroJudicial_Ejecutar")]
        public ErrorDto<string> CobroJudicial_Ejecutar(int codEmpresa, CobroJudicialRequestDto request)
        {
            return _bl.CobroJudicial_Ejecutar(
                codEmpresa,
                request.operacion,
                request.usuario,
                request.notas
            );
        }

        [HttpGet("Avisos_Listar")]
        public ActionResult<ErrorDto<List<COAvisoDto>>> Avisos_Listar(int codEmpresa, int operacion)
        {
            return _bl.Avisos_Listar(codEmpresa, operacion);
        }
    }
}
