using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX_EstudioCrd.FrmPreaConsultasModels;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaConsultasDB
    {

        private readonly PortalDB _portalDB;

        public FrmPreaConsultasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);

        }

        private const string Capacidad = @"
        SELECT 
            RTRIM(M.COD_CAPACIDAD) AS item,
            RTRIM(M.COD_CAPACIDAD) + ': ' + RTRIM(R.descripcion) AS descripcion
        FROM Crd_Clasificacion_Capacidad M
        INNER JOIN CRD_CLASIFICACION_RAZON R ON M.COD_RAZON = R.COD_RAZON";

        private const string Endeudamiento = @"
        SELECT 
            RTRIM(M.COD_ENDEUDAMIENTO) AS item,
            RTRIM(M.COD_ENDEUDAMIENTO) + ': ' + RTRIM(R.descripcion) AS descripcion
        FROM CRD_CLASIFICACION_ENDEUDAMIENTO M
        INNER JOIN CRD_CLASIFICACION_RAZON R ON M.COD_RAZON = R.COD_RAZON";

        private const string Garantia = @"
        SELECT 
            RTRIM(M.COD_GARANTIA) AS item,
            RTRIM(M.COD_GARANTIA) + ': ' + RTRIM(R.descripcion) AS descripcion
        FROM CRD_CLASIFICACION_GARANTIA M
        INNER JOIN CRD_CLASIFICACION_RAZON R ON M.COD_RAZON = R.COD_RAZON";

        private const string Historial = @"
        SELECT 
            RTRIM(M.COD_HISTORIAL) AS item,
            RTRIM(M.COD_HISTORIAL) + ': ' + RTRIM(M.descripcion) AS descripcion
        FROM CRD_CLASIFICACION_HISTORIAL M
        INNER JOIN CRD_CLASIFICACION_RAZON R ON M.COD_RAZON = R.COD_RAZON";

        private const string Morosidad = @"
        SELECT 
            A.cod_mora AS item,
            CASE
                WHEN A.tipo = 'A' THEN 'Al Día    '
                WHEN A.tipo = 'M' THEN 'Mora      '
                WHEN A.tipo = 'C' THEN 'Cbr.Jud   '
                WHEN A.tipo = 'I' THEN 'Incobrable'
                ELSE ''
            END + ': ' + RTRIM(B.descripcion) AS descripcion
        FROM Cbr_Clasificacion_Mora A
        INNER JOIN Crd_Clasificacion_Razon B ON A.cod_Razon = B.Cod_Razon
        ORDER BY A.cod_mora";

        /// <summary>
        /// Consulta los catálogos necesarios para los filtros de la pantalla de consultas de estudios crediticios, incluyendo capacidad, endeudamiento, garantía, historial, morosidad, estados, tipos de fecha y trámites.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<PreaConsultasCatalogosResponse> PreaConsultas_Catalogos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var response = new PreaConsultasCatalogosResponse
                {
                    FechaCorte = DateTime.Today,
                    FechaInicio = DateTime.Today.AddMonths(-3),
                    Capacidad = PreaConsultas_Catalogo_Consultar(conn, Capacidad),
                    Endeudamiento = PreaConsultas_Catalogo_Consultar(conn, Endeudamiento),
                    Garantia = PreaConsultas_Catalogo_Consultar(conn, Garantia),
                    Historial = PreaConsultas_Catalogo_Consultar(conn, Historial),
                    Morosidad = PreaConsultas_Catalogo_Consultar(conn, Morosidad),
                    Estados = PreaConsultas_Estados_Obtener(),
                    TiposFecha = PreaConsultas_TiposFecha_Obtener(),
                    Tramites = PreaConsultas_Tramites_Obtener()
                };

                return response;
            });
        }

        /// <summary>
        /// Agrega la opción "TODOS" al resultado de una consulta de catálogo, permitiendo que los filtros puedan seleccionar todos los valores disponibles.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private static List<DropDownListaGenericaModel> PreaConsultas_Catalogo_Consultar(IDbConnection conn, string query)
        {
            return
                  [
                      new DropDownListaGenericaModel
                    {
                        item = "TODOS",
                        descripcion = "TODOS"
                    },
                    .. conn.Query<DropDownListaGenericaModel>(query)
                  ];
        }

        /// <summary>
        /// Ejecuta consulta segun filtros   
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<ConsultaLista> PreaConsultas_Grid_Obtener(int CodEmpresa, PreaConsultasFiltroRequest request, bool esExportar)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Request inválido.",
                    -1,
                    new ConsultaLista());
            }
            try
            {
                var normalized = PreaConsultas_NormalizeRequest(request);
                var (listSql, countSql, parameters) = PreaConsultas_BuildGridSql(normalized, esExportar);

                var total = connection.QuerySingle<int>(countSql, parameters);

                List<PreaConsultasGridModel> rows =
                        [.. connection.Query<PreaConsultasGridModel>(listSql, parameters)];

                var response = new ConsultaLista
                {
                    total = total,
                    lista = rows
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                     "Error al consultar estudios crediticios.",
                     -1,
                     new ConsultaLista());
            }
        }

        /// <summary>
        /// Metodo para normalizar y validar los parámetros de la consulta, asegurando que los valores de texto estén correctamente formateados y que los parámetros de paginación tengan valores válidos antes de construir la consulta SQL. Esto ayuda a prevenir errores y mejorar la robustez de la aplicación.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static PreaConsultasFiltroRequest PreaConsultas_NormalizeRequest(PreaConsultasFiltroRequest request)
        {
            return new PreaConsultasFiltroRequest
            {
                Usuario = request.Usuario?.Trim(),
                Estado = request.Estado?.Trim(),
                TipoFecha = request.TipoFecha?.Trim(),
                FechaInicio = request.FechaInicio,
                FechaCorte = request.FechaCorte,
                CodLinea = request.CodLinea?.Trim(),
                CodDestino = request.CodDestino?.Trim(),
                CodInstitucion = request.CodInstitucion,
                IdComite = request.IdComite,
                ClasificaGarantia = request.ClasificaGarantia?.Trim(),
                ClasificaMorosidad = request.ClasificaMorosidad?.Trim(),
                ClasificaCapacidad = request.ClasificaCapacidad?.Trim(),
                ClasificaEndeudamiento = request.ClasificaEndeudamiento?.Trim(),
                ClasificaHistorial = request.ClasificaHistorial?.Trim(),
                TramiteEstado = request.TramiteEstado?.Trim(),
                Filtro = request.Filtro?.Trim(),
                Pagina = request.Pagina.GetValueOrDefault(1) <= 0 ? 1 : request.Pagina,
                Paginacion = request.Paginacion.GetValueOrDefault(30) <= 0 ? 30 : request.Paginacion,
                SortOrder = request.SortOrder.GetValueOrDefault(0),
                SortField = request.SortField?.Trim()
            };
        }

        /// <summary>
        /// Metodo para construir dinámicamente las consultas SQL de listado y conteo de estudios crediticios según los filtros proporcionados en la solicitud. Este método genera la cláusula WHERE basada en los filtros aplicados, así como la cláusula ORDER BY según el campo y orden de clasificación especificados. Además, maneja la paginación de resultados cuando no se trata de una exportación completa.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        private static (string ListSql, string CountSql, DynamicParameters Parameters) PreaConsultas_BuildGridSql(PreaConsultasFiltroRequest request, bool esExportar)
        {
            var parameters = new DynamicParameters();

            var where = PreaConsultas_Filtros_Build(request, parameters, true);
            var orderBy = PreaConsultas_OrderBy_Build(request.SortField, request.SortOrder);

            var baseFrom = $@"
        FROM vCrd_Estudio_Crediticio
        WHERE {where}";

            var countSql = $@"
        SELECT COUNT(1)
        {baseFrom}";

            var listSql = $@"
        SELECT
            '' AS Btn,
            expediente AS Expediente,
            estado_desc AS EstadoDesc,
            cedula AS Cedula,
            nombre AS Nombre,
            Linea_Desc AS LineaDesc,
            Destino_Desc AS DestinoDesc,
            monto AS Monto,
            plazo AS Plazo,
            tasa AS Tasa,
            cuota AS Cuota,
            REFUNDICIONES AS Refundiciones,
            DESEMBOLSOS AS Desembolsos,
            MONTO_COLOCADO AS MontoColocado,
            institucion_desc AS InstitucionDesc,
            departamento_Desc AS DepartamentoDesc,
            Oficina_Desc AS OficinaDesc,
            Usuario,
            Clasifica_capacidad AS ClasificaCapacidad,
            Clasifica_endeudamiento AS ClasificaEndeudamiento,
            Clasifica_historial AS ClasificaHistorial,
            Clasifica_garantia AS ClasificaGarantia,
            Clasifica_morosidad AS ClasificaMorosidad,
            Registro_Fecha AS RegistroFecha,
            Gestion_Fecha AS GestionFecha,
            Operacion,
            Tramite_Desc AS TramiteDesc
        {baseFrom}
        {orderBy}";

            if (!esExportar)
            {
                var pagina = request.Pagina.GetValueOrDefault(1);
                var paginacion = request.Paginacion.GetValueOrDefault(30);
                var offset = (pagina - 1) * paginacion;

                parameters.Add("@Offset", offset);
                parameters.Add("@Paginacion", paginacion);

                listSql += @"
            OFFSET @Offset ROWS
            FETCH NEXT @Paginacion ROWS ONLY";
            }

            return (listSql, countSql, parameters);
        }

        /// <summary>
        /// Metodo para consultar el resumen de estudios crediticios agrupados por un criterio específico (línea, destino, garantía, institución, estado o tendencia) según el tipo de resumen solicitado. La consulta se construye dinámicamente para incluir los filtros aplicados y agrupar los resultados según el criterio seleccionado, proporcionando un conteo de casos, sumas de montos, refundiciones, desembolsos y monto colocado para cada grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaConsultasResumenModel>> PreaConsultas_Resumen_Obtener(int CodEmpresa, PreaConsultasFiltroRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<List<PreaConsultasResumenModel>>(
                    "Request inválido.",
                    -1,
                    []);
            }
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var parameters = new DynamicParameters();
                var where = PreaConsultas_Filtros_Build(request, parameters, false);
                var (codigo, descripcion, groupBy, orderBy) = PreaConsultas_ResumenAgrupacion_Obtener(request.TipoResumen);

                var query = PreaConsultas_ResumenSql_Build(
                       codigo,
                       descripcion,
                       groupBy,
                       orderBy,
                       where);

                List<PreaConsultasResumenModel> result =
            [.. connection.Query<PreaConsultasResumenModel>(query, parameters)];



                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<List<PreaConsultasResumenModel>>(
                    "Error al consultar resumen de estudios crediticios.",
                    -1,
                    []);
            }
        }
        private static string PreaConsultas_ResumenSql_Build(string codigo, string descripcion,string groupBy,string orderBy, string where)
        {
            return $@"
                SELECT
                    {codigo} AS Codigo,
                    {descripcion} AS Descripcion,
                    COUNT(*) AS Casos,
                    SUM(Monto) AS Monto,
                    SUM(Refundiciones) AS Refundiciones,
                    SUM(Desembolsos) AS Desembolsos,
                    SUM(Monto - Refundiciones) AS MontoColocado
                FROM vCrd_Estudio_Crediticio
                WHERE {where}
                GROUP BY {groupBy}
                ORDER BY {orderBy}";
        }

        /// <summary>
        /// Metodo para construir la cláusula WHERE de las consultas SQL de estudios crediticios según los filtros proporcionados en la solicitud. Este método agrega condiciones a la cláusula WHERE para cada filtro aplicado, utilizando parámetros para evitar inyecciones SQL
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameters"></param>
        /// <param name="validarFiltro"></param>
        /// <returns></returns>
        private static string PreaConsultas_Filtros_Build(PreaConsultasFiltroRequest request, DynamicParameters parameters, bool validarFiltro)
        {
            var filters = new List<string>();

            parameters.Add("@Usuario", $"%{request.Usuario ?? string.Empty}%");
            filters.Add("Usuario LIKE @Usuario");

            if (!string.IsNullOrWhiteSpace(request.TipoFecha)
                && !request.TipoFecha.Equals("Todas", StringComparison.OrdinalIgnoreCase)
                && request.FechaInicio.HasValue
                && request.FechaCorte.HasValue)
            {
                var campoFecha = request.TipoFecha.Equals("Gestión", StringComparison.OrdinalIgnoreCase)
                    ? "Gestion_Fecha"
                    : "Registro_Fecha";

                parameters.Add("@FechaInicio", request.FechaInicio.Value.Date);
                parameters.Add("@FechaCorte", request.FechaCorte.Value.Date.AddDays(1).AddSeconds(-1));

                filters.Add($"{campoFecha} BETWEEN @FechaInicio AND @FechaCorte");
            }

            if (!string.IsNullOrWhiteSpace(request.Estado) && request.Estado != "T")
            {
                parameters.Add("@Estado", request.Estado);
                filters.Add("Estado = @Estado");
            }

            if (!string.IsNullOrWhiteSpace(request.CodLinea))
            {
                parameters.Add("@CodLinea", request.CodLinea);
                filters.Add("cod_Linea = @CodLinea");
            }

            if (!string.IsNullOrWhiteSpace(request.CodDestino))
            {
                parameters.Add("@CodDestino", request.CodDestino);
                filters.Add("cod_destino = @CodDestino");
            }

            if (request.CodInstitucion.HasValue && request.CodInstitucion != 0)
            {
                parameters.Add("@CodInstitucion", request.CodInstitucion.Value);
                filters.Add("cod_institucion = @CodInstitucion");
            }

            if (request.IdComite.HasValue && request.IdComite != 0)
            {
                parameters.Add("@IdComite", request.IdComite.Value);
                filters.Add("id_comite = @IdComite");
            }

            PreaConsultas_FiltroTexto_Agregar(filters, parameters, "clasifica_Garantia", "@ClasificaGarantia", request.ClasificaGarantia);
            PreaConsultas_FiltroTexto_Agregar(filters, parameters, "clasifica_Morosidad", "@ClasificaMorosidad", request.ClasificaMorosidad);
            PreaConsultas_FiltroTexto_Agregar(filters, parameters, "clasifica_Capacidad", "@ClasificaCapacidad", request.ClasificaCapacidad);
            PreaConsultas_FiltroTexto_Agregar(filters, parameters, "clasifica_Endeudamiento", "@ClasificaEndeudamiento", request.ClasificaEndeudamiento);
            PreaConsultas_FiltroTexto_Agregar(filters, parameters, "clasifica_Historial", "@ClasificaHistorial", request.ClasificaHistorial);

            if (!string.IsNullOrWhiteSpace(request.TramiteEstado) && request.TramiteEstado != "T")
            {
                parameters.Add("@TramiteEstado", request.TramiteEstado);
                filters.Add("Tramite_Estado = @TramiteEstado");
            }

            if (validarFiltro)
            {
                PreaConsultas_FiltroGlobal_Agregar(filters, parameters, request.Filtro);
            }
            return string.Join(" AND ", filters);
        }

        /// <summary>
        /// Metodo para validacion de filtros tipo texto
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="parameters"></param>
        /// <param name="columnName"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        private static void PreaConsultas_FiltroTexto_Agregar(List<string> filters, DynamicParameters parameters, string columnName, string parameterName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Equals("TODOS", StringComparison.OrdinalIgnoreCase)
                || value.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            parameters.Add(parameterName, value);
            filters.Add($"{columnName} = {parameterName}");
        }

        /// <summary>
        /// Metodo para obtener la configuración de agrupación y ordenamiento de la consulta de resumen según el tipo de resumen solicitado, permitiendo que la consulta se adapte dinámicamente para agrupar por línea, destino, garantía, institución, estado o tendencia según la selección del usuario. 
        /// </summary>
        /// <param name="tipoResumen"></param>
        /// <returns></returns>
        private static (string Codigo, string Descripcion, string GroupBy, string OrderBy) PreaConsultas_ResumenAgrupacion_Obtener(string? tipoResumen)
        {
            return tipoResumen?.Trim().ToUpperInvariant() switch
            {
                "DESTINO" => (
                    "cod_Destino",
                    "Destino_desc",
                    "cod_Destino, Destino_desc",
                    "Destino_desc"),

                "GARANTIA" => (
                    "garantia",
                    "Garantia_desc",
                    "garantia, Garantia_desc",
                    "Garantia_desc"),

                "INSTITUCION" => (
                    "cod_institucion",
                    "Institucion_desc",
                    "cod_institucion, Institucion_desc",
                    "Institucion_desc"),

                "ESTADO" => (
                    "Estado",
                    "Estado_desc",
                    "Estado, Estado_desc",
                    "Estado_desc"),

                "TENDENCIA" => (
                    "YEAR(registro_Fecha)",
                    @"CASE MONTH(Registro_Fecha)
                WHEN 1 THEN 'Enero'
                WHEN 2 THEN 'Febrero'
                WHEN 3 THEN 'Marzo'
                WHEN 4 THEN 'Abril'
                WHEN 5 THEN 'Mayo'
                WHEN 6 THEN 'Junio'
                WHEN 7 THEN 'Julio'
                WHEN 8 THEN 'Agosto'
                WHEN 9 THEN 'Setiembre'
                WHEN 10 THEN 'Octubre'
                WHEN 11 THEN 'Noviembre'
                WHEN 12 THEN 'Diciembre'
              END",
                    "YEAR(registro_Fecha), MONTH(Registro_Fecha)",
                    "YEAR(registro_Fecha), MONTH(Registro_Fecha)"),

                _ => (
                    "cod_linea",
                    "linea_desc",
                    "cod_linea, Linea_Desc",
                    "linea_desc")
            };
        }

        /// <summary>
        ///  Metodo para agregar filtro global de búsqueda en múltiples columnas, permitiendo que el usuario pueda ingresar un término de búsqueda que se aplicará a varias columnas relevantes en la consulta de estudios crediticios
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="parameters"></param>
        /// <param name="filtro"></param>
        private static void PreaConsultas_FiltroGlobal_Agregar(List<string> filters, DynamicParameters parameters, string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            parameters.Add("@Filtro", $"%{filtro}%");

            filters.Add(@"(
            expediente LIKE @Filtro
            OR estado_desc LIKE @Filtro
            OR cedula LIKE @Filtro
            OR nombre LIKE @Filtro
            OR Linea_Desc LIKE @Filtro
            OR Destino_Desc LIKE @Filtro
            OR institucion_desc LIKE @Filtro
            OR departamento_Desc LIKE @Filtro
            OR Oficina_Desc LIKE @Filtro
            OR Usuario LIKE @Filtro
            OR Clasifica_capacidad LIKE @Filtro
            OR Clasifica_endeudamiento LIKE @Filtro
            OR Clasifica_historial LIKE @Filtro
            OR Clasifica_garantia LIKE @Filtro
            OR Clasifica_morosidad LIKE @Filtro
            OR Operacion LIKE @Filtro
            OR Tramite_Desc LIKE @Filtro
        )");
        }

        /// <summary>
        /// Metodo para ordenar los resultados de la consulta de estudios crediticios según el campo y orden de clasificación especificados en la solicitud
        /// </summary>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static string PreaConsultas_OrderBy_Build(string? sortField, int? sortOrder)
        {
            var columnName = PreaConsultas_SortColumn_Obtener(sortField);

            var direction = sortOrder switch
            {
                1 => "ASC",
                2 => "DESC",
                _ => "DESC"
            };

            return $"ORDER BY {columnName} {direction}";
        }

        /// <summary>
        /// Metodo para obtener el nombre de la columna correspondiente en la base de datos según el campo de ordenación especificado en la solicitud
        /// </summary>
        /// <param name="sortField"></param>
        /// <returns></returns>
        private static string PreaConsultas_SortColumn_Obtener(string? sortField)
        {
            return sortField?.Trim() switch
            {
                "expediente" => "expediente",
                "estadoDesc" => "estado_desc",
                "cedula" => "cedula",
                "nombre" => "nombre",
                "lineaDesc" => "Linea_Desc",
                "destinoDesc" => "Destino_Desc",
                "monto" => "monto",
                "plazo" => "plazo",
                "tasa" => "tasa",
                "cuota" => "cuota",
                "refundiciones" => "REFUNDICIONES",
                "desembolsos" => "DESEMBOLSOS",
                "montoColocado" => "MONTO_COLOCADO",
                "institucionDesc" => "institucion_desc",
                "departamentoDesc" => "departamento_Desc",
                "oficinaDesc" => "Oficina_Desc",
                "usuario" => "Usuario",
                "clasificaCapacidad" => "Clasifica_capacidad",
                "clasificaEndeudamiento" => "Clasifica_endeudamiento",
                "clasificaHistorial" => "Clasifica_historial",
                "clasificaGarantia" => "Clasifica_garantia",
                "clasificaMorosidad" => "Clasifica_morosidad",
                "registroFecha" => "Registro_Fecha",
                "gestionFecha" => "Gestion_Fecha",
                "operacion" => "Operacion",
                "tramiteDesc" => "Tramite_Desc",
                _ => "Registro_Fecha"
            };
        }

        /// <summary>
        /// Metodo para obtener la lista de estados disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        private static List<DropDownListaGenericaModel> PreaConsultas_Estados_Obtener()
        {
            return
            [
                new() { item = "T", descripcion = "Todos" },
        new() { item = "R", descripcion = "Recibido" },
        new() { item = "P", descripcion = "Pendiente" },
        new() { item = "A", descripcion = "Autorizado" },
        new() { item = "B", descripcion = "Abandonado" },
        new() { item = "D", descripcion = "Denegado" }
            ];
        }

        /// <summary>
        /// Metodo para obtener la lista de tipos de fecha disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        private static List<DropDownListaGenericaModel> PreaConsultas_TiposFecha_Obtener()
        {
            return
            [
                new() { item = "Registro", descripcion = "Registro" },
        new() { item = "Gestión", descripcion = "Gestión" },
        new() { item = "Todas", descripcion = "Todas" }
            ];
        }

        /// <summary>
        /// Metodo para obtener la lista de trámites disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        private static List<DropDownListaGenericaModel> PreaConsultas_Tramites_Obtener()
        {
            return
            [
                new() { item = "T", descripcion = "Todos" },
        new() { item = "NA", descripcion = "SGT No Indica" },
        new() { item = "P", descripcion = "SGT En Proceso" },
        new() { item = "F", descripcion = "SGT Formalizada" },
        new() { item = "N", descripcion = "SGT Anulada" }
            ];
        }

        /// <summary>
        /// Metodo para obtener la lista de usuarios disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Usuarios_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT 
                RTRIM(USUARIO) AS item,
                RTRIM(DESCRIPCION) AS descripcion
            FROM USUARIOS
            WHERE ESTADO = 'A'
            ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener la lista de líneas disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Lineas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT 
                RTRIM(CODIGO) AS item,
                RTRIM(DESCRIPCION) AS descripcion
            FROM CATALOGO
            WHERE LINEA_INTERNA = 1
              AND RETENCION = 'N'
              AND POLIZA = 'N'
            ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener la lista de destinos disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Destinos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT 
                RTRIM(COD_DESTINO) AS item,
                RTRIM(DESCRIPCION) AS descripcion
            FROM CATALOGO_DESTINOS
            ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener la lista de instituciones disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT 
                COD_INSTITUCION AS item,
                RTRIM(DESCRIPCION) AS descripcion
            FROM INSTITUCIONES
            ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener la lista de comités disponibles para filtrar los estudios crediticios
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PreaConsultas_Comites_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT 
                ID_COMITE AS item,
                RTRIM(DESCRIPCION) AS descripcion
            FROM COMITES
            ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

    }
}
