using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoCargosController : ControllerBase
    {
        private readonly FrmCrCatalogoCargosBl _bl;

        public FrmCrCatalogoCargosController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoCargosBl(config);
        }

        [HttpGet("CrCatalogoCargos_Obtener")]
        public ErrorDto<List<CrCatalogoCargoData>> CrCatalogoCargos_Obtener(int codEmpresa)
            => _bl.CrCatalogoCargos_Obtener(codEmpresa);

        [HttpPost("CrCatalogoCargos_Guardar")]
        public ErrorDto CrCatalogoCargos_Guardar(int codEmpresa, CrCatalogoCargoGuardarRequest request)
            => _bl.CrCatalogoCargos_Guardar(codEmpresa, request);

        [HttpDelete("CrCatalogoCargos_Eliminar")]
        public ErrorDto CrCatalogoCargos_Eliminar(int codEmpresa, CrCatalogoCargoEliminarRequest request)
            => _bl.CrCatalogoCargos_Eliminar(codEmpresa, request);

        [HttpGet("CrCatalogoCargos_AsignacionArbol_Obtener")]
        public ErrorDto<List<CrCatalogoCargoArbolData>> CrCatalogoCargos_AsignacionArbol_Obtener(int codEmpresa)
            => _bl.CrCatalogoCargos_AsignacionArbol_Obtener(codEmpresa);

        [HttpPost("CrCatalogoCargos_AsignacionCargos_Obtener")]
        public ErrorDto<List<CrCatalogoCargoAsignacionData>> CrCatalogoCargos_AsignacionCargos_Obtener(
            int codEmpresa,
            CrCatalogoCargoAsignacionObtenerRequest request)
            => _bl.CrCatalogoCargos_AsignacionCargos_Obtener(codEmpresa, request);

        [HttpPost("CrCatalogoCargos_Asignacion_Guardar")]
        public ErrorDto CrCatalogoCargos_Asignacion_Guardar(
            int codEmpresa,
            CrCatalogoCargoAsignacionGuardarRequest request)
            => _bl.CrCatalogoCargos_Asignacion_Guardar(codEmpresa, request);

        [HttpGet("CrCatalogoCargos_TablaAplicacionCargos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogoCargos_TablaAplicacionCargos_Obtener(int codEmpresa)
            => _bl.CrCatalogoCargos_TablaAplicacionCargos_Obtener(codEmpresa);

        [HttpPost("CrCatalogoCargos_TablaAplicacion_Obtener")]
        public ErrorDto<List<CrCatalogoCargoTablaAplicacionData>> CrCatalogoCargos_TablaAplicacion_Obtener(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionObtenerRequest request)
            => _bl.CrCatalogoCargos_TablaAplicacion_Obtener(codEmpresa, request);

        [HttpPost("CrCatalogoCargos_TablaAplicacion_Guardar")]
        public ErrorDto CrCatalogoCargos_TablaAplicacion_Guardar(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionGuardarRequest request)
            => _bl.CrCatalogoCargos_TablaAplicacion_Guardar(codEmpresa, request);

        [HttpDelete("CrCatalogoCargos_TablaAplicacion_Eliminar")]
        public ErrorDto CrCatalogoCargos_TablaAplicacion_Eliminar(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionEliminarRequest request)
            => _bl.CrCatalogoCargos_TablaAplicacion_Eliminar(codEmpresa, request);
    }
}