namespace Galileo_API.Controllers.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrBitacoraEspecialController : ControllerBase
    {
        private readonly FrmCrBitacoraEspecialBL _bl;

        public FrmCrBitacoraEspecialController(IConfiguration config)
        {
            _bl = new FrmCrBitacoraEspecialBL(config);
        }

        [Authorize]
        [HttpGet("CrBitacoraEspecial_Socios_Obtener")]
        public ErrorDto<List<CrBitacoraEspecialSocioModel>> CrBitacoraEspecial_Socios_Obtener(int CodEmpresa)
        {
            return _bl.CrBitacoraEspecial_Socios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CrBitacoraEspecial_Usuarios_Obtener")]
        public ErrorDto<List<CrBitacoraEspecialUsuarioModel>> CrBitacoraEspecial_Usuarios_Obtener(int CodEmpresa)
        {
            return _bl.CrBitacoraEspecial_Usuarios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CrBitacoraEspecial_Movimientos_Obtener")]
        public ErrorDto<List<CrBitacoraEspecialMovimientoModel>> CrBitacoraEspecial_Movimientos_Obtener(int CodEmpresa)
        {
            return _bl.CrBitacoraEspecial_Movimientos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CrBitacoraEspecial_Registros_Obtener")]
        public ErrorDto<List<CrBitacoraEspecialRegistroModel>> CrBitacoraEspecial_Registros_Obtener(int CodEmpresa, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            return _bl.CrBitacoraEspecial_Registros_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CrBitacoraEspecial_Asignar")]
        public ErrorDto CrBitacoraEspecial_Asignar(int CodEmpresa, CrBitacoraEspecialAsignarRequest request)
        {
            return _bl.CrBitacoraEspecial_Asignar(CodEmpresa, request);
        }
    }
}
