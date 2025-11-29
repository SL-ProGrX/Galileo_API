using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Contabilidad;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using Galileo.Models.ProGrX_Contabilidad;

namespace Galileo.Controllers.ProGrX_Contabilidad
{
    [Route("api/frmPres_Formulacion_Automatica")]
    [Route("api/FrmPresFormulacionAutomatica")]
    [ApiController]
    public class FrmPresFormulacionAutomaticaController : ControllerBase
    {
        readonly FrmPresFormulacionAutomaticaBl _bl;
        public FrmPresFormulacionAutomaticaController(IConfiguration config)
        {
            _bl = new FrmPresFormulacionAutomaticaBl(config);
        }

        [Authorize]
        [HttpGet("Pres_Formulacion_Automatica_Modelos_Obtener")]
        public ErrorDto<List<PresModelisLista>> Pres_Formulacion_Automatica_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _bl.Pres_Modelos_Obtener(CodEmpresa, CodContab, Usuario);
        }

        [Authorize]
        [HttpGet("Pres_Formulacion_Automatica_Obtener")]
        public ErrorDto<List<PresFormulacionAutoDto>> Pres_Formulacion_Automatica_Obtener(
           int CodEmpresa, string CodModelo, string vTipo, string Usuario)
        {
            return _bl.Pres_Formulacion_Automatica(CodEmpresa, CodModelo, vTipo, Usuario);
        }

    }
}
