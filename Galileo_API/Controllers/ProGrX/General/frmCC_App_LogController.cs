using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmCC_App_Log")]
    [ApiController]
    [Authorize]
    public class FrmCcAppLogController :
        ControllerBase
    {
        private readonly FrmCcAppLogBl _bl;

        public FrmCcAppLogController(
            IConfiguration config)
        {
            _bl = new FrmCcAppLogBl(config);
        }

        [HttpGet(
            "CC_App_Log_Estadistica_Obtener")]
        public ErrorDto<List<EstadisticaData>>
            CC_App_Log_Estadistica_Obtener(
                int CodEmpresa,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_App_Log_Estadistica_Obtener(
                    CodEmpresa,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpGet(
            "CC_App_Log_Estadistica_Detalle_Obtener")]
        public ErrorDto<List<EstadisticaDetalleData>>
            CC_App_Log_Estadistica_Detalle_Obtener(
                int CodEmpresa,
                string Codigo,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_App_Log_Estadistica_Detalle_Obtener(
                    CodEmpresa,
                    Codigo,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpGet(
            "CC_App_Log_Estadistica_Analisis_Obtener")]
        public ErrorDto<List<EstadisticaAnalisisData>>
            CC_App_Log_Estadistica_Analisis_Obtener(
                int CodEmpresa,
                string FechaInicio,
                string FechaCorte,
                int Ingreso)
        {
            return _bl
                .CC_App_Log_Estadistica_Analisis_Obtener(
                    CodEmpresa,
                    FechaInicio,
                    FechaCorte,
                    Ingreso);
        }
    }
}