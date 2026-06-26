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
    public class FrmInvAsignaUbicacionController : ControllerBase
    {
        private readonly FrmInvAsignaUbicacionBL _bl;
        public FrmInvAsignaUbicacionController(IConfiguration config)
        {
            _bl = new FrmInvAsignaUbicacionBL(config);
        }

        [HttpGet("InvUbicaciones_Obtener")]
        public ErrorDto<AsignaUbicacionDto?> InvUbicaciones_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            return _bl.InvUbicaciones_Obtener(CodEmpresa, CodAsignaUbicacion);
        }

        [HttpGet("InvUbicacionProduc_Obtener")]
        public ErrorDto<List<AsignaUbicacionDetalleDto>> InvUbicacionProduc_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            return _bl.InvUbicacionProduc_Obtener(CodEmpresa, CodAsignaUbicacion);
        }

        [HttpGet("InvUbicacion_scroll")]
        public ErrorDto<AsignaUbicacionDto?> InvUbicacion_scroll(int CodEmpresa, int scrollValue, int? CodAsignaUbicacion)
        {
            return _bl.InvUbicacion_scroll(CodEmpresa, scrollValue, CodAsignaUbicacion);
        }

        [HttpPost("InvAsignaUbicacion_Insertar")]
        public ErrorDto InvAsignaUbicacion_Insertar(int CodEmpresa, AsignaUbicacionDto request)
        {
            return _bl.InvAsignaUbicacion_Insertar(CodEmpresa, request);
        }

        [HttpPost("InvAsignaUbicacion_Actualizar")]
        public ErrorDto InvAsignaUbicacion_Actualizar(int CodEmpresa, AsignaUbicacionDto request)
        {
            return _bl.InvAsignaUbicacion_Actualizar(CodEmpresa, request);
        }

        [HttpPost("InvAsignaUbicacion_Eliminar")]
        public ErrorDto InvAsignaUbicacion_Eliminar(int CodEmpresa, int CodAsignaUbicacion)
        {
            return _bl.InvAsignaUbicacion_Eliminar(CodEmpresa, CodAsignaUbicacion);
        }

        [HttpPost("InvAsignaUbicacionProduc_Insertar")]
        public ErrorDto InvRequesicionProduc_Insertar(int CodEmpresa, int CodRequisicion, List<AsignaUbicacionDetalleDto> producLineas)
        {
            return _bl.InvAsignaUbicacionProduc_Insertar(CodEmpresa, CodRequisicion, producLineas);
        }

        [HttpGet("InvAsignaUbicacion_Lista")]
        public ErrorDto<List<AsignaUbicacionDto>> InvAsignaUbicacion_Lista(int CodEmpresa)
        {
            return _bl.InvAsignaUbicacion_Lista(CodEmpresa);
        }

        [HttpPost("InvAsignacionUbicacion_CerrarOrden_Finalizar")]
        public ErrorDto InvAsignacionUbicacion_CerrarOrden_Finalizar(int CodEmpresa, int codigoAsignaUbicacion, string Usuario, string Estado)
        {
            return _bl.InvAsignacionUbicacion_CerrarOrden_Finalizar(CodEmpresa, codigoAsignaUbicacion, Usuario, Estado);
        }

        [HttpPost("InvAsignaUbicacionnProduc_Eliminar")]
        public ErrorDto InvAsignaUbicacionnProduc_Eliminar(int CodEmpresa, int CodAsignaUbicacion, int Linea)
        {
            return _bl.InvAsignaUbicacionProduc_Eliminar(CodEmpresa, CodAsignaUbicacion, Linea);
        }
    }
}