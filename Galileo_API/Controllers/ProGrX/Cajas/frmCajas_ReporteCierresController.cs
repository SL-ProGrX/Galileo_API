using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasReporteCierresController : ControllerBase
    {
        private readonly FrmCajasReporteCierresBl BL_Cajas_ReporteCierres;

        public FrmCajasReporteCierresController(IConfiguration config)
            => BL_Cajas_ReporteCierres = new FrmCajasReporteCierresBl(config);

        [Authorize]
        [HttpGet("Cajas_Aperturas_Consulta")]
        public ErrorDto<List<CajasAperturaReporteDto>> Cajas_Aperturas_Consulta(int codEmpresa, string codCaja, DateTime fechaInicio,
            DateTime fechaCorte, string filtro)
        {
            return BL_Cajas_ReporteCierres.Cajas_Aperturas_Consulta(codEmpresa, codCaja, fechaInicio, fechaCorte, filtro);
        }
        [Authorize]
        [HttpGet("Cajas_Acceso_Consulta")]
        public ErrorDto<List<CajasAccesoDto>> Cajas_Acceso_Consulta(int codEmpresa, string codCaja, DateTime fechaInicio,
           DateTime fechaCorte)
        {
            return BL_Cajas_ReporteCierres.Cajas_Acceso_Consulta(codEmpresa, codCaja, fechaInicio, fechaCorte);
        }
        [Authorize]
        [HttpGet("Cajas_Depositos_Consulta")]
        public ErrorDto<List<CajasDepositoDto>> Cajas_Depositos_Consulta(int codEmpresa, string codCaja, int codApertura)
        {
            return BL_Cajas_ReporteCierres.Cajas_Depositos_Consulta(codEmpresa, codCaja, codApertura);
        }
        [Authorize]
        [HttpPost("Cajas_Cierre_Forzado")]
        public ErrorDto<bool> Cajas_Cierre_Forzado(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return BL_Cajas_ReporteCierres.Cajas_Cierre_Forzado(codEmpresa, codCaja, codApertura, usuario);
        }
        [Authorize]
        [HttpPost("Cajas_Cierre_Recibe")]
        public ErrorDto<bool> Cajas_Cierre_Recibe(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return BL_Cajas_ReporteCierres.Cajas_Cierre_Recibe(codEmpresa, codCaja, codApertura, usuario);
        }
        [Authorize]
        [HttpPost("Cajas_Cierre_Revisa")]
        public ErrorDto<bool> Cajas_Cierre_Revisa(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return BL_Cajas_ReporteCierres.Cajas_Cierre_Revisa(codEmpresa, codCaja, codApertura, usuario
                );
        }
        [Authorize]
        [HttpGet("Cajas_Definicion_Lista")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Cajas_Definicion_Lista(int codEmpresa)
        {
            return BL_Cajas_ReporteCierres.Cajas_Definicion_Lista(codEmpresa);
        }
        [Authorize]
        [HttpPost("Cajas_Cierre_Forzar")]
        public ActionResult<ErrorDto<bool>> Cajas_Cierre_Forzar(int codEmpresa,string codCaja,int codApertura,string usuario)
        {
            return BL_Cajas_ReporteCierres.Cajas_Cierre_Forzar(codEmpresa, codCaja, codApertura, usuario);
        }

    }
}