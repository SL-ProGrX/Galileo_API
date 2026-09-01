using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;
using Galileo_API.Services.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCcProcesoMensualProcesoController : ControllerBase
    {
        private readonly CcProcesoMensualProcesoDb _procesoDb;
        private readonly CcProcesoMensualProcesoQueue _queue;

        public FrmCcProcesoMensualProcesoController(
            IConfiguration config,
            CcProcesoMensualProcesoQueue queue)
        {
            _procesoDb = new CcProcesoMensualProcesoDb(config);
            _queue = queue;
        }

        private string ObtenerPropietario() =>
            User.FindFirst(ClaimTypes.Name)?.Value ?? "Sistema";

        /// <summary>
        /// POST: Inicia un proceso resiliente o retorna uno existente si está activo.
        /// </summary>
        [HttpPost("ProcesoIniciar")]
        [Authorize]
        public ErrorDto<CcProcesoMensualProcesoResultado> ProcesoIniciar(
            [FromQuery] int codEmpresa,
            [FromBody] CcProcesoMensualProcesoIniciarRequest request)
        {
            var propietario = ObtenerPropietario();

            var resultado = _procesoDb.Proceso_Iniciar(codEmpresa, propietario, request);

            if (resultado.Result is not null && resultado.Result.ProcesoId != Guid.Empty)
            {
                var trabajo = new CcProcesoMensualProcesoTrabajo
                {
                    CodEmpresa = codEmpresa,
                    ProcesoId = resultado.Result.ProcesoId
                };

                _queue.Encolar(trabajo);
            }

            return resultado;
        }

        /// <summary>
        /// GET: Obtiene el estado actual del proceso (para polling).
        /// </summary>
        [HttpGet("ProcesoEstado")]
        [Authorize]
        public ErrorDto<CcProcesoMensualProcesoResultado> ProcesoEstado(
            [FromQuery] int codEmpresa,
            [FromQuery] Guid procesoId)
        {
            var propietario = ObtenerPropietario();
            return _procesoDb.Proceso_Estado_Obtener(codEmpresa, procesoId, propietario);
        }

        /// <summary>
        /// GET: Obtiene los errores de un proceso.
        /// </summary>
        [HttpGet("ProcesoErrores")]
        [Authorize]
        public ErrorDto<List<CcProcesoMensualProcesoError>> ProcesoErrores(
            [FromQuery] int codEmpresa,
            [FromQuery] Guid procesoId)
        {
            var errores = _procesoDb.Proceso_Errores_Obtener(codEmpresa, procesoId);
            return DbHelper.CreateOkResponse(errores);
        }
    }
}
