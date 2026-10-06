using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_TablaDevoluciones")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslTablaDevolucionesController
        : ControllerBase
    {
        private readonly FrmFslTablaDevolucionesBl _bl;

        public FrmFslTablaDevolucionesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslTablaDevolucionesBl(config);
        }

        [HttpGet(
            "FSL_TablaDevoluciones_Garantias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_TablaDevoluciones_Garantias_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_TablaDevoluciones_Garantias_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_TablaDevoluciones_Lista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslTablaDevolucionDto>>
            FSL_TablaDevoluciones_Lista_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl
                .FSL_TablaDevoluciones_Lista_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpPost(
            "FSL_TablaDevoluciones_Devolucion_Registrar")]
        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Registrar(
                int CodEmpresa,
                [FromBody]
                FslTablaDevolucionGuardarRequest request)
        {
            return _bl
                .FSL_TablaDevoluciones_Devolucion_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_TablaDevoluciones_Devolucion_Actualizar")]
        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslTablaDevolucionGuardarRequest request)
        {
            return _bl
                .FSL_TablaDevoluciones_Devolucion_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_TablaDevoluciones_Devolucion_Eliminar")]
        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Eliminar(
                int CodEmpresa,
                int codDevolucion,
                string usuario)
        {
            return _bl
                .FSL_TablaDevoluciones_Devolucion_Eliminar(
                    CodEmpresa,
                    codDevolucion,
                    usuario);
        }
    }
}