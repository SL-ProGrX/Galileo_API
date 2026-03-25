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
