using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Lógica de negocio de los Expedientes Fosol (frmFSL_Expediente).
    /// </summary>
    public class FrmFslExpedienteBL
    {
        private readonly FrmFslExpedienteDB _db;

        public FrmFslExpedienteBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmFslExpedienteDB(config);
        }

        /// <summary>Lista de planes activos.</summary>
        public ErrorDto<List<FslMenusData>> FslPlanLista_Obtener(int CodCliente)
            => _db.FslPlanLista_Obtener(CodCliente);

        /// <summary>Lista de comités activos.</summary>
        public ErrorDto<List<FslMenusData>> FslComiteLista_Obtener(int CodCliente)
            => _db.FslComiteLista_Obtener(CodCliente);

        /// <summary>Lista de enfermedades activas.</summary>
        public ErrorDto<List<FslMenusData>> FslEnfermedadesLista_Obtener(int CodCliente)
            => _db.FslEnfermedadesLista_Obtener(CodCliente);

        /// <summary>Lista de causas de un plan.</summary>
        public ErrorDto<List<FslMenusData>> FslCausasLista_Obtener(int CodCliente, string cod_plan)
            => _db.FslCausasLista_Obtener(CodCliente, cod_plan);

        /// <summary>Detalle de un expediente.</summary>
        public ErrorDto<FslExpedienteDatos> FslExpediente_Obtener(int CodCliente, int cod_expediente)
            => _db.FslExpediente_Obtener(CodCliente, cod_expediente);

        /// <summary>Requisitos de un expediente.</summary>
        public ErrorDto<List<FslRequisitosExp>> FslRequisitos_Obtener(int CodCliente, int cod_expediente)
            => _db.FslRequisitos_Obtener(CodCliente, cod_expediente);

        /// <summary>Operaciones (créditos) de un expediente.</summary>
        public ErrorDto<List<FslOperacionesDatos>> FslOperaciones_Obtener(int CodCliente, int cod_expediente)
            => _db.FslOperaciones_Obtener(CodCliente, cod_expediente);

        /// <summary>Resolución (miembros) de un expediente.</summary>
        public ErrorDto<List<FslResolucionDatos>> FslResolucion_Obtener(int CodCliente, int cod_expediente)
            => _db.FslResolucion_Obtener(CodCliente, cod_expediente);

        /// <summary>Validaciones de la resolución de un expediente.</summary>
        public ErrorDto<List<FslResolucionValidacionesDatos>> FslResolucionlVal_Obtener(int CodCliente, int cod_expediente)
            => _db.FslResolucionlVal_Obtener(CodCliente, cod_expediente);

        /// <summary>Gestiones de un expediente.</summary>
        public ErrorDto<List<FslExpGestiones>> FslExpGestiones_Obtener(int CodCliente, int cod_expediente)
            => _db.FslExpGestiones_Obtener(CodCliente, cod_expediente);

        /// <summary>Apelaciones de un expediente.</summary>
        public ErrorDto<List<FslApelacionDatos>> FslApelaciones_Obtener(int CodCliente, int cod_expediente)
            => _db.FslApelaciones_Obtener(CodCliente, cod_expediente);

        /// <summary>Lista de expedientes.</summary>
        public ErrorDto<FslExpedienteListaData> FslExpedientesLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _db.FslExpedientesLista_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Valida si un caso ya fue presentado.</summary>
        public ErrorDto FslExpediente_Valida(int CodCliente, string cedula, string tipo, string causa)
            => _db.FslExpediente_Valida(CodCliente, cedula, tipo, causa);

        /// <summary>Obtiene el usuario vinculado de un miembro de comité.</summary>
        public ErrorDto FslUsuarioVinculado_Obtener(int CodCliente, string cedula, string cod_comite)
            => _db.FslUsuarioVinculado_Obtener(CodCliente, cedula, cod_comite);

        /// <summary>Inserta un expediente.</summary>
        public ErrorDto FslExpediente_Insertar(int CodCliente, string jsonExp)
            => _db.FslExpediente_Insertar(CodCliente, jsonExp);

        /// <summary>Actualiza un expediente.</summary>
        public ErrorDto FslExpediente_Actualizar(int CodCliente, string jsonExp)
            => _db.FslExpediente_Actualizar(CodCliente, jsonExp);

        /// <summary>Actualiza el estado de un requisito del expediente.</summary>
        public ErrorDto FslExpRequisto_Actualizar(int CodCliente, FslExpedienteUpdate requisito)
            => _db.FslExpRequisto_Actualizar(CodCliente, requisito);

        /// <summary>Guarda la resolución de un expediente.</summary>
        public ErrorDto FslResolucion_Guardar(int CodCliente, FslResolucionGuardar resolucion)
            => _db.FslResolucion_Guardar(CodCliente, resolucion);

        /// <summary>Valida las credenciales de un miembro del comité.</summary>
        public ErrorDto FslMiembroValida(int CodCliente, FslMiembroValida usuario)
            => _db.FslMiembroValida(CodCliente, usuario);

        /// <summary>Aplica (procesa) un expediente Fosol.</summary>
        public ErrorDto FslExpediente_Aplicar(int CodCliente, long cod_expediente, string usuario)
            => _db.FslExpediente_Aplicar(CodCliente, cod_expediente, usuario);
    }
}
