using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using System.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslTipoAplicacionDB
    {
        /// <summary>
        /// Registra un plan de aplicaci&oacute;n FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del plan.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto
            FSL_TipoAplicacion_Plan_Registrar(
                int CodEmpresa,
                FslTipoAplicacionPlanGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TipoAplicacion_Plan_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var codigo =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.cod_plan);

            var descripcion =
                FSL_TipoAplicacion_Texto_Normalizar(
                    request.descripcion);

            var tipoDesembolso =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.tipo_desembolso);

            var usuario =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.usuario);

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = usuario,
                    movimiento =
                        MovimientoRegistrar,
                    detalle =
                        $"Planes de Aplicaci&oacute;n Id.:{codigo}",
                    ejecutar = connection =>
                    {
                        if (
                            FSL_TipoAplicacion_Plan_Existe(
                                connection,
                                codigo)
                        )
                        {
                            return DbHelper.ErrorResponse(
                                MensajePlanExistente,
                                CodigoValidacion);
                        }

                        const string sql = """
                            INSERT INTO FSL_PLANES
                            (
                                COD_PLAN,
                                DESCRIPCION,
                                TIPO_DESEMBOLSO,
                                ACTIVO,
                                REGISTRO_FECHA,
                                REGISTRO_USUARIO
                            )
                            VALUES
                            (
                                @codigo,
                                @descripcion,
                                @tipoDesembolso,
                                @activo,
                                GETDATE(),
                                @usuario
                            );
                            """;

                        connection.Execute(
                            sql,
                            new
                            {
                                codigo,
                                descripcion,
                                tipoDesembolso,
                                activo = request.activo,
                                usuario
                            });

                        return DbHelper.OkResponse(
                            "Plan registrado correctamente.");
                    }
                });
        }

        /// <summary>
        /// Actualiza un plan de aplicaci&oacute;n FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del plan.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TipoAplicacion_Plan_Actualizar(
                int CodEmpresa,
                FslTipoAplicacionPlanGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TipoAplicacion_Plan_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var codigo =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.cod_plan);

            var descripcion =
                FSL_TipoAplicacion_Texto_Normalizar(
                    request.descripcion);

            var tipoDesembolso =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.tipo_desembolso);

            var usuario =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.usuario);

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = usuario,
                    movimiento =
                        MovimientoModificar,
                    detalle =
                        $"Planes de Aplicaci&oacute;n Id.:{codigo}",
                    ejecutar = connection =>
                    {
                        if (
                            !FSL_TipoAplicacion_Plan_Existe(
                                connection,
                                codigo)
                        )
                        {
                            return DbHelper.ErrorResponse(
                                MensajePlanNoExiste,
                                CodigoValidacion);
                        }

                        const string sql = """
                            UPDATE FSL_PLANES
                            SET
                                DESCRIPCION = @descripcion,
                                TIPO_DESEMBOLSO =
                                    @tipoDesembolso,
                                ACTIVO = @activo
                            WHERE COD_PLAN = @codigo;
                            """;

                        connection.Execute(
                            sql,
                            new
                            {
                                codigo,
                                descripcion,
                                tipoDesembolso,
                                activo = request.activo
                            });

                        return DbHelper.OkResponse(
                            "Plan actualizado correctamente.");
                    }
                });
        }

        /// <summary>
        /// Elimina un plan de aplicaci&oacute;n FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TipoAplicacion_Plan_Eliminar(
                int CodEmpresa,
                string? codPlan,
                string? usuario)
        {
            var codigo =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    codPlan);

            var usuarioNormalizado =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    usuario);

            var validacion =
                FSL_TipoAplicacion_CodigoUsuario_Validar(
                    codigo,
                    usuarioNormalizado,
                    MensajePlanRequerido);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = usuarioNormalizado,
                    movimiento =
                        MovimientoEliminar,
                    detalle =
                        $"Planes de Aplicaci&oacute;n Id.:{codigo}",
                    ejecutar = connection =>
                    {
                        if (
                            !FSL_TipoAplicacion_Plan_Existe(
                                connection,
                                codigo)
                        )
                        {
                            return DbHelper.ErrorResponse(
                                MensajePlanNoExiste,
                                CodigoValidacion);
                        }

                        const string sql = """
                            DELETE FROM FSL_PLANES
                            WHERE COD_PLAN = @codigo;
                            """;

                        connection.Execute(
                            sql,
                            new
                            {
                                codigo
                            });

                        return DbHelper.OkResponse(
                            "Plan eliminado correctamente.");
                    }
                });
        }

        /// <summary>
        /// Registra una causa asociada con un plan FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n de la causa.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto
            FSL_TipoAplicacion_Causa_Registrar(
                int CodEmpresa,
                FslTipoAplicacionCausaGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TipoAplicacion_Causa_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var codigoCausa =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.cod_causa);

            var codigoPlan =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.cod_plan);

            var descripcion =
                FSL_TipoAplicacion_Texto_Normalizar(
                    request.descripcion);

            var montoBase =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.monto_base);

            var tipoTabla =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.tipo_tabla);

            var usuario =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.usuario);

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = usuario,
                    movimiento =
                        MovimientoRegistrar,
                    detalle =
                        $"Planes de Apl: {codigoPlan}..Causa Id.:{codigoCausa}",
                    ejecutar = connection =>
                    {
                        if (
                            FSL_TipoAplicacion_Causa_Existe(
                                connection,
                                codigoPlan,
                                codigoCausa)
                        )
                        {
                            return DbHelper.ErrorResponse(
                                MensajeCausaExistente,
                                CodigoValidacion);
                        }

                        const string sql = """
                            INSERT INTO FSL_PLANES_CAUSAS
                            (
                                COD_CAUSA,
                                COD_PLAN,
                                DESCRIPCION,
                                MONTO_BASE,
                                TIPO_TABLA,
                                ACTIVA,
                                REGISTRO_FECHA,
                                REGISTRO_USUARIO
                            )
                            VALUES
                            (
                                @codigoCausa,
                                @codigoPlan,
                                @descripcion,
                                @montoBase,
                                @tipoTabla,
                                @activa,
                                GETDATE(),
                                @usuario
                            );
                            """;

                        connection.Execute(
                            sql,
                            new
                            {
                                codigoCausa,
                                codigoPlan,
                                descripcion,
                                montoBase,
                                tipoTabla,
                                activa = request.activa,
                                usuario
                            });

                        return DbHelper.OkResponse(
                            "Causa registrada correctamente.");
                    }
                });
        }

        /// <summary>
        /// Actualiza una causa asociada con un plan FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n de la causa.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TipoAplicacion_Causa_Actualizar(
                int CodEmpresa,
                FslTipoAplicacionCausaGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TipoAplicacion_Causa_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var codigoCausa =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.cod_causa);

            var codigoPlan =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.cod_plan);

            var descripcion =
                FSL_TipoAplicacion_Texto_Normalizar(
                    request.descripcion);

            var montoBase =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.monto_base);

            var tipoTabla =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.tipo_tabla);

            var usuario =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.usuario);

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = usuario,
                    movimiento =
                        MovimientoModificar,
                    detalle =
                        $"Planes de Apl: {codigoPlan}..Causa Id.:{codigoCausa}",
                    ejecutar = connection =>
                    {
                        if (
                            !FSL_TipoAplicacion_Causa_Existe(
                                connection,
                                codigoPlan,
                                codigoCausa)
                        )
                        {
                            return DbHelper.ErrorResponse(
                                MensajeCausaNoExiste,
                                CodigoValidacion);
                        }

                        const string sql = """
                            UPDATE FSL_PLANES_CAUSAS
                            SET
                                DESCRIPCION = @descripcion,
                                MONTO_BASE = @montoBase,
                                TIPO_TABLA = @tipoTabla,
                                ACTIVA = @activa
                            WHERE
                                COD_CAUSA = @codigoCausa
                                AND COD_PLAN = @codigoPlan;
                            """;

                        connection.Execute(
                            sql,
                            new
                            {
                                codigoCausa,
                                codigoPlan,
                                descripcion,
                                montoBase,
                                tipoTabla,
                                activa = request.activa
                            });

                        return DbHelper.OkResponse(
                            "Causa actualizada correctamente.");
                    }
                });
        }

        /// <summary>
        /// Elimina una causa asociada con un plan FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="codCausa">
        /// C&oacute;digo de la causa.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TipoAplicacion_Causa_Eliminar(
                int CodEmpresa,
                string? codPlan,
                string? codCausa,
                string? usuario)
        {
            var codigoPlan =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    codPlan);

            var codigoCausa =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    codCausa);

            var usuarioNormalizado =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    usuario);

            var validacion =
                FSL_TipoAplicacion_CausaEliminar_Validar(
                    codigoPlan,
                    codigoCausa,
                    usuarioNormalizado);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = usuarioNormalizado,
                    movimiento =
                        MovimientoEliminar,
                    detalle =
                        $"Planes de Apl: {codigoPlan}..Causa Id.:{codigoCausa}",
                    ejecutar = connection =>
                    {
                        if (
                            !FSL_TipoAplicacion_Causa_Existe(
                                connection,
                                codigoPlan,
                                codigoCausa)
                        )
                        {
                            return DbHelper.ErrorResponse(
                                MensajeCausaNoExiste,
                                CodigoValidacion);
                        }

                        const string sql = """
                            DELETE FROM FSL_PLANES_CAUSAS
                            WHERE
                                COD_CAUSA = @codigoCausa
                                AND COD_PLAN = @codigoPlan;
                            """;

                        connection.Execute(
                            sql,
                            new
                            {
                                codigoCausa,
                                codigoPlan
                            });

                        return DbHelper.OkResponse(
                            "Causa eliminada correctamente.");
                    }
                });
        }

        private ErrorDto
            FSL_TipoAplicacion_Operacion_Ejecutar(
                int CodEmpresa,
                FslTipoAplicacionOperacion operacion)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        CodEmpresa);

                var resultado =
                    operacion.ejecutar(connection);

                if (resultado.Code != 0)
                {
                    return resultado;
                }

                FSL_TipoAplicacion_Bitacora_Registrar(
                    CodEmpresa,
                    operacion.usuario,
                    operacion.movimiento,
                    operacion.detalle);

                return resultado;
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
            catch (SecurityException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        private static bool
            FSL_TipoAplicacion_Plan_Existe(
                SqlConnection connection,
                string codigo)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM FSL_PLANES
                WHERE COD_PLAN = @codigo;
                """;

            return connection
                .QueryFirstOrDefault<int>(
                    sql,
                    new
                    {
                        codigo
                    }) > 0;
        }

        private static bool
            FSL_TipoAplicacion_Causa_Existe(
                SqlConnection connection,
                string codigoPlan,
                string codigoCausa)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM FSL_PLANES_CAUSAS
                WHERE
                    COD_PLAN = @codigoPlan
                    AND COD_CAUSA = @codigoCausa;
                """;

            return connection
                .QueryFirstOrDefault<int>(
                    sql,
                    new
                    {
                        codigoPlan,
                        codigoCausa
                    }) > 0;
        }

        private static string
            FSL_TipoAplicacion_Plan_Validar(
                FslTipoAplicacionPlanGuardarRequest request)
        {
            if (
                string.IsNullOrWhiteSpace(
                    request.cod_plan)
            )
            {
                return MensajePlanRequerido;
            }

            if (
                string.IsNullOrWhiteSpace(
                    request.descripcion)
            )
            {
                return MensajePlanDescripcionRequerida;
            }

            var tipoDesembolso =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.tipo_desembolso);

            if (
                tipoDesembolso is not "F" and not "T"
            )
            {
                return MensajePlanTipoInvalido;
            }

            return string.IsNullOrWhiteSpace(
                request.usuario)
                    ? MensajeUsuarioRequerido
                    : string.Empty;
        }

        private static string
            FSL_TipoAplicacion_Causa_Validar(
                FslTipoAplicacionCausaGuardarRequest request)
        {
            if (
                string.IsNullOrWhiteSpace(
                    request.cod_plan)
            )
            {
                return MensajePlanRequerido;
            }

            if (
                string.IsNullOrWhiteSpace(
                    request.cod_causa)
            )
            {
                return MensajeCausaRequerida;
            }

            if (
                string.IsNullOrWhiteSpace(
                    request.descripcion)
            )
            {
                return MensajeCausaDescripcionRequerida;
            }

            var montoBase =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.monto_base);

            if (montoBase is not "F" and not "S")
            {
                return MensajeMontoBaseInvalido;
            }

            var tipoTabla =
                FSL_TipoAplicacion_Codigo_Normalizar(
                    request.tipo_tabla);

            if (
                tipoTabla is not "F"
                    and not "I"
                    and not "S"
                    and not "X"
            )
            {
                return MensajeTipoTablaInvalido;
            }

            return string.IsNullOrWhiteSpace(
                request.usuario)
                    ? MensajeUsuarioRequerido
                    : string.Empty;
        }

        private static string
            FSL_TipoAplicacion_CodigoUsuario_Validar(
                string codigo,
                string usuario,
                string mensajeCodigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return mensajeCodigo;
            }

            return string.IsNullOrWhiteSpace(usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        private static string
            FSL_TipoAplicacion_CausaEliminar_Validar(
                string codigoPlan,
                string codigoCausa,
                string usuario)
        {
            if (string.IsNullOrWhiteSpace(codigoPlan))
            {
                return MensajePlanRequerido;
            }

            return FSL_TipoAplicacion_CodigoUsuario_Validar(
                codigoCausa,
                usuario,
                MensajeCausaRequerida);
        }

        private sealed class
            FslTipoAplicacionOperacion
        {
            public required string usuario { get; init; }

            public required string movimiento { get; init; }

            public required string detalle { get; init; }

            public required Func<
                SqlConnection,
                ErrorDto> ejecutar
            { get; init; }
        }
    }
}