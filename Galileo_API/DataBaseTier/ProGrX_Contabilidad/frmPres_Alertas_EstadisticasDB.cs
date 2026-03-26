using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ProGrX_Contabilidad;
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
        /// Busca el presupuesto según filtros puestos por el usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<List<PresVistaPresupuestoAlertasData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodCliente);
            PresVistaPresupuestoAlertasBuscar filtros = JsonConvert.DeserializeObject<PresVistaPresupuestoAlertasBuscar>(datos) ?? new PresVistaPresupuestoAlertasBuscar();

            var info = new ErrorDto<List<PresVistaPresupuestoAlertasData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresVistaPresupuestoAlertasData>()
            };
            try
            {
                var procedure = "[spPres_W_VistaPresupuestoAlertas]";
                var values = new
                {
                    COD_EMPRESA = CodCliente,
                    COD_CONTA = filtros.cod_conta,
                    COD_MODELO = filtros.cod_modelo,
                    COD_UNIDAD = filtros.cod_unidad,
                    CENTRO_COSTO = filtros.centro_costo,
                    ANIO = filtros.anio,
                    MES = filtros.mes,
                    TIPO_VISTA = filtros.tipo_vista,
                    CtaMov = filtros.ctaMov ? (short?)1 : null,
                    Tipo_Alerta = string.IsNullOrEmpty(filtros.tipo_alerta) ? "T" : filtros.tipo_alerta,
                    Justificacion = string.IsNullOrWhiteSpace(filtros.justificacion) ? "T" : filtros.justificacion
                };

                info.Result = connection.Query<PresVistaPresupuestoAlertasData>(procedure, values, commandType: CommandType.StoredProcedure, commandTimeout: 600).ToList();

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = new List<PresVistaPresupuestoAlertasData>();
            }

            return info;
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

                var primera = request.lineas.First();

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

                if ((DateTime.Now.Date - cierreFecha.Value.Date).TotalDays > 30)
                {
                    result.Code = -1;
                    result.Description = "El periodo excede los 30 días permitidos para justificar.";
                    result.Result.permitido_justificar = false;
                    result.Result.mensaje = result.Description;
                    return result;
                }

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

                if (periodo == null || (periodo.ESTADO ?? string.Empty) != "C")
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Solo se puede abrir justificación sobre un periodo cerrado."
                    };
                }

                DateTime? cierreFecha = periodo.CIERRE_FECHA;
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
                    inicio = new DateTime(request.anio, request.mes, 1),
                    corte = new DateTime(request.anio, request.mes, 1).AddMonths(1).AddDays(-1),
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

                //if ((DateTime.Now.Date - cierreFecha.Value.Date).TotalDays > 30)
                //{
                //    result.Code = -1;
                //    result.Result.periodo_registrado = false;
                //    result.Result.puede_guardar_seleccion = true;
                //    result.Result.requiere_configuracion = false;
                //    result.Result.fuera_de_plazo = true;
                //    result.Result.mensaje = "El periodo cerrado excede los 30 días permitidos para configurar justificación.";
                //    result.Description = "El periodo cerrado excede los 30 días permitidos para configurar justificación.";
                //    return result;
                //}

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

                result.Result.periodo_registrado = true;
                result.Result.puede_guardar_seleccion = false;
                result.Result.requiere_configuracion = false;
                result.Result.fuera_de_plazo = false;
                result.Result.inicio = justifica.INICIO;
                result.Result.corte = justifica.CORTE;
                result.Result.bloqueo_visualizacion = justifica.BLOQUEO_VISUALIZACION ?? false;
                result.Result.mensaje = "El periodo cerrado ya fue configurado para justificación. No se permite guardar otra selección.";

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

            if (validacion.Result.periodo_registrado)
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
    GETDATE(),
    @usuario,
    @bloqueo_visualizacion
);";

                connection.Execute(sqlInsert, new
                {
                    request.cod_modelo,
                    request.cod_contabilidad,
                    inicio = new DateTime(request.anio, request.mes, 1),
                    corte = new DateTime(request.anio, request.mes, 1).AddMonths(1).AddDays(-1),
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
    }
}
