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
    public class FrmCrComisionesCatalogoController : ControllerBase
    {
        private readonly FrmCrComisionesCatalogoBL _bl;

        public FrmCrComisionesCatalogoController(IConfiguration config)
        {
            _bl = new FrmCrComisionesCatalogoBL(config);
        }

        [HttpGet("Cr_ComisionesCatalogo_Obtener")]
        public ErrorDto<List<CrComisionesCatalogoData>> Cr_ComisionesCatalogo_Obtener(int codEmpresa)
            => _bl.Cr_ComisionesCatalogo_Obtener(codEmpresa);

        [HttpPost("Cr_ComisionesCatalogo_Guardar")]
        public ErrorDto Cr_ComisionesCatalogo_Guardar(int codEmpresa, CrComisionesCatalogoGuardarRequest request)
            => _bl.Cr_ComisionesCatalogo_Guardar(codEmpresa, request);

        [HttpDelete("Cr_ComisionesCatalogo_Eliminar")]
        public ErrorDto Cr_ComisionesCatalogo_Eliminar(int codEmpresa, CrComisionesCatalogoEliminarRequest request)
            => _bl.Cr_ComisionesCatalogo_Eliminar(codEmpresa, request);

        [HttpPost("Cr_ComisionesCatalogo_Porcentajes_Obtener")]
        public ErrorDto<List<CrComisionesCatalogoPorcentajeData>> Cr_ComisionesCatalogo_Porcentajes_Obtener(
            int codEmpresa,
            CrComisionesCatalogoPorcentajesRequest request)
            => _bl.Cr_ComisionesCatalogo_Porcentajes_Obtener(codEmpresa, request);

        [HttpPost("Cr_ComisionesCatalogo_Porcentaje_Guardar")]
        public ErrorDto Cr_ComisionesCatalogo_Porcentaje_Guardar(
            int codEmpresa,
            CrComisionesCatalogoPorcentajeGuardarRequest request)
            => _bl.Cr_ComisionesCatalogo_Porcentaje_Guardar(codEmpresa, request);

        [HttpDelete("Cr_ComisionesCatalogo_Porcentaje_Eliminar")]
        public ErrorDto Cr_ComisionesCatalogo_Porcentaje_Eliminar(
            int codEmpresa,
            CrComisionesCatalogoPorcentajeEliminarRequest request)
            => _bl.Cr_ComisionesCatalogo_Porcentaje_Eliminar(codEmpresa, request);

        [HttpPost("Cr_ComisionesCatalogo_Lineas_Obtener")]
        public ErrorDto<List<CrComisionesCatalogoLineaData>> Cr_ComisionesCatalogo_Lineas_Obtener(
            int codEmpresa,
            CrComisionesCatalogoLineasRequest request)
            => _bl.Cr_ComisionesCatalogo_Lineas_Obtener(codEmpresa, request);

        [HttpPost("Cr_ComisionesCatalogo_Linea_Asignar")]
        public ErrorDto Cr_ComisionesCatalogo_Linea_Asignar(
            int codEmpresa,
            CrComisionesCatalogoLineaAsignarRequest request)
            => _bl.Cr_ComisionesCatalogo_Linea_Asignar(codEmpresa, request);

        [HttpGet("Cr_ComisionesCatalogo_Cuenta_Obtener")]
        public ErrorDto<CrComisionesCatalogoCuentaLookupData?> Cr_ComisionesCatalogo_Cuenta_Obtener(
            int codEmpresa,
            string cuenta)
            => _bl.Cr_ComisionesCatalogo_Cuenta_Obtener(codEmpresa, cuenta);
    }
}
