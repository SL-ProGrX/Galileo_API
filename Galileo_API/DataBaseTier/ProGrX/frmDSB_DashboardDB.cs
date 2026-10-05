using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX
{
    public class FrmDsbDashboardDB
    {
        private readonly PortalDB _portalDb;

        public FrmDsbDashboardDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
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

                var corte = data.Resumen.Corte.Date.AddDays(1).AddTicks(-1);
                data.Edades = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Clientes_Asociados_Edades",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.Generaciones = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Clientes_Asociados_Generacion",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.Causas = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Clientes_Consulta_Causas",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.OpcionesTop = connection.Query<DashboardTopOpcionData>(
                    "spDSB_Main_KPI_Access",
                    new { Usuario = usuario, Tipo = "T", Categoria = "CLI" },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardClientesPuntoData>> Clientes_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
        {
            var data = new List<DashboardClientesPuntoData>();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Clientes.", -2, data);

                var resumen = connection.QueryFirstOrDefault<DashboardClientesResumenData>(
                    "spDSB_Clientes_Consulta",
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);
                if (resumen is null) return DbHelper.CreateOkResponse(data);

                var corte = resumen.Corte.Date.AddDays(1).AddTicks(-1);
                if (origen == "kpi")
                {
                    string[] permitidos = ["Total", "Nuevos", "Reingresos", "Salidas",
                        "ExAsociados", "INuevos", "IReingresos", "IExAsociados"];
                    if (!permitidos.Contains(indicador))
                        return DbHelper.CreateErrorResponse("El indicador no es válido.", -2, data);

                    data = connection.Query<DashboardClientesHistogramaData>(
                        "spDSB_Clientes_Consulta_Histograma",
                        new { Corte = corte, Dato = indicador },
                        commandType: CommandType.StoredProcedure)
                        .Select(x => new DashboardClientesPuntoData
                        {
                            Corte = x.Descripcion,
                            Value = x.Value
                        }).ToList();
                }
                else
                {
                    var procedimiento = origen switch
                    {
                        "edades" => "spDSB_Clientes_Asociados_Edades",
                        "generaciones" => "spDSB_Clientes_Asociados_Generacion",
                        "causas" => "spDSB_Clientes_Consulta_Causas",
                        _ => null
                    };
                    if (procedimiento is null || !string.IsNullOrEmpty(filtro) && filtro.Length > 100)
                        return DbHelper.CreateErrorResponse("El gráfico solicitado no es válido.", -2, data);

                    data = connection.Query<DashboardClientesPuntoData>(
                        procedimiento,
                        new { Corte = corte, Formato = "H", Tipo = origen == "edades" ? "" : "G", Codigo = filtro },
                        commandType: CommandType.StoredProcedure).ToList();
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<DashboardCreditosData> Creditos_Obtener(int codEmpresa, string usuario)
        {
            var data = new DashboardCreditosData();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, "CRD"))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Crédito y Cobros.", -2, data);

                data.Resumen = connection.QueryFirstOrDefault<DashboardCreditosResumenData>(
                    "spDSB_Creditos_Consulta",
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);

                if (data.Resumen is null)
                    return DbHelper.CreateOkResponse(data);

                var corte = data.Resumen.Corte.Date.AddDays(1).AddTicks(-1);
                data.Garantias = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Creditos_Consulta",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.Morosidad = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Creditos_Consulta_Morosidad",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.TppGarantia = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Creditos_Consulta_Tpp_Garantia",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.OpcionesTop = connection.Query<DashboardTopOpcionData>(
                    "spDSB_Main_KPI_Access",
                    new { Usuario = usuario, Tipo = "T", Categoria = "CRD" },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardClientesPuntoData>> Creditos_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
        {
            var data = new List<DashboardClientesPuntoData>();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, "CRD"))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Crédito y Cobros.", -2, data);

                var resumen = connection.QueryFirstOrDefault<DashboardCreditosResumenData>(
                    "spDSB_Creditos_Consulta",
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);
                if (resumen is null) return DbHelper.CreateOkResponse(data);

                var corte = resumen.Corte.Date.AddDays(1).AddTicks(-1);
                if (origen == "kpi")
                {
                    string[] permitidos = ["Tpp", "Pipp", "IMora_Activa", "ICbrJud",
                        "IRefinancia", "ICancela", "CCPr", "CLPr", "TSaldo",
                        "Colocacion", "Operaciones", "Refinancia", "TCbrJud"];
                    if (!permitidos.Contains(indicador))
                        return DbHelper.CreateErrorResponse("El indicador no es válido.", -2, data);

                    data = connection.Query<DashboardClientesHistogramaData>(
                        "spDSB_Creditos_Consulta_Histograma",
                        new { Corte = corte, Dato = indicador },
                        commandType: CommandType.StoredProcedure)
                        .Select(x => new DashboardClientesPuntoData
                        {
                            Corte = x.Descripcion,
                            Value = x.Value
                        }).ToList();
                }
                else
                {
                    var procedimiento = origen switch
                    {
                        "garantias" => "spDSB_Creditos_Consulta",
                        "morosidad" => "spDSB_Creditos_Consulta_Morosidad",
                        "tppGarantia" => "spDSB_Creditos_Consulta_Tpp_Garantia",
                        _ => null
                    };
                    if (procedimiento is null || !string.IsNullOrEmpty(filtro) && filtro.Length > 100)
                        return DbHelper.CreateErrorResponse("El gráfico solicitado no es válido.", -2, data);

                    data = connection.Query<DashboardClientesPuntoData>(
                        procedimiento,
                        new { Corte = corte, Formato = "H", Tipo = "G", Codigo = filtro },
                        commandType: CommandType.StoredProcedure).ToList();
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

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

                var corte = data.Resumen.Corte.Date.AddDays(1).AddTicks(-1);
                data.Planes = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Captacion_Consulta",
                    new { Corte = corte, Formato = "R", Tipo = "Plan" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.Grupos = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Captacion_Consulta",
                    new { Corte = corte, Formato = "R", Tipo = "Grupo" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.Patrimonio = connection.Query<DashboardClientesPuntoData>(
                    "spDSB_Clientes_Consulta_Patrimonio",
                    new { Corte = corte, Formato = "R", Tipo = "G" },
                    commandType: CommandType.StoredProcedure).ToList();
                data.OpcionesTop = connection.Query<DashboardTopOpcionData>(
                    "spDSB_Main_KPI_Access",
                    new { Usuario = usuario, Tipo = "T", Categoria = "FND" },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

        public ErrorDto<List<DashboardClientesPuntoData>> Ahorros_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
        {
            var data = new List<DashboardClientesPuntoData>();
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                if (!TieneAcceso(connection, usuario, "FND"))
                    return DbHelper.CreateErrorResponse("No tiene acceso al dashboard de Ahorros.", -2, data);

                var resumen = connection.QueryFirstOrDefault<DashboardAhorrosResumenData>(
                    "spDSB_Captacion_Resumen",
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);
                if (resumen is null) return DbHelper.CreateOkResponse(data);

                var corte = resumen.Corte.Date.AddDays(1).AddTicks(-1);
                if (origen == "kpi")
                {
                    string[] permitidos = ["CNT", "TOT", "AP", "RND", "Aportes", "Retiros"];
                    if (!permitidos.Contains(indicador))
                        return DbHelper.CreateErrorResponse("El indicador no es válido.", -2, data);

                    data = connection.Query<DashboardClientesHistogramaData>(
                        "spDSB_Captacion_Consulta_Histograma",
                        new { Corte = corte, Dato = indicador },
                        commandType: CommandType.StoredProcedure)
                        .Select(x => new DashboardClientesPuntoData
                        {
                            Corte = x.Descripcion,
                            Value = x.Value
                        }).ToList();
                }
                else
                {
                    var (procedimiento, tipo) = origen switch
                    {
                        "planes" => ("spDSB_Captacion_Consulta", "Plan"),
                        "grupos" => ("spDSB_Captacion_Consulta", "Grupo"),
                        "patrimonio" => ("spDSB_Clientes_Consulta_Patrimonio", "G"),
                        _ => ((string?)null, "")
                    };
                    if (procedimiento is null || !string.IsNullOrEmpty(filtro) && filtro.Length > 100)
                        return DbHelper.CreateErrorResponse("El gráfico solicitado no es válido.", -2, data);

                    data = connection.Query<DashboardClientesPuntoData>(
                        procedimiento,
                        new { Corte = corte, Formato = "H", Tipo = tipo, Codigo = filtro },
                        commandType: CommandType.StoredProcedure).ToList();
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, data);
            }
        }

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

                var fila = connection.QueryFirstOrDefault(
                    config.ProcedimientoResumen,
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);
                if (fila is null) return DbHelper.CreateOkResponse(data);

                var valoresFila = (IDictionary<string, object?>)fila;
                var corte = Convert.ToDateTime(ObtenerValor(valoresFila, "Corte"));
                data.Resumen = new DashboardModuloResumenData
                {
                    Corte = corte,
                    Valores = config.Campos.ToDictionary(
                        item => item.Key,
                        item => ConvertirNumero(ObtenerValor(valoresFila, item.Value)))
                };

                var corteCompleto = corte.Date.AddDays(1).AddTicks(-1);
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

                data.OpcionesTop = connection.Query<DashboardTopOpcionData>(
                    "spDSB_Main_KPI_Access",
                    new { Usuario = usuario, Tipo = "T", Categoria = categoria },
                    commandType: CommandType.StoredProcedure).ToList();

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

                var fila = connection.QueryFirstOrDefault(
                    config.ProcedimientoResumen,
                    new { Corte = (DateTime?)null, Formato = "R", Tipo = "T" },
                    commandType: CommandType.StoredProcedure);
                if (fila is null) return DbHelper.CreateOkResponse(data);
                var valoresFila = (IDictionary<string, object?>)fila;
                var corte = Convert.ToDateTime(ObtenerValor(valoresFila, "Corte"))
                    .Date.AddDays(1).AddTicks(-1);

                if (origen == "kpi")
                {
                    if (!config.Indicadores.TryGetValue(indicador, out var dato))
                        return DbHelper.CreateErrorResponse(
                            "El indicador solicitado no es válido.", -2, data);

                    data = connection.Query<DashboardModuloHistogramaData>(
                        config.ProcedimientoHistograma,
                        new { Corte = corte, Dato = dato },
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
                            "El gráfico solicitado no es válido.", -2, data);

                    data = connection.Query<DashboardModuloPuntoData>(
                        config.ProcedimientoConsulta,
                        new
                        {
                            Corte = corte,
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
                    "spDSB_Main_KPI_Access",
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
                        ["U_Mes"] = "U_Mes",
                        ["U_Acumulada"] = "U_Acumulada",
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
                        ["U_Mes"] = "U_Mes",
                        ["U_Acumulada"] = "U_Acumulada",
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
                        ["Total"] = "Total",
                        ["Transferencias"] = "Transferencias",
                        ["Cheques"] = "Cheques",
                        ["Depositos"] = "depositos"
                    },
                    new Dictionary<string, string>
                    {
                        ["TEc"] = "TEc",
                        ["CKc"] = "CKc",
                        ["DPc"] = "DPc",
                        ["Total"] = "Sal",
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
                        ["TASA_EFECTIVA"] = "TASA_EFECTIVA",
                        ["VALOR_LIBROS"] = "VALOR_LIBROS",
                        ["PYD_SALDO"] = "PYD_SALDO",
                        ["INTERES_ACUM_MONTO"] = "INTERES_ACUM_MONTO",
                        ["INTERES_MES"] = "INTERES_MES"
                    },
                    new Dictionary<string, string>
                    {
                        ["TASA_EFECTIVA"] = "TASA_EFECTIVA",
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
            foreach (var item in fila)
                if (string.Equals(item.Key, nombre, StringComparison.OrdinalIgnoreCase))
                    return item.Value;
            return null;
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

        private sealed class DashboardModuloHistogramaData
        {
            public DateTime Descripcion { get; set; }
            public double? Value { get; set; }
        }

        private static bool TieneAcceso(IDbConnection connection, string usuario, string categoria = "CLI")
        {
            if (string.IsNullOrWhiteSpace(usuario)) return false;
            return connection.Query<DashboardCategoriaData>(
                "spDSB_Main_Categorias_Access",
                new { Usuario = usuario },
                commandType: CommandType.StoredProcedure).Any(x => x.Cod_Categoria == categoria);
        }

        private class DashboardClientesHistogramaData
        {
            public DateTime Descripcion { get; set; }
            public double? Value { get; set; }
        }
    }
}
