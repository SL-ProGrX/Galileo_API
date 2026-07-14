using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSeguimientoReqCarController : ControllerBase
    {
        private readonly FrmCrSeguimientoReqCarBl _bl;

        public FrmCrSeguimientoReqCarController(IConfiguration config)
        {
            _bl = new FrmCrSeguimientoReqCarBl(config);
        }

        [HttpGet("CrSeguimientoReqCar_CargaInicial_Obtener")]
        public ErrorDto<CrSeguimientoReqCarCargaInicialData> CrSeguimientoReqCar_CargaInicial_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrSeguimientoReqCar_CargaInicial_Obtener(codEmpresa, request);

        [HttpGet("CrSeguimientoReqCar_Requisitos_Obtener")]
        public ErrorDto<List<CrSeguimientoReqCarRequisitoData>> CrSeguimientoReqCar_Requisitos_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrSeguimientoReqCar_Requisitos_Obtener(codEmpresa, request);

        [HttpGet("CrSeguimientoReqCar_Cargos_Obtener")]
        public ErrorDto<CrSeguimientoReqCarCargosData> CrSeguimientoReqCar_Cargos_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrSeguimientoReqCar_Cargos_Obtener(codEmpresa, request);

        [HttpPost("CrSeguimientoReqCar_Requisitos_Guardar")]
        public ErrorDto CrSeguimientoReqCar_Requisitos_Guardar(
            int codEmpresa,
            [FromBody] CrSeguimientoReqCarRequisitosGuardarRequest request)
            => _bl.CrSeguimientoReqCar_Requisitos_Guardar(codEmpresa, request);

        [HttpPost("CrSeguimientoReqCar_Cargo_Aplicar")]
        public ErrorDto CrSeguimientoReqCar_Cargo_Aplicar(
            int codEmpresa,
            [FromBody] CrSeguimientoReqCarCargoAplicarRequest request)
            => _bl.CrSeguimientoReqCar_Cargo_Aplicar(codEmpresa, request);

        [HttpPost("CrSeguimientoReqCar_Prima_Guardar")]
        public ErrorDto CrSeguimientoReqCar_Prima_Guardar(
            int codEmpresa,
            [FromBody] CrSeguimientoReqCarPrimaGuardarRequest request)
            => _bl.CrSeguimientoReqCar_Prima_Guardar(codEmpresa, request);
    }
}