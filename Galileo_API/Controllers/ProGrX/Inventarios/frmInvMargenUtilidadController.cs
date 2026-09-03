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
    public sealed class FrmInvMargenUtilidadController
        : ControllerBase
    {
        private readonly FrmInvMargenUtilidadBl _bl;

        public FrmInvMargenUtilidadController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmInvMargenUtilidadBl(config);
        }

        [HttpGet(
            "INV_MargenUtilidad_Lineas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_MargenUtilidad_Lineas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_MargenUtilidad_Lineas_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_MargenUtilidad_Sublineas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_MargenUtilidad_Sublineas_Obtener(
                int CodEmpresa,
                int codLinea)
        {
            return _bl
                .INV_MargenUtilidad_Sublineas_Obtener(
                    CodEmpresa,
                    codLinea);
        }

        [HttpGet(
            "INV_MargenUtilidad_Precios_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_MargenUtilidad_Precios_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_MargenUtilidad_Precios_Obtener(
                    CodEmpresa);
        }

        [HttpPost(
            "INV_MargenUtilidad_Cambios_Aplicar")]
        public ErrorDto
            INV_MargenUtilidad_Cambios_Aplicar(
                int CodEmpresa,
                InvMargenUtilidadAplicarRequest request)
        {
            return _bl
                .INV_MargenUtilidad_Cambios_Aplicar(
                    CodEmpresa,
                    request);
        }
    }
}