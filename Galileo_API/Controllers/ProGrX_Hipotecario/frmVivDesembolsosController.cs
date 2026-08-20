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

        [HttpPost("ConsultaDesembolso_Listar")]
        public ErrorDto<List<ConsultaDesembolsoDto>> ConsultaDesembolso_Listar(
            int codEmpresa,
            ConsultaDesembolsoRequestDto request)
        {
            return _bl.ConsultaDesembolso_Listar(codEmpresa, request);
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

        [Authorize]
        [HttpGet("Bancos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Bancos_Listar(int codEmpresa, string usuario)
        {
            return _bl.Bancos_Listar(codEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("Cuentas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Listar(int codEmpresa, string bancoId)
        {
            return _bl.Cuentas_Listar(codEmpresa, bancoId);
        }

        [Authorize]
        [HttpGet("Conceptos_Listar")]
        public ErrorDto<List<ConceptoApiDto>> Conceptos_Listar(int codEmpresa)
        {
            return _bl.Conceptos_Listar(codEmpresa);
        }

        [Authorize]
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
        public ErrorDto<int> Desembolso_Guardar(int codEmpresa, ViviendaDesembolsoRequestDto request)
        {
            return _bl.Desembolso_Guardar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("Cedulas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cedulas_Listar(int codEmpresa)
        {
            return _bl.Cedulas_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Contactos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Contactos_Listar(int codEmpresa,string tipo)
        {
            return _bl.Contactos_Listar(codEmpresa, tipo);
        }

        [Authorize]
        [HttpGet("TiposDesembolso_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposDesembolso_Listar(int codEmpresa)
        {
            return _bl.TiposDesembolso_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Garantias_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Garantias_Listar(int codEmpresa,int operacion)
        {
            return _bl.Garantias_Listar(codEmpresa, operacion);
        }


    }
}
