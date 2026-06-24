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
    public class FrmInvControlActivosController : ControllerBase
    {
        private readonly FrmInvControlActivosBL _bl;
        public FrmInvControlActivosController(IConfiguration config)
        {
            _bl = new FrmInvControlActivosBL(config);
        }

        [HttpGet("InvControlActivosLista_Obtener")]
        public ErrorDto<InvControlActivosLista> InvControlActivosLista_Obtener(int CodEmpresa, string usuario, string filtros)
        {
            return _bl.InvControlActivosLista_Obtener(CodEmpresa, usuario, filtros);
        }

        [HttpPost("InvControlActivos_Actualizar")]
        public ErrorDto InvControlActivos_Actualizar(int CodEmpresa, InvControlActivosDto activo)
        {
            return _bl.InvControlActivos_Actualizar(CodEmpresa, activo);
        }

        [HttpGet("InvNumeroPlacaId_Obtener")]
        public ErrorDto InvNumeroPlacaId_Obtener(int CodEmpresa)
        {
            return _bl.InvNumeroPlacaId_Obtener(CodEmpresa);
        }

        [HttpGet("InvActivosDepartamentos_Obtener")]
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosDepartamentos_Obtener(int CodEmpresa)
        {
            return _bl.InvActivosDepartamentos_Obtener(CodEmpresa);
        }

        [HttpGet("InvActivosSeccion_Obtener")]
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosSeccion_Obtener(int CodEmpresa, string? departamento)
        {
            return _bl.InvActivosSeccion_Obtener(CodEmpresa, departamento);
        }

        [HttpGet("InvActivosResponsable_Obtener")]
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosResponsable_Obtener(int CodEmpresa, string? departamento, string? seccion)
        {
            return _bl.InvActivosResponsable_Obtener(CodEmpresa, departamento, seccion);
        }

        [HttpGet("InvActivosLocalizaciones_Obtener")]
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosLocalizaciones_Obtener(int CodEmpresa)
        {
            return _bl.InvActivosLocalizaciones_Obtener(CodEmpresa);
        }

    }
}