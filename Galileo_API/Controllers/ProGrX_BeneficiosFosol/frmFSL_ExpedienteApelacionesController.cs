using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_ExpedienteApelaciones")]
    [ApiController]
    [Authorize]
    public sealed class FrmFslExpedienteApelacionesController :
        ControllerBase
    {
        private readonly FrmFslExpedienteApelacionesBl _bl;

        public FrmFslExpedienteApelacionesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl =
                new FrmFslExpedienteApelacionesBl(
                    config);
        }

        [HttpGet(
            "FSL_ExpedienteApelaciones_Expediente_Obtener")]
        public ErrorDto<FslExpedienteDatos>
            FSL_ExpedienteApelaciones_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Expediente_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        [HttpGet(
            "FSL_ExpedienteApelaciones_Catalogo_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_ExpedienteApelaciones_Catalogo_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Catalogo_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_ExpedienteApelaciones_Historico_Obtener")]
        public ErrorDto<
            List<FslExpedienteApelacionData>>
            FSL_ExpedienteApelaciones_Historico_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Historico_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        [HttpGet(
            "FSL_ExpedienteApelaciones_Miembros_Obtener")]
        public ErrorDto<
            List<FslExpedienteResolucionMiembroData>>
            FSL_ExpedienteApelaciones_Miembros_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Miembros_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        [HttpGet(
            "FSL_ExpedienteApelaciones_UsuarioVinculado_Obtener")]
        public ErrorDto<string>
            FSL_ExpedienteApelaciones_UsuarioVinculado_Obtener(
                int CodEmpresa,
                string cedula,
                string codComite)
        {
            return _bl
                .FSL_ExpedienteApelaciones_UsuarioVinculado_Obtener(
                    CodEmpresa,
                    cedula,
                    codComite);
        }

        [HttpPost(
            "FSL_ExpedienteApelaciones_Miembro_Validar")]
        public ErrorDto
            FSL_ExpedienteApelaciones_Miembro_Validar(
                int CodEmpresa,
                [FromBody]
                FslExpedienteMiembroValidarRequest? request)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Miembro_Validar(
                    CodEmpresa,
                    request);
        }

        [HttpPost(
            "FSL_ExpedienteApelaciones_Apelacion_Agregar")]
        public ErrorDto
            FSL_ExpedienteApelaciones_Apelacion_Agregar(
                int CodEmpresa,
                [FromBody]
                FslExpedienteApelacionAgregarRequest? request)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Apelacion_Agregar(
                    CodEmpresa,
                    request);
        }

        [HttpPut(
            "FSL_ExpedienteApelaciones_Resolucion_Guardar")]
        public ErrorDto
            FSL_ExpedienteApelaciones_Resolucion_Guardar(
                int CodEmpresa,
                [FromBody]
                FslExpedienteApelacionResolucionGuardarRequest?
                    request)
        {
            return _bl
                .FSL_ExpedienteApelaciones_Resolucion_Guardar(
                    CodEmpresa,
                    request);
        }
    }
}