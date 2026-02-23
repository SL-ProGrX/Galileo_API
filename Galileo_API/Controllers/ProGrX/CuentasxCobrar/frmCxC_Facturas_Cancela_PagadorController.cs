
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
    public class FrmCxCFacturasCancelaPagadorController : ControllerBase
    {
        private readonly FrmCxCFacturasCancelaPagadorBl _bl;

        public FrmCxCFacturasCancelaPagadorController(IConfiguration config) => _bl = new FrmCxCFacturasCancelaPagadorBl(config);

        [HttpGet("CxCFactCancPag_TipoDoc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            return _bl.CxCFactCancPag_TipoDoc_Obtener(codEmpresa, caja);
        }

        [HttpGet("CxCFactCancPag_Pagadores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_Pagadores_Obtener(int codEmpresa)
        {
            return _bl.CxCFactCancPag_Pagadores_Obtener(codEmpresa);
        }

        [HttpGet("CxCFactCancPag_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFactCancPag_Divisas_Obtener(int codEmpresa, string codPagador)
        {
            return _bl.CxCFactCancPag_Divisas_Obtener(codEmpresa, codPagador);
        }

        [HttpGet("CxCFactCancPag_FacturasPendientes_Obtener")]
        public ErrorDto<List<CxCFactPendienteCancelacionData>> CxCFactCancPag_FacturasPendientes_Obtener(int codEmpresa, CxCFactCancPagFacturasRequest filtro)
        {
            return _bl.CxCFactCancPag_FacturasPendientes_Obtener(codEmpresa, filtro);
        }

        [HttpPost("CxCFactCancPag_Abono_Registrar")]
        public ErrorDto CxCFactCancPag_Abono_Registrar(int codEmpresa, CxCFactCancPagRegistrarAbonoRequest request)
        {
            return _bl.CxCFactCancPag_Abono_Registrar(codEmpresa, request);
        }
    }
}