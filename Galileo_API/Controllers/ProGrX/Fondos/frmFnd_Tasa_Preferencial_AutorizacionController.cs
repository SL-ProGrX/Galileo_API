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
    public class FrmFndTasaPreferencialAutorizacionController : ControllerBase
    {
        private readonly FrmFndTasaPreferencialAutorizacionBl _bl;

        public FrmFndTasaPreferencialAutorizacionController(IConfiguration config)
        {
            _bl = new FrmFndTasaPreferencialAutorizacionBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_TasaPref_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_TasaPref_Obtener(int CodEmpresa, bool Exporta, string Data, string Filtros)
        {
            return _bl.Fnd_TasaPref_Obtener(CodEmpresa, Exporta, Data, Filtros);
        }

        [Authorize]
        [HttpPost("Fnd_TasaPref_Autorizar")]
        public ErrorDto Fnd_TasaPref_Autorizar(int CodEmpresa, string Gestion, string Autorizador, List<FndTPListDto> Gestiones)
        {
            return _bl.Fnd_TasaPref_Autorizar(CodEmpresa, Gestion, Autorizador, Gestiones);
        }
    }
}