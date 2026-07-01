using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvPaquetesController : ControllerBase
    {
       private readonly FrmInvPaquetesBL _bl;

        public FrmInvPaquetesController(IConfiguration config)
        {
            _bl = new FrmInvPaquetesBL(config);
        }

        [HttpGet("Paquetes_Obtener")]
        public ErrorDto<PaqueteDataLista> Paquetes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Paquetes_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Paquetes_ObtenerTodos")]
        public ErrorDto<List<PaqueteDto>> Paquetes_ObtenerTodos(int CodEmpresa)
        {
            return _bl.Paquetes_ObtenerTodos(CodEmpresa);
        }

        [HttpGet("Paquete_Obtener")]
        public ErrorDto<PaqueteDto> Paquete_Obtener(int CodEmpresa, int Cod_Paquete)
        {
            return _bl.Paquete_Obtener(CodEmpresa, Cod_Paquete);
        }

        [HttpGet("Paquete_ObtenerDetalles")]
        public ErrorDto<List<PaqueteDetalleDto>> Paquete_ObtenerDetalles(int CodEmpresa, int Cod_Paquete)
        {
            return _bl.Paquete_ObtenerDetalles(CodEmpresa, Cod_Paquete);
        }

        [HttpPost("Paquete_Insertar")]
        public ErrorDto Paquete_Insertar(int CodEmpresa, PaqueteDto request)
        {
            return _bl.Paquete_Insertar(CodEmpresa, request);
        }

        [HttpPost("Paquete_Actualizar")]
        public ErrorDto Paquete_Actualizar(int CodEmpresa, PaqueteDto request)
        {
            return _bl.Paquete_Actualizar(CodEmpresa, request);
        }

        [HttpPost("PaqueteDetalle_Insertar")]
        public ErrorDto PaqueteDetalle_Insertar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return _bl.PaqueteDetalle_Insertar(CodEmpresa, request);
        }

        [HttpPost("PaqueteDetalle_Actualizar")]
        public ErrorDto PaqueteDetalle_Actualizar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return _bl.PaqueteDetalle_Actualizar(CodEmpresa, request);
        }

        [HttpPost("PaqueteDetalle_Eliminar")]
        public ErrorDto PaqueteDetalle_Eliminar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return _bl.PaqueteDetalle_Eliminar(CodEmpresa, request);
        }
    }
}