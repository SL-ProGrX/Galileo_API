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
    public class FrmCrSeguimientoDesembolsosController : ControllerBase
    {
        private readonly FrmCrSeguimientoDesembolsosBL BL;

        public FrmCrSeguimientoDesembolsosController(IConfiguration config)
        {
            BL = new FrmCrSeguimientoDesembolsosBL(config);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Inicializar")]
        public ErrorDto<CrSeguimientoDesembolsosInicializarDto> CR_SeguimientoDesembolsos_Inicializar(
            int CodEmpresa,
            long operacion,
            string usuario)
        {
            return BL.CR_SeguimientoDesembolsos_Inicializar(CodEmpresa, operacion, usuario);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Lista_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_SeguimientoDesembolsos_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            return BL.CR_SeguimientoDesembolsos_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Lista_Export")]
        public ErrorDto<TablasListaGenericaModel> CR_SeguimientoDesembolsos_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return BL.CR_SeguimientoDesembolsos_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Detalle_Obtener")]
        public ErrorDto<CrSeguimientoDesembolsosData> CR_SeguimientoDesembolsos_Detalle_Obtener(
            int CodEmpresa,
            long idDesembolso)
        {
            return BL.CR_SeguimientoDesembolsos_Detalle_Obtener(CodEmpresa, idDesembolso);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Conceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoDesembolsos_Conceptos_Obtener(
            int CodEmpresa,
            string? texto)
        {
            return BL.CR_SeguimientoDesembolsos_Conceptos_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Concepto_Info_Obtener")]
        public ErrorDto<CrSeguimientoDesembolsosConceptoDto> CR_SeguimientoDesembolsos_Concepto_Info_Obtener(
            int CodEmpresa,
            int codConcepto)
        {
            return BL.CR_SeguimientoDesembolsos_Concepto_Info_Obtener(CodEmpresa, codConcepto);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_Bancos_Obtener")]
        public ErrorDto<List<CrSeguimientoDesembolsosBancoDto>> CR_SeguimientoDesembolsos_Bancos_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return BL.CR_SeguimientoDesembolsos_Bancos_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoDesembolsos_CuentasBancarias_Obtener")]
        public ErrorDto<List<CrSeguimientoDesembolsosCuentaBancariaDto>> CR_SeguimientoDesembolsos_CuentasBancarias_Obtener(
            int CodEmpresa,
            string identificacion,
            int bancoId,
            int divisaCheck)
        {
            return BL.CR_SeguimientoDesembolsos_CuentasBancarias_Obtener(
                CodEmpresa,
                identificacion,
                bancoId,
                divisaCheck);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoDesembolsos_Guardar")]
        public ErrorDto<CrSeguimientoDesembolsosResumenDto> CR_SeguimientoDesembolsos_Guardar(
            int CodEmpresa,
            [FromBody] CrSeguimientoDesembolsosGuardarRequest request)
        {
            return BL.CR_SeguimientoDesembolsos_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoDesembolsos_Eliminar")]
        public ErrorDto<CrSeguimientoDesembolsosResumenDto> CR_SeguimientoDesembolsos_Eliminar(
            int CodEmpresa,
            [FromBody] CrSeguimientoDesembolsosEliminarRequest request)
        {
            return BL.CR_SeguimientoDesembolsos_Eliminar(CodEmpresa, request);
        }
    }
}