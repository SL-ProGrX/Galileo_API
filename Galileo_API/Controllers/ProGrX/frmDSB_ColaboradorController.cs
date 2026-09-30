using Galileo.BusinessLogic.ProGrX;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Galileo.Controllers.ProGrX
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmDsbColaboradorController : ControllerBase
    {
        private const string UserNameClaim = "UserName";

        private readonly FrmDsbColaboradorBL _bl;

        public FrmDsbColaboradorController(IConfiguration config)
        {
            _bl = new FrmDsbColaboradorBL(config);
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
    }
}
