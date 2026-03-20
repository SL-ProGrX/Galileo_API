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
                const string query = @"SELECT COD_DESVIACION as item, CONCAT(DESCRIPCION, ' | Tipo: ',TIPO, ' | Operador: ',OPERADOR,' | Valor: ',VALOR_DESVIACION ) as DESCRIPCION  FROM PRES_TIPOS_DESVIACIONES WHERE ACTIVA = 1";

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

    }
}
