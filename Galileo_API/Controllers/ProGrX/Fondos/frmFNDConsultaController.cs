using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndConsultaController : ControllerBase
    {
        private readonly FrmFndConsultaBl BlFndConsulta;

        public FrmFndConsultaController(IConfiguration config)
        {
            BlFndConsulta = new FrmFndConsultaBl(config);
        }

        [Authorize]
        [HttpGet("FND_Consulta_Obtener")]
        public ErrorDto<List<FndConsultaDto>> FND_Consulta_Obtener(int CodEmpresa, string Filtros)
        {
            return BlFndConsulta.FND_Consulta_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpGet("FND_Consulta_Operadora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Consulta_Operadora_Obtener(int CodEmpresa)
        {
            return BlFndConsulta.FND_Consulta_Operadora_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_Consulta_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Consulta_Planes_Obtener(int CodEmpresa, int? Operadora)
        {
            return BlFndConsulta.FND_Consulta_Planes_Obtener(CodEmpresa, Operadora);
        }
    }
}