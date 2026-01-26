using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PGalileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesTePlanesController : ControllerBase
    {
        private readonly FrmTesTePlanesBL PlanesBL;

        public FrmTesTePlanesController(IConfiguration config)
        {
            PlanesBL = new FrmTesTePlanesBL(config);
        }

        [HttpGet("TES_Planes_Scroll")]
        public ErrorDto<TesBancoPlanesData> TES_Planes_Scroll(int CodEmpresa, int scrollCode, string codPlan, int banco)
        {
            return PlanesBL.TES_Planes_Scroll(CodEmpresa, scrollCode, codPlan, banco);
        }

        [HttpGet("TES_PlanesConsulta_Obtener")]
        public ErrorDto<TesBancoPlanesData> TES_PlanesConsulta_Obtener(int CodEmpresa, int banco, string codPlan)
        {
            return PlanesBL.TES_PlanesConsulta_Obtener(CodEmpresa, banco, codPlan);
        }

        [HttpGet("TES_Planes_BancosGrupos_Obtener")]
        public ErrorDto<Galileo.Models.ProGrX.Bancos.TesBancosGruposData> TES_Planes_BancosGrupos_Obtener(int CodEmpresa, int banco)
        {
            return PlanesBL.TES_Planes_BancosGrupos_Obtener(CodEmpresa, banco);
        }

        [HttpPost("TES_Planes_Guardar")]
        public ErrorDto TES_Planes_Guardar(int CodEmpresa, string infoPlan)
        {
            return PlanesBL.TES_Planes_Guardar(CodEmpresa, infoPlan);
        }

        [HttpDelete("TES_Planes_Borrar")]
        public ErrorDto TES_Planes_Borrar(int CodEmpresa, string infoPlan)
        {
            return PlanesBL.TES_Planes_Borrar(CodEmpresa, infoPlan);
        }
    }
}