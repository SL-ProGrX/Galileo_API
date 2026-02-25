using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX_Polizas.FrmPolizasPeConsultasModels;
using System.Text;
using Galileo.Models;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasPeConsultasDB
    {
        private readonly PortalDB _portalDb;


        public FrmPolizasPeConsultasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        ///  Busca pólizas PE según los criterios especificados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="esExportar"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<PolizasPeConsultasBuscarResponseDto> PolizasPeConsultas_Buscar(
        int codEmpresa,
        bool esExportar,
        PolizasPeConsultasBuscarRequestDto request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<PolizasPeConsultasBuscarResponseDto>(
                    "Request inválido.",
                    -1,
                    new PolizasPeConsultasBuscarResponseDto()
                );
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var normalized = NormalizeRequest(request);
                var (listSql, countSql, parameters) = BuildBuscarSql(normalized, esExportar);

                var total = connection.QuerySingle<int>(countSql, parameters);
                var rows = connection.Query<PolizasPeConsultasDto>(listSql, parameters).AsList();

                var response = BuildResponse(rows, total);

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<PolizasPeConsultasBuscarResponseDto>(
                    "Error al consultar PolizasPeConsultas.",
                    -1,
                    new PolizasPeConsultasBuscarResponseDto()
                );
            }
        }

        /// <summary>
        /// Normaliza los datos de la solicitud para asegurar que los filtros se apliquen correctamente.
        /// <param name="request"></param>
        /// <returns></returns>
        private static PolizasPeConsultasBuscarRequestDto NormalizeRequest(PolizasPeConsultasBuscarRequestDto request)
        {


            var pesoInicio = request.PesoInicio;
            var pesoCorte = request.PesoCorte;
            NormalizeRange(ref pesoInicio, ref pesoCorte);
            request.PesoInicio = pesoInicio;
            request.PesoCorte = pesoCorte;

            var capacidadInicio = request.CapacidadInicio;
            var capacidadCorte = request.CapacidadCorte;
            NormalizeRange(ref capacidadInicio, ref capacidadCorte);
            request.CapacidadInicio = capacidadInicio;
            request.CapacidadCorte = capacidadCorte;

            var cilindrajeInicio = request.CilindrajeInicio;
            var cilindrajeCorte = request.CilindrajeCorte;
            NormalizeRange(ref cilindrajeInicio, ref cilindrajeCorte);
            request.CilindrajeInicio = cilindrajeInicio;
            request.CilindrajeCorte = cilindrajeCorte;

            request.UserRegistra = NormalizeLike(request.UserRegistra);
            request.UserActualiza = NormalizeLike(request.UserActualiza);
            request.PersonaId = NormalizeLike(request.PersonaId);
            request.Nombre = NormalizeLike(request.Nombre);
            request.IdPrincipal = NormalizeLike(request.IdPrincipal);
            request.IdProvisional = NormalizeLike(request.IdProvisional);
            request.ChasisNumero = NormalizeLike(request.ChasisNumero);
            request.VinMotor = NormalizeLike(request.VinMotor);
            request.Color = NormalizeLike(request.Color);
            request.Filtro = NormalizeLike(request.Filtro);

            NormalizeFechasVence(request);

            return request;
        }

        /// <summary>
        /// Asegura que el valor de inicio no sea mayor que el valor de corte para un rango dado. Si ambos valores tienen valor y el inicio es mayor que el corte, se intercambian para mantener la coherencia del rango. Esta función es útil para evitar errores en la consulta SQL cuando el usuario ingresa un rango en orden inverso, garantizando que siempre se aplique correctamente el filtro entre los valores de inicio y corte, independientemente del orden en que se proporcionen. Si alguno de los valores no tiene valor, no se realiza ninguna acción, permitiendo que la lógica de filtrado maneje esos casos según corresponda.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private static void NormalizeRange(ref decimal? start, ref decimal? end)
        {
            if (!start.HasValue || !end.HasValue) return;
            if (start.Value <= end.Value) return;

            (start, end) = (end, start);
        }

        /// <summary>
        /// Normaliza un valor de texto para su uso en filtros de tipo LIKE. La función recorta los espacios en blanco al inicio y al final del valor, y si el resultado es una cadena vacía o solo espacios, devuelve null. Esto es útil para evitar que se apliquen filtros con valores no significativos (como espacios) que podrían afectar los resultados de la consulta SQL. Al devolver null para valores vacíos o solo espacios, se asegura que esos filtros no se incluyan en la construcción de la consulta, permitiendo que se muestren más resultados en lugar de filtrar por un valor no válido.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string? NormalizeLike(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        /// <summary>
        /// Normaliza las fechas de vencimiento para el filtro de cobertura. Si el filtro de vencimiento está activado pero no se proporcionan ambas fechas (inicio y corte), se desactiva el filtro para evitar errores en la consulta. Si ambas fechas están presentes pero el inicio es mayor que el corte, se intercambian para asegurar que el rango sea válido. Esto garantiza que el filtro de vencimiento se aplique correctamente y evita situaciones donde un rango de fechas mal configurado podría resultar en una consulta sin resultados o con resultados incorrectos. Al manejar estas validaciones y ajustes en esta función, se centraliza la lógica relacionada con las fechas de vencimiento, facilitando su mantenimiento y asegurando un comportamiento consistente en toda la aplicación.
        /// </summary>
        /// <param name="request"></param>
        private static void NormalizeFechasVence(PolizasPeConsultasBuscarRequestDto request)
        {
            if (!request.FiltrarVenceCobertura) return;

            if (!request.VenceInicio.HasValue || !request.VenceCorte.HasValue)
            {
                // Si el front activa el check pero no manda fechas, no filtramos (evita errores).
                request.FiltrarVenceCobertura = false;
                return;
            }

            if (request.VenceInicio.Value > request.VenceCorte.Value)
            {
                (request.VenceInicio, request.VenceCorte) = (request.VenceCorte, request.VenceInicio);
            }
        }

        /// <summary>
        /// Construye las consultas SQL para buscar pólizas PE según los criterios especificados en la solicitud. La función genera tanto la consulta para obtener los datos paginados como la consulta para contar el total de registros que coinciden con los filtros aplicados. Se utilizan parámetros dinámicos para evitar inyecciones SQL y se construyen las cláusulas WHERE, ORDER BY y de paginación según los filtros y opciones de ordenamiento proporcionados en la solicitud. Si la opción de exportar está activada, no se aplica la paginación para obtener todos los registros que coinciden con los criterios.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        private static (string ListSql, string CountSql, DynamicParameters Parameters) BuildBuscarSql(PolizasPeConsultasBuscarRequestDto request, bool esExportar)
        {
            var (offset, pageSize) = GetPaging(request, esExportar);

            var sb = new StringBuilder();
            var p = new DynamicParameters();

            sb.AppendLine(" WHERE PE_ACTIVA = 0");

            AppendIf(sb, request.SoloVencida, " AND PE_VENCIDA = 1 ");

            AppendVenceCobertura(sb, p, request);
            AppendEqualsInt(sb, p, "ID_PRESENTACION", "@PresentacionId", request.PresentacionId);
            AppendEqualsString(sb, p, "ID_COMBUSTIBLE", "@CombustibleId", request.CombustibleId);
            AppendEqualsInt(sb, p, "ID_MODELO", "@ModeloId", request.ModeloId);
            AppendEqualsString(sb, p, "EstadoActual", "@EstadoPersonaId", request.EstadoPersonaId);

            AppendEqualsInt(sb, p, "ANIO", "@Anio", request.Anio);
            AppendEqualsInt(sb, p, "PUERTAS_NUMERO", "@PuertasNumero", request.PuertasNumero);

            AppendUnidadYRango(sb, p, new UnidadRangoFilter
            {
                UnidadColumn = "PESO_UD",
                UnidadParam = "@PesoUd",
                UnidadValue = request.PesoUd,
                RangoColumn = "PESO",
                RangoInicioParam = "@PesoInicio",
                RangoCorteParam = "@PesoCorte",
                RangoInicio = request.PesoInicio,
                RangoCorte = request.PesoCorte
            });
            AppendUnidadYRango(sb, p, new UnidadRangoFilter
            {
                UnidadColumn = "CAPACIDAD_UD",
                UnidadParam = "@CapacidadUd",
                UnidadValue = request.CapacidadUd,

                RangoColumn = "CAPACIDAD",
                RangoInicioParam = "@CapacidadInicio",
                RangoCorteParam = "@CapacidadCorte",
                RangoInicio = request.CapacidadInicio,
                RangoCorte = request.CapacidadCorte
            });

            AppendUnidadYRango(sb, p, new UnidadRangoFilter
            {
                UnidadColumn = "CILINDRAJE_UD",
                UnidadParam = "@CilindrajeUd",
                UnidadValue = request.CilindrajeUd,

                RangoColumn = "CILINDRAJE",
                RangoInicioParam = "@CilindrajeInicio",
                RangoCorteParam = "@CilindrajeCorte",
                RangoInicio = request.CilindrajeInicio,
                RangoCorte = request.CilindrajeCorte
            });
            AppendLike(sb, p, "REGISTRO_USUARIO", "@UserRegistra", request.UserRegistra);
            AppendLike(sb, p, "ACTUALIZA_USUARIO", "@UserActualiza", request.UserActualiza);
            AppendLike(sb, p, "CEDULA", "@PersonaId", request.PersonaId);
            AppendLike(sb, p, "NOMBRE", "@Nombre", request.Nombre);
            AppendLike(sb, p, "ID_PRINCIPAL", "@IdPrincipal", request.IdPrincipal);
            AppendLike(sb, p, "ID_PROVISIONAL", "@IdProvisional", request.IdProvisional);
            AppendLike(sb, p, "CHASIS_NUMERO", "@ChasisNumero", request.ChasisNumero);
            AppendLike(sb, p, "VIN_MOTOR", "@VinMotor", request.VinMotor);
            AppendLike(sb, p, "COLOR", "@Color", request.Color);
            AppendGlobalFiltro(sb, p, request.Filtro);

            var sortField = (request.SortField ?? string.Empty).Trim();
            var field = sortField switch
            {
                "prendaId" => "PRENDA_ID",
                "codPreanalisis" => "COD_PREANALISIS",
                "idSolicitud" => "ID_SOLICITUD",
                "cedula" => "CEDULA",
                "nombre" => "NOMBRE",
                "tipoPrendaDesc" => "TIPO_PRENDA_DESC",
                "descripcion" => "DESCRIPCION",
                "cobertura" => "COBERTURA",
                "porcCobertura" => "PORC_COBERTURA",
                "estadoDesc" => "ESTADO_DESC",
                "idPrincipal" => "ID_PRINCIPAL",
                "idProvisional" => "ID_PROVISIONAL",
                "avaluo" => "AVALUO",
                "valorFiscal" => "VALOR_FISCAL",
                "valorMercado" => "VALOR_MERCADO",
                "creditoMonto" => "CREDITO_MONTO",
                "creditoSaldo" => "CREDITO_SALDO",
                "creditoDivisa" => "CREDITO_DIVISA",
                "registroFecha" => "REGISTRO_FECHA",
                "registroUsuario" => "REGISTRO_USUARIO",
                "actualizaFecha" => "ACTUALIZA_FECHA",
                "actualizaUsuario" => "ACTUALIZA_USUARIO",
                "comercializaDesc" => "COMERCIALIZA_DESC",
                "marcaDesc" => "MARCA_DESC",
                "modeloDesc" => "MODELO_DESC",
                "anio" => "ANIO",
                "presentacionDesc" => "PRESENTACION_DESC",
                "serie" => "SERIE",
                "color" => "COLOR",
                "chasisNumero" => "CHASIS_NUMERO",
                "vinMotor" => "VIN_MOTOR",
                "puertasNumero" => "PUERTAS_NUMERO",
                "peso" => "PESO",
                "capacidad" => "CAPACIDAD",
                "cilindraje" => "CILINDRAJE",
                "tomo" => "TOMO",
                "folio" => "FOLIO",
                "notario" => "NOTARIO",
                "notarioRegistroFecha" => "NOTARIO_REGISTRO_FECHA",
                "polizaMntFormalizacion" => "POLIZA_MNT_FORMALIZACION",
                "polizaRstPlan" => "POLIZA_RST_PLAN",
                "peNumero" => "PE_NUMERO",
                "peVence" => "PE_VENCE",
                "pePrima" => "PE_PRIMA",
                _ => "REGISTRO_FECHA"
            };

            var dir = request.SortOrder == 1 ? "ASC" : "DESC";

            var orderBySql = $" ORDER BY {field} {dir}";
            var whereSql = sb.ToString();
            var pagingSql = esExportar ? string.Empty : " OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            if (!esExportar)
            {
                p.Add("@offset", offset);
                p.Add("@pageSize", pageSize);
            }


            var listSql = $"{SelectBaseQuery}\n{whereSql}\n{orderBySql}\n{pagingSql}";
            var countSql = $"SELECT COUNT(1) FROM vCrd_Prendas_Integral\n{whereSql}";

            return (listSql, countSql, p);


        }

        /// <summary>
        /// Agrega un filtro global a la consulta SQL que permite buscar coincidencias en múltiples columnas utilizando una sola cadena de búsqueda. Si el valor del filtro es nulo o solo contiene espacios, no se agrega ningún filtro. Si el valor es válido, se construye una cláusula SQL que utiliza el operador LIKE para comparar la cadena de búsqueda con varias columnas relevantes (como PRENDA_ID, COD_PREANALISIS, CEDULA, NOMBRE, etc.), permitiendo que el usuario busque por cualquier término que pueda coincidir con esos campos. El parámetro se agrega a los parámetros dinámicos con comodines para permitir coincidencias parciales en cualquier posición dentro de las columnas especificadas.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="p"></param>
        /// <param name="filtro"></param>
        private static void AppendGlobalFiltro(StringBuilder sb, DynamicParameters p, string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return;

            sb.AppendLine(@"
                    AND (
                        CAST(PRENDA_ID AS VARCHAR(50)) LIKE @term OR
                        COD_PREANALISIS LIKE @term OR
                        ID_SOLICITUD LIKE @term OR
                        CEDULA LIKE @term OR
                        NOMBRE LIKE @term OR
                        ID_PRINCIPAL LIKE @term OR
                        ID_PROVISIONAL LIKE @term OR
                        CHASIS_NUMERO LIKE @term OR
                        VIN_MOTOR LIKE @term OR
                        COLOR LIKE @term OR
                        MARCA_DESC LIKE @term OR
                        MODELO_DESC LIKE @term OR
                        PRESENTACION_DESC LIKE @term OR
                        COMERCIALIZA_DESC LIKE @term OR
                        NOTARIO LIKE @term OR
                        TITULAR_NOMBRE LIKE @term OR
                        PE_NUMERO LIKE @term OR
                        A_CEDULA LIKE @term OR
                        (A_APELLIDO_1 + ' ' + A_APELLIDO_2 + ' ' + A_NOMBRE) LIKE @term
                    )");
            p.Add("@term", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Consulta base para obtener los datos de las pólizas PE. Esta consulta selecciona una amplia gama de columnas relacionadas con las prendas, incluyendo información de identificación, descripción, valores, fechas, y detalles específicos de las pólizas PE como número, vencimiento, prima, cobertura, entre otros. La consulta se basa en la vista vCrd_Prendas_Integral, que probablemente consolida información de varias tablas relacionadas con las prendas y sus pólizas. Al construir la consulta base de esta manera, se facilita la aplicación de filtros dinámicos y ordenamientos según los criterios especificados por el usuario en la solicitud.
        /// </summary>
        private const string SelectBaseQuery = @"
            SELECT
                0 AS Btn,
                PRENDA_ID AS PrendaId,
                COD_PREANALISIS AS CodPreanalisis,
                ID_SOLICITUD AS IdSolicitud,
                CEDULA,
                NOMBRE,
                TIPO_PRENDA_DESC AS TipoPrendaDesc,
                DESCRIPCION,
                COBERTURA,
                PORC_COBERTURA AS PorcCobertura,
                ESTADO_DESC AS EstadoDesc,
                ID_PRINCIPAL AS IdPrincipal,
                ID_PROVISIONAL AS IdProvisional,
                AVALUO,
                VALOR_FISCAL AS ValorFiscal,
                VALOR_MERCADO AS ValorMercado,
                CREDITO_MONTO AS CreditoMonto,
                CREDITO_SALDO AS CreditoSaldo,
                CREDITO_DIVISA AS CreditoDivisa,
                REGISTRO_FECHA AS RegistroFecha,
                REGISTRO_USUARIO AS RegistroUsuario,
                ACTUALIZA_FECHA AS ActualizaFecha,
                ACTUALIZA_USUARIO AS ActualizaUsuario,
                COMERCIALIZA_DESC AS ComercializaDesc,
                MARCA_DESC AS MarcaDesc,
                MODELO_DESC AS ModeloDesc,
                ANIO,
                PRESENTACION_DESC AS PresentacionDesc,
                SERIE,
                COLOR,
                CHASIS_NUMERO AS ChasisNumero,
                VIN_MOTOR AS VinMotor,
                PUERTAS_NUMERO AS PuertasNumero,
                PESO,
                CAPACIDAD,
                CILINDRAJE,
                TOMO,
                FOLIO,
                NOTARIO,
                NOTARIO_REGISTRO_FECHA AS NotarioRegistroFecha,
                POLIZA_MNT_FORMALIZACION AS PolizaMntFormalizacion,
                POLIZA_RST_PLAN AS PolizaRstPlan,
                PESO_UD_DESC AS PesoUdDesc,
                CAPACIDAD_UD_DESC AS CapacidadUdDesc,
                CILINDRAJE_UD_DESC AS CilindrajeUdDesc,
                CASE WHEN PE_ACTIVA = 1 THEN 'Sí' ELSE 'No' END AS PeActiva,
                PE_NUMERO AS PeNumero,
                PE_VENCE AS PeVence,
                PE_PRIMA AS PePrima,
                PE_FRECUENCIA AS PeFrecuencia,
                CASE WHEN PE_VENCIDA = 1 THEN 'Sí' ELSE 'No' END AS PeVencida,
                A_CEDULA AS PeCedula,
                (A_APELLIDO_1 + ' ' + A_APELLIDO_2 + ' ' + A_NOMBRE) AS PeNombre,
                PE_COBERTURA AS PeCobertura,
                CASE WHEN TITULAR_TERCERO = 1 THEN 'Sí' ELSE 'No' END AS TitularTercero,
                TITULAR_NOMBRE AS TitularNombre 
        FROM vCrd_Prendas_Integral";
        private static void AppendIf(StringBuilder sb, bool condition, string sql)
        {
            if (condition)
            {
                sb.AppendLine(sql);
            }
        }

        /// <summary>
        /// Agrega condiciones SQL para filtrar 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="p"></param>
        /// <param name="request"></param>
        private static void AppendVenceCobertura(StringBuilder sb, DynamicParameters p, PolizasPeConsultasBuscarRequestDto request)
        {
            if (!request.FiltrarVenceCobertura || !request.VenceInicio.HasValue || !request.VenceCorte.HasValue)
            {
                return;
            }

            // VB6: entre yyyy-mm-dd 00:00:00 y yyyy-mm-dd 23:59:59
            var inicio = request.VenceInicio.Value.Date;
            var corte = request.VenceCorte.Value.Date.AddDays(1).AddTicks(-1);

            sb.AppendLine("AND PE_VENCE BETWEEN @VenceInicio AND @VenceCorte");
            p.Add("@VenceInicio", inicio);
            p.Add("@VenceCorte", corte);
        }

        /// <summary>
        /// Agrega una condición SQL para filtrar por igualdad en una columna específica. Esta función es útil para campos numéricos donde el usuario puede querer buscar coincidencias exactas. La función verifica que el valor proporcionado tenga valor antes de agregar la condición al SQL y el parámetro correspondiente. Si el valor no tiene valor, no se agrega ningún filtro para esa columna, permitiendo que se incluyan todos los registros independientemente del contenido de esa columna.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="p"></param>
        /// <param name="column"></param>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        private static void AppendEqualsInt(StringBuilder sb, DynamicParameters p, string column, string paramName, int? value)
        {
            if (!value.HasValue) return;
            sb.AppendLine($"AND {column} = {paramName}");
            p.Add(paramName, value.Value);
        }

        private const string TodosValue = "TODOS";
        /// <summary>
        /// Agrega una condición SQL para filtrar por igualdad en una columna específica. Esta función es útil para campos de texto donde el usuario puede querer buscar coincidencias exactas. La función verifica que el valor proporcionado no sea nulo o solo espacios antes de agregar la condición al SQL y el parámetro correspondiente. Si el valor es "TODOS" (ignorando mayúsculas), no se aplica ningún filtro para esa columna, permitiendo que se incluyan todos los registros independientemente del contenido de esa columna. Esto facilita la búsqueda cuando el usuario desea ver todos los resultados sin aplicar un filtro específico en ese campo.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="p"></param>
        /// <param name="column"></param>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        private static void AppendEqualsString(StringBuilder sb, DynamicParameters p, string column, string paramName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var trimmed = value.Trim();

            if (string.Equals(trimmed, TodosValue, StringComparison.OrdinalIgnoreCase))
                return;

            sb.AppendLine($"AND {column} = {paramName}");
            p.Add(paramName, trimmed);
        }

        /// <summary>
        /// Agrega una condición SQL para filtrar por coincidencia parcial (LIKE) en una columna específica. Esta función es útil para campos de texto donde el usuario puede querer buscar coincidencias que contengan cierta cadena, sin importar su posición dentro del texto. La función verifica que el valor proporcionado no sea nulo o solo espacios antes de agregar la condición al SQL y el parámetro correspondiente, utilizando comodines (%) para permitir coincidencias parciales. Si el valor es "TODOS", no se aplica ningún filtro para ese campo, permitiendo que se incluyan todos los registros independientemente del contenido de esa columna.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="p"></param>
        /// <param name="column"></param>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        private static void AppendLike(StringBuilder sb, DynamicParameters p, string column, string paramName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            sb.AppendLine($"AND {column} LIKE {paramName}");
            p.Add(paramName, $"%{value.Trim()}%");
        }

        /// <summary>
        /// Calcula el offset y el pageSize para la paginación de la consulta. Si es una exportación, se devuelven valores que indican que no se debe aplicar paginación (offset 0 y pageSize 0). Para consultas normales, se asegura de que el pageSize sea al menos 1 y que el offset no sea negativo, basándose en los valores proporcionados en la solicitud. Esta función centraliza la lógica de paginación, facilitando su mantenimiento y asegurando un comportamiento consistente en toda la aplicación.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        private static (int offset, int pageSize) GetPaging(PolizasPeConsultasBuscarRequestDto request, bool esExportar)
        {
            if (esExportar) return (0, 0);
            var pageSize = Math.Max(1, request.Paginacion.GetValueOrDefault(30));
            var offsetRaw = request.Pagina.GetValueOrDefault(0);
            var offset = Math.Max(0, offsetRaw);


            return (offset, pageSize);
        }

        /// <summary>
        /// Agrega condiciones SQL para filtrar por una unidad y un rango asociado a esa unidad. Esta función es útil para campos como peso, capacidad o cilindraje, donde el usuario puede seleccionar una unidad (por ejemplo, kg, litros, cc) y especificar un rango de valores. La función verifica que se hayan proporcionado tanto la unidad como los valores de inicio y corte del rango antes de agregar las condiciones al SQL y los parámetros correspondientes. Si la unidad es "TODOS", no se aplica ningún filtro para ese campo.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="parameters"></param>
        /// <param name="filter"></param>
        private static void AppendUnidadYRango(
           StringBuilder sb,
           DynamicParameters parameters,
           UnidadRangoFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.UnidadValue)) return;
            if (string.Equals(filter.UnidadValue, TodosValue, StringComparison.OrdinalIgnoreCase))
                return;
            if (!filter.RangoInicio.HasValue || !filter.RangoCorte.HasValue) return;

            sb.AppendLine($"AND {filter.UnidadColumn} = {filter.UnidadParam}");
            sb.AppendLine($"AND {filter.RangoColumn} BETWEEN {filter.RangoInicioParam} AND {filter.RangoCorteParam}");

            parameters.Add(filter.UnidadParam, filter.UnidadValue);
            parameters.Add(filter.RangoInicioParam, filter.RangoInicio.Value);
            parameters.Add(filter.RangoCorteParam, filter.RangoCorte.Value);
        }

        /// <summary>
        /// Construye la respuesta para la búsqueda de pólizas PE, calculando el total de registros y el total del valor de mercado. Esta función se encarga de preparar los datos para que el front-end pueda mostrar la información de manera adecuada, incluyendo los totales necesarios para la paginación y el resumen de resultados.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private static PolizasPeConsultasBuscarResponseDto BuildResponse(List<PolizasPeConsultasDto> rows, int total)
        {
            decimal totalValorMercado = 0m;

            for (var i = 0; i < rows.Count; i++)
            {
                totalValorMercado += rows[i].ValorMercado ?? 0m;
            }

            return new PolizasPeConsultasBuscarResponseDto
            {
                TotalRegistros = total,
                TotalValorMercado = totalValorMercado,
                Items = rows
            };
        }

        /// <summary>
        /// Consulta para obtener los estados de persona. 
        /// </summary>
        private const string QryEstadosPersona = @"
            SELECT
                RTRIM(cod_estado) AS item,
                RTRIM(descripcion) AS descripcion
            FROM afi_Estados_Persona
            ORDER BY descripcion;";

        /// <summary>
        /// Consulta para obtener las presentaciones.
        /// </summary>
        private const string QryPresentaciones = @"
            SELECT
                ID_PRESENTACION AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_PRENDAS_PRESENTACION
            ORDER BY descripcion;";

        /// <summary>
        /// Consulta para obtener los modelos.
        /// </summary>
        private const string QryModelos = @"
            SELECT
                ID_MODELO AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_PRENDAS_MODELOS
            WHERE Activo = 1
            ORDER BY descripcion;";

        /// <summary>
        /// Consulta para obtener los tipos de combustible.     
        /// </summary>
        private const string QryCombustibles = @"
            SELECT
                ID_COMBUSTIBLE AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_PRENDAS_COMBUSTIBLE
            ORDER BY descripcion;";

        /// <summary>
        /// Consulta para obtener las unidades de peso. 
        /// </summary>
        private const string QryUnidadesPeso = @"
            SELECT
                ID_Unidad AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_PRENDAS_uds
            WHERE Peso_Apl = 1
              AND Activa = 1
            ORDER BY descripcion;";

        /// <summary>
        /// Consulta para obtener las unidades de capacidad. 
        /// </summary>
        private const string QryUnidadesCapacidad = @"
            SELECT
                ID_Unidad AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_PRENDAS_uds
            WHERE Capacidad_Apl = 1
              AND Activa = 1
            ORDER BY descripcion;";

        /// <summary>
        /// Consulta para obtener las unidades de cilindraje.
        /// </summary>
        private const string QryUnidadesCilindraje = @"
            SELECT
                ID_Unidad AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_PRENDAS_uds
            WHERE Cilindraje_Apl = 1
              AND Activa = 1
            ORDER BY descripcion;";

        /// <summary>
        /// Ejecuta una consulta de combo genérica y devuelve los resultados como una lista de DropDownListaGenericaModel.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private ErrorDto<List<DropDownListaGenericaModel>> ExecuteComboQuery(int codEmpresa, string query)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de estados de persona para las pólizas PE. Esta información se utiliza para llenar el combo de estados en el filtro de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_EstadosPersona_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryEstadosPersona);

        /// <summary>
        /// Obtiene la lista de presentaciones para las pólizas PE. Esta información se utiliza para llenar el combo de presentaciones en el filtro de búsqueda.    
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Presentaciones_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryPresentaciones);

        /// <summary>
        /// Obtiene la lista de modelos para las pólizas PE. Esta información se utiliza para llenar el combo de modelos en el filtro de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Modelos_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryModelos);

        /// <summary>
        /// Obtiene la lista de combustibles para las pólizas PE. Esta información se utiliza para llenar el combo de combustibles en el filtro de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_Combustibles_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryCombustibles);

        /// <summary>
        /// Obtiene la lista de unidades de peso para las pólizas PE. Esta información se utiliza para llenar el combo de unidades de peso en el filtro de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesPeso_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryUnidadesPeso);

        /// <summary>
        /// Obtiene la lista de unidades de capacidad para las pólizas PE. Esta información se utiliza para llenar el combo de unidades de capacidad en el filtro de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesCapacidad_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryUnidadesCapacidad);

        /// <summary>
        /// Obtiene la lista de unidades de cilindraje para las pólizas PE. Esta información se utiliza para llenar el combo de unidades de cilindraje en el filtro de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizasPeConsultas_UnidadesCilindraje_Obtener(int codEmpresa)
            => ExecuteComboQuery(codEmpresa, QryUnidadesCilindraje);
    }
}
