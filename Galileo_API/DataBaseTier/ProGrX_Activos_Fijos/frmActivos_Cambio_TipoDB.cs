using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosCambioTipoDb
    {
        private readonly PortalDB _portalDB;

        public FrmActivosCambioTipoDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Helpers genéricos para reducir duplicación
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialResult"></param>
        /// <returns></returns>
        private static ErrorDto<T> CreateOkResponse<T>(T initialResult)
        {
            return new ErrorDto<T>
            {
                Code        = 0,
                Description = "Ok",
                Result      = initialResult
            };
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que retornan listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<List<T>> ExecuteListQuery<T>(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = null;
            }

            return result;
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que retornan un solo registro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="defaultValue"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<T> ExecuteSingleQuery<T>(
            int codEmpresa,
            string sql,
            T defaultValue,
            object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = defaultValue;
            }

            return result;
        }


        /// <summary>
        /// Obtiene listas genéricas de tipos de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(tipo_activo) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM   Activos_tipo_activo
                ORDER BY tipo_activo";

            return ExecuteListQuery<DropDownListaGenericaModel>(CodEmpresa, sql);
        }


        /// <summary>
        /// Consulta los datos principales de un activo por su placa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPrincipalesData> Activos_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            const string sql = @"
                SELECT  A.Num_Placa,
                        A.Nombre,
                        A.vida_util_en,
                        A.vida_util,
                        A.met_depreciacion,
                        A.tipo_activo,
                        T.descripcion AS Tipo_Activo_Desc
                FROM    Activos_Principal A
                        INNER JOIN Activos_tipo_activo T
                            ON A.tipo_activo = T.tipo_activo
                WHERE   A.num_placa = @placa";

            return ExecuteSingleQuery(
                CodEmpresa,
                sql,
                new ActivosPrincipalesData(),
                new { placa });
        }


        /// <summary>
        /// Obtiene la lista de activos.
        /// </summary>
        public ErrorDto<List<ActivosData>> Activos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT num_placa,
                       Placa_Alterna,
                       Nombre
                FROM   Activos_Principal
                WHERE  estado = 'A'";

            return ExecuteListQuery<ActivosData>(CodEmpresa, sql);
        }
    }
}