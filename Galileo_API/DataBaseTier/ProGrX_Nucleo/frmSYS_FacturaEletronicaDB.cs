using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using PgxAPI.Models.ProGrX_Nucleo;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysFacturaEletronicaDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string CodClienteKey = "cod_cliente";
        private const string IdentificacionKey = "identificacion";
        private const string NombreKey = "nombre";
        private const string FacturaKey = "factura";
        private const string FechaInicioKey = "fecha_inicio";
        private const string FechaCorteKey = "fecha_corte";
        private const string EstadoKey = "estado";
        private const string DateFormat = "yyyy-MM-dd";

        public FrmSysFacturaEletronicaDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }
        

        /// <summary>
        /// Lista clientes para Facturación Electrónica (SYS_FE_PARAMETROS).
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Clientes_DropDown_Obtener(int CodEmpresa)
        {
            var connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = new SqlConnection(connStr);

                const string query = @"
                SELECT
                    RTRIM(COD_CLIENTE)  AS item,
                    RTRIM(RAZON_SOCIAL) AS descripcion
                FROM SYS_FE_PARAMETROS
                ORDER BY RAZON_SOCIAL;";

                resp.Result = cn.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
        
        
        /// <summary>
        /// Lista cortes realizados por cliente (SYS_FE_CLIENTE_CORTES) con lazy load.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<FeCortesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new FeCortesLista
                {
                    total = 0,
                    lista = new List<FeCorteItem>()
                }
            };

            try
            {
                string codCliente = GetCodClienteFromFiltros(filtros);

                if (string.IsNullOrWhiteSpace(codCliente))
                    return SetError(result, "parametros.cod_cliente es requerido para listar cortes.");

                var (paginacion, exportAll, offset) = GetPagination(filtros);

                var (orderByCol, orderDir) = GetOrderBy(filtros);
                string orderBy = $"ORDER BY {orderByCol} {orderDir}";

                string filtroGlobal = (filtros?.filtro ?? "").Trim();
                string where = BuildWhereClause(filtroGlobal);

                string sqlCount = $@"
                SELECT COUNT(1)
                FROM SYS_FE_CLIENTE_CORTES
                {where};
                ";

                string sqlData = $@"
                SELECT
                    CORTE_ID as corte_id,
                    CORTE as corte,
                    FACTURACION as facturacion,
                    CASE WHEN METODO_BASE = 'D' THEN 'Devengado' ELSE 'Efectivo' END as metodo,
                    REGISTRO_USUARIO as reg_usuario,
                    REGISTRO_FECHA as reg_fecha
                FROM SYS_FE_CLIENTE_CORTES
                {where}
                {orderBy}
                ";

                if (!exportAll)
                {
                    sqlData += @"
                OFFSET @OFFSET ROWS
                FETCH NEXT @FETCH ROWS ONLY
                ";
                }

                using var connection = new SqlConnection(stringConn);

                var p = BuildParameters(codCliente, filtroGlobal, exportAll, offset, paginacion);

                result.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, p);
                result.Result.lista = connection.Query<FeCorteItem>(sqlData, p).ToList();

                return result;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                if (result.Result == null)
                {
                    result.Result = new FeCortesLista
                    {
                        total = 0,
                        lista = new List<FeCorteItem>()
                    };
                }
                else
                {
                    result.Result.total = 0;
                    result.Result.lista = new List<FeCorteItem>();
                }
                return result;
            }
        }


        /// <summary>
        /// Obtiene cod_cliente desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static string GetCodClienteFromFiltros(FiltrosLazyLoadData filtros)
        {
            string? codCliente = null;
            if (filtros?.parametros is IDictionary<string, object> dictObj)
            {
                if (dictObj.TryGetValue(CodClienteKey, out var v) && v != null)
                    codCliente = v.ToString();
            }
            else if (filtros?.parametros is IDictionary<string, string> dictStr)
            {
                dictStr.TryGetValue(CodClienteKey, out codCliente);
            }
            return (codCliente ?? "").Trim();
        }


        /// <summary>
        ///     Obtiene parámetros de paginación desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static (int paginacion, bool exportAll, int offset) GetPagination(FiltrosLazyLoadData filtros)
        {
            int pagina = filtros?.pagina ?? 1;
            int paginacion = filtros?.paginacion ?? 30;
            bool exportAll = (pagina == 0 || paginacion == 0);

            if (!exportAll)
            {
                if (pagina < 1) pagina = 1;
                if (paginacion < 1) paginacion = 30;
            }

            int offset = exportAll ? 0 : (pagina - 1) * paginacion;
            return (paginacion, exportAll, offset);
        }


        /// <summary>
        ///     Obtiene columna y dirección de ordenamiento desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static (string orderByCol, string orderDir) GetOrderBy(FiltrosLazyLoadData filtros)
        {
            int sortOrder = filtros?.sortOrder ?? 0;
            string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();

            string orderByCol = "CORTE";
            string orderDir = "DESC";

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                switch (sortField)
                {
                    case "corte_id": orderByCol = "CORTE_ID"; break;
                    case "corte": orderByCol = "CORTE"; break;
                    case "facturacion": orderByCol = "FACTURACION"; break;
                    case "metodo":
                    case "metodo_base": orderByCol = "METODO_BASE"; break;
                    case "reg_usuario":
                    case "registro_usuario": orderByCol = "REGISTRO_USUARIO"; break;
                    case "reg_fecha":
                    case "registro_fecha": orderByCol = "REGISTRO_FECHA"; break;
                    default: orderByCol = "CORTE"; break;
                }
                orderDir = (sortOrder == 1) ? "ASC" : "DESC";
            }
            return (orderByCol, orderDir);
        }

        
        /// <summary>
        ///     Construye cláusula WHERE para consulta de cortes.
        /// </summary>
        /// <param name="filtroGlobal"></param>
        /// <returns></returns>
        private static string BuildWhereClause(string filtroGlobal)
        {
            string where = @"
                WHERE COD_CLIENTE = @COD_CLIENTE
                ";
            if (!string.IsNullOrWhiteSpace(filtroGlobal))
            {
                where += @"
                AND (
                    REGISTRO_USUARIO LIKE @FILTRO
                    OR CAST(CORTE_ID AS varchar(20)) LIKE @FILTRO
                )
                ";
            }
            return where;
        }


        private static DynamicParameters BuildParameters(string codCliente, string filtroGlobal, bool exportAll, int offset, int paginacion)
        {
            var p = new DynamicParameters();
            p.Add("@COD_CLIENTE", codCliente, DbType.String);

            if (!string.IsNullOrWhiteSpace(filtroGlobal))
                p.Add("@FILTRO", $"%{filtroGlobal}%", DbType.String);

            if (!exportAll)
            {
                p.Add("@OFFSET", offset, DbType.Int32);
                p.Add("@FETCH", paginacion, DbType.Int32);
            }
            return p;
        }


        /// <summary>
        ///   Establece error en resultado de lista de cortes.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        private static ErrorDto<FeCortesLista> SetError(ErrorDto<FeCortesLista> result, string description)
        {
            result.Code = -1;
            result.Description = description;
            if (result.Result == null)
            {
                result.Result = new FeCortesLista
                {
                    total = 0,
                    lista = new List<FeCorteItem>()
                };
            }
            else
            {
                result.Result.total = 0;
                result.Result.lista = new List<FeCorteItem>();
            }
            return result;
        }
        
        
        /// <summary>
        /// Registra/Reprocesa corte usando spCrd_Facturacion_Corte.
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto FE_Corte_Registrar(int CodEmpresa, FeRegistrarCorteDto dto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                if (string.IsNullOrWhiteSpace(dto.cod_cliente))
                {
                    result.Code = -1;
                    result.Description = "cod_cliente es requerido.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(dto.usuario))
                {
                    result.Code = -1;
                    result.Description = "usuario es requerido.";
                    return result;
                }

                if (!DateTime.TryParseExact(
                        (dto.fecha_corte ?? "").Trim(),
                        DateFormat,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var fechaCorte))
                {
                    result.Code = -1;
                    result.Description = "fecha_corte inválida. Formato esperado: YYYY-MM-DD.";
                    return result;
                }

                if (!DateTime.TryParseExact(
                        (dto.fecha_factura ?? "").Trim(),
                        DateFormat,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var fechaFactura))
                {
                    result.Code = -1;
                    result.Description = "fecha_factura inválida. Formato esperado: YYYY-MM-DD.";
                    return result;
                }

                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                p.Add("@Cliente", dto.cod_cliente, DbType.String);
                p.Add("@Corte", fechaCorte, DbType.DateTime);
                p.Add("@Usuario", dto.usuario, DbType.String);
                p.Add("@FechaFactura", fechaFactura, DbType.DateTime);

                connection.Execute("spCrd_Facturacion_Corte", p, commandType: CommandType.StoredProcedure);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = dto.usuario,
                    DetalleMovimiento = $"FE Corte Cliente: {dto.cod_cliente} Corte: {dto.fecha_corte} Factura: {dto.fecha_factura}",
                    Movimiento = "Registra Corte - WEB",
                    Modulo = vModulo
                });

                return result;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                return result;
            }
        }


        /// <summary>
        /// Lista facturas (Detalle) usando spProGrX_Facturas_Consulta.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDto<FeFacturasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new FeFacturasLista
                {
                    total = 0,
                    lista = new List<FeFacturaItem>()
                }
            };

            try
            {
                var parametros = ExtractFacturaParametros(filtros);

                var validationError = ValidateFacturaParametros(parametros);
                if (validationError != null)
                {
                    result.Code = -1;
                    result.Description = validationError;
                    return result;
                }

                var (ini, fin) = GetFechas(parametros.fechaInicioStr, parametros.fechaCorteStr);

                var (paginacion, exportAll, offset) = GetPagination(filtros);

                using var connection = new SqlConnection(stringConn);

                var p = BuildFacturaParameters(parametros, ini, fin);

                var data = connection.Query<FeFacturaItem>(
                    "spProGrX_Facturas_Consulta",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                SortFacturaData(data, filtros);

                result.Result.total = data.Count;
                result.Result.lista = PaginateFacturaData(data, paginacion, offset, exportAll);

                return result;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<FeFacturaItem>();
                return result;
            }
        }


        /// <summary>
        /// Extrae parámetros de factura desde filtros.
        /// </summary>
        private sealed class FacturaParametros
        {
            public string codCliente = "";
            public string identificacion = "";
            public string nombre = "";
            public string factura = "";
            public string fechaInicioStr = "";
            public string fechaCorteStr = "";
            public string estado = "T";
        }


        /// <summary>
        /// Obtiene parámetros de factura desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static FacturaParametros ExtractFacturaParametros(FiltrosLazyLoadData filtros)
        {
            var parametros = new FacturaParametros();

            if (filtros?.parametros is IDictionary<string, object> dictObj)
            {
                SetFacturaParametrosFromObjectDict(parametros, dictObj);
            }
            else if (filtros?.parametros is IDictionary<string, string> dictStr)
            {
                SetFacturaParametrosFromStringDict(parametros, dictStr);
            }

            return parametros;
        }


        /// <summary>
        ///    Establece parámetros de factura desde diccionario de objetos.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="dictObj"></param>
        private static void SetFacturaParametrosFromObjectDict(FacturaParametros parametros, IDictionary<string, object> dictObj)
        {
            parametros.codCliente = GetTrimmedValue(dictObj, CodClienteKey);
            parametros.identificacion = GetTrimmedValue(dictObj, IdentificacionKey);
            parametros.nombre = GetTrimmedValue(dictObj, NombreKey);
            parametros.factura = GetTrimmedValue(dictObj, FacturaKey);
            parametros.fechaInicioStr = GetTrimmedValue(dictObj, FechaInicioKey);
            parametros.fechaCorteStr = GetTrimmedValue(dictObj, FechaCorteKey);
            parametros.estado = GetTrimmedValue(dictObj, EstadoKey, "T");
        }


        /// <summary>
        ///   Establece parámetros de factura desde diccionario de strings.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="dictStr"></param>
        private static void SetFacturaParametrosFromStringDict(FacturaParametros parametros, IDictionary<string, string> dictStr)
        {
            parametros.codCliente = GetTrimmedValue(dictStr, CodClienteKey);
            parametros.identificacion = GetTrimmedValue(dictStr, IdentificacionKey);
            parametros.nombre = GetTrimmedValue(dictStr, NombreKey);
            parametros.factura = GetTrimmedValue(dictStr, FacturaKey);
            parametros.fechaInicioStr = GetTrimmedValue(dictStr, FechaInicioKey);
            parametros.fechaCorteStr = GetTrimmedValue(dictStr, FechaCorteKey);
            parametros.estado = GetTrimmedValue(dictStr, EstadoKey, "T");
        }


        /// <summary>
        ///   Obtiene valor recortado desde diccionario de objetos.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetTrimmedValue(IDictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict.TryGetValue(key, out var value) && value != null)
                return value.ToString()?.Trim() ?? defaultValue;
            return defaultValue;
        }


        /// <summary>
        ///  Obtiene valor recortado desde diccionario de strings.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetTrimmedValue(IDictionary<string, string> dict, string key, string defaultValue = "")
        {
            if (dict.TryGetValue(key, out var value) && value != null)
                return value.Trim();
            return defaultValue;
        }


        /// <summary>
        ///  Valida parámetros de factura.
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        private static string? ValidateFacturaParametros(FacturaParametros parametros)
        {
            if (string.IsNullOrWhiteSpace(parametros.codCliente))
                return "parametros.cod_cliente es requerido.";

            if (!DateTime.TryParseExact(
                    parametros.fechaInicioStr,
                    DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
                return "parametros.fecha_inicio inválida. Formato esperado: YYYY-MM-DD.";

            if (!DateTime.TryParseExact(
                    parametros.fechaCorteStr,
                    DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
                return "parametros.fecha_corte inválida. Formato esperado: YYYY-MM-DD.";

            return null;
        }


        /// <summary>
        ///  Construye parámetros de factura para spProGrX_Facturas_Consulta.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="ini"></param>
        /// <param name="fin"></param>
        /// <returns></returns>
        private static DynamicParameters BuildFacturaParameters(FacturaParametros parametros, DateTime ini, DateTime fin)
        {
            var p = new DynamicParameters();
            p.Add("@CodCliente", parametros.codCliente, DbType.String);
            p.Add("@FiltroFactura", parametros.factura ?? "", DbType.String);
            p.Add("@FiltroId", parametros.identificacion ?? "", DbType.String);
            p.Add("@FiltroRazonSocial", parametros.nombre ?? "", DbType.String);
            p.Add("@Inicio", ini, DbType.DateTime);
            p.Add("@Corte", fin, DbType.DateTime);
            p.Add("@Estado", string.IsNullOrWhiteSpace(parametros.estado) ? "T" : parametros.estado.Substring(0, 1), DbType.String);
            return p;
        }


        /// <summary>
        ///   Ordena lista de facturas según filtros.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filtros"></param>
        private static void SortFacturaData(List<FeFacturaItem> data, FiltrosLazyLoadData filtros)
        {
            int sortOrder = filtros?.sortOrder ?? 0;
            string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                data.Sort((a, b) =>
                {
                    int dir = (sortOrder == 1) ? 1 : -1;

                    switch (sortField)
                    {
                        case "tipo":
                            return dir * string.Compare(a.tipo ?? "", b.tipo ?? "", StringComparison.OrdinalIgnoreCase);

                        case "comprobante":
                            return dir * string.Compare(a.comprobante ?? "", b.comprobante ?? "", StringComparison.OrdinalIgnoreCase);

                        case IdentificacionKey:
                            return dir * string.Compare(a.identificacion ?? "", b.identificacion ?? "", StringComparison.OrdinalIgnoreCase);

                        case "razon_social":
                        case NombreKey:
                            return dir * string.Compare(a.razon_social ?? "", b.razon_social ?? "", StringComparison.OrdinalIgnoreCase);

                        case "fecha":
                            return dir * Nullable.Compare(a.fecha, b.fecha);

                        case "total":
                            return dir * a.total.CompareTo(b.total);

                        case "total_exento":
                            return dir * a.total_exento.CompareTo(b.total_exento);

                        case "total_gravado":
                            return dir * a.total_gravado.CompareTo(b.total_gravado);

                        case "total_impuestos":
                            return dir * a.total_impuestos.CompareTo(b.total_impuestos);

                        case "total_descuentos":
                            return dir * a.total_descuentos.CompareTo(b.total_descuentos);

                        case "total_comprobante":
                            return dir * a.total_comprobante.CompareTo(b.total_comprobante);

                        default:
                            return dir * Nullable.Compare(b.fecha, a.fecha);
                    }
                });
            }
            else
            {
                data.Sort((a, b) => Nullable.Compare(b.fecha, a.fecha));
            }
        }


        /// <summary>
        ///  Pagina lista de facturas según parámetros.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="paginacion"></param>
        /// <param name="offset"></param>
        /// <param name="exportAll"></param>
        /// <returns></returns>
        private static List<FeFacturaItem> PaginateFacturaData(List<FeFacturaItem> data, int paginacion, int offset, bool exportAll)
        {
            if (exportAll)
                return data;

            var page = new List<FeFacturaItem>(paginacion);
            int start = offset;
            int end = offset + paginacion;

            for (int i = 0; i < data.Count; i++)
            {
                if (i < start) continue;
                if (i >= end) break;
                page.Add(data[i]);
            }

            return page;
        }


        /// <summary>
        /// Obtiene el detalle de líneas de una factura usando spProGrX_Factura_Detalle.
        /// <param name="CodEmpresa"></param>
        /// <param name="codCliente"></param>
        /// <param name="idFactura"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<FeFacturaDetalleItem>> FE_Factura_Detalle_Obtener(int CodEmpresa, string codCliente, int idFactura)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<List<FeFacturaDetalleItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FeFacturaDetalleItem>()
            };

            try
            {
                if (string.IsNullOrWhiteSpace(codCliente))
                {
                    resp.Code = -1;
                    resp.Description = "codCliente es requerido.";
                    resp.Result = null;
                    return resp;
                }

                if (idFactura <= 0)
                {
                    resp.Code = -1;
                    resp.Description = "idFactura inválido.";
                    resp.Result = null;
                    return resp;
                }

                using var cn = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                p.Add("@CodCliente", codCliente.Trim(), DbType.String);
                p.Add("@IdFactura", idFactura, DbType.Int32);

                resp.Result = cn.Query<FeFacturaDetalleItem>(
                    "spProGrX_Factura_Detalle",
                    p,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return resp;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
                return resp;
            }
        }


        /// <summary>
        /// Obtiene el resumen (cabecera + lista) usando spProGrX_Facturas_Consulta_Rsm.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<FeFacturasResumen>
            {
                Code = 0,
                Description = "Ok",
                Result = new FeFacturasResumen()
                {
                    cabecera = new FeFacturasResumenCabecera(),
                    lista = new List<FeFacturaResumenItem>()
                }
            };

            try
            {
                var parametros = ParseResumenParametros(filtros);

                var validationError = ValidateResumenParametros(parametros);
                if (validationError != null)
                {
                    resp.Code = -1;
                    resp.Description = validationError;
                    return resp;
                }

                var (ini, fin) = GetFechas(parametros.fechaInicioStr, parametros.fechaCorteStr);

                using var cn = new SqlConnection(stringConn);

                var pBase = BuildResumenParameters(parametros, ini, fin);

                resp.Result.cabecera = GetResumenCabecera(cn, pBase);
                resp.Result.lista = GetResumenLista(cn, pBase, filtros);

                return resp;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
                return resp;
            }
        }


        /// <summary>
        /// Obtiene lista de resumen de facturas
        /// </summary>
        private sealed class ResumenParametros
        {
            public string codCliente = "";
            public string identificacion = "";
            public string nombre = "";
            public string factura = "";
            public string fechaInicioStr = "";
            public string fechaCorteStr = "";
            public string estado = "T";
        }


        /// <summary>
        /// Obtiene parámetros de resumen desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static ResumenParametros ParseResumenParametros(FiltrosLazyLoadData filtros)
        {
            var parametros = new ResumenParametros();

            if (filtros?.parametros is IDictionary<string, object> dictObj)
            {
                parametros.codCliente = GetTrimmedValue(dictObj, CodClienteKey);
                parametros.identificacion = GetTrimmedValue(dictObj, IdentificacionKey);
                parametros.nombre = GetTrimmedValue(dictObj, NombreKey);
                parametros.factura = GetTrimmedValue(dictObj, FacturaKey);
                parametros.fechaInicioStr = GetTrimmedValue(dictObj, FechaInicioKey);
                parametros.fechaCorteStr = GetTrimmedValue(dictObj, FechaCorteKey);
                parametros.estado = GetTrimmedValue(dictObj, EstadoKey, "T");
            }
            else if (filtros?.parametros is IDictionary<string, string> dictStr)
            {
                parametros.codCliente = GetTrimmedValue(dictStr, CodClienteKey);
                parametros.identificacion = GetTrimmedValue(dictStr, IdentificacionKey);
                parametros.nombre = GetTrimmedValue(dictStr, NombreKey);
                parametros.factura = GetTrimmedValue(dictStr, FacturaKey);
                parametros.fechaInicioStr = GetTrimmedValue(dictStr, FechaInicioKey);
                parametros.fechaCorteStr = GetTrimmedValue(dictStr, FechaCorteKey);
                parametros.estado = GetTrimmedValue(dictStr, EstadoKey, "T");
            }

            return parametros;
        }


        /// <summary>
        /// Valida parámetros de resumen.
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        private static string? ValidateResumenParametros(ResumenParametros parametros)
        {
            if (string.IsNullOrWhiteSpace(parametros.codCliente))
                return "parametros.cod_cliente es requerido.";

            if (!DateTime.TryParseExact(
                    parametros.fechaInicioStr,
                    DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
                return "parametros.fecha_inicio inválida. Formato esperado: YYYY-MM-DD.";

            if (!DateTime.TryParseExact(
                    parametros.fechaCorteStr,
                    DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _))
                return "parametros.fecha_corte inválida. Formato esperado: YYYY-MM-DD.";

            return null;
        }


        /// <summary>
        /// Obtiene fechas de inicio y corte desde cadenas.
        /// </summary>
        /// <param name="fechaInicioStr"></param>
        /// <param name="fechaCorteStr"></param>
        /// <returns></returns>
        private static (DateTime ini, DateTime fin) GetFechas(string fechaInicioStr, string fechaCorteStr)
        {
            DateTime.TryParseExact(
                fechaInicioStr,
                DateFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var fechaInicio);

            DateTime.TryParseExact(
                fechaCorteStr,
                DateFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var fechaCorte);

            var ini = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var fin = new DateTime(fechaCorte.Year, fechaCorte.Month, fechaCorte.Day, 23, 59, 59, DateTimeKind.Unspecified);

            return (ini, fin);
        }


        /// <summary>
        /// Construye parámetros de resumen para spProGrX_Facturas_Consulta_Rsm.
        /// </summary>
        /// <param name="parametros"></param>
        /// <param name="ini"></param>
        /// <param name="fin"></param>
        /// <returns></returns>
        private static DynamicParameters BuildResumenParameters(ResumenParametros parametros, DateTime ini, DateTime fin)
        {
            var p = new DynamicParameters();
            p.Add("@CodCliente", parametros.codCliente, DbType.String);
            p.Add("@FiltroFactura", parametros.factura ?? "", DbType.String);
            p.Add("@FiltroId", parametros.identificacion ?? "", DbType.String);
            p.Add("@FiltroRazonSocial", parametros.nombre ?? "", DbType.String);
            p.Add("@Inicio", ini, DbType.DateTime);
            p.Add("@Corte", fin, DbType.DateTime);
            p.Add("@Estado", string.IsNullOrWhiteSpace(parametros.estado) ? "T" : parametros.estado.Substring(0, 1), DbType.String);
            return p;
        }


        /// <summary>
        /// Obtiene la cabecera del resumen de facturas.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="pBase"></param>
        /// <returns></returns>
        private static FeFacturasResumenCabecera GetResumenCabecera(SqlConnection cn, DynamicParameters pBase)
        {
            var pR = new DynamicParameters(pBase);
            pR.Add("@Tipo", "R", DbType.String);

            var head = cn.QueryFirstOrDefault<dynamic>(
                "spProGrX_Facturas_Consulta_Rsm",
                pR,
                commandType: CommandType.StoredProcedure
            );

            var cabecera = new FeFacturasResumenCabecera();
            if (head != null)
            {
                try
                {
                    cabecera.no_facturas = head.Facturas == null ? 0 : (int)head.Facturas;
                    cabecera.inicio = head.Inicio == null ? (DateTime?)null : (DateTime)head.Inicio;
                    cabecera.corte = head.Corte == null ? (DateTime?)null : (DateTime)head.Corte;
                    cabecera.monto_facturado = head.Total_Venta == null ? 0m : (decimal)head.Total_Venta;
                }
                catch
                {
                    cabecera.no_facturas = 0;
                    cabecera.inicio = null;
                    cabecera.corte = null;
                    cabecera.monto_facturado = 0m;
                }
            }
            return cabecera;
        }


        /// <summary>
        /// Obtiene la lista del resumen de facturas.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="pBase"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static List<FeFacturaResumenItem> GetResumenLista(SqlConnection cn, DynamicParameters pBase, FiltrosLazyLoadData filtros)
        {
            var pD = new DynamicParameters(pBase);
            pD.Add("@Tipo", "D", DbType.String);

            var lista = cn.Query<FeFacturaResumenItem>(
                "spProGrX_Facturas_Consulta_Rsm",
                pD,
                commandType: CommandType.StoredProcedure
            ).ToList();

            int sortOrder = filtros?.sortOrder ?? 0;
            string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                lista.Sort((a, b) =>
                {
                    int dir = (sortOrder == 1) ? 1 : -1;

                    switch (sortField)
                    {
                        case "tipo":
                            return dir * string.Compare(a.tipo ?? "", b.tipo ?? "", StringComparison.OrdinalIgnoreCase);

                        case "lineas":
                            return dir * a.lineas.CompareTo(b.lineas);

                        case "detalle":
                            return dir * string.Compare(a.detalle ?? "", b.detalle ?? "", StringComparison.OrdinalIgnoreCase);

                        case "facturado":
                            return dir * a.facturado.CompareTo(b.facturado);

                        default:
                            return dir * a.facturado.CompareTo(b.facturado);
                    }
                });
            }
            else
            {
                lista.Sort((a, b) => b.facturado.CompareTo(a.facturado));
            }

            return lista;
        }


        /// <summary>
        /// Obtiene cliente por identificación (SYS_FE_CLIENTES).
        /// <param name="CodEmpresa"></param>
        /// <param name="codCliente"></param>
        /// <param name="identificacion"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<FeClienteInfo> FE_Cliente_PorIdentificacion_Obtener(int CodEmpresa, string codCliente, string identificacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<FeClienteInfo>
            {
                Code = 0,
                Description = "Ok",
                Result = new FeClienteInfo()
            };

            try
            {
                if (string.IsNullOrWhiteSpace(codCliente))
                {
                    resp.Code = -1;
                    resp.Description = "codCliente es requerido.";
                    resp.Result = null;
                    return resp;
                }

                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    resp.Code = -1;
                    resp.Description = $"{IdentificacionKey} es requerida.";
                    resp.Result = null;
                    return resp;
                }

                using var cn = new SqlConnection(stringConn);

                const string q = @"
                SELECT TOP 1
                    RTRIM(CEDULA) AS identificacion,
                    RTRIM(NOMBRE) AS nombre
                FROM SYS_FE_CLIENTES
                WHERE COD_CLIENTE = @COD_CLIENTE
                  AND RTRIM(CEDULA) = RTRIM(@CEDULA)
                ORDER BY CLIENTE_ID;";

                resp.Result = cn.QueryFirstOrDefault<FeClienteInfo>(
                    q,
                    new { COD_CLIENTE = codCliente.Trim(), CEDULA = identificacion.Trim() }
                );

                if (resp.Result == null)
                {
                    resp.Result = new FeClienteInfo();
                }

                return resp;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
                return resp;
            }
        }
        
        
        /// <summary>
        /// Lista clientes (SYS_FE_CLIENTES) por cod_cliente, con filtros simples.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDto<FeClientesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new FeClientesLista { total = 0, lista = new List<FeClienteItem>() }
            };

            try
            {
                var parametros = ExtractClienteParametros(filtros);

                if (string.IsNullOrWhiteSpace(parametros.codCliente))
                    return SetClienteError(resp, "parametros.cod_cliente es requerido.");

                var (_, paginacion, exportAll, offset) = GetClientePagination(filtros);

                using var cn = new SqlConnection(stringConn);

                var (where, p) = BuildClienteWhereAndParams(parametros);

                var data = cn.Query<FeClienteItem>(BuildClienteQuery(where), p).ToList();

                SortClienteData(data, filtros);

                resp.Result.total = data.Count;
                resp.Result.lista = PaginateClienteData(data, paginacion, offset, exportAll);

                return resp;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
                return resp;
            }
        }


        /// <summary>
        /// Establece error en respuesta de cliente.
        /// </summary>
        private sealed class ClienteParametros
        {
            public string codCliente = "";
            public string identificacion = "";
            public string nombre = "";
        }


        /// <summary>
        /// Obtiene parámetros de cliente desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static ClienteParametros ExtractClienteParametros(FiltrosLazyLoadData filtros)
        {
            var parametros = new ClienteParametros();

            if (filtros?.parametros is IDictionary<string, object> dictObj)
            {
                parametros.codCliente = GetTrimmedValue(dictObj, CodClienteKey);
                parametros.identificacion = GetTrimmedValue(dictObj, IdentificacionKey);
                parametros.nombre = GetTrimmedValue(dictObj, NombreKey);
            }
            else if (filtros?.parametros is IDictionary<string, string> dictStr)
            {
                parametros.codCliente = GetTrimmedValue(dictStr, CodClienteKey);
                parametros.identificacion = GetTrimmedValue(dictStr, IdentificacionKey);
                parametros.nombre = GetTrimmedValue(dictStr, NombreKey);
            }

            return parametros;
        }


        /// <summary>
        /// Obtiene paginación de cliente desde filtros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static (int pagina, int paginacion, bool exportAll, int offset) GetClientePagination(FiltrosLazyLoadData filtros)
        {
            int pagina = filtros?.pagina ?? 1;
            int paginacion = filtros?.paginacion ?? 30;
            bool exportAll = (pagina == 0 || paginacion == 0);

            if (!exportAll)
            {
                if (pagina < 1) pagina = 1;
                if (paginacion < 1) paginacion = 30;
            }

            int offset = exportAll ? 0 : (pagina - 1) * paginacion;
            return (pagina, paginacion, exportAll, offset);
        }


        /// <summary>
        /// Construye cláusula WHERE y parámetros para consulta de cliente.
        /// </summary>
        /// <param name="parametros"></param>
        /// <returns></returns>
        private static (string where, DynamicParameters p) BuildClienteWhereAndParams(ClienteParametros parametros)
        {
            string where = @"
                WHERE COD_CLIENTE = @COD_CLIENTE
                ";
            var p = new DynamicParameters();
            p.Add("@COD_CLIENTE", parametros.codCliente, DbType.String);

            if (!string.IsNullOrWhiteSpace(parametros.identificacion))
            {
                where += " AND RTRIM(CEDULA) LIKE @CEDULA ";
                p.Add("@CEDULA", $"%{parametros.identificacion}%", DbType.String);
            }

            if (!string.IsNullOrWhiteSpace(parametros.nombre))
            {
                where += " AND RTRIM(NOMBRE) LIKE @NOMBRE ";
                p.Add("@NOMBRE", $"%{parametros.nombre}%", DbType.String);
            }

            return (where, p);
        }


        /// <summary>
        /// Construye consulta de cliente.
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        private static string BuildClienteQuery(string where)
        {
            return $@"
                SELECT
                    RTRIM(CLIENTE_ID_FE) AS id_prov,
                    RTRIM(TIPO_ID)       AS tipo_id,
                    RTRIM(CEDULA)        AS identificacion,
                    RTRIM(NOMBRE)        AS razon_social,
                    RTRIM(EMAIL1)        AS email1,
                    RTRIM(EMAIL2)        AS email2,
                    RTRIM(TELEFONO1)     AS telefono1,
                    RTRIM(TELEFONO2)     AS telefono2,
                    RTRIM(PROVINCIA)     AS provincia,
                    RTRIM(CANTON)        AS canton,
                    RTRIM(DISTRITO)      AS distrito,
                    RTRIM(BARRIO)        AS barrio,
                    RTRIM(DIRECCION)     AS direccion
                FROM SYS_FE_CLIENTES
                {where};
                ";
        }


        /// <summary>
        /// Ordena lista de clientes según filtros.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filtros"></param>
        private static void SortClienteData(List<FeClienteItem> data, FiltrosLazyLoadData filtros)
        {
            int sortOrder = filtros?.sortOrder ?? 0;
            string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(sortField))
            {
                data.Sort((a, b) =>
                {
                    int dir = (sortOrder == 1) ? 1 : -1;

                    switch (sortField)
                    {
                        case "id_prov":
                            return dir * string.Compare(a.id_prov ?? "", b.id_prov ?? "", StringComparison.OrdinalIgnoreCase);

                        case "tipo_id":
                            return dir * string.Compare(a.tipo_id ?? "", b.tipo_id ?? "", StringComparison.OrdinalIgnoreCase);

                        case IdentificacionKey:
                            return dir * string.Compare(a.identificacion ?? "", b.identificacion ?? "", StringComparison.OrdinalIgnoreCase);

                        case "razon_social":
                        case NombreKey:
                            return dir * string.Compare(a.razon_social ?? "", b.razon_social ?? "", StringComparison.OrdinalIgnoreCase);

                        case "email1":
                            return dir * string.Compare(a.email1 ?? "", b.email1 ?? "", StringComparison.OrdinalIgnoreCase);

                        case "email2":
                            return dir * string.Compare(a.email2 ?? "", b.email2 ?? "", StringComparison.OrdinalIgnoreCase);

                        default:
                            return dir * string.Compare(a.identificacion ?? "", b.identificacion ?? "", StringComparison.OrdinalIgnoreCase);
                    }
                });
            }
            else
            {
                data.Sort((a, b) => string.Compare(a.identificacion ?? "", b.identificacion ?? "", StringComparison.OrdinalIgnoreCase));
            }
        }


        /// <summary>
        /// Pagina lista de clientes según parámetros.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="paginacion"></param>
        /// <param name="offset"></param>
        /// <param name="exportAll"></param>
        /// <returns></returns>
        private static List<FeClienteItem> PaginateClienteData(List<FeClienteItem> data, int paginacion, int offset, bool exportAll)
        {
            if (exportAll)
                return data;

            var page = new List<FeClienteItem>(paginacion);
            int start = offset;
            int end = offset + paginacion;

            for (int i = 0; i < data.Count; i++)
            {
                if (i < start) continue;
                if (i >= end) break;
                page.Add(data[i]);
            }

            return page;
        }


        /// <summary>
        /// Establece error en respuesta de cliente.
        /// </summary>
        /// <param name="resp"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        private static ErrorDto<FeClientesLista> SetClienteError(ErrorDto<FeClientesLista> resp, string description)
        {
            resp.Code = -1;
            resp.Description = description;
            resp.Result = null;
            return resp;
        }

    }
}