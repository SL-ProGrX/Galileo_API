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
    [Authorize]
    public class FrmDsbDashboardController : ControllerBase
    {
        private readonly FrmDsbDashboardBL _bl;

        public FrmDsbDashboardController(IConfiguration config)
        {
            _bl = new FrmDsbDashboardBL(config);
        }

        [HttpGet("Categorias_Obtener")]
        public ActionResult<ErrorDto<List<DashboardCategoriaData>>> Categorias_Obtener(int CodEmpresa)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Categorias_Obtener(CodEmpresa, usuario));
        }

        [HttpGet("Clientes_Obtener")]
        public ActionResult<ErrorDto<DashboardClientesData>> Clientes_Obtener(int CodEmpresa)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Clientes_Obtener(CodEmpresa, usuario));
        }

        [HttpGet("Clientes_Tendencia_Obtener")]
        public ActionResult<ErrorDto<List<DashboardClientesPuntoData>>> Clientes_Tendencia_Obtener(
            int CodEmpresa, string Origen, string Indicador, string? Filtro = null)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Clientes_Tendencia_Obtener(CodEmpresa, usuario, Origen, Indicador, Filtro));
        }

        [HttpGet("Clientes_Top_Obtener")]
        public ActionResult<ErrorDto<List<DashboardTopFilaData>>> Clientes_Top_Obtener(
            int CodEmpresa, string Codigo, int Dias = 30, int Cantidad = 10)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Clientes_Top_Obtener(CodEmpresa, usuario, Codigo, Dias, Cantidad));
        }

        [HttpGet("Creditos_Obtener")]
        public ActionResult<ErrorDto<DashboardCreditosData>> Creditos_Obtener(int CodEmpresa)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Creditos_Obtener(CodEmpresa, usuario));
        }

        [HttpGet("Creditos_Tendencia_Obtener")]
        public ActionResult<ErrorDto<List<DashboardClientesPuntoData>>> Creditos_Tendencia_Obtener(
            int CodEmpresa, string Origen, string Indicador, string? Filtro = null)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Creditos_Tendencia_Obtener(CodEmpresa, usuario, Origen, Indicador, Filtro));
        }

        [HttpGet("Creditos_Top_Obtener")]
        public ActionResult<ErrorDto<List<DashboardTopFilaData>>> Creditos_Top_Obtener(
            int CodEmpresa, string Codigo, int Dias = 30, int Cantidad = 10)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Creditos_Top_Obtener(CodEmpresa, usuario, Codigo, Dias, Cantidad));
        }

        [HttpGet("Ahorros_Obtener")]
        public ActionResult<ErrorDto<DashboardAhorrosData>> Ahorros_Obtener(int CodEmpresa)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Ahorros_Obtener(CodEmpresa, usuario));
        }

        [HttpGet("Ahorros_Tendencia_Obtener")]
        public ActionResult<ErrorDto<List<DashboardClientesPuntoData>>> Ahorros_Tendencia_Obtener(
            int CodEmpresa, string Origen, string Indicador, string? Filtro = null)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Ahorros_Tendencia_Obtener(CodEmpresa, usuario, Origen, Indicador, Filtro));
        }

        [HttpGet("Ahorros_Top_Obtener")]
        public ActionResult<ErrorDto<List<DashboardTopFilaData>>> Ahorros_Top_Obtener(
            int CodEmpresa, string Codigo, int Dias = 30, int Cantidad = 10)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Ahorros_Top_Obtener(CodEmpresa, usuario, Codigo, Dias, Cantidad));
        }

        [HttpGet("Modulo_Obtener")]
        public ActionResult<ErrorDto<DashboardModuloData>> Modulo_Obtener(
            int CodEmpresa, string Categoria)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Modulo_Obtener(CodEmpresa, usuario, Categoria));
        }

        [HttpGet("Modulo_Tendencia_Obtener")]
        public ActionResult<ErrorDto<List<DashboardModuloPuntoData>>> Modulo_Tendencia_Obtener(
            int CodEmpresa, string Categoria, string Origen, string Indicador,
            string? Filtro = null)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Modulo_Tendencia_Obtener(
                CodEmpresa, usuario, Categoria, Origen, Indicador, Filtro));
        }

        [HttpGet("Modulo_Top_Obtener")]
        public ActionResult<ErrorDto<List<DashboardTopFilaData>>> Modulo_Top_Obtener(
            int CodEmpresa, string Categoria, string Codigo,
            int Dias = 30, int Cantidad = 10)
        {
            var usuario = UsuarioAutenticado();
            if (usuario is null) return Unauthorized();
            return Ok(_bl.Modulo_Top_Obtener(
                CodEmpresa, usuario, Categoria, Codigo, Dias, Cantidad));
        }

        private string? UsuarioAutenticado()
            => User.FindFirst("UserName")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
