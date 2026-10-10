using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_TablasTipos")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslTablasTiposController
        : ControllerBase
    {
        private readonly FrmFslTablasTiposBl _bl;

        public FrmFslTablasTiposController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslTablasTiposBl(config);
        }

        [HttpGet(
            "FSL_TablasTipos_Lista_Obtener")]
        public ErrorDto<
            FslListaPaginadaDto<FslTablaTipoDto>>
            FSL_TablasTipos_Lista_Obtener(
                int CodEmpresa,
                string filtros = "")
        {
            return _bl
                .FSL_TablasTipos_Lista_Obtener(
                    CodEmpresa,
                    filtros);
        }

        [HttpPost(
            "FSL_TablasTipos_Tipo_Registrar")]
        public ErrorDto
            FSL_TablasTipos_Tipo_Registrar(
                int CodEmpresa,
                [FromBody]
                FslTablaTipoGuardarRequest request)
        {
            return _bl
                .FSL_TablasTipos_Tipo_Registrar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_TablasTipos_Tipo_Actualizar")]
        public ErrorDto
            FSL_TablasTipos_Tipo_Actualizar(
                int CodEmpresa,
                [FromBody]
                FslTablaTipoGuardarRequest request)
        {
            return _bl
                .FSL_TablasTipos_Tipo_Actualizar(
                    CodEmpresa,
                    request);
        }

        [HttpDelete(
            "FSL_TablasTipos_Tipo_Eliminar")]
        public ErrorDto
            FSL_TablasTipos_Tipo_Eliminar(
                int CodEmpresa,
                string tipo,
                string codigo,
                string usuario)
        {
            return _bl
                .FSL_TablasTipos_Tipo_Eliminar(
                    CodEmpresa,
                    tipo,
                    codigo,
                    usuario);
        }
    }
}