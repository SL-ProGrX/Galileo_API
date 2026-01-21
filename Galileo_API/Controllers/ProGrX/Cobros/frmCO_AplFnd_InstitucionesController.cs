using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Cobros;

namespace PgxAPI.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOAplFndInstitucionesController : ControllerBase
    {
        private readonly FrmCOAplFndInstitucionesBL _bl;

        public FrmCOAplFndInstitucionesController(IConfiguration config)
            => _bl = new FrmCOAplFndInstitucionesBL(config);

        [Authorize]
        [HttpGet("Co_AplFnd_Instituciones_Lista_Obtener")]
        public ErrorDto<CoAplFndInstitucionesListaResult> Co_AplFnd_Instituciones_Lista_Obtener(
            int CodEmpresa,
            string usuario,
            string filtros)
        {
            return _bl.Co_AplFnd_Instituciones_Lista_Obtener(CodEmpresa, usuario, filtros);
        }

        [Authorize]
        [HttpGet("Co_AplFnd_Instituciones_Lista_Export")]
        public ErrorDto<CoAplFndInstitucionesListaResult> Co_AplFnd_Instituciones_Lista_Export(
            int CodEmpresa,
            string usuario,
            string filtros)
        {
            return _bl.Co_AplFnd_Instituciones_Lista_Export(CodEmpresa, usuario, filtros);
        }

        [Authorize]
        [HttpPost("Co_AplFnd_Instituciones_Actualizar")]
        public ErrorDto Co_AplFnd_Instituciones_Actualizar(
            int CodEmpresa,
            CoAplFndInstitucionesActualizarRequest req)
        {
            return _bl.Co_AplFnd_Instituciones_Actualizar(CodEmpresa, req);
        }
    }
}
