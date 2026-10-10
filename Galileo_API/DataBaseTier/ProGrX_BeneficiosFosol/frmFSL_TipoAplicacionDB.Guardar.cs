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
            return FSL_TipoAplicacion_Plan_Guardar(
                CodEmpresa,
                request,
                FslTipoAplicacionModoGuardar.Registrar);
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
            return FSL_TipoAplicacion_Plan_Guardar(
                CodEmpresa,
                request,
                FslTipoAplicacionModoGuardar.Actualizar);
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
                    movimiento = MovimientoEliminar,
                    detalle =
                        $"Planes de Aplicaci&oacute;n Id.:{codigo}",
                    ejecutar = connection =>
                        FSL_TipoAplicacion_Plan_Eliminar_Ejecutar(
                            connection,
                            codigo)
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
            return FSL_TipoAplicacion_Causa_Guardar(
                CodEmpresa,
                request,
                FslTipoAplicacionModoGuardar.Registrar);
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
            return FSL_TipoAplicacion_Causa_Guardar(
                CodEmpresa,
                request,
                FslTipoAplicacionModoGuardar.Actualizar);
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
                    movimiento = MovimientoEliminar,
                    detalle =
                        $"Planes de Apl: {codigoPlan}..Causa Id.:{codigoCausa}",
                    ejecutar = connection =>
                        FSL_TipoAplicacion_Causa_Eliminar_Ejecutar(
                            connection,
                            codigoPlan,
                            codigoCausa)
                });
        }

        /// <summary>
        /// Coordina el registro o la actualizaci&oacute;n
        /// de un plan de aplicaci&oacute;n.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del plan.
        /// </param>
        /// <param name="modo">
        /// Operaci&oacute;n de persistencia.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
        private ErrorDto
            FSL_TipoAplicacion_Plan_Guardar(
                int CodEmpresa,
                FslTipoAplicacionPlanGuardarRequest request,
                FslTipoAplicacionModoGuardar modo)
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

            var datos =
                FSL_TipoAplicacion_Plan_Datos_Crear(
                    request);

            var esRegistro =
                modo ==
                FslTipoAplicacionModoGuardar.Registrar;

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = datos.usuario,
                    movimiento = esRegistro
                        ? MovimientoRegistrar
                        : MovimientoModificar,
                    detalle =
                        $"Planes de Aplicaci&oacute;n Id.:{datos.codigo}",
                    ejecutar = connection =>
                        FSL_TipoAplicacion_Plan_Persistir(
                            connection,
                            datos,
                            modo)
                });
        }

        /// <summary>
        /// Crea la informaci&oacute;n normalizada de un plan.
        /// </summary>
        /// <param name="request">
        /// Informaci&oacute;n recibida.
        /// </param>
        /// <returns>
        /// Informaci&oacute;n normalizada.
        /// </returns>
        private static FslTipoAplicacionPlanDatos
            FSL_TipoAplicacion_Plan_Datos_Crear(
                FslTipoAplicacionPlanGuardarRequest request)
        {
            return new FslTipoAplicacionPlanDatos
            {
                codigo =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.cod_plan),
                descripcion =
                    FSL_TipoAplicacion_Texto_Normalizar(
                        request.descripcion),
                tipo_desembolso =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.tipo_desembolso),
                activo = request.activo,
                usuario =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.usuario)
            };
        }

        /// <summary>
        /// Persiste el registro o la actualizaci&oacute;n
        /// de un plan.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="datos">
        /// Informaci&oacute;n normalizada del plan.
        /// </param>
        /// <param name="modo">
        /// Operaci&oacute;n de persistencia.
        /// </param>
        /// <returns>
        /// Resultado de la persistencia.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Plan_Persistir(
                SqlConnection connection,
                FslTipoAplicacionPlanDatos datos,
                FslTipoAplicacionModoGuardar modo)
        {
            var existe =
                FSL_TipoAplicacion_Plan_Existe(
                    connection,
                    datos.codigo);

            if (
                modo ==
                    FslTipoAplicacionModoGuardar.Registrar &&
                existe
            )
            {
                return DbHelper.ErrorResponse(
                    MensajePlanExistente,
                    CodigoValidacion);
            }

            if (
                modo ==
                    FslTipoAplicacionModoGuardar.Actualizar &&
                !existe
            )
            {
                return DbHelper.ErrorResponse(
                    MensajePlanNoExiste,
                    CodigoValidacion);
            }

            return modo ==
                FslTipoAplicacionModoGuardar.Registrar
                    ? FSL_TipoAplicacion_Plan_Registrar_Ejecutar(
                        connection,
                        datos)
                    : FSL_TipoAplicacion_Plan_Actualizar_Ejecutar(
                        connection,
                        datos);
        }

        /// <summary>
        /// Ejecuta el registro de un plan.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="datos">
        /// Informaci&oacute;n normalizada del plan.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Plan_Registrar_Ejecutar(
                SqlConnection connection,
                FslTipoAplicacionPlanDatos datos)
        {
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
                    datos.codigo,
                    datos.descripcion,
                    tipoDesembolso =
                        datos.tipo_desembolso,
                    datos.activo,
                    datos.usuario
                });

            return DbHelper.OkResponse(
                "Plan registrado correctamente.");
        }

        /// <summary>
        /// Ejecuta la actualizaci&oacute;n de un plan.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="datos">
        /// Informaci&oacute;n normalizada del plan.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&oacute;n.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Plan_Actualizar_Ejecutar(
                SqlConnection connection,
                FslTipoAplicacionPlanDatos datos)
        {
            const string sql = """
                UPDATE FSL_PLANES
                SET
                    DESCRIPCION = @descripcion,
                    TIPO_DESEMBOLSO = @tipoDesembolso,
                    ACTIVO = @activo
                WHERE COD_PLAN = @codigo;
                """;

            connection.Execute(
                sql,
                new
                {
                    datos.codigo,
                    datos.descripcion,
                    tipoDesembolso =
                        datos.tipo_desembolso,
                    datos.activo
                });

            return DbHelper.OkResponse(
                "Plan actualizado correctamente.");
        }

        /// <summary>
        /// Ejecuta la eliminaci&oacute;n de un plan.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="codigo">
        /// C&oacute;digo del plan.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&oacute;n.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Plan_Eliminar_Ejecutar(
                SqlConnection connection,
                string codigo)
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

        /// <summary>
        /// Coordina el registro o la actualizaci&oacute;n
        /// de una causa.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n de la causa.
        /// </param>
        /// <param name="modo">
        /// Operaci&oacute;n de persistencia.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
        private ErrorDto
            FSL_TipoAplicacion_Causa_Guardar(
                int CodEmpresa,
                FslTipoAplicacionCausaGuardarRequest request,
                FslTipoAplicacionModoGuardar modo)
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

            var datos =
                FSL_TipoAplicacion_Causa_Datos_Crear(
                    request);

            var esRegistro =
                modo ==
                FslTipoAplicacionModoGuardar.Registrar;

            return FSL_TipoAplicacion_Operacion_Ejecutar(
                CodEmpresa,
                new FslTipoAplicacionOperacion
                {
                    usuario = datos.usuario,
                    movimiento = esRegistro
                        ? MovimientoRegistrar
                        : MovimientoModificar,
                    detalle =
                        $"Planes de Apl: {datos.codigo_plan}..Causa Id.:{datos.codigo_causa}",
                    ejecutar = connection =>
                        FSL_TipoAplicacion_Causa_Persistir(
                            connection,
                            datos,
                            modo)
                });
        }

        /// <summary>
        /// Crea la informaci&oacute;n normalizada de una causa.
        /// </summary>
        /// <param name="request">
        /// Informaci&oacute;n recibida.
        /// </param>
        /// <returns>
        /// Informaci&oacute;n normalizada.
        /// </returns>
        private static FslTipoAplicacionCausaDatos
            FSL_TipoAplicacion_Causa_Datos_Crear(
                FslTipoAplicacionCausaGuardarRequest request)
        {
            return new FslTipoAplicacionCausaDatos
            {
                codigo_causa =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.cod_causa),
                codigo_plan =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.cod_plan),
                descripcion =
                    FSL_TipoAplicacion_Texto_Normalizar(
                        request.descripcion),
                monto_base =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.monto_base),
                tipo_tabla =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.tipo_tabla),
                activa = request.activa,
                usuario =
                    FSL_TipoAplicacion_Codigo_Normalizar(
                        request.usuario)
            };
        }

        /// <summary>
        /// Persiste el registro o la actualizaci&oacute;n
        /// de una causa.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="datos">
        /// Informaci&oacute;n normalizada de la causa.
        /// </param>
        /// <param name="modo">
        /// Operaci&oacute;n de persistencia.
        /// </param>
        /// <returns>
        /// Resultado de la persistencia.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Causa_Persistir(
                SqlConnection connection,
                FslTipoAplicacionCausaDatos datos,
                FslTipoAplicacionModoGuardar modo)
        {
            var existe =
                FSL_TipoAplicacion_Causa_Existe(
                    connection,
                    datos.codigo_plan,
                    datos.codigo_causa);

            if (
                modo ==
                    FslTipoAplicacionModoGuardar.Registrar &&
                existe
            )
            {
                return DbHelper.ErrorResponse(
                    MensajeCausaExistente,
                    CodigoValidacion);
            }

            if (
                modo ==
                    FslTipoAplicacionModoGuardar.Actualizar &&
                !existe
            )
            {
                return DbHelper.ErrorResponse(
                    MensajeCausaNoExiste,
                    CodigoValidacion);
            }

            return modo ==
                FslTipoAplicacionModoGuardar.Registrar
                    ? FSL_TipoAplicacion_Causa_Registrar_Ejecutar(
                        connection,
                        datos)
                    : FSL_TipoAplicacion_Causa_Actualizar_Ejecutar(
                        connection,
                        datos);
        }

        /// <summary>
        /// Ejecuta el registro de una causa.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="datos">
        /// Informaci&oacute;n normalizada de la causa.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Causa_Registrar_Ejecutar(
                SqlConnection connection,
                FslTipoAplicacionCausaDatos datos)
        {
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
                    codigoCausa =
                        datos.codigo_causa,
                    codigoPlan =
                        datos.codigo_plan,
                    datos.descripcion,
                    montoBase =
                        datos.monto_base,
                    tipoTabla =
                        datos.tipo_tabla,
                    datos.activa,
                    datos.usuario
                });

            return DbHelper.OkResponse(
                "Causa registrada correctamente.");
        }

        /// <summary>
        /// Ejecuta la actualizaci&oacute;n de una causa.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="datos">
        /// Informaci&oacute;n normalizada de la causa.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&oacute;n.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Causa_Actualizar_Ejecutar(
                SqlConnection connection,
                FslTipoAplicacionCausaDatos datos)
        {
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
                    codigoCausa =
                        datos.codigo_causa,
                    codigoPlan =
                        datos.codigo_plan,
                    datos.descripcion,
                    montoBase =
                        datos.monto_base,
                    tipoTabla =
                        datos.tipo_tabla,
                    datos.activa
                });

            return DbHelper.OkResponse(
                "Causa actualizada correctamente.");
        }

        /// <summary>
        /// Ejecuta la eliminaci&oacute;n de una causa.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="codigoPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="codigoCausa">
        /// C&oacute;digo de la causa.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&oacute;n.
        /// </returns>
        private static ErrorDto
            FSL_TipoAplicacion_Causa_Eliminar_Ejecutar(
                SqlConnection connection,
                string codigoPlan,
                string codigoCausa)
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

        /// <summary>
        /// Ejecuta una operaci&oacute;n y registra su bit&aacute;cora
        /// cuando finaliza correctamente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="operacion">
        /// Informaci&oacute;n de la operaci&oacute;n.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
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

        /// <summary>
        /// Determina si existe un plan.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="codigo">
        /// C&oacute;digo del plan.
        /// </param>
        /// <returns>
        /// Verdadero cuando el plan existe.
        /// </returns>
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

        /// <summary>
        /// Determina si existe una causa asociada con un plan.
        /// </summary>
        /// <param name="connection">
        /// Conexi&oacute;n SQL activa.
        /// </param>
        /// <param name="codigoPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="codigoCausa">
        /// C&oacute;digo de la causa.
        /// </param>
        /// <returns>
        /// Verdadero cuando la causa existe.
        /// </returns>
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

        /// <summary>
        /// Valida la informaci&oacute;n requerida de un plan.
        /// </summary>
        /// <param name="request">
        /// Informaci&oacute;n del plan.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o texto vac&iacute;o.
        /// </returns>
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

        /// <summary>
        /// Valida la informaci&oacute;n requerida de una causa.
        /// </summary>
        /// <param name="request">
        /// Informaci&oacute;n de la causa.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o texto vac&iacute;o.
        /// </returns>
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

        /// <summary>
        /// Valida un c&oacute;digo y el usuario responsable.
        /// </summary>
        /// <param name="codigo">
        /// C&oacute;digo requerido.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <param name="mensajeCodigo">
        /// Mensaje utilizado cuando el c&oacute;digo es inv&aacute;lido.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o texto vac&iacute;o.
        /// </returns>
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

        /// <summary>
        /// Valida los datos requeridos para eliminar una causa.
        /// </summary>
        /// <param name="codigoPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="codigoCausa">
        /// C&oacute;digo de la causa.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o texto vac&iacute;o.
        /// </returns>
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

        private enum FslTipoAplicacionModoGuardar
        {
            Registrar,
            Actualizar
        }

        private sealed class
            FslTipoAplicacionPlanDatos
        {
            public required string codigo { get; init; }

            public required string descripcion { get; init; }

            public required string tipo_desembolso { get; init; }

            public required bool activo { get; init; }

            public required string usuario { get; init; }
        }

        private sealed class
            FslTipoAplicacionCausaDatos
        {
            public required string codigo_causa { get; init; }

            public required string codigo_plan { get; init; }

            public required string descripcion { get; init; }

            public required string monto_base { get; init; }

            public required string tipo_tabla { get; init; }

            public required bool activa { get; init; }

            public required string usuario { get; init; }
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