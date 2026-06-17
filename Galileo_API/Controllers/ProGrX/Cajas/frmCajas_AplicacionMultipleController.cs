using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasAplicacionMultipleController : ControllerBase
    {
        private readonly FrmCajasAplicacionMultipleBl BL_Cajas_AM;

        public FrmCajasAplicacionMultipleController(IConfiguration config)
        {
            BL_Cajas_AM = new FrmCajasAplicacionMultipleBl(config);
        }

        [Authorize]
        [HttpGet("Cajas_AM_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_AM_Documentos_Obtener(
            int codEmpresa,
            string codCaja)
        {
            return BL_Cajas_AM.Cajas_AM_Documentos_Obtener(codEmpresa, codCaja);
        }

        [Authorize]
        [HttpGet("Cajas_AM_ClienteInicial_Obtener")]
        public ErrorDto<CajasAmClienteInicialDto> Cajas_AM_ClienteInicial_Obtener(
            int codEmpresa,
            string cedula)
        {
            return BL_Cajas_AM.Cajas_AM_ClienteInicial_Obtener(codEmpresa, cedula);
        }

   
        [Authorize]
        [HttpPost("Cajas_AM_Validar")]
        public ErrorDto<CajasAmValidacionDto> Cajas_AM_Validar(
            int codEmpresa,
            [FromBody] CajasAMValidarRequestDto request)
        {
            return BL_Cajas_AM.Cajas_AM_Validar(
                codEmpresa,
                request.codcaja ?? string.Empty,
                request.codapertura ?? 0,
                request.sesionid ?? 0,
                request.usuario ?? string.Empty,
                request.monto ?? 0,
                request.tiquete ?? string.Empty
            );
        }


        [Authorize]
        [HttpPost("Cajas_AM_Creditos_Pendientes")]
        public ErrorDto<List<CajasCreditoPendienteDto>> Cajas_AM_Creditos_Pendientes(
            int codEmpresa,
            [FromBody] CajasAMCreditosPendientesRequestDto request)
        {
            return BL_Cajas_AM.Cajas_AM_Creditos_Pendientes(
                codEmpresa,request
            );
        }

 
        [Authorize]
        [HttpPost("Cajas_AM_Creditos_Agregar")]
        public ErrorDto<bool> Cajas_AM_Creditos_Agregar(
            int codEmpresa,
            [FromBody] List<CajasAmAgregarRequestDto> items)
        {
            return BL_Cajas_AM.Cajas_AM_Creditos_Agregar(
                codEmpresa, items
            );
        }

  
        [Authorize]
        [HttpPost("Cajas_AM_Eliminar")]
        public ErrorDto<bool> Cajas_AM_Eliminar(
            int codEmpresa,
            [FromBody] List<long> ids)
        {
            return BL_Cajas_AM.Cajas_AM_Eliminar(codEmpresa, ids);
        }

        [Authorize]
        [HttpPost("Cajas_AM_Aplicar")]
        public ErrorDto<long> Cajas_AM_Aplicar(
            int codEmpresa,
            [FromBody] CajasAmAplicarRequestDto request)
        {
            return BL_Cajas_AM.Cajas_AM_Aplicar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("Cajas_AM_Seleccionados")]
        public ErrorDto<List<CajasAmSeleccionadoDto>> Cajas_AM_Seleccionados(
            int codEmpresa,
            string cedula,
            string codCaja,
            int codApertura,
            string tiquete)
        {
            return BL_Cajas_AM.Cajas_AM_Seleccionados(
                codEmpresa, cedula, codCaja, codApertura, tiquete
            );
        }
    }
}
