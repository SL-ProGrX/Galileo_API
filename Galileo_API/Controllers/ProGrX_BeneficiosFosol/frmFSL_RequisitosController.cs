using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_Requisitos")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslRequisitosController
        : ControllerBase
    {
        private readonly FrmFslRequisitosBL _bl;

        public FrmFslRequisitosController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslRequisitosBL(config);
        }

        [HttpGet(
            "FSL_Requisitos_Lista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslRequisitoDto>>
            FSL_Requisitos_Lista_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl.FSL_Requisitos_Lista_Obtener(
                CodEmpresa,
                filtros);
        }

        [HttpGet(
            "FSL_Requisitos_Planes_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Requisitos_Planes_Obtener(
                int CodEmpresa)
        {
            return _bl.FSL_Requisitos_Planes_Obtener(
                CodEmpresa);
        }

        [HttpGet(
            "FSL_Requisitos_Causas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Requisitos_Causas_Obtener(
                int CodEmpresa,
                string codPlan)
        {
            return _bl.FSL_Requisitos_Causas_Obtener(
                CodEmpresa,
                codPlan);
        }

        [HttpGet(
            "FSL_Requisitos_Asignaciones_Obtener")]
        public ErrorDto<
            List<FslRequisitoCausaDto>>
            FSL_Requisitos_Asignaciones_Obtener(
                int CodEmpresa,
                string codPlan,
                string codCausa)
        {
            return _bl
                .FSL_Requisitos_Asignaciones_Obtener(
                    CodEmpresa,
                    codPlan,
                    codCausa);
        }

        [HttpPost(
            "FSL_Requisitos_Requisito_Registrar")]
        public ErrorDto
            FSL_Requisitos_Requisito_Registrar(
                int CodEmpresa,
                [FromBody]
                FslRequisitoGuardarRequest request)
        {
            return _bl
                .FSL_Requisitos_Requisito_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_Requisitos_Requisito_Actualizar")]
        public ErrorDto
            FSL_Requisitos_Requisito_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslRequisitoGuardarRequest request)
        {
            return _bl
                .FSL_Requisitos_Requisito_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_Requisitos_Requisito_Eliminar")]
        public ErrorDto
            FSL_Requisitos_Requisito_Eliminar(
                int CodEmpresa,
                string codRequisito,
                string usuario)
        {
            return _bl
                .FSL_Requisitos_Requisito_Eliminar(
                    CodEmpresa,
                    codRequisito,
                    usuario);
        }

        [HttpPut(
            "FSL_Requisitos_Asignacion_Actualizar")]
        public ErrorDto
            FSL_Requisitos_Asignacion_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslRequisitoAsignacionRequest request)
        {
            return _bl
                .FSL_Requisitos_Asignacion_Actualizar(
                    CodEmpresa,
                    request);
        }
    }
}