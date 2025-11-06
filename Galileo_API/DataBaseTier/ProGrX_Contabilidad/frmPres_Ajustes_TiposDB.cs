using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRES;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_Ajustes_TiposDB
    {
        private readonly IConfiguration _config;

        public frmPres_Ajustes_TiposDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los tipos de ajustes de la empresa especificada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<PresAjustestTiposLista> PresAjustestTipos_Obtener(int CodEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<PresAjustestTiposLista>();
            response.Result = new PresAjustestTiposLista();
            response.Result.total = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select * from pres_tipos_ajustes order by cod_ajuste";

                    response.Result.lista = connection.Query<PresAjustestTiposDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Inserta un nuevo tipo de ajuste en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PresAjustesTipo_Insertar(int CodEmpresa, PresAjustestTiposDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);

                // Verificar si el código ya existe
                var checkQuery = "SELECT COUNT(1) FROM pres_tipos_ajustes WHERE UPPER(cod_ajuste) = UPPER( @CodAjuste )";
                int exists = connection.ExecuteScalarAsync<int>(checkQuery, new { CodAjuste = request.Cod_Ajuste }).Result;

                if (exists > 0)
                {
                    resp.Code = -2;
                    resp.Description = $"Ya existe un tipo de ajuste con el código '{request.Cod_Ajuste}'.";
                    return resp;
                }

                var query = @"
            INSERT INTO pres_tipos_ajustes (
                cod_ajuste, descripcion, ACTIVO, ajuste_libre_positivo, 
                ajuste_libre_negativo, ajuste_entre_cuentas, 
                ajuste_cta_dif_Naturaleza, REGISTRO_FECHA, REGISTRO_USUARIO
            ) VALUES (
                @CodAjuste, @Descripcion, @Activo, @ALP, @ALN, @AEC, @ACDN, 
                GETDATE(), @RegistroUsuario
            )";

                var parameters = new DynamicParameters();
                parameters.Add("CodAjuste", request.Cod_Ajuste, DbType.String);
                parameters.Add("Descripcion", request.Descripcion, DbType.String);
                parameters.Add("Activo", request.Activo, DbType.Int32);
                parameters.Add("ALP", request.Ajuste_Libre_Positivo, DbType.Int32);
                parameters.Add("ALN", request.Ajuste_Libre_Negativo, DbType.Int32);
                parameters.Add("AEC", request.Ajuste_Entre_Cuentas, DbType.Int32);
                parameters.Add("ACDN", request.Ajuste_Cta_Dif_Naturaleza, DbType.Int32);
                parameters.Add("RegistroUsuario", request.Registro_Usuario, DbType.String);

                resp.Code = connection.ExecuteAsync(query, parameters).Result;
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
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PresAjustesTipo_Actualizar(int CodEmpresa, PresAjustestTiposDto request)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "update pres_tipos_ajustes set DESCRIPCION = @Descripcion , ACTIVO = @Activo , AJUSTE_LIBRE_POSITIVO = @ALP , " +
                        "AJUSTE_LIBRE_NEGATIVO = @ALN , AJUSTE_ENTRE_CUENTAS = @AEC , AJUSTE_CTA_DIF_NATURALEZA = @ACDN where COD_AJUSTE = @CodAjuste";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodAjuste", request.Cod_Ajuste, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Int32);
                    parameters.Add("ALP", request.Ajuste_Libre_Positivo, DbType.Int32);
                    parameters.Add("ALN", request.Ajuste_Libre_Negativo, DbType.Int32);
                    parameters.Add("AEC", request.Ajuste_Entre_Cuentas, DbType.Int32);
                    parameters.Add("ACDN", request.Ajuste_Cta_Dif_Naturaleza, DbType.Int32);
                    parameters.Add("RegistroUsuario", request.Registro_Usuario, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                }
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
        /// <param name="CodEmpresa"></param>
        /// <param name="CodAjuste"></param>
        /// <returns></returns>
        public ErrorDto PresAjustesTipo_Eliminar(int CodEmpresa, string CodAjuste)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pres_tipos_ajustes where cod_ajuste = @CodAjuste";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodAjuste", CodAjuste, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                }
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
