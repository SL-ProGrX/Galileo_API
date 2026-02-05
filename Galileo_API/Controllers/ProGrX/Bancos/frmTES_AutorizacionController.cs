using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesAutorizacionController : ControllerBase
    {
        private readonly FrmTesAutorizacionBL AutorizacionBL;
        public FrmTesAutorizacionController(IConfiguration config)
        {

            AutorizacionBL = new FrmTesAutorizacionBL(config);
        }

        [HttpGet("TES_AutorizacionBancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_AutorizacionBancos_Obtener(int CodEmpresa, string usuario) 
        {
            return AutorizacionBL.TES_AutorizacionBancos_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("TES_TiposDocsAutoriza_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocsAutoriza_Obtener(int CodEmpresa, string usuario, int banco, int tipo_autorizacion)
        {
            return AutorizacionBL.TES_TiposDocsAutoriza_Obtener(CodEmpresa, usuario, banco, tipo_autorizacion);
        }

        [HttpGet("TES_SolicitudesPendientes_Obtener")]
        public ErrorDto<TesSolicitudesLista> TES_SolicitudesPendientes_Obtener(int CodEmpresa, string filtros)
        {
            return AutorizacionBL.TES_SolicitudesPendientes_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_Autorizacion_Aplicar")]
        public ErrorDto TES_Autorizacion_Aplicar(TesAutorizaParametros nsoliictud)
        {
            return AutorizacionBL.TES_Autorizacion_Aplicar(nsoliictud);
        }

        [HttpGet("TES_AutorizacionDoc_Obtener")]
        public ErrorDto<TesAutorizacionData> TES_AutorizacionDoc_Obtener(int CodEmpresa, string usuario)
        {
            return AutorizacionBL.TES_AutorizacionDoc_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("TES_AutorizacionFirma_Obtener")]
        public ErrorDto<TesFirmasAutData> TES_AutorizacionFirma_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return AutorizacionBL.TES_AutorizacionFirma_Obtener(CodEmpresa, usuario, banco);
        }

        [HttpGet("TES_AutorizacionBuscar_Obtener")]
        public ErrorDto<TesAccesosUsuariosLista> TES_AutorizacionBuscar_Obtener(int CodEmpresa, string filtro)
        {
            return AutorizacionBL.TES_AutorizacionBuscar_Obtener(CodEmpresa, filtro);
        }
    }
}