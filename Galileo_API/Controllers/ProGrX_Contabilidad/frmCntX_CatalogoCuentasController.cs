using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Authorize]
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
        {
            return _bl.CntXCatalogoDivisas(codEmpresa, codContabilidad);
        }

        [HttpGet("CntXCatalogoTiposCuenta")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoTiposCuenta([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
        {
            return _bl.CntXCatalogoTiposCuenta(codEmpresa, codContabilidad);
        }

        [HttpGet("CntXCatalogoUnidades")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoUnidades([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
        {
            return _bl.CntXCatalogoUnidades(codEmpresa, codContabilidad);
        }

        [HttpGet("CntXCatalogoCentrosCosto")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoCentrosCosto([FromQuery] int codEmpresa, [FromQuery] int codContabilidad)
        {
            return _bl.CntXCatalogoCentrosCosto(codEmpresa, codContabilidad);
        }

        [HttpGet("CntXCatalogoCentrosCostoPorUnidad")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> CntXCatalogoCentrosCostoPorUnidad([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string codUnidad)
        {
            return _bl.CntXCatalogoCentrosCostoPorUnidad(codEmpresa, codContabilidad, codUnidad);
        }

        [HttpGet("CntXCatalogoCuentaObtener")]
        public ActionResult<ErrorDto<CntXCatalogoCuentaLookupDto>> CntXCatalogoCuentaObtener([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string cuenta)
        {
            return _bl.CntXCatalogoCuentaObtener(codEmpresa, codContabilidad, cuenta);
        }

        [HttpPost("CntXCatalogoConsulta")]
        public ActionResult<ErrorDto<List<CntXCatalogoCuentaDto>>> CntXCatalogoConsulta([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentasFiltroRequest filtro)
        {
            return _bl.CntXCatalogoConsulta(codEmpresa, filtro);
        }

        [HttpGet("CntXCatalogoDetalle")]
        public ActionResult<ErrorDto<CntXCatalogoCuentaDetalleResponse>> CntXCatalogoDetalle([FromQuery] int codEmpresa, [FromQuery] int codContabilidad, [FromQuery] string cuenta)
        {
            return _bl.CntXCatalogoDetalle(codEmpresa, codContabilidad, cuenta);
        }

        [HttpPost("CntXCatalogoDetalleGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoDetalleGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaDetalleGuardarRequest request)
        {
            return _bl.CntXCatalogoDetalleGuardar(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoCuentaEstadoGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoCuentaEstadoGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaEstadoRequest request)
        {
            return _bl.CntXCatalogoCuentaEstadoGuardar(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoCuentaGuardar")]
        public ActionResult<ErrorDto<CntXCatalogoCuentaGuardarResponse>> CntXCatalogoCuentaGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaGuardarRequest request)
        {
            return _bl.CntXCatalogoCuentaGuardar(codEmpresa, request);
        }

        [HttpDelete("CntXCatalogoCuentaEliminar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoCuentaEliminar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoCuentaEliminarRequest request)
        {
            return _bl.CntXCatalogoCuentaEliminar(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoMapeo")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoMapeo([FromQuery] int codEmpresa, [FromBody] CntXCatalogoMapeoRequest request)
        {
            return _bl.CntXCatalogoMapeo(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoBajaNivel")]
        public ActionResult<ErrorDto<CntXCatalogoBajaNivelDto>> CntXCatalogoBajaNivel([FromQuery] int codEmpresa, [FromBody] CntXCatalogoBajaNivelRequest request)
        {
            return _bl.CntXCatalogoBajaNivel(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoFormatoActualizar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoFormatoActualizar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoFormatoRequest request)
        {
            return _bl.CntXCatalogoFormatoActualizar(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoRevision")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoRevision([FromQuery] int codEmpresa, [FromBody] CntXCatalogoRevisionRequest request)
        {
            return _bl.CntXCatalogoRevision(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoTraduccionGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoTraduccionGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoTraduccionGuardarRequest request)
        {
            return _bl.CntXCatalogoTraduccionGuardar(codEmpresa, request);
        }

        [HttpDelete("CntXCatalogoTraduccionEliminar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoTraduccionEliminar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoTraduccionGuardarRequest request)
        {
            return _bl.CntXCatalogoTraduccionEliminar(codEmpresa, request);
        }

        [HttpPost("CntXCatalogoProrrataGuardar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoProrrataGuardar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoProrrataGuardarRequest request)
        {
            return _bl.CntXCatalogoProrrataGuardar(codEmpresa, request);
        }

        [HttpDelete("CntXCatalogoProrrataEliminar")]
        public ActionResult<ErrorDto<bool>> CntXCatalogoProrrataEliminar([FromQuery] int codEmpresa, [FromBody] CntXCatalogoProrrataGuardarRequest request)
        {
            return _bl.CntXCatalogoProrrataEliminar(codEmpresa, request);
        }
    }
}
