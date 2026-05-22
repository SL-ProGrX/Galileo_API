using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaEstadoPreanalisisController : ControllerBase
    {
        private readonly FrmPreaEstadoPreanalisisBL _bl;

        public FrmPreaEstadoPreanalisisController(IConfiguration config)
        {
            _bl = new FrmPreaEstadoPreanalisisBL(config);
        }

        [HttpGet("Prea_frmPreaEstadoPreanalisis_Cargar")]
        public ErrorDto<FrmPreaEstadoPreanalisisCargarResponse> Prea_frmPreaEstadoPreanalisis_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string? tipo)
        {
            return _bl.Prea_frmPreaEstadoPreanalisis_Cargar(codEmpresa, usuario, cod_preanalisis, tipo ?? string.Empty);
        }

        [HttpPut("Prea_frmPreaEstadoPreanalisis_Guardar")]
        public ErrorDto<FrmPreaEstadoPreanalisisGuardarResponse> Prea_frmPreaEstadoPreanalisis_Guardar(
            int codEmpresa,
            FrmPreaEstadoPreanalisisGuardarRequest request)
        {
            return _bl.Prea_frmPreaEstadoPreanalisis_Guardar(codEmpresa, request);
        }

        [HttpPost("Prea_frmPreaEstadoPreanalisis_Causa_Registrar")]
        public ErrorDto<FrmPreaEstadoPreanalisisCausaRegistrarResponse> Prea_frmPreaEstadoPreanalisis_Causa_Registrar(
            int codEmpresa,
            FrmPreaEstadoPreanalisisCausaRegistrarRequest request)
        {
            return _bl.Prea_frmPreaEstadoPreanalisis_Causa_Registrar(codEmpresa, request);
        }
    }
}
