using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ControlTramites
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmAfBenSeguimientoRevisionesTagController
        : ControllerBase
    {
        private readonly FrmAfBenSeguimientoRevisionesTagBl _bl;

        public FrmAfBenSeguimientoRevisionesTagController(
            IConfiguration config)
        {
            _bl = new FrmAfBenSeguimientoRevisionesTagBl(
                config);
        }

        [HttpGet(
            "AF_frmAF_BenSeguimientoRevisionesTag_Beneficios_Obtener")]
        public ErrorDto<List<AfBenSeguimientoBeneficioData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Beneficios_Obtener(
                int codEmpresa,
                string? cedula = null)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Beneficios_Obtener(
                    codEmpresa,
                    cedula);
        }

        [HttpGet(
            "AF_frmAF_BenSeguimientoRevisionesTag_Seguimiento_Obtener")]
        public ErrorDto<List<AfBenSeguimientoRegistroData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Seguimiento_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Seguimiento_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "AF_frmAF_BenSeguimientoRevisionesTag_Etiquetas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_frmAF_BenSeguimientoRevisionesTag_Etiquetas_Obtener(
                int codEmpresa,
                string usuario)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Etiquetas_Obtener(
                    codEmpresa,
                    usuario);
        }

        [HttpGet(
            "AF_frmAF_BenSeguimientoRevisionesTag_Aviso_Obtener")]
        public ErrorDto
            AF_frmAF_BenSeguimientoRevisionesTag_Aviso_Obtener(
                int codEmpresa,
                string tagCodigo)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Aviso_Obtener(
                    codEmpresa,
                    tagCodigo);
        }

        [HttpGet(
            "AF_frmAF_BenSeguimientoRevisionesTag_Omisiones_Obtener")]
        public ErrorDto<List<AfBenSeguimientoOmisionData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Omisiones_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Omisiones_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "AF_frmAF_BenSeguimientoRevisionesTag_Omision_Cambiar")]
        public ErrorDto<AfBenSeguimientoOmisionCambiarData>
            AF_frmAF_BenSeguimientoRevisionesTag_Omision_Cambiar(
                int codEmpresa,
                AfBenSeguimientoOmisionCambiarRequest request)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Omision_Cambiar(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "AF_frmAF_BenSeguimientoRevisionesTag_Aplicar")]
        public ErrorDto
            AF_frmAF_BenSeguimientoRevisionesTag_Aplicar(
                int codEmpresa,
                AfBenSeguimientoAplicarRequest request)
        {
            return _bl
                .AF_frmAF_BenSeguimientoRevisionesTag_Aplicar(
                    codEmpresa,
                    request);
        }
    }
}