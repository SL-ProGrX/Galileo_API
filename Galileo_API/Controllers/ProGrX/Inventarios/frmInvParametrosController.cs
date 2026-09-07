using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvParametrosController
        : ControllerBase
    {
        private readonly FrmInvParametrosBl _bl;

        public FrmInvParametrosController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmInvParametrosBl(config);
        }

        [HttpGet(
            "INV_Parametros_Parametros_Obtener")]
        public ErrorDto<ParametrosGenDto?>
            INV_Parametros_Parametros_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_Parametros_Parametros_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_Parametros_Contabilidades_Obtener")]
        public ErrorDto<List<CntXContaDto>>
            INV_Parametros_Contabilidades_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_Parametros_Contabilidades_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_Parametros_Cuentas_Descripciones_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_Parametros_Cuentas_Descripciones_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            return _bl
                .INV_Parametros_Cuentas_Descripciones_Obtener(
                    CodEmpresa,
                    codContabilidad);
        }

        [HttpGet(
            "INV_Parametros_Asientos_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_Parametros_Asientos_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            return _bl
                .INV_Parametros_Asientos_Obtener(
                    CodEmpresa,
                    codContabilidad);
        }

        [HttpPost(
            "INV_Parametros_Actualizar")]
        public ErrorDto
            INV_Parametros_Actualizar(
                int CodEmpresa,
                ParametrosGenDto request)
        {
            return _bl
                .INV_Parametros_Actualizar(
                    CodEmpresa,
                    request);
        }
    }
}