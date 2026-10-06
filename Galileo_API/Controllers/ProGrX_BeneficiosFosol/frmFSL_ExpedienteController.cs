using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Authorize]
    [ApiController]
    [Route("api/frmFSL_Expediente")]
    public sealed class FrmFslExpedienteController : ControllerBase
    {
        private readonly FrmFslExpedienteBL _bl;

        public FrmFslExpedienteController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmFslExpedienteBL(config);
        }

        [HttpGet("FSL_Expediente_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Planes_Obtener(int CodEmpresa)
            => _bl.FSL_Expediente_Planes_Obtener(CodEmpresa);

        [HttpGet("FSL_Expediente_Comites_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Comites_Obtener(int CodEmpresa)
            => _bl.FSL_Expediente_Comites_Obtener(CodEmpresa);

        [HttpGet("FSL_Expediente_Enfermedades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Enfermedades_Obtener(int CodEmpresa)
            => _bl.FSL_Expediente_Enfermedades_Obtener(CodEmpresa);

        [HttpGet("FSL_Expediente_Causas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Causas_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_plan")] string codPlan)
            => _bl.FSL_Expediente_Causas_Obtener(
                CodEmpresa,
                codPlan);

        [HttpGet("FSL_Expediente_Obtener")]
        public ErrorDto<FslExpedienteDatos>
            FSL_Expediente_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_Navegacion_Obtener")]
        public ErrorDto<long>
            FSL_Expediente_Navegacion_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente,
                bool siguiente)
            => _bl.FSL_Expediente_Navegacion_Obtener(
                CodEmpresa,
                codExpediente,
                siguiente);

        [HttpGet("FSL_Expediente_Requisitos_Obtener")]
        public ErrorDto<List<FslExpedienteRequisitoData>>
            FSL_Expediente_Requisitos_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_Requisitos_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_Operaciones_Obtener")]
        public ErrorDto<List<FslExpedienteOperacionData>>
            FSL_Expediente_Operaciones_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_Operaciones_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_ResolucionMiembros_Obtener")]
        public ErrorDto<List<FslExpedienteResolucionMiembroData>>
            FSL_Expediente_ResolucionMiembros_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_ResolucionMiembros_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_ResolucionValidaciones_Obtener")]
        public ErrorDto<FslExpedienteResolucionValidacionesData?>
            FSL_Expediente_ResolucionValidaciones_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_ResolucionValidaciones_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_Gestiones_Obtener")]
        public ErrorDto<List<FslExpedienteGestionData>>
            FSL_Expediente_Gestiones_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_Gestiones_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_Apelaciones_Obtener")]
        public ErrorDto<List<FslExpedienteApelacionData>>
            FSL_Expediente_Apelaciones_Obtener(
                int CodEmpresa,
                [FromQuery(Name = "cod_expediente")] long codExpediente)
            => _bl.FSL_Expediente_Apelaciones_Obtener(
                CodEmpresa,
                codExpediente);

        [HttpGet("FSL_Expediente_UsuarioVinculado_Obtener")]
        public ErrorDto<string?>
            FSL_Expediente_UsuarioVinculado_Obtener(
                int CodEmpresa,
                string cedula,
                [FromQuery(Name = "cod_comite")] string codComite)
            => _bl.FSL_Expediente_UsuarioVinculado_Obtener(
                CodEmpresa,
                cedula,
                codComite);

        [HttpGet("FSL_Expediente_Registro_Validar")]
        public ErrorDto
            FSL_Expediente_Registro_Validar(
                int CodEmpresa,
                string cedula,
                [FromQuery(Name = "cod_plan")] string codPlan,
                [FromQuery(Name = "cod_causa")] string codCausa)
            => _bl.FSL_Expediente_Registro_Validar(
                CodEmpresa,
                cedula,
                codPlan,
                codCausa);

        [HttpPost("FSL_Expediente_Insertar")]
        public ErrorDto<FslExpedienteGuardarResultado>
            FSL_Expediente_Insertar(
                int CodEmpresa,
                [FromBody] FslExpedienteGuardarRequest request)
            => _bl.FSL_Expediente_Insertar(
                CodEmpresa,
                request);

        [HttpPost("FSL_Expediente_Resolucion_Guardar")]
        public ErrorDto
            FSL_Expediente_Resolucion_Guardar(
                int CodEmpresa,
                [FromBody] FslExpedienteResolucionGuardarRequest request)
            => _bl.FSL_Expediente_Resolucion_Guardar(
                CodEmpresa,
                request);

        [HttpPost("FSL_Expediente_Miembro_Validar")]
        public ErrorDto
            FSL_Expediente_Miembro_Validar(
                int CodEmpresa,
                [FromBody] FslExpedienteMiembroValidarRequest request)
            => _bl.FSL_Expediente_Miembro_Validar(
                CodEmpresa,
                request);

        [HttpPost("FSL_Expediente_Aplicar")]
        public ErrorDto<FslExpedienteAplicarResultado>
            FSL_Expediente_Aplicar(
                int CodEmpresa,
                [FromBody] FslExpedienteAplicarRequest request)
            => _bl.FSL_Expediente_Aplicar(
                CodEmpresa,
                request);

        [HttpPut("FSL_Expediente_Actualizar")]
        public ErrorDto
            FSL_Expediente_Actualizar(
                int CodEmpresa,
                [FromBody] FslExpedienteGuardarRequest request)
            => _bl.FSL_Expediente_Actualizar(
                CodEmpresa,
                request);

        [HttpPut("FSL_Expediente_Requisito_Actualizar")]
        public ErrorDto
            FSL_Expediente_Requisito_Actualizar(
                int CodEmpresa,
                [FromBody] FslExpedienteRequisitoActualizarRequest request)
            => _bl.FSL_Expediente_Requisito_Actualizar(
                CodEmpresa,
                request);
    }
}
