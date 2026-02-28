using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXExploradorContableController : ControllerBase
    {
        private readonly FrmCntXExploradorContableBl _bl;

        public FrmCntXExploradorContableController(IConfiguration config) => _bl = new FrmCntXExploradorContableBl(config);

        [HttpGet("Cuentas")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas(int codEmpresa)
        {
            return _bl.Cuentas_Obtener(codEmpresa);
        }

        [HttpGet("TiposAsiento")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento(int codEmpresa, int cod_contabilidad)
        {
            return _bl.TiposAsiento_Obtener(codEmpresa, cod_contabilidad);
        }

        [HttpGet("Periodos")]
        public ErrorDto<List<CntxPeriodoDto>> Periodos(int codEmpresa, string estado) // "P"|"C"
        {
            return _bl.Periodos_Obtener(codEmpresa, estado);
        }

        [HttpPost("ListarAsientos")]
        public ErrorDto<List<CntxAsientoRsmDto>> ListarAsientos(int codEmpresa, int cod_contabilidad, [FromBody] CntxExploradorFiltrosDto filtros)
        {
            return _bl.Asientos_Listar(codEmpresa, cod_contabilidad, filtros);
        }

        [HttpPost("AsientoDetalle")]
        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle(int codEmpresa, [FromBody] CntxExploradorFiltrosDto filtros)
        {
            return _bl.AsientoDetalle_Listar(codEmpresa, filtros);
        }

        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
        {
            return _bl.FechaServidor_Obtener(codEmpresa);
        }

        #region CATALOGO CUENTAS

        [HttpGet("CuentasPorPadre")]
        public ErrorDto<List<CntxCuentaDto>> CuentasPorPadre(
            int codEmpresa,
            string? codCuentaPadre)
        {
            return _bl.CuentasPorPadre(codEmpresa, codCuentaPadre);
        }

        #endregion

        #region AREAS

        #endregion

        #region PLANTILLAS

        #endregion



        [HttpGet("AsientosTreePorTipo")]
        public ActionResult<ErrorDto<List<CntxAsientoTreeDto>>> AsientosTreePorTipo(int codEmpresa, int cod_contabilidad, string tipo, int anio, int mes)
        {
            var result = _bl.AsientosTreePorTipo(codEmpresa, cod_contabilidad, tipo, anio, mes);

            return Ok(result);
        }

        [HttpGet("TiposCuenta")]
        public ErrorDto<List<CntxTipoCuentaDto>> TiposCuenta(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Cntx_TiposCuenta_Obtener(codEmpresa, cod_contabilidad);
        }

        [HttpGet("CuentasRaizPorTipo")]
        public ErrorDto<List<CntxCuentaDto>> CuentasRaizPorTipo(int codEmpresa, int cod_contabilidad, string tipoCuenta)
        {
            return _bl.Cntx_CuentasRaizPorTipo_Obtener(
                codEmpresa,
                cod_contabilidad,
                tipoCuenta);
        }


        // ============================================
        // OBTENER DIFERIDOS
        // ============================================
        [HttpGet("diferidos")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Diferidos(int codEmpresa, int codContabilidad)
        {
            var result = _bl.Diferidos_Obtener(codEmpresa, codContabilidad);

            return Ok(result);
        }

        // ============================================
        // OBTENER PLANTILLAS POR DIFERIDO
        // ============================================
        [HttpGet("diferidos/plantillas")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Plantillas(int codEmpresa, int codContabilidad, int codDiferido)
        {
            var result = _bl.DiferidoPlantillas_Obtener(codEmpresa, codContabilidad, codDiferido);

            return Ok(result);
        }

        // ============================================
        // OBTENER HISTÓRICO DE DIFERIDO
        // ============================================
        [HttpGet("diferidos/historico")]
        public ActionResult<ErrorDto<List<CntxDiferidoHistoricoDto>>> Historico(int codEmpresa, int codContabilidad, int codDiferido, int codPlantilla)
        {
            var result = _bl.DiferidoHistorico_Obtener(
                codEmpresa,
                codContabilidad,
                codDiferido,
                codPlantilla);

            return Ok(result);
        }


        [HttpGet("TiposAsientos_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
          => _bl.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);

        [HttpGet("Unidades_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(int codEmpresa, int cod_contabilidad)
            => _bl.Cntx_Unidades_Buscar(codEmpresa, cod_contabilidad);

        [HttpGet("CentroCosto_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Buscar(int codEmpresa, int cod_contabilidad)
            => _bl.Cntx_CentroCosto_Buscar(codEmpresa, cod_contabilidad);

        [HttpGet("Divisas_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Buscar(int codEmpresa, int cod_contabilidad)
            => _bl.Cntx_Divisas_Buscar(codEmpresa, cod_contabilidad);


        [HttpGet("Asientos_Resumen")]
        public ErrorDto<List<CntxAsientoResumenDto>> Asientos_Resumen(int codEmpresa,int cod_contabilidad,int anio,int mes)
        {
            return _bl.Asientos_Resumen(codEmpresa,cod_contabilidad,anio,mes
            );
        }

    }
}
