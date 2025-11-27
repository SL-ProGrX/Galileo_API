using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmPresAjustesTiposDb
    {
        private readonly IConfiguration _config;

        public FrmPresAjustesTiposDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(stringConn);
        }

        #endregion

        /// <summary>
        /// Obtiene los tipos de ajustes de la empresa especificada.
        /// </summary>
        public ErrorDto<PresAjustestTiposLista> PresAjustestTipos_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<PresAjustestTiposLista>
            {
                Result = new PresAjustestTiposLista()
            };

            const string sql = @"
                SELECT 
                    cod_ajuste                  AS Cod_Ajuste,
                    descripcion                 AS Descripcion,
                    ACTIVO                      AS Activo,
                    ajuste_libre_positivo       AS Ajuste_Libre_Positivo,
                    ajuste_libre_negativo       AS Ajuste_Libre_Negativo,
                    ajuste_entre_cuentas        AS Ajuste_Entre_Cuentas,
                    ajuste_cta_dif_Naturaleza   AS Ajuste_Cta_Dif_Naturaleza,
                    REGISTRO_FECHA              AS Registro_Fecha,
                    REGISTRO_USUARIO            AS Registro_Usuario
                FROM pres_tipos_ajustes
                ORDER BY cod_ajuste;";

            try
            {
                using var connection = CreateConnection(codEmpresa);
                response.Result.lista = connection.Query<PresAjustestTiposDto>(sql).ToList();
                response.Result.total = response.Result.lista.Count;
                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "PresAjustestTipos_Obtener: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Inserta un nuevo tipo de ajuste en la base de datos.
        /// </summary>
        public ErrorDto PresAjustesTipo_Insertar(int codEmpresa, PresAjustestTiposDto request)
        {
            var resp = new ErrorDto();

            const string checkSql = @"
                SELECT COUNT(1) 
                FROM pres_tipos_ajustes 
                WHERE UPPER(cod_ajuste) = UPPER(@CodAjuste);";

            const string insertSql = @"
                INSERT INTO pres_tipos_ajustes (
                    cod_ajuste,
                    descripcion,
                    ACTIVO,
                    ajuste_libre_positivo,
                    ajuste_libre_negativo,
                    ajuste_entre_cuentas,
                    ajuste_cta_dif_Naturaleza,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                VALUES (
                    @CodAjuste,
                    @Descripcion,
                    @Activo,
                    @ALP,
                    @ALN,
                    @AEC,
                    @ACDN,
                    GETDATE(),
                    @RegistroUsuario
                );";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var checkParams = new { CodAjuste = request.Cod_Ajuste };
                int exists = connection.ExecuteScalar<int>(checkSql, checkParams);

                if (exists > 0)
                {
                    resp.Code = -2;
                    resp.Description = $"Ya existe un tipo de ajuste con el código '{request.Cod_Ajuste}'.";
                    return resp;
                }

                var parameters = new DynamicParameters();
                parameters.Add("CodAjuste",        request.Cod_Ajuste,                 DbType.String);
                parameters.Add("Descripcion",      request.Descripcion,               DbType.String);
                parameters.Add("Activo",           request.Activo,                    DbType.Int32);
                parameters.Add("ALP",              request.Ajuste_Libre_Positivo,     DbType.Int32);
                parameters.Add("ALN",              request.Ajuste_Libre_Negativo,     DbType.Int32);
                parameters.Add("AEC",              request.Ajuste_Entre_Cuentas,      DbType.Int32);
                parameters.Add("ACDN",             request.Ajuste_Cta_Dif_Naturaleza, DbType.Int32);
                parameters.Add("RegistroUsuario",  request.Registro_Usuario,          DbType.String);

                resp.Code = connection.Execute(insertSql, parameters);
                resp.Description = resp.Code > 0 ? "OK" : "No se insertó ningún registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "PresAjustesTipo_Insertar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza un tipo de ajuste existente en la base de datos.
        /// </summary>
        public ErrorDto PresAjustesTipo_Actualizar(int codEmpresa, PresAjustestTiposDto request)
        {
            var resp = new ErrorDto();

            const string updateSql = @"
                UPDATE pres_tipos_ajustes
                SET 
                    DESCRIPCION             = @Descripcion,
                    ACTIVO                  = @Activo,
                    AJUSTE_LIBRE_POSITIVO   = @ALP,
                    AJUSTE_LIBRE_NEGATIVO   = @ALN,
                    AJUSTE_ENTRE_CUENTAS    = @AEC,
                    AJUSTE_CTA_DIF_NATURALEZA = @ACDN,
                    REGISTRO_USUARIO        = @RegistroUsuario,
                    REGISTRO_FECHA          = GETDATE()
                WHERE COD_AJUSTE = @CodAjuste;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("CodAjuste",       request.Cod_Ajuste,                 DbType.String);
                parameters.Add("Descripcion",     request.Descripcion,               DbType.String);
                parameters.Add("Activo",          request.Activo,                    DbType.Int32);
                parameters.Add("ALP",             request.Ajuste_Libre_Positivo,     DbType.Int32);
                parameters.Add("ALN",             request.Ajuste_Libre_Negativo,     DbType.Int32);
                parameters.Add("AEC",             request.Ajuste_Entre_Cuentas,      DbType.Int32);
                parameters.Add("ACDN",            request.Ajuste_Cta_Dif_Naturaleza, DbType.Int32);
                parameters.Add("RegistroUsuario", request.Registro_Usuario,          DbType.String);

                int filas = connection.Execute(updateSql, parameters);
                resp.Code = filas;
                resp.Description = filas > 0 ? "OK" : "No existe el registro.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "PresAjustesTipo_Actualizar: " + ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Elimina un tipo de ajuste de la base de datos.
        /// </summary>
        public ErrorDto PresAjustesTipo_Eliminar(int codEmpresa, string codAjuste)
        {
            var resp = new ErrorDto();

            const string sql = @"
                DELETE FROM pres_tipos_ajustes 
                WHERE cod_ajuste = @CodAjuste;";

            try
            {
                using var connection = CreateConnection(codEmpresa);
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("CodAjuste", codAjuste, DbType.String);

                int filas = connection.Execute(sql, parameters);

                resp.Code = filas; // 1 si borró, 0 si no encontró
                resp.Description = filas > 0 ? "OK" : "No existe el registro.";
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                // Violación de clave foránea
                resp.Code = -1;
                resp.Description = "No se puede eliminar el tipo de ajuste porque está siendo usado por otros registros.";
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                // Deadlock
                resp.Code = -1;
                resp.Description = "La operación fue bloqueada por otra transacción. Inténtalo nuevamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "PresAjustesTipo_Eliminar: " + ex.Message;
            }

            return resp;
        }

    }
}
