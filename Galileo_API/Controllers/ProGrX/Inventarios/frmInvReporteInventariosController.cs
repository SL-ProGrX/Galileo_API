using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FrmInvReporteInventariosController
        : ControllerBase
    {
        private readonly FrmInvReporteInventariosBL _bl;

        public FrmInvReporteInventariosController(
            IConfiguration config)
        {
            _bl = new FrmInvReporteInventariosBL(
                config);
        }

        [HttpGet(
            "INV_ReporteInventarios_Bodegas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_ReporteInventarios_Bodegas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_ReporteInventarios_Bodegas_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_ReporteInventarios_Lineas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_ReporteInventarios_Lineas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_ReporteInventarios_Lineas_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "INV_ReporteInventarios_Sublineas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_ReporteInventarios_Sublineas_Obtener(
                int CodEmpresa,
                int CodLinea)
        {
            return _bl
                .INV_ReporteInventarios_Sublineas_Obtener(
                    CodEmpresa,
                    CodLinea);
        }
    }
}