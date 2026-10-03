using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de las gestiones de expedientes FOSOL.
    /// </summary>
    public sealed class FrmFslExpedienteGestionesDB
    {
        private const int CodigoValidacion = -2;

        private const string MensajeExpedienteRequerido =
            "El c&oacute;digo del expediente es requerido.";

        private const string MensajeGestionRequerida =
            "El tipo de gesti&oacute;n es requerido.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private readonly PortalDB _portalDb;
        private readonly FrmFslExpedienteDB _expedienteDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario.
        /// </summary>
        /// <param name="config">
        /// Configuraci&oacute;n de la aplicaci&oacute;n.
        /// </param>
        public FrmFslExpedienteGestionesDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _expedienteDb =
                new FrmFslExpedienteDB(config);
        }

        /// <summary>
        /// Obtiene el encabezado del expediente seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codExpediente">
        /// C&oacute;digo del expediente.
        /// </param>
        /// <returns>
        /// Informaci&oacute;n general del expediente.
        /// </returns>
        public ErrorDto<FslExpedienteDatos>
            FSL_ExpedienteGestiones_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _expedienteDb.FSL_Expediente_Obtener(
                CodEmpresa,
                codExpediente);
        }

        /// <summary>
        /// Obtiene los tipos de gesti&oacute;n activos.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <returns>
        /// Tipos de gesti&oacute;n disponibles.
        /// </returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_ExpedienteGestiones_Catalogo_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_GESTION) AS item,
                    RTRIM(COD_GESTION) + ' - ' +
                    RTRIM(ISNULL(DESCRIPCION, ''))
                        AS descripcion
                FROM FSL_TIPOS_GESTIONES
                WHERE ACTIVA = 1;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene el hist&oacute;rico de gestiones del expediente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codExpediente">
        /// C&oacute;digo del expediente.
        /// </param>
        /// <returns>
        /// Gestiones registradas en el expediente.
        /// </returns>
        public ErrorDto<List<FslExpedienteGestionData>>
            FSL_ExpedienteGestiones_Historico_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            if (codExpediente <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion,
                    new List<
                        FslExpedienteGestionData>());
            }

            return _expedienteDb
                .FSL_Expediente_Gestiones_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        /// <summary>
        /// Registra una gesti&oacute;n en el expediente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n de la gesti&oacute;n.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto
            FSL_ExpedienteGestiones_Gestion_Agregar(
                int CodEmpresa,
                FslExpedienteGestionAgregarRequest?
                    request)
        {
            var validacion =
                FSL_ExpedienteGestiones_Gestion_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            var solicitud = request!;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        CodEmpresa);

                connection.Open();

                connection.Execute(
                    "[spFSL_GestionRegistra]",
                    new
                    {
                        Expediente =
                            solicitud.cod_expediente,
                        Tipo =
                            solicitud.cod_gestion.Trim(),
                        Notas =
                            solicitud.notas?.Trim() ??
                            string.Empty,
                        Usuario =
                            solicitud.usuario
                                .Trim()
                                .ToUpperInvariant()
                    },
                    commandType:
                        CommandType.StoredProcedure);

                return DbHelper.OkResponse(
                    "Gesti&oacute;n registrada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        private static ErrorDto?
            FSL_ExpedienteGestiones_Gestion_Validar(
                FslExpedienteGestionAgregarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La informaci&oacute;n de la gesti&oacute;n es requerida.",
                    CodigoValidacion);
            }

            if (request.cod_expediente <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.cod_gestion))
            {
                return DbHelper.ErrorResponse(
                    MensajeGestionRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            return null;
        }
    }
}