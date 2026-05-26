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
    public class FrmCxCRemesasTesoreriaController : ControllerBase
    {
        private readonly FrmCxCRemesasTesoreriaBL _bl;

        public FrmCxCRemesasTesoreriaController(IConfiguration config)
        {
            _bl = new FrmCxCRemesasTesoreriaBL(config);
        }

        #region Remesas

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Remesas_Lista_Obtener")]
        public ErrorDto<CxCRemesasTesoreriaRemesaLista> CxC_RemesasTesoreria_Remesas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _bl.CxC_RemesasTesoreria_Remesas_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Remesas_Lista_Export")]
        public ErrorDto<CxCRemesasTesoreriaRemesaLista> CxC_RemesasTesoreria_Remesas_Lista_Export(int CodEmpresa, string parametros)
        {
            return _bl.CxC_RemesasTesoreria_Remesas_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Remesa_Obtener")]
        public ErrorDto<CxCRemesasTesoreriaRemesaData> CxC_RemesasTesoreria_Remesa_Obtener(int CodEmpresa, int tesoreriaRemesa)
        {
            return _bl.CxC_RemesasTesoreria_Remesa_Obtener(CodEmpresa, tesoreriaRemesa);
        }

        [Authorize]
        [HttpPost("CxC_RemesasTesoreria_Remesa_Guardar")]
        public ErrorDto<CxCRemesasTesoreriaRemesaData> CxC_RemesasTesoreria_Remesa_Guardar(int CodEmpresa, [FromBody] CxCRemesasTesoreriaRemesaGuardarRequest request)
        {
            return _bl.CxC_RemesasTesoreria_Remesa_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CxC_RemesasTesoreria_Remesa_Eliminar")]
        public ErrorDto CxC_RemesasTesoreria_Remesa_Eliminar(int CodEmpresa, int tesoreriaRemesa, string usuario)
        {
            return _bl.CxC_RemesasTesoreria_Remesa_Eliminar(CodEmpresa, tesoreriaRemesa, usuario);
        }

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Remesas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Remesas_Dropdown_Obtener(int CodEmpresa, string? estado)
        {
            return _bl.CxC_RemesasTesoreria_Remesas_Dropdown_Obtener(CodEmpresa, estado);
        }

        #endregion

        #region Carga

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Oficinas_Carga_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Oficinas_Carga_Dropdown_Obtener(int CodEmpresa, int tesoreriaRemesa)
        {
            return _bl.CxC_RemesasTesoreria_Oficinas_Carga_Dropdown_Obtener(CodEmpresa, tesoreriaRemesa);
        }

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Carga_Lista_Obtener")]
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Carga_Lista_Obtener(int CodEmpresa, int tesoreriaRemesa, string? codOficina, string parametros)
        {
            return _bl.CxC_RemesasTesoreria_Carga_Lista_Obtener(CodEmpresa, tesoreriaRemesa, codOficina, parametros);
        }

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Carga_Lista_Export")]
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Carga_Lista_Export(int CodEmpresa, int tesoreriaRemesa, string? codOficina, string parametros)
        {
            return _bl.CxC_RemesasTesoreria_Carga_Lista_Export(CodEmpresa, tesoreriaRemesa, codOficina, parametros);
        }

        [Authorize]
        [HttpPost("CxC_RemesasTesoreria_Carga_Aplicar")]
        public ErrorDto CxC_RemesasTesoreria_Carga_Aplicar(int CodEmpresa, [FromBody] CxCRemesasTesoreriaCargaAplicarRequest request)
        {
            return _bl.CxC_RemesasTesoreria_Carga_Aplicar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CxC_RemesasTesoreria_Carga_Cerrar")]
        public ErrorDto CxC_RemesasTesoreria_Carga_Cerrar(int CodEmpresa, [FromBody] CxCRemesasTesoreriaCerrarRequest request)
        {
            return _bl.CxC_RemesasTesoreria_Carga_Cerrar(CodEmpresa, request);
        }

        #endregion

        #region Traslado
        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Traslado_Lista_Obtener")]
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Traslado_Lista_Obtener(int CodEmpresa, int tesoreriaRemesa, string parametros)
        {
            return _bl.CxC_RemesasTesoreria_Traslado_Lista_Obtener(CodEmpresa, tesoreriaRemesa, parametros);
        }

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Traslado_Lista_Export")]
        public ErrorDto<CxCRemesasTesoreriaOperacionLista> CxC_RemesasTesoreria_Traslado_Lista_Export(int CodEmpresa, int tesoreriaRemesa, string parametros)
        {
            return _bl.CxC_RemesasTesoreria_Traslado_Lista_Export(CodEmpresa, tesoreriaRemesa, parametros);
        }

        [Authorize]
        [HttpPost("CxC_RemesasTesoreria_Traslado_Aplicar")]
        public ErrorDto CxC_RemesasTesoreria_Traslado_Aplicar(int CodEmpresa, [FromBody] CxCRemesasTesoreriaTrasladoAplicarRequest request)
        {
            return _bl.CxC_RemesasTesoreria_Traslado_Aplicar(CodEmpresa, request);
        }
        #endregion

        #region Reportes

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Oficinas_Reporte_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_RemesasTesoreria_Oficinas_Reporte_Dropdown_Obtener(int CodEmpresa, DateTime fechaInicio, DateTime fechaCorte, bool todasFechas)
        {
            return _bl.CxC_RemesasTesoreria_Oficinas_Reporte_Dropdown_Obtener(CodEmpresa, fechaInicio, fechaCorte, todasFechas);
        }

        #endregion

        #region Reactivacion

        [Authorize]
        [HttpGet("CxC_RemesasTesoreria_Reactivacion_Operacion_Obtener")]
        public ErrorDto<CxCRemesasTesoreriaReactivacionDto> CxC_RemesasTesoreria_Reactivacion_Operacion_Obtener(int CodEmpresa, int operacion)
        {
            return _bl.CxC_RemesasTesoreria_Reactivacion_Operacion_Obtener(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("CxC_RemesasTesoreria_Reactivacion_Aplicar")]
        public ErrorDto CxC_RemesasTesoreria_Reactivacion_Aplicar(int CodEmpresa, [FromBody] CxCRemesasTesoreriaReactivacionAplicarRequest request)
        {
            return _bl.CxC_RemesasTesoreria_Reactivacion_Aplicar(CodEmpresa, request);
        }

        #endregion
    }
}