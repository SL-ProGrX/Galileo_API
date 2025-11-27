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
    }
}