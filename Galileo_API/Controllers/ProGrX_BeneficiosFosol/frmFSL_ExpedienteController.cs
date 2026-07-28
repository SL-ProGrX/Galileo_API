using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Endpoints de los Expedientes Fosol (frmFSL_Expediente).
    /// </summary>
    [Route("api/frmFSL_Expediente")]
    [ApiController]
    public class FrmFslExpedienteController : ControllerBase
    {
        private readonly FrmFslExpedienteBL _bl;

        public FrmFslExpedienteController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmFslExpedienteBL(config);
        }

        /// <summary>Lista de planes activos.</summary>
        [Authorize]
        [HttpGet("FslPlanLista_Obtener")]
        public ErrorDto<List<FslMenusData>> FslPlanLista_Obtener(int CodCliente)
            => _bl.FslPlanLista_Obtener(CodCliente);

        /// <summary>Lista de comités activos.</summary>
        [Authorize]
        [HttpGet("FslComiteLista_Obtener")]
        public ErrorDto<List<FslMenusData>> FslComiteLista_Obtener(int CodCliente)
            => _bl.FslComiteLista_Obtener(CodCliente);

        /// <summary>Lista de enfermedades activas.</summary>
        [Authorize]
        [HttpGet("FslEnfermedadesLista_Obtener")]
        public ErrorDto<List<FslMenusData>> FslEnfermedadesLista_Obtener(int CodCliente)
            => _bl.FslEnfermedadesLista_Obtener(CodCliente);

        /// <summary>Lista de causas de un plan.</summary>
        [Authorize]
        [HttpGet("FslCausasLista_Obtener")]
        public ErrorDto<List<FslMenusData>> FslCausasLista_Obtener(int CodCliente, string cod_plan)
            => _bl.FslCausasLista_Obtener(CodCliente, cod_plan);

        /// <summary>Detalle de un expediente.</summary>
        [Authorize]
        [HttpGet("FslExpediente_Obtener")]
        public ErrorDto<FslExpedienteDatos> FslExpediente_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslExpediente_Obtener(CodCliente, cod_expediente);

        /// <summary>Requisitos de un expediente.</summary>
        [Authorize]
        [HttpGet("FslRequisitos_Obtener")]
        public ErrorDto<List<FslRequisitosExp>> FslRequisitos_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslRequisitos_Obtener(CodCliente, cod_expediente);

        /// <summary>Operaciones (créditos) de un expediente.</summary>
        [Authorize]
        [HttpGet("FslOperaciones_Obtener")]
        public ErrorDto<List<FslOperacionesDatos>> FslOperaciones_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslOperaciones_Obtener(CodCliente, cod_expediente);

        /// <summary>Resolución (miembros) de un expediente.</summary>
        [Authorize]
        [HttpGet("FslResolucion_Obtener")]
        public ErrorDto<List<FslResolucionDatos>> FslResolucion_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslResolucion_Obtener(CodCliente, cod_expediente);

        /// <summary>Validaciones de la resolución de un expediente.</summary>
        [Authorize]
        [HttpGet("FslResolucionlVal_Obtener")]
        public ErrorDto<List<FslResolucionValidacionesDatos>> FslResolucionlVal_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslResolucionlVal_Obtener(CodCliente, cod_expediente);

        /// <summary>Gestiones de un expediente.</summary>
        [Authorize]
        [HttpGet("FslExpGestiones_Obtener")]
        public ErrorDto<List<FslExpGestiones>> FslExpGestiones_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslExpGestiones_Obtener(CodCliente, cod_expediente);

        /// <summary>Apelaciones de un expediente.</summary>
        [Authorize]
        [HttpGet("FslApelaciones_Obtener")]
        public ErrorDto<List<FslApelacionDatos>> FslApelaciones_Obtener(int CodCliente, int cod_expediente)
            => _bl.FslApelaciones_Obtener(CodCliente, cod_expediente);

        /// <summary>Lista de expedientes.</summary>
        [Authorize]
        [HttpGet("FslExpedientesLista_Obtener")]
        public ErrorDto<FslExpedienteListaData> FslExpedientesLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _bl.FslExpedientesLista_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Valida si un caso ya fue presentado.</summary>
        [Authorize]
        [HttpGet("FslExpediente_Valida")]
        public ErrorDto FslExpediente_Valida(int CodCliente, string cedula, string tipo, string causa)
            => _bl.FslExpediente_Valida(CodCliente, cedula, tipo, causa);

        /// <summary>Obtiene el usuario vinculado de un miembro de comité.</summary>
        [Authorize]
        [HttpGet("FslUsuarioVinculado_Obtener")]
        public ErrorDto FslUsuarioVinculado_Obtener(int CodCliente, string cedula, string cod_comite)
            => _bl.FslUsuarioVinculado_Obtener(CodCliente, cedula, cod_comite);

        /// <summary>Inserta un expediente.</summary>
        [Authorize]
        [HttpPost("FslExpediente_Insertar")]
        public ErrorDto FslExpediente_Insertar(int CodCliente, [FromBody] string jsonExp)
            => _bl.FslExpediente_Insertar(CodCliente, jsonExp);

        /// <summary>Valida las credenciales de un miembro del comité.</summary>
        [Authorize]
        [HttpPost("FslMiembroValida")]
        public ErrorDto FslMiembroValida(int CodCliente, [FromBody] FslMiembroValida usuario)
            => _bl.FslMiembroValida(CodCliente, usuario);

        /// <summary>Guarda la resolución de un expediente.</summary>
        [Authorize]
        [HttpPost("FslResolucion_Guardar")]
        public ErrorDto FslResolucion_Guardar(int CodCliente, [FromBody] FslResolucionGuardar resolucion)
            => _bl.FslResolucion_Guardar(CodCliente, resolucion);

        /// <summary>Aplica (procesa) un expediente Fosol.</summary>
        [Authorize]
        [HttpPost("FslExpediente_Aplicar")]
        public ErrorDto FslExpediente_Aplicar(int CodCliente, long cod_expediente, string usuario)
            => _bl.FslExpediente_Aplicar(CodCliente, cod_expediente, usuario);

        /// <summary>Actualiza un expediente.</summary>
        [Authorize]
        [HttpPut("FslExpediente_Actualizar")]
        public ErrorDto FslExpediente_Actualizar(int CodCliente, [FromBody] string jsonExp)
            => _bl.FslExpediente_Actualizar(CodCliente, jsonExp);

        /// <summary>Actualiza el estado de un requisito del expediente.</summary>
        [Authorize]
        [HttpPut("FslExpRequisto_Actualizar")]
        public ErrorDto FslExpRequisto_Actualizar(int CodCliente, [FromBody] FslExpedienteUpdate requisito)
            => _bl.FslExpRequisto_Actualizar(CodCliente, requisito);
    }
}
