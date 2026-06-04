using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrPrendasParametrosController : ControllerBase
    {
        private readonly FrmCrPrendasParametrosBL BL;

        public FrmCrPrendasParametrosController(IConfiguration config)
        {
            BL = new FrmCrPrendasParametrosBL(config);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Catalogo_Lista_Obtener")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCatalogoData>> CR_PrendasParametros_Catalogo_Lista_Obtener(int CodEmpresa, string tipo)
        {
            return BL.CR_PrendasParametros_Catalogo_Lista_Obtener(CodEmpresa, tipo);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Catalogo_Lista_Export")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCatalogoData>> CR_PrendasParametros_Catalogo_Lista_Export(int CodEmpresa, string tipo)
        {
            return BL.CR_PrendasParametros_Catalogo_Lista_Export(CodEmpresa, tipo);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Catalogos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_PrendasParametros_Catalogos_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_PrendasParametros_Catalogos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Catalogo_Guardar")]
        public ErrorDto CR_PrendasParametros_Catalogo_Guardar(int CodEmpresa, string usuario, [FromBody] CrPrendasCatalogoGuardarRequest request)
        {
            return BL.CR_PrendasParametros_Catalogo_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Catalogo_Eliminar")]
        public ErrorDto CR_PrendasParametros_Catalogo_Eliminar(int CodEmpresa, string usuario, [FromBody] CrPrendasCatalogoEliminarRequest request)
        {
            return BL.CR_PrendasParametros_Catalogo_Eliminar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Coberturas_Lista_Obtener")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> CR_PrendasParametros_Coberturas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_PrendasParametros_Coberturas_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Coberturas_Lista_Export")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> CR_PrendasParametros_Coberturas_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_PrendasParametros_Coberturas_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Coberturas_Guardar")]
        public ErrorDto CR_PrendasParametros_Coberturas_Guardar(int CodEmpresa, string usuario, [FromBody] CrPrendasCoberturaGuardarRequest request)
        {
            return BL.CR_PrendasParametros_Coberturas_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Coberturas_Eliminar")]
        public ErrorDto CR_PrendasParametros_Coberturas_Eliminar(int CodEmpresa, string usuario, [FromBody] CrPrendasCoberturaEliminarRequest request)
        {
            return BL.CR_PrendasParametros_Coberturas_Eliminar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Polizas_F4_Obtener")]
        public ErrorDto<List<CrPrendasPolizaF4Data>> CR_PrendasParametros_Polizas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CR_PrendasParametros_Polizas_F4_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Comercializa_Lista_Obtener")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> CR_PrendasParametros_Comercializa_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_PrendasParametros_Comercializa_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Comercializa_Lista_Export")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> CR_PrendasParametros_Comercializa_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_PrendasParametros_Comercializa_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Comercializa_Consulta")]
        public ErrorDto<CrPrendasComercializaData> CR_PrendasParametros_Comercializa_Consulta(int CodEmpresa, int codigo)
        {
            return BL.CR_PrendasParametros_Comercializa_Consulta(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Comercializa_Guardar")]
        public ErrorDto CR_PrendasParametros_Comercializa_Guardar(int CodEmpresa, string usuario, [FromBody] CrPrendasComercializaGuardarRequest request)
        {
            return BL.CR_PrendasParametros_Comercializa_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Comercializa_Eliminar")]
        public ErrorDto CR_PrendasParametros_Comercializa_Eliminar(int CodEmpresa, string usuario, [FromBody] CrPrendasComercializaEliminarRequest request)
        {
            return BL.CR_PrendasParametros_Comercializa_Eliminar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Comercializa_F4_Obtener")]
        public ErrorDto<List<CrPrendasComercializaF4Data>> CR_PrendasParametros_Comercializa_F4_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CR_PrendasParametros_Comercializa_F4_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_TiposId_Dropdown_Obtener")]
        public ErrorDto<List<CrPrendasTipoIdData>> CR_PrendasParametros_TiposId_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_PrendasParametros_TiposId_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Bancos_Dropdown_Obtener")]
        public ErrorDto<List<CrPrendasBancoData>> CR_PrendasParametros_Bancos_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_PrendasParametros_Bancos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Cuentas_Lista_Obtener")]
        public ErrorDto<List<CrPrendasCuentaData>> CR_PrendasParametros_Cuentas_Lista_Obtener(int CodEmpresa, string identificacion)
        {
            return BL.CR_PrendasParametros_Cuentas_Lista_Obtener(CodEmpresa, identificacion);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Unidades_Lista_Obtener")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> CR_PrendasParametros_Unidades_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_PrendasParametros_Unidades_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_PrendasParametros_Unidades_Lista_Export")]
        public ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> CR_PrendasParametros_Unidades_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_PrendasParametros_Unidades_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Unidades_Guardar")]
        public ErrorDto CR_PrendasParametros_Unidades_Guardar(int CodEmpresa, string usuario, [FromBody] CrPrendasUnidadGuardarRequest request)
        {
            return BL.CR_PrendasParametros_Unidades_Guardar(CodEmpresa, request, usuario);
        }

        [Authorize]
        [HttpPost("CR_PrendasParametros_Unidades_Eliminar")]
        public ErrorDto CR_PrendasParametros_Unidades_Eliminar(int CodEmpresa, string usuario, [FromBody] CrPrendasUnidadEliminarRequest request)
        {
            return BL.CR_PrendasParametros_Unidades_Eliminar(CodEmpresa, request, usuario);
        }
    }
}