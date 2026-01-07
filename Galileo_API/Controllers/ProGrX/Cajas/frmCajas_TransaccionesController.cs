using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasTransaccionesController : ControllerBase
    {
        private readonly FrmCajasTransaccionesBL _bl;

        public FrmCajasTransaccionesController(IConfiguration config)
        {
            _bl = new FrmCajasTransaccionesBL(config);
        }

        [Authorize]
        [HttpGet("CajasTransacciones_Socios_Obtener")]
        public ErrorDto<List<CajasSocioResult>> CajasTransacciones_Socios_Obtener(int codEmpresa)
        {
            return _bl.CajasTransacciones_Socios_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("CajasTransacciones_Servicios_Obtener")]
        public ErrorDto<List<CajasServicioResult>> CajasTransacciones_Servicios_Obtener(int codEmpresa, [FromBody] CajasServicioConsultaParams param)
        {
            return _bl.CajasTransacciones_Servicios_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpGet("CajasTransacciones_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasTransacciones_Divisas_Obtener(int codEmpresa, string codContabilidad)
        {
            return _bl.CajasTransacciones_Divisas_Obtener(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet("CajasTransacciones_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasTransacciones_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return _bl.CajasTransacciones_Documentos_Obtener(codEmpresa, codCaja);
        }

        [Authorize]
        [HttpPost("CajasTransacciones_Validacion")]
        public ErrorDto<CajasTransacValidacionResult?> CajasTransacciones_Validacion(int codEmpresa, [FromBody] CajasTransacValidacionParams param)
        {
            return _bl.CajasTransacciones_Validacion(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasTransacciones_ServiciosDatos")]
        public ErrorDto<CajasServiciosDatosResult?> CajasTransacciones_ServiciosDatos(int codEmpresa, [FromBody] CajasServiciosDatosParams param)
        {
            return _bl.CajasTransacciones_ServiciosDatos(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("SifTransacciones_Insertar")]
        public ErrorDto<bool> SifTransacciones_Insertar(int codEmpresa, [FromBody] SifTransaccionInsertParams param)
        {
            return _bl.SifTransacciones_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasServiciosTransac_Insertar")]
        public ErrorDto<bool> CajasServiciosTransac_Insertar(int codEmpresa, [FromBody] CajasServiciosTransacInsertParams param)
        {
            return _bl.CajasServiciosTransac_Insertar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("SifDocsAsiento_Ejecutar")]
        public ErrorDto<SifDocsAsientoResult> SifDocsAsiento_Ejecutar(int codEmpresa, [FromBody] SifDocsAsientoParams param)
        {
            return _bl.SifDocsAsiento_Ejecutar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasDesglocePagosDocFinal_Ejecutar")]
        public ErrorDto<bool> CajasDesglocePagosDocFinal_Ejecutar(int codEmpresa, [FromBody] CajasDesglocePagosDocFinalParams param)
        {
            return _bl.CajasDesglocePagosDocFinal_Ejecutar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasIntercambioRegistra")]
        public ErrorDto<bool> CajasIntercambioRegistra(int codEmpresa, [FromBody] CajasIntercambioRegistraParams param)
        {
            return _bl.CajasIntercambioRegistra(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasValoresTransitoRegistra")]
        public ErrorDto<bool> CajasValoresTransitoRegistra(int codEmpresa, [FromBody] CajasValoresTransitoRegistraParams param)
        {
            return _bl.CajasValoresTransitoRegistra(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasGeneralTE_Ejecutar")]
        public ErrorDto<bool> CajasGeneralTE_Ejecutar(int codEmpresa, [FromBody] CajasGeneralTEParams param)
        {
            return _bl.CajasGeneralTE_Ejecutar(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CajasReciboDigital")]
        public ErrorDto<bool> CajasReciboDigital(int codEmpresa, [FromBody] CajasReciboDigitalParams param)
        {
            return _bl.CajasReciboDigital(codEmpresa, param);
        }
    }

}
