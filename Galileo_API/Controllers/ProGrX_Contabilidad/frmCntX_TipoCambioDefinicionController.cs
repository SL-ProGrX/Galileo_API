using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXTipoCambioDefinicionController : ControllerBase
    {
        private readonly FrmCntXTipoCambioDefinicionBl _bl;

        public FrmCntXTipoCambioDefinicionController(IConfiguration config) => 
            _bl = new FrmCntXTipoCambioDefinicionBl(config);

        [HttpGet("CntXDivisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDivisas_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXTipoCambio_Obtener")]
        public ErrorDto<List<CntXTipoCambioData>> CntXTipoCambio_Obtener(int codEmpresa, int codConta, string codDivisa, int lineas)
        {
            return _bl.CntXTipoCambio_Obtener(codEmpresa, codConta, codDivisa, lineas);
        }

        [HttpPost("CntXTipoCambio_Guardar")]
        public ErrorDto CntXTipoCambio_Guardar(int codEmpresa, int codConta, string usuario, CntXTipoCambioData request)
        {
            return _bl.CntXTipoCambio_Guardar(codEmpresa, codConta, usuario, request);
        }

        [HttpDelete("CntXTipoCambio_Eliminar")]
        public ErrorDto CntXTipoCambio_Eliminar(int codEmpresa, int codConta, string usuario, string codDivisa, int idCambio)
        {
            return _bl.CntXTipoCambio_Eliminar(codEmpresa, codConta, usuario, codDivisa, idCambio);
        }
    }
}