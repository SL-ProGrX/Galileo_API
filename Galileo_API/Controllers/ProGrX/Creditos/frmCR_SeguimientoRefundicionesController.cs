using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrSeguimientoRefundicionesController : ControllerBase
    {
        private readonly FrmCrSeguimientoRefundicionesBL BL;

        public FrmCrSeguimientoRefundicionesController(IConfiguration config)
        {
            BL = new FrmCrSeguimientoRefundicionesBL(config);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Inicializar")]
        public ErrorDto<CrSeguimientoRefundicionesInicializarDto> CR_SeguimientoRefundiciones_Inicializar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesInicializarRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Inicializar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Lista_Obtener")]
        public ErrorDto<CrSeguimientoRefundicionesListaDto> CR_SeguimientoRefundiciones_Lista_Obtener(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesListaRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Lista_Exportar")]
        public ErrorDto<CrSeguimientoRefundicionesListaDto> CR_SeguimientoRefundiciones_Lista_Exportar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesListaRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Lista_Exportar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Prestamos_Obtener")]
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Prestamos_Obtener(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesPrestamosRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Prestamos_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Prestamos_Exportar")]
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Prestamos_Exportar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesPrestamosRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Prestamos_Exportar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Terceros_Obtener")]
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Terceros_Obtener(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Terceros_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Terceros_Exportar")]
        public ErrorDto<CrSeguimientoRefundicionesCreditosListaDto> CR_SeguimientoRefundiciones_Terceros_Exportar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesConsultaTercerosRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Terceros_Exportar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Refunde_Datos")]
        public ErrorDto<CrSeguimientoRefundicionDatosDto> CR_SeguimientoRefundiciones_Refunde_Datos(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesRefundeDatosRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Refunde_Datos(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Guardar")]
        public ErrorDto CR_SeguimientoRefundiciones_Guardar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionGuardarRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Eliminar")]
        public ErrorDto CR_SeguimientoRefundiciones_Eliminar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesEliminarRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Eliminar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoRefundiciones_Actualizar")]
        public ErrorDto CR_SeguimientoRefundiciones_Actualizar(
            int CodEmpresa,
            [FromBody] CrSeguimientoRefundicionesActualizarRequest request)
        {
            return BL.CR_SeguimientoRefundiciones_Actualizar(CodEmpresa, request);
        }
    }
}