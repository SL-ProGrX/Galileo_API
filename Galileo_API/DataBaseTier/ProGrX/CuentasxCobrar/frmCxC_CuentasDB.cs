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
                return DbHelper.CreateErrorResponse<CxCCuentasBusquedaOperacionItem>("La operación es requerida.");
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
                return DbHelper.CreateErrorResponse<CxCCuentasConsultaData>("La operación es requerida.");
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
                response.Description = "La operación es requerida.";
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

            var response = new ErrorDto<CxCCuentasPersonasFiltroLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasPersonasFiltroLista()
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var texto = (filtros.filtro ?? string.Empty).Trim();
                var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
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
            ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlLista += CxCCuentasConstantes.paginacionSql;
                }

                var param = new
                {
                    filtro = string.IsNullOrWhiteSpace(texto) ? null : texto,
                    like,
                    offset,
                    fetch
                };

                response.Result.total = conn.QuerySingle<int>(sqlCount, param);
                response.Result.lista = conn.Query<CxCCuentasPersonasFiltroItem>(sqlLista, param).ToList();
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar las personas de CxC. {ex.Message}";
                response.Result = new CxCCuentasPersonasFiltroLista();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar las personas de CxC. {ex.Message}";
                response.Result = new CxCCuentasPersonasFiltroLista();
            }

            return response;
        }


        /// <summary>
        /// Obtiene una persona de CxC por cédula desde la vista de búsqueda.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula a consultar.</param>
        /// <returns>Registro encontrado de la vista vCxC_Personas_Filtro.</returns>
        public ErrorDto<CxCCuentasPersonasFiltroItem> CxCCuentasPersonaFiltroPorCedula_Obtener(int codEmpresa, string cedula)
        {
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();

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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasPersonasFiltroItem>(sql, new
                {
                    cedula = cedulaNormalizada
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasPersonasFiltroItem>("No se encontró la cédula.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result ?? new ErrorDto<CxCCuentasPersonasFiltroItem>();
        }


        /// <summary>
        /// Obtiene un concepto de CxC por código.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codConcepto">Código del concepto.</param>
        /// <returns>Datos del concepto.</returns>
        public ErrorDto<CxCCuentasConceptoData> CxCCuentasConcepto_Obtener(int codEmpresa, string codConcepto)
        {
            var codigoNormalizado = (codConcepto ?? string.Empty).Trim();

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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasConceptoData>(sql, new
                {
                    codConcepto = codigoNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasConceptoData>("No se encontró el concepto.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result ?? new ErrorDto<CxCCuentasConceptoData>();
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
            var codigoNormalizado = (codConcepto ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codigoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConceptosFiltroItem>("El concepto es requerido.");
            }

            if (tipo is not (0 or 1))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasConceptosFiltroItem>(CxCCuentasConstantes.scrollValido);
            }

            var sql = tipo == 1
                ? @"
            SELECT TOP 1
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            WHERE Activo = 1
              AND cod_Concepto < @codConcepto
            ORDER BY cod_Concepto DESC;"
                : @"
            SELECT TOP 1
                ISNULL(cod_Concepto, '') AS cod_concepto,
                ISNULL(Descripcion, '') AS descripcion
            FROM CxC_Conceptos
            WHERE Activo = 1
              AND cod_Concepto > @codConcepto
            ORDER BY cod_Concepto ASC;";

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasConceptosFiltroItem>(sql, new
                {
                    codConcepto = codigoNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasConceptosFiltroItem>("No hay más conceptos para navegar.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result ?? new ErrorDto<CxCCuentasConceptosFiltroItem>();
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

            var response = new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>()
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var texto = (filtros.filtro ?? string.Empty).Trim();
                var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
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
            ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlLista += CxCCuentasConstantes.paginacionSql;
                }

                var param = new
                {
                    filtro = string.IsNullOrWhiteSpace(texto) ? null : texto,
                    like,
                    offset,
                    fetch
                };

                response.Result.total = conn.QuerySingle<int>(sqlCount, param);
                response.Result.lista = conn.Query<CxCCuentasConceptosFiltroItem>(sqlLista, param).ToList();
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar conceptos. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar conceptos. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasConceptosFiltroItem>();
            }

            return response;
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
            var contratoNormalizado = (codContrato ?? string.Empty).Trim();
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();

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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasContratoData>(sql, new
                {
                    codContrato = contratoNormalizado,
                    cedula = cedulaNormalizada
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasContratoData>("No se encontró el contrato.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result!;
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
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var conceptoNormalizado = (codConcepto ?? string.Empty).Trim();
            var contratoNormalizado = (codContrato ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(conceptoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratosFiltroItem>("La cédula y el concepto son requeridos.");
            }

            if (tipo is not (0 or 1))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasContratosFiltroItem>(CxCCuentasConstantes.scrollValido);
            }

            var sql = tipo == 1
                ? @"
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
            ORDER BY Cn.cod_contrato DESC;"
                : @"
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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasContratosFiltroItem>(sql, new
                {
                    cedula = cedulaNormalizada,
                    codConcepto = conceptoNormalizado,
                    codContrato = contratoNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasContratosFiltroItem>("No hay más contratos para navegar.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result!;
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

            var response = new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>()
            };

            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var conceptoNormalizado = (codConcepto ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(conceptoNormalizado))
            {
                response.Code = -1;
                response.Description = "La cédula y el concepto son requeridos.";
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var texto = (filtros.filtro ?? string.Empty).Trim();
                var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
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
            ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlLista += CxCCuentasConstantes.paginacionSql;
                }

                var param = new
                {
                    cedula = cedulaNormalizada,
                    codConcepto = conceptoNormalizado,
                    filtro = string.IsNullOrWhiteSpace(texto) ? null : texto,
                    like,
                    offset,
                    fetch
                };

                response.Result.total = conn.QuerySingle<int>(sqlCount, param);
                response.Result.lista = conn.Query<CxCCuentasContratosFiltroItem>(sqlLista, param).ToList();
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar contratos. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar contratos. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasContratosFiltroItem>();
            }

            return response;
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
            var clienteNormalizado = (cedulaCliente ?? string.Empty).Trim();
            var contratoNormalizado = (codContrato ?? string.Empty).Trim();
            var pagadorNormalizado = (cedulaPagador ?? string.Empty).Trim();

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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasPagadorData>(sql, new
                {
                    cedulaCliente = clienteNormalizado,
                    codContrato = contratoNormalizado,
                    cedulaPagador = pagadorNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasPagadorData>("No se encontró el pagador.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result!;
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
            var clienteNormalizado = (cedulaCliente ?? string.Empty).Trim();
            var contratoNormalizado = (codContrato ?? string.Empty).Trim();
            var pagadorNormalizado = (cedulaPagador ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPagadoresFiltroItem>("Cliente y contrato son requeridos.");
            }

            if (tipo is not (0 or 1))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasPagadoresFiltroItem>(CxCCuentasConstantes.scrollValido);
            }

            var sql = tipo == 1
                ? @"
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
            ORDER BY Pg.Cedula_Pagador DESC;"
                : @"
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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasPagadoresFiltroItem>(sql, new
                {
                    cedulaCliente = clienteNormalizado,
                    codContrato = contratoNormalizado,
                    cedulaPagador = pagadorNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasPagadoresFiltroItem>("No hay más pagadores para navegar.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result!;
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

            var response = new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>()
            };

            var clienteNormalizado = (cedulaCliente ?? string.Empty).Trim();
            var contratoNormalizado = (codContrato ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(clienteNormalizado) || string.IsNullOrWhiteSpace(contratoNormalizado))
            {
                response.Code = -1;
                response.Description = "Cliente y contrato son requeridos.";
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var texto = (filtros.filtro ?? string.Empty).Trim();
                var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
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
            ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlLista += CxCCuentasConstantes.paginacionSql;
                }

                var param = new
                {
                    cedulaCliente = clienteNormalizado,
                    codContrato = contratoNormalizado,
                    filtro = string.IsNullOrWhiteSpace(texto) ? null : texto,
                    like,
                    offset,
                    fetch
                };

                response.Result.total = conn.QuerySingle<int>(sqlCount, param);
                response.Result.lista = conn.Query<CxCCuentasPagadoresFiltroItem>(sqlLista, param).ToList();
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar pagadores. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar pagadores. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasPagadoresFiltroItem>();
            }

            return response;
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
            var clienteNormalizado = (cedulaCliente ?? string.Empty).Trim();
            var autorizadoNormalizado = (cedulaAutorizado ?? string.Empty).Trim();

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

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasAutorizadoData>(sql, new
                {
                    cedulaCliente = clienteNormalizado,
                    cedulaAutorizado = autorizadoNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasAutorizadoData>("No se encontró el autorizado.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result!;
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
            var clienteNormalizado = (cedulaCliente ?? string.Empty).Trim();
            var autorizadoNormalizado = (cedulaAutorizado ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(clienteNormalizado))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAutorizadosFiltroItem>("El cliente es requerido.");
            }

            if (tipo is not (0 or 1))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAutorizadosFiltroItem>(CxCCuentasConstantes.scrollValido);
            }

            var sql = tipo == 1
                ? @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado < @cedulaAutorizado
            ORDER BY Pa.Cedula_Autorizado DESC;"
                : @"
            SELECT TOP 1
                ISNULL(Per.Cedula, '') AS cedula,
                ISNULL(Per.Nombre, '') AS nombre
            FROM CxC_Personas Per
            INNER JOIN CXC_PERSONAS_AUTORIZADOS Pa
                ON Per.Cedula = Pa.Cedula_Autorizado
            WHERE Pa.cedula = @cedulaCliente
              AND Pa.Cedula_Autorizado > @cedulaAutorizado
            ORDER BY Pa.Cedula_Autorizado ASC;";

            var response = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var data = conn.QueryFirstOrDefault<CxCCuentasAutorizadosFiltroItem>(sql, new
                {
                    cedulaCliente = clienteNormalizado,
                    cedulaAutorizado = autorizadoNormalizado
                });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasAutorizadosFiltroItem>("No hay más autorizados para navegar.");
                }

                return DbHelper.CreateOkResponse(data);
            });

            return response.Result!;
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

            var response = new ErrorDto<CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>()
            };

            var clienteNormalizado = (cedulaCliente ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(clienteNormalizado))
            {
                response.Code = -1;
                response.Description = "El cliente es requerido.";
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var texto = (filtros.filtro ?? string.Empty).Trim();
                var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
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
            ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlLista += CxCCuentasConstantes.paginacionSql;
                }

                var param = new
                {
                    cedulaCliente = clienteNormalizado,
                    filtro = string.IsNullOrWhiteSpace(texto) ? null : texto,
                    like,
                    offset,
                    fetch
                };

                response.Result.total = conn.QuerySingle<int>(sqlCount, param);
                response.Result.lista = conn.Query<CxCCuentasAutorizadosFiltroItem>(sqlLista, param).ToList();
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible consultar autorizados. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al consultar autorizados. {ex.Message}";
                response.Result = new CxCCuentasBusquedaGenericaLista<CxCCuentasAutorizadosFiltroItem>();
            }

            return response;
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
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var bancoNormalizado = (banco ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(cedulaNormalizada) || string.IsNullOrWhiteSpace(bancoNormalizado))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>("La cédula y el banco son requeridos.");
            }

            const string sql = @"exec spSys_Cuentas_Bancarias @Identificacion, @BancoId, 1;";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    Identificacion = cedulaNormalizada,
                    BancoId = bancoNormalizado
                }).ToList();

                return lista;
            });
        }

        #endregion
    }
}
