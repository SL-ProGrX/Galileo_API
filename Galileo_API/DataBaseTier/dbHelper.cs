using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Security;

namespace Galileo.DataBaseTier
{
    public static class DbHelper
    {

        /// <summary>
        /// Valida y normaliza el código de empresa antes de abrir conexiones a bases de datos de cliente.
        /// </summary>
        private static int NormalizarCodEmpresa(int codEmpresa)
        {
            if (codEmpresa <= 0 || codEmpresa > 999999)
            {
                throw new SecurityException("El código de empresa no es válido.");
            }

            return codEmpresa;
        }

        /// <summary>
        /// Valida que el texto SQL no esté vacío y que no contenga patrones inseguros evidentes.
        /// </summary>
        private static string ValidarSql(string sql)
        {
            var sqlSeguro = (sql ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(sqlSeguro))
            {
                throw new SecurityException("La instrucción SQL es requerida.");
            }

            if (sqlSeguro.Contains('\0'))
            {
                throw new SecurityException("La instrucción SQL no es válida.");
            }

            return sqlSeguro;
        }

        public static ErrorDto<T> CreateOkResponse<T>(T initialResult = default, string description = "Ok")
            => new() { Code = 0, Description = description, Result = initialResult };

        public static ErrorDto CreateOkResponse()
            => new() { Code = 0, Description = "OK" };

        /// <summary>
        /// Ejecuta una consulta que retorna una lista usando una empresa previamente validada.
        /// </summary>
        public static ErrorDto<List<T>> ExecuteListQuery<T>(PortalDB portalDb, int codEmpresa, string sql, object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                var codEmpresaSeguro = NormalizarCodEmpresa(codEmpresa);
                var sqlSeguro = ValidarSql(sql);

                using var connection = portalDb.CreateConnection(codEmpresaSeguro);
                result.Result = connection.Query<T>(sqlSeguro, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una consulta que retorna un único registro usando una empresa previamente validada.
        /// </summary>
        public static ErrorDto<T?> ExecuteSingleQuery<T>(PortalDB portalDb, int codEmpresa, string sql, T? defaultValue = default, object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                var codEmpresaSeguro = NormalizarCodEmpresa(codEmpresa);
                var sqlSeguro = ValidarSql(sql);

                using var connection = portalDb.CreateConnection(codEmpresaSeguro);
                result.Result = connection.QueryFirstOrDefault<T>(sqlSeguro, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = defaultValue;
            }

            return result;
        }

        public static ErrorDto<T?> ExecuteSingleQuery<T>(string connectionString, string sql, T? defaultValue = default, object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                var sqlSeguro = ValidarSql(sql);

                using var connection = new SqlConnection(connectionString);
                result.Result = connection.QueryFirstOrDefault<T>(sqlSeguro, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = defaultValue;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción sin retorno usando una empresa previamente validada.
        /// </summary>
        public static ErrorDto ExecuteNonQuery(PortalDB portalDb, int codEmpresa, string sql, object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                var codEmpresaSeguro = NormalizarCodEmpresa(codEmpresa);
                var sqlSeguro = ValidarSql(sql);

                using var connection = portalDb.CreateConnection(codEmpresaSeguro);
                connection.Execute(sqlSeguro, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        public static ErrorDto ExecuteNonQuery(string connectionString, string sql, object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                var sqlSeguro = ValidarSql(sql);

                using var connection = new SqlConnection(connectionString);
                connection.Execute(sqlSeguro, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una instrucción y retorna la cantidad de filas afectadas usando una empresa previamente validada.
        /// </summary>
        public static ErrorDto<int> ExecuteNonQueryWithResult(PortalDB portalDb, int codEmpresa, string sql, object? parameters = null)
        {
            var result = CreateOkResponse(0);

            try
            {
                var codEmpresaSeguro = NormalizarCodEmpresa(codEmpresa);
                var sqlSeguro = ValidarSql(sql);

                using var connection = portalDb.CreateConnection(codEmpresaSeguro);
                result.Result = connection.Execute(sqlSeguro, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }

            return result;
        }

        /// <summary>
        /// Ejecuta una acción con conexión abierta usando una empresa previamente validada.
        /// </summary>
        public static ErrorDto<T> WithConn<T>(PortalDB portalDb, int codEmpresa, Func<SqlConnection, T> action)
        {
            try
            {
                var codEmpresaSeguro = NormalizarCodEmpresa(codEmpresa);

                using var conn = portalDb.CreateConnection(codEmpresaSeguro);
                var result = action(conn);

                return new ErrorDto<T> { Code = 0, Description = "Ok", Result = result };
            }
            catch (Exception ex)
            {
                return new ErrorDto<T> { Code = -1, Description = ex.Message, Result = default };
            }
        }

        public static ErrorDto<T> CreateErrorResponse<T>(string msg, int code = -1, T result = default) =>
            new ErrorDto<T> { Code = code, Description = msg, Result = result };

        public static ErrorDto OkResponse(string msg) =>
           new ErrorDto { Code = 0, Description = msg };

        public static ErrorDto ErrorResponse(string msg, int code = -1) =>
            new ErrorDto { Code = code, Description = msg };

        /// <summary>
        /// Abre una conexión de cliente usando un código de empresa validado.
        /// </summary>
        public static SqlConnection OpenConnection(PortalDB portalDb, int codEmpresa)
        {
            var codEmpresaSeguro = NormalizarCodEmpresa(codEmpresa);
            var cs = portalDb.ObtenerDbConnStringEmpresa(codEmpresaSeguro);
            return new SqlConnection(cs);
        }

        public static ErrorDto<List<T>> ExecuteStoredProcedureList<T>(
        string connectionString,
        string procedureName,
        object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                var spSeguro = ValidarStoredProcedure(procedureName);

                using var connection = new SqlConnection(connectionString);
                result.Result = connection.Query<T>(
                    spSeguro,
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<T>();
            }

            return result;
        }

        public static ErrorDto<T?> ExecuteStoredProcedureSingle<T>(
            string connectionString,
            string procedureName,
            T? defaultValue = default,
            object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {

                var spSeguro = ValidarStoredProcedure(procedureName);

                using var connection = new SqlConnection(connectionString);
                result.Result = connection.QueryFirstOrDefault<T>(
                    spSeguro,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = defaultValue;
            }

            return result;
        }

        
        // ====== Helpers de TES_SolicitudesPendientes_Obtener ======
        public static T DeserializeOrNew<T>(string? json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json))
                return new T();

            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }

        //Helper de paginaciones

        /// <summary>
        /// Valida que el nombre del procedimiento almacenado sea un identificador SQL esperado.
        /// </summary>
        private static string ValidarStoredProcedure(string procedureName)
        {
            var spSeguro = (procedureName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(spSeguro))
            {
                throw new SecurityException("El nombre del procedimiento es requerido.");
            }

            if (spSeguro.Contains('\0'))
            {
                throw new SecurityException("El nombre del procedimiento no es válido.");
            }

            return spSeguro;
        }

    }

    public sealed class LazyLoadSpec
    {
        public DynamicParameters Params { get; init; } = new();
        public int Offset { get; init; }
        public int PageSize { get; init; }
        public int SortCode { get; init; }
        public bool IsAsc { get; init; }
        public bool HasFilter { get; init; }
    }

    public static class LazyLoadHelper
    {
        public static LazyLoadSpec Build(
            FiltrosLazyLoadData? filtros,
            IReadOnlyDictionary<string, int> sortMap,
            string defaultSort = "item")
        {
            filtros ??= new FiltrosLazyLoadData();

            var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
            var filtroValor = hasFilter ? $"%{filtros.filtro}%" : null;

            var sortField = (filtros.sortField ?? defaultSort).Trim();
            if (!sortMap.TryGetValue(sortField, out var sortCode))
                sortCode = sortMap[defaultSort];

            var isAsc = filtros.sortOrder != 0;

            var pageSize = Math.Max(1, filtros.paginacion);
            var offset = Math.Max(0, filtros.pagina);

            var p = new DynamicParameters();
            p.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
            p.Add("@filtro", filtroValor, DbType.String);
            p.Add("@sortCode", sortCode, DbType.Int32);
            p.Add("@isAsc", isAsc ? 1 : 0, DbType.Int32);
            p.Add("@offset", offset, DbType.Int32);
            p.Add("@pageSize", pageSize, DbType.Int32);
            p.Add("@fetch", pageSize, DbType.Int32);

            return new LazyLoadSpec
            {
                Params = p,
                Offset = offset,
                PageSize = pageSize,
                SortCode = sortCode,
                IsAsc = isAsc,
                HasFilter = hasFilter
            };
        }
    }
}