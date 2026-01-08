using Galileo.Models.ERROR;
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
        [HttpGet("Cajas_AM_Validar")]
        public ErrorDto<CajasAmValidacionDto> Cajas_AM_Validar(
            int codEmpresa,
            string codCaja,
            int codApertura,
            int sesionId,
            string usuario,
            decimal monto,
            string tiquete)
        {
            return BL_Cajas_AM.Cajas_AM_Validar(
                codEmpresa, codCaja, codApertura,
                sesionId, usuario, monto, tiquete
            );
        }


        [Authorize]
        [HttpGet("Cajas_AM_Creditos_Pendientes")]
        public ErrorDto<List<CajasCreditoPendienteDto>> Cajas_AM_Creditos_Pendientes(
            int codEmpresa, CajasAMCreditosPendientesRequestDto request)
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
    }
}