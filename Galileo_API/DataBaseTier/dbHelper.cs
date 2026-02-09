using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier
{
    public static class DbHelper
    {
        public static ErrorDto<T> CreateOkResponse<T>(T initialResult = default!)
            => new() { Code = 0, Description = "Ok", Result = initialResult };

        public static ErrorDto CreateOkResponse()
            => new() { Code = 0, Description = "OK" };

        public static ErrorDto<List<T>> ExecuteListQuery<T>(PortalDB portalDb, int codEmpresa, string sql, object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        public static ErrorDto<T?> ExecuteSingleQuery<T>(PortalDB portalDb, int codEmpresa, string sql, T? defaultValue = default, object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
                result.Result = connection.QueryFirstOrDefault<T>(sql, parameters);
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
                using var connection = new SqlConnection(connectionString);
                result.Result = connection.QueryFirstOrDefault<T>(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = defaultValue;
            }

            return result;
        }

        public static ErrorDto ExecuteNonQuery(PortalDB portalDb, int codEmpresa, string sql, object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
                connection.Execute(sql, parameters);
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
                using var connection = new SqlConnection(connectionString);
                connection.Execute(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    
        public static ErrorDto<int> ExecuteNonQueryWithResult(PortalDB portalDb, int codEmpresa, string sql, object? parameters = null)
        {
            var result = CreateOkResponse(0);

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
                result.Result = connection.Execute(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }

            return result;
        }

        public static ErrorDto<T> WithConn<T>(PortalDB portalDb,int codEmpresa, Func<SqlConnection, T> action)
        {
            try
            {
                using var conn = portalDb.CreateConnection(codEmpresa);
                var result = action(conn);
                return new ErrorDto<T> { Code = 0, Description = "Ok", Result = result };
            }
            catch (Exception ex)
            {
                return new ErrorDto<T> { Code = -1, Description = ex.Message, Result = default };
            }
        }
        
        public static ErrorDto<T> CreateErrorResponse<T>(string msg, int code = -1, T result = default!) =>
            new ErrorDto<T> { Code = code, Description = msg, Result = result };

        public static ErrorDto OkResponse(string msg) =>
           new ErrorDto { Code = 0, Description = msg };

        public static ErrorDto ErrorResponse(string msg, int code = -1) =>
            new ErrorDto { Code = code, Description = msg };

        public static SqlConnection OpenConnection(PortalDB portalDb,int codEmpresa)
        {
            var cs = portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(cs);
        }

        // ====== Helpers de TES_SolicitudesPendientes_Obtener ======
        public static T DeserializeOrNew<T>(string? json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json))
                return new T();

            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }

        //Helper de paginaciones


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