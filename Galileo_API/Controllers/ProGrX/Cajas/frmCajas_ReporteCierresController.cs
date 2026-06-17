using Galileo.Models;
using Galileo.Models.ERROR;
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
        private readonly FrmCajasReporteCierresBl _bl;

        public FrmCajasReporteCierresController(IConfiguration config)
            => _bl = new FrmCajasReporteCierresBl(config);

        [Authorize]
        [HttpGet("Cajas_Aperturas_Consulta")]
        public ErrorDto<List<CajasAperturaReporteDto>> Cajas_Aperturas_Consulta(int codEmpresa, string? codCaja, DateTime fechaInicio,
            DateTime fechaCorte, string filtro)
        {
            return _bl.Cajas_Aperturas_Consulta(codEmpresa, codCaja ?? string.Empty, fechaInicio, fechaCorte, filtro);
        }

        [Authorize]
        [HttpGet("Cajas_Acceso_Consulta")]
        public ErrorDto<List<CajasAccesoDto>> Cajas_Acceso_Consulta(int codEmpresa, string? codCaja, DateTime fechaInicio,
           DateTime fechaCorte)
        {
            return _bl.Cajas_Acceso_Consulta(codEmpresa, codCaja ?? string.Empty, fechaInicio, fechaCorte);
        }

        [Authorize]
        [HttpGet("Cajas_Depositos_Consulta")]
        public ErrorDto<List<CajasDepositoDto>> Cajas_Depositos_Consulta(int codEmpresa, string codCaja, int codApertura)
        {
            return _bl.Cajas_Depositos_Consulta(codEmpresa, codCaja, codApertura);
        }

        [Authorize]
        [HttpPost("Cajas_Cierre_Forzado")]
        public ErrorDto<bool> Cajas_Cierre_Forzado(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return _bl.Cajas_Cierre_Forzado(codEmpresa, codCaja, codApertura, usuario);
        }

        [Authorize]
        [HttpPost("Cajas_Cierre_Recibe")]
        public ErrorDto<bool> Cajas_Cierre_Recibe(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return _bl.Cajas_Cierre_Recibe(codEmpresa, codCaja, codApertura, usuario);
        }

        [Authorize]
        [HttpPost("Cajas_Cierre_Revisa")]
        public ErrorDto<bool> Cajas_Cierre_Revisa(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return _bl.Cajas_Cierre_Revisa(codEmpresa, codCaja, codApertura, usuario
                );
        }

        [Authorize]
        [HttpGet("Cajas_Definicion_Lista")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> Cajas_Definicion_Lista(int codEmpresa)
        {
            return _bl.Cajas_Definicion_Lista(codEmpresa);
        }

        [Authorize]
        [HttpPost("Cajas_Cierre_Forzar")]
        public ActionResult<ErrorDto<bool>> Cajas_Cierre_Forzar(int codEmpresa,string codCaja,int codApertura,string usuario)
        {
            return _bl.Cajas_Cierre_Forzar(codEmpresa, codCaja, codApertura, usuario);
        }

        [Authorize]
        [HttpPost("Cajas_Cierre_Verificacion")]
        public ErrorDto<CajasCierreVerificacionDto> Cajas_Cierre_Verificacion(int codEmpresa, string codCaja, int codApertura)
        {
            return _bl.Cajas_Cierre_Verificacion(codEmpresa, codCaja, codApertura);
        }

        [Authorize]
        [HttpPost("Cajas_Cierre_Preliminar_Aplicar")]
        public ErrorDto<bool> Cajas_Cierre_Preliminar_Aplicar(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return _bl.Cajas_Cierre_Preliminar_Aplicar(codEmpresa, codCaja, codApertura, usuario);
        }
    }
}
