using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslExpedienteDB
    {
        private const int CodigoValidacion = -2;
        private const string EstadoPendiente = "P";
        private const string EstadoAprobado = "A";
        private const string EstadoRechazado = "R";

        private const string MensajeExpedienteRequerido =
            "El c&oacute;digo del expediente es requerido.";

        private const string MensajeExpedienteNoExiste =
            "El expediente indicado no existe.";

        private const string MensajeExpedienteNoPendiente =
            "No se puede modificar este tr&aacute;mite porque no se encuentra pendiente.";

        private const string MensajeNotasRequeridas =
            "Las notas del expediente deben contener m&aacute;s de 10 caracteres.";

        private readonly PortalDB _portalDb;
        private readonly string _securityConnectionString;

        public FrmFslExpedienteDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityConnectionString =
                config.GetConnectionString("DefaultConnString") ??
                throw new InvalidOperationException(
                    "No se encontr&oacute; la conexi&oacute;n de seguridad.");
        }

        /// <summary>
        /// Obtiene los planes FOSOL activos.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <returns>Planes activos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Planes_Obtener(int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_PLAN) AS item,
                    RTRIM(COD_PLAN) + ' - ' +
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
                FROM FSL_PLANES
                WHERE ACTIVO = 1
                ORDER BY COD_PLAN;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los comit&#233;s FOSOL activos.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <returns>Comit&#233;s activos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Comites_Obtener(int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_COMITE) AS item,
                    RTRIM(COD_COMITE) + ' - ' +
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
                FROM FSL_COMITES
                WHERE ACTIVO = 1
                ORDER BY COD_COMITE;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los tipos de enfermedades activos.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <returns>Enfermedades activas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Enfermedades_Obtener(int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_ENFERMEDAD) AS item,
                    RTRIM(COD_ENFERMEDAD) + ' - ' +
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
                FROM FSL_TIPOS_ENFERMEDADES
                WHERE ACTIVA = 1
                ORDER BY COD_ENFERMEDAD;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene las causas relacionadas con un plan.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <param name="codPlan">C&#243;digo del plan.</param>
        /// <returns>Causas del plan.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Expediente_Causas_Obtener(
                int CodEmpresa,
                string codPlan)
        {
            if (string.IsNullOrWhiteSpace(codPlan))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo del plan es requerido.",
                    CodigoValidacion,
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = """
                SELECT
                    RTRIM(COD_CAUSA) AS item,
                    RTRIM(COD_CAUSA) + ' - ' +
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
                FROM FSL_PLANES_CAUSAS
                WHERE COD_PLAN = @cod_plan
                ORDER BY COD_CAUSA;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                sql,
                new { cod_plan = codPlan.Trim() });
        }

        private sealed class FslResolucionContexto
        {
            public string estado { get; set; } = string.Empty;
            public int numero_resolutores { get; set; } = 0;
            public bool cumple_requisitos { get; set; } = false;
            public bool cumple_tiempo { get; set; } = false;
            public bool cumple_registro { get; set; } = false;
        }
    }
}