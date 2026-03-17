using Galileo.BusinessLogic.ProGrX_Contabilidad;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.Models.ProGrX_Contabilidad;

namespace Galileo.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPresAlertasEstadisticasController : ControllerBase
    {
        
        private readonly FrmPresAlertasEstadisticasBL _BL;
        public FrmPresAlertasEstadisticasController(IConfiguration config)
        {
            _BL = new FrmPresAlertasEstadisticasBL(config);
        }

        [Authorize]
        [HttpGet("PresAlertasEstadisticasTipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PresAlertasEstadisticasTipos_Obtener(int CodEmpresa)
        {
            return _BL.PresAlertasEstadisticasTipos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("PresPlanning_Obtener")]
        public ErrorDto<List<PresVistaPresupuestoAlertasData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            return _BL.PresPlanning_Obtener(CodCliente, datos);
        }

        [Authorize]
        [HttpPost("PresAlertaJustificacion_Guardar")]
        public ErrorDto PresAlertaJustificacion_Guardar(int CodEmpresa, [FromBody] PresAlertaJustificacionGuardarRequest data)
        {
            return _BL.PresAlertaJustificacion_Guardar(CodEmpresa, data);
        }

        [Authorize]
        [HttpGet("PresAlertaJustificacionBitacora_Obtener")]
        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBitacora_Obtener(
            int CodEmpresa,
            int codConta,
            string codModelo,
            string codUnidad,
            string codCentroCosto,
            string codCuenta,
            int anio,
            int mes,
            string tipoAlerta)
        {
            return _BL.PresAlertaJustificacionBitacora_Obtener(
                CodEmpresa, codConta, codModelo, codUnidad, codCentroCosto, codCuenta, anio, mes, tipoAlerta
            );
        }

    }
}
