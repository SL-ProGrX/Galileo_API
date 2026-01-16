using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesBancosDocController : ControllerBase
    {
        private readonly FrmTesBancosDocBL _bancosDocBL;

        public FrmTesBancosDocController(IConfiguration config)
        {
            _bancosDocBL = new FrmTesBancosDocBL(config);
        }

        [HttpGet("Tes_BancoDocGrupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancoDocGrupos_Obtener(int CodEmpresa)
        {
            return _bancosDocBL.Tes_BancoDocGrupos_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_BancoDocBancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancoDocBancos_Obtener(int CodEmpresa, string CodGrupo)
        {
            return _bancosDocBL.Tes_BancoDocBancos_Obtener(CodEmpresa, CodGrupo);
        }

        [HttpGet("Tes_BancoDocTipos_Obtener")]
        public ErrorDto<List<TesBancosDocData>> Tes_BancoDocTipos_Obtener(int CodEmpresa, string id_banco)
        {
            return _bancosDocBL.Tes_BancoDocTipos_Obtener(CodEmpresa, id_banco);
        }

        [HttpGet("Tes_BancoDoc_Obtener")]
        public ErrorDto<TesBancoDocDto> Tes_BancoDoc_Obtener(int CodEmpresa, int id_banco, string tipo)
        {
            return _bancosDocBL.Tes_BancoDoc_Obtener(CodEmpresa, id_banco, tipo);
        }

        [HttpPost("Tes_BancoDoc_Guardar")]
        public ErrorDto Tes_BancoDoc_Guardar(int CodEmpresa, string bancoDoc)
        {
            return _bancosDocBL.Tes_BancoDoc_Guardar(CodEmpresa, bancoDoc);
        }

        [HttpDelete("TesBancoDoc_Eliminar")]
        public ErrorDto TesBancoDoc_Eliminar(int CodEmpresa, int id_banco, string tipo, string usuario)
        {
            return _bancosDocBL.TesBancoDoc_Eliminar(CodEmpresa, id_banco, tipo, usuario);
        }

    }
}
