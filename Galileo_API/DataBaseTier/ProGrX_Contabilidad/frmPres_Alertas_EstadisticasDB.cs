using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ProGrX_Contabilidad;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmPresAlertasEstadisticasDB
    {
        private readonly PortalDB _portalDb; 
        public FrmPresAlertasEstadisticasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Método para obtener una lista de tipos de alertas estadísticas.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PresAlertasEstadisticasTipos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"SELECT
                      COD_DESVIACION AS item
                    , CONCAT(DESCRIPCION, ' | Orden: ', ISNULL(CONVERT(varchar(20), ORDEN_EVALUACION), '0')) AS descripcion
                FROM PRES_TIPOS_DESVIACIONES
                WHERE ACTIVA = 1
                ORDER BY ISNULL(ORDEN_EVALUACION, 999999), COD_DESVIACION";

                return conn.Query<DropDownListaGenericaModel>(
                    query
                ).ToList();
            });
        }


        /// <summary>
        /// Obtiene la información de alertas presupuestarias según el estado del periodo
        /// y la configuración registrada para justificación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="datos">Filtros serializados.</param>
        /// <returns>Lista final a mostrar y estado de justificación.</returns>
        public ErrorDto<PresVistaPresupuestoAlertasResponse> PresPlanning_Obtener(int CodCliente, string datos)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodCliente);
            PresVistaPresupuestoAlertasBuscar filtros = JsonConvert.DeserializeObject<PresVistaPresupuestoAlertasBuscar>(datos) ?? new PresVistaPresupuestoAlertasBuscar();

            var info = new ErrorDto<PresVistaPresupuestoAlertasResponse>
            {
                Code = 0,
                Description = "OK",
                Result = new PresVistaPresupuestoAlertasResponse()
            };

            try
            {
                var periodo = ObtenerPeriodoContable(connection, filtros);

                if (periodo == null)
                {
                    info.Code = -1;
                    info.Description = "No existe el periodo contable.";
                    info.Result.mensaje = info.Description;
                    return info;
                }

                bool periodoCerrado = string.Equals(periodo.ESTADO ?? string.Empty, "C", StringComparison.OrdinalIgnoreCase);
                var justifica = ObtenerConfiguracionJustificacion(connection, filtros);

                if (!periodoCerrado)
                {
                    info.Result.lista = ObtenerVistaDesdeStoredProcedure(connection, CodCliente, filtros);
                    info.Result.permitir_justificar = false;
                    info.Result.usa_exclusiones = false;
                    info.Result.mensaje = "El periodo está abierto. Se muestra la vista actual y no se permiten justificaciones.";
                    return info;
                }

                if (justifica != null)
                {
                    info.Result.lista = ObtenerVistaDesdeExclusiones(connection, CodCliente, filtros);
                    info.Result.usa_exclusiones = true;

                    if (justifica.corte.HasValue && justifica.corte.Value.Date < DateTime.Now.Date)
                    {
                        info.Result.permitir_justificar = false;
                        info.Result.mensaje = "El periodo de justificación ya venció. Se muestran las exclusiones registradas.";
                    }
                    else if (justifica.bloqueo_visualizacion)
                    {
                        info.Result.permitir_justificar = false;
                        info.Result.mensaje = "El periodo está en bloqueo de visualización. Puede seguir ajustando exclusiones desde Control, pero aún no se permiten justificaciones.";
                    }
                    else
                    {
                        info.Result.permitir_justificar = true;
                        info.Result.mensaje = "El periodo está liberado para justificación. Se muestran las exclusiones registradas.";
                    }

                    return info;
                }

                info.Result.lista = ObtenerVistaDesdeStoredProcedure(connection, CodCliente, filtros);
                info.Result.permitir_justificar = false;
                info.Result.usa_exclusiones = false;
                info.Result.mensaje = "El periodo está cerrado pero no tiene configuración para justificación. Se muestra la vista actual.";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new PresVistaPresupuestoAlertasResponse();
            }

            return info;
        }

        /// <summary>
        /// Obtiene el periodo contable según los filtros indicados.
        /// </summary>
        private static dynamic? ObtenerPeriodoContable(SqlConnection connection, PresVistaPresupuestoAlertasBuscar filtros)
        {
            const string sqlPeriodo = @"
                    SELECT
                          ESTADO
                        , CIERRE_FECHA
                    FROM dbo.CNTX_PERIODOS
                    WHERE COD_CONTABILIDAD = @cod_conta
                      AND ANIO = @anio
                      AND MES = @mes;";

            return connection.QueryFirstOrDefault(sqlPeriodo, new
            {
                cod_conta = filtros.cod_conta,
                anio = filtros.anio,
                mes = filtros.mes
            });
        }

        /// <summary>
        /// Obtiene la configuración de justificación del periodo consultado.
        /// </summary>
        private static PresAlertasJustificaPeriodoData? ObtenerConfiguracionJustificacion(SqlConnection connection, PresVistaPresupuestoAlertasBuscar filtros)
        {
            const string sqlJustifica = @"
                SELECT TOP (1)
                      id_periodo
                    , cod_modelo
                    , cod_contabilidad
                    , inicio
                    , corte
                    , fecha
                    , usuario
                    , bloqueo_visualizacion
                FROM dbo.PRES_ALERTAS_JUSTICA_PERIODO
                WHERE cod_modelo = @cod_modelo
                  AND cod_contabilidad = @cod_conta
                  AND YEAR(inicio) = @anio
                  AND MONTH(inicio) = @mes
                ORDER BY id_periodo DESC;";

            return connection.QueryFirstOrDefault<PresAlertasJustificaPeriodoData>(sqlJustifica, new
            {
                cod_modelo = filtros.cod_modelo,
                cod_conta = filtros.cod_conta,
                anio = filtros.anio,
                mes = filtros.mes
            });
        }

        /// <summary>
        /// Obtiene la vista de alertas desde el stored procedure principal.
        /// </summary>
        private static List<PresVistaPresupuestoAlertasData> ObtenerVistaDesdeStoredProcedure(
            SqlConnection connection,
            int codCliente,
            PresVistaPresupuestoAlertasBuscar filtros)
        {
            const string procedure = "[spPres_W_VistaPresupuestoAlertas]";

            var values = new
            {
                COD_EMPRESA = codCliente,
                COD_CONTA = filtros.cod_conta,
                COD_MODELO = filtros.cod_modelo,
                COD_UNIDAD = filtros.cod_unidad,
                CENTRO_COSTO = filtros.centro_costo,
                ANIO = filtros.anio,
                MES = filtros.mes,
                TIPO_VISTA = filtros.tipo_vista,
                CtaMov = filtros.ctaMov ? (short?)1 : null,
                Tipo_Alerta = string.IsNullOrWhiteSpace(filtros.tipo_alerta) ? "T" : filtros.tipo_alerta,
                Justificacion = string.IsNullOrWhiteSpace(filtros.justificacion) ? "T" : filtros.justificacion
            };

            return connection.Query<PresVistaPresupuestoAlertasData>(
                procedure,
                values,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 600
            ).ToList();
        }

        /// <summary>
        /// Obtiene la vista de alertas desde las exclusiones del control,
        /// enriquecida con la justificación actual si existe.
        /// </summary>
        private static List<PresVistaPresupuestoAlertasData> ObtenerVistaDesdeExclusiones(
            SqlConnection connection,
            int codCliente,
            PresVistaPresupuestoAlertasBuscar filtros)
        {
            const string sqlExclusiones = @"
SELECT
      e.id_exclusion
    , e.cod_cuenta
    , e.cod_unidad
    , e.cod_centro_costo
    , e.cuenta
    , e.descripcion
    , ISNULL(e.real_mes, 0) AS real_mes
    , ISNULL(e.mensual, 0) AS mensual
    , ISNULL(e.diferencia_mes, 0) AS diferencia_mes
    , ISNULL(e.real_acumulado, 0) AS real_acumulado
    , ISNULL(e.acumulado, 0) AS acumulado
    , ISNULL(e.diferencia_acumulada, 0) AS diferencia_acumulada
    , ISNULL(e.pres_total, 0) AS pres_total
    , ISNULL(e.diferencia_total, 0) AS diferencia_total
    , ISNULL(e.ejecutado_mes, 0) AS ejecutado_mes
    , ISNULL(e.ejecutado_acumulado, 0) AS ejecutado_acumulado
    , ISNULL(e.ejecutado_total, 0) AS ejecutado_total
    , CAST(ISNULL(e.acepta_movimientos, 0) AS bit) AS acepta_movimientos
    , DATEFROMPARTS(e.anio, e.mes, 1) AS periodo
    , ISNULL(e.mensual, 0) AS pre_mensual_inicial
    , ISNULL(e.pres_total, 0) AS presupuesto
    , e.tipo_alerta AS alerta_tipo
    , e.alerta_descripcion
    , CAST(COALESCE(j.justificada, e.justificada, 0) AS bit) AS justificada
    , COALESCE(j.justificacion_actual, e.justificacion_actual, '') AS justificacion_actual
    , COALESCE(j.modifica_fecha, j.registro_fecha, e.justificacion_fecha) AS justificacion_fecha
    , COALESCE(j.modifica_usuario, j.registro_usuario, e.justificacion_usuario, '') AS justificacion_usuario
    , e.registro_fecha
    , ISNULL(e.registro_usuario, '') AS registro_usuario
FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION e
LEFT JOIN dbo.PRES_ALERTAS_JUSTIFICACIONES j
       ON j.cod_empresa = e.cod_empresa
      AND j.cod_conta = e.cod_contabilidad
      AND j.cod_modelo = e.cod_modelo
      AND j.cod_unidad = e.cod_unidad
      AND j.cod_centro_costo = e.cod_centro_costo
      AND j.cod_cuenta = e.cod_cuenta
      AND j.anio = e.anio
      AND j.mes = e.mes
      AND j.tipo_alerta = e.tipo_alerta
WHERE e.cod_empresa = @cod_empresa
  AND e.cod_contabilidad = @cod_conta
  AND e.cod_modelo = @cod_modelo
  AND e.anio = @anio
  AND e.mes = @mes
  AND (@tipo_alerta = 'T' OR e.tipo_alerta = @tipo_alerta)
  AND (
        @tipo_vista = 'G'
        OR (
             @cod_unidad = 'TODOS'
             AND (
                   @tipo_vista = 'U'
                   OR @tipo_vista = 'C'
                 )
           )
        OR (@tipo_vista = 'U' AND e.cod_unidad = @cod_unidad)
        OR (@tipo_vista = 'C' AND e.cod_unidad = @cod_unidad AND e.cod_centro_costo = @centro_costo)
      )
  AND (
        @justificacion = 'T'
        OR (@justificacion = 'S' AND COALESCE(j.justificada, e.justificada, 0) = 1)
        OR (@justificacion = 'N' AND COALESCE(j.justificada, e.justificada, 0) = 0)
      )
ORDER BY e.cuenta, e.cod_unidad, e.cod_centro_costo, e.tipo_alerta;";

            return connection.Query<PresVistaPresupuestoAlertasData>(sqlExclusiones, new
            {
                cod_empresa = codCliente,
                cod_conta = filtros.cod_conta,
                cod_modelo = filtros.cod_modelo,
                anio = filtros.anio,
                mes = filtros.mes,
                tipo_alerta = string.IsNullOrWhiteSpace(filtros.tipo_alerta) ? "T" : filtros.tipo_alerta,
                justificacion = string.IsNullOrWhiteSpace(filtros.justificacion) ? "T" : filtros.justificacion,
                tipo_vista = filtros.tipo_vista,
                cod_unidad = filtros.cod_unidad,
                centro_costo = filtros.centro_costo
            }).ToList();
        }

        /// <summary>
        /// Guarda o actualiza la justificación actual de una alerta presupuestaria
        /// y registra el movimiento en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="data">Datos de la justificación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PresAlertaJustificacion_Guardar(int codEmpresa, PresAlertaJustificacionGuardarRequest data)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var validacionPeriodo = PresAlertasJustificaPeriodo_Validar(codEmpresa, new PresAlertasJustificaPeriodoRequest
            {
                cod_modelo = data.cod_modelo,
                cod_contabilidad = data.cod_conta,
                anio = data.anio,
                mes = data.mes,
                usuario = data.usuario,
                bloqueo_visualizacion = false
            });

            if (validacionPeriodo.Code == -1 || validacionPeriodo.Result == null || !validacionPeriodo.Result.permitido_justificar)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = validacionPeriodo.Description
                };
            }

            var result = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {

                const string sqlExiste = @"
                        SELECT id_justificacion
                        FROM dbo.PRES_ALERTAS_JUSTIFICACIONES
                        WHERE cod_empresa = @cod_empresa
                          AND cod_conta = @cod_conta
                          AND cod_modelo = @cod_modelo
                          AND cod_unidad = @cod_unidad
                          AND cod_centro_costo = @cod_centro_costo
                          AND cod_cuenta = @cod_cuenta
                          AND anio = @anio
                          AND mes = @mes
                          AND tipo_alerta = @tipo_alerta;";

                var parametros = new
                {
                    cod_empresa = codEmpresa,
                    data.cod_conta,
                    data.cod_modelo,
                    data.cod_unidad,
                    data.cod_centro_costo,
                    data.cod_cuenta,
                    data.anio,
                    data.mes,
                    data.tipo_alerta,
                    data.alerta_descripcion,
                    data.justificada,
                    justificacion = data.justificacion,
                    data.usuario
                };

                int? idJustificacion = connection.QueryFirstOrDefault<int?>(
                    sqlExiste,
                    parametros
                );

                string accion;

                if (idJustificacion.HasValue)
                {
                    const string sqlUpdate = @"
                    UPDATE dbo.PRES_ALERTAS_JUSTIFICACIONES
                       SET alerta_descripcion = @alerta_descripcion,
                           justificada = @justificada,
                           justificacion_actual = @justificacion,
                           modifica_fecha = GETDATE(),
                           modifica_usuario = @usuario
                     WHERE id_justificacion = @id_justificacion;";

                    connection.Execute(
                        sqlUpdate,
                        new
                        {
                            id_justificacion = idJustificacion.Value,
                            data.alerta_descripcion,
                            data.justificada,
                            justificacion = data.justificacion,
                            data.usuario
                        }
                    );

                    accion = "MODIFICA";
                }
                else
                {
                    const string sqlInsert = @"
INSERT INTO dbo.PRES_ALERTAS_JUSTIFICACIONES
(
    cod_empresa,
    cod_conta,
    cod_modelo,
    cod_unidad,
    cod_centro_costo,
    cod_cuenta,
    anio,
    mes,
    tipo_alerta,
    alerta_descripcion,
    justificada,
    justificacion_actual,
    registro_fecha,
    registro_usuario
)
VALUES
(
    @cod_empresa,
    @cod_conta,
    @cod_modelo,
    @cod_unidad,
    @cod_centro_costo,
    @cod_cuenta,
    @anio,
    @mes,
    @tipo_alerta,
    @alerta_descripcion,
    @justificada,
    @justificacion,
    GETDATE(),
    @usuario
);

SELECT CAST(SCOPE_IDENTITY() AS int);";

                    idJustificacion = connection.QuerySingle<int>(
                        sqlInsert,
                        parametros
                    );

                    accion = "REGISTRA";
                }

                const string sqlBitacora = @"
INSERT INTO dbo.PRES_ALERTAS_JUSTIFICACIONES_BIT
(
    id_justificacion,
    cod_empresa,
    cod_conta,
    cod_modelo,
    cod_unidad,
    cod_centro_costo,
    cod_cuenta,
    anio,
    mes,
    tipo_alerta,
    accion,
    justificada,
    justificacion,
    fecha_registro,
    usuario_registro
)
VALUES
(
    @id_justificacion,
    @cod_empresa,
    @cod_conta,
    @cod_modelo,
    @cod_unidad,
    @cod_centro_costo,
    @cod_cuenta,
    @anio,
    @mes,
    @tipo_alerta,
    @accion,
    @justificada,
    @justificacion,
    GETDATE(),
    @usuario
);";

                connection.Execute(
                    sqlBitacora,
                    new
                    {
                        id_justificacion = idJustificacion.Value,
                        cod_empresa = codEmpresa,
                        data.cod_conta,
                        data.cod_modelo,
                        data.cod_unidad,
                        data.cod_centro_costo,
                        data.cod_cuenta,
                        data.anio,
                        data.mes,
                        data.tipo_alerta,
                        accion,
                        data.justificada,
                        justificacion = data.justificacion,
                        data.usuario
                    }
                );
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Obtiene la bitácora de justificaciones de una alerta presupuestaria.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codConta">Código de contabilidad.</param>
        /// <param name="codModelo">Código de modelo.</param>
        /// <param name="codUnidad">Código de unidad.</param>
        /// <param name="codCentroCosto">Código de centro de costo.</param>
        /// <param name="codCuenta">Código de cuenta.</param>
        /// <param name="anio">Año del periodo.</param>
        /// <param name="mes">Mes del periodo.</param>
        /// <param name="tipoAlerta">Tipo de alerta.</param>
        /// <returns>Lista de movimientos de bitácora.</returns>
        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBitacora_Obtener(
            PresAlertaJustificacionBitRequest resquest)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, resquest.codEmpresa);

            var result = new ErrorDto<List<PresAlertaJustificacionBitacoraData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertaJustificacionBitacoraData>()
            };

            try
            {
                const string sql = @"
SELECT
      id_bitacora
    , accion
    , justificada
    , justificacion
    , fecha_registro
    , usuario_registro
FROM dbo.PRES_ALERTAS_JUSTIFICACIONES_BIT
WHERE cod_empresa = @codEmpresa
  AND cod_conta = @codConta
  AND cod_modelo = @codModelo
  AND cod_unidad = @codUnidad
  AND cod_centro_costo = @codCentroCosto
  AND cod_cuenta = @codCuenta
  AND anio = @anio
  AND mes = @mes
  AND tipo_alerta = @tipoAlerta
ORDER BY id_bitacora DESC;";

                result.Result = connection.Query<PresAlertaJustificacionBitacoraData>(
                    sql,
                    new
                    {
                        resquest.codEmpresa,
                        resquest.codConta,
                        resquest.codModelo,
                        resquest.codUnidad,
                        resquest.codCentroCosto,
                        resquest.codCuenta,
                        resquest.anio,
                        resquest.mes,
                        resquest.tipoAlerta
                    }
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertaJustificacionBitacoraData>();
            }

            return result;
        }


        /// <summary>
        /// Obtiene el catálogo de tipos de justificación asociados a un tipo de alerta.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="tipoAlerta">Código del tipo de alerta.</param>
        /// <returns>Lista de tipos de justificación disponibles.</returns>
        public ErrorDto<List<PresAlertaTipoJustificacionData>> PresAlertaTipoJustificacion_Obtener(int codEmpresa, string tipoAlerta)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<List<PresAlertaTipoJustificacionData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertaTipoJustificacionData>()
            };

            try
            {
                const string sql = @"
                    SELECT
                          RTRIM(COD_TP_JUSTIFICACION) AS cod_tp_justificacion
                        , RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.PRES_TIPOS_JUSTIFICACION
                    WHERE ID_JUSTIFICACION = @tipoAlerta
                      AND ISNULL(ACTIVA, 0) = 1
                    ORDER BY COD_TP_JUSTIFICACION;";

                result.Result = connection.Query<PresAlertaTipoJustificacionData>(
                    sql,
                    new { tipoAlerta = (tipoAlerta ?? string.Empty).Trim().ToUpperInvariant() }
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertaTipoJustificacionData>();
            }

            return result;
        }


        #region Control de Alertas y Justificacion

        /// <summary>
        /// Guarda el respaldo de líneas seleccionadas del control de alertas presupuestarias.
        /// Antes de registrar, elimina el respaldo anterior del mismo periodo para mantener una sola foto vigente.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Líneas seleccionadas a respaldar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PresAlertasControlExclusion_Guardar(int codEmpresa, PresAlertasControlExclusionGuardarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                if (request.lineas == null || request.lineas.Count == 0)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Debe indicar al menos una línea seleccionada para guardar."
                    };
                }

                var primera = request.lineas[0];

                var estadoPeriodo = PresAlertasControlPeriodo_Validar(codEmpresa, new PresAlertasControlPeriodoConfigRequest
                {
                    cod_modelo = primera.cod_modelo,
                    cod_contabilidad = primera.cod_contabilidad,
                    anio = primera.anio,
                    mes = primera.mes,
                    usuario = request.usuario,
                    bloqueo_visualizacion = false,
                    fecha = DateTime.Now.Date,
                    corte = DateTime.Now.Date
                });

                if (estadoPeriodo.Code == 0
                    && estadoPeriodo.Result != null
                    && estadoPeriodo.Result.periodo_registrado
                    && !estadoPeriodo.Result.bloqueo_visualizacion)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El periodo ya fue liberado para justificación. No se permite modificar la selección."
                    };
                }

                const string sqlDeletePrevio = @"
DELETE FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION
WHERE cod_empresa = @cod_empresa
  AND cod_contabilidad = @cod_contabilidad
  AND cod_modelo = @cod_modelo
  AND anio = @anio
  AND mes = @mes;";

                connection.Execute(sqlDeletePrevio, new
                {
                    cod_empresa = codEmpresa,
                    cod_contabilidad = primera.cod_contabilidad,
                    cod_modelo = (primera.cod_modelo ?? string.Empty).Trim(),
                    anio = primera.anio,
                    mes = primera.mes
                });

                const string sqlInsert = @"
INSERT INTO dbo.PRES_ALERTAS_CONTROL_EXCLUSION
(
    cod_empresa,
    cod_contabilidad,
    cod_modelo,
    anio,
    mes,
    tipo_alerta,
    cod_cuenta,
    cod_unidad,
    cod_centro_costo,
    cuenta,
    descripcion,
    real_mes,
    mensual,
    diferencia_mes,
    real_acumulado,
    acumulado,
    diferencia_acumulada,
    pres_total,
    diferencia_total,
    ejecutado_mes,
    ejecutado_acumulado,
    ejecutado_total,
    acepta_movimientos,
    alerta_descripcion,
    justificada,
    justificacion_actual,
    justificacion_fecha,
    justificacion_usuario,
    registro_usuario,
    registro_fecha
)
VALUES
(
    @cod_empresa,
    @cod_contabilidad,
    @cod_modelo,
    @anio,
    @mes,
    @tipo_alerta,
    @cod_cuenta,
    @cod_unidad,
    @cod_centro_costo,
    @cuenta,
    @descripcion,
    @real_mes,
    @mensual,
    @diferencia_mes,
    @real_acumulado,
    @acumulado,
    @diferencia_acumulada,
    @pres_total,
    @diferencia_total,
    @ejecutado_mes,
    @ejecutado_acumulado,
    @ejecutado_total,
    @acepta_movimientos,
    @alerta_descripcion,
    @justificada,
    @justificacion_actual,
    @justificacion_fecha,
    @justificacion_usuario,
    @registro_usuario,
    GETDATE()
);";

                foreach (var item in request.lineas)
                {
                    connection.Execute(sqlInsert, new
                    {
                        cod_empresa = codEmpresa,
                        cod_contabilidad = item.cod_contabilidad,
                        cod_modelo = (item.cod_modelo ?? string.Empty).Trim(),
                        anio = item.anio,
                        mes = item.mes,
                        tipo_alerta = (item.tipo_alerta ?? string.Empty).Trim(),
                        cod_cuenta = (item.cod_cuenta ?? string.Empty).Trim(),
                        cod_unidad = (item.cod_unidad ?? string.Empty).Trim(),
                        cod_centro_costo = (item.cod_centro_costo ?? string.Empty).Trim(),
                        cuenta = item.cuenta,
                        descripcion = item.descripcion,
                        real_mes = item.real_mes,
                        mensual = item.mensual,
                        diferencia_mes = item.diferencia_mes,
                        real_acumulado = item.real_acumulado,
                        acumulado = item.acumulado,
                        diferencia_acumulada = item.diferencia_acumulada,
                        pres_total = item.pres_total,
                        diferencia_total = item.diferencia_total,
                        ejecutado_mes = item.ejecutado_mes,
                        ejecutado_acumulado = item.ejecutado_acumulado,
                        ejecutado_total = item.ejecutado_total,
                        acepta_movimientos = item.acepta_movimientos,
                        alerta_descripcion = item.alerta_descripcion,
                        justificada = item.justificada,
                        justificacion_actual = item.justificacion_actual,
                        justificacion_fecha = item.justificacion_fecha,
                        justificacion_usuario = item.justificacion_usuario,
                        registro_usuario = (request.usuario ?? string.Empty).Trim()
                    });
                }

                result.Description = "OK";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Obtiene las líneas excluidas del control de alertas para un periodo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro del periodo.</param>
        /// <returns>Lista de exclusiones.</returns>
        public ErrorDto<List<PresAlertasControlExclusionData>> PresAlertasControlExclusion_Obtener(int codEmpresa, PresAlertasControlExclusionFiltroRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<List<PresAlertasControlExclusionData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertasControlExclusionData>()
            };

            try
            {
                const string sql = @"
SELECT
      id_exclusion
    , cod_empresa
    , cod_contabilidad
    , cod_modelo
    , anio
    , mes
    , tipo_alerta
    , cod_cuenta
    , cod_unidad
    , cod_centro_costo
    , cuenta
    , descripcion
    , real_mes
    , mensual
    , diferencia_mes
    , real_acumulado
    , acumulado
    , diferencia_acumulada
    , pres_total
    , diferencia_total
    , ejecutado_mes
    , ejecutado_acumulado
    , ejecutado_total
    , acepta_movimientos
    , alerta_descripcion
    , justificada
    , justificacion_actual
    , justificacion_fecha
    , justificacion_usuario
    , registro_usuario
    , registro_fecha
FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION
WHERE cod_empresa = @cod_empresa
  AND cod_contabilidad = @cod_contabilidad
  AND cod_modelo = @cod_modelo
  AND anio = @anio
  AND mes = @mes
  AND (@tipo_alerta = 'T' OR tipo_alerta = @tipo_alerta)
ORDER BY tipo_alerta, cuenta;";

                result.Result = connection.Query<PresAlertasControlExclusionData>(sql, new
                {
                    cod_empresa = codEmpresa,
                    request.cod_contabilidad,
                    request.cod_modelo,
                    request.anio,
                    request.mes,
                    tipo_alerta = string.IsNullOrWhiteSpace(request.tipo_alerta) ? "T" : request.tipo_alerta
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertasControlExclusionData>();
            }

            return result;
        }

        /// <summary>
        /// Elimina una línea excluida del control de alertas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Registro a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PresAlertasControlExclusion_Eliminar(int codEmpresa, PresAlertasControlExclusionEliminarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                const string sql = @"
DELETE FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION
WHERE id_exclusion = @id_exclusion;";

                var rows = connection.Execute(sql, new
                {
                    request.id_exclusion
                });

                result.Code = rows;
                result.Description = rows > 0 ? "OK" : "No existe el registro.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Valida si un periodo cerrado permite registrar justificaciones de alertas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Periodo a validar.</param>
        /// <returns>Estado del periodo para justificar.</returns>
        public ErrorDto<PresAlertasJustificaPeriodoData> PresAlertasJustificaPeriodo_Validar(int codEmpresa, PresAlertasJustificaPeriodoRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<PresAlertasJustificaPeriodoData>
            {
                Code = 0,
                Description = "OK",
                Result = new PresAlertasJustificaPeriodoData()
            };

            try
            {
                const string sqlPeriodo = @"
SELECT
      ANIO
    , MES
    , COD_CONTABILIDAD
    , ESTADO
    , CIERRE_FECHA
FROM dbo.CNTX_PERIODOS
WHERE COD_CONTABILIDAD = @cod_contabilidad
  AND ANIO = @anio
  AND MES = @mes;";

                var periodo = connection.QueryFirstOrDefault(sqlPeriodo, new
                {
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                });

                if (periodo == null)
                {
                    result.Code = -1;
                    result.Description = "No existe el periodo contable.";
                    result.Result.permitido_justificar = false;
                    result.Result.mensaje = result.Description;
                    return result;
                }

                string estado = periodo.ESTADO ?? string.Empty;
                DateTime? cierreFecha = periodo.CIERRE_FECHA;

                if (estado != "C")
                {
                    result.Code = -1;
                    result.Description = "Solo se puede habilitar justificación en periodos cerrados.";
                    result.Result.permitido_justificar = false;
                    result.Result.mensaje = result.Description;
                    return result;
                }

                if (!cierreFecha.HasValue)
                {
                    result.Code = -1;
                    result.Description = "El periodo cerrado no tiene fecha de cierre.";
                    result.Result.permitido_justificar = false;
                    result.Result.mensaje = result.Description;
                    return result;
                }

                /**
                ** Se comenta temporalmente hasta validar un presupuesto actualizado 
                if ((DateTime.Now.Date - cierreFecha.Value.Date).TotalDays > 30)
                {
                    result.Code = -1;
                    result.Description = "El periodo excede los 30 días permitidos para justificar.";
                    result.Result.permitido_justificar = false;
                    result.Result.mensaje = result.Description;
                    return result;
                }
                **/

                const string sqlJustifica = @"
SELECT TOP (1)
      id_periodo
    , cod_modelo
    , cod_contabilidad
    , inicio
    , corte
    , fecha
    , usuario
    , bloqueo_visualizacion
FROM dbo.PRES_ALERTAS_JUSTICA_PERIODO
WHERE cod_modelo = @cod_modelo
  AND cod_contabilidad = @cod_contabilidad
  AND YEAR(inicio) = @anio
  AND MONTH(inicio) = @mes
ORDER BY id_periodo DESC;";

                var data = connection.QueryFirstOrDefault<PresAlertasJustificaPeriodoData>(sqlJustifica, new
                {
                    request.cod_modelo,
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                });

                if (data == null)
                {
                    result.Code = -1;
                    result.Description = "El periodo no está habilitado para justificar.";
                    result.Result = new PresAlertasJustificaPeriodoData
                    {
                        permitido_justificar = false,
                        mensaje = result.Description
                    };
                    return result;
                }

                if (data.corte.HasValue && data.corte.Value.Date < DateTime.Now.Date)
                {
                    result.Code = -1;
                    result.Description = "El periodo de justificación ya venció.";
                    data.permitido_justificar = false;
                    data.mensaje = result.Description;
                    result.Result = data;
                    return result;
                }

                if (data.bloqueo_visualizacion)
                {
                    result.Code = -1;
                    result.Description = "El periodo continúa bloqueado para visualización. Debe liberarlo desde Control antes de justificar.";
                    data.permitido_justificar = false;
                    data.mensaje = result.Description;
                    result.Result = data;
                    return result;
                }

                data.permitido_justificar = true;
                data.mensaje = "OK";
                result.Result = data;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new PresAlertasJustificaPeriodoData
                {
                    permitido_justificar = false,
                    mensaje = ex.Message
                };
            }

            return result;
        }

        /// <summary>
        /// Registra la habilitación de un periodo cerrado para justificar alertas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del periodo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PresAlertasJustificaPeriodo_Abrir(int codEmpresa, PresAlertasJustificaPeriodoRequest request)
        {
            var validacion = PresAlertasJustificaPeriodo_Validar(codEmpresa, request);

            if (validacion.Code == 0 && validacion.Result != null && validacion.Result.permitido_justificar)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El periodo ya está habilitado para justificar."
                };
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                const string sqlPeriodo = @"
SELECT
      ESTADO
    , CIERRE_FECHA
FROM dbo.CNTX_PERIODOS
WHERE COD_CONTABILIDAD = @cod_contabilidad
  AND ANIO = @anio
  AND MES = @mes;";

                var periodo = connection.QueryFirstOrDefault(sqlPeriodo, new
                {
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                });

                if (periodo == null || (periodo!.ESTADO ?? string.Empty) != "C")
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Solo se puede abrir justificación sobre un periodo cerrado."
                    };
                }

                DateTime? cierreFecha = periodo!.CIERRE_FECHA;
                if (!cierreFecha.HasValue || (DateTime.Now.Date - cierreFecha.Value.Date).TotalDays > 30)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El periodo no cumple la regla de máximo 30 días desde el cierre."
                    };
                }

                const string sqlInsert = @"
INSERT INTO dbo.PRES_ALERTAS_JUSTICA_PERIODO
(
    cod_modelo,
    cod_contabilidad,
    inicio,
    corte,
    fecha,
    usuario,
    bloqueo_visualizacion
)
VALUES
(
    @cod_modelo,
    @cod_contabilidad,
    @inicio,
    @corte,
    GETDATE(),
    @usuario,
    @bloqueo_visualizacion
);";

                connection.Execute(sqlInsert, new
                {
                    request.cod_modelo,
                    request.cod_contabilidad,
                    inicio = new DateTime(request.anio, request.mes, 1, 0, 0, 0, DateTimeKind.Local),
                    corte = new DateTime(request.anio, request.mes, 1, 0, 0, 0, DateTimeKind.Local).AddMonths(1).AddDays(-1),
                    request.usuario,
                    bloqueo_visualizacion = request.bloqueo_visualizacion
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Valida el estado de un periodo para control de alertas y configuración de justificación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del periodo.</param>
        /// <returns>Estado del periodo para control y justificación.</returns>
        public ErrorDto<PresAlertasControlPeriodoEstadoData> PresAlertasControlPeriodo_Validar(int codEmpresa, PresAlertasControlPeriodoConfigRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<PresAlertasControlPeriodoEstadoData>
            {
                Code = 0,
                Description = "OK",
                Result = new PresAlertasControlPeriodoEstadoData()
            };

            try
            {
                const string sqlPeriodo = @"
SELECT
      ESTADO
    , CIERRE_FECHA
FROM dbo.CNTX_PERIODOS
WHERE COD_CONTABILIDAD = @cod_contabilidad
  AND ANIO = @anio
  AND MES = @mes;";

                var periodo = connection.QueryFirstOrDefault(sqlPeriodo, new
                {
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                });

                if (periodo == null)
                {
                    result.Code = -1;
                    result.Description = "No existe el periodo contable.";
                    result.Result.mensaje = result.Description;
                    return result;
                }

                string estado = periodo.ESTADO ?? string.Empty;
                DateTime? cierreFecha = periodo.CIERRE_FECHA;

                result.Result.periodo_cerrado = estado == "C";
                result.Result.cierre_fecha = cierreFecha;

                if (estado != "C")
                {
                    result.Code = -1;
                    result.Result.periodo_registrado = false;
                    result.Result.puede_guardar_seleccion = true;
                    result.Result.requiere_configuracion = false;
                    result.Result.fuera_de_plazo = false;
                    result.Description = "El periodo está pendiente. Puede guardar selección, pero no requiere configuración de justificación.";
                    result.Result.mensaje = "El periodo está pendiente. Puede guardar selección, pero no requiere configuración de justificación.";
                    return result;
                }

                if (!cierreFecha.HasValue)
                {
                    result.Code = -1;
                    result.Result.periodo_registrado = false;
                    result.Result.puede_guardar_seleccion = true;
                    result.Result.requiere_configuracion = true;
                    result.Result.fuera_de_plazo = true;
                    result.Result.mensaje = "El periodo está cerrado, pero no tiene fecha de cierre.";
                    result.Description = "El periodo está cerrado, pero no tiene fecha de cierre.";
                    return result;
                }

                /**
                Se comenta temporalmente para revisar casos de periodos sin fecha de cierre, pero se deja la validación para futuros ajustes en la regla de negocio.
                if ((DateTime.Now.Date - cierreFecha.Value.Date).TotalDays > 30)
                {
                    result.Code = -1;
                    result.Result.periodo_registrado = false;
                    result.Result.puede_guardar_seleccion = true;
                    result.Result.requiere_configuracion = false;
                    result.Result.fuera_de_plazo = true;
                    result.Result.mensaje = "El periodo cerrado excede los 30 días permitidos para configurar justificación.";
                    result.Description = "El periodo cerrado excede los 30 días permitidos para configurar justificación.";
                    return result;
                }
                **/

                const string sqlJustifica = @"
SELECT TOP (1)
      INICIO
    , CORTE
    , BLOQUEO_VISUALIZACION
FROM dbo.PRES_ALERTAS_JUSTICA_PERIODO
WHERE COD_MODELO = @cod_modelo
  AND COD_CONTABILIDAD = @cod_contabilidad
  AND YEAR(INICIO) = @anio
  AND MONTH(INICIO) = @mes
ORDER BY FECHA DESC;";

                var justifica = connection.QueryFirstOrDefault(sqlJustifica, new
                {
                    request.cod_modelo,
                    request.cod_contabilidad,
                    request.anio,
                    request.mes
                });

                if (justifica == null)
                {
                    result.Result.periodo_registrado = false;
                    result.Result.puede_guardar_seleccion = true;
                    result.Result.requiere_configuracion = true;
                    result.Result.fuera_de_plazo = false;
                    result.Result.mensaje = "El periodo está cerrado y requiere configuración para justificación.";
                    return result;
                }

                bool bloqueoVisualizacion = justifica.BLOQUEO_VISUALIZACION ?? false;

                result.Result.periodo_registrado = true;
                result.Result.puede_guardar_seleccion = bloqueoVisualizacion;
                result.Result.requiere_configuracion = false;
                result.Result.fuera_de_plazo = false;
                result.Result.inicio = justifica.INICIO;
                result.Result.corte = justifica.CORTE;
                result.Result.bloqueo_visualizacion = bloqueoVisualizacion;
                result.Result.mensaje = bloqueoVisualizacion
                    ? "El periodo cerrado ya fue configurado y sigue bloqueado. Puede continuar guardando selección."
                    : "El periodo cerrado ya fue liberado para justificación. No se permite guardar otra selección.";

                return result;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new PresAlertasControlPeriodoEstadoData
                {
                    mensaje = ex.Message
                };
                return result;
            }
        }

        /// <summary>
        /// Registra la configuración de un periodo cerrado para habilitar justificaciones.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del periodo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PresAlertasControlPeriodo_Registrar(int codEmpresa, PresAlertasControlPeriodoConfigRequest request)
        {
            var validacion = PresAlertasControlPeriodo_Validar(codEmpresa, request);

            if (validacion.Code == -1)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = validacion.Description
                };
            }

            if (validacion.Result!.periodo_registrado)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El periodo ya fue configurado para justificación."
                };
            }

            if (!validacion.Result.requiere_configuracion)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = validacion.Result.mensaje
                };
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            var fecha = request.fecha?.Date;
            var corte = request.corte?.Date;
            var hoy = DateTime.Now.Date;
            var fechaMax = hoy.AddDays(30);

            if (fecha != hoy)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La fecha del registro debe ser la fecha actual."
                };
            }

            if (corte < hoy)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La fecha fin no puede ser menor a la fecha actual."
                };
            }

            if (corte > fechaMax)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La fecha fin no puede ser mayor a 30 días desde la fecha actual."
                };
            }

            try
            {
                const string sqlInsert = @"
                        INSERT INTO dbo.PRES_ALERTAS_JUSTICA_PERIODO
                        (
                            COD_MODELO,
                            COD_CONTABILIDAD,
                            INICIO,
                            CORTE,
                            FECHA,
                            USUARIO,
                            BLOQUEO_VISUALIZACION
                        )
                        VALUES
                        (
                            @cod_modelo,
                            @cod_contabilidad,
                            @inicio,
                            @corte,
                            @fecha,
                            @usuario,
                            @bloqueo_visualizacion
                        );";

                connection.Execute(sqlInsert, new
                {
                    cod_modelo = request.cod_modelo,
                    cod_contabilidad = request.cod_contabilidad,
                    inicio = new DateTime(request.anio, request.mes, 1, 0, 0, 0, DateTimeKind.Local),
                    corte = request.corte?.Date,
                    fecha = request.fecha?.Date,
                    usuario = request.usuario,
                    bloqueo_visualizacion =request.bloqueo_visualizacion
                });

               
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Obtiene los periodos habilitados para justificar alertas presupuestarias.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro de consulta por contabilidad y modelo.</param>
        /// <returns>Lista de periodos registrados en PRES_ALERTAS_JUSTICA_PERIODO.</returns>
        public ErrorDto<List<PresAlertasJustificaPeriodoConsultaData>> PresAlertasJustificaPeriodo_Obtener(
            int codEmpresa,
            PresAlertasJustificaPeriodoConsultaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<List<PresAlertasJustificaPeriodoConsultaData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertasJustificaPeriodoConsultaData>()
            };

            try
            {
                const string sql = @"
SELECT
      id_periodo
    , RTRIM(cod_modelo) AS cod_modelo
    , cod_contabilidad
    , inicio
    , corte
    , fecha
    , RTRIM(usuario) AS usuario
    , CAST(ISNULL(bloqueo_visualizacion, 0) AS bit) AS bloqueo_visualizacion
FROM dbo.PRES_ALERTAS_JUSTICA_PERIODO
WHERE cod_contabilidad = @cod_contabilidad
  AND (@cod_modelo = '' OR cod_modelo = @cod_modelo)
ORDER BY inicio DESC, fecha DESC;";

                result.Result = connection.Query<PresAlertasJustificaPeriodoConsultaData>(
                    sql,
                    new
                    {
                        cod_contabilidad = request.cod_contabilidad,
                        cod_modelo = (request.cod_modelo ?? string.Empty).Trim()
                    }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertasJustificaPeriodoConsultaData>();
            }

            return result;
        }

        /// <summary>
        /// Actualiza el bloqueo de visualización de un periodo registrado.
        /// Solo permite pasar de bloqueo Sí a bloqueo No.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Periodo a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PresAlertasControlPeriodo_ActualizarBloqueo(int codEmpresa, PresAlertasControlPeriodoBloqueoActualizarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };

            try
            {
                const string sqlConsulta = @"
SELECT TOP (1)
      id_periodo
    , CAST(ISNULL(bloqueo_visualizacion, 0) AS bit) AS bloqueo_visualizacion
FROM dbo.PRES_ALERTAS_JUSTICA_PERIODO
WHERE id_periodo = @id_periodo;";

                var periodo = connection.QueryFirstOrDefault(sqlConsulta, new
                {
                    request.id_periodo
                });

                if (periodo == null)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "No se encontró el periodo indicado."
                    };
                }

                bool bloqueoActual = periodo.bloqueo_visualizacion ?? false;

                if (!bloqueoActual)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "El periodo ya está liberado y no permite más cambios."
                    };
                }

                if (request.bloqueo_visualizacion)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Solo se permite liberar el periodo cambiando el bloqueo a No."
                    };
                }

                const string sqlUpdate = @"
UPDATE dbo.PRES_ALERTAS_JUSTICA_PERIODO
   SET bloqueo_visualizacion = @bloqueo_visualizacion,
       usuario = @usuario,
       fecha = GETDATE()
WHERE id_periodo = @id_periodo;";

                connection.Execute(sqlUpdate, new
                {
                    request.id_periodo,
                    request.usuario,
                    request.bloqueo_visualizacion
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        #region DashBoard

        /// <summary>
        /// Obtiene el resumen general del dashboard de alertas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro del dashboard.</param>
        /// <returns>Totales de exclusiones y justificaciones.</returns>
        public ErrorDto<PresAlertasDashboardResumenData> PresAlertasDashboardResumen_Obtener(int codEmpresa, PresAlertasDashboardFiltroRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<PresAlertasDashboardResumenData>
            {
                Code = 0,
                Description = "OK",
                Result = new PresAlertasDashboardResumenData()
            };

            try
            {
                const string sql = @"
SELECT
      COUNT(1) AS total_excluidas
    , SUM(CASE WHEN ISNULL(j.justificada, ISNULL(e.justificada, 0)) = 1 THEN 1 ELSE 0 END) AS total_justificadas
    , SUM(CASE WHEN ISNULL(j.justificada, ISNULL(e.justificada, 0)) = 0 THEN 1 ELSE 0 END) AS total_pendientes
FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION e
LEFT JOIN dbo.PRES_ALERTAS_JUSTIFICACIONES j
       ON j.cod_empresa = e.cod_empresa
      AND j.cod_conta = e.cod_contabilidad
      AND j.cod_modelo = e.cod_modelo
      AND j.cod_unidad = e.cod_unidad
      AND j.cod_centro_costo = e.cod_centro_costo
      AND j.cod_cuenta = e.cod_cuenta
      AND j.anio = e.anio
      AND j.mes = e.mes
      AND j.tipo_alerta = e.tipo_alerta
WHERE e.cod_empresa = @cod_empresa
  AND e.cod_contabilidad = @cod_contabilidad
  AND e.cod_modelo = @cod_modelo
  AND e.anio = @anio
  AND e.mes = @mes
  AND (@tipo_alerta = 'T' OR e.tipo_alerta = @tipo_alerta);";

                var data = connection.QueryFirstOrDefault<PresAlertasDashboardResumenData>(sql, new
                {
                    cod_empresa = codEmpresa,
                    request.cod_contabilidad,
                    request.cod_modelo,
                    request.anio,
                    request.mes,
                    tipo_alerta = string.IsNullOrWhiteSpace(request.tipo_alerta) ? "T" : request.tipo_alerta
                }) ?? new PresAlertasDashboardResumenData();

                if (data.total_excluidas > 0)
                {
                    data.porcentaje_justificado = Math.Round((decimal)data.total_justificadas * 100m / data.total_excluidas, 2);
                }

                result.Result = data;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new PresAlertasDashboardResumenData();
            }

            return result;
        }

        /// <summary>
        /// Obtiene el resumen de exclusiones y justificaciones por unidad.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro del dashboard.</param>
        /// <returns>Lista agrupada por unidad.</returns>
        public ErrorDto<List<PresAlertasDashboardUnidadData>> PresAlertasDashboardUnidad_Obtener(int codEmpresa, PresAlertasDashboardFiltroRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<List<PresAlertasDashboardUnidadData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertasDashboardUnidadData>()
            };

            try
            {
                const string sql = @"
SELECT
      ISNULL(e.cod_unidad, 'SIN_UNIDAD') AS cod_unidad
    , COUNT(1) AS excluidas
    , SUM(CASE WHEN ISNULL(j.justificada, ISNULL(e.justificada, 0)) = 1 THEN 1 ELSE 0 END) AS justificadas
    , SUM(CASE WHEN ISNULL(j.justificada, ISNULL(e.justificada, 0)) = 0 THEN 1 ELSE 0 END) AS pendientes
FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION e
LEFT JOIN dbo.PRES_ALERTAS_JUSTIFICACIONES j
       ON j.cod_empresa = e.cod_empresa
      AND j.cod_conta = e.cod_contabilidad
      AND j.cod_modelo = e.cod_modelo
      AND j.cod_unidad = e.cod_unidad
      AND j.cod_centro_costo = e.cod_centro_costo
      AND j.cod_cuenta = e.cod_cuenta
      AND j.anio = e.anio
      AND j.mes = e.mes
      AND j.tipo_alerta = e.tipo_alerta
WHERE e.cod_empresa = @cod_empresa
  AND e.cod_contabilidad = @cod_contabilidad
  AND e.cod_modelo = @cod_modelo
  AND e.anio = @anio
  AND e.mes = @mes
  AND (@tipo_alerta = 'T' OR e.tipo_alerta = @tipo_alerta)
GROUP BY e.cod_unidad
ORDER BY e.cod_unidad;";

                result.Result = connection.Query<PresAlertasDashboardUnidadData>(sql, new
                {
                    cod_empresa = codEmpresa,
                    request.cod_contabilidad,
                    request.cod_modelo,
                    request.anio,
                    request.mes,
                    tipo_alerta = string.IsNullOrWhiteSpace(request.tipo_alerta) ? "T" : request.tipo_alerta
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertasDashboardUnidadData>();
            }

            return result;
        }

        /// <summary>
        /// Obtiene el resumen por categoría de alerta y tipo de justificación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro del dashboard.</param>
        /// <returns>Lista resumida por alerta y justificación.</returns>
        public ErrorDto<List<PresAlertasDashboardJustificacionData>> PresAlertasDashboardJustificacion_Obtener(int codEmpresa, PresAlertasDashboardFiltroRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new ErrorDto<List<PresAlertasDashboardJustificacionData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertasDashboardJustificacionData>()
            };

            try
            {
                const string sql = @"
SELECT
      e.tipo_alerta
    , MAX(e.alerta_descripcion) AS alerta_descripcion
    , LTRIM(RTRIM(
        CASE
          WHEN CHARINDEX('|', ISNULL(j.justificacion_actual, e.justificacion_actual)) > 0
            THEN LEFT(ISNULL(j.justificacion_actual, e.justificacion_actual), CHARINDEX('|', ISNULL(j.justificacion_actual, e.justificacion_actual)) - 1)
          ELSE ISNULL(j.justificacion_actual, e.justificacion_actual)
        END
      )) AS tipo_justificacion
    , COUNT(1) AS cantidad
FROM dbo.PRES_ALERTAS_CONTROL_EXCLUSION e
LEFT JOIN dbo.PRES_ALERTAS_JUSTIFICACIONES j
       ON j.cod_empresa = e.cod_empresa
      AND j.cod_conta = e.cod_contabilidad
      AND j.cod_modelo = e.cod_modelo
      AND j.cod_unidad = e.cod_unidad
      AND j.cod_centro_costo = e.cod_centro_costo
      AND j.cod_cuenta = e.cod_cuenta
      AND j.anio = e.anio
      AND j.mes = e.mes
      AND j.tipo_alerta = e.tipo_alerta
WHERE e.cod_empresa = @cod_empresa
  AND e.cod_contabilidad = @cod_contabilidad
  AND e.cod_modelo = @cod_modelo
  AND e.anio = @anio
  AND e.mes = @mes
  AND (@tipo_alerta = 'T' OR e.tipo_alerta = @tipo_alerta)
  AND ISNULL(j.justificada, ISNULL(e.justificada, 0)) = 1
GROUP BY
      e.tipo_alerta
    , LTRIM(RTRIM(
        CASE
          WHEN CHARINDEX('|', ISNULL(j.justificacion_actual, e.justificacion_actual)) > 0
            THEN LEFT(ISNULL(j.justificacion_actual, e.justificacion_actual), CHARINDEX('|', ISNULL(j.justificacion_actual, e.justificacion_actual)) - 1)
          ELSE ISNULL(j.justificacion_actual, e.justificacion_actual)
        END
      ))
ORDER BY e.tipo_alerta, tipo_justificacion;";

                result.Result = connection.Query<PresAlertasDashboardJustificacionData>(sql, new
                {
                    cod_empresa = codEmpresa,
                    request.cod_contabilidad,
                    request.cod_modelo,
                    request.anio,
                    request.mes,
                    tipo_alerta = string.IsNullOrWhiteSpace(request.tipo_alerta) ? "T" : request.tipo_alerta
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertasDashboardJustificacionData>();
            }

            return result;
        }

        #endregion

    }
}
