using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [ApiController]
    [Authorize]
    [Route("api/FrmSIFRecepcionNdNc")]
    public sealed class FrmSifRecepcionNdNcController : ControllerBase
    {
        private readonly FrmSifRecepcionNdNcBl _bl;

        public FrmSifRecepcionNdNcController(IConfiguration config)
        {
            _bl = new FrmSifRecepcionNdNcBl(config);
        }

        [HttpGet("SIF_RecepcionNdNc_Inicializar")]
        public ErrorDto<SifRecepcionNdNcInicializaData>
            SIF_RecepcionNdNc_Inicializar(
                int codEmpresa)
        {
            return _bl.SIF_RecepcionNdNc_Inicializar(
                codEmpresa);
        }

        [HttpGet("SIF_RecepcionNdNc_Documentos_Obtener")]
        public ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            SIF_RecepcionNdNc_Documentos_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl.SIF_RecepcionNdNc_Documentos_Obtener(
                codEmpresa,
                request);
        }

        [HttpGet("SIF_RecepcionNdNc_Pendientes_Obtener")]
        public ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            SIF_RecepcionNdNc_Pendientes_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl.SIF_RecepcionNdNc_Pendientes_Obtener(
                codEmpresa,
                request);
        }

        [HttpGet("SIF_RecepcionNdNc_Consulta_Obtener")]
        public ErrorDto<List<SifRecepcionNdNcConsultaData>>
            SIF_RecepcionNdNc_Consulta_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl.SIF_RecepcionNdNc_Consulta_Obtener(
                codEmpresa,
                request);
        }

        [HttpPost("SIF_RecepcionNdNc_Movimiento_Aplicar")]
        public ErrorDto<int>
            SIF_RecepcionNdNc_Movimiento_Aplicar(
                int codEmpresa,
                SifRecepcionNdNcAplicarRequest request)
        {
            request.usuario =
                User.Identity?.Name?.Trim() ?? string.Empty;

            return _bl.SIF_RecepcionNdNc_Movimiento_Aplicar(
                codEmpresa,
                request);
        }
    }
}