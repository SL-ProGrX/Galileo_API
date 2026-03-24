using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxcCuentasDB
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

        /// <summary>
        /// Obtiene las facturas registradas de una operación de CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <returns>Lista de facturas y totales.</returns>
        public ErrorDto<CxCCuentasFacturasLista> CxCCuentasFacturas_Obtener(int codEmpresa, long operacion)
        {
            var response = new ErrorDto<CxCCuentasFacturasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasFacturasLista()
            };

            if (operacion <= 0)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.operacionRequerida;
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"exec spCxC_Operacion_Facturas @Operacion, 0;";

                var lista = conn.Query<CxCCuentasFacturasData>(sql, new { Operacion = operacion }).ToList();

                foreach (var item in lista)
                {
                    item.adelanto_tipo_desc = (item.adelanto_tipo ?? string.Empty).Trim().ToUpperInvariant() == "P"
                        ? "Porcentual"
                        : "Monto";
                }

                response.Result.lista = lista;
                response.Result.casos = lista.Count;
                response.Result.total = lista.Sum(x => x.monto);
                response.Result.adelanto = lista.Sum(x => x.adelanto_monto);
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar las facturas de la operación. {ex.Message}";
                response.Result = new CxCCuentasFacturasLista();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar las facturas de la operación. {ex.Message}";
                response.Result = new CxCCuentasFacturasLista();
            }

            return response;
        }

        /// <summary>
        /// Obtiene las facturas adelantadas pendientes para una cédula y pagador.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="cedulaPagador">Cédula del pagador.</param>
        /// <returns>Lista de facturas adelantadas y totales.</returns>
        public ErrorDto<CxCCuentasFacturasAdelantadasLista> CxCCuentasFacturasAdelantadas_Obtener(
            int codEmpresa,
            string cedula,
            string cedulaPagador)
        {
            var response = new ErrorDto<CxCCuentasFacturasAdelantadasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasFacturasAdelantadasLista()
            };

            if (string.IsNullOrWhiteSpace(cedula))
            {
                response.Code = -1;
                response.Description = "La cédula es requerida.";
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"exec spCxC_Facturas_Adelantadas_Pendientes @Cedula, @Pagador;";

                var lista = conn.Query<CxCCuentasFacturasAdelantadasData>(sql, new
                {
                    Cedula = cedula.Trim(),
                    Pagador = (cedulaPagador ?? string.Empty).Trim()
                }).ToList();

                response.Result.lista = lista;
                response.Result.casos = lista.Count;
                response.Result.total = lista.Sum(x => x.monto);
                response.Result.adelanto = lista.Sum(x => x.adelanto_monto);
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar las facturas adelantadas. {ex.Message}";
                response.Result = new CxCCuentasFacturasAdelantadasLista();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar las facturas adelantadas. {ex.Message}";
                response.Result = new CxCCuentasFacturasAdelantadasLista();
            }

            return response;
        }


        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
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

        private ErrorDto<TItem> EjecutarConsultaScroll<TItem>(
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

        private ErrorDto<CxCCuentasBusquedaGenericaLista<TItem>> EjecutarListaLazy<TItem>(
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
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"{request.mensajeDb} {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<TItem>();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"{request.mensajeGeneral} {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<TItem>();
            }

            return response;
        }

        #region Recepcion
        /// <summary>
        /// Obtiene la lista lazy de personas para búsqueda de cédula en CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="filtros">Filtros lazy de la búsqueda.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de personas y total de registros.</returns>
        public ErrorDto<CxCCuentasPersonasFiltroLista> CxCCuentasPersonasFiltro_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "cedula" => "Cedula",
                "nombre" => "Nombre",
                "categoria" => "Categoria",
                _ => "Cedula"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string where = @"
            WHERE
                (@filtro IS NULL)
                OR (ISNULL(Cedula, '') LIKE @like)
                OR (ISNULL(Nombre, '') LIKE @like)
                OR (ISNULL(Categoria, '') LIKE @like)";

            var sqlCount = $@"
            SELECT COUNT(1)
            FROM vCxC_Personas_Filtro
            {where};";

            var sqlLista = $@"
            SELECT
                ISNULL(Cedula, '') AS cedula,
                ISNULL(Nombre, '') AS nombre,
                ISNULL(Categoria, '') AS categoria
            FROM vCxC_Personas_Filtro
            {where}
            ORDER BY {orderByField} {direction}
            ";

            var listaResponse = EjecutarListaLazy<CxCCuentasPersonasFiltroItem>(
                    new EjecutarListaLazyLoadRequest
                    {
                        codEmpresa = codEmpresa,
                        filtros = filtros,
                        esExportar = esExportar,
                        sqlCount = sqlCount,
                        sqlLista = sqlLista,
                        mensajeDb = "No fue posible consultar las personas de CxC.",
                        mensajeGeneral = "Error inesperado al consultar las personas de CxC."
                    });

            if (listaResponse.Code == -1)
            {
                return new ErrorDto<CxCCuentasPersonasFiltroLista>
                {
                    Code = -1,
                    Description = listaResponse.Description,
                    Result = new CxCCuentasPersonasFiltroLista()
                };
            }

            return DbHelper.CreateOkResponse(new CxCCuentasPersonasFiltroLista
            {
                total = listaResponse.Result?.total ?? 0,
                lista = listaResponse.Result?.lista ?? new List<CxCCuentasPersonasFiltroItem>()
            });
        }

        /// <summary>
        /// Obtiene una persona de CxC por cédula desde la vista de búsqueda.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula a consultar.</param>
        /// <returns>Registro encontrado de la vista vCxC_Personas_Filtro.</returns>
        public ErrorDto<CxCCuentasPersonasFiltroItem> CxCCuentasPersonaFiltroPorCedula_Obtener(int codEmpresa, string cedula)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPersonasFiltroItem>("La cédula es requerida.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(Cedula, '') AS cedula,
                ISNULL(Nombre, '') AS nombre,
                ISNULL(Categoria, '') AS categoria
            FROM vCxC_Personas_Filtro
            WHERE Cedula = @cedula;";

            return EjecutarConsultaUnica<CxCCuentasPersonasFiltroItem>(
                codEmpresa,
                sql,
                new { cedula = cedulaNormalizada },
                "No se encontró la cédula.",
                "No fue posible consultar la cédula.",
                "Error inesperado al consultar la cédula.");
        }

        /// <summary>
        /// Obtiene un concepto de CxC por código.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <returns>Datos del concepto.</returns>
        public ErrorDto<CxCCuentasConceptoData> CxCCuentasConcepto_Obtener(int codEmpresa, string codConcepto)
        {
            var codigoNormalizado = NormalizarTexto(codConcepto);

            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConceptoData>("El concepto es requerido.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(C.cod_Concepto, '') AS cod_concepto,
                ISNULL(C.Descripcion, '') AS descripcion,
                ISNULL(C.Requiere_Contrato, 0) AS requiere_contrato,
                ISNULL(C.Proceso_Descuento, 0) AS proceso_descuento,
                ISNULL(C.PAGADOR_DEFAULT, '') AS pagadorid,
                ISNULL(P.Nombre, '') AS pagadordesc,
                ISNULL(C.Genera_Desembolso, 0) AS genera_desembolso
            FROM CxC_Conceptos C
            LEFT JOIN CxC_Personas P
                ON C.PAGADOR_DEFAULT = P.cedula
            WHERE C.cod_Concepto = @codConcepto;";

            return EjecutarConsultaUnica<CxCCuentasConceptoData>(
                codEmpresa,
                sql,
                new { codConcepto = codigoNormalizado },
                "No se encontró el concepto.",
                "No fue posible consultar el concepto.",
                "Error inesperado al consultar el concepto.");
        }

        /// <summary>
        /// Obtiene el concepto anterior o siguiente para navegación.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codConcepto">Código actual.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Concepto encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasConceptosFiltroItem> CxCCuentasConceptoScroll_Obtener(int codEmpresa, string codConcepto, int tipo)
        {
            var codigoNormalizado = NormalizarTexto(codConcepto);

            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConceptosFiltroItem>("El concepto es requerido.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            WHERE Activo = 1
              AND cod_Concepto < @codConcepto
            ORDER BY cod_Concepto DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            WHERE Activo = 1
              AND cod_Concepto > @codConcepto
            ORDER BY cod_Concepto ASC;";

            return EjecutarConsultaScroll<CxCCuentasConceptosFiltroItem>(
                 new EjecutarConsultaScrollRequest
                 {
                     codEmpresa = codEmpresa,
                     tipo = tipo,
                     sqlAnterior = sqlAnterior,
                     sqlSiguiente = sqlSiguiente,
                     parametros = new { codConcepto = codigoNormalizado },
                     mensajeNoEncontrado = "No hay más conceptos para navegar.",
                     mensajeDb = "No fue posible navegar conceptos.",
                     mensajeGeneral = "Error inesperado al navegar conceptos."
                 });
        }

        /// <summary>
        /// Obtiene la lista lazy de conceptos activos para búsqueda.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="filtros">Filtros lazy serializados.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de conceptos.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>> CxCCuentasConceptosFiltro_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "descripcion" => "Descripcion",
                _ => "cod_Concepto"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string where = @"
            WHERE Activo = 1
              AND (
                    @filtro IS NULL
                    OR ISNULL(cod_Concepto, '') LIKE @like
                    OR ISNULL(Descripcion, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            FROM CxC_Conceptos
            {where};";

            var sqlLista = $@"
            SELECT
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            {where}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasConceptosFiltroItem>(
                 new EjecutarListaLazyLoadRequest
                 {
                     codEmpresa = codEmpresa,
                     filtros = filtros,
                     esExportar = esExportar,
                     sqlCount = sqlCount,
                     sqlLista = sqlLista,
                     mensajeDb = "No fue posible consultar conceptos.",
                     mensajeGeneral = "Error inesperado al consultar conceptos."
                 });
        }

        /// <summary>
        /// Obtiene el detalle de un contrato según la cédula y contrato seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <returns>Detalle del contrato.</returns>
        public ErrorDto<CxCCuentasContratoData> CxCCuentasContratoDetalle_Obtener(int codEmpresa, string codContrato, string cedula)
        {
            var contratoNormalizado = NormalizarTexto(codContrato);
            var cedulaNormalizada = NormalizarTexto(cedula);

            if (string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratoData>("El contrato es requerido.");
            }

            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratoData>("La cédula es requerida.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(Cnt.Cod_Contrato, '') AS cod_contrato,
                ISNULL(Cnt.Descripcion, '') AS descripcion,
                ISNULL(Cnt.PAGADORES_ABIERTO, 0) AS pagadores_abierto,
                ISNULL(Per.Tasa_Corriente, Cnt.Tasa_Corriente) AS tasa_corriente,
                ISNULL(Per.Tasa_Mora, Cnt.Tasa_Mora) AS tasa_mora,
                ISNULL(Per.Plazo, Cnt.Plazo) AS plazo
            FROM CxC_Contratos Cnt
            LEFT JOIN CxC_Personas_Contratos Per
                ON Cnt.Cod_Contrato = Per.cod_contrato
               AND Per.Activo = 1
               AND Per.Cedula = @cedula
            WHERE Cnt.cod_Contrato = @codContrato
              AND (Per.Cedula IS NOT NULL OR Cnt.Suscripcion_Abierta = 1);";

            return EjecutarConsultaUnica<CxCCuentasContratoData>(
                codEmpresa,
                sql,
                new
                {
                    codContrato = contratoNormalizado,
                    cedula = cedulaNormalizada
                },
                "No se encontró el contrato.",
                "No fue posible consultar el contrato.",
                "Error inesperado al consultar el contrato.");
        }

        /// <summary>
        /// Obtiene el contrato anterior o siguiente permitido para el cliente y concepto.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <param name="codContrato">Contrato actual.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Contrato encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasContratosFiltroItem> CxCCuentasContratoScroll_Obtener(
            int codEmpresa,
            string cedula,
            string codConcepto,
            string codContrato,
            int tipo)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);
            var conceptoNormalizado = NormalizarTexto(codConcepto);
            var contratoNormalizado = NormalizarTexto(codContrato);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(conceptoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratosFiltroItem>("La cédula y el concepto son requeridos.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(Cn.Cod_Contrato, '') AS cod_contrato,
                ISNULL(Cn.Descripcion, '') AS descripcion
            FROM CxC_Conceptos_Contratos Cnt
            INNER JOIN CxC_Contratos Cn
                ON Cnt.Cod_Contrato = Cn.cod_Contrato
            LEFT JOIN CxC_Personas_Contratos Pc
                ON Cnt.cod_Contrato = Pc.cod_Contrato
               AND Cnt.Cod_Concepto = @codConcepto
               AND Pc.Cedula = @cedula
            WHERE Cn.Activo = 1
              AND Cnt.Cod_Concepto = @codConcepto
              AND (Pc.Cedula IS NOT NULL OR Cn.Suscripcion_Abierta = 1)
              AND Cn.cod_contrato < @codContrato
            ORDER BY Cn.cod_contrato DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(Cn.Cod_Contrato, '') AS cod_contrato,
                ISNULL(Cn.Descripcion, '') AS descripcion
            FROM CxC_Conceptos_Contratos Cnt
            INNER JOIN CxC_Contratos Cn
                ON Cnt.Cod_Contrato = Cn.cod_Contrato
            LEFT JOIN CxC_Personas_Contratos Pc
                ON Cnt.cod_Contrato = Pc.cod_Contrato
               AND Cnt.Cod_Concepto = @codConcepto
               AND Pc.Cedula = @cedula
            WHERE Cn.Activo = 1
              AND Cnt.Cod_Concepto = @codConcepto
              AND (Pc.Cedula IS NOT NULL OR Cn.Suscripcion_Abierta = 1)
              AND Cn.cod_contrato > @codContrato
            ORDER BY Cn.cod_contrato ASC;";

            return EjecutarConsultaScroll<CxCCuentasContratosFiltroItem>(
                 new EjecutarConsultaScrollRequest
                 {
                     codEmpresa = codEmpresa,
                     tipo = tipo,
                     sqlAnterior = sqlAnterior,
                     sqlSiguiente = sqlSiguiente,
                     parametros = new
                     {
                         cedula = cedulaNormalizada,
                         codConcepto = conceptoNormalizado,
                         codContrato = contratoNormalizado
                     },
                     mensajeNoEncontrado = "No hay más contratos para navegar.",
                     mensajeDb = "No fue posible navegar contratos.",
                     mensajeGeneral = "Error inesperado al navegar contratos."
                 });
        }

        /// <summary>
        /// Obtiene la lista lazy de contratos permitidos para un cliente y concepto.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <param name="filtros">Filtros lazy de búsqueda.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de contratos.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>> CxCCuentasContratosFiltro_Obtener(
            int codEmpresa,
            string cedula,
            string codConcepto,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var cedulaNormalizada = NormalizarTexto(cedula);
            var conceptoNormalizado = NormalizarTexto(codConcepto);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(conceptoNormalizado))
            {
                return new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>>
                {
                    Code = -1,
                    Description = "La cédula y el concepto son requeridos.",
                    Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>()
                };
            }

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "descripcion" => "Cnt.Descripcion",
                _ => "Cnt.cod_Contrato"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string fromWhere = @"
            FROM CxC_Personas_Contratos Con
            INNER JOIN CxC_Contratos Cnt
                ON Con.Cod_Contrato = Cnt.cod_contrato
            WHERE Con.cedula = @cedula
              AND Con.cod_contrato IN (
                    SELECT cod_contrato
                    FROM CxC_Conceptos_Contratos
                    WHERE cod_concepto = @codConcepto
                  )
              AND Con.Activo = 1
              AND (
                    @filtro IS NULL
                    OR ISNULL(Cnt.cod_Contrato, '') LIKE @like
                    OR ISNULL(Cnt.Descripcion, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            {fromWhere};";

            var sqlLista = $@"
            SELECT
                ISNULL(Cnt.cod_Contrato, '') AS cod_contrato,
                ISNULL(Cnt.Descripcion, '') AS descripcion
            {fromWhere}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasContratosFiltroItem>(
                    new EjecutarListaLazyLoadRequest
                    {
                        codEmpresa = codEmpresa,
                        filtros = filtros,
                        esExportar = esExportar,
                        sqlCount = sqlCount,
                        sqlLista = sqlLista,
                        parametrosAdicionales = new
                        {
                            cedula = cedulaNormalizada,
                            codConcepto = conceptoNormalizado
                        },
                        mensajeDb = "No fue posible consultar contratos.",
                        mensajeGeneral = "Error inesperado al consultar contratos."
                    });
        }

        /// <summary>
        /// Obtiene un pagador permitido para un cliente y contrato.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedulaPagador">Cédula del pagador.</param>
        /// <returns>Datos del pagador.</returns>
        public ErrorDto<CxCCuentasPagadorData> CxCCuentasPagador_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var contratoNormalizado = NormalizarTexto(codContrato);
            var pagadorNormalizado = NormalizarTexto(cedulaPagador);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) ||
                string.IsNullOrWhiteSpace(contratoNormalizado) ||
                string.IsNullOrWhiteSpace(pagadorNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPagadorData>("Cliente, contrato y pagador son requeridos.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(P.cedula, '') AS cedula,
                ISNULL(P.nombre, '') AS nombre
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND Pg.Cedula_Pagador = @cedulaPagador
              AND ISNULL(Pg.Activo, 1) = 1;";

            return EjecutarConsultaUnica<CxCCuentasPagadorData>(
                codEmpresa,
                sql,
                new
                {
                    cedulaCliente = clienteNormalizado,
                    codContrato = contratoNormalizado,
                    cedulaPagador = pagadorNormalizado
                },
                "No se encontró el pagador.",
                "No fue posible consultar el pagador.",
                "Error inesperado al consultar el pagador.");
        }

        /// <summary>
        /// Obtiene el pagador anterior o siguiente permitido para un cliente y contrato.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="cedulaPagador">Cédula actual del pagador.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Pagador encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasPagadoresFiltroItem> CxCCuentasPagadorScroll_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            string cedulaPagador,
            int tipo)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var contratoNormalizado = NormalizarTexto(codContrato);
            var pagadorNormalizado = NormalizarTexto(cedulaPagador);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPagadoresFiltroItem>("Cliente y contrato son requeridos.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(P.Cedula, '') AS cedula,
                ISNULL(P.Nombre, '') AS nombre
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND Pg.Cedula_Pagador < @cedulaPagador
              AND ISNULL(Pg.Activo, 1) = 1
            ORDER BY Pg.Cedula_Pagador DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(P.Cedula, '') AS cedula,
                ISNULL(P.Nombre, '') AS nombre
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND Pg.Cedula_Pagador > @cedulaPagador
              AND ISNULL(Pg.Activo, 1) = 1
            ORDER BY Pg.Cedula_Pagador ASC;";

            return EjecutarConsultaScroll<CxCCuentasPagadoresFiltroItem>(
                new EjecutarConsultaScrollRequest
                {
                    codEmpresa = codEmpresa,
                    tipo = tipo,
                    sqlAnterior = sqlAnterior,
                    sqlSiguiente = sqlSiguiente,
                    parametros = new
                    {
                        cedulaCliente = clienteNormalizado,
                        codContrato = contratoNormalizado,
                        cedulaPagador = pagadorNormalizado
                    },
                    mensajeNoEncontrado = "No hay más pagadores para navegar.",
                    mensajeDb = "No fue posible navegar pagadores.",
                    mensajeGeneral = "Error inesperado al navegar pagadores."
                });
        }

        /// <summary>
        /// Obtiene la lista lazy de pagadores permitidos para un cliente y contrato.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="codContrato">Código del contrato.</param>
        /// <param name="filtros">Filtros lazy serializados.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de pagadores.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>> CxCCuentasPagadoresFiltro_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string codContrato,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var contratoNormalizado = NormalizarTexto(codContrato);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>>
                {
                    Code = -1,
                    Description = "Cliente y contrato son requeridos.",
                    Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>()
                };
            }

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "nombre" => "P.Nombre",
                _ => "P.Cedula"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string fromWhere = @"
            FROM CxC_Personas P
            INNER JOIN CxC_Personas_Contratos_Pagadores Pg
                ON P.Cedula = Pg.Cedula_Pagador
            WHERE Pg.Cedula = @cedulaCliente
              AND Pg.Cod_Contrato = @codContrato
              AND ISNULL(Pg.Activo, 1) = 1
              AND (
                    @filtro IS NULL
                    OR ISNULL(P.Cedula, '') LIKE @like
                    OR ISNULL(P.Nombre, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            {fromWhere};";

            var sqlLista = $@"
            SELECT
                ISNULL(P.Cedula, '') AS cedula,
                ISNULL(P.Nombre, '') AS nombre
            {fromWhere}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasPagadoresFiltroItem>(
                    new EjecutarListaLazyLoadRequest
                    {
                        codEmpresa = codEmpresa,
                        filtros = filtros,
                        esExportar = esExportar,
                        sqlCount = sqlCount,
                        sqlLista = sqlLista,
                        parametrosAdicionales = new
                        {
                            cedulaCliente = clienteNormalizado,
                            codContrato = contratoNormalizado
                        },
                        mensajeDb = "No fue posible consultar pagadores.",
                        mensajeGeneral = "Error inesperado al consultar pagadores."
                    });
        }

        /// <summary>
        /// Obtiene un autorizado permitido para un cliente.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="cedulaAutorizado">Cédula del autorizado.</param>
        /// <returns>Datos del autorizado.</returns>
        public ErrorDto<CxCCuentasAutorizadoData> CxCCuentasAutorizado_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaAutorizado)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var autorizadoNormalizado = NormalizarTexto(cedulaAutorizado);

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(autorizadoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAutorizadoData>("Cliente y autorizado son requeridos.");
            }

            const string sql = @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado = @cedulaAutorizado;";

            return EjecutarConsultaUnica<CxCCuentasAutorizadoData>(
                codEmpresa,
                sql,
                new
                {
                    cedulaCliente = clienteNormalizado,
                    cedulaAutorizado = autorizadoNormalizado
                },
                "No se encontró el autorizado.",
                "No fue posible consultar el autorizado.",
                "Error inesperado al consultar el autorizado.");
        }

        /// <summary>
        /// Obtiene el autorizado anterior o siguiente permitido para un cliente.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="cedulaAutorizado">Cédula actual del autorizado.</param>
        /// <param name="tipo">1: anterior, 0: siguiente.</param>
        /// <returns>Autorizado encontrado para navegación.</returns>
        public ErrorDto<CxCCuentasAutorizadosFiltroItem> CxCCuentasAutorizadoScroll_Obtener(
            int codEmpresa,
            string cedulaCliente,
            string cedulaAutorizado,
            int tipo)
        {
            var clienteNormalizado = NormalizarTexto(cedulaCliente);
            var autorizadoNormalizado = NormalizarTexto(cedulaAutorizado);

            if (string.IsNullOrWhiteSpace(clienteNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAutorizadosFiltroItem>("El cliente es requerido.");
            }

            const string sqlAnterior = @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado < @cedulaAutorizado
            ORDER BY Pa.Cedula_Autorizado DESC;";

            const string sqlSiguiente = @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado > @cedulaAutorizado
            ORDER BY Pa.Cedula_Autorizado ASC;";

            return EjecutarConsultaScroll<CxCCuentasAutorizadosFiltroItem>(
                new EjecutarConsultaScrollRequest
                {
                    codEmpresa = codEmpresa,
                    tipo = tipo,
                    sqlAnterior = sqlAnterior,
                    sqlSiguiente = sqlSiguiente,
                    parametros = new
                    {
                        cedulaCliente = clienteNormalizado,
                        cedulaAutorizado = autorizadoNormalizado
                    },
                    mensajeNoEncontrado = "No hay más autorizados para navegar.",
                    mensajeDb = "No fue posible navegar autorizados.",
                    mensajeGeneral = "Error inesperado al navegar autorizados."
                });
        }

        /// <summary>
        /// Obtiene la lista lazy de autorizados permitidos para un cliente.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedulaCliente">Cédula del cliente.</param>
        /// <param name="filtros">Filtros lazy serializados.</param>
        /// <param name="esExportar">Indica si la consulta es para exportación.</param>
        /// <returns>Lista paginada de autorizados.</returns>
        public ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>> CxCCuentasAutorizadosFiltro_Obtener(
            int codEmpresa,
            string cedulaCliente,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var clienteNormalizado = NormalizarTexto(cedulaCliente);

            if (string.IsNullOrWhiteSpace(clienteNormalizado))
            {
                return new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>>
                {
                    Code = -1,
                    Description = "El cliente es requerido.",
                    Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>()
                };
            }

            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var orderByField = sortField switch
            {
                "nombre" => "Per.Nombre",
                _ => "Per.Cedula"
            };

            var direction = filtros.sortOrder == 1 ? "ASC" : "DESC";

            const string fromWhere = @"
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND (
                    @filtro IS NULL
                    OR ISNULL(Per.Cedula, '') LIKE @like
                    OR ISNULL(Per.Nombre, '') LIKE @like
                  )";

            var sqlCount = $@"
            SELECT COUNT(1)
            {fromWhere};";

            var sqlLista = $@"
            SELECT
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            {fromWhere}
            ORDER BY {orderByField} {direction}
            ";

            return EjecutarListaLazy<CxCCuentasAutorizadosFiltroItem>(
                new EjecutarListaLazyLoadRequest
                {
                    codEmpresa = codEmpresa,
                    filtros = filtros,
                    esExportar = esExportar,
                    sqlCount = sqlCount,
                    sqlLista = sqlLista,
                    mensajeDb = "No fue posible consultar autorizados.",
                    mensajeGeneral = "Error inesperado al consultar autorizados.",
                    parametrosAdicionales = new
                    {
                        cedulaCliente = clienteNormalizado
                    }
                });
        }

        /// <summary>
        /// Obtiene las cuentas bancarias de un cliente según el banco seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="banco">Banco seleccionado.</param>
        /// <returns>Lista de cuentas bancarias.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentasCuentasBancarias_Obtener(int codEmpresa, string cedula, string banco)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);
            var bancoNormalizado = NormalizarTexto(banco);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(bancoNormalizado))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>("La cédula y el banco son requeridos.");
            }

            const string sql = @"exec spSys_Cuentas_Bancarias @Identificacion, @BancoId, 1;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    Identificacion = cedulaNormalizada,
                    BancoId = bancoNormalizado
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>($"No fue posible consultar las cuentas bancarias. {ex.Message}");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>($"Error inesperado al consultar las cuentas bancarias. {ex.Message}");
            }
        }

        #endregion

        #region Facturas
        private static ErrorDto<CxCCuentasFacturaMantenimientoResult> CrearErrorFacturaMantenimiento(string mensaje)
        {
            return new ErrorDto<CxCCuentasFacturaMantenimientoResult>
            {
                Code = -1,
                Description = mensaje,
                Result = new CxCCuentasFacturaMantenimientoResult()
            };
        }

        private static string NormalizarMayusculas(string? valor)
        {
            return NormalizarTexto(valor).ToUpperInvariant();
        }

        private static string? ValidarOperacionFactura(long operacion, string estado, string autorizaEstado)
        {
            if (operacion <= 0)
            {
                return CxCCuentasConstantes.operacionRequerida;
            }

            if (estado is "A" or "D")
            {
                return "La operación no está pendiente o recibida, no pueden realizarse los cambios.";
            }

            if (autorizaEstado != "P")
            {
                return "La operación ya fue autorizada o denegada.";
            }

            return null;
        }

        private static string? ValidarRegistroFactura(
            CxCCuentasFacturaRegistraRequest request,
            string factura,
            string divisa,
            string facturaEstado,
            string adelantoTipo,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(factura) || request.importe <= 0)
            {
                return "El número de factura o el importe no es válido.";
            }

            if (string.IsNullOrWhiteSpace(divisa) ||
                string.IsNullOrWhiteSpace(facturaEstado) ||
                string.IsNullOrWhiteSpace(usuario))
            {
                return "Faltan datos requeridos para registrar la factura.";
            }

            if (request.tipo_cambio <= 0 || request.monto <= 0)
            {
                return "El tipo de cambio o el monto no es válido.";
            }

            if (adelantoTipo is not ("P" or "M"))
            {
                return "El tipo de adelanto no es válido.";
            }

            if (request.fecha_emision is null || request.fecha_pago is null)
            {
                return "Las fechas de emisión y pago son requeridas.";
            }

            return null;
        }

        private static string? ValidarEliminacionFactura(string factura, string usuario)
        {
            if (string.IsNullOrWhiteSpace(factura))
            {
                return "La factura es requerida.";
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return "El usuario es requerido.";
            }

            return null;
        }

        private static string? ValidarVinculacionFactura(CxCCuentasFacturaVincularRequest request, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return "El usuario es requerido.";
            }

            if (request.facturas is null || request.facturas.Count == 0)
            {
                return "Debe seleccionar al menos una factura.";
            }

            return null;
        }

        private static string? ValidarItemFacturaVincular(
            CxCCuentasFacturaVincularItem item,
            string factura,
            string divisa)
        {
            if (string.IsNullOrWhiteSpace(factura) || string.IsNullOrWhiteSpace(divisa))
            {
                return "Hay facturas seleccionadas con datos incompletos.";
            }

            if (item.importe <= 0 || item.tipo_cambio <= 0 || item.monto <= 0)
            {
                return "Hay facturas seleccionadas con importes inválidos.";
            }

            return null;
        }

        private ErrorDto<CxCCuentasFacturaMantenimientoResult> EjecutarFacturaMantenimiento(
            int codEmpresa,
            string mensajeDb,
            string mensajeGeneral,
            Func<SqlConnection, ErrorDto<CxCCuentasFacturaMantenimientoResult>> accion)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                return accion(conn);
            }
            catch (DbException ex)
            {
                return CrearErrorFacturaMantenimiento($"{mensajeDb} {ex.Message}");
            }
            catch (Exception ex)
            {
                return CrearErrorFacturaMantenimiento($"{mensajeGeneral} {ex.Message}");
            }
        }

        private static bool FacturaDisponibleParaOperacion(SqlConnection conn, long operacion, string factura)
        {
            const string sql = @"
                SELECT dbo.fxCxC_FacturaValida(@operacion, @factura) AS pass;";

            var pass = conn.QueryFirstOrDefault<int>(sql, new
            {
                operacion,
                factura
            });

            return pass != 0;
        }

        private static ErrorDto<CxCCuentasFacturaMantenimientoResult> EjecutarConsultaFacturaMantenimiento(
            SqlConnection conn,
            string sql,
            object parametros)
        {
            var data = conn.QueryFirstOrDefault<CxCCuentasFacturaMantenimientoResult>(sql, parametros);

            return DbHelper.CreateOkResponse(data ?? new CxCCuentasFacturaMantenimientoResult());
        }

        /// <summary>
        /// Registra una factura en la operación de CxC y recalcula sus totales.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos de la factura a registrar.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Registra(
            int codEmpresa,
            CxCCuentasFacturaRegistraRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento("La solicitud es requerida.");
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var factura = NormalizarTexto(request.factura);
            var divisa = NormalizarTexto(request.divisa);
            var facturaEstado = NormalizarMayusculas(request.factura_estado);
            var adelantoTipo = NormalizarMayusculas(request.adelanto_tipo);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarRegistroFactura(request, factura, divisa, facturaEstado, adelantoTipo, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible registrar la factura.",
                "Error inesperado al registrar la factura.",
                conn =>
                {
                    if (!FacturaDisponibleParaOperacion(conn, request.operacion, factura))
                    {
                        return CrearErrorFacturaMantenimiento("Esta factura ya ha sido utilizada anteriormente con este cliente.");
                    }

                    const string sqlRegistra = @"
                        exec spCxC_Operacion_Factura_Registra
                            @operacion,
                            @factura,
                            @divisa,
                            @factura_estado,
                            @importe,
                            @tipo_cambio,
                            @monto,
                            @adelanta,
                            @adelanto_tipo,
                            @adelanto,
                            @fecha_emision,
                            @fecha_pago,
                            @usuario,
                            'I',
                            1,
                            1,
                            0;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sqlRegistra, new
                    {
                        operacion = request.operacion,
                        factura,
                        divisa,
                        factura_estado = facturaEstado,
                        importe = request.importe,
                        tipo_cambio = request.tipo_cambio,
                        monto = request.monto,
                        adelanta = request.adelanta ? 1 : 0,
                        adelanto_tipo = adelantoTipo,
                        adelanto = request.adelanta ? request.adelanto : 0,
                        fecha_emision = request.fecha_emision!.Value.ToString(CxCCuentasConstantes.fechaFormat),
                        fecha_pago = request.fecha_pago!.Value.ToString(CxCCuentasConstantes.fechaFormat),
                        usuario
                    });
                });
        }

        /// <summary>
        /// Elimina una factura de la operación de CxC y recalcula sus totales.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos mínimos de la factura a eliminar.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Elimina(
            int codEmpresa,
            CxCCuentasFacturaEliminaRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento("La solicitud es requerida.");
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var factura = NormalizarTexto(request.factura);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarEliminacionFactura(factura, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible eliminar la factura.",
                "Error inesperado al eliminar la factura.",
                conn =>
                {
                    const string sql = @"
                        exec spCxC_Operacion_Factura_Registra
                            @operacion,
                            @factura,
                            '',
                            '',
                            0,
                            0,
                            0,
                            0,
                            'M',
                            0,
                            null,
                            null,
                            @usuario,
                            'E',
                            1,
                            1,
                            0;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sql, new
                    {
                        operacion = request.operacion,
                        factura,
                        usuario
                    });
                });
        }

        /// <summary>
        /// Vincula facturas adelantadas a una operación de CxC y recalcula sus totales.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos de las facturas a vincular.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Vincular(
            int codEmpresa,
            CxCCuentasFacturaVincularRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento("La solicitud es requerida.");
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarVinculacionFactura(request, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible vincular las facturas.",
                "Error inesperado al vincular las facturas.",
                conn =>
                {
                    const string sqlRegistra = @"
                        exec spCxC_Operacion_Factura_Registra
                            @operacion,
                            @factura,
                            @divisa,
                            'T',
                            @importe,
                            @tipo_cambio,
                            @monto,
                            0,
                            'M',
                            @adelanto,
                            @fecha_emision,
                            @fecha_pago,
                            @usuario,
                            'I',
                            1,
                            0,
                            @operacion_origen;";

                    foreach (var item in request.facturas)
                    {
                        var factura = NormalizarTexto(item.factura);
                        var divisa = NormalizarTexto(item.divisa);

                        var mensajeItem = ValidarItemFacturaVincular(item, factura, divisa);
                        if (!string.IsNullOrWhiteSpace(mensajeItem))
                        {
                            return CrearErrorFacturaMantenimiento(mensajeItem);
                        }

                        conn.Execute(sqlRegistra, new
                        {
                            operacion = request.operacion,
                            factura,
                            divisa,
                            importe = item.importe,
                            tipo_cambio = item.tipo_cambio,
                            monto = item.monto,
                            adelanto = item.adelanto,
                            fecha_emision = item.fecha_emision?.ToString(CxCCuentasConstantes.fechaFormat),
                            fecha_pago = item.fecha_pago?.ToString(CxCCuentasConstantes.fechaFormat),
                            usuario,
                            operacion_origen = item.operacion_origen
                        });
                    }

                    const string sqlActualiza = @"
                        exec spCxC_Operacion_Facturas_Actualiza @operacion, 1, @usuario;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sqlActualiza, new
                    {
                        operacion = request.operacion,
                        usuario
                    });
                });
        }

        #endregion
    }
}
