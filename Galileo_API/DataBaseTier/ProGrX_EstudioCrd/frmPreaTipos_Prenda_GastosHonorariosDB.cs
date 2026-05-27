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
        private const string MensajeTipoInvalido = "El tipo indicado no es vÃ¡lido.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmPreaTiposPrendaGastosHonorariosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de gastos, honorarios o exÃ¡menes prendarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfigListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(int CodEmpresa, string tipo, string filtrosJson)
        {
            var result = new CrPreaConfigListaResult();

            try
            {
                var tipoNorm = NormalizeTipo(tipo);
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                    "EXEC spCrd_Prea_Config_Prenda_Listas @Tipo",
                    new { Tipo = tipoNorm },
                    commandType: CommandType.Text)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapListaRow)
                    .ToList();

                var filtrada = ApplyFiltro(rows, filtros, tipoNorm);
                var ordenada = ApplySort(filtrada, filtros, tipoNorm).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfigListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfigListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfigListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfigListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de gastos, honorarios o exÃ¡menes prendarios para exportaciÃ³n.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfigListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export(int CodEmpresa, string tipo, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(
                CodEmpresa,
                tipo,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda un registro de gastos, honorarios o exÃ¡menes prendarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Guardar(int CodEmpresa, string usuario, string tipo, CrPreaConfigGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse("La solicitud es requerida.");
                }

                var tipoNorm = NormalizeTipo(tipo);
                var usuarioNorm = NormalizeUsuario(usuario);

                ValidateGuardarRequest(tipoNorm, request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var spResult = conn.QueryFirstOrDefault<CrPreaSpResultDto>(
                    BuildGuardarSql(tipoNorm),
                    BuildGuardarParameters(tipoNorm, request, usuarioNorm),
                    commandType: CommandType.Text) ?? new CrPreaSpResultDto();

                if (spResult.Pass != 1)
                {
                    return DbHelper.ErrorResponse(spResult.Mensaje, -2);
                }

                Bitacora(CodEmpresa, usuarioNorm, spResult);

                return DbHelper.OkResponse($"{spResult.Mensaje}, {spResult.Movimiento} satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Elimina un registro de gastos, honorarios o exÃ¡menes prendarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Eliminar(int CodEmpresa, string usuario, string tipo, int id)
        {
            try
            {
                if (id <= 0)
                {
                    return DbHelper.ErrorResponse("El identificador es requerido.", -2);
                }

                var tipoNorm = NormalizeTipo(tipo);
                var usuarioNorm = NormalizeUsuario(usuario);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var spResult = conn.QueryFirstOrDefault<CrPreaSpResultDto>(
                    BuildEliminarSql(tipoNorm),
                    BuildEliminarParameters(tipoNorm, id, usuarioNorm),
                    commandType: CommandType.Text) ?? new CrPreaSpResultDto();

                if (spResult.Pass != 1)
                {
                    return DbHelper.ErrorResponse(spResult.Mensaje, -2);
                }

                Bitacora(CodEmpresa, usuarioNorm, spResult);

                return DbHelper.OkResponse($"{spResult.Mensaje}, Eliminado satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Deserializa los filtros lazy enviados desde Angular.
        /// </summary>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        private static FiltrosLazyLoadData DeserializeFiltros(string filtrosJson)
        {
            if (string.IsNullOrWhiteSpace(filtrosJson))
            {
                return new FiltrosLazyLoadData();
            }

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtrosJson) ?? new FiltrosLazyLoadData();
        }

        /// <summary>
        /// Normaliza el tipo prendario recibido desde Angular.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string NormalizeTipo(string tipo)
        {
            var tipoNorm = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            return tipoNorm switch
            {
                TipoConstitucion => TipoConstitucion,
                TipoTraspaso => TipoTraspaso,
                TipoExamen => TipoExamen,
                _ => throw new InvalidOperationException(MensajeTipoInvalido)
            };
        }

        /// <summary>
        /// Normaliza el usuario de sesiÃ³n.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string NormalizeUsuario(string usuario)
        {
            var usuarioNorm = (usuario ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(usuarioNorm))
            {
                throw new InvalidOperationException("El usuario es requerido.");
            }

            return usuarioNorm;
        }

        /// <summary>
        /// Valida la solicitud de guardado segÃºn el tipo prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        private static void ValidateGuardarRequest(string tipo, CrPreaConfigGuardarRequest request)
        {
            var estado = (request.estado ?? string.Empty).Trim().ToUpperInvariant();
            if (estado != "A" && estado != "I")
            {
                throw new InvalidOperationException("El estado indicado no es vÃ¡lido.");
            }

            if (tipo == TipoExamen)
            {
                if (request.edad_min > request.edad_max)
                {
                    throw new InvalidOperationException("El rango de edad mÃ­nima no puede ser mayor al de edad mÃ¡xima.");
                }

                if (request.monto_min > request.monto_max)
                {
                    throw new InvalidOperationException("El monto mÃ­nimo no puede ser mayor al monto mÃ¡ximo.");
                }

                if (string.IsNullOrWhiteSpace((request.rango_edad ?? string.Empty).Trim()))
                {
                    throw new InvalidOperationException("La descripciÃ³n del rango de edad es requerida.");
                }

                if (string.IsNullOrWhiteSpace((request.descripcion_examenes ?? string.Empty).Trim()))
                {
                    throw new InvalidOperationException("La descripciÃ³n de exÃ¡menes es requerida.");
                }

                return;
            }

            if (request.monto_min > request.monto_max)
            {
                throw new InvalidOperationException("El monto mÃ­nimo no puede ser mayor al monto mÃ¡ximo.");
            }
        }

        /// <summary>
        /// Construye la llamada parametrizada al SP de guardado prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string BuildGuardarSql(string tipo)
        {
            return tipo == TipoExamen
                ? @"EXEC spCrd_Prea_Config_Examen_Prenda_Requisito_Add @Id, @RangoDesc, @EdadMin, @EdadMax, @MontoMin, @MontoMax, @Descripcion, @Estado, @Usuario"
                : @"EXEC spCrd_Prea_Config_Prenda_Add @Id, @MontoMin, @MontoMax, @Gastos, @Honorarios, @Estado, @Usuario, @Tipo";
        }

        /// <summary>
        /// Construye los parÃ¡metros de guardado del SP prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static object BuildGuardarParameters(string tipo, CrPreaConfigGuardarRequest request, string usuario)
        {
            if (tipo == TipoExamen)
            {
                return new
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
                };
            }

            return new
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
        /// Construye la llamada parametrizada al SP de eliminaciÃ³n prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string BuildEliminarSql(string tipo)
        {
            return tipo == TipoExamen
                ? @"EXEC spCrd_Prea_Config_Examen_Prenda_Requisito_Del @Id, @Usuario"
                : @"EXEC spCrd_Prea_Config_Prenda_Del @Id, @Usuario, @Tipo";
        }

        /// <summary>
        /// Construye los parÃ¡metros de eliminaciÃ³n del SP prendario.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static object BuildEliminarParameters(string tipo, int id, string usuario)
        {
            if (tipo == TipoExamen)
            {
                return new
                {
                    Id = id,
                    Usuario = usuario
                };
            }

            return new
            {
                Id = id,
                Usuario = usuario,
                Tipo = tipo
            };
        }

        /// <summary>
        /// Registra en bitÃ¡cora el resultado confirmado por el SP.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="spResult"></param>
        private void Bitacora(int CodEmpresa, string usuario, CrPreaSpResultDto spResult)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = spResult.Mensaje,
                Movimiento = $"{spResult.Movimiento} - WEB",
                Modulo = ModuloEstudioCredito
            });
        }

        /// <summary>
        /// Convierte una fila del SP prendario al DTO usado por Angular.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfigListaData MapListaRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfigListaData
            {
                id = GetInt(row, "ID_PARAM", "ID_REQUISITO", "ID"),
                monto_min = GetDecimal(row, "MONTO_MIN"),
                monto_max = GetDecimal(row, "MONTO_MAX"),
                gastos = GetDecimal(row, "GASTOS"),
                honorarios = GetDecimal(row, "HONORARIOS"),
                total = GetDecimal(row, "TOTAL"),
                rango_edad = GetString(row, "RANGO_EDAD"),
                edad_min = GetShort(row, "EDAD_MIN"),
                edad_max = GetShort(row, "EDAD_MAX"),
                descripcion_examenes = GetString(row, "DESCRIPCION_EXAMENES"),
                estado_desc = GetString(row, "ESTADO_DESC"),
                estado = ResolveEstado(GetString(row, "ESTADO_DESC")),
                registro_usuario = GetString(row, "REGISTRO_USUARIO"),
                registro_fecha = GetDate(row, "REGISTRO_FECHA"),
                modifica_usuario = GetString(row, "MODIFICA_USUARIO"),
                modifica_fecha = GetDate(row, "MODIFICA_FECHA"),
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
        private static IEnumerable<CrPreaConfigListaData> ApplyFiltro(IEnumerable<CrPreaConfigListaData> rows, FiltrosLazyLoadData filtros, string tipo)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            var texto = filtro.ToUpperInvariant();

            return rows.Where(x =>
                x.id.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.monto_min.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.monto_max.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.gastos.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.honorarios.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.total.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.estado_desc.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.registro_usuario.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                x.modifica_usuario.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                (tipo == TipoExamen &&
                    (x.rango_edad.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     x.descripcion_examenes.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     x.edad_min.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     x.edad_max.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase)))
            );
        }

        /// <summary>
        /// Aplica ordenamiento a la lista prendaria con columnas permitidas.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfigListaData> ApplySort(IEnumerable<CrPreaConfigListaData> rows, FiltrosLazyLoadData filtros, string tipo)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfigListaData, object?> keySelector = sortField switch
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

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.id)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.id);
        }

        /// <summary>
        /// Aplica paginaciÃ³n lazy a la lista prendaria.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfigListaData> ApplyPaging(IEnumerable<CrPreaConfigListaData> rows, FiltrosLazyLoadData filtros)
        {
            var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            var paginacion = filtros.paginacion < 0 ? 0 : filtros.paginacion;

            if (paginacion == 0)
            {
                return rows;
            }

            return rows.Skip(pagina).Take(paginacion);
        }

        /// <summary>
        /// Obtiene el primer valor string disponible segÃºn las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static string GetString(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            return Convert.ToString(value)?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el primer valor entero disponible segÃºn las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static int GetInt(IDictionary<string, object?> row, params string[] keys)
        {
            var text = GetString(row, keys);
            return int.TryParse(text, out var value) ? value : 0;
        }

        /// <summary>
        /// Obtiene el primer valor decimal disponible segÃºn las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static decimal GetDecimal(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            return value == null ? 0m : Convert.ToDecimal(value);
        }

        /// <summary>
        /// Obtiene el primer valor short disponible segÃºn las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static short GetShort(IDictionary<string, object?> row, params string[] keys)
        {
            var text = GetString(row, keys);
            return short.TryParse(text, out var value) ? value : (short)0;
        }

        /// <summary>
        /// Obtiene el primer valor fecha disponible segÃºn las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static DateTime? GetDate(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            return value == null ? null : Convert.ToDateTime(value);
        }

        /// <summary>
        /// Convierte la descripciÃ³n de estado en su cÃ³digo estÃ¡ndar.
        /// </summary>
        /// <param name="estadoDesc"></param>
        /// <returns></returns>
        private static string ResolveEstado(string estadoDesc)
        {
            return estadoDesc.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? "A" : "I";
        }
    }
}


