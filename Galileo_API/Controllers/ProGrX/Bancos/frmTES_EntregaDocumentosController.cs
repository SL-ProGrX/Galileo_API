using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesEntregaDocumentosController : ControllerBase
    {
        private readonly FrmTesEntregaDocumentosBL _bl;

        public FrmTesEntregaDocumentosController(IConfiguration config)
        {
            _bl = new FrmTesEntregaDocumentosBL(config);
        }

        [HttpGet("Tes_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaBancosDocumentos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return _bl.Tes_Bancos_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaTiposDocumentos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return _bl.Tes_Tipos_Obtener(CodEmpresa, cod_Banco);
        }

        [HttpGet("listaPendientes_Obtener")]
        public ErrorDto<List<EntregaDocumentoPendientesDto>> listaPendientes_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.listaPendientes_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_documentosPendientes_Guardar")]
        public ErrorDto TES_documentosPendientes_Guardar(int CodEmpresa, string trasladoLista, string estadoCheck, string usuario)
        {
            return _bl.TES_documentosPendientes_Guardar(CodEmpresa, trasladoLista, estadoCheck, usuario);
        }


    }
}
