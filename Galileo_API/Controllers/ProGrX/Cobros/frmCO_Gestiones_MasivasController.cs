using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCOGestionesMasivasController : ControllerBase
    {
        private readonly FrmCOGestionesMasivasBL _bl;

        public FrmCOGestionesMasivasController(IConfiguration config)
        {
            _bl = new FrmCOGestionesMasivasBL(config);
        }
        [Authorize]
        [HttpGet("CO_GestionesMasivas_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CO_GestionesMasivas_Usuarios_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CO_GestionesMasivas_Gestiones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Gestiones_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CO_GestionesMasivas_Gestiones_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CO_GestionesMasivas_Causas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Causas_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CO_GestionesMasivas_Causas_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CO_GestionesMasivas_Arreglos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CO_GestionesMasivas_Arreglos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CO_GestionesMasivas_Arreglos_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpPost("CO_GestionesMasivas_Cargar")]
        public ErrorDto<CoGestionesMasivasCargaResultDto> CO_GestionesMasivas_Cargar(int CodEmpresa,string usuarioSesion,[FromBody] CoGestionesMasivasCargaRequest request)
        {
            return _bl.CO_GestionesMasivas_Cargar(CodEmpresa, usuarioSesion, request);
        }
        [Authorize]
        [HttpPost("CO_GestionesMasivas_Procesar")]
        public ErrorDto CO_GestionesMasivas_Procesar(int CodEmpresa,string usuarioSesion,[FromBody] CoGestionesMasivasProcesarRequest request)
        {
            return _bl.CO_GestionesMasivas_Procesar(CodEmpresa, usuarioSesion, request);
        }
    }
}