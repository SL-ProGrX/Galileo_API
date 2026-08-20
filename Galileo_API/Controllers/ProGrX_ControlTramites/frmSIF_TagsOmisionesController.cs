namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
    using Galileo_API.Models.ProGrX_ControlTramites;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmSifTagsOmisionesController : ControllerBase
    {
        private readonly FrmSifTagsOmisionesBL _bl;

        public FrmSifTagsOmisionesController(IConfiguration config)
        {
            _bl = new FrmSifTagsOmisionesBL(config);
        }

        [Authorize]
        [HttpGet("SifTagsOmisiones_Obtener")]
        public ErrorDto<List<SifTagsOmisionesModel>> SifTagsOmisiones_Obtener(int CodEmpresa)
        {
            return _bl.SifTagsOmisiones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("SifTagsOmisiones_Guardar")]
        public ErrorDto SifTagsOmisiones_Guardar(int CodEmpresa, SifTagsOmisionesGuardarRequest request)
        {
            return _bl.SifTagsOmisiones_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpDelete("SifTagsOmisiones_Eliminar")]
        public ErrorDto SifTagsOmisiones_Eliminar(int CodEmpresa, SifTagsOmisionesEliminarRequest request)
        {
            return _bl.SifTagsOmisiones_Eliminar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("SifTagsOmisiones_Modulos_Obtener")]
        public ErrorDto<List<SifTagsOmisionesModuloOpcion>> SifTagsOmisiones_Modulos_Obtener(int CodEmpresa)
        {
            return _bl.SifTagsOmisiones_Modulos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("SifTagsOmisiones_Asignacion_Obtener")]
        public ErrorDto<List<SifTagsOmisionesAsignacionModel>> SifTagsOmisiones_Asignacion_Obtener(
            int CodEmpresa,
            string Cod_Modulo)
        {
            return _bl.SifTagsOmisiones_Asignacion_Obtener(CodEmpresa, Cod_Modulo);
        }

        [Authorize]
        [HttpPost("SifTagsOmisiones_Asignacion_Guardar")]
        public ErrorDto SifTagsOmisiones_Asignacion_Guardar(
            int CodEmpresa,
            SifTagsOmisionesAsignacionRequest request)
        {
            return _bl.SifTagsOmisiones_Asignacion_Guardar(CodEmpresa, request);
        }
    }
}
