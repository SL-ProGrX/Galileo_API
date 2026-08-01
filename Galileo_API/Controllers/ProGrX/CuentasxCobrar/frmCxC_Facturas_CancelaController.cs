using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCFacturasCancelaController : ControllerBase
    {
        private readonly FrmCxCFacturasCancelaBL _bl;

        public FrmCxCFacturasCancelaController(IConfiguration config)
        {
            _bl = new FrmCxCFacturasCancelaBL(config);
        }

        [Authorize]
        [HttpGet("CxCFacturasCancelaPagadores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaPagadores_Obtener(
            int codEmpresa,
            string cedulaCliente)
        {
            return _bl.CxCFacturasCancelaPagadores_Obtener(codEmpresa, cedulaCliente);
        }

        [Authorize]
        [HttpGet("CxCFacturasCancelaDivisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaDivisas_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaPagador)
        {
            return _bl.CxCFacturasCancelaDivisas_Obtener(codEmpresa, cedulaCliente, cedulaPagador);
        }

        [Authorize]
        [HttpGet("CxCFacturasCancelaFacturas_Obtener")]
        public ErrorDto<List<CxCFacturasCancelaPendienteDto>> CxCFacturasCancelaFacturas_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaPagador,
            string codDivisa)
        {
            return _bl.CxCFacturasCancelaFacturas_Obtener(codEmpresa, cedulaCliente, cedulaPagador, codDivisa);
        }

        [Authorize]
        [HttpGet("CxCFacturasCancelaTipoDocumento_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasCancelaTipoDocumento_Obtener(
            int codEmpresa,
            string codigoCaja)
        {
            return _bl.CxCFacturasCancelaTipoDocumento_Obtener(codEmpresa, codigoCaja);
        }

        [Authorize]
        [HttpPost("CxCFacturasCancelaFactura_Registrar")]
        public ErrorDto<bool> CxCFacturasCancelaFactura_Registrar(
            int codEmpresa,
            CxCFacturasCancelaFacturaRequestDto request)
        {
            return _bl.CxCFacturasCancelaFactura_Registrar(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("CxCFacturasCancelaAbono_Registrar")]
        public ErrorDto<bool> CxCFacturasCancelaAbono_Registrar(
            int codEmpresa,
            CxCFacturasCancelaAbonoRequestDto request)
        {
            return _bl.CxCFacturasCancelaAbono_Registrar(codEmpresa, request);
        }
    }
}
