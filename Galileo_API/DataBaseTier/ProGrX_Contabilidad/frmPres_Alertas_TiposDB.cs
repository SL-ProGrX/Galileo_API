using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmPresAlertasTiposDb
    {
        private readonly IConfiguration _config;

        public FrmPresAlertasTiposDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codCliente)
        {
            var connString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codCliente);
            return new SqlConnection(connString);
        }

        private static string NormalizeCode(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
        private static string NormalizeText(string? value) => (value ?? string.Empty).Trim();

        #endregion

        /// <summary>
        /// Obtiene la lista lazy de tipos de alerta 
        /// </summary>
        public ErrorDto<AlertasTiposLista> AlertasTipos_Obtener(
            int codCliente,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            var response = new ErrorDto<AlertasTiposLista>
            {
                Result = new AlertasTiposLista()
            };

            const string sqlBaseCount = @"
                SELECT COUNT(*)
                FROM PRES_TIPOS_DESVIACIONES";

            const string sqlBaseSelect = @"
                SELECT 
                    cod_desviacion,
                    descripcion,
                    activa,
                    requiere_justificacion,
                    tipo,
                    valor_desviacion,
                    registro_usuario,
                    registro_fecha,
                    modifica_fecha,
                    modifica_usuario
                FROM PRES_TIPOS_DESVIACIONES";

            try
            {
                using var connection = CreateConnection(codCliente);

                var parameters = new DynamicParameters();
                var whereBuilder = new StringBuilder();

                // WHERE (filtro)
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    whereBuilder.Append(" WHERE (COD_DESVIACION LIKE @Filtro OR DESCRIPCION LIKE @Filtro)");
                    parameters.Add("Filtro", $"%{filtro}%");
                }

                // 1) Total de registros (con filtro si aplica)
                var countQueryBuilder = new StringBuilder(sqlBaseCount);
                if (whereBuilder.Length > 0)
                {
                    countQueryBuilder.Append(whereBuilder);
                }

                response.Result.total = connection.ExecuteScalar<int>(countQueryBuilder.ToString(), parameters);

                // 2) Query de datos (select + where + order + paginación)
                var dataQueryBuilder = new StringBuilder(sqlBaseSelect);
                if (whereBuilder.Length > 0)
                {
                    dataQueryBuilder.Append(whereBuilder);
                }

                dataQueryBuilder.Append(" ORDER BY COD_DESVIACION");

                // Paginar solo si vienen ambos valores
                if (pagina.HasValue && paginacion.HasValue)
                {
                    // Mantengo tu lógica original: usabas "OFFSET pagina ROWS"
                    var offset = (pagina.Value) * paginacion.Value;
                    var pageSize = paginacion.Value;

                    dataQueryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                    parameters.Add("Offset", offset, DbType.Int32);
                    parameters.Add("PageSize", pageSize, DbType.Int32);
                }

                var finalSql = dataQueryBuilder.ToString();
                response.Result.lista = connection.Query<AlertasTiposDto>(finalSql, parameters).ToList();

                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "AlertasTipos_Obtener: " + ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Guardo el registro del tipo de Alerta
        /// </summary>
        public ErrorDto AlertasTipos_Insertar(int codCliente, AlertasTiposDto alertatipo)
        {
            var resp = new ErrorDto();

            const string insertSql = @"
                INSERT INTO PRES_TIPOS_DESVIACIONES
                (
                    [COD_DESVIACION],
                    [DESCRIPCION],
                    [ACTIVA],
                    [REQUIERE_JUSTIFICACION],
                    [TIPO],
                    [VALOR_DESVIACION],
                    [REGISTRO_USUARIO],
                    [REGISTRO_FECHA],
                    [MODIFICA_USUARIO],
                    [MODIFICA_FECHA]
                )
                VALUES
                (
                    @CodDesviacion,
                    @Descripcion,
                    @Activa,
                    @RequiereJustificacion,
                    @Tipo,
                    @ValorDesviacion,
                    @RegistroUsuario,
                    GETDATE(),
                    @ModificaUsuario,
                    @ModificaFecha
                );";

            try
            {
                using var connection = CreateConnection(codCliente);

                var parameters = new
                {
                    CodDesviacion = alertatipo.cod_desviacion,
                    Descripcion = alertatipo.descripcion,
                    Activa = alertatipo.activa,
                    RequiereJustificacion = alertatipo.requiere_justificacion,
                    Tipo = alertatipo.tipo,
                    ValorDesviacion = alertatipo.valor_desviacion,
                    RegistroUsuario = alertatipo.registro_usuario,
                    ModificaUsuario = alertatipo.modifica_usuario,
                    ModificaFecha = alertatipo.modifica_fecha
                };

                resp.Code = connection.Execute(insertSql, parameters);
                resp.Description = resp.Code > 0 ? "OK" : "No se insertó ningún registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTipos_Insertar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza el tipo de alerta
        /// </summary>
        public ErrorDto AlertasTipos_Actualizar(int codCliente, AlertasTiposDto alertatipo)
        {
            var resp = new ErrorDto();

            const string updateSql = @"
                UPDATE PRES_TIPOS_DESVIACIONES
                SET 
                    DESCRIPCION = @Descripcion,
                    ACTIVA = @Activa,
                    REQUIERE_JUSTIFICACION = @RequiereJustificacion,
                    TIPO = @Tipo,
                    VALOR_DESVIACION = @ValorDesviacion,
                    MODIFICA_USUARIO = @ModificaUsuario,
                    MODIFICA_FECHA = @ModificaFecha
                WHERE COD_DESVIACION = @CodDesviacion;";

            try
            {
                using var connection = CreateConnection(codCliente);

                var parameters = new
                {
                    CodDesviacion = alertatipo.cod_desviacion,
                    Descripcion = alertatipo.descripcion,
                    Activa = alertatipo.activa,
                    RequiereJustificacion = alertatipo.requiere_justificacion,
                    Tipo = alertatipo.tipo,
                    ValorDesviacion = alertatipo.valor_desviacion,
                    ModificaUsuario = alertatipo.modifica_usuario,
                    ModificaFecha = DateTime.Now
                };

                int filas = connection.Execute(updateSql, parameters);
                resp.Code = filas;
                resp.Description = filas > 0 ? "OK" : "No existe el registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTipos_Actualizar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Eliminar tipo de alerta
        /// </summary>
        public ErrorDto AlertasTipos_Eliminar(int codCliente, string codDesviacion)
        {
            var resp = new ErrorDto();

            const string deleteSql = @"
                DELETE FROM PRES_TIPOS_DESVIACIONES
                WHERE COD_DESVIACION = @CodDesviacion;";

            try
            {
                using var connection = CreateConnection(codCliente);

                var parameters = new { CodDesviacion = codDesviacion };

                int filas = connection.Execute(deleteSql, parameters);
                resp.Code = filas;
                resp.Description = filas > 0 ? "OK" : "No existe el registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTipos_Eliminar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>Obtiene los tipos de justificación asociados a un tipo de alerta.</summary>
        public ErrorDto<AlertasTiposJustificacionLista> AlertasTiposJustificacion_Obtener(int codCliente, string id_justificacion, string? filtro)
        {
            var response = new ErrorDto<AlertasTiposJustificacionLista> { Result = new AlertasTiposJustificacionLista() };
            const string sql = @"
        SELECT RTRIM(COD_TP_JUSTIFICACION) AS cod_tp_justificacion,
               RTRIM(ID_JUSTIFICACION) AS id_justificacion,
               RTRIM(DESCRIPCION) AS descripcion,
               CAST(ISNULL(ACTIVA, 0) AS bit) AS activa,
               RTRIM(REGISTRA_USUARIO) AS registra_usuario,
               REGISTRA_FECHA AS registra_fecha,
               RTRIM(MODIFICA_USUARIO) AS modifica_usuario,
               MODIFICA_FECHA AS modifica_fecha
        FROM PRES_TIPOS_JUSTIFICACION
        WHERE ID_JUSTIFICACION = @id_justificacion
          AND (@filtro = '' OR COD_TP_JUSTIFICACION LIKE @likeFiltro OR DESCRIPCION LIKE @likeFiltro)
        ORDER BY COD_TP_JUSTIFICACION;";

            try
            {
                using var connection = CreateConnection(codCliente);
                var lista = connection.Query<AlertasTiposJustificacionDto>(sql, new
                {
                    id_justificacion = NormalizeCode(id_justificacion),
                    filtro = NormalizeText(filtro),
                    likeFiltro = $"%{NormalizeText(filtro)}%"
                }).ToList();

                response.Result.lista = lista;
                response.Result.total = lista.Count;
                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "AlertasTiposJustificacion_Obtener: " + ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>Guarda o actualiza un tipo de justificación de alerta.</summary>
        public ErrorDto AlertasTiposJustificacion_Guardar(int codCliente, AlertasTiposJustificacionDto request)
        {
            var resp = new ErrorDto();
            var idJustificacion = NormalizeCode(request.id_justificacion);
            var codJustificacion = NormalizeCode(request.cod_tp_justificacion);
            var descripcion = NormalizeText(request.descripcion);
            var usuario = NormalizeCode(string.IsNullOrWhiteSpace(request.modifica_usuario) ? request.registra_usuario : request.modifica_usuario);

            if (string.IsNullOrWhiteSpace(idJustificacion) || string.IsNullOrWhiteSpace(codJustificacion) || string.IsNullOrWhiteSpace(descripcion))
                return new ErrorDto { Code = -1, Description = "Debe indicar tipo de alerta, código y descripción." };

            const string sqlPadre = @"SELECT COUNT(*) FROM PRES_TIPOS_DESVIACIONES WHERE COD_DESVIACION = @id_justificacion;";
            const string sqlExiste = @"SELECT COUNT(*) FROM PRES_TIPOS_JUSTIFICACION WHERE ID_JUSTIFICACION = @id_justificacion AND COD_TP_JUSTIFICACION = @cod_tp_justificacion;";
            const string sqlInsert = @"
        INSERT INTO PRES_TIPOS_JUSTIFICACION
        (COD_TP_JUSTIFICACION, ID_JUSTIFICACION, DESCRIPCION, ACTIVA, REGISTRA_USUARIO, REGISTRA_FECHA)
        VALUES (@cod_tp_justificacion, @id_justificacion, @descripcion, @activa, @usuario, GETDATE());";
            const string sqlUpdate = @"
        UPDATE PRES_TIPOS_JUSTIFICACION
        SET DESCRIPCION = @descripcion,
            ACTIVA = @activa,
            MODIFICA_USUARIO = @usuario,
            MODIFICA_FECHA = GETDATE()
        WHERE ID_JUSTIFICACION = @id_justificacion
          AND COD_TP_JUSTIFICACION = @cod_tp_justificacion;";

            try
            {
                using var connection = CreateConnection(codCliente);

                if (connection.ExecuteScalar<int>(sqlPadre, new { id_justificacion = idJustificacion }) == 0)
                    return new ErrorDto { Code = -1, Description = $"No existe el tipo de alerta {idJustificacion}." };

                var param = new
                {
                    cod_tp_justificacion = codJustificacion,
                    id_justificacion = idJustificacion,
                    descripcion,
                    activa = request.activa ?? true,
                    usuario
                };

                var existe = connection.ExecuteScalar<int>(sqlExiste, param) > 0;
                resp.Code = connection.Execute(existe ? sqlUpdate : sqlInsert, param);
                resp.Description = resp.Code > 0 ? "OK" : "No se guardó ningún registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTiposJustificacion_Guardar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>Elimina un tipo de justificación asociado a un tipo de alerta.</summary>
        public ErrorDto AlertasTiposJustificacion_Eliminar(int codCliente, AlertasTiposJustificacionEliminarRequest request)
        {
            var resp = new ErrorDto();
            const string sql = @"
        DELETE FROM PRES_TIPOS_JUSTIFICACION
        WHERE ID_JUSTIFICACION = @id_justificacion
          AND COD_TP_JUSTIFICACION = @cod_tp_justificacion;";

            try
            {
                using var connection = CreateConnection(codCliente);
                resp.Code = connection.Execute(sql, new
                {
                    id_justificacion = NormalizeCode(request.id_justificacion),
                    cod_tp_justificacion = NormalizeCode(request.cod_tp_justificacion)
                });
                resp.Description = resp.Code > 0 ? "OK" : "No existe el registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "AlertasTiposJustificacion_Eliminar: " + ex.Message;
            }

            return resp;
        }

    }
}