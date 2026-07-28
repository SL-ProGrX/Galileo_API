using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de las Apelaciones de Expediente Fosol (frmFSL_ExpedienteApelaciones).
    /// </summary>
    public partial class FrmFslExpedienteApelacionesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslExpedienteApelacionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene los tipos de apelación activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de tipos de apelación.</returns>
        public ErrorDto<List<FslTipoApelacion>> FslTipoApelacion_Obtener(int CodCliente)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_apelacion AS item, RTRIM(cod_apelacion) + ' - ' + DESCRIPCION AS descripcion
                                     FROM FSL_TIPOS_APELACIONES WHERE ACTIVA = 1";
                return connection.Query<FslTipoApelacion>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslTipoApelacion>>("FslTipoApelacion_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Registra una apelación al expediente, validando que no exista una pendiente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="expediente">Datos de la apelación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslApelacion_Aplicar(int CodCliente, FslApleacionAplicar expediente)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlLinea = @"SELECT ISNULL(MAX(Linea), 0) AS Linea FROM FSL_EXPEDIENTES_APELACIONES
                                          WHERE cod_Expediente = @cod_expediente AND resolucion = 'P'";
                var linea = connection.QueryFirstOrDefault<int>(sqlLinea, new { expediente.cod_expediente });

                if (linea > 0)
                {
                    return DbHelper.ErrorResponse("Ya se encuentra registrada una apelación (Pendiente de Resolución) a este expediente, verifique!");
                }

                var res = connection.Query<int>("[spFSL_ApelacionRegistra]", new
                {
                    Expediente = expediente.cod_expediente,
                    tipo = expediente.cod_apelacion,
                    PresentaCedula = expediente.presentaCedula,
                    PresentaNombre = expediente.presentaNombre,
                    PresentaNotas = expediente.presentaNotas,
                    Usuario = expediente.usuario
                }, commandType: CommandType.StoredProcedure).FirstOrDefault();

                return res == 0
                    ? DbHelper.ErrorResponse("No fue posible aplicar la operación")
                    : new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Aplica la resolución de una apelación: valida el número de resolutores, actualiza expediente
        /// y apelación, y reasigna los miembros del comité.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="apelacion">JSON con la resolución.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslResolucionApelacion_Aplicar(int CodCliente, string apelacion)
        {
            var expediente = JsonConvert.DeserializeObject<FslResolucionApleacion>(apelacion) ?? new FslResolucionApleacion();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var numResolutores = connection.QueryFirstOrDefault<int>(
                    "SELECT NUMERO_RESOLUTORES FROM FSL_Comites WHERE cod_Comite = @cod_comite", new { expediente.cod_comite });

                if (expediente.miembros.Count < numResolutores)
                {
                    return DbHelper.ErrorResponse($"Debe de indicar al menos ({numResolutores}) miembros del comité VALIDADOS! que den la resolución!");
                }

                var linea = connection.QueryFirstOrDefault<int>(
                    "SELECT ISNULL(MAX(Linea), 0) AS Linea FROM FSL_EXPEDIENTES_APELACIONES WHERE cod_Expediente = @cod_expediente AND resolucion = 'P'",
                    new { expediente.cod_expediente });

                ActualizarResolucionExpediente(connection, expediente, linea);
                ReasignarComite(connection, expediente, linea);

                return DbHelper.OkResponse("Expediente actualizado satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el estado del expediente y los datos de resolución de la apelación.
        /// </summary>
        private static void ActualizarResolucionExpediente(SqlConnection connection, FslResolucionApleacion expediente, int linea)
        {
            connection.Execute(@"UPDATE FSL_EXPEDIENTES SET RESOLUCION_ESTADO = @estado, ESTADO = @resolucion_estado
                                 WHERE COD_EXPEDIENTE = @cod_expediente",
                new { expediente.estado, expediente.resolucion_estado, expediente.cod_expediente });

            connection.Execute(@"UPDATE FSL_EXPEDIENTES_APELACIONES
                                 SET RESOLUCION_NOTAS = @resolucion_notas, RESOLUCION = @cod_resolucion,
                                     RESOLUCION_FECHA = GETDATE(), RESOLUCION_USUARIO = @resolucion_usuario
                                 WHERE COD_EXPEDIENTE = @cod_expediente AND Linea = @linea",
                new { expediente.resolucion_notas, expediente.cod_resolucion, expediente.resolucion_usuario, expediente.cod_expediente, linea });
        }

        /// <summary>
        /// Reasigna los miembros del comité a la apelación del expediente.
        /// </summary>
        private static void ReasignarComite(SqlConnection connection, FslResolucionApleacion expediente, int linea)
        {
            connection.Execute("DELETE FSL_EXPEDIENTE_COMITE WHERE COD_EXPEDIENTE = @cod_expediente", new { expediente.cod_expediente });

            const string sqlInsert = @"INSERT FSL_EXPEDIENTES_APELACIONES_COMITE
                                        (LINEA, COD_EXPEDIENTE, COD_COMITE, CEDULA, ASIGNA_FECHA, ASIGNA_USUARIO)
                                       VALUES
                                        (@linea, @cod_expediente, @cod_comite, @cedula, GETDATE(), @resolucion_usuario)";

            foreach (var item in expediente.miembros)
            {
                connection.Execute(sqlInsert, new
                {
                    linea,
                    expediente.cod_expediente,
                    expediente.cod_comite,
                    item.cedula,
                    expediente.resolucion_usuario
                });
            }
        }
    }
}
