using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslExpedienteDB
    {
        /// <summary>
        /// Valida si el socio puede registrar el caso.
        /// </summary>
        public ErrorDto
            FSL_Expediente_Registro_Validar(
                int CodEmpresa,
                string cedula,
                string codPlan,
                string codCausa)
        {
            if (string.IsNullOrWhiteSpace(cedula) ||
                string.IsNullOrWhiteSpace(codPlan) ||
                string.IsNullOrWhiteSpace(codCausa))
            {
                return DbHelper.ErrorResponse(
                    "La c&eacute;dula, el plan y la causa son requeridos.",
                    CodigoValidacion);
            }

            using var connection =
                DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                var valido = FSL_Expediente_Registro_Valido(
                    connection,
                    null,
                    cedula,
                    codPlan,
                    codCausa,
                    0);

                return valido
                    ? DbHelper.CreateOkResponse()
                    : DbHelper.ErrorResponse(
                        "El caso ya fue presentado anteriormente, verifique.",
                        CodigoValidacion);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida las credenciales del miembro del comit&#233;.
        /// </summary>
        public ErrorDto
            FSL_Expediente_Miembro_Validar(
                int CodEmpresa,
                FslExpedienteMiembroValidarRequest request)
        {
            _ = CodEmpresa;

            if (string.IsNullOrWhiteSpace(request.usuario) ||
                string.IsNullOrWhiteSpace(request.clave))
            {
                return DbHelper.ErrorResponse(
                    "El usuario y la clave son requeridos.",
                    CodigoValidacion);
            }

            try
            {
                using var connection =
                    new SqlConnection(_securityConnectionString);

                var existe = connection.QueryFirstOrDefault<int>(
                    "spSEG_Logon",
                    new
                    {
                        Usuario = request.usuario.Trim(),
                        Clave = request.clave
                    },
                    commandType: CommandType.StoredProcedure);

                return existe == 0
                    ? DbHelper.ErrorResponse(
                        "No fue posible validar al usuario. Verifique su contrase&ntilde;a.",
                        CodigoValidacion)
                    : DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Aplica los c&#225;lculos FOSOL del expediente.
        /// </summary>
        public ErrorDto<FslExpedienteAplicarResultado>
            FSL_Expediente_Aplicar(
                int CodEmpresa,
                FslExpedienteAplicarRequest request)
        {
            var resultado = new FslExpedienteAplicarResultado();

            if (request.cod_expediente <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion,
                    resultado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion,
                    resultado);
            }

            using var connection =
                DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                resultado =
                    connection.QueryFirstOrDefault<
                        FslExpedienteAplicarResultado>(
                        "spFSL_AplicacionFosol",
                        new
                        {
                            Expediente = request.cod_expediente,
                            Usuario = request.usuario.Trim()
                        },
                        commandType: CommandType.StoredProcedure) ??
                    new FslExpedienteAplicarResultado();

                if (string.IsNullOrWhiteSpace(
                        resultado.tipo_documento) ||
                    string.IsNullOrWhiteSpace(
                        resultado.numero_documento))
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible aplicar el expediente.",
                        -1,
                        resultado);
                }

                return DbHelper.CreateOkResponse(
                    resultado,
                    "Aplicaci&oacute;n realizada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    resultado);
            }
        }

        private static string?
            FSL_Expediente_Datos_Validar(
                FslExpedienteGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "La c&eacute;dula es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return "El plan es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_causa))
            {
                return "La causa es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_comite))
            {
                return "El comit&eacute; es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_enfermedad))
            {
                return "La enfermedad es requerida.";
            }

            if (request.fecha_establece_causa == null ||
                request.enfermedad_fecha == null)
            {
                return "Las fechas del expediente son requeridas.";
            }

            if (request.notas.Trim().Length <= 10)
            {
                return MensajeNotasRequeridas;
            }

            if (string.IsNullOrWhiteSpace(request.registro_usuario) &&
                string.IsNullOrWhiteSpace(request.modifica_usuario))
            {
                return "El usuario es requerido.";
            }

            return null;
        }

        private static bool
            FSL_Expediente_Registro_Valido(
                SqlConnection connection,
                SqlTransaction? transaction,
                string cedula,
                string codPlan,
                string codCausa,
                long codExpediente)
        {
            const string sql = """
                SELECT dbo.fxFSL_ExpedienteValidaRegistro
                (
                    @cedula,
                    @cod_plan,
                    @cod_causa,
                    @cod_expediente
                );
                """;

            return connection.QueryFirstOrDefault<int>(
                       sql,
                       new
                       {
                           cedula = cedula.Trim(),
                           cod_plan = codPlan.Trim(),
                           cod_causa = codCausa.Trim(),
                           cod_expediente = codExpediente
                       },
                       transaction) != 0;
        }

        private static string?
            FSL_Expediente_ResolucionRequest_Validar(
                FslExpedienteResolucionGuardarRequest request)
        {
            if (request.cod_expediente <= 0)
            {
                return MensajeExpedienteRequerido;
            }

            if (string.IsNullOrWhiteSpace(request.cod_comite))
            {
                return "El comit&eacute; es requerido.";
            }

            if (string.IsNullOrWhiteSpace(
                    request.resolucion_usuario))
            {
                return "El usuario de resoluci&oacute;n es requerido.";
            }

            var estado = request.resolucion_estado.Trim();

            if (estado != EstadoAprobado &&
                estado != EstadoRechazado)
            {
                return "La resoluci&oacute;n indicada no es v&aacute;lida.";
            }

            if (request.resolucion_notas.Trim().Length < 10)
            {
                return "Indique una nota v&aacute;lida para la resoluci&oacute;n.";
            }

            return null;
        }

        private static string?
            FSL_Expediente_ResolucionContexto_Validar(
                FslResolucionContexto contexto,
                FslExpedienteResolucionGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(contexto.estado))
            {
                return MensajeExpedienteNoExiste;
            }

            if (contexto.estado != EstadoPendiente)
            {
                return "El expediente no se encuentra pendiente; no puede registrarse una resoluci&oacute;n.";
            }

            if (request.resolucion_estado == EstadoAprobado)
            {
                if (!contexto.cumple_requisitos)
                {
                    return "El expediente no cumple los requisitos para su aprobaci&oacute;n.";
                }

                if (!contexto.cumple_tiempo)
                {
                    return "El expediente no cumple el tiempo de presentaci&oacute;n para su aprobaci&oacute;n.";
                }

                if (!contexto.cumple_registro)
                {
                    return "El expediente no puede aprobarse porque existen otras solicitudes activas de la misma persona.";
                }
            }

            var totalMiembros = request.miembros
                .Where(item =>
                    !string.IsNullOrWhiteSpace(item.cedula))
                .Select(item => item.cedula.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return totalMiembros < contexto.numero_resolutores
                ? $"Debe indicar al menos ({contexto.numero_resolutores}) miembros del comit&eacute; validados."
                : null;
        }
    }
}