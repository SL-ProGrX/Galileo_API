using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_ExpedienteGestiones")]
    [ApiController]
    [Authorize]
    public class FrmFslExpedienteGestionesController :
        ControllerBase
    {
        private readonly FrmFslExpedienteGestionesBL _bl;

        public FrmFslExpedienteGestionesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl =
                new FrmFslExpedienteGestionesBL(
                    config);
        }

        [HttpGet(
            "FSL_ExpedienteGestiones_Expediente_Obtener")]
        public ErrorDto<FslExpedienteDatos>
            FSL_ExpedienteGestiones_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _bl
                .FSL_ExpedienteGestiones_Expediente_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        [HttpGet(
            "FSL_ExpedienteGestiones_Catalogo_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_ExpedienteGestiones_Catalogo_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_ExpedienteGestiones_Catalogo_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_ExpedienteGestiones_Historico_Obtener")]
        public ErrorDto<
            List<FslExpedienteGestionData>>
            FSL_ExpedienteGestiones_Historico_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _bl
                .FSL_ExpedienteGestiones_Historico_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        [HttpPost(
            "FSL_ExpedienteGestiones_Gestion_Agregar")]
        public ErrorDto
            FSL_ExpedienteGestiones_Gestion_Agregar(
                int CodEmpresa,
                [FromBody]
                FslExpedienteGestionAgregarRequest?
                    request)
        {
            return _bl
                .FSL_ExpedienteGestiones_Gestion_Agregar(
                    CodEmpresa,
                    request);
        }
    }
}
