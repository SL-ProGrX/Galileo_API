using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Galileo.Models.TES;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfCdRemesasComitesController : ControllerBase
    {
        private readonly FrmAfCdRemesasComitesBL _bl;

        public FrmAfCdRemesasComitesController(IConfiguration config)
        {
            _bl = new FrmAfCdRemesasComitesBL(config);
        }

        [HttpGet("AfCdRemesasTes_Lista")]
        public ActionResult<ErrorDto<List<AfCdRemesaTesDto>>> AfCdRemesasTes_Lista([FromQuery] int codEmpresa)
            => _bl.AfCdRemesasTes_Lista(codEmpresa);

        [HttpPost("AfCdRemesasTes_Guardar")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_Guardar([FromQuery] int codEmpresa, [FromBody] AfCdRemesaTesSaveDto dto)
            => _bl.AfCdRemesasTes_Guardar(codEmpresa, dto);

        [HttpDelete("AfCdRemesasTes_Eliminar")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_Eliminar([FromQuery] int codEmpresa, [FromQuery] int codRemesa)
            => _bl.AfCdRemesasTes_Eliminar(codEmpresa, codRemesa);

        [HttpGet("AfCdRemesasTes_ActivasPendientes")]
        public ActionResult<ErrorDto<List<AfCdRemesaTesDto>>> AfCdRemesasTes_ActivasPendientes([FromQuery] int codEmpresa)
            => _bl.AfCdRemesasTes_ActivasPendientes(codEmpresa);

        [HttpGet("AfCdRemesasTes_Fechas")]
        public ActionResult<ErrorDto<AfCdRemesaTesFechasDto>> AfCdRemesasTes_Fechas([FromQuery] int codEmpresa, [FromQuery] int codRemesa)
            => _bl.AfCdRemesasTes_Fechas(codEmpresa, codRemesa);

        [HttpGet("AfCdRemesasTes_BancosPorFechas")]
        public ActionResult<ErrorDto<List<AfCdBancoDto>>> AfCdRemesasTes_BancosPorFechas(
            [FromQuery] int codEmpresa,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaCorte)
            => _bl.AfCdRemesasTes_BancosPorFechas(codEmpresa, fechaInicio, fechaCorte);

        [HttpGet("AfCdRemesasTes_OperacionesPorBanco")]
        public ActionResult<ErrorDto<List<AfCdCuentaOperacionDto>>> AfCdRemesasTes_OperacionesPorBanco(
            [FromQuery] int codEmpresa,
            [FromQuery] int idBanco,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaCorte)
            => _bl.AfCdRemesasTes_OperacionesPorBanco(codEmpresa, idBanco, fechaInicio, fechaCorte);

        [HttpGet("AfCdRemesasTes_ActividadesPorOperacion")]
        public ActionResult<ErrorDto<List<AfCdCuentaActividadDto>>> AfCdRemesasTes_ActividadesPorOperacion(
            [FromQuery] int codEmpresa,
            [FromQuery] int noperacion)
            => _bl.AfCdRemesasTes_ActividadesPorOperacion(codEmpresa, noperacion);

        [HttpGet("AfCdRemesasTes_ObtenerEstado")]
        public ActionResult<ErrorDto<AfCdRemesaEstadoDto>> AfCdRemesasTes_ObtenerEstado(
            [FromQuery] int codEmpresa,
            [FromQuery] int codRemesa,
            [FromQuery] string estado)
            => _bl.AfCdRemesasTes_ObtenerEstado(codEmpresa, codRemesa, estado);

        [HttpGet("AfCdRemesasTes_ObtenerRemesaPorBanco")]
        public ActionResult<ErrorDto<AfCdCuentaRemesaDto>> AfCdRemesasTes_ObtenerRemesaPorBanco(
            [FromQuery] int codEmpresa,
            [FromQuery] int codRemesa,
            [FromQuery] int idBanco)
            => _bl.AfCdRemesasTes_ObtenerRemesaPorBanco(codEmpresa, codRemesa, idBanco);

        [HttpPost("AfCdRemesasTes_CuentaRemesaSp")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_CuentaRemesaSp(
            [FromQuery] int codEmpresa,
            [FromBody] AfCdCuentaRemesaSpParams param)
            => _bl.AfCdRemesasTes_CuentaRemesaSp(codEmpresa, param);

        [HttpPost("AfCdRemesasTes_ActualizarEstado")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_ActualizarEstado(
            [FromQuery] int codEmpresa,
            [FromQuery] int codRemesa,
            [FromQuery] string estado)
            => _bl.AfCdRemesasTes_ActualizarEstado(codEmpresa, codRemesa, estado);

        [HttpPost("AfCdCuentas_ActualizarEstadoPorRemesa")]
        public ActionResult<ErrorDto<bool>> AfCdCuentas_ActualizarEstadoPorRemesa(
            [FromQuery] int codEmpresa,
            [FromQuery] int codRemesa,
            [FromQuery] string estado)
            => _bl.AfCdCuentas_ActualizarEstadoPorRemesa(codEmpresa, codRemesa, estado);

        [HttpPost("AfCdRemesasTes_Filtradas")]
        public ActionResult<ErrorDto<List<AfCdRemesaTesDto>>> AfCdRemesasTes_Filtradas(
            [FromQuery] int codEmpresa,
            [FromBody] AfCdRemesaTesFiltroParams filtro)
            => _bl.AfCdRemesasTes_Filtradas(codEmpresa, filtro);

        [HttpPost("AfCdRemesasComiteDetalle_Lista")]
        public ActionResult<ErrorDto<List<AfCdRemesaComiteDetalleDto>>> AfCdRemesasComiteDetalle_Lista(
            [FromQuery] int codEmpresa,
            [FromBody] AfCdRemesaComiteDetalleParams param)
            => _bl.AfCdRemesasComiteDetalle_Lista(codEmpresa, param);
        
        [HttpGet("AfCdRemesasTes_ResumenCerradas")]
        public ActionResult<ErrorDto<List<AfCdRemesaResumenDto>>> AfCdRemesasTes_ResumenCerradas(
            [FromQuery] int codEmpresa)
            => _bl.AfCdRemesasTes_ResumenCerradas(codEmpresa);
        
        [HttpGet("AfCdRemesasTes_DetallePorRemesa")]
        public ActionResult<ErrorDto<List<AfCdRemesaDetalleDto>>> AfCdRemesasTes_DetallePorRemesa(
            [FromQuery] int codEmpresa,
            [FromQuery] int codRemesa)
            => _bl.AfCdRemesasTes_DetallePorRemesa(codEmpresa, codRemesa);

        [HttpGet("TesTokens_ObtenerActivo")]
        public ActionResult<ErrorDto<TesTokenDto>> TesTokens_ObtenerActivo([FromQuery] int codEmpresa)
            => _bl.TesTokens_ObtenerActivo(codEmpresa);

        [HttpGet("TesTokens_ObtenerConsec")]
        public ActionResult<ErrorDto<TesTokenConsecDto>> TesTokens_ObtenerConsec([FromQuery] int codEmpresa, [FromQuery] string fecha)
            => _bl.TesTokens_ObtenerConsec(codEmpresa, fecha);

        [HttpPost("TesTokens_Insertar")]
        public ActionResult<ErrorDto<bool>> TesTokens_Insertar([FromQuery] int codEmpresa, [FromBody] TesTokenInsertDto dto)
            => _bl.TesTokens_Insertar(codEmpresa, dto);

        [HttpPost("AfCdRemesasTes_Desembolso")]
        public ActionResult<ErrorDto<bool>> AfCdRemesasTes_Desembolso(
            [FromQuery] int codEmpresa,
            [FromBody] AfCdRemesaDesembolsoParams param)
            => _bl.AfCdRemesasTes_Desembolso(codEmpresa, param);
    }
}
