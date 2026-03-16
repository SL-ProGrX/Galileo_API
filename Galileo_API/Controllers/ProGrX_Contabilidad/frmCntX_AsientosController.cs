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
    public class FrmCntXAsientosController : ControllerBase
    {
        private readonly FrmCntXAsientosBl _bl;

        public FrmCntXAsientosController(IConfiguration config) 
            => _bl = new FrmCntXAsientosBl(config);

        [HttpGet("CntXAsientos_Obtener")]
        public ErrorDto<CntXAsientoData?> CntXAsientos_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            return _bl.CntXAsientos_Obtener(codEmpresa, codConta, tipoAsiento, numAsiento);
        }

        [HttpGet("CntXAsientos_Detalle_Obtener")]
        public ErrorDto<List<CntXAsientoDetalleData>> CntXAsientos_Detalle_Obtener(int codEmpresa, int codConta, string tipoAsiento, string numAsiento)
        {
            return _bl.CntXAsientos_Detalle_Obtener(codEmpresa, codConta, tipoAsiento, numAsiento);
        }

        [HttpGet("CntXAsientos_Scroll_Obtener")]
        public ErrorDto<CntXAsientoData?> CntXAsientos_Scroll_Obtener(
            int codEmpresa, int codConta, int anio, int mes, string tipoAsiento, string numAsiento, int scrollCode)
        {
            return _bl.CntXAsientos_Scroll_Obtener(codEmpresa, codConta, anio, mes, tipoAsiento, numAsiento, scrollCode);
        }

        [HttpGet("CntXTiposAsientos_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXTiposAsientos_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXTiposAsientos_Lista_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXTiposAsientos_Descripcion_Obtener")]
        public ErrorDto<string?> CntXTiposAsientos_Descripcion_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _bl.CntXTiposAsientos_Descripcion_Obtener(codEmpresa, codConta, tipoAsiento);
        }

    }
}