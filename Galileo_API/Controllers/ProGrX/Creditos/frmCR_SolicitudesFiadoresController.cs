using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrSolicitudesFiadoresController : ControllerBase
    {
        private readonly FrmCrSolicitudesFiadoresBL BL;

        public FrmCrSolicitudesFiadoresController(IConfiguration config)
        {
            BL = new FrmCrSolicitudesFiadoresBL(config);
        }

        [Authorize]
        [HttpGet("CR_SolicitudesFiadores_Instituciones_Obtener")]
        public ErrorDto<List<CrSolicitudesFiadoresInstitucionDto>> CR_SolicitudesFiadores_Instituciones_Obtener(
            int CodEmpresa)
        {
            return BL.CR_SolicitudesFiadores_Instituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_SolicitudesFiadores_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_SolicitudesFiadores_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            return BL.CR_SolicitudesFiadores_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_SolicitudesFiadores_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CR_SolicitudesFiadores_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return BL.CR_SolicitudesFiadores_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_SolicitudesFiadores_Detalle_Obtener")]
        public ErrorDto<CrSolicitudesFiadoresDetalleDto> CR_SolicitudesFiadores_Detalle_Obtener(
            int CodEmpresa,
            long fiaConsec)
        {
            return BL.CR_SolicitudesFiadores_Detalle_Obtener(CodEmpresa, fiaConsec);
        }

        [Authorize]
        [HttpGet("CR_SolicitudesFiadores_Socio_Obtener")]
        public ErrorDto<CrSolicitudesFiadoresSocioDto> CR_SolicitudesFiadores_Socio_Obtener(
            int CodEmpresa,
            string cedula)
        {
            return BL.CR_SolicitudesFiadores_Socio_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("CR_SolicitudesFiadores_Guardar")]
        public ErrorDto CR_SolicitudesFiadores_Guardar(
            int CodEmpresa,
            [FromBody] CrSolicitudesFiadoresGuardarRequest request)
        {
            return BL.CR_SolicitudesFiadores_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SolicitudesFiadores_Eliminar")]
        public ErrorDto CR_SolicitudesFiadores_Eliminar(
            int CodEmpresa,
            [FromBody] CrSolicitudesFiadoresEliminarRequest request)
        {
            return BL.CR_SolicitudesFiadores_Eliminar(CodEmpresa, request);
        }
    }
}