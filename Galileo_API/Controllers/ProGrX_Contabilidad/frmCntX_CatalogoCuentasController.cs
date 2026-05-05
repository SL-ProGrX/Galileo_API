using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXCatalogoCuentasController : ControllerBase
    {
        private readonly FrmCntXCatalogoCuentasBL _bl;

        public FrmCntXCatalogoCuentasController(IConfiguration config)
        {
            _bl = new FrmCntXCatalogoCuentasBL(config);
        }

        [HttpGet("CntXCatalogoDivisas")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoDivisas([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXCatalogoDivisas(codEmpresa, codContabilidad);

        [HttpGet("CntXCatalogoTiposCuenta")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoTiposCuenta([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXCatalogoTiposCuenta(codEmpresa, codContabilidad);

        [HttpGet("CntXCatalogoUnidades")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoUnidades([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXCatalogoUnidades(codEmpresa, codContabilidad);

        [HttpGet("CntXCatalogoCentrosCosto")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoCentrosCosto([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
            => _bl.CntXCatalogoCentrosCosto(codEmpresa, codContabilidad);

        [HttpPost("CntXCatalogoConsulta")]
        public ActionResult<ErrorDto<List<CntXCatalogoCuentaDto>>> CntXCatalogoConsulta([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentasFiltroRequest filtro)
            => _bl.CntXCatalogoConsulta(codEmpresa, filtro);

        [HttpGet("CntXCatalogoDetalle")]
        public ActionResult<ErrorDto<CntXCatalogoCuentaDetalleResponse>> CntXCatalogoDetalle([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string cuenta)
            => _bl.CntXCatalogoDetalle(codEmpresa, codContabilidad, cuenta);

        [HttpPost("CntXCatalogoDetalleGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoDetalleGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaDetalleGuardarRequest request)
            => _bl.CntXCatalogoDetalleGuardar(codEmpresa, request);

        [HttpPost("CntXCatalogoCuentaEstadoGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoCuentaEstadoGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaEstadoRequest request)
            => _bl.CntXCatalogoCuentaEstadoGuardar(codEmpresa, request);

        [HttpPost("CntXCatalogoCuentaGuardar")]
        public ActionResult<ErrorDto<CntXCatalogoCuentaGuardarResponse>> CntXCatalogoCuentaGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaGuardarRequest request)
            => _bl.CntXCatalogoCuentaGuardar(codEmpresa, request);

        [HttpPost("CntXCatalogoMapeo")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoMapeo([FromQuery] int codEmpresa, [FromBody] CntXCatalogoMapeoRequest request)
            => _bl.CntXCatalogoMapeo(codEmpresa, request);

        [HttpPost("CntXCatalogoBajaNivel")]
        public ActionResult<ErrorDto<CntXCatalogoBajaNivelDto>> CntXCatalogoBajaNivel([FromQuery] int codEmpresa, [FromBody] CntXCatalogoBajaNivelRequest request)
            => _bl.CntXCatalogoBajaNivel(codEmpresa, request);

        [HttpPost("CntXCatalogoTraduccionGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoTraduccionGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoTraduccionGuardarRequest request)
            => _bl.CntXCatalogoTraduccionGuardar(codEmpresa, request);

        [HttpDelete("CntXCatalogoTraduccionEliminar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoTraduccionEliminar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoTraduccionGuardarRequest request)
            => _bl.CntXCatalogoTraduccionEliminar(codEmpresa, request);

        [HttpPost("CntXCatalogoProrrataGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoProrrataGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoProrrataGuardarRequest request)
            => _bl.CntXCatalogoProrrataGuardar(codEmpresa, request);

        [HttpDelete("CntXCatalogoProrrataEliminar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoProrrataEliminar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoProrrataGuardarRequest request)
            => _bl.CntXCatalogoProrrataEliminar(codEmpresa, request);
    }
}
