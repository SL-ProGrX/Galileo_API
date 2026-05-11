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

        [HttpGet("CR_APA_Contactos_Obtener")]
        public ErrorDto<FrmCrApaContactosListaDto> CR_APA_Contactos_Obtener(
    int codEmpresa,
    string cod_acreedor,
    string filtro)
        {
            return _bl.CR_APA_Contactos_Obtener(codEmpresa, cod_acreedor, filtro);
        }

        [HttpPost("CR_APA_Contacto_Guardar")]
        public ErrorDto<int> CR_APA_Contacto_Guardar(
            int codEmpresa,
            FrmCrApaContactoGuardarRequest request)
        {
            return _bl.CR_APA_Contacto_Guardar(codEmpresa, request);
        }

        [HttpDelete("CR_APA_Contacto_Eliminar")]
        public ErrorDto<int> CR_APA_Contacto_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string codigo)
        {
            return _bl.CR_APA_Contacto_Eliminar(codEmpresa, cod_acreedor, codigo);
        }

        [HttpGet("CR_APA_Autorizados_Obtener")]
        public ErrorDto<FrmCrApaAutorizadosListaDto> CR_APA_Autorizados_Obtener(
    int codEmpresa,
    string cod_acreedor,
    string filtro)
        {
            return _bl.CR_APA_Autorizados_Obtener(codEmpresa, cod_acreedor, filtro);
        }

        [HttpPost("CR_APA_Autorizado_Guardar")]
        public ErrorDto<int> CR_APA_Autorizado_Guardar(
            int codEmpresa,
            FrmCrApaAutorizadoGuardarRequest request)
        {
            return _bl.CR_APA_Autorizado_Guardar(codEmpresa, request);
        }

        [HttpDelete("CR_APA_Autorizado_Eliminar")]
        public ErrorDto<int> CR_APA_Autorizado_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string cedula)
        {
            return _bl.CR_APA_Autorizado_Eliminar(codEmpresa, cod_acreedor, cedula);
        }

    }
}
