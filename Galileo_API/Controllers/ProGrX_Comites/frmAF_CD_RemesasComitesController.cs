using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
    }
}
