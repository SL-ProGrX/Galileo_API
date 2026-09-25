using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public partial class FrmInvTranRequisicionesDB
    {
        /// <summary>
        /// Inserta el encabezado de una requisición con las columnas usadas por VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la requisición.</param>
        /// <returns>Consecutivo generado.</returns>
        public ErrorDto InvTranRequisicion_Insertar(
            int CodEmpresa,
            TranRequisicionData? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos de la requisici&oacute;n son requeridos.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "La causa de la requisici&oacute;n es requerida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.genera_user))
            {
                return DbHelper.ErrorResponse(
                    "El usuario generador es requerido.",
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        int consecutivo =
                            InvTranRequisiciones_Consecutivo_Obtener(
                                connection,
                                transaction);

                        connection.Execute(
                            """
                            INSERT INTO pv_requisiciones
                                (cod_requisicion, cod_entsal, genera_fecha,
                                 documento, notas, genera_user, estado,
                                 plantilla)
                            VALUES
                                (@CodRequisicion, @CodEntsal, GETDATE(),
                                 @Documento, @Notas, @GeneraUser, 'S',
                                 @Plantilla)
                            """,
                            new
                            {
                                CodRequisicion = consecutivo,
                                CodEntsal = request.cod_entsal.Trim(),
                                Documento = request.documento ?? string.Empty,
                                Notas = request.notas ?? string.Empty,
                                GeneraUser = request.genera_user.Trim(),
                                Plantilla = request.plantilla
                            },
                            transaction);

                        transaction.Commit();

                        return consecutivo.ToString(
                            "D10",
                            CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return result.Code == 0
                ? DbHelper.OkResponse(result.Result ?? string.Empty)
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al insertar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el encabezado de una requisición solicitada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la requisición.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto InvTranRequisicion_Actualizar(
            int CodEmpresa,
            TranRequisicionData? request)
        {
            if (request is null || request.cod_requisicion <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeCodigoRequisicionRequerido,
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "La causa de la requisici&oacute;n es requerida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.genera_user))
            {
                return DbHelper.ErrorResponse(
                    "El usuario generador es requerido.",
                    -2);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                UPDATE pv_requisiciones
                SET cod_entsal = @CodEntsal,
                    genera_fecha = GETDATE(),
                    documento = @Documento,
                    notas = @Notas,
                    genera_user = @GeneraUser,
                    plantilla = @Plantilla
                WHERE cod_requisicion = @CodRequisicion
                  AND estado = 'S'
                """,
                new
                {
                    CodRequisicion = request.cod_requisicion,
                    CodEntsal = request.cod_entsal.Trim(),
                    Documento = request.documento ?? string.Empty,
                    Notas = request.notas ?? string.Empty,
                    GeneraUser = request.genera_user.Trim(),
                    Plantilla = request.plantilla
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al actualizar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "Requisici&oacute;n actualizada correctamente.")
                : DbHelper.ErrorResponse(
                    "La requisici&oacute;n no existe o no est&aacute; solicitada.",
                    -2);
        }

        /// <summary>
        /// Elimina una requisición y su detalle mediante el endpoint existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto InvTranRequesicion_Eliminar(
            int CodEmpresa,
            int CodRequisicion)
        {
            if (CodRequisicion <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeCodigoRequisicionRequerido,
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        connection.Execute(
                            """
                            DELETE FROM pv_requi_detalle
                            WHERE cod_requisicion = @CodRequisicion
                            """,
                            new { CodRequisicion },
                            transaction);

                        int afectados = connection.Execute(
                            """
                            DELETE FROM pv_requisiciones
                            WHERE cod_requisicion = @CodRequisicion
                            """,
                            new { CodRequisicion },
                            transaction);

                        transaction.Commit();
                        return afectados;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al eliminar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "Requisici&oacute;n eliminada correctamente.")
                : DbHelper.ErrorResponse(
                    "No se encontr&oacute; la requisici&oacute;n.",
                    -2);
        }

        /// <summary>
        /// Reemplaza el detalle de una requisición como lo hace VB6 al guardar.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="producLineas">Líneas visibles que se van a guardar.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto InvRequesicionProduc_Insertar(
            int CodEmpresa,
            int CodRequisicion,
            List<InvReqProduc>? producLineas)
        {
            if (CodRequisicion <= 0 || producLineas is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del detalle son requeridos.",
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        string? estado = connection.QueryFirstOrDefault<string>(
                            """
                            SELECT estado
                            FROM pv_requisiciones WITH (UPDLOCK, HOLDLOCK)
                            WHERE cod_requisicion = @CodRequisicion
                            """,
                            new { CodRequisicion },
                            transaction);

                        if (estado != "S")
                        {
                            transaction.Rollback();
                            return false;
                        }

                        InvTranRequisiciones_Detalle_Eliminar(
                            connection,
                            transaction,
                            CodRequisicion);

                        for (int indice = 0; indice < producLineas.Count; indice++)
                        {
                            InvReqProduc item = producLineas[indice];

                            if (string.IsNullOrWhiteSpace(item.cod_producto)
                                || item.cantidad <= 0)
                            {
                                continue;
                            }

                            InvTranRequisiciones_Detalle_Insertar(
                                connection,
                                transaction,
                                CodRequisicion,
                                indice + 1,
                                item);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al guardar los productos de la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result
                ? DbHelper.OkResponse(
                    "Informaci&oacute;n guardada satisfactoriamente.")
                : DbHelper.ErrorResponse(
                    "La requisici&oacute;n no existe o no est&aacute; solicitada.",
                    -2);
        }

        /// <summary>
        /// Elimina una línea mediante el endpoint existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="Linea">Número de línea.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto InvRequisicionProduc_Eliminar(
            int CodEmpresa,
            int CodRequisicion,
            int Linea)
        {
            if (CodRequisicion <= 0 || Linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La requisici&oacute;n y la l&iacute;nea son requeridas.",
                    -2);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                DELETE FROM pv_requi_detalle
                WHERE cod_requisicion = @CodRequisicion
                  AND linea = @Linea
                """,
                new { CodRequisicion, Linea });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al eliminar la l&iacute;nea de requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "L&iacute;nea de requisici&oacute;n eliminada correctamente.")
                : DbHelper.ErrorResponse(
                    "No se encontr&oacute; la l&iacute;nea de requisici&oacute;n.",
                    -2);
        }
    }
}