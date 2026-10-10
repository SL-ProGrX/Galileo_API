using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_TipoAplicacion")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslTipoAplicacionController
        : ControllerBase
    {
        private readonly FrmFslTipoAplicacionBl _bl;

        public FrmFslTipoAplicacionController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslTipoAplicacionBl(
                config);
        }

        [HttpGet(
            "FSL_TipoAplicacion_Planes_Lista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<
                FslTipoAplicacionPlanDto>>
            FSL_TipoAplicacion_Planes_Lista_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl
                .FSL_TipoAplicacion_Planes_Lista_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpGet(
            "FSL_TipoAplicacion_Causas_Lista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<
                FslTipoAplicacionCausaDto>>
            FSL_TipoAplicacion_Causas_Lista_Obtener(
                int CodEmpresa,
                string codPlan,
                string filtros = "")
        {
            return _bl
                .FSL_TipoAplicacion_Causas_Lista_Obtener(
                    CodEmpresa,
                    codPlan,
                    filtros);
        }

        [HttpGet(
            "FSL_TipoAplicacion_Planes_Selector_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_TipoAplicacion_Planes_Selector_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_TipoAplicacion_Planes_Selector_Obtener(
                    CodEmpresa);
        }

        [HttpPost(
            "FSL_TipoAplicacion_Plan_Registrar")]
        public ErrorDto
            FSL_TipoAplicacion_Plan_Registrar(
                int CodEmpresa,
                [FromBody]
                FslTipoAplicacionPlanGuardarRequest request)
        {
            return _bl
                .FSL_TipoAplicacion_Plan_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_TipoAplicacion_Plan_Actualizar")]
        public ErrorDto
            FSL_TipoAplicacion_Plan_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslTipoAplicacionPlanGuardarRequest request)
        {
            return _bl
                .FSL_TipoAplicacion_Plan_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_TipoAplicacion_Plan_Eliminar")]
        public ErrorDto
            FSL_TipoAplicacion_Plan_Eliminar(
                int CodEmpresa,
                string codPlan,
                string usuario)
        {
            return _bl
                .FSL_TipoAplicacion_Plan_Eliminar(
                    CodEmpresa,
                    codPlan,
                    usuario);
        }

        [HttpPost(
            "FSL_TipoAplicacion_Causa_Registrar")]
        public ErrorDto
            FSL_TipoAplicacion_Causa_Registrar(
                int CodEmpresa,
                [FromBody]
                FslTipoAplicacionCausaGuardarRequest request)
        {
            return _bl
                .FSL_TipoAplicacion_Causa_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_TipoAplicacion_Causa_Actualizar")]
        public ErrorDto
            FSL_TipoAplicacion_Causa_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslTipoAplicacionCausaGuardarRequest request)
        {
            return _bl
                .FSL_TipoAplicacion_Causa_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_TipoAplicacion_Causa_Eliminar")]
        public ErrorDto
            FSL_TipoAplicacion_Causa_Eliminar(
                int CodEmpresa,
                string codPlan,
                string codCausa,
                string usuario)
        {
            return _bl
                .FSL_TipoAplicacion_Causa_Eliminar(
                    CodEmpresa,
                    codPlan,
                    codCausa,
                    usuario);
        }
    }
}