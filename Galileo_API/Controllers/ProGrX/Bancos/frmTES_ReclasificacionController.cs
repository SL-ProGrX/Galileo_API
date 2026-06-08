using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesReclasificacionController : ControllerBase
    {
        private readonly FrmTesReclasificacionBL _ReclasificacionBL;
        public FrmTesReclasificacionController(IConfiguration config)
        {
            _ReclasificacionBL = new FrmTesReclasificacionBL(config);
        }

        
        [HttpGet("TES_ReclasificacionBancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReclasificacionBancos_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return _ReclasificacionBL.TES_ReclasificacionBancos_Obtener(CodEmpresa, usuario ,gestion);
        }

        [HttpGet("TES_Reclasificacion_Obtener")]
        public ErrorDto<TesReclasificacionDto> TES_Reclasificacion_Obtener(int CodEmpresa, int solicitud)
        {
            return _ReclasificacionBL.TES_Reclasificacion_Obtener(CodEmpresa, solicitud);
        }

        [HttpGet("TES_Reclasificacion_CuentaBanco")]
        public ErrorDto<string> TES_Reclasificacion_CuentaBanco(int CodEmpresa, int id_banco)
        {
            return _ReclasificacionBL.TES_Reclasificacion_CuentaBanco(CodEmpresa, id_banco);
        }

        [HttpGet("TES_TiposDocsCargaCboAcceso_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocsCargaCboAcceso_Obtener(int CodEmpresa, string usuario, int id_banco, string tipo)
        {
            return _ReclasificacionBL.tes_TiposDocsCargaCboAcceso_Obtener(CodEmpresa, usuario, id_banco, tipo);
        }

        [Authorize]
        [HttpPost("TES_Reclasificacion_CambiaBanco")]
        public ErrorDto TES_Reclasificacion_CambiaBanco(int CodEmpresa, TesReclasificaBancoModel data)
        {
            return _ReclasificacionBL.TES_Reclasificacion_CambiaBanco(CodEmpresa, data);
        }

        [HttpPost("TES_Reclasificacion_CambiaDocumento")]
        public ErrorDto TES_Reclasificacion_CambiaDocumento(int CodEmpresa, TesReclasificaDocumentoModel data)
        {
            return _ReclasificacionBL.TES_Reclasificacion_CambiaDocumento(CodEmpresa, data);
        }

        [HttpPost("TES_Reclasificacion_CambiaSolicitud")]
        public async Task<ErrorDto> TES_Reclasificacion_CambiaSolicitud(int CodEmpresa, TesReclasificaSolicitudModel data)
        {
            return await _ReclasificacionBL.TES_Reclasificacion_CambiaSolicitud(CodEmpresa, data);
        }

        [HttpGet("TES_Solicitudes_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, string filtro)
        {
            return _ReclasificacionBL.TES_Solicitudes_Obtener(CodEmpresa, filtro);
        }

        [HttpGet("TiposIdentificacion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            return _ReclasificacionBL.TiposIdentificacion_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_ReclasificaId_Valida")]
        public ErrorDto<bool> Tes_ReclasificaId_Valida(int CodEmpresa, string? tipo)
        {
            return _ReclasificacionBL.Tes_ReclasificaId_Valida(CodEmpresa, tipo);
        }
    }
}
