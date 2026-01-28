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
    public class FrmTesDocumentosDupController : ControllerBase
    {
        private readonly FrmTesDocumentesDupBL _bl;

        public FrmTesDocumentosDupController(IConfiguration config)
        {
            _bl = new FrmTesDocumentesDupBL(config);
        }

        [HttpGet("Tes_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaBancos>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return _bl.Tes_Bancos_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaTipos>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return _bl.Tes_Tipos_Obtener(CodEmpresa, cod_Banco);
        }

        [HttpGet("Documentos_Duplicados_Obtener")]
        public ErrorDto<List<DocumentoDuplicadosLista>> Documentos_Duplicados_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Documentos_Duplicados_Obtener(CodEmpresa, filtros);
        }
    }
}
