using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslExpedienteDB
    {
        /// <summary>
        /// Inserta un expediente validando notas y registro, y ejecuta los SP de requisitos y operaciones.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="jsonExp">JSON con los datos del expediente.</param>
        /// <returns>Resultado con el código del expediente en Description.</returns>
        public ErrorDto FslExpediente_Insertar(int CodCliente, string jsonExp)
        {
            var expediente = JsonConvert.DeserializeObject<FslExpedienteDatos>(jsonExp) ?? new FslExpedienteDatos();

            if (string.IsNullOrEmpty(expediente.notas))
            {
                return DbHelper.ErrorResponse("- Indique una Nota valida!");
            }

            if (expediente.notas.Length <= 10)
            {
                return DbHelper.ErrorResponse("- Notas del expediente debe tener mas de 10 letras!");
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var validacion = fxValida(connection, expediente.cedula, expediente.cod_plan, expediente.cod_causa);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                var tipoDesembolso = fxPlanTipoDesembolso(connection, expediente.cod_plan);
                InsertarExpediente(connection, expediente, tipoDesembolso);

                var vCodExpediente = fxExpedienteConsecutivo(connection, expediente.cedula);

                var reqError = spFSL_ExpedienteRequisitos(connection, vCodExpediente, expediente.registro_usuario);
                if (reqError.Code != 0)
                {
                    return reqError;
                }

                var opError = spFSL_ExpedienteOperaciones(connection, vCodExpediente, expediente.registro_usuario);
                if (opError.Code != 0)
                {
                    return opError;
                }

                return new ErrorDto { Code = 0, Description = vCodExpediente.ToString() };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un expediente pendiente y reejecuta los SP de requisitos y operaciones.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="jsonExp">JSON con los datos del expediente.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslExpediente_Actualizar(int CodCliente, string jsonExp)
        {
            var expediente = JsonConvert.DeserializeObject<FslExpedienteDatos>(jsonExp) ?? new FslExpedienteDatos();

            if (expediente.estado != "P")
            {
                return DbHelper.ErrorResponse("No se puede modificar este tramite porque no se encuentra pendiente");
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var tipoDesembolso = fxPlanTipoDesembolso(connection, expediente.cod_plan);
                ActualizarExpediente(connection, expediente, tipoDesembolso);

                var reqError = spFSL_ExpedienteRequisitos(connection, expediente.cod_expediente, expediente.registro_usuario);
                if (reqError.Code != 0)
                {
                    return reqError;
                }

                var opError = spFSL_ExpedienteOperaciones(connection, expediente.cod_expediente, expediente.registro_usuario);
                if (opError.Code != 0)
                {
                    return opError;
                }

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta el registro del expediente con consecutivo calculado por función.
        /// </summary>
        private static void InsertarExpediente(SqlConnection connection, FslExpedienteDatos e, string tipoDesembolso)
        {
            const string sql = @"INSERT FSL_EXPEDIENTES
                                    (COD_EXPEDIENTE, CEDULA, COD_PLAN, COD_CAUSA, COD_COMITE, COD_ENFERMEDAD, ESTADO, RESOLUCION_ESTADO,
                                     PRESENTA_CEDULA, PRESENTA_NOMBRE, PRESENTA_NOTAS, REFERENCIA_DOCUMENTO, REFERENCIA_NUMERO,
                                     ENFERMEDAD_FECHA, ENFERMEDAD_USUARIO, ENFERMEDAD_NOTAS, FECHA_ESTABLECE_CAUSA, NOTAS,
                                     TOTAL_DISPONIBLE, TOTAL_APLICADO, TOTAL_SOBRANTE, REGISTRO_FECHA, REGISTRO_USUARIO, TIPO_DESEMBOLSO)
                                 VALUES
                                    (dbo.fxFSL_ExpedienteConsecutivo(), @cedula, @cod_plan, @cod_causa, @cod_comite, @cod_enfermedad, 'P', 'P',
                                     @presenta_cedula, @presenta_nombre, @presenta_notas, @referencia_documento, @referencia_numero,
                                     @enfermedad_fecha, @enfermedad_usuario, @enfermedad_notas, @fecha_establece_causa, @notas,
                                     0, 0, 0, GETDATE(), @registro_usuario, @tipoDesembolso)";

            connection.Execute(sql, new
            {
                e.cedula, e.cod_plan, e.cod_causa, e.cod_comite, e.cod_enfermedad,
                e.presenta_cedula, e.presenta_nombre, e.presenta_notas, e.referencia_documento, e.referencia_numero,
                e.enfermedad_fecha, e.enfermedad_usuario, e.enfermedad_notas, e.fecha_establece_causa, e.notas,
                e.registro_usuario, tipoDesembolso
            });
        }

        /// <summary>
        /// Actualiza el registro del expediente.
        /// </summary>
        private static void ActualizarExpediente(SqlConnection connection, FslExpedienteDatos e, string tipoDesembolso)
        {
            const string sql = @"UPDATE FSL_EXPEDIENTES SET
                                    COD_PLAN = @cod_plan, COD_CAUSA = @cod_causa, COD_COMITE = @cod_comite, COD_ENFERMEDAD = @cod_enfermedad,
                                    notas = @notas, PRESENTA_CEDULA = @presenta_cedula, PRESENTA_NOMBRE = @presenta_nombre,
                                    REFERENCIA_DOCUMENTO = @referencia_documento, REFERENCIA_NUMERO = @referencia_numero,
                                    PRESENTA_NOTAS = @presenta_notas, FECHA_ESTABLECE_CAUSA = @fecha_establece_causa,
                                    ENFERMEDAD_FECHA = @enfermedad_fecha, ENFERMEDAD_NOTAS = @enfermedad_notas,
                                    MODIFICA_USUARIO = @modifica_usuario, MODIFICA_FECHA = GETDATE(), TIPO_DESEMBOLSO = @tipoDesembolso
                                 WHERE COD_EXPEDIENTE = @cod_expediente";

            connection.Execute(sql, new
            {
                e.cod_plan, e.cod_causa, e.cod_comite, e.cod_enfermedad, e.notas, e.presenta_cedula, e.presenta_nombre,
                e.referencia_documento, e.referencia_numero, e.presenta_notas, e.fecha_establece_causa,
                e.enfermedad_fecha, e.enfermedad_notas, e.modifica_usuario, tipoDesembolso, e.cod_expediente
            });
        }

        /// <summary>
        /// Obtiene el tipo de desembolso de un plan.
        /// </summary>
        private static string fxPlanTipoDesembolso(SqlConnection connection, string cod_plan)
        {
            const string sql = "SELECT TIPO_DESEMBOLSO FROM FSL_PLANES WHERE cod_plan = @cod_plan";
            return connection.QueryFirstOrDefault<string>(sql, new { cod_plan }) ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el último consecutivo de expediente de una cédula.
        /// </summary>
        private static long fxExpedienteConsecutivo(SqlConnection connection, string cedula)
        {
            const string sql = "SELECT ISNULL(MAX(cod_Expediente), 0) AS Ultimo FROM FSL_Expedientes WHERE Cedula = @cedula";
            return connection.QueryFirstOrDefault<long>(sql, new { cedula });
        }

        /// <summary>
        /// Ejecuta el SP que actualiza los requisitos del expediente.
        /// </summary>
        private static ErrorDto spFSL_ExpedienteRequisitos(SqlConnection connection, long expediente, string usuario)
        {
            try
            {
                connection.Execute("[spFSL_ExpedienteRequisitos]", new { Expediente = expediente, Usuario = usuario },
                    commandType: CommandType.StoredProcedure);
                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta el SP que actualiza los cálculos de créditos (FOSOL) del expediente.
        /// </summary>
        private static ErrorDto spFSL_ExpedienteOperaciones(SqlConnection connection, long expediente, string usuario)
        {
            try
            {
                connection.Execute("[spFSL_ExpedienteOperaciones]", new { Expediente = expediente, Usuario = usuario },
                    commandType: CommandType.StoredProcedure);
                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el estado de un requisito del expediente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="requisito">Datos del requisito.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslExpRequisto_Actualizar(int CodCliente, FslExpedienteUpdate requisito)
        {
            const string sql = @"UPDATE FSL_EXPEDIENTES_REQUISITOS
                                 SET Estado = @estado, registro_fecha = GETDATE(), registro_usuario = @registro_usuario
                                 WHERE cod_expediente = @cod_expediente AND Cod_Requisito = @cod_Requisito";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                estado = requisito.estado ? 1 : 0,
                requisito.registro_usuario,
                requisito.cod_expediente,
                requisito.cod_Requisito
            });

            if (result.Code == 0)
            {
                result.Description = "Requisito actualizado satisfactoriamente!";
            }

            return result;
        }

        /// <summary>
        /// Guarda la resolución de un expediente: valida el número de resolutores, actualiza el expediente
        /// y reasigna los miembros del comité.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="resolucion">Datos de la resolución.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslResolucion_Guardar(int CodCliente, FslResolucionGuardar resolucion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var numResolutores = connection.QueryFirstOrDefault<int>(
                    "SELECT NUMERO_RESOLUTORES FROM FSL_Comites WHERE cod_Comite = @cod_comite", new { resolucion.cod_comite });

                if (resolucion.miembros.Count < numResolutores)
                {
                    return DbHelper.ErrorResponse($"Debe de indicar al menos ({numResolutores}) miembros del comite VALIDADOS! que den la resolucion!");
                }

                connection.Execute(@"UPDATE FSL_EXPEDIENTES
                                     SET RESOLUCION_NOTAS = @resolucion_notas, RESOLUCION_ESTADO = @resolucion_estado,
                                         RESOLUCION_FECHA = GETDATE(), RESOLUCION_USUARIO = @resolucion_usuario, ESTADO = @resolucion_estado
                                     WHERE COD_EXPEDIENTE = @cod_expediente",
                    new { resolucion.resolucion_notas, resolucion.resolucion_estado, resolucion.resolucion_usuario, resolucion.cod_expediente });

                connection.Execute("DELETE FSL_EXPEDIENTE_COMITE WHERE COD_EXPEDIENTE = @cod_expediente", new { resolucion.cod_expediente });

                const string sqlInsert = @"INSERT FSL_EXPEDIENTE_COMITE
                                            (COD_EXPEDIENTE, COD_COMITE, CEDULA, ASIGNA_FECHA, ASIGNA_USUARIO, RESOLUCION_ESTADO)
                                           VALUES
                                            (@cod_expediente, @cod_comite, @cedula, GETDATE(), @resolucion_usuario, @estado)";

                foreach (var item in resolucion.miembros)
                {
                    connection.Execute(sqlInsert, new
                    {
                        resolucion.cod_expediente,
                        resolucion.cod_comite,
                        item.cedula,
                        resolucion.resolucion_usuario,
                        resolucion.estado
                    });
                }

                return DbHelper.OkResponse("Expediente actualizado satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
