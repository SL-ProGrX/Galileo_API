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
        public ErrorDto<CntXAsientoData?> CntXAsientos_Scroll_Obtener(int codEmpresa, string request, int scrollCode)
        {
            return _bl.CntXAsientos_Scroll_Obtener(codEmpresa, request, scrollCode);
        }

        [HttpGet("CntXAsientos_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Lista_Obtener(int codEmpresa, int codConta, string tipoAsiento, bool periodoActual, int anio, int mes)
        {
            return _bl.CntXAsientos_Lista_Obtener(codEmpresa, codConta, tipoAsiento, periodoActual, anio, mes);
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

        [HttpGet("CntXCentroCostosporUnidad_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCentroCostosporUnidad_Lista_Obtener(int codEmpresa, int codConta, string codUnidad)
        {
            return _bl.CntXCentroCostosporUnidad_Lista_Obtener(codEmpresa, codConta, codUnidad);
        }

        [HttpGet("CntXAsientos_Consecutivo_Obtener")]
        public ErrorDto<string?> CntXAsientos_Consecutivo_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _bl.CntXAsientos_Consecutivo_Obtener(codEmpresa, codConta, tipoAsiento);
        }

        [HttpPost("CntXAsientos_Guardar")]
        public ErrorDto CntXAsientos_Guardar(int codEmpresa, string usuario, bool edita, CntXAsientoGuardarRequest request)
        {
            return _bl.CntXAsientos_Guardar(codEmpresa, usuario, edita, request);
        }

        [HttpDelete("CntXAsientos_Eliminar")]
        public ErrorDto CntXAsientos_Eliminar(int codEmpresa, int codConta, string tipoAsiento, string numAsiento, string ts, string usuario)
        {
            return _bl.CntXAsientos_Eliminar(codEmpresa, codConta, tipoAsiento, numAsiento, ts, usuario);
        }

        [HttpPost("CntXAsientos_Autorizar")]
        public ErrorDto CntXAsientos_Autorizar(int codEmpresa, int codConta, string tipoAsiento, string numAsiento, string usuario)
        {
            return _bl.CntXAsientos_Autorizar(codEmpresa, codConta, tipoAsiento, numAsiento, usuario);
        }

        [HttpPost("CntXAsientos_Copiar")]
        public ErrorDto CntXAsientos_Copiar(int codEmpresa, string usuario, CntXAsientoCopiarRequest request)
        {
            return _bl.CntXAsientos_Copiar(codEmpresa, usuario, request);
        }

        [HttpPost("CntXAsientos_Reversar")]
        public ErrorDto CntXAsientos_Reversar(int codEmpresa, CntXAsientoData request)
        {
            return _bl.CntXAsientos_Reversar(codEmpresa, request);
        }

        [HttpPost("CntXAsientos_Mayorizar")]
        public ErrorDto CntXAsientos_Mayorizar(int codEmpresa, CntXAsientoData request)
        {
            return _bl.CntXAsientos_Mayorizar(codEmpresa, request);
        }

        [HttpGet("CntXAsientos_FxNotaCuenta_Obtener")]
        public ErrorDto CntXAsientos_FxNotaCuenta_Obtener(int codEmpresa, int codConta, string vCuenta, int anio, int mes)
        {
            return _bl.CntXAsientos_FxNotaCuenta_Obtener(codEmpresa, codConta, vCuenta, anio, mes);
        }
    }
}