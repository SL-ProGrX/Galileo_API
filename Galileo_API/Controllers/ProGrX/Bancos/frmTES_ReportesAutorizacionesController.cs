using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesReportesAutorizacionesController : ControllerBase
    {
        private readonly FrmTesReportesAutorizacionesBL _reportesBL;

        public FrmTesReportesAutorizacionesController(IConfiguration config)
        {
            _reportesBL = new FrmTesReportesAutorizacionesBL(config);
        }

        [HttpGet("sbTesBancoCargaCboAccesoGeneral")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return _reportesBL.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        [HttpGet("sbTesTiposDocsCargaCbo")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return _reportesBL.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        [HttpGet("Tes_RepAuthUsuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_RepAuthUsuarios_Obtener(int CodEmpresa)
        {
            return _reportesBL.Tes_RepAuthUsuarios_Obtener(CodEmpresa);
        }
    }
}
