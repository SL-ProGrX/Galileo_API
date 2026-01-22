using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesChequesBancoPuenteController : ControllerBase
    {
        private readonly FrmTesChequesBancoPuenteBL _chequesBancoPuenteBL;

        public FrmTesChequesBancoPuenteController(IConfiguration config)
        {
            _chequesBancoPuenteBL = new FrmTesChequesBancoPuenteBL(config);
        }

        
        [HttpGet("TES_BancosGestion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosGestion_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return _chequesBancoPuenteBL.TES_BancosGestion_Obtener(CodEmpresa, usuario, gestion);
        }

        [HttpGet("TES_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int CodEmpresa)
        {
            return _chequesBancoPuenteBL.TES_Bancos_Obtener(CodEmpresa);
        }

        [HttpGet("TES_ChequePuenteLista_Obtener")]
        public ErrorDto<List<ChequesBancoPuenteData>> TES_ChequePuenteLista_Obtener(int CodEmpresa, int id_banco)
        {
            return _chequesBancoPuenteBL.TES_ChequePuenteLista_Obtener(CodEmpresa, id_banco);
        }

        [HttpPost("TES_ChequesBanco_Aplica")]
        public ErrorDto TES_ChequesBanco_Aplica(int CodEmpresa, int id_banco, int banco, string usuario, List<ChequesBancoPuenteData> data)
        {
            return _chequesBancoPuenteBL.TES_ChequesBanco_Aplica(CodEmpresa,id_banco,banco,usuario,data);
        }

    }
}
