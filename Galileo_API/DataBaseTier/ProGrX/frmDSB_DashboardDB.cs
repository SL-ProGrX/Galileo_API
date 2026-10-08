using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX
{
    public class FrmDsbDashboardDB
    {
        private const string ProcedureMainKpiAccess = "spDSB_Main_KPI_Access";
        private const string TotalIndicator = "Total";
        private const string InvalidChartMessage = "El gráfico solicitado no es válido.";
        private const string ProcedureCreditosConsulta = "spDSB_Creditos_Consulta";
        private const string ProcedureCaptacionConsulta = "spDSB_Captacion_Consulta";
        private const string UtilidadMensual = "U_Mes";
        private const string UtilidadAcumulada = "U_Acumulada";
        private const string TasaEfectiva = "TASA_EFECTIVA";
        private static readonly TimeSpan ModuloCorteCacheDuracion = TimeSpan.FromMinutes(1);
        private static readonly DashboardClientesTendenciaConfig ClientesTendenciaConfig = new(
            "CLI",
            "No tiene acceso al dashboard de Clientes.",
            "spDSB_Clientes_Consulta_Histograma",
            [TotalIndicator, "Nuevos", "Reingresos", "Salidas", "ExAsociados",
                "INuevos", "IReingresos", "IExAsociados"],
            connection => ObtenerCorteResumen<DashboardClientesResumenData>(
                connection,
                "spDSB_Clientes_Consulta",
                resumen => resumen.Corte),
            origen => origen switch
            {
                "edades" => ("spDSB_Clientes_Asociados_Edades", ""),
                "generaciones" => ("spDSB_Clientes_Asociados_Generacion", "G"),
                "causas" => ("spDSB_Clientes_Consulta_Causas", "G"),
                _ => ((string?)null, "")
            });
        private static readonly DashboardClientesTendenciaConfig CreditosTendenciaConfig = new(
            "CRD",
            "No tiene acceso al dashboard de Crédito y Cobros.",
            "spDSB_Creditos_Consulta_Histograma",
            ["Tpp", "Pipp", "IMora_Activa", "ICbrJud", "IRefinancia", "ICancela",
                "CCPr", "CLPr", "TSaldo", "Colocacion", "Operaciones", "Refinancia", "TCbrJud"],
            connection => ObtenerCorteResumen<DashboardCreditosResumenData>(
                connection,
                ProcedureCreditosConsulta,
                resumen => resumen.Corte),
            origen => origen switch
            {
                "garantias" => (ProcedureCreditosConsulta, "G"),
                "morosidad" => ("spDSB_Creditos_Consulta_Morosidad", "G"),
                "tppGarantia" => ("spDSB_Creditos_Consulta_Tpp_Garantia", "G"),
                _ => ((string?)null, "")
            });
        private static readonly DashboardClientesTendenciaConfig AhorrosTendenciaConfig = new(
            "FND",
            "No tiene acceso al dashboard de Ahorros.",
            "spDSB_Captacion_Consulta_Histograma",
            ["CNT", "TOT", "AP", "RND", "Aportes", "Retiros"],
            connection => ObtenerCorteResumen<DashboardAhorrosResumenData>(
                connection,
                "spDSB_Captacion_Resumen",
                resumen => resumen.Corte),
            origen => origen switch
            {
                "planes" => (ProcedureCaptacionConsulta, "Plan"),
                "grupos" => (ProcedureCaptacionConsulta, "Grupo"),
                "patrimonio" => ("spDSB_Clientes_Consulta_Patrimonio", "G"),
                _ => ((string?)null, "")
            });
        private readonly PortalDB _portalDb;
        private readonly IMemoryCache _cache;

        public FrmDsbDashboardDB(IConfiguration config, IMemoryCache cache)
        {
            _portalDb = new PortalDB(config);
            _cache = cache;
        }

        public ErrorDto<List<DashboardCategoriaData>> Categorias_Obtener(int codEmpresa, string usuario)
        {
            var data = new List<DashboardCategoriaData>();
            try
            {
                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.CreateErrorResponse("No fue posible identificar al usuario.", -2, data);

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                data = connection.Query<DashboardCategoriaData>(
                    "spDSB_Main_Categorias_Access",
                    new { Usuario = usuario },
                    commandType: CommandType.StoredProcedure).ToList();
                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<DashboardClientesData> Clientes_Obtener(int codEmpresa, string usuario)
        {
            var data = new DashboardClientesData();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Clientes.", -2, data);

                data.Resumen = connection.QueryFirstOrDefault<DashboardClientesResumenData>(
                    "spDSB_Clientes_Consulta",
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);

                if (data.Resumen is null)
                    return DbHelper.CreateOkResponse(data);

                var corte = CorteCompleto(data.Resumen.Corte);
                data.Edades = ObtenerPuntosResumen(
                    connection,
                    "spDSB_Clientes_Asociados_Edades",
                    corte,
                    "G");
                data.Generaciones = ObtenerPuntosResumen(
                    connection,
                    "spDSB_Clientes_Asociados_Generacion",
                    corte,
                    "G");
                data.Causas = ObtenerPuntosResumen(
                    connection,
                    "spDSB_Clientes_Consulta_Causas",
                    corte,
                    "G");
                data.OpcionesTop = ObtenerOpcionesTop(
                    connection,
                    usuario,
                    "CLI");

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardClientesPuntoData>> Clientes_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
            => Tendencia_Clientes_Obtener(
                codEmpresa, usuario, origen, indicador, filtro, ClientesTendenciaConfig);

        public ErrorDto<DashboardCreditosData> Creditos_Obtener(int codEmpresa, string usuario)
        {
            var data = new DashboardCreditosData();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, "CRD"))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Crédito y Cobros.", -2, data);

                data.Resumen = connection.QueryFirstOrDefault<DashboardCreditosResumenData>(
                    ProcedureCreditosConsulta,
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);

                if (data.Resumen is null)
                    return DbHelper.CreateOkResponse(data);

                var corte = CorteCompleto(data.Resumen.Corte);
                data.Garantias = ObtenerPuntosResumen(
                    connection,
                    ProcedureCreditosConsulta,
                    corte,
                    "G");
                data.Morosidad = ObtenerPuntosResumen(
                    connection,
                    "spDSB_Creditos_Consulta_Morosidad",
                    corte,
                    "G");
                data.TppGarantia = ObtenerPuntosResumen(
                    connection,
                    "spDSB_Creditos_Consulta_Tpp_Garantia",
                    corte,
                    "G");
                data.OpcionesTop = ObtenerOpcionesTop(
                    connection,
                    usuario,
                    "CRD");

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardClientesPuntoData>> Creditos_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
            => Tendencia_Clientes_Obtener(
                codEmpresa, usuario, origen, indicador, filtro, CreditosTendenciaConfig);

        public ErrorDto<List<DashboardTopFilaData>> Clientes_Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad)
            => Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad, "CLI");

        public ErrorDto<List<DashboardTopFilaData>> Creditos_Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad)
            => Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad, "CRD");

        public ErrorDto<DashboardAhorrosData> Ahorros_Obtener(int codEmpresa, string usuario)
        {
            var data = new DashboardAhorrosData();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, "FND"))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Ahorros.", -2, data);

                data.Resumen = connection.QueryFirstOrDefault<DashboardAhorrosResumenData>(
                    "spDSB_Captacion_Resumen",
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);
                if (data.Resumen is null) return DbHelper.CreateOkResponse(data);

                var corte = CorteCompleto(data.Resumen.Corte);
                data.Planes = ObtenerPuntosResumen(
                    connection,
                    ProcedureCaptacionConsulta,
                    corte,
                    "Plan");
                data.Grupos = ObtenerPuntosResumen(
                    connection,
                    ProcedureCaptacionConsulta,
                    corte,
                    "Grupo");
                data.Patrimonio = ObtenerPuntosResumen(
                    connection,
                    "spDSB_Clientes_Consulta_Patrimonio",
                    corte,
                    "G");
                data.OpcionesTop = ObtenerOpcionesTop(
                    connection,
                    usuario,
                    "FND");

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardClientesPuntoData>> Ahorros_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
            => Tendencia_Clientes_Obtener(
                codEmpresa, usuario, origen, indicador, filtro, AhorrosTendenciaConfig);

        public ErrorDto<List<DashboardTopFilaData>> Ahorros_Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad)
            => Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad, "FND");

        public ErrorDto<DashboardModuloData> Modulo_Obtener(
            int codEmpresa, string usuario, string categoria)
        {
            var data = new DashboardModuloData();
            var config = ObtenerModuloConfig(categoria);
            if (config is null)
                return DbHelper.CreateErrorResponse("La categoría del dashboard no es válida.", -2, data);

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, categoria))
                    return DbHelper.CreateErrorResponse(
                        $"No tiene acceso al dashboard de {NombreCategoria(categoria)}.", -2, data);

                var valoresFila = ObtenerResumenModulo(connection, config);
                if (valoresFila is null) return DbHelper.CreateOkResponse(data);
                var corte = Convert.ToDateTime(ObtenerValor(valoresFila, "Corte"));
                data.Resumen = new DashboardModuloResumenData
                {
                    Corte = corte,
                    Valores = config.Campos.ToDictionary(
                        item => item.Key,
                        item => ConvertirNumero(ObtenerValor(valoresFila, item.Value)))
                };

                var corteCompleto = CorteCompleto(corte);
                GuardarModuloCorteEnCache(codEmpresa, categoria, corteCompleto);
                foreach (var grafico in config.Graficos)
                {
                    var puntos = connection.Query<DashboardModuloPuntoData>(
                        config.ProcedimientoConsulta,
                        new
                        {
                            Corte = corteCompleto,
                            Formato = "R",
                            Tipo = grafico,
                            Codigo = (string?)null
                        },
                        commandType: CommandType.StoredProcedure);
                    foreach (var punto in puntos)
                    {
                        punto.CodigoGrafico = grafico;
                        data.Puntos.Add(punto);
                    }
                }

                data.OpcionesTop = ObtenerOpcionesTop(
                    connection,
                    usuario,
                    categoria);

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardModuloPuntoData>> Modulo_Tendencia_Obtener(
            int codEmpresa, string usuario, string categoria, string origen,
            string indicador, string? filtro)
        {
            var data = new List<DashboardModuloPuntoData>();
            var config = ObtenerModuloConfig(categoria);
            if (config is null)
                return DbHelper.CreateErrorResponse(
                    "La categoría del dashboard no es válida.", -2, data);

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, categoria))
                    return DbHelper.CreateErrorResponse(
                        $"No tiene acceso al dashboard de {NombreCategoria(categoria)}.", -2, data);

                var corte = ObtenerModuloCorte(connection, codEmpresa, categoria, config);
                if (corte is null) return DbHelper.CreateOkResponse(data);

                if (origen == "kpi")
                {
                    if (!config.Indicadores.TryGetValue(indicador, out var dato))
                        return DbHelper.CreateErrorResponse(
                            "El indicador solicitado no es válido.", -2, data);

                    data = connection.Query<DashboardHistogramaData>(
                        config.ProcedimientoHistograma,
                        new { Corte = corte.Value, Dato = dato },
                        commandType: CommandType.StoredProcedure)
                        .Select(item => new DashboardModuloPuntoData
                        {
                            CodigoGrafico = indicador,
                            Corte = item.Descripcion,
                            Descripcion = item.Descripcion.ToString("yyyy-MM-dd"),
                            Value = item.Value
                        }).ToList();
                }
                else
                {
                    if (origen != "grafico" ||
                        !config.Graficos.Contains(indicador, StringComparer.Ordinal) ||
                        !string.IsNullOrEmpty(filtro) && filtro.Length > 100)
                        return DbHelper.CreateErrorResponse(
                            InvalidChartMessage, -2, data);

                    data = connection.Query<DashboardModuloPuntoData>(
                        config.ProcedimientoConsulta,
                        new
                        {
                            Corte = corte.Value,
                            Formato = "H",
                            Tipo = indicador,
                            Codigo = filtro
                        },
                        commandType: CommandType.StoredProcedure).ToList();
                    foreach (var punto in data) punto.CodigoGrafico = indicador;
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardTopFilaData>> Modulo_Top_Obtener(
            int codEmpresa, string usuario, string categoria, string codigo,
            int dias, int cantidad)
        {
            if (ObtenerModuloConfig(categoria) is null)
                return DbHelper.CreateErrorResponse(
                    "La categoría del dashboard no es válida.",
                    -2,
                    new List<DashboardTopFilaData>());
            return Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad, categoria);
        }

        private ErrorDto<List<DashboardTopFilaData>> Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad, string categoria)
        {
            var data = new List<DashboardTopFilaData>();
            if (!new[] { 7, 15, 30, 60, 120, 180, 365 }.Contains(dias)
                || !new[] { 10, 25, 50, 100 }.Contains(cantidad))
                return DbHelper.CreateErrorResponse("Los filtros del ranking no son válidos.", -2, data);

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, categoria))
                    return DbHelper.CreateErrorResponse(
                        categoria switch
                        {
                            "CLI" => "No tiene acceso al dashboard de Clientes.",
                            "CRD" => "No tiene acceso al dashboard de Crédito y Cobros.",
                            "FND" => "No tiene acceso al dashboard de Ahorros.",
                            _ => $"No tiene acceso al dashboard de {NombreCategoria(categoria)}."
                        }, -2, data);

                var autorizado = connection.Query<DashboardTopOpcionData>(
                    ProcedureMainKpiAccess,
                    new { Usuario = usuario, Tipo = "T", Categoria = categoria },
                    commandType: CommandType.StoredProcedure).Any(x => x.Cod_Kpi == codigo);
                if (!autorizado)
                    return DbHelper.CreateErrorResponse("El ranking no está autorizado.", -2, data);

                var corte = connection.QuerySingle<DateTime>("SELECT dbo.myGetdate()");
                data = connection.Query<DashboardTopFilaData>(
                    "spDSB_Main_Top_Executor",
                    new { List_Id = codigo, Inicio = corte.Date.AddDays(-dias), Corte = corte.Date, Top = cantidad },
                    commandType: CommandType.StoredProcedure).ToList();
                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        private static DashboardModuloConfig? ObtenerModuloConfig(string categoria)
            => categoria switch
            {
                "FIN" => new DashboardModuloConfig(
                    "spDSB_Contabilidad_Resumen",
                    "spDSB_Contabilidad_Consulta",
                    "spDSB_Contabilidad_Consulta_Histograma",
                    new Dictionary<string, string>
                    {
                        ["ROA"] = "ROA",
                        ["ROE"] = "ROE",
                        [UtilidadMensual] = UtilidadMensual,
                        [UtilidadAcumulada] = UtilidadAcumulada,
                        ["I"] = "Ingresos",
                        ["G"] = "Gastos",
                        ["A"] = "Activos",
                        ["P"] = "Pasivos",
                        ["C"] = "Patrimonio"
                    },
                    new Dictionary<string, string>
                    {
                        ["ROA"] = "ROA",
                        ["ROE"] = "ROE",
                        [UtilidadMensual] = UtilidadMensual,
                        [UtilidadAcumulada] = UtilidadAcumulada,
                        ["I"] = "I",
                        ["G"] = "G",
                        ["A"] = "A",
                        ["P"] = "P",
                        ["C"] = "C"
                    },
                    ["G", "Res", "Bal"]),
                "TES" => new DashboardModuloConfig(
                    "spDSB_Bancos_Resumen",
                    "spDSB_Bancos_Consulta",
                    "spDSB_Bancos_Consulta_Histograma",
                    new Dictionary<string, string>
                    {
                        ["TEc"] = "TEc",
                        ["CKc"] = "CKc",
                        ["DPc"] = "DPc",
                        [TotalIndicator] = TotalIndicator,
                        ["Transferencias"] = "Transferencias",
                        ["Cheques"] = "Cheques",
                        ["Depositos"] = "depositos"
                    },
                    new Dictionary<string, string>
                    {
                        ["TEc"] = "TEc",
                        ["CKc"] = "CKc",
                        ["DPc"] = "DPc",
                        [TotalIndicator] = "Sal",
                        ["Transferencias"] = "TEm",
                        ["Cheques"] = "CKm",
                        ["Depositos"] = "DPm"
                    },
                    ["B", "I", "C"]),
                "IVR" => new DashboardModuloConfig(
                    "spDSB_Inversiones_Resumen",
                    "spDSB_Inversiones_Consulta",
                    "spDSB_Inversiones_Consulta_Histograma",
                    new Dictionary<string, string>
                    {
                        [TasaEfectiva] = TasaEfectiva,
                        ["VALOR_LIBROS"] = "VALOR_LIBROS",
                        ["PYD_SALDO"] = "PYD_SALDO",
                        ["INTERES_ACUM_MONTO"] = "INTERES_ACUM_MONTO",
                        ["INTERES_MES"] = "INTERES_MES"
                    },
                    new Dictionary<string, string>
                    {
                        [TasaEfectiva] = TasaEfectiva,
                        ["VALOR_LIBROS"] = "VL",
                        ["PYD_SALDO"] = "PYD",
                        ["INTERES_ACUM_MONTO"] = "IA",
                        ["INTERES_MES"] = "IM"
                    },
                    ["Inst", "Adm", "Cat"]),
                _ => null
            };

        private static string NombreCategoria(string categoria)
            => categoria switch
            {
                "FIN" => "Financieros",
                "TES" => "Bancos y Cajas",
                "IVR" => "Inversiones",
                _ => "el módulo"
            };

        private static object? ObtenerValor(
            IDictionary<string, object?> fila, string nombre)
        {
            return fila
                .Where(item => string.Equals(
                    item.Key,
                    nombre,
                    StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Value)
                .FirstOrDefault();
        }

        private static double? ConvertirNumero(object? valor)
            => valor is null or DBNull ? null : Convert.ToDouble(valor);

        private sealed record DashboardModuloConfig(
            string ProcedimientoResumen,
            string ProcedimientoConsulta,
            string ProcedimientoHistograma,
            Dictionary<string, string> Campos,
            Dictionary<string, string> Indicadores,
            string[] Graficos);

        private ErrorDto<List<DashboardClientesPuntoData>> Tendencia_Clientes_Obtener(
            int codEmpresa,
            string usuario,
            string origen,
            string indicador,
            string? filtro,
            DashboardClientesTendenciaConfig config)
        {
            var data = new List<DashboardClientesPuntoData>();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, config.Categoria))
                    return DbHelper.CreateErrorResponse(config.MensajeAcceso, -2, data);

                var corte = config.ObtenerCorte(connection);
                if (corte is null) return DbHelper.CreateOkResponse(data);

                if (origen == "kpi")
                {
                    if (!config.IndicadoresKpi.Contains(indicador))
                        return DbHelper.CreateErrorResponse(
                            "El indicador no es válido.", -2, data);

                    data = ObtenerHistogramaClientes(
                        connection,
                        config.ProcedimientoHistograma,
                        corte.Value,
                        indicador);
                }
                else
                {
                    var (procedimiento, tipo) = config.ResolverGrafico(origen);
                    if (procedimiento is null || FiltroGraficoInvalido(filtro))
                        return DbHelper.CreateErrorResponse(InvalidChartMessage, -2, data);

                    data = connection.Query<DashboardClientesPuntoData>(
                        procedimiento,
                        new { Corte = corte.Value, Formato = "H", Tipo = tipo, Codigo = filtro },
                        commandType: CommandType.StoredProcedure).ToList();
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        private static List<DashboardClientesPuntoData> ObtenerPuntosResumen(
            IDbConnection connection,
            string procedimiento,
            DateTime corte,
            string tipo)
        {
            return connection.Query<DashboardClientesPuntoData>(
                procedimiento,
                new { Corte = corte, Formato = "R", Tipo = tipo },
                commandType: CommandType.StoredProcedure).ToList();
        }

        private static List<DashboardTopOpcionData> ObtenerOpcionesTop(
            IDbConnection connection,
            string usuario,
            string categoria)
        {
            return connection.Query<DashboardTopOpcionData>(
                ProcedureMainKpiAccess,
                new { Usuario = usuario, Tipo = "T", Categoria = categoria },
                commandType: CommandType.StoredProcedure).ToList();
        }

        private static IDictionary<string, object?>? ObtenerResumenModulo(
            IDbConnection connection,
            DashboardModuloConfig config)
        {
            var fila = connection.QueryFirstOrDefault(
                config.ProcedimientoResumen,
                new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                commandType: CommandType.StoredProcedure);
            return fila is null ? null : (IDictionary<string, object?>)fila;
        }

        private DateTime? ObtenerModuloCorte(
            IDbConnection connection,
            int codEmpresa,
            string categoria,
            DashboardModuloConfig config)
        {
            if (_cache.TryGetValue(
                    ObtenerModuloCorteCacheKey(codEmpresa, categoria),
                    out DateTime corte))
                return corte;

            var valoresFila = ObtenerResumenModulo(connection, config);
            if (valoresFila is null) return null;

            corte = CorteCompleto(
                Convert.ToDateTime(ObtenerValor(valoresFila, "Corte")));
            GuardarModuloCorteEnCache(codEmpresa, categoria, corte);
            return corte;
        }

        private static string ObtenerModuloCorteCacheKey(
            int codEmpresa,
            string categoria)
            => $"DashboardModuloCorte:{codEmpresa}:{categoria}";

        private void GuardarModuloCorteEnCache(
            int codEmpresa,
            string categoria,
            DateTime corte)
        {
            _cache.Set(
                ObtenerModuloCorteCacheKey(codEmpresa, categoria),
                corte,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.Add(
                        ModuloCorteCacheDuracion)
                });
        }

        private static DateTime? ObtenerCorteResumen<TResumen>(
            IDbConnection connection,
            string procedimiento,
            Func<TResumen, DateTime> selectorCorte)
        {
            var resumen = connection.QueryFirstOrDefault<TResumen>(
                procedimiento,
                new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                commandType: CommandType.StoredProcedure);
            return resumen is null ? null : CorteCompleto(selectorCorte(resumen));
        }

        private static DateTime CorteCompleto(DateTime corte)
            => corte.Date.AddDays(1).AddTicks(-1);

        private static bool FiltroGraficoInvalido(string? filtro)
            => !string.IsNullOrEmpty(filtro) && filtro.Length > 100;

        private static List<DashboardClientesPuntoData> ObtenerHistogramaClientes(
            IDbConnection connection,
            string procedimiento,
            DateTime corte,
            string indicador)
        {
            return connection.Query<DashboardHistogramaData>(
                procedimiento,
                new { Corte = corte, Dato = indicador },
                commandType: CommandType.StoredProcedure)
                .Select(item => new DashboardClientesPuntoData
                {
                    Corte = item.Descripcion,
                    Value = item.Value
                }).ToList();
        }

        private static bool TieneAcceso(IDbConnection connection, string usuario, string categoria = "CLI")
        {
            if (string.IsNullOrWhiteSpace(usuario)) return false;
            return connection.Query<DashboardCategoriaData>(
                "spDSB_Main_Categorias_Access",
                new { Usuario = usuario },
                commandType: CommandType.StoredProcedure).Any(x => x.Cod_Categoria == categoria);
        }

        private sealed record DashboardClientesTendenciaConfig(
            string Categoria,
            string MensajeAcceso,
            string ProcedimientoHistograma,
            string[] IndicadoresKpi,
            Func<IDbConnection, DateTime?> ObtenerCorte,
            Func<string, (string? Procedimiento, string Tipo)> ResolverGrafico);

        internal sealed class DashboardHistogramaData
        {
            public DashboardHistogramaData()
            {
            }

            public DateTime Descripcion { get; set; }
            public double? Value { get; set; }
        }
    }
}
