using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneProdPagoDB
    {
        private const string MensajeRegistrosRequeridos =
            "Debe seleccionar al menos un beneficio.";
        private const string MensajeUsuarioRequerido =
            "El usuario que realiza la entrega es requerido.";
        private const string MensajeSinRegistrosActualizados =
            "No se encontraron beneficios pendientes para actualizar.";

        /// <summary>
        /// Procesa la entrega de los beneficios seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Usuario y beneficios seleccionados.</param>
        /// <returns>Resultado del proceso de entrega.</returns>
        public ErrorDto AF_BeneProdPago_Entrega_Procesar(
            int CodEmpresa,
            AfiBeneProdPagoEntregaRequest request)
        {
            var validacion =
                AF_BeneProdPago_Entrega_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            using var connection = DbHelper.OpenConnection(
                AF_BeneProdPago_Portal_Crear(),
                CodEmpresa);

            try
            {
                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    const string sql = """
                        UPDATE afi_bene_otorga
                        SET
                            estado = 'E',
                            autoriza_user = @usuario,
                            autoriza_fecha = dbo.MyGetdate()
                        WHERE cedula = @cedula
                            AND cod_beneficio = @cod_beneficio
                            AND consec = @consec
                            AND estado = 'S'
                        """;

                    var usuario = request.usuario
                        .Trim()
                        .ToUpperInvariant();

                    var parametros = request.registros.Select(
                        registro => new
                        {
                            usuario,
                            cedula = registro.cedula.Trim(),
                            cod_beneficio =
                                registro.cod_beneficio.Trim(),
                            registro.consec
                        });

                    var registrosActualizados =
                        connection.Execute(
                            sql,
                            parametros,
                            transaction);

                    if (registrosActualizados == 0)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            MensajeSinRegistrosActualizados,
                            CodigoValidacion);
                    }

                    transaction.Commit();

                    return DbHelper.OkResponse(
                        "Los beneficios fueron entregados correctamente.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida la solicitud de entrega de beneficios.
        /// </summary>
        /// <param name="request">Solicitud que se debe validar.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string AF_BeneProdPago_Entrega_Validar(
            AfiBeneProdPagoEntregaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return MensajeUsuarioRequerido;
            }

            if (request.registros.Count == 0)
            {
                return MensajeRegistrosRequeridos;
            }

            if (request.registros.Any(
                registro =>
                    registro.consec <= 0 ||
                    string.IsNullOrWhiteSpace(
                        registro.cod_beneficio) ||
                    string.IsNullOrWhiteSpace(
                        registro.cedula)))
            {
                return "Los datos del beneficio seleccionado no son v&aacute;lidos.";
            }

            return string.Empty;
        }
    }
}