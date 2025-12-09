using Dapper;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public static class DbHelper
    {
        public static ErrorDto<T> CreateOkResponse<T>(T initialResult)
        {
            return new ErrorDto<T>
            {
                Code        = 0,
                Description = "Ok",
                Result      = initialResult
            };
        }

        public static ErrorDto CreateOkResponse()
        {
            return new ErrorDto
            {
                Code        = 0,
                Description = "Ok"
            };
        }

        public static ErrorDto<List<T>> ExecuteListQuery<T>(
            PortalDB portalDb,
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse(new List<T>());

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
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

        public static ErrorDto<T> ExecuteSingleQuery<T>(
            PortalDB portalDb,
            int codEmpresa,
            string sql,
            T defaultValue,
            object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
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

        public static ErrorDto ExecuteNonQuery(
            PortalDB portalDb,
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = portalDb.CreateConnection(codEmpresa);
                connection.Execute(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}
