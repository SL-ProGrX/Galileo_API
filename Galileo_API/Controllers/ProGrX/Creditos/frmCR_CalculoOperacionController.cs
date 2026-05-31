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
    public class FrmCrCalculoOperacionController : ControllerBase
    {
        private readonly FrmCrCalculoOperacionBl _bl;

        public FrmCrCalculoOperacionController(IConfiguration config)
        {
            _bl = new FrmCrCalculoOperacionBl(config);
        }

        [HttpGet("CrCalculoOperacion_Cedula_Obtener")]
        public ErrorDto<CrCalculoOperacionPantallaData> CrCalculoOperacion_Cedula_Obtener(int codEmpresa, string cedula)
            => _bl.CrCalculoOperacion_Cedula_Obtener(codEmpresa, cedula);

        [HttpGet("CrCalculoOperacion_Codigo_Obtener")]
        public ErrorDto<CrCalculoOperacionCodigoData> CrCalculoOperacion_Codigo_Obtener(int codEmpresa, string cedula, string codigo)
            => _bl.CrCalculoOperacion_Codigo_Obtener(codEmpresa, cedula, codigo);

        [HttpGet("CrCalculoOperacion_Rangos_Obtener")]
        public ErrorDto<CrCalculoOperacionRangosData> CrCalculoOperacion_Rangos_Obtener(int codEmpresa, string codigo, decimal monto)
            => _bl.CrCalculoOperacion_Rangos_Obtener(codEmpresa, codigo, monto);

        [HttpGet("CrCalculoOperacion_Disponibles_Obtener")]
        public ErrorDto<List<CrCalculoOperacionDisponibleData>> CrCalculoOperacion_Disponibles_Obtener(int codEmpresa, string cedula)
            => _bl.CrCalculoOperacion_Disponibles_Obtener(codEmpresa, cedula);

        [HttpGet("CrCalculoOperacion_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCalculoOperacion_Catalogo_Obtener(int codEmpresa)
            => _bl.CrCalculoOperacion_Catalogo_Obtener(codEmpresa);
    }
}