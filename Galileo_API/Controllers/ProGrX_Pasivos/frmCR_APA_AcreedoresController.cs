using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Pasivos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrApaAcreedoresController : ControllerBase
    {
        private readonly FrmCrApaAcreedoresBL _bl;

        public FrmCrApaAcreedoresController(IConfiguration config)
        {
            _bl = new FrmCrApaAcreedoresBL(config);
        }

        [HttpGet("CR_APA_Acreedores_Obtener")]
        public ErrorDto<FrmCrApaAcreedoresGridLista> CR_APA_Acreedores_Obtener(
            int codEmpresa,
            string filtro)
        {
            return _bl.CR_APA_Acreedores_Obtener(codEmpresa, filtro);
        }

        [HttpGet("CR_APA_Acreedor_Obtener")]
        public ErrorDto<FrmCrApaAcreedorDatosDto> CR_APA_Acreedor_Obtener(
    int codEmpresa,
    string cod_acreedor)
        {
            return _bl.CR_APA_Acreedor_Obtener(codEmpresa, cod_acreedor);
        }

        [HttpPost("CR_APA_Acreedor_Insertar")]
        public ErrorDto<int> CR_APA_Acreedor_Insertar(
    int codEmpresa,
    FrmCrApaAcreedorGuardarRequest request)
        {
            return _bl.CR_APA_Acreedor_Insertar(codEmpresa, request);
        }

        [HttpPut("CR_APA_Acreedor_Actualizar")]
        public ErrorDto<int> CR_APA_Acreedor_Actualizar(
            int codEmpresa,
            FrmCrApaAcreedorGuardarRequest request)
        {
            return _bl.CR_APA_Acreedor_Actualizar(codEmpresa, request);
        }

        [HttpGet("CR_APA_Bancos_Obtener")]
        public ErrorDto<List<FrmCrApaBancoDto>> CR_APA_Bancos_Obtener(int codEmpresa)
        {
            return _bl.CR_APA_Bancos_Obtener(codEmpresa);
        }

        [HttpGet("CR_APA_Banco_Obtener")]
        public ErrorDto<FrmCrApaBancoDto> CR_APA_Banco_Obtener(
            int codEmpresa,
            int id_banco)
        {
            return _bl.CR_APA_Banco_Obtener(codEmpresa, id_banco);
        }
    }
}
