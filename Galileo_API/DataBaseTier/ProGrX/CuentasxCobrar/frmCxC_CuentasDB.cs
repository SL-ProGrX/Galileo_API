using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public partial class FrmCxcCuentasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;

        public FrmCxcCuentasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la fecha del servidor.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <returns>Fecha actual del servidor.</returns>
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _proGrxMain.fxFechaServidor(codEmpresa, 0);
        }

        public ErrorDto<string> fxCxC_Parametro(int codEmpresa, string codParametro)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            return MCxCDb.fxCxC_Parametro(conn, codEmpresa, codParametro);
        }

        public ErrorDto<object> EjecutaConsultas(
           int codEmpresa,
           string mensajeGeneral,
           Func<SqlConnection, ErrorDto<object>> accion)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                return accion(conn);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>($"{mensajeGeneral} {ex.Message}");
            }
        }


        /// <summary>
        /// Obtiene la lista lazy de operaciones de CxC para búsquedas.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="filtros">Filtros lazy load del buscador.</param>
        /// <param name="esExportar">Indica si la consulta se ejecuta sin paginación.</param>
        /// <returns>Lista paginada de operaciones y total filtrado.</returns>
        public ErrorDto<CxCCuentasBusquedaOperacionLista> CxCCuentasBusquedaOperacionLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var result = new ErrorDto<CxCCuentasBusquedaOperacionLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasBusquedaOperacionLista
                {
                    total = 0,
                    lista = new List<CxCCuentasBusquedaOperacionItem>()
                }
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var texto = filtros.filtro?.Trim();
                var tieneFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = tieneFiltro ? $"%{texto}%" : null;

                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var orderByField = sortField switch
                {
                    "operacion" => "Operacion",
                    "cedula" => "Cedula",
                    "num_documento" => "NUM_DOCUMENTO",
                    "cod_concepto" => "COD_CONCEPTO",
                    "cod_oficina" => "COD_OFICINA",
                    _ => "Operacion"
                };

                var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

                const string where = @"
                    WHERE
                        (@filtro IS NULL)
                        OR (CAST(Operacion AS NVARCHAR(30)) LIKE @like)
                        OR (ISNULL(Cedula, '') LIKE @like)
                        OR (ISNULL(NUM_DOCUMENTO, '') LIKE @like)
                        OR (ISNULL(COD_CONCEPTO, '') LIKE @like)
                        OR (ISNULL(COD_OFICINA, '') LIKE @like)";

                var sqlCount = $@"
                    SELECT COUNT(1)
                    FROM CxC_Cuentas
                     {where}";

                var sqlLista = $@"
                    SELECT
                        Operacion AS operacion,
                        ISNULL(Cedula, '') AS cedula,
                        ISNULL(NUM_DOCUMENTO, '') AS num_documento,
                        ISNULL(COD_CONCEPTO, '') AS cod_concepto,
                        ISNULL(COD_OFICINA, '') AS cod_oficina
                    FROM CxC_Cuentas
                    {where}
                    ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlLista += CxCCuentasConstantes.paginacionSql;
                }

                var parametros = new
                {
                    filtro = tieneFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                };

                result.Result.total = conn.Query<int>(sqlCount, parametros).FirstOrDefault();
                result.Result.lista = conn.Query<CxCCuentasBusquedaOperacionItem>(sqlLista, parametros).ToList();
            }
            catch (DbException)
            {
                result.Code = -1;
                result.Description = "No fue posible consultar las operaciones de CxC.";
                result.Result.total = 0;
                result.Result.lista = new List<CxCCuentasBusquedaOperacionItem>();
            }
            catch (Exception)
            {
                result.Code = -1;
                result.Description = "Error inesperado al consultar las operaciones de CxC.";
                result.Result.total = 0;
                result.Result.lista = new List<CxCCuentasBusquedaOperacionItem>();
            }

            return result;
        }

        /// <summary>
        /// Obtiene una operación de CxC por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <returns>Datos básicos de la operación.</returns>
        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacion_Obtener(int codEmpresa, long operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>(CxCCuentasConstantes.operacionRequerida);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT TOP 1
                        Operacion AS operacion,
                        ISNULL(Cedula, '') AS cedula,
                        ISNULL(NUM_DOCUMENTO, '') AS num_documento,
                        ISNULL(COD_CONCEPTO, '') AS cod_concepto,
                        ISNULL(COD_OFICINA, '') AS cod_oficina
                    FROM dbo.CxC_Cuentas
                    WHERE Operacion = @operacion;";

                var item = conn.QueryFirstOrDefault<CxCCuentasBusquedaOperacionItem>(sql, new { operacion });

                if (item is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("No se encontró la operación.");
                }

                return DbHelper.CreateOkResponse(item);
            }
            catch (DbException)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("No fue posible consultar la operación.");
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("Error inesperado al consultar la operación.");
            }
        }

        /// <summary>
        /// Obtiene la operación anterior o siguiente a partir de una operación actual.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="operacion">Operación actual.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Datos básicos de la operación encontrada.</returns>
        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacionScroll_Obtener(int codEmpresa, long operacion, int tipo)
        {

            if (tipo is not (0 or 1))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>(CxCCuentasConstantes.scrollValido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var sql = tipo == 1
                    ? @"
                        SELECT TOP 1
                            Operacion AS operacion,
                            ISNULL(Cedula, '') AS cedula,
                            ISNULL(NUM_DOCUMENTO, '') AS num_documento,
                            ISNULL(COD_CONCEPTO, '') AS cod_concepto,
                            ISNULL(COD_OFICINA, '') AS cod_oficina
                        FROM dbo.CxC_Cuentas
                        WHERE Operacion < @operacion
                        ORDER BY Operacion DESC;"
                    : @"
                        SELECT TOP 1
                            Operacion AS operacion,
                            ISNULL(Cedula, '') AS cedula,
                            ISNULL(NUM_DOCUMENTO, '') AS num_documento,
                            ISNULL(COD_CONCEPTO, '') AS cod_concepto,
                            ISNULL(COD_OFICINA, '') AS cod_oficina
                        FROM dbo.CxC_Cuentas
                        WHERE Operacion > @operacion
                        ORDER BY Operacion ASC;";

                var item = conn.QueryFirstOrDefault<CxCCuentasBusquedaOperacionItem>(sql, new { operacion });

                if (item is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("No hay más operaciones para navegar.");
                }

                return DbHelper.CreateOkResponse(item);
            }
            catch (DbException)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("No fue posible navegar entre operaciones.");
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("Error inesperado al navegar entre operaciones.");
            }
        }

        /// <summary>
        /// Obtiene las divisas para el registro de facturas.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista de divisas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Divisas_Obtener(int codEmpresa, int codContabilidad)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT
                        RTRIM(cod_divisa) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM CntX_Divisas
                    WHERE cod_contabilidad = @codContabilidad
                    ORDER BY cod_divisa;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new { codContabilidad }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"No fue posible consultar las divisas. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"Error inesperado al consultar las divisas. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene las oficinas activas.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <returns>Lista de oficinas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Oficinas_Obtener(int codEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT
                        RTRIM(cod_oficina) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM SIF_Oficinas
                    WHERE estado = 1
                    ORDER BY cod_oficina;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"No fue posible consultar las oficinas. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    $"Error inesperado al consultar las oficinas. {ex.Message}",
                    result: new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene los bancos autorizados para CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <returns>Lista de bancos autorizados.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Bancos_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                const string sql = @"exec spCxC_Bancos_Autorizados;";
                var datos = conn.Query<BancoAutorizadoComboDto>(sql).ToList();

                foreach (var item in datos)
                {
                    response.Result.Add(new DropDownListaGenericaModel
                    {
                        item = item.IdX.ToString(),
                        descripcion = item.ItmX ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }

            return response;
        }

        /// <summary>
        /// Consulta una operación de CxC para cargar el formulario principal.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <returns>Datos completos de la operación.</returns>
        public ErrorDto<CxCCuentasConsultaData> CxCCuentas_Consulta_Obtener(int codEmpresa, long operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConsultaData>(CxCCuentasConstantes.operacionRequerida);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                            SELECT *
                            FROM vCxC_Cuentas_Consulta
                            WHERE Operacion = @operacion;";

                var data = conn.QueryFirstOrDefault<CxCCuentasConsultaData>(sql, new { operacion });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasConsultaData>("No se encontró la operación.");
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConsultaData>($"No fue posible consultar la operación. {ex.Message}");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConsultaData>($"Error inesperado al consultar la operación. {ex.Message}");
            }
        }

 
        public static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        public static string NormalizarMayusculas(string? valor)
        {
            return NormalizarTexto(valor).ToUpperInvariant();
        }

        private static bool EsTipoScrollValido(int tipo)
        {
            return tipo is 0 or 1;
        }

        private static DynamicParameters CrearParametrosLazy(FiltrosLazyLoadData filtros, object? parametrosAdicionales = null)
        {
            filtros ??= new FiltrosLazyLoadData();

            var texto = NormalizarTexto(filtros.filtro);
            var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";

            var parametros = new DynamicParameters(parametrosAdicionales);
            parametros.Add("filtro", string.IsNullOrWhiteSpace(texto) ? null : texto);
            parametros.Add("like", like);
            parametros.Add("offset", filtros.pagina);
            parametros.Add("fetch", filtros.paginacion);

            return parametros;
        }

        private ErrorDto<TItem> EjecutarConsultaUnica<TItem>(
            int codEmpresa,
            string sql,
            object parametros,
            string mensajeNoEncontrado,
            string mensajeDb,
            string mensajeGeneral)
            where TItem : class
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var data = conn.QueryFirstOrDefault<TItem>(sql, parametros);

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<TItem>(mensajeNoEncontrado);
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<TItem>($"{mensajeDb} {ex.Message}");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TItem>($"{mensajeGeneral} {ex.Message}");
            }
        }

        public ErrorDto<TItem> EjecutarConsultaScroll<TItem>(
            EjecutarConsultaScrollRequest request)
            where TItem : class
        {
            if (!EsTipoScrollValido(request.tipo))
            {
                return DbHelper.CreateErrorResponse<TItem>(CxCCuentasConstantes.scrollValido);
            }

            var sql = request.tipo == 1 ? request.sqlAnterior : request.sqlSiguiente;

            return EjecutarConsultaUnica<TItem>(
                request.codEmpresa,
                sql,
                request.parametros!,
                request.mensajeNoEncontrado,
                request.mensajeDb,
                request.mensajeGeneral);
        }

        public ErrorDto<CxCCuentasBusquedaGenericaLista<TItem>> EjecutarListaLazy<TItem>(
          EjecutarListaLazyLoadRequest request )
        {
            request.filtros ??= new FiltrosLazyLoadData();

            var response = new ErrorDto<CxCCuentasBusquedaGenericaLista<TItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasBusquedaGenericaLista<TItem>()
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, request.codEmpresa);

                var parametros = CrearParametrosLazy(request.filtros, request.parametrosAdicionales);
                var usarPaginacion = request.filtros.paginacion > 0 && !request.esExportar;
                var sqlListaFinal = usarPaginacion
                    ? $"{request.sqlLista}{CxCCuentasConstantes.paginacionSql}"
                    : request.sqlLista;

                response.Result.total = conn.QuerySingle<int>(request.sqlCount, parametros);
                response.Result.lista = conn.Query<TItem>(sqlListaFinal, parametros).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"{request.mensajeGeneral} {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<TItem>();
            }

            return response;
        }   
    }
}
