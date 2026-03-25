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
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasLista>(CxCCuentasConstantes.operacionRequerida);
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

                return DbHelper.CreateOkResponse<CxCCuentasFacturasLista>(
                     new CxCCuentasFacturasLista
                     {
                         lista = lista,
                         casos = lista.Count,
                         total = lista.Sum(x => x.monto),
                         adelanto = lista.Sum(x => x.adelanto_monto)
                     });
               
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasLista>($"Error inesperado al consultar las facturas de la operación. {ex.Message}");
            }

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
            var response = DbHelper.CreateOkResponse<CxCCuentasFacturasAdelantadasLista>();
            var cedulaNormalizada = NormalizarTexto(cedula);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasAdelantadasLista>("La cédula es requerida.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"exec spCxC_Facturas_Adelantadas_Pendientes @Cedula, @Pagador;";
                var lista = conn.Query<CxCCuentasFacturasAdelantadasData>(sql, new
                {
                    Cedula = cedulaNormalizada,
                    Pagador = NormalizarTexto(cedulaPagador)
                }).ToList();

                return DbHelper.CreateOkResponse<CxCCuentasFacturasAdelantadasLista>(
                        new CxCCuentasFacturasAdelantadasLista
                        {
                            lista = lista,
                            casos = lista.Count,
                            total = lista.Sum(x => x.monto),
                            adelanto = lista.Sum(x => x.adelanto_monto),
                            adelantoListado = true
                        }
                    );
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasAdelantadasLista>($"Error inesperado al consultar las facturas adelantadas. {ex.Message}");
            }
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

        #region Guardar
        private sealed class CxCCuentasGuardarContext
        {
            public string usuario { get; init; } = string.Empty;
            public string cedula { get; init; } = string.Empty;
            public string cod_concepto { get; init; } = string.Empty;
            public string cod_oficina { get; init; } = string.Empty;
            public string emitir_tipo { get; init; } = string.Empty;
            public string estado { get; init; } = "R";
            public string? cedula_pagador { get; init; }
            public string? cedula_autorizado { get; init; }
            public string? notas { get; init; }
            public string? emitir_banco { get; init; }
            public string? emitir_cuenta { get; init; }
            public string? num_documento { get; init; }
            public string? cod_contrato { get; init; }
            public int plazo_dias { get; init; }
            public int freq_pago { get; init; }
            public DateTime? fecha_inicio { get; init; }
            public int adelanto_comision_apl { get; init; }
        }

        private static CxCCuentasGuardarContext CrearGuardarContext(CxCCuentasSaveParams param)
        {
            return new CxCCuentasGuardarContext
            {
                usuario = NormalizarTexto(param.usuario),
                cedula = NormalizarTexto(param.cedula),
                cod_concepto = NormalizarTexto(param.cod_concepto),
                cod_oficina = NormalizarTexto(param.cod_oficina),
                emitir_tipo = NormalizarTexto(param.emitir_tipo),
                estado = string.IsNullOrWhiteSpace(param.estado) ? "R" : NormalizarMayusculas(param.estado),
                cedula_pagador = string.IsNullOrWhiteSpace(param.cedula_pagador) ? null : NormalizarTexto(param.cedula_pagador),
                cedula_autorizado = string.IsNullOrWhiteSpace(param.cedula_autorizado) ? null : NormalizarTexto(param.cedula_autorizado),
                notas = string.IsNullOrWhiteSpace(param.notas) ? null : param.notas.Trim(),
                emitir_banco = string.IsNullOrWhiteSpace(param.emitir_banco) ? null : NormalizarTexto(param.emitir_banco),
                emitir_cuenta = string.IsNullOrWhiteSpace(param.emitir_cuenta) ? null : NormalizarTexto(param.emitir_cuenta),
                num_documento = string.IsNullOrWhiteSpace(param.num_documento) ? null : param.num_documento.Trim(),
                cod_contrato = string.IsNullOrWhiteSpace(param.cod_contrato) ? null : NormalizarTexto(param.cod_contrato),
                plazo_dias = param.plazo * 30,
                freq_pago = param.chk_cta_apl ? 30 : 0,
                fecha_inicio = param.chk_cta_apl ? param.fecha_inicio : null,
                adelanto_comision_apl = param.adelanto_comision_apl ? 1 : 0
            };
        }

        private static string? ValidarGuardarRequest(CxCCuentasSaveParams param, CxCCuentasGuardarContext context)
        {
            if (string.IsNullOrWhiteSpace(context.usuario) ||
                string.IsNullOrWhiteSpace(context.cedula) ||
                string.IsNullOrWhiteSpace(context.cod_concepto) ||
                string.IsNullOrWhiteSpace(context.cod_oficina) ||
                string.IsNullOrWhiteSpace(context.emitir_tipo))
            {
                return "Faltan datos requeridos para guardar la operación.";
            }

            if (param.monto <= 0)
            {
                return "El monto debe ser mayor a cero.";
            }

            if (param.plazo <= 0)
            {
                return "El plazo debe ser mayor a cero.";
            }

            if (param.cuota <= 0)
            {
                return "La cuota debe ser mayor a cero.";
            }

            if (param.chk_cta_apl && param.fecha_inicio is null)
            {
                return "La fecha de inicio es requerida cuando aplica cuenta.";
            }

            return null;
        }

        private static bool ExisteOperacionGuardar(SqlConnection conn, long operacion)
        {
            if (operacion <= 0)
            {
                return false;
            }

            return conn.QuerySingleOrDefault<int>(
                @"SELECT COUNT(1)
                  FROM CxC_Cuentas
                  WHERE Operacion = @operacion;",
                new { operacion }) > 0;
        }

        private static long ObtenerNuevaOperacionGuardar(SqlConnection conn)
        {
            return conn.QuerySingle<long>(
                @"SELECT ISNULL(MAX(Operacion), 0) + 1
                  FROM CxC_Cuentas;");
        }

        private static DynamicParameters CrearParametrosGuardar(
            long operacion,
            CxCCuentasSaveParams param,
            CxCCuentasGuardarContext context)
        {
            var parametros = new DynamicParameters();

            parametros.Add("operacion", operacion);
            parametros.Add("cedula", context.cedula);
            parametros.Add("cedula_pagador", context.cedula_pagador);
            parametros.Add("cedula_autorizado", context.cedula_autorizado);
            parametros.Add("cod_concepto", context.cod_concepto);
            parametros.Add("cod_oficina", context.cod_oficina);
            parametros.Add("notas", context.notas);
            parametros.Add("monto", param.monto);
            parametros.Add("emitir_tipo", context.emitir_tipo);
            parametros.Add("emitir_banco", context.emitir_banco);
            parametros.Add("emitir_cuenta", context.emitir_cuenta);
            parametros.Add("tasa_corriente", param.tasa_corriente);
            parametros.Add("tasa_mora", param.tasa_mora);
            parametros.Add("cuota", param.cuota);
            parametros.Add("plazo_dias", context.plazo_dias);
            parametros.Add("plazo", param.plazo);
            parametros.Add("estado", context.estado);
            parametros.Add("num_documento", context.num_documento);
            parametros.Add("cod_contrato", context.cod_contrato);
            parametros.Add("usuario", context.usuario);
            parametros.Add("adelanto_monto", param.adelanto_monto);
            parametros.Add("adelanto_porcentaje", param.adelanto_porcentaje);
            parametros.Add("adelanto_comision_apl", context.adelanto_comision_apl);
            parametros.Add("adelanto_comision", param.adelanto_comision);
            parametros.Add("adelanto_comision_dias", param.adelanto_comision_dias);
            parametros.Add("freq_pago", context.freq_pago);
            parametros.Add("fecha_inicio", context.fecha_inicio);

            return parametros;
        }

        private static void InsertarOperacionGuardar(SqlConnection conn, DynamicParameters parametros)
        {
            const string sqlInsert = @"
                INSERT INTO CxC_Cuentas
                (
                    OPERACION,
                    CEDULA,
                    CEDULA_PAGADOR,
                    COD_CONCEPTO,
                    COD_OFICINA,
                    NOTAS,
                    MONTO,
                    SALDO,
                    REBAJOS_TOTAL,
                    EMITIR_TIPO,
                    EMITIR_BANCO,
                    EMITIR_CUENTA,
                    DESEMBOLSO_MONTO,
                    TIPO_PLAZO,
                    TASA_CORRIENTE,
                    TASA_MORA,
                    CUOTA,
                    DIAS_PLAZO,
                    PLAZO,
                    AMORTIZA,
                    INTERESC,
                    ESTADO,
                    NUM_DOCUMENTO,
                    COD_CONTRATO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO,
                    FECHA_ULTMOV,
                    AUTORIZA_ESTADO,
                    ADELANTO_MONTO,
                    ADELANTO_PORCENTAJE,
                    DESEMBOLSO_REALIZADO,
                    DESEMBOLSO_PENDIENTE,
                    CEDULA_AUTORIZADO,
                    ADELANTO_COMISION_APL,
                    ADELANTO_COMISION,
                    ADELANTO_COMISION_DIAS,
                    FREQ_PAGO,
                    FECHA_INICIO
                )
                VALUES
                (
                    @operacion,
                    @cedula,
                    @cedula_pagador,
                    @cod_concepto,
                    @cod_oficina,
                    @notas,
                    @monto,
                    @monto,
                    0,
                    @emitir_tipo,
                    @emitir_banco,
                    @emitir_cuenta,
                    @monto,
                    'M',
                    @tasa_corriente,
                    @tasa_mora,
                    @cuota,
                    @plazo_dias,
                    @plazo,
                    0,
                    0,
                    'R',
                    @num_documento,
                    @cod_contrato,
                    dbo.MyGetdate(),
                    @usuario,
                    dbo.MyGetdate(),
                    'P',
                    @adelanto_monto,
                    @adelanto_porcentaje,
                    0,
                    0,
                    @cedula_autorizado,
                    @adelanto_comision_apl,
                    @adelanto_comision,
                    @adelanto_comision_dias,
                    @freq_pago,
                    @fecha_inicio
                );";

            conn.Execute(sqlInsert, parametros);
        }

        private static void ActualizarOperacionGuardar(SqlConnection conn, DynamicParameters parametros)
        {
            const string sqlUpdate = @"
                UPDATE CxC_Cuentas
                SET
                    CEDULA_PAGADOR = @cedula_pagador,
                    CEDULA_AUTORIZADO = @cedula_autorizado,
                    COD_CONCEPTO = @cod_concepto,
                    COD_OFICINA = @cod_oficina,
                    NOTAS = @notas,
                    MONTO = @monto,
                    EMITIR_TIPO = @emitir_tipo,
                    EMITIR_BANCO = @emitir_banco,
                    EMITIR_CUENTA = @emitir_cuenta,
                    TASA_CORRIENTE = @tasa_corriente,
                    TASA_MORA = @tasa_mora,
                    CUOTA = @cuota,
                    COD_CONTRATO = @cod_contrato,
                    ESTADO = @estado,
                    NUM_DOCUMENTO = @num_documento,
                    DESEMBOLSO_MONTO = @monto,
                    DIAS_PLAZO = @plazo_dias,
                    PLAZO = @plazo,
                    ADELANTO_MONTO = @adelanto_monto,
                    ADELANTO_PORCENTAJE = @adelanto_porcentaje,
                    ADELANTO_COMISION_APL = @adelanto_comision_apl,
                    ADELANTO_COMISION = @adelanto_comision,
                    ADELANTO_COMISION_DIAS = @adelanto_comision_dias,
                    FREQ_PAGO = @freq_pago,
                    FECHA_INICIO = @fecha_inicio
                WHERE Operacion = @operacion;";

            conn.Execute(sqlUpdate, parametros);
        }

        private static void RecalcularOperacionGuardada(
            SqlConnection conn,
            long operacion,
            string usuario,
            decimal monto)
        {
            var facturasRegistradas = conn.QuerySingleOrDefault<int>(
                @"SELECT COUNT(1)
                  FROM CxC_Operacion_Facturas
                  WHERE Operacion = @operacion;",
                new { operacion });

            if (facturasRegistradas > 0)
            {
                conn.Execute(
                    @"exec spCxC_Operacion_Facturas_Actualiza @operacion, 0, @usuario;",
                    new { operacion, usuario });

                return;
            }

            conn.Execute(
                @"exec spCxC_CuentaCargosActualiza @operacion, @monto;",
                new { operacion, monto });
        }

        /// <summary>
        /// Guarda una operación de CxC en modo inserción o actualización.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="param">Datos de la operación.</param>
        /// <returns>Número de operación guardada.</returns>
        public ErrorDto<long> CxCCuentas_Guardar(int codEmpresa, CxCCuentasSaveParams param)
        {
            var response = new ErrorDto<long>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            if (param is null)
            {
                response.Code = -1;
                response.Description = "Los datos de la operación son requeridos.";
                return response;
            }

            var context = CrearGuardarContext(param);
            var mensajeValidacion = ValidarGuardarRequest(param, context);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                response.Code = -1;
                response.Description = mensajeValidacion;
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var existeOperacion = ExisteOperacionGuardar(conn, param.operacion);
                var operacion = existeOperacion ? param.operacion : ObtenerNuevaOperacionGuardar(conn);
                var parametros = CrearParametrosGuardar(operacion, param, context);

                if (existeOperacion)
                {
                    ActualizarOperacionGuardar(conn, parametros);
                }
                else
                {
                    InsertarOperacionGuardar(conn, parametros);
                }

                RecalcularOperacionGuardada(conn, operacion, context.usuario, param.monto);
                response.Result = operacion;
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible guardar la operación. {ex.Message}";
                response.Result = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al guardar la operación. {ex.Message}";
                response.Result = 0;
            }

            return response;
        }

        #endregion

        #region Activar

        /// <summary>
        /// Verifica si una operación de CxC puede activarse.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos mínimos requeridos para validar la activación.</param>
        /// <returns>Resultado de validación de activación.</returns>
        public ErrorDto<CxCCuentasActivacionVerificaResult> CxCCuentasActivacion_Verifica(
            int codEmpresa,
            CxCCuentasActivacionRequest request)
        {
            var response = DbHelper.CreateOkResponse<CxCCuentasActivacionVerificaResult>();

            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasActivacionVerificaResult>(CxCCuentasConstantes.solicitudRequerida);
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasActivacionVerificaResult>(CxCCuentasConstantes.operacionRequerida);
            }

            var emitirTipo = NormalizarMayusculas(request.emitir_tipo);
            var emitirCuenta = NormalizarTexto(request.emitir_cuenta);
            var mensajes = new List<string>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (emitirTipo == "TE" && string.IsNullOrWhiteSpace(emitirCuenta))
                {
                    mensajes.Add("- No se ha especificado una cuenta de ahorros para realizarle la transferencia...");
                }

                const string sqlMontos = @"
                    SELECT
                        Monto,
                        dbo.fxCxC_CuentaRebajos(@operacion, 'TOT') AS Rebajos,
                        ISNULL(dbo.fxCxC_CuentaIngresos(@operacion), 0) AS Ingresos
                    FROM CxC_Cuentas
                    WHERE Operacion = @operacion;";

                var montos = conn.QueryFirstOrDefault(sqlMontos, new
                {
                    operacion = request.operacion
                });

                if (montos is null)
                {
                    mensajes.Add("- No existe la operación indicada.");
                }
                else
                {
                    decimal monto = montos.Monto ?? 0m;
                    decimal rebajos = montos.Rebajos ?? 0m;
                    decimal ingresos = montos.Ingresos ?? 0m;

                    if (rebajos > (monto + ingresos))
                    {
                        mensajes.Add("- El monto de los rebajos es mayor que el monto de la operación más otros ingresos.");
                    }
                }

                const string sqlFacturas = @"exec spCxC_Operacion_Facturas_Verifica @operacion;";
                var facturas = conn.Query(sqlFacturas, new
                {
                    operacion = request.operacion
                });

                foreach (var item in facturas)
                {
                    mensajes.Add($"- Factura No.: {item.cod_Factura}, se encuentra registrada en la Operación: {item.Operacion}");
                }

                response.Result!.pass = mensajes.Count == 0;
                response.Result!.mensaje = string.Join(Environment.NewLine, mensajes);

                return DbHelper.CreateOkResponse<CxCCuentasActivacionVerificaResult>();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasActivacionVerificaResult>($"Error inesperado al verificar la activación. {ex.Message}");
            }

        }

        /// <summary>
        /// Activa una operación de CxC aplicando rebajos, ingresos y estado de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos requeridos para activar la operación.</param>
        /// <returns>Resultado del proceso de activación.</returns>
        public ErrorDto<bool> CxCCuentasActivacion_Activar(
            int codEmpresa,
            CxCCuentasActivacionRequest request)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            if (request is null)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.solicitudRequerida;
                response.Result = false;
                return response;
            }

            if (request.operacion <= 0)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.operacionRequerida;
                response.Result = false;
                return response;
            }

            var usuario = NormalizarTexto(request.usuario);
            var emitirTipo = NormalizarMayusculas(request.emitir_tipo);
            var numDocumento = string.IsNullOrWhiteSpace(request.num_documento) ? null : request.num_documento.Trim();

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(emitirTipo))
            {
                response.Code = -1;
                response.Description = "Faltan datos requeridos para activar la operación.";
                response.Result = false;
                return response;
            }

            var verifica = CxCCuentasActivacion_Verifica(codEmpresa, request);
            if (verifica.Code == -1 || verifica.Result is null || !verifica.Result.pass)
            {
                response.Code = -1;
                response.Description = verifica.Result?.mensaje ?? verifica.Description;
                response.Result = false;
                return response;
            }

            DbTransaction? transaction = null;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                transaction = conn.BeginTransaction();

                const string sqlContexto = @"
            SELECT
                C.Monto,
                dbo.fxCxC_CuentaRebajos(C.Operacion, 'TOT') AS Rebajos,
                ISNULL(dbo.fxCxC_CuentaIngresos(C.Operacion), 0) AS Ingresos,
                ISNULL(P.Genera_Desembolso, 0) AS Genera_Desembolso,
                dbo.MyGetdate() AS Fecha_Server
            FROM CxC_Cuentas C
            INNER JOIN CxC_Conceptos P
                ON C.cod_concepto = P.cod_concepto
            WHERE C.Operacion = @operacion;";

                var contexto = conn.QueryFirstOrDefault(sqlContexto, new
                {
                    operacion = request.operacion
                }, transaction);

                if (contexto is null)
                {
                    transaction.Rollback();
                    response.Code = -1;
                    response.Description = "No se encontró la operación para activar.";
                    response.Result = false;
                    return response;
                }

                decimal monto = contexto.Monto ?? 0m;
                decimal rebajos = contexto.Rebajos ?? 0m;
                decimal ingresos = contexto.Ingresos ?? 0m;
                int generaDesembolso = contexto.Genera_Desembolso ?? 0;
                DateTime fechaServer = contexto.Fecha_Server ?? DateTime.Now;

                const string sqlActiva = @"
            UPDATE CxC_Cuentas
            SET
                Estado = 'A',
                Activa_Fecha = dbo.MyGetdate(),
                Activa_Usuario = @usuario,
                Rebajos_Total = @rebajos,
                Ingresos_Total = @ingresos,
                Desembolso_Monto = Monto + @ingresos - @rebajos,
                Num_Documento = @num_documento
            WHERE Operacion = @operacion;";

                conn.Execute(sqlActiva, new
                {
                    operacion = request.operacion,
                    usuario,
                    rebajos,
                    ingresos,
                    num_documento = numDocumento
                }, transaction);

                if (request.es_factoreo)
                {
                    const string sqlPendienteFactoreo = @"
                UPDATE CxC_Cuentas
                SET Desembolso_Pendiente =
                    CASE
                        WHEN (Desembolso_Realizado + Desembolso_Pendiente) > Desembolso_Monto
                            THEN Desembolso_Monto - Desembolso_Realizado
                        ELSE Desembolso_Pendiente
                    END
                WHERE Operacion = @operacion;";

                    conn.Execute(sqlPendienteFactoreo, new
                    {
                        operacion = request.operacion
                    }, transaction);
                }
                else
                {
                    const string sqlPendiente = @"
                UPDATE CxC_Cuentas
                SET Desembolso_Pendiente = Desembolso_Monto - Desembolso_Realizado
                WHERE Operacion = @operacion;";

                    conn.Execute(sqlPendiente, new
                    {
                        operacion = request.operacion
                    }, transaction);
                }

                if ((monto + ingresos) <= rebajos || generaDesembolso == 0)
                {
                    const string sqlTesoreriaC = @"
                UPDATE CxC_Cuentas
                SET
                    Tesoreria_Fecha = dbo.MyGetdate(),
                    Tesoreria_Solicitud = 0,
                    Tesoreria_Estado = 'C',
                    Tesoreria_Usuario = @usuario
                WHERE Operacion = @operacion;";

                    conn.Execute(sqlTesoreriaC, new
                    {
                        operacion = request.operacion,
                        usuario
                    }, transaction);
                }
                else
                {
                    const string sqlTesoreriaP = @"
                UPDATE CxC_Cuentas
                SET Tesoreria_Estado = 'P'
                WHERE Operacion = @operacion;";

                    conn.Execute(sqlTesoreriaP, new
                    {
                        operacion = request.operacion
                    }, transaction);
                }

                const string sqlDetalle = @"
            exec spCxC_CuentaActivaDetalle @operacion, @fecha, @usuario;";

                conn.Execute(sqlDetalle, new
                {
                    operacion = request.operacion,
                    fecha = fechaServer.ToString(CxCCuentasConstantes.fechaFormat),
                    usuario
                }, transaction);

                transaction.Commit();
            }
            catch (DbException ex)
            {
                transaction?.Rollback();
                response.Code = -1;
                response.Description = $"No fue posible activar la operación. {ex.Message}";
                response.Result = false;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                response.Code = -1;
                response.Description = $"Error inesperado al activar la operación. {ex.Message}";
                response.Result = false;
            }

            return response;
        }

        #endregion

        #region Anular

        /// <summary>
        /// Verifica si una operación de CxC puede anularse.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos mínimos requeridos para validar la anulación.</param>
        /// <returns>Resultado de validación de anulación.</returns>
        public ErrorDto<CxCCuentasAnulacionVerificaResult> CxCCuentasAnulacion_Verifica(
            int codEmpresa,
            CxCCuentasAnulacionRequest request)
        {
            var response = DbHelper.CreateOkResponse<CxCCuentasAnulacionVerificaResult>();

            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>(CxCCuentasConstantes.solicitudRequerida);
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>(CxCCuentasConstantes.operacionRequerida);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT TOP 1
                        ISNULL(Estado, '') AS estado
                    FROM CxC_Cuentas
                    WHERE Operacion = @operacion;";

                var estado = conn.QueryFirstOrDefault<string>(sql, new
                {
                    operacion = request.operacion
                });

                if (string.IsNullOrWhiteSpace(estado))
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>("No se encontró la operación indicada.");
                }

                var pass = estado.Trim().ToUpperInvariant() == "A";

                response.Result!.pass = pass;
                response.Result!.mensaje = pass ? string.Empty : "Solo pueden anularse operaciones activas.";

                return DbHelper.CreateOkResponse<CxCCuentasAnulacionVerificaResult>(response.Result);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>($"Error inesperado al verificar la anulación. {ex.Message}");
            }

        }

        /// <summary>
        /// Anula una operación de CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos requeridos para anular la operación.</param>
        /// <returns>Resultado del proceso de anulación.</returns>
        public ErrorDto<bool> CxCCuentasAnulacion_Anular(
            int codEmpresa,
            CxCCuentasAnulacionRequest request)
        {
            var response = DbHelper.CreateOkResponse<bool>();

            if (request is null)
            {
               return DbHelper.CreateErrorResponse<bool>(CxCCuentasConstantes.solicitudRequerida);
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<bool>(CxCCuentasConstantes.operacionRequerida);
            }

            var usuario = NormalizarTexto(request.usuario);
            var notas = string.IsNullOrWhiteSpace(request.notas) ? string.Empty : request.notas.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<bool>(CxCCuentasConstantes.usuarioRequerido);
            }

            var verifica = CxCCuentasAnulacion_Verifica(codEmpresa, request);
            if (verifica.Code == -1 || verifica.Result is null || !verifica.Result.pass)
            {
                return DbHelper.CreateErrorResponse<bool>(verifica.Result?.mensaje ?? verifica.Description!);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    exec spCxC_Cuenta_Anulacion @operacion, @usuario, @notas;";

                conn.Execute(sql, new
                {
                    operacion = request.operacion,
                    usuario,
                    notas
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<bool>($"Error inesperado al anular la operación. {ex.Message}");
            }

            return response;
        }

        #endregion

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
                return DbHelper.CreateErrorResponse<CxCCuentasPersonasFiltroLista>(listaResponse.Description!);
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
                return CxCCuentasConstantes.usuarioRequerido;
            }

            return null;
        }

        private static string? ValidarVinculacionFactura(CxCCuentasFacturaVincularRequest request, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CxCCuentasConstantes.usuarioRequerido;
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
            string mensajeGeneral,
            Func<SqlConnection, ErrorDto<CxCCuentasFacturaMantenimientoResult>> accion)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                return accion(conn);
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
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
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
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
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
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
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

        private sealed class CxCCuentasFacturaCargaFilaNormalizada
        {
            public string factura { get; init; } = string.Empty;
            public string divisa { get; init; } = string.Empty;
            public string factura_estado { get; init; } = string.Empty;
            public decimal importe { get; init; }
            public decimal tipo_cambio { get; init; }
            public decimal monto { get; init; }
            public int adelanta { get; init; }
            public string adelanto_tipo { get; init; } = "M";
            public decimal adelanto_monto { get; init; }
            public string fecha_emite { get; init; } = string.Empty;
            public string fecha_pago { get; init; } = string.Empty;
        }

        private static string? ValidarCargaArchivoFactura(
            CxCCuentasFacturaCargaRequest request,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CxCCuentasConstantes.usuarioRequerido;
            }

            if (request.facturas is null || request.facturas.Count == 0)
            {
                return "Debe seleccionar al menos una factura.";
            }

            return null;
        }

        private static string? ValidarFilaFacturaCarga(
            CxCCuentasFacturaCargaItem item,
            string factura,
            string divisa)
        {
            if (string.IsNullOrWhiteSpace(factura) || string.IsNullOrWhiteSpace(divisa))
            {
                return "Hay filas del archivo con datos incompletos.";
            }

            if (item.importe <= 0 || item.tipo_cambio <= 0)
            {
                return "Hay filas del archivo con importes inválidos.";
            }

            return null;
        }

        private static CxCCuentasFacturaCargaFilaNormalizada NormalizarFilaFacturaCarga(CxCCuentasFacturaCargaItem item)
        {
            var factura = NormalizarTexto(item.factura);
            var divisa = NormalizarTexto(item.divisa);
            var facturaEstado = NormalizarMayusculas(item.estado);
            var adelantoBase = facturaEstado == "A" ? item.monto : 0;
            var adelantoMonto = item.adelanto > 0 ? item.adelanto : adelantoBase;
            var monto = item.importe * item.tipo_cambio;

            if (adelantoMonto > monto)
            {
                adelantoMonto = monto;
            }

            var adelanta = adelantoMonto > 0 || facturaEstado == "A";
            var adelantoTipo = facturaEstado == "A" && adelantoMonto == 0 ? "P" : "M";

            return new CxCCuentasFacturaCargaFilaNormalizada
            {
                factura = factura,
                divisa = divisa,
                factura_estado = facturaEstado,
                importe = item.importe,
                tipo_cambio = item.tipo_cambio,
                monto = monto,
                adelanta = adelanta ? 1 : 0,
                adelanto_tipo = adelantoTipo,
                adelanto_monto = adelantoMonto,
                fecha_emite = item.fecha_emite,
                fecha_pago = item.fecha_pago
            };
        }

        private static void RegistrarFilaFacturaCarga(
            SqlConnection conn,
            long operacion,
            string usuario,
            CxCCuentasFacturaCargaFilaNormalizada fila)
        {
            const string sqlRegistra = @"
                exec spCxC_Operacion_Factura_Registra
                    @Operacion,
                    @Factura,
                    @Divisa,
                    @Estado,
                    @Importe,
                    @TipoCambio,
                    @Monto,
                    @Adelanta,
                    @AdelantaTipo,
                    @AdelantaMonto,
                    @FechaEmite,
                    @FechaPago,
                    @Usuario,
                    'I',
                    0,
                    0,
                    0;";

            conn.Execute(sqlRegistra, new
            {
                Operacion = operacion,
                Factura = fila.factura,
                Divisa = fila.divisa,
                Estado = fila.factura_estado,
                Importe = fila.importe,
                TipoCambio = fila.tipo_cambio,
                Monto = fila.monto,
                Adelanta = fila.adelanta,
                AdelantaTipo = fila.adelanto_tipo,
                AdelantaMonto = fila.adelanto_monto,
                FechaEmite = fila.fecha_emite,
                FechaPago = fila.fecha_pago,
                Usuario = usuario
            });
        }

        /// <summary>
        /// Procesa un lote de facturas leído desde archivo y recalcula los totales de la operación.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos del lote de facturas.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_CargarArchivo(
            int codEmpresa,
            CxCCuentasFacturaCargaRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarCargaArchivoFactura(request, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible cargar el archivo de facturas.",
                conn =>
                {
                    foreach (var item in request.facturas)
                    {
                        var factura = NormalizarTexto(item.factura);
                        var divisa = NormalizarTexto(item.divisa);
                        var mensajeFila = ValidarFilaFacturaCarga(item, factura, divisa);

                        if (!string.IsNullOrWhiteSpace(mensajeFila))
                        {
                            return CrearErrorFacturaMantenimiento(mensajeFila);
                        }

                        var fila = NormalizarFilaFacturaCarga(item);
                        RegistrarFilaFacturaCarga(conn, request.operacion, usuario, fila);
                    }

                    const string sqlActualiza = @"
                        exec spCxC_Operacion_Facturas_Actualiza @operacion, 1;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sqlActualiza, new
                    {
                        operacion = request.operacion
                    });
                });
        }
        #endregion

        #region Activacion

        /// <summary>
        /// Obtiene el detalle o resumen de activación de una operación de CxC según la opción seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos de consulta del detalle de activación.</param>
        /// <returns>Detalle de activación y estado de tesorería.</returns>
        public ErrorDto<CxCCuentasActivacionDetalleResult> CxCCuentasActivacionDetalle_Obtener(
            int codEmpresa,
            CxCCuentasActivacionDetalleRequest request)
        {
            var response = new ErrorDto<CxCCuentasActivacionDetalleResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasActivacionDetalleResult()
            };

            if (request is null || request.operacion <= 0)
            {
                response.Code = -1;
                response.Description = CxCCuentasConstantes.operacionRequerida;
                response.Result = new CxCCuentasActivacionDetalleResult();
                return response;
            }

            var opcion = (request.opcion ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(opcion))
            {
                opcion = "RSM";
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sqlTesoreria = @"
            SELECT TOP 1
                ISNULL(C.Genera_Desembolso, 0) AS procesa_tesoreria
            FROM CxC_Cuentas R
            INNER JOIN CxC_Conceptos C
                ON R.cod_concepto = C.cod_concepto
            WHERE R.Operacion = @operacion;";

                response.Result.procesa_tesoreria = conn.QueryFirstOrDefault<bool>(sqlTesoreria, new
                {
                    operacion = request.operacion
                });

                switch (opcion)
                {
                    case "ING":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                    SELECT
                        ISNULL(R.cod_cargo, '') AS descripcion,
                        ISNULL(A.monto, 0) AS monto,
                        CONCAT(ISNULL(R.descripcion, ''), ' | ',
                               CASE WHEN ISNULL(A.tipo, '') = 'P' THEN 'Porcentual' ELSE 'Monto' END,
                               ' | Valor: ', CONVERT(varchar(50), ISNULL(A.valor, 0))) AS detalle
                    FROM CxC_Cargos R
                    INNER JOIN CxC_Cuentas_Ingresos A
                        ON R.cod_cargo = A.cod_cargo
                    WHERE A.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    case "CRD":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                    SELECT
                        CONVERT(varchar(50), Reb.id_solicitud) AS descripcion,
                        ISNULL(Reb.monto, 0) AS monto,
                        CONCAT(ISNULL(Cat.codigo, ''), ' | ', ISNULL(Cat.descripcion, '')) AS detalle
                    FROM CxC_Cuentas_Rebajos_Crd Reb
                    INNER JOIN Reg_Creditos Crd
                        ON Reb.id_solicitud = Crd.id_Solicitud
                    INNER JOIN catalogo Cat
                        ON Crd.codigo = Cat.codigo
                    WHERE Reb.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    case "CXC":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                    SELECT
                        CONVERT(varchar(50), R.Operacion_Aplicada) AS descripcion,
                        ISNULL(R.Monto, 0) AS monto,
                        CONCAT(ISNULL(Cta.cod_concepto, ''), ' | ', ISNULL(C.Descripcion, '')) AS detalle
                    FROM CxC_Cuentas_Rebajos R
                    INNER JOIN CxC_Cuentas Cta
                        ON R.Operacion_Aplicada = Cta.Operacion
                    INNER JOIN CxC_Conceptos C
                        ON Cta.cod_concepto = C.cod_concepto
                    WHERE R.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    case "CAR":
                        response.Result.lista = conn.Query<CxCCuentasActivacionDetalleItem>(@"
                    SELECT
                        ISNULL(R.cod_cargo, '') AS descripcion,
                        ISNULL(A.monto, 0) AS monto,
                        CONCAT(ISNULL(R.descripcion, ''), ' | ',
                               CASE WHEN ISNULL(A.tipo, '') = 'P' THEN 'Porcentual' ELSE 'Monto' END,
                               ' | Valor: ', CONVERT(varchar(50), ISNULL(A.valor, 0))) AS detalle
                    FROM CxC_Cargos R
                    INNER JOIN CxC_Cuentas_Rebajos_Cargos A
                        ON R.cod_cargo = A.cod_cargo
                    WHERE A.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        }).ToList();
                        break;

                    default:
                        var resumen = conn.QueryFirstOrDefault(@"
                    SELECT
                        C.Monto,
                        ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'CRD'), 0) AS Crd,
                        ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'CxC'), 0) AS CxC,
                        ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'CAR'), 0) AS Car,
                        ISNULL(dbo.fxCxC_CuentaRebajos(@operacion, 'ADL'), 0) AS Adl,
                        ISNULL(dbo.fxCxC_CuentaIngresos(@operacion), 0) AS Ing
                    FROM CxC_Cuentas C
                    WHERE C.Operacion = @operacion;", new
                        {
                            operacion = request.operacion
                        });

                        if (resumen is not null)
                        {
                            decimal monto = resumen.Monto ?? 0m;
                            decimal ing = resumen.Ing ?? 0m;
                            decimal crd = resumen.Crd ?? 0m;
                            decimal cxc = resumen.CxC ?? 0m;
                            decimal car = resumen.Car ?? 0m;
                            decimal adl = resumen.Adl ?? 0m;
                            decimal desembolsar = monto + ing - (crd + cxc + car + adl);

                            response.Result.lista = new List<CxCCuentasActivacionDetalleItem>
                    {
                        new() { descripcion = "Monto Aprobado", monto = monto, detalle = string.Empty },
                        new() { descripcion = "(+) Otros Ingresos", monto = ing, detalle = string.Empty },
                        new() { descripcion = "(-) Abonos a Créditos", monto = crd, detalle = string.Empty },
                        new() { descripcion = "(-) Abonos a CxC Pendientes", monto = cxc, detalle = string.Empty },
                        new() { descripcion = "(-) Cargos Registrados", monto = car, detalle = string.Empty },
                        new() { descripcion = "(-) Adelantos", monto = adl, detalle = string.Empty },
                        new() { descripcion = "Monto a Desembolsar", monto = desembolsar, detalle = string.Empty }
                    };
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar el detalle de activación. {ex.Message}";
                response.Result = new CxCCuentasActivacionDetalleResult();
            }

            return response;
        }

        #endregion

    }
}
