using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Credito;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRSolCreacionAgendaController : ControllerBase
    {
        private readonly FrmCRSolCreacionAgendaBL _bl;

        public FrmCRSolCreacionAgendaController(IConfiguration config)
        {
            _bl = new FrmCRSolCreacionAgendaBL(config);
        }

        [Authorize]
        [HttpGet("CR_SolCreacionAgenda_Comites_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SolCreacionAgenda_Comites_Obtener(int CodEmpresa)
        {
            return _bl.CR_SolCreacionAgenda_Comites_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CR_SolCreacionAgenda_Acta_Generar")]
        public ErrorDto<CrSolCreacionAgendaReporteData> CR_SolCreacionAgenda_Acta_Generar(int CodEmpresa, CrSolCreacionAgendaActaData acta)
        {
            return _bl.CR_SolCreacionAgenda_Acta_Generar(CodEmpresa, acta);
        }

        [Authorize]
        [HttpGet("CR_SolCreacionAgenda_Acta_Consulta")]
        public ErrorDto<int> CR_SolCreacionAgenda_Acta_Consulta(int CodEmpresa, int id_comite)
        {
            return _bl.CR_SolCreacionAgenda_Acta_Consulta(CodEmpresa, id_comite);
        }
    }
}