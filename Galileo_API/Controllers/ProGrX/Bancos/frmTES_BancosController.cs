using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesBancosController : ControllerBase
    {
        private readonly FrmTesBancosBL BancosBL;

        public FrmTesBancosController(IConfiguration config)
        {
            BancosBL = new FrmTesBancosBL(config);
        }

        [Authorize]
        [HttpGet("TES_Banco_Obtener")]
        public ErrorDto<TesBancoDto> TES_Banco_Obtener(int CodEmpresa, int Contabilidad, int Banco)
        {
            return BancosBL.TES_Banco_Obtener(CodEmpresa, Contabilidad, Banco);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Scroll_Obtener")]
        public ErrorDto<TesBancoDto> TES_Bancos_Scroll_Obtener(int CodEmpresa, int Contabilidad, int scrollCode, int Banco)
        {
            return BancosBL.TES_Bancos_Scroll_Obtener(CodEmpresa, Contabilidad, scrollCode, Banco);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_Bancos_Lista_Obtener(int CodEmpresa, string filtro)
        {
            return BancosBL.TES_Bancos_Lista_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Grupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int CodEmpresa)
        {
            return BancosBL.TES_Bancos_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaDivisas>> TES_Bancos_Divisas_Obtener(int CodEmpresa)
        {
            return BancosBL.TES_Bancos_Divisas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Formatos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Formatos_Obtener(int CodEmpresa)
        {
            return BancosBL.TES_Bancos_Formatos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Unidades_Obtener(int CodEmpresa)
        {
            return BancosBL.TES_Bancos_Unidades_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Bancos_CentrosCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_CentrosCostos_Obtener(int CodEmpresa)
        {
            return BancosBL.TES_Bancos_CentrosCostos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Conceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Conceptos_Obtener(int CodEmpresa)
        {
            return BancosBL.TES_Bancos_Conceptos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("TES_Bancos_Cierres_Obtener")]
        public ErrorDto<List<TesBancosCierres>> TES_Bancos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            return BancosBL.TES_Bancos_Cierres_Obtener(CodEmpresa, Banco);
        }


        [Authorize]
        [HttpPost("TES_Bancos_Guardar")]
        public ErrorDto TES_Bancos_Guardar(int CodEmpresa, bool vEdita, string Usuario, TesBancoDto Parametros)
        {
            return BancosBL.TES_Bancos_Guardar(CodEmpresa, vEdita, Usuario, Parametros);
        }

        [Authorize]
        [HttpDelete("TES_Bancos_Borrar")]
        public ErrorDto TES_Bancos_Borrar(int CodEmpresa, int Banco, string Usuario)
        {
            return BancosBL.TES_Bancos_Borrar(CodEmpresa, Banco, Usuario);
        }


        [Authorize]
        [HttpPost("TES_Bancos_RangoFirmas_Actualizar")]
        public ErrorDto TES_Bancos_RangoFirmas_Actualizar(int CodEmpresa, int Banco, int FirmaDesde, int FirmaHasta, string Usuario)
        {
            return BancosBL.TES_Bancos_RangoFirmas_Actualizar(CodEmpresa, Banco, FirmaDesde, FirmaHasta, Usuario);
        }


        [Authorize]
        [HttpPost("TES_Bancos_SaldoFecha_Actualizar")]
        public ErrorDto TES_Bancos_SaldoFecha_Actualizar(int CodEmpresa, string Parametros)
        {
            return BancosBL.TES_Bancos_SaldoFecha_Actualizar(CodEmpresa, Parametros);
        }

        [Authorize]
        [HttpPost("TES_Bancos_Conciliacion_Actualizar")]
        public ErrorDto TES_Bancos_Conciliacion_Actualizar(int CodEmpresa, string Parametros)
        {
            return BancosBL.TES_Bancos_Conciliacion_Actualizar(CodEmpresa, Parametros);
        }

        [Authorize]
        [HttpGet("TES_BancosGrupos_Lista")]
        public ErrorDto<List<TesBancosGruposAsgDto>> TES_BancosGrupos_Lista(int CodEmpresa, int id_banco)
        {
            return BancosBL.TES_BancosGrupos_Lista(CodEmpresa, id_banco);
        }

        [Authorize]
        [HttpPost("TES_BancosGrupos_Asignar")]
        public ErrorDto TES_BancosGrupos_Asignar(int CodEmpresa, int id_banco, bool asigna, TesBancosGruposAsgDto grupo)
        {
            return BancosBL.TES_BancosGrupos_Asignar(CodEmpresa, id_banco, asigna, grupo);
        }


        [HttpPost("TES_BancosArchivos_Subir")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> TES_BancosArchivos_Subir(
             int CodEmpresa,
             int CodBanco,
              string documento,
             IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo requerido.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".rdl" && ext != ".rdlc")
                return BadRequest("Extensión inválida. Solo .rdl/.rdlc.");

            ErrorDto result = await BancosBL.TES_BancosArchivos_Subir(CodEmpresa, CodBanco, documento, file);

            if (result.Code == -1)
            {
                return BadRequest("Error al guardar Archivo.");
            }

            return Ok(new { ok = true, path = result.Description });
        }

        [HttpGet("TES_BancosArchivos_DescargarDocumento")]
        public ErrorDto<ArchivoDto> DescargarDocumento(int codEmpresa, int codBanco, string documento)
        {
            return BancosBL.ResolverDocumento(codEmpresa, codBanco, documento);
        }

    }
}