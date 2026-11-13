using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreAnalisisController : ControllerBase
    {
        private readonly FrmPreAnalisisBL _bl;

        public FrmPreAnalisisController(IConfiguration config)
        {
            _bl = new FrmPreAnalisisBL(config);
        }

        [HttpGet("Prea_frmPreAnalisis_Cargar")]
        public ErrorDto<FrmPreAnalisisCargarResponse> Prea_frmPreAnalisis_Cargar(int codEmpresa, long operacion)
        {
            return _bl.Prea_frmPreAnalisis_Cargar(codEmpresa, operacion);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Prea_frmPreAnalisis_Calcular")]
        public ErrorDto<FrmPreAnalisisCalcularResponse> Prea_frmPreAnalisis_Calcular(
            int codEmpresa,
            FrmPreAnalisisCalcularRequest request)
        {
            return _bl.Prea_frmPreAnalisis_Calcular(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Prea_frmPreAnalisis_Deudor_Guardar")]
        public ErrorDto<FrmPreAnalisisDeudorGuardarResponse> Prea_frmPreAnalisis_Deudor_Guardar(
            int codEmpresa,
            FrmPreAnalisisDeudorGuardarRequest request)
        {
            return _bl.Prea_frmPreAnalisis_Deudor_Guardar(codEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPut("Prea_frmPreAnalisis_Observaciones_Guardar")]
        public ErrorDto Prea_frmPreAnalisis_Observaciones_Guardar(
            int codEmpresa,
            FrmPreAnalisisObservacionesGuardarRequest request)
        {
            return _bl.Prea_frmPreAnalisis_Observaciones_Guardar(codEmpresa, request);
        }

        [HttpGet("Prea_frmPreAnalisis_Operaciones_Lista")]
        public ErrorDto<TablasListaGenericaModel> Prea_frmPreAnalisis_Operaciones_Lista(int codEmpresa, string filtros)
        {
            return _bl.Prea_frmPreAnalisis_Operaciones_Lista(codEmpresa, filtros);
        }

        [HttpGet("Prea_frmPreAnalisis_Comites_Lista")]
        public ErrorDto<List<FrmPreAnalisisComiteData>> Prea_frmPreAnalisis_Comites_Lista(int codEmpresa)
        {
            return _bl.Prea_frmPreAnalisis_Comites_Lista(codEmpresa);
        }

        [HttpGet("Prea_frmPreAnalisis_Reporte_Solicitudes")]
        public ErrorDto<List<long>> Prea_frmPreAnalisis_Reporte_Solicitudes(int codEmpresa, int idComite, int acta)
        {
            return _bl.Prea_frmPreAnalisis_Reporte_Solicitudes(codEmpresa, idComite, acta);
        }

        [HttpGet("Prea_frmPreAnalisis_Reporte_Parametros")]
        public ErrorDto<Dictionary<string, string>> Prea_frmPreAnalisis_Reporte_Parametros(
            int codEmpresa,
            long operacion,
            string tipo)
        {
            return _bl.Prea_frmPreAnalisis_Reporte_Parametros(codEmpresa, operacion, tipo);
        }
    }
}
