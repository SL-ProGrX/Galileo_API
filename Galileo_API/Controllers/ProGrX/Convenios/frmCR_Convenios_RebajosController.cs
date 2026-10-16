using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosRebajosController
        : ControllerBase
    {
        private readonly FrmCrConveniosRebajosBl _bl;

        public FrmCrConveniosRebajosController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmCrConveniosRebajosBl(config);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Inicializar")]
        public ErrorDto<CrConveniosRebajosInicialData?>
            CR_Convenios_Rebajos_Inicializar(
                [FromQuery] int codEmpresa)
        {
            return _bl.CR_Convenios_Rebajos_Inicializar(
                codEmpresa);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Convenios_Obtener")]
        public ErrorDto<List<CrConvenioConsultaData>>
            CR_Convenios_Rebajos_Convenios_Obtener(
                [FromQuery] int codEmpresa)
        {
            return _bl
                .CR_Convenios_Rebajos_Convenios_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Convenio_Obtener")]
        public ErrorDto<CrConvenioConsultaData?>
            CR_Convenios_Rebajos_Convenio_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] string? descripcion)
        {
            return _bl
                .CR_Convenios_Rebajos_Convenio_Obtener(
                    codEmpresa,
                    codConvenio,
                    descripcion);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Convenio_Navegar")]
        public ErrorDto<CrConvenioConsultaData?>
            CR_Convenios_Rebajos_Convenio_Navegar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] bool siguiente)
        {
            return _bl
                .CR_Convenios_Rebajos_Convenio_Navegar(
                    codEmpresa,
                    codConvenio,
                    siguiente);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Cargos_Obtener")]
        public ErrorDto<List<CrConveniosRebajosCargoData>>
            CR_Convenios_Rebajos_Cargos_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio)
        {
            return _bl
                .CR_Convenios_Rebajos_Cargos_Obtener(
                    codEmpresa,
                    codConvenio);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Transacciones_Obtener")]
        public ErrorDto
            <List<CrConveniosRebajosTransaccionBusquedaData>>
            CR_Convenios_Rebajos_Transacciones_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio)
        {
            return _bl
                .CR_Convenios_Rebajos_Transacciones_Obtener(
                    codEmpresa,
                    codConvenio);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Transaccion_Obtener")]
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Transaccion_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] int idTransac)
        {
            return _bl
                .CR_Convenios_Rebajos_Transaccion_Obtener(
                    codEmpresa,
                    codConvenio,
                    idTransac);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Transaccion_Navegar")]
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Transaccion_Navegar(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] int idTransac,
                [FromQuery] bool siguiente)
        {
            return _bl
                .CR_Convenios_Rebajos_Transaccion_Navegar(
                    codEmpresa,
                    codConvenio,
                    idTransac,
                    siguiente);
        }

        [ValidateAntiForgeryToken]
        [HttpPost(
            "CR_Convenios_Rebajos_Registrar")]
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Registrar(
                [FromQuery] int codEmpresa,
                [FromBody]
                CrConveniosRebajosGuardarRequest request)
        {
            return _bl.CR_Convenios_Rebajos_Registrar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut(
            "CR_Convenios_Rebajos_Actualizar")]
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Actualizar(
                [FromQuery] int codEmpresa,
                [FromBody]
                CrConveniosRebajosGuardarRequest request)
        {
            return _bl.CR_Convenios_Rebajos_Actualizar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete(
            "CR_Convenios_Rebajos_Eliminar")]
        public ErrorDto
            CR_Convenios_Rebajos_Eliminar(
                [FromQuery] int codEmpresa,
                [FromBody]
                CrConveniosRebajosEliminarRequest request)
        {
            return _bl.CR_Convenios_Rebajos_Eliminar(
                codEmpresa,
                request);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Cobros_Obtener")]
        public ErrorDto<List<CrConveniosRebajosCobroData>>
            CR_Convenios_Rebajos_Cobros_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] int estado = 3)
        {
            return _bl
                .CR_Convenios_Rebajos_Cobros_Obtener(
                    codEmpresa,
                    codConvenio,
                    estado);
        }

        [HttpGet(
            "CR_Convenios_Rebajos_Pagos_Obtener")]
        public ErrorDto<List<CrConveniosRebajosPagoData>>
            CR_Convenios_Rebajos_Pagos_Obtener(
                [FromQuery] int codEmpresa,
                [FromQuery] string? codConvenio,
                [FromQuery] int idTransac)
        {
            return _bl
                .CR_Convenios_Rebajos_Pagos_Obtener(
                    codEmpresa,
                    codConvenio,
                    idTransac);
        }
    }
}