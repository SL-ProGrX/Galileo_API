using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprRechazoMotivosDB
    {
        private readonly PortalDB _portalDB;

        // Evitar literales repetidos (Sonar)
        private const string DefaultSortField = "COD_RECHAZO";
        private const string SortFieldDescripcion = "DESCRIPCION";
        private const string SortFieldActivo = "ACTIVO";
        private const string PaginationClause = " OFFSET @off ROWS FETCH NEXT @take ROWS ONLY ";

        public FrmCprRechazoMotivosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        // Helper: usa DbHelper.WithConn y devuelve ErrorDto plano (sin ErrorDto<ErrorDto>)
        private ErrorDto WithConn(int codEmpresa, Func<SqlConnection, ErrorDto> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? (r.Result ?? DbHelper.ErrorResponse("Error desconocido.", -1))
                : DbHelper.ErrorResponse(r.Description ?? "Error desconocido.", -1);
        }

        // Helper: para resultados tipados (la acción devuelve T, NO ErrorDto<T>)
        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> action)
        {
            var r = DbHelper.WithConn(_portalDB, codEmpresa, action);
            return r.Code == 0
                ? new ErrorDto<T> { Code = 0, Description = "Ok", Result = r.Result }
                : new ErrorDto<T> { Code = -1, Description = r.Description, Result = default! };
        }

        private static FiltrosLazyLoadData ParseFiltrosLazy(string vFiltros)
        {
            try
            {
                return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(vFiltros) ?? new FiltrosLazyLoadData();
            }
            catch
            {
                return new FiltrosLazyLoadData();
            }
        }

        private static DynamicParameters BuildListadoParams(FiltrosLazyLoadData filtro)
        {
            var dp = new DynamicParameters();

            dp.Add("q", BuildSearchLike(filtro.filtro));

            var (off, take) = NormalizePaging(filtro.pagina, filtro.paginacion);
            dp.Add("off", off);
            dp.Add("take", take);

            return dp;
        }

        private static string? BuildSearchLike(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            return $"%{raw.Trim()}%";
        }

        private static (int off, int take) NormalizePaging(int pagina, int paginacion)
        {
            var off = pagina < 0 ? 0 : pagina;
            var take = paginacion <= 0 ? 10 : paginacion;
            return (off, take);
        }

        private static string BuildListadoOrderBy(FiltrosLazyLoadData filtro)
        {
            var sortField = NormalizeSortField(filtro.sortField);
            var sortDir = (filtro.sortOrder == 0) ? "DESC" : "ASC";

            // ORDER BY solo con literales (whitelist)
            return (sortField, sortDir) switch
            {
                (SortFieldDescripcion, "ASC") => $"ORDER BY {SortFieldDescripcion} ASC",
                (SortFieldDescripcion, "DESC") => $"ORDER BY {SortFieldDescripcion} DESC",
                (SortFieldActivo, "ASC") => $"ORDER BY {SortFieldActivo} ASC",
                (SortFieldActivo, "DESC") => $"ORDER BY {SortFieldActivo} DESC",
                (DefaultSortField, "ASC") => "ORDER BY COD_RECHAZO ASC",
                _ => "ORDER BY COD_RECHAZO DESC"
            };
        }

        private static string NormalizeSortField(string? sortField)
        {
            var sf = string.IsNullOrWhiteSpace(sortField) ? DefaultSortField : sortField.Trim();

            return sf.ToUpperInvariant() switch
            {
                DefaultSortField => DefaultSortField,
                SortFieldDescripcion => SortFieldDescripcion,
                SortFieldActivo => SortFieldActivo,
                _ => DefaultSortField
            };
        }

        private static int ObtenerTotalListado(SqlConnection conn, DynamicParameters dp)
        {
            const string countSql = @"SELECT COUNT(COD_RECHAZO)
                                      FROM CPR_RECHAZO_TIPOS
                                      WHERE (@q IS NULL OR COD_RECHAZO LIKE @q OR DESCRIPCION LIKE @q);";

            return conn.ExecuteScalar<int>(countSql, dp);
        }

        private static List<CprRechazosMotivosDto> ObtenerListado(SqlConnection conn, DynamicParameters dp, string orderBy)
        {
            var dataSql = $@"SELECT COD_RECHAZO, DESCRIPCION, ACTIVO
                             FROM CPR_RECHAZO_TIPOS
                             WHERE (@q IS NULL OR COD_RECHAZO LIKE @q OR DESCRIPCION LIKE @q)
                             {orderBy}
                             {PaginationClause};";

            return conn.Query<CprRechazosMotivosDto>(dataSql, dp).ToList();
        }

        private static ErrorDto<CprRechazosMotivosLista> NormalizeListadoResponse(ErrorDto<CprRechazosMotivosLista> r)
        {
            if (r.Code != 0)
                return ErrorListado(r.Description);

            if (r.Result == null)
                return OkListado(new CprRechazosMotivosLista { total = 0, lista = new List<CprRechazosMotivosDto>() });

            r.Result.lista ??= new List<CprRechazosMotivosDto>();
            return OkListado(r.Result);
        }

        private static ErrorDto<CprRechazosMotivosLista> OkListado(CprRechazosMotivosLista result)
        {
            return new ErrorDto<CprRechazosMotivosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = result
            };
        }

        private static ErrorDto<CprRechazosMotivosLista> ErrorListado(string? message)
        {
            return new ErrorDto<CprRechazosMotivosLista>
            {
                Code = -1,
                Description = string.IsNullOrWhiteSpace(message) ? "Error" : message,
                Result = new CprRechazosMotivosLista { total = 0, lista = new List<CprRechazosMotivosDto>() }
            };
        }

        /// <summary>
        /// Obtiene la lista de motivos de rechazo
        /// </summary>
        public ErrorDto<CprRechazosMotivosLista> CprRechazoMotivoLista_Obtener(int CodCliente, string vFiltros)
        {
            var filtro = ParseFiltrosLazy(vFiltros);

            try
            {
                var r = WithConn(CodCliente, conn =>
                {
                    var dp = BuildListadoParams(filtro);
                    var orderBy = BuildListadoOrderBy(filtro);

                    var total = ObtenerTotalListado(conn, dp);
                    var lista = ObtenerListado(conn, dp, orderBy);

                    return new CprRechazosMotivosLista
                    {
                        total = total,
                        lista = lista
                    };
                });

                return NormalizeListadoResponse(r);
            }
            catch (Exception ex)
            {
                return ErrorListado(ex.Message);
            }
        }

        /// <summary>
        /// Guarda un motivo de rechazo (insert/update según isNew)
        /// </summary>
        public ErrorDto CprRechazoMotivo_Guardar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            try
            {
                if (motivo == null)
                    return DbHelper.ErrorResponse("Motivo inválido.", -1);

                var cod = (motivo.cod_rechazo ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("El código del motivo es requerido.", -1);

                return WithConn(CodCliente, conn =>
                {
                    const string existsSql = @"
                        SELECT COUNT(COD_RECHAZO)
                        FROM CPR_RECHAZO_TIPOS
                        WHERE UPPER(COD_RECHAZO) = @cod;";

                    var count = conn.ExecuteScalar<int>(existsSql, new { cod = cod.ToUpperInvariant() });

                    if (motivo.isNew)
                    {
                        if (count > 0)
                            return DbHelper.ErrorResponse($"El motivo de rechazo con el código {cod} ya existe.", -2);

                        return Insertar_Internal(conn, motivo);
                    }

                    // update
                    if (count == 0)
                        return DbHelper.ErrorResponse($"El motivo de rechazo con el código {cod} no existe.", -3);

                    return Actualizar_Internal(conn, motivo);
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static ErrorDto Insertar_Internal(SqlConnection conn, CprRechazosMotivosDto motivo)
        {
            var activo = motivo.activo ? 1 : 0;

            const string sql = @"
                INSERT INTO CPR_RECHAZO_TIPOS
                (
                    COD_RECHAZO,
                    DESCRIPCION,
                    ACTIVO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                VALUES
                (
                    @cod_rechazo,
                    @descripcion,
                    @activo,
                    GETDATE(),
                    @registro_usuario
                );";

            conn.Execute(sql, new
            {
                cod_rechazo = (motivo.cod_rechazo ?? string.Empty).Trim(),
                descripcion = (motivo.descripcion ?? string.Empty).Trim(),
                activo,
                registro_usuario = (motivo.modifica_usuario ?? motivo.registro_usuario ?? string.Empty).Trim()
            });

            return DbHelper.OkResponse("Motivo agregado correctamente");
        }

        private static ErrorDto Actualizar_Internal(SqlConnection conn, CprRechazosMotivosDto motivo)
        {
            var activo = motivo.activo ? 1 : 0;

            const string sql = @"
                UPDATE CPR_RECHAZO_TIPOS
                   SET DESCRIPCION = @descripcion,
                       ACTIVO = @activo,
                       MODIFICA_FECHA = GETDATE(),
                       MODIFICA_USUARIO = @modifica_usuario
                 WHERE COD_RECHAZO = @cod_rechazo;";

            conn.Execute(sql, new
            {
                cod_rechazo = (motivo.cod_rechazo ?? string.Empty).Trim(),
                descripcion = (motivo.descripcion ?? string.Empty).Trim(),
                activo,
                modifica_usuario = (motivo.modifica_usuario ?? string.Empty).Trim()
            });

            return DbHelper.OkResponse("Motivo actualizado correctamente");
        }

        /// <summary>
        /// Elimina un motivo de rechazo
        /// </summary>
        public ErrorDto cprRechazoMotivo_Eliminar(int CodCliente, string cod_rechazo)
        {
            try
            {
                var cod = (cod_rechazo ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("Código de rechazo inválido.", -1);

                return WithConn(CodCliente, conn =>
                {
                    const string sql = @"DELETE FROM CPR_RECHAZO_TIPOS WHERE COD_RECHAZO = @cod;";
                    conn.Execute(sql, new { cod });
                    return DbHelper.OkResponse("Motivo eliminado correctamente");
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }
    }
}