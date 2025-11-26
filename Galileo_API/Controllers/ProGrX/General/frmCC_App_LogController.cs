using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/frmCC_App_Log")]
    [ApiController]
    public class FrmCcAppLogController : ControllerBase
    {
        readonly FrmCcAppLogBl App_LogBL;
        public FrmCcAppLogController(IConfiguration config)
        {
            App_LogBL = new FrmCcAppLogBl(config);
        }

        [HttpGet("CC_Estadistica_SP")]
        public List<EstadisticaData> CC_Estadistica_SP(int CodEmpresa, string FechaInicio, string FechaCorte)
        {
            return App_LogBL.CC_Estadistica_SP(CodEmpresa, FechaInicio, FechaCorte);
        }

        [HttpGet("CC_Estadistica_Detalle_SP")]
        public List<EstadisticaDetalleData> CC_Estadistica_Detalle_SP(int CodEmpresa, string Codigo, string FechaInicio, string FechaCorte)
        {
            return App_LogBL.CC_Estadistica_Detalle_SP(CodEmpresa, Codigo, FechaInicio, FechaCorte);
        }

        [HttpGet("CC_Estadistica_Analisis_SP")]
        public List<EstadisticaAnalisisData> CC_Estadistica_Analisis_SP(int CodEmpresa, string FechaInicio, string FechaCorte, int Ingreso)
        {
            return App_LogBL.CC_Estadistica_Analisis_SP(CodEmpresa, FechaInicio, FechaCorte, Ingreso);
        }
    }
}