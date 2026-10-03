using Galileo.BusinessLogic;
using Galileo.BusinessLogic.ProGrX;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Galileo.Controllers.ProGrX
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmDsbColaboradorController : ControllerBase
    {
        private const string UserNameClaim = "UserName";

        private readonly FrmDsbColaboradorBL _bl;
        private readonly MReportingServicesBL _reportes;

        public FrmDsbColaboradorController(IConfiguration config)
        {
            _bl = new FrmDsbColaboradorBL(config);
            _reportes = new MReportingServicesBL(config);
        }

        [Authorize]
        [HttpGet("Colaborador_Vinculado_Obtener")]
        public ActionResult<ErrorDto<ColaboradorVinculoData>> Colaborador_Vinculado_Obtener(
            int CodEmpresa)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Vinculado_Obtener(CodEmpresa, usuario));
        }

        [Authorize]
        [HttpGet("Colaborador_Perfil_Obtener")]
        public ActionResult<ErrorDto<ColaboradorPerfilData>> Colaborador_Perfil_Obtener(
            int CodEmpresa,
            string AppVersion)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Perfil_Obtener(
                CodEmpresa,
                usuario,
                AppVersion));
        }

        [Authorize]
        [HttpPost("Colaborador_Foto_Cambia")]
        public ActionResult<ErrorDto<bool>> Colaborador_Foto_Cambia(
            int CodEmpresa,
            [FromBody] ColaboradorFotoCambiaRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Foto_Cambia(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpGet("Colaborador_Consulta_Id")]
        public ActionResult<ErrorDto<List<ColaboradorEmpleadoOpcionData>>> Colaborador_Consulta_Id(
            int CodEmpresa,
            string Identificacion,
            string? EmpleadoId = null)
        {
            return Ok(_bl.Colaborador_Consulta_Id(
                CodEmpresa,
                Identificacion,
                EmpleadoId));
        }

        [Authorize]
        [HttpPost("Colaborador_Acceso")]
        public ActionResult<ErrorDto<ColaboradorAccesoData>> Colaborador_Acceso(
            int CodEmpresa,
            [FromBody] ColaboradorAccesoRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Acceso(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Clave_Reestablece")]
        public ActionResult<ErrorDto<ColaboradorClaveReestableceData>> Colaborador_Clave_Reestablece(
            int CodEmpresa,
            [FromBody] ColaboradorClaveReestableceRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Clave_Reestablece(
                CodEmpresa,
                usuario,
                request));
        }

        [Authorize]
        [HttpPost("Colaborador_Clave_Cambia")]
        public ActionResult<ErrorDto<bool>> Colaborador_Clave_Cambia(
            int CodEmpresa,
            [FromBody] ColaboradorClaveCambiaRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Clave_Cambia(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Menu_Obtener")]
        public ActionResult<ErrorDto<List<Dictionary<string, object?>>>> Colaborador_Menu_Obtener(
            int CodEmpresa,
            [FromBody] ColaboradorMenuRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Menu_Obtener(
                CodEmpresa,
                usuario,
                request));
        }

        [Authorize]
        [HttpPost("Colaborador_Autorizacion_Registrar")]
        public ActionResult<ErrorDto<bool>> Colaborador_Autorizacion_Registrar(
            int CodEmpresa,
            [FromBody] ColaboradorAutorizacionRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Autorizacion_Registrar(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Traslado_Gestionar")]
        public ActionResult<ErrorDto<bool>> Colaborador_Traslado_Gestionar(
            int CodEmpresa,
            [FromBody] ColaboradorTrasladoGestionRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Traslado_Gestionar(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Traslado_Configuracion_Obtener")]
        public ActionResult<ErrorDto<ColaboradorTrasladoConfiguracionData>> Colaborador_Traslado_Configuracion_Obtener(
            int CodEmpresa,
            [FromBody] ColaboradorTrasladoAccesoRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Traslado_Configuracion_Obtener(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Traslado_Registrar")]
        public ActionResult<ErrorDto<ColaboradorTrasladoRegistroData>> Colaborador_Traslado_Registrar(
            int CodEmpresa,
            [FromBody] ColaboradorTrasladoRegistrarRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Traslado_Registrar(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Traslado_Detalle_Obtener")]
        public ActionResult<ErrorDto<List<ColaboradorTrasladoPlacaData>>> Colaborador_Traslado_Detalle_Obtener(
            int CodEmpresa,
            [FromBody] ColaboradorTrasladoDetalleRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Traslado_Detalle_Obtener(CodEmpresa, usuario, request));
        }

        [Authorize]
        [HttpPost("Colaborador_Reporte_Obtener")]
        public IActionResult Colaborador_Reporte_Obtener(
            int CodEmpresa,
            [FromBody] ColaboradorReporteRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            var validacion = _bl.Colaborador_Reporte_Validar(CodEmpresa, usuario, request);
            if (validacion.Code != 0)
            {
                return StatusCode(validacion.Code == -1 ? 500 : 403, validacion);
            }

            var nombres = new Dictionary<string, string>
            {
                ["boletasPago"] = "RH_Boleta_Pago",
                ["vacaciones"] = "RH_Boleta_Vacaciones",
                ["incapacidades"] = "RH_Boleta_Incapacidades",
                ["permisos"] = "RH_Boleta_Permisos",
                ["accionesPersonal"] = "RH_Boleta_Accion_Personal",
                ["traslados"] = "Activos_BoletaTraslado"
            };
            var esTraslado = request.Opcion == "traslados";
            var parametros = esTraslado
                ? new Dictionary<string, object?>
                {
                    ["filtros"] = " WHERE ACTIVOS_TRASLADOS.COD_TRASLADO = @BoletaId",
                    ["BoletaId"] = request.BoletaId.Trim(),
                    ["Empresa"] = request.NombreEmpresa,
                    ["fxUsuario"] = usuario,
                    ["fxFecha"] = DateTime.Now.ToString("dd/MM/yyyy"),
                    ["fxSubTitulo"] = "TRASLADO DE ACTIVOS Y CAMBIO DE RESPONSABLES"
                }
                : new Dictionary<string, object?>
                {
                    ["EmpleadoId"] = request.EmpleadoId.Trim(),
                    ["Empresa"] = request.NombreEmpresa,
                    ["Usuario"] = usuario,
                    ["BoletaId"] = request.BoletaId.Trim(),
                    ["CodNomina"] = request.CodNomina.Trim(),
                    ["NominaNum"] = request.NominaNum
                };

            var reporte = new FrmReporteGlobal
            {
                codEmpresa = CodEmpresa,
                parametros = JsonSerializer.Serialize(parametros),
                nombreReporte = nombres[request.Opcion],
                usuario = usuario,
                cod_reporte = "P",
                folder = esTraslado ? "Activos" : "RH"
            };
            var resultado = _reportes.ReporteRDLC_v2(reporte);

            if (resultado is not FileContentResult pdf)
            {
                return resultado;
            }

            if (!esTraslado)
            {
                var bitacora = _bl.Colaborador_Reporte_Bitacora(CodEmpresa, usuario, request);
                if (bitacora.Code != 0)
                {
                    return StatusCode(500, bitacora);
                }
            }

            var nombreArchivo = $"{reporte.nombreReporte}.pdf";
            Response.Headers["Content-Disposition"] = $"inline; filename={nombreArchivo}";
            pdf.FileDownloadName = nombreArchivo;
            return pdf;
        }

        [Authorize]
        [HttpPost("Colaborador_Solicitud_Configuracion_Obtener")]
        public ActionResult<ErrorDto<ColaboradorSolicitudConfiguracionData>> Colaborador_Solicitud_Configuracion_Obtener(
            int CodEmpresa,
            [FromBody] ColaboradorSolicitudConfiguracionRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Solicitud_Configuracion_Obtener(
                CodEmpresa,
                usuario,
                request));
        }

        [Authorize]
        [HttpPost("Colaborador_Solicitud_Dias_Obtener")]
        public ActionResult<ErrorDto<int>> Colaborador_Solicitud_Dias_Obtener(
            int CodEmpresa,
            [FromBody] ColaboradorSolicitudDiasRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Solicitud_Dias_Obtener(
                CodEmpresa,
                usuario,
                request));
        }

        [Authorize]
        [HttpPost("Colaborador_Solicitud_Registrar")]
        public ActionResult<ErrorDto<ColaboradorSolicitudRegistroData>> Colaborador_Solicitud_Registrar(
            int CodEmpresa,
            [FromBody] ColaboradorSolicitudRegistrarRequest request)
        {
            var usuario = User.FindFirst(UserNameClaim)?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Unauthorized();
            }

            return Ok(_bl.Colaborador_Solicitud_Registrar(
                CodEmpresa,
                usuario,
                request));
        }
    }
}
