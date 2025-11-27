using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmPres_Alertas_TiposController : ControllerBase
    {
        readonly FrmPresAlertasTiposBl _bl;

        public frmPres_Alertas_TiposController(IConfiguration config)
        {
            _bl = new FrmPresAlertasTiposBl(config);
        }


        [Authorize]
        [HttpGet("AlertasTipos_Obtener")]
        public ErrorDto<AlertasTiposLista> AlertasTipos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.AlertasTipos_Obtener(CodCliente, pagina, paginacion, filtro);
        }


        [Authorize]
        [HttpPost("AlertasTipos_Insertar")]
        public ErrorDto AlertasTipos_Insertar(int CodCliente, AlertasTiposDto request)
        {
            return _bl.AlertasTipos_Insertar(CodCliente, request);
        }


        [Authorize]
        [HttpPost("AlertasTipos_Actualizar")]
        public ErrorDto AlertasTipos_Actualizar(int CodCliente, AlertasTiposDto request)
        {
            return _bl.AlertasTipos_Actualizar(CodCliente, request);
        }


        [Authorize]
        [HttpPost("AlertasTipos_Eliminar")]
        public ErrorDto AlertasTipos_Eliminar(int CodCliente, string tipoalerta)
        {
            return _bl.AlertasTipos_Eliminar(CodCliente, tipoalerta);
        }

    }
}
