using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesDesAutorizacionesController : ControllerBase
    {
        private readonly FrmTesDesAutorizacionesBL DesAutorizacionesBL;

        public FrmTesDesAutorizacionesController(IConfiguration config)
        {
            DesAutorizacionesBL = new FrmTesDesAutorizacionesBL(config);
        }

        [HttpGet("TES_DesAutorizaciones_Ctas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_DesAutorizaciones_Ctas_Obtener(int CodEmpresa, string usuario)
        {
            return DesAutorizacionesBL.TES_DesAutorizaciones_Ctas_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("TES_DesAutorizaciones_TiposDocs_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_DesAutorizaciones_TiposDocs_Obtener(int CodEmpresa, string usuario, int banco, int tipo_autorizacion)
        {
            return DesAutorizacionesBL.TES_DesAutorizaciones_TiposDocs_Obtener(CodEmpresa, usuario, banco, tipo_autorizacion);
        }

        [HttpGet("TES_DesAutorizaciones_Obtener")]
        public ErrorDto<TesSolicitudesLista> TES_DesAutorizaciones_Obtener(int CodEmpresa, string filtros)
        {
            return DesAutorizacionesBL.TES_DesAutorizaciones_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_DesAutorizaciones_Aplicar")]
        public ErrorDto TES_DesAutorizaciones_Aplicar(int CodEmpresa, string clave, string usuario, int tipo_autorizacion, string solicitudesLista)
        {
            return DesAutorizacionesBL.TES_DesAutorizaciones_Aplicar(CodEmpresa, clave, usuario, tipo_autorizacion, solicitudesLista);
        }
    }
}