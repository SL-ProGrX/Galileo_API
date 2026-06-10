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
    public class FrmCrRemesasCreditoController : ControllerBase
    {
        private readonly FrmCrRemesasCreditoBL BL;

        public FrmCrRemesasCreditoController(IConfiguration config)
        {
            BL = new FrmCrRemesasCreditoBL(config);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Fuente_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Fuente_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_RemesasCredito_Fuente_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Estado_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Estado_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_RemesasCredito_Estado_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Grupos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_RemesasCredito_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Usuarios_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Usuarios_Dropdown_Obtener(int CodEmpresa, string? codGrupo)
        {
            return BL.CR_RemesasCredito_Usuarios_Dropdown_Obtener(CodEmpresa, codGrupo);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Destinos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Destinos_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_RemesasCredito_Destinos_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_DestinosLinea_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_DestinosLinea_Dropdown_Obtener(int CodEmpresa, string? codigo)
        {
            return BL.CR_RemesasCredito_DestinosLinea_Dropdown_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Oficinas_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Oficinas_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_RemesasCredito_Oficinas_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Tags_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Tags_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_RemesasCredito_Tags_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Lista_Obtener")]
        public ErrorDto<CrRemesasCreditoLista> CR_RemesasCredito_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_RemesasCredito_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Lista_Export")]
        public ErrorDto<CrRemesasCreditoLista> CR_RemesasCredito_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_RemesasCredito_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_RemesasCredito_Crear")]
        public ErrorDto<CrRemesasCreditoCrearResult> CR_RemesasCredito_Crear(int CodEmpresa, [FromBody] CrRemesasCreditoCrearRequest request)
        {
            return BL.CR_RemesasCredito_Crear(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Tags_Lista_Obtener")]
        public ErrorDto<CrRemesasCreditoTagLista> CR_RemesasCredito_Tags_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_RemesasCredito_Tags_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Tags_Lista_Export")]
        public ErrorDto<CrRemesasCreditoTagLista> CR_RemesasCredito_Tags_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_RemesasCredito_Tags_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_RemesasCredito_Tags_Guardar")]
        public ErrorDto CR_RemesasCredito_Tags_Guardar(int CodEmpresa, [FromBody] CrRemesasCreditoTagGuardarRequest request)
        {
            return BL.CR_RemesasCredito_Tags_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Informes_Lista_Obtener")]
        public ErrorDto<CrRemesasCreditoInformeLista> CR_RemesasCredito_Informes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_RemesasCredito_Informes_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Informes_Lista_Export")]
        public ErrorDto<CrRemesasCreditoInformeLista> CR_RemesasCredito_Informes_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_RemesasCredito_Informes_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_ArchivoDigital_Consultar")]
        public ErrorDto<CrRemesasCreditoArchivoDigitalDto> CR_RemesasCredito_ArchivoDigital_Consultar(int CodEmpresa, int remesa)
        {
            return BL.CR_RemesasCredito_ArchivoDigital_Consultar(CodEmpresa, remesa);
        }

        [Authorize]
        [HttpPost("CR_RemesasCredito_ArchivoDigital_Recibir")]
        public ErrorDto CR_RemesasCredito_ArchivoDigital_Recibir(int CodEmpresa, [FromBody] CrRemesasCreditoArchivoDigitalRequest request)
        {
            return BL.CR_RemesasCredito_ArchivoDigital_Recibir(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_RemesasCredito_Consulta_Operacion_Obtener")]
        public ErrorDto<CrRemesasCreditoConsultaDto> CR_RemesasCredito_Consulta_Operacion_Obtener(int CodEmpresa, long operacion)
        {
            return BL.CR_RemesasCredito_Consulta_Operacion_Obtener(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("CR_RemesasCredito_Listados_Cargar")]
        public ErrorDto<CrRemesasCreditoListadoCargaResult> CR_RemesasCredito_Listados_Cargar(int CodEmpresa, [FromBody] CrRemesasCreditoListadoCargaRequest request)
        {
            return BL.CR_RemesasCredito_Listados_Cargar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_RemesasCredito_Listados_Export")]
        public ErrorDto<CrRemesasCreditoListadoCargaResult> CR_RemesasCredito_Listados_Export(int CodEmpresa, [FromBody] CrRemesasCreditoListadoCargaRequest request)
        {
            return BL.CR_RemesasCredito_Listados_Export(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_RemesasCredito_Reporte_Datos_Obtener")]
        public ErrorDto<CrRemesasCreditoReporteDto> CR_RemesasCredito_Reporte_Datos_Obtener(int CodEmpresa, [FromBody] CrRemesasCreditoReporteRequest request)
        {
            return BL.CR_RemesasCredito_Reporte_Datos_Obtener(CodEmpresa, request);
        }
    }
}