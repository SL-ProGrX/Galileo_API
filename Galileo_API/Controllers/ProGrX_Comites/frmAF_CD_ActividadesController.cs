using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfCdActividadesController : ControllerBase
    {
        private readonly FrmAfCdActividadesBL _bl;

        public FrmAfCdActividadesController(IConfiguration config)
        {
            _bl = new FrmAfCdActividadesBL(config);
        }

        [HttpGet("AfCdActividades_Lista")]
        public ActionResult<ErrorDto<List<AfCdActividadDto>>> AfCdActividades_Lista([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.AfCdActividades_Lista(codEmpresa, codContabilidad);

        [HttpPost("AfCdActividades_Upsert")]
        public ActionResult<ErrorDto<bool>> AfCdActividades_Upsert([FromQuery] int codEmpresa, [FromBody] AfCdActividadDto dto)
            => _bl.AfCdActividades_Upsert(codEmpresa, dto);

        [HttpGet("AfCdActividades_ComitesPorActividad")]
        public ActionResult<ErrorDto<List<AfCdActividadComiteDto>>> AfCdActividades_ComitesPorActividad([FromQuery] int codEmpresa, [FromQuery] int codActividad)
            => _bl.AfCdActividades_ComitesPorActividad(codEmpresa, codActividad);

        [HttpDelete("AfCdActividades_EliminarComitesPorActividad")]
        public ActionResult<ErrorDto<bool>> AfCdActividades_EliminarComitesPorActividad([FromQuery] int codEmpresa, [FromQuery] int codActividad)
            => _bl.AfCdActividades_EliminarComitesPorActividad(codEmpresa, codActividad);

        [HttpGet("AfCdActividades_SimpleLista")]
        public ActionResult<ErrorDto<List<AfCdActividadSimpleDto>>> AfCdActividades_SimpleLista([FromQuery] int codEmpresa)
            => _bl.AfCdActividades_SimpleLista(codEmpresa);

        [HttpGet("AfCdActividades_RangosPorActividad")]
        public ActionResult<ErrorDto<List<AfCdActividadRangoDto>>> AfCdActividades_RangosPorActividad([FromQuery] int codEmpresa, [FromQuery] int codActividad)
            => _bl.AfCdActividades_RangosPorActividad(codEmpresa, codActividad);

        [HttpPost("AfCdActividades_RangoUpsert")]
        public ActionResult<ErrorDto<bool>> AfCdActividades_RangoUpsert([FromQuery] int codEmpresa, [FromQuery] int codActividad, [FromBody] AfCdActividadRangoDto dto)
            => _bl.AfCdActividades_RangoUpsert(codEmpresa, codActividad, dto);

        [HttpDelete("AfCdActividades_RangoDelete")]
        public ActionResult<ErrorDto<bool>> AfCdActividades_RangoDelete([FromQuery] int codEmpresa, [FromQuery] int codActividad, [FromQuery] int codMonto)
            => _bl.AfCdActividades_RangoDelete(codEmpresa, codActividad, codMonto);

        [HttpGet("AfCdActividades_DropDownLista")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> AfCdActividades_DropDownLista([FromQuery] int codEmpresa)
            => _bl.AfCdActividades_DropDownLista(codEmpresa);

        [HttpGet("AfCdCuentas_Consulta")]
        public ActionResult<ErrorDto<List<AfCdCuentaConsultaDto>>> AfCdCuentas_Consulta([FromQuery] int codEmpresa, [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] string codActividad)
            => _bl.AfCdCuentas_Consulta(codEmpresa, fechaInicio, fechaFin, codActividad);
    }
}
