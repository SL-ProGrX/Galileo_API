using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmPres_Ajustes_TiposController : ControllerBase
    {
        readonly FrmPresAjustesTiposBl _bl;
        public frmPres_Ajustes_TiposController(IConfiguration config)
        {
            _bl = new FrmPresAjustesTiposBl(config);
        }

        [HttpGet("PRES_AjustestTipos_Obtener")]
        [Authorize]
        public ErrorDto<PresAjustestTiposLista> PRES_AjustestTipos_Obtener(int CodEmpresa)
        {
            return _bl.PresAjustestTipos_Obtener(CodEmpresa);
        }

        [HttpPost("PRES_AjustesTipo_Eliminar")]
        [Authorize]
        public ErrorDto PresAjustesTipo_Eliminar(int CodEmpresa, string CodAjuste)
        {
            return _bl.PresAjustesTipo_Eliminar(CodEmpresa, CodAjuste);
        }

        [HttpPost("PRES_AjustesTipo_Insertar")]
        [Authorize]
        public ErrorDto PresAjustesTipo_Insertar(int CodEmpresa, PresAjustestTiposDto Info)
        {
            return _bl.PresAjustesTipo_Insertar(CodEmpresa, Info);
        }

        [HttpPost("PRES_AjustesTipo_Actualizar")]
        [Authorize]
        public ErrorDto PresAjustesTipo_Actualizar(int CodEmpresa, PresAjustestTiposDto Info)
        {
            return _bl.PresAjustesTipo_Actualizar(CodEmpresa, Info);
        }
    }
}
