using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Alertas_Tipos")]
    [Route("api/FrmPresAlertasTipos")]
    [ApiController]
    public class FrmPresAlertasTiposController : ControllerBase
    {
        readonly FrmPresAlertasTiposBl _bl;

        public FrmPresAlertasTiposController(IConfiguration config)
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

        [Authorize]
        [HttpGet("AlertasTiposJustificacion_Obtener")]
        public ErrorDto<AlertasTiposJustificacionLista> AlertasTiposJustificacion_Obtener(int CodCliente, string id_justificacion, string? filtro)
        {
            return _bl.AlertasTiposJustificacion_Obtener(CodCliente, id_justificacion, filtro);
        }

        [Authorize]
        [HttpPost("AlertasTiposJustificacion_Guardar")]
        public ErrorDto AlertasTiposJustificacion_Guardar(int CodCliente, [FromBody] AlertasTiposJustificacionDto request)
        {
            return _bl.AlertasTiposJustificacion_Guardar(CodCliente, request);
        }

        [Authorize]
        [HttpPost("AlertasTiposJustificacion_Eliminar")]
        public ErrorDto AlertasTiposJustificacion_Eliminar(int CodCliente, [FromBody] AlertasTiposJustificacionEliminarRequest request)
        {
            return _bl.AlertasTiposJustificacion_Eliminar(CodCliente, request);
        }

        [Authorize]
        [HttpGet("AlertasTiposDetalle_Obtener")]
        public ErrorDto<AlertasTiposDetalleLista> AlertasTiposDetalle_Obtener(int CodCliente, string cod_desviacion, string? filtro)
        {
            return _bl.AlertasTiposDetalle_Obtener(CodCliente, cod_desviacion, filtro);
        }

        [Authorize]
        [HttpPost("AlertasTiposDetalle_Guardar")]
        public ErrorDto AlertasTiposDetalle_Guardar(int CodCliente, [FromBody] AlertasTiposDetalleDto request)
        {
            return _bl.AlertasTiposDetalle_Guardar(CodCliente, request);
        }

        [Authorize]
        [HttpPost("AlertasTiposDetalle_Eliminar")]
        public ErrorDto AlertasTiposDetalle_Eliminar(int CodCliente, [FromBody] AlertasTiposDetalleEliminarRequest request)
        {
            return _bl.AlertasTiposDetalle_Eliminar(CodCliente, request);
        }
    }
}
