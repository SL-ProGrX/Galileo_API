using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivDesembolsosController : ControllerBase
    {
        private readonly FrmVivDesembolsosBl _bl;

        public FrmVivDesembolsosController(IConfiguration config)
        {
            _bl = new FrmVivDesembolsosBl(config);
        }


        [Authorize]
        [HttpGet("Operaciones_Listar")]
        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _bl.Operaciones_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Lineas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            return _bl.Lineas_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Desembolso_Consultar")]
        public ErrorDto<VivDesembolsoHeaderDto> Desembolso_Consultar(int codEmpresa, int operacion)
        {
            return _bl.Desembolso_Consultar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Desembolsos_Listar")]
        public ErrorDto<List<VivDesembolsoDto>> Desembolsos_Listar(int codEmpresa, int operacion)
        {
            return _bl.Desembolsos_Listar(codEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("Pendientes_Listar")]
        public ErrorDto<List<VivDesembolsoPendienteDto>> Pendientes_Listar(int codEmpresa, int operacion)
        {
            return _bl.Pendientes_Listar(codEmpresa, operacion);
        }

        [HttpGet("Bancos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Bancos_Listar(int codEmpresa, string usuario)
        {
            return _bl.Bancos_Listar(codEmpresa, usuario);
        }

        [HttpGet("Cuentas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Listar(int codEmpresa, string bancoId)
        {
            return _bl.Cuentas_Listar(codEmpresa, bancoId);
        }

        [HttpGet("Conceptos_Listar")]
        public ErrorDto<List<ConceptoApiDto>> Conceptos_Listar(int codEmpresa)
        {
            return _bl.Conceptos_Listar(codEmpresa);
        }

        [HttpGet("PermiteDesembolso")]
        public ErrorDto<bool> PermiteDesembolso(int codEmpresa, int operacion, int index)
        {
            return _bl.PermiteDesembolso(codEmpresa, operacion, index);
        }

        [Authorize]
        [HttpPost("ActivarDesembolsoPendiente")]
        public ErrorDto<bool> ActivarDesembolsoPendiente(int codEmpresa, ActivarDesembolsoPendienteRequestDto request)
        {
            return _bl.ActivarDesembolsoPendiente(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("Pendiente_Cambiar")]
        public ErrorDto<CambioPendienteResponseDto> Pendiente_Cambiar(int codEmpresa, CambioPendienteRequestDto request)
        {
            return _bl.Pendiente_Cambiar(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("Pendiente_Agregar")]
        public ErrorDto<bool> Pendiente_Agregar(int codEmpresa, AgregarPendienteRequestDto request)
        {
            return _bl.Pendiente_Agregar(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("DesembolsoDetalle_Guardar")]
        public ErrorDto<bool> DesembolsoDetalle_Guardar(int codEmpresa, List<DesembolsoDetalleDto> detalles)
        {
            return _bl.DesembolsoDetalle_Guardar(codEmpresa, detalles);
        }

        [Authorize]
        [HttpPost("Desembolso_Guardar")]
        public ErrorDto<int> Desembolso_Guardar(int codEmpresa,ViviendaDesembolsoRequestDto request)
        {
            return _bl.Desembolso_Guardar(codEmpresa, request);
        }
    }
}
