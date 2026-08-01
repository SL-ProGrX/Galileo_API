using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXUnidadesController : ControllerBase
    {
        private readonly FrmCntXUnidadesBl _bl;

        public FrmCntXUnidadesController(IConfiguration config)
        {
            _bl = new FrmCntXUnidadesBl(config);
        }


        [Authorize]
        [HttpGet("CntX_Unidades_Listar")]
        public ErrorDto<List<CntXUnidadDto>> CntX_Unidades_Listar(int CodEmpresa, int CodContabilidad)
        {
            return _bl.CntX_Unidades_Listar(CodEmpresa, CodContabilidad);
        }

        [Authorize]
        [HttpPost("CntX_Unidades_Guardar")]
        public ErrorDto<bool> CntX_Unidades_Guardar(int CodEmpresa, int CodContabilidad, string Usuario,
            [FromBody] CntXUnidadGuardarDto dto
        )
        {
            return _bl.CntX_Unidades_Guardar(CodEmpresa, CodContabilidad, Usuario, dto);
        }

        [Authorize]
        [HttpDelete("CntX_Unidades_Eliminar")]
        public ErrorDto<bool> CntX_Unidades_Eliminar(int CodEmpresa, int CodContabilidad, string Usuario, string CodUnidad
        )
        {
            return _bl.CntX_Unidades_Eliminar(CodEmpresa, CodContabilidad, Usuario, CodUnidad);
        }


        [Authorize]
        [HttpGet("CntX_Unidades_Activas_Listar")]
        public ErrorDto<List<CntXUnidadActivaDto>> CntX_Unidades_Activas_Listar(int CodEmpresa, int CodContabilidad)
        {
            return _bl.CntX_Unidades_Activas_Listar(CodEmpresa, CodContabilidad);
        }

        [Authorize]
        [HttpGet("CntX_CentrosCosto_PorUnidad")]
        public ErrorDto<List<CntXCentroCostoDto>> CntX_CentrosCosto_PorUnidad(int CodEmpresa, int CodContabilidad, string cod_unidad)
        {
            return _bl.CntX_CentrosCosto_PorUnidad(CodEmpresa, CodContabilidad, cod_unidad);
        }

        [Authorize]
        [HttpPost("CntX_Unidades_CC_Guardar")]
        public ErrorDto<bool> CntX_Unidades_CC_Guardar(int CodEmpresa, int CodContabilidad, string Usuario,
            [FromBody] CntXUnidadCCGuardarDto dto
        )
        {
            return _bl.CntX_Unidades_CC_Guardar(CodEmpresa, CodContabilidad, Usuario, dto);
        }

        [Authorize]
        [HttpGet("CntX_Unidades_CC_Consulta")]
        public ErrorDto<List<CntXCentroCostoDto>> CntX_Unidades_CC_Consulta(int codEmpresa, int codContabilidad, string codUnidad
        )
        {
            return _bl.CntX_Unidades_CC_Consulta(codEmpresa, codContabilidad, codUnidad);
        }

    }
}
