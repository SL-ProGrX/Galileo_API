using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PgxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesAccesosController : ControllerBase
    {

        private readonly FrmTesAccesosBL _AccesosBL;
        public FrmTesAccesosController(IConfiguration config)
        {
            _AccesosBL = new FrmTesAccesosBL(config);
        }

        
        [HttpGet("TES_AccesosBancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosBancos_Obtener(int CodEmpresa)
        {
            return _AccesosBL.Tes_AccesosBancos_Obtener(CodEmpresa);
        }

        
        [HttpGet("TES_AccesosCuentas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosCuentas_Obtener(int CodEmpresa, string cod_banco)
        {
            return _AccesosBL.Tes_AccesosCuentas_Obtener(CodEmpresa, cod_banco);
        }

        
        [HttpGet("Tes_AccesosUsuarioBuscar_Obtener")]
        public ErrorDto<TesAccesosUsuariosLista> Tes_AccesosUsuarioBuscar_Obtener(int CodEmpresa, string filtro)
        {
            return _AccesosBL.Tes_AccesosUsuarioBuscar_Obtener(CodEmpresa, filtro);
        }

        
        [HttpGet("Tes_AccesosUsuarioBuscar_scroll")]
        public ErrorDto<DropDownListaGenericaModel> Tes_AccesosUsuarioBuscar_scroll(int CodEmpresa, string nombre, int? scroll)
        {
            return _AccesosBL.Tes_AccesosUsuarioBuscar_scroll(CodEmpresa, nombre, scroll);
        }

        #region Cuentas

        
        [HttpGet("Tes_AccesosUsuarios_Obtener")]
        public ErrorDto<List<TesAccesosUsuariosData>> Tes_AccesosUsuarios_Obtener(int CodEmpresa, int cod_banco)
        {
            return _AccesosBL.Tes_AccesosUsuarios_Obtener(CodEmpresa, cod_banco);
        }

        
        [HttpPost("Tes_AccesosCuentas_Asignar")]
        public ErrorDto Tes_AccesosCuentas_Asignar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosBL.Tes_AccesosCuentas_Asignar(CodEmpresa, id_banco, nombre);
        }

        
        [HttpDelete("Tes_AccesosCuentas_Eliminar")]
        public ErrorDto Tes_AccesosCuentas_Eliminar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosBL.Tes_AccesosCuentas_Eliminar(CodEmpresa, id_banco, nombre);
        }


        #endregion

        #region Usuarios

        
        [HttpGet("Tes_AccesosUserBancos_Obtener")]
        public ErrorDto<List<TesAccesosBancosData>> Tes_AccesosUserBancos_Obtener(int CodEmpresa, string nombre, string cod_grupo)
        {
            return _AccesosBL.Tes_AccesosUserBancos_Obtener(CodEmpresa, nombre, cod_grupo);
        }

        
        [HttpPost("Tes_AccesosUsuarios_Asignar")]
        public ErrorDto Tes_AccesosUsuarios_Asignar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosBL.Tes_AccesosUsuarios_Asignar(CodEmpresa, id_banco, nombre);
        }

        
        [HttpDelete("Tes_AccesosUsuarios_Eliminar")]
        public ErrorDto Tes_AccesosUsuarios_Eliminar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosBL.Tes_AccesosUsuarios_Eliminar(CodEmpresa, id_banco, nombre);
        }

        #endregion

        #region Accesos

        
        [HttpGet("Tes_AccesosBancoUser_Obtener")]
        public ErrorDto<List<TesAccesosBancosData>> Tes_AccesosBancoUser_Obtener(int CodEmpresa, string nombre)
        {
            return _AccesosBL.Tes_AccesosBancoUser_Obtener(CodEmpresa, nombre);
        }

        
        [HttpGet("Tes_AccesosDocumentos_Obtener")]
        public ErrorDto<List<TesAccesosDocumentosData>> Tes_AccesosDocumentos_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            return _AccesosBL.Tes_AccesosDocumentos_Obtener(CodEmpresa, usuario, id_banco);
        }
        
        [HttpGet("Tes_AccesosConceptos_Obtener")]
        public ErrorDto<List<TesAccesosConceptosData>> Tes_AccesosConceptos_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            return _AccesosBL.Tes_AccesosConceptos_Obtener(CodEmpresa, usuario, id_banco);
        }
        
        [HttpGet("Tes_AccesosUnidades_Obtener")]
        public ErrorDto<List<TesAccesosUnidadesData>> Tes_AccesosUnidades_Obtener(int CodEmpresa, string usuario, int id_banco, int contabilidad)
        {
            return _AccesosBL.Tes_AccesosUnidades_Obtener(CodEmpresa, usuario, id_banco, contabilidad);
        }

        
        [HttpGet("Tes_AccesosFirmas_Obtener")]
        public ErrorDto<TesAccesosFirmasData> Tes_AccesosFirmas_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            return _AccesosBL.Tes_AccesosFirmas_Obtener(CodEmpresa, id_banco, usuario);
        }

        
        [HttpPost("Tes_AccesosDocumentos_Guardar")]
        public ErrorDto Tes_AccesosDocumentos_Guardar(int CodEmpresa, string usuario, int id_banco, TesAccesosDocumentosData documento)
        {
            return _AccesosBL.Tes_AccesosDocumentos_Guardar(CodEmpresa, usuario, id_banco, documento);
        }

        
        [HttpPost("Tes_AccesosConceptos_Guardar")]
        public ErrorDto Tes_AccesosConceptos_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked, TesAccesosConceptosData concepto)
        {
            return _AccesosBL.Tes_AccesosConceptos_Guardar(CodEmpresa, usuario, id_banco, itemChecked, concepto);
        }

        
        [HttpPost("Tes_AccesosUnidades_Guardar")]
        public ErrorDto Tes_AccesosUnidades_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked, TesAccesosUnidadesData unidad)
        {
            return _AccesosBL.Tes_AccesosUnidades_Guardar(CodEmpresa, usuario, id_banco, itemChecked, unidad);
        }

        
        [HttpPost("Tes_AccesosFirmas_Guardar")]
        public ErrorDto Tes_AccesosFirmas_Guardar(int CodEmpresa, TesAccesosFirmasData firmas)
        {
            return _AccesosBL.Tes_AccesosFirmas_Guardar(CodEmpresa, firmas);
        }

        #endregion

        #region Copia

        
        [HttpPost("Tes_AccesosUsuarios_Copiar")]
        public ErrorDto Tes_AccesosUsuarios_Copiar(int CodEmpresa, string usuarioOrigen, string usuarioDestino)
        {
            return _AccesosBL.Tes_AccesosUsuarios_Copiar(CodEmpresa, usuarioOrigen, usuarioDestino);
        }

        
        [HttpDelete("Tes_AccesosUsuarios_EliminarInactivos")]
        public ErrorDto Tes_AccesosUsuarios_EliminarInactivos(int CodEmpresa)
        {
            return _AccesosBL.Tes_AccesosUsuarios_EliminarInactivos(CodEmpresa);
        }

        #endregion
    }
}
