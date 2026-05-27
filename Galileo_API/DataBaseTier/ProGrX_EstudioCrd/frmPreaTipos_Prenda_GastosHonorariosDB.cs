using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaTiposPrendaGastosHonorariosDB
    {
        private const int ModuloEstudioCredito = 3;
        private const string TipoConstitucion = "C";
        private const string TipoTraspaso = "T";
        private const string TipoExamen = "E";
        private const string MensajeTipoInvalido = "El tipo indicado no es válido.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmPreaTiposPrendaGastosHonorariosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de gastos, honorarios o exámenes prendarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfigListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(int CodEmpresa, string tipo, string filtrosJson)
        {
            return ObtenerListaPrendaria(CodEmpresa, tipo, filtrosJson, false);
        }

        /// <summary>
        /// Obtiene la lista completa de gastos, honorarios o exámenes prendarios para exportación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfigListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export(int CodEmpresa, string tipo, string filtrosJson)
        {
            return ObtenerListaPrendaria(CodEmpresa, tipo, filtrosJson, true);
        }

        /// <summary>
        /// Guarda un registro de gastos, honorarios o exámenes prendarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Guardar(int CodEmpresa, string usuario, string tipo, CrPreaConfigGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.");
            }

            try
            {
                var tipoNorm = NormalizarTipo(tipo);
                var usuarioNorm = NormalizarUsuario(usuario);
                ValidarGuardado(tipoNorm, request);

                var resultado = EjecutarSpResultado(
                    CodEmpresa,
                    usuarioNorm,
                    SqlGuardar(tipoNorm),
                    ParametrosGuardar(tipoNorm, request, usuarioNorm));

                return resultado.Pass == 1
                    ? DbHelper.OkResponse($"{resultado.Mensaje}, {resultado.Movimiento} satisfactoriamente!")
                    : DbHelper.ErrorResponse(resultado.Mensaje, -2);
            }
            catch (Exception ex) when (EsErrorControlado(ex))
            {
                return DbHelper.ErrorResponse(ex.Message, ex is SqlException ? -1 : -2);
            }
        }

        /// <summary>
        /// Elimina un registro de gastos, honorarios o exámenes prendarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Eliminar(int CodEmpresa, string usuario, string tipo, int id)
        {
            if (id <= 0)
            {
                return DbHelper.ErrorResponse("El identificador es requerido.", -2);
            }

            try
            {
                var tipoNorm = NormalizarTipo(tipo);
                var usuarioNorm = NormalizarUsuario(usuario);
                var resultado = EjecutarSpResultado(
                    CodEmpresa,
                    usuarioNorm,
                    SqlEliminar(tipoNorm),
                    ParametrosEliminar(tipoNorm, id, usuarioNorm));

                return resultado.Pass == 1
                    ? DbHelper.OkResponse($"{resultado.Mensaje}, Eliminado satisfactoriamente!")
                    : DbHelper.ErrorResponse(resultado.Mensaje, -2);
            }
            catch (Exception ex) when (EsErrorControlado(ex))
            {
                return DbHelper.ErrorResponse(ex.Message, ex is SqlException ? -1 : -2);
            }
        }

        /// <summary>
        /// Ejecuta la consulta prendaria y aplica filtros lazy.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <param name="exportar"></param>
        /// <returns></returns>
        private ErrorDto<CrPreaConfigListaResult> ObtenerListaPrendaria(int CodEmpresa, string tipo, string filtrosJson, bool exportar)
        {
            var result = new CrPreaConfigListaResult();

            try
            {
                var tipoNorm = NormalizarTipo(tipo);
                var filtros = LeerFiltros(filtrosJson);
                if (exportar)
                {
                    filtros.pagina = 0;
                    filtros.paginacion = 0;
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var rows = conn.Query(
                    "EXEC spCrd_Prea_Config_Prenda_Listas @Tipo",
                    new { Tipo = tipoNorm },
                    commandType: CommandType.Text)
                    .Cast<IDictionary<string, object?>>()
                    .Select(FilaALista)
                    .ToList();

                var ordenada = Ordenar(Filtrar(rows, filtros, tipoNorm), filtros, tipoNorm).ToList();
                result.total = ordenada.Count;
                result.lista = Paginar(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex) when (EsErrorControlado(ex))
            {
                return DbHelper.CreateErrorResponse<CrPreaConfigListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Ejecuta un SP prendario de guardado o eliminación y registra bitácora si procede.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="sql"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        private CrPreaSpResultDto EjecutarSpResultado(int CodEmpresa, string usuario, string sql, object parametros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var resultado = conn.QueryFirstOrDefault<CrPreaSpResultDto>(
                sql,
                parametros,
                commandType: CommandType.Text) ?? new CrPreaSpResultDto();

            if (resultado.Pass == 1)
            {
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = resultado.Mensaje,
                    Movimiento = $"{resultado.Movimiento} - WEB",
                    Modulo = ModuloEstudioCredito
                });
            }

            return resultado;
        }

        /// <summary>
        /// Deserializa los filtros lazy enviados desde Angular.
        /// </summary>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        private static FiltrosLazyLoadData LeerFiltros(string filtrosJson)
        {
            return string.IsNullOrWhiteSpace(filtrosJson)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtrosJson) ?? new FiltrosLazyLoadData();
        }

        /// <summary>
        /// Normaliza el tipo prendario recibido desde Angular.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string NormalizarTipo(string tipo)
        {
            var valor = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return valor is TipoConstitucion or TipoTraspaso or TipoExamen
                ? valor
                : throw new InvalidOperationException(MensajeTipoInvalido);
        }

        /// <summary>
        /// Normaliza el usuario de sesión.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string NormalizarUsuario(string usuario)
        {
            var valor = (usuario ?? string.Empty).Trim().ToUpperInvariant();
            return string.IsNullOrWhiteSpace(valor)
                ? throw new InvalidOperationException("El usuario es requerido.")
                : valor;
        }

        /// <summary>
        /// Valida la solicitud de guardado según el tipo prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        private static void ValidarGuardado(string tipo, CrPreaConfigGuardarRequest request)
        {
            var estado = (request.estado ?? string.Empty).Trim().ToUpperInvariant();
            if (estado != "A" && estado != "I")
            {
                throw new InvalidOperationException("El estado indicado no es válido.");
            }

            if (request.monto_min > request.monto_max)
            {
                throw new InvalidOperationException("El monto mínimo no puede ser mayor al monto máximo.");
            }

            if (tipo != TipoExamen)
            {
                return;
            }

            if (request.edad_min > request.edad_max)
            {
                throw new InvalidOperationException("El rango de edad mínima no puede ser mayor al de edad máxima.");
            }

            if (string.IsNullOrWhiteSpace(request.rango_edad?.Trim()))
            {
                throw new InvalidOperationException("La descripción del rango de edad es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.descripcion_examenes?.Trim()))
            {
                throw new InvalidOperationException("La descripción de exámenes es requerida.");
            }
        }

        /// <summary>
        /// Construye la llamada parametrizada al SP de guardado prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string SqlGuardar(string tipo)
        {
            return tipo == TipoExamen
                ? "EXEC spCrd_Prea_Config_Examen_Prenda_Requisito_Add @Id, @RangoDesc, @EdadMin, @EdadMax, @MontoMin, @MontoMax, @Descripcion, @Estado, @Usuario"
                : "EXEC spCrd_Prea_Config_Prenda_Add @Id, @MontoMin, @MontoMax, @Gastos, @Honorarios, @Estado, @Usuario, @Tipo";
        }

        /// <summary>
        /// Construye la llamada parametrizada al SP de eliminación prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string SqlEliminar(string tipo)
        {
            return tipo == TipoExamen
                ? "EXEC spCrd_Prea_Config_Examen_Prenda_Requisito_Del @Id, @Usuario"
                : "EXEC spCrd_Prea_Config_Prenda_Del @Id, @Usuario, @Tipo";
        }

        /// <summary>
        /// Construye los parámetros de guardado del SP prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static object ParametrosGuardar(string tipo, CrPreaConfigGuardarRequest request, string usuario)
        {
            return tipo == TipoExamen
                ? new
                {
                    Id = request.id,
                    RangoDesc = request.rango_edad.Trim(),
                    EdadMin = request.edad_min,
                    EdadMax = request.edad_max,
                    MontoMin = request.monto_min,
                    MontoMax = request.monto_max,
                    Descripcion = request.descripcion_examenes.Trim(),
                    Estado = request.estado.Trim().ToUpperInvariant(),
                    Usuario = usuario
                }
                : new
                {
                    Id = request.id,
                    MontoMin = request.monto_min,
                    MontoMax = request.monto_max,
                    Gastos = request.gastos,
                    Honorarios = request.honorarios,
                    Estado = request.estado.Trim().ToUpperInvariant(),
                    Usuario = usuario,
                    Tipo = tipo
                };
        }

        /// <summary>
        /// Construye los parámetros de eliminación del SP prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static object ParametrosEliminar(string tipo, int id, string usuario)
        {
            return tipo == TipoExamen
                ? new { Id = id, Usuario = usuario }
                : new { Id = id, Usuario = usuario, Tipo = tipo };
        }

        /// <summary>
        /// Convierte una fila del SP prendario al DTO usado por Angular.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfigListaData FilaALista(IDictionary<string, object?> row)
        {
            var estadoDesc = Texto(row, "ESTADO_DESC");
            return new CrPreaConfigListaData
            {
                id = Entero(row, "ID_PARAM", "ID_REQUISITO", "ID"),
                monto_min = Decimal(row, "MONTO_MIN"),
                monto_max = Decimal(row, "MONTO_MAX"),
                gastos = Decimal(row, "GASTOS"),
                honorarios = Decimal(row, "HONORARIOS"),
                total = Decimal(row, "TOTAL"),
                rango_edad = Texto(row, "RANGO_EDAD"),
                edad_min = Corto(row, "EDAD_MIN"),
                edad_max = Corto(row, "EDAD_MAX"),
                descripcion_examenes = Texto(row, "DESCRIPCION_EXAMENES"),
                estado_desc = estadoDesc,
                estado = estadoDesc.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? "A" : "I",
                registro_usuario = Texto(row, "REGISTRO_USUARIO"),
                registro_fecha = Fecha(row, "REGISTRO_FECHA"),
                modifica_usuario = Texto(row, "MODIFICA_USUARIO"),
                modifica_fecha = Fecha(row, "MODIFICA_FECHA"),
                isNew = false
            };
        }

        /// <summary>
        /// Aplica filtro global a la lista prendaria en memoria.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfigListaData> Filtrar(IEnumerable<CrPreaConfigListaData> rows, FiltrosLazyLoadData filtros, string tipo)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(row => ValoresBusqueda(row, tipo).Any(valor => valor.Contains(filtro, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Aplica ordenamiento a la lista prendaria con columnas permitidas.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfigListaData> Ordenar(IEnumerable<CrPreaConfigListaData> rows, FiltrosLazyLoadData filtros, string tipo)
        {
            var campo = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var ascendente = filtros.sortOrder == 1;
            var selector = SelectorOrden(campo, tipo);

            return ascendente
                ? rows.OrderBy(selector).ThenBy(x => x.id)
                : rows.OrderByDescending(selector).ThenByDescending(x => x.id);
        }

        /// <summary>
        /// Aplica paginación lazy a la lista prendaria.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfigListaData> Paginar(IEnumerable<CrPreaConfigListaData> rows, FiltrosLazyLoadData filtros)
        {
            var pagina = Math.Max(filtros.pagina, 0);
            var paginacion = Math.Max(filtros.paginacion, 0);
            return paginacion == 0 ? rows : rows.Skip(pagina).Take(paginacion);
        }

        /// <summary>
        /// Retorna los valores disponibles para búsqueda global.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static IEnumerable<string> ValoresBusqueda(CrPreaConfigListaData row, string tipo)
        {
            yield return row.id.ToString();
            yield return row.monto_min.ToString("N2");
            yield return row.monto_max.ToString("N2");
            yield return row.gastos.ToString("N2");
            yield return row.honorarios.ToString("N2");
            yield return row.total.ToString("N2");
            yield return row.estado_desc;
            yield return row.registro_usuario;
            yield return row.modifica_usuario;

            if (tipo == TipoExamen)
            {
                yield return row.rango_edad;
                yield return row.descripcion_examenes;
                yield return row.edad_min.ToString();
                yield return row.edad_max.ToString();
            }
        }

        /// <summary>
        /// Retorna el selector permitido para ordenamiento.
        /// </summary>
        /// <param name="campo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static Func<CrPreaConfigListaData, object?> SelectorOrden(string campo, string tipo)
        {
            return campo switch
            {
                "id" => x => x.id,
                "monto_min" => x => x.monto_min,
                "monto_max" => x => x.monto_max,
                "gastos" => x => x.gastos,
                "honorarios" => x => x.honorarios,
                "total" when tipo != TipoExamen => x => x.total,
                "rango_edad" when tipo == TipoExamen => x => x.rango_edad,
                "edad_min" when tipo == TipoExamen => x => x.edad_min,
                "edad_max" when tipo == TipoExamen => x => x.edad_max,
                "descripcion_examenes" when tipo == TipoExamen => x => x.descripcion_examenes,
                "estado_desc" => x => x.estado_desc,
                "registro_usuario" => x => x.registro_usuario,
                "registro_fecha" => x => x.registro_fecha,
                "modifica_usuario" => x => x.modifica_usuario,
                "modifica_fecha" => x => x.modifica_fecha,
                _ => x => x.id
            };
        }

        /// <summary>
        /// Determina si la excepción corresponde al flujo controlado del DB.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool EsErrorControlado(Exception ex)
        {
            return ex is JsonException or SqlException or InvalidOperationException or ArgumentException;
        }

        private static string Texto(IDictionary<string, object?> row, params string[] keys)
        {
            var valor = Valor(row, keys);
            return Convert.ToString(valor)?.Trim() ?? string.Empty;
        }

        private static int Entero(IDictionary<string, object?> row, params string[] keys)
        {
            return int.TryParse(Texto(row, keys), out var valor) ? valor : 0;
        }

        private static decimal Decimal(IDictionary<string, object?> row, params string[] keys)
        {
            var valor = Valor(row, keys);
            return valor == null ? 0m : Convert.ToDecimal(valor);
        }

        private static short Corto(IDictionary<string, object?> row, params string[] keys)
        {
            return short.TryParse(Texto(row, keys), out var valor) ? valor : (short)0;
        }

        private static DateTime? Fecha(IDictionary<string, object?> row, params string[] keys)
        {
            var valor = Valor(row, keys);
            return valor == null ? null : Convert.ToDateTime(valor);
        }

        private static object? Valor(IDictionary<string, object?> row, params string[] keys)
        {
            return keys
                .Select(key => row.TryGetValue(key, out var valor) ? valor : null)
                .FirstOrDefault(valor => valor != null && valor != DBNull.Value);
        }
    }
}
