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
    public class FrmCrPreaConfigDB
    {
        private const int ModuloEstudioCredito = 3;
        private const string TipoCons = "CONS";
        private const string TipoCanc = "CANC";
        private const string TipoTras = "TRAS";
        private const string TipoExam = "EXAM";
        const string MensajeTipoInvalido = "El tipo indicado no es válido.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCrPreaConfigDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }
        /// <summary>
        /// Obtiene la lista paginada de configuración según el tipo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfigListaResult> CR_Prea_Config_Lista_Obtener(int CodEmpresa,string tipo,string filtrosJson)
        {
            var result = new CrPreaConfigListaResult();

            try
            {
                var tipoNorm = NormalizeTipo(tipo);
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                    "spCrd_Prea_Config_Listas",
                    new { Lista = tipoNorm },
                    commandType: CommandType.StoredProcedure)
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
        /// Obtiene la lista completa de configuración según el tipo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfigListaResult> CR_Prea_Config_Lista_Export(int CodEmpresa,string tipo,string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Config_Lista_Obtener(
                CodEmpresa,
                tipo,
                JsonConvert.SerializeObject(filtros));
        }
        /// <summary>
        /// Guarda un registro de configuración según el tipo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Config_Guardar(int CodEmpresa, string usuario,string tipo, CrPreaConfigGuardarRequest request)
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

                var sp = ResolveGuardarSp(tipoNorm);
                var parameters = BuildGuardarParameters(tipoNorm, request, usuarioNorm);

                var spResult = conn.QueryFirstOrDefault<CrPreaSpResultDto>(
                    sp,
                    parameters,
                    commandType: CommandType.StoredProcedure) ?? new CrPreaSpResultDto();

                if (spResult.Pass != 1)
                {
                    return DbHelper.ErrorResponse(spResult.Mensaje, -2);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = spResult.Mensaje,
                    Movimiento = $"{spResult.Movimiento} - WEB",
                    Modulo = ModuloEstudioCredito
                });

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
        /// Elimina un registro de configuración según el tipo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Config_Eliminar(int CodEmpresa,string usuario,string tipo,int id)
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

                var sp = ResolveEliminarSp(tipoNorm);
                var spResult = conn.QueryFirstOrDefault<CrPreaSpResultDto>(
                    sp,
                    new
                    {
                        Id = id,
                        Usuario = usuarioNorm
                    },
                    commandType: CommandType.StoredProcedure) ?? new CrPreaSpResultDto();

                if (spResult.Pass != 1)
                {
                    return DbHelper.ErrorResponse(spResult.Mensaje, -2);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = spResult.Mensaje,
                    Movimiento = $"{spResult.Movimiento} - WEB",
                    Modulo = ModuloEstudioCredito
                });

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
        /// Obtiene la información actual del avalúo CFIA.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaAvaluoCfiaDto> CR_Prea_AvaluoCFIA_Obtener(int CodEmpresa)
        {
            var result = new CrPreaAvaluoCfiaDto();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                result = conn.QueryFirstOrDefault<CrPreaAvaluoCfiaDto>(
                    "spCrdPreaListaAvaluoCFIA",
                    commandType: CommandType.StoredProcedure) ?? new CrPreaAvaluoCfiaDto();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaAvaluoCfiaDto>(ex.Message, -1, result);
            }
        }
        /// <summary>
        /// Guarda la información del avalúo CFIA.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_AvaluoCFIA_Guardar(int CodEmpresa,string usuario,CrPreaAvaluoCfiaGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse("La solicitud es requerida.");
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateAvaluoCfiaRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaModificaAvaluoCFIA",
                    new
                    {
                        VALOR_FORMULA_CRD_HIP = request.valor_formula_crd_hip,
                        VALOR_FORMULA_ASECCSS = request.valor_formula_aseccss,
                        VALOR_PORC_IVA = request.valor_porc_iva,
                        MONTO_HONORARIOS_MIN_IVA = request.monto_honorarios_min_iva,
                        USUARIO_MODIFICA = usuarioNorm
                    },
                    commandType: CommandType.StoredProcedure);

                var detalle = string.Concat(
                    "Fórmula Crédito Hipotecario: ", request.valor_formula_crd_hip.ToString("N2"),
                    ", Fórmula Interna: ", request.valor_formula_aseccss.ToString("N2"),
                    ", IVA: ", request.valor_porc_iva.ToString("N2"),
                    ", Honorarios Mínimos: ", request.monto_honorarios_min_iva.ToString("N2"));

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = detalle,
                    Movimiento = "Registra - WEB",
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }
        /// <summary>
        /// Deserializa el JSON de filtros lazy.
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
        /// Normaliza el tipo de configuración recibido.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string NormalizeTipo(string tipo)
        {
            var tipoNorm = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            return tipoNorm switch
            {
                TipoCons => TipoCons,
                TipoCanc => TipoCanc,
                TipoTras => TipoTras,
                TipoExam => TipoExam,
                _ => throw new InvalidOperationException(MensajeTipoInvalido)
            };
        }
        /// <summary>
        /// Normaliza el usuario de sesión.
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
        /// Convierte una fila dinámica del SP en el DTO unificado de lista.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfigListaData MapListaRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfigListaData
            {
                id = GetInt(row, "ID_PARAM", "ID_REQUISITO"),
                monto_min = GetDecimal(row, "MONTO_MIN"),
                monto_max = GetDecimal(row, "MONTO_MAX"),
                gastos = GetDecimal(row, "GASTOS"),
                honorarios = GetDecimal(row, "HONORARIOS"),
                impuesto = GetDecimal(row, "IMP_TRASPASO"),
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
        /// Aplica el filtro global en memoria a la lista.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfigListaData> ApplyFiltro(IEnumerable<CrPreaConfigListaData> rows,FiltrosLazyLoadData filtros,string tipo)
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
                (tipo == TipoTras && x.impuesto.ToString("N2").Contains(texto, StringComparison.OrdinalIgnoreCase)) ||
                (tipo == TipoExam &&
                    (x.rango_edad.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     x.descripcion_examenes.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     x.edad_min.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                     x.edad_max.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase)))
            );
        }
        /// <summary>
        /// Aplica el ordenamiento en memoria con whitelist de columnas.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfigListaData> ApplySort(IEnumerable<CrPreaConfigListaData> rows,FiltrosLazyLoadData filtros,string tipo)
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
                "impuesto" when tipo == TipoTras => x => x.impuesto,
                "total" when tipo != TipoExam => x => x.total,
                "rango_edad" when tipo == TipoExam => x => x.rango_edad,
                "edad_min" when tipo == TipoExam => x => x.edad_min,
                "edad_max" when tipo == TipoExam => x => x.edad_max,
                "descripcion_examenes" when tipo == TipoExam => x => x.descripcion_examenes,
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
        /// Aplica la paginación lazy a la lista ya filtrada y ordenada.
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

            var skip = pagina;
            return rows.Skip(skip).Take(paginacion);
        }
        /// <summary>
        /// Valida la solicitud de guardado según el tipo indicado.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        private static void ValidateGuardarRequest(string tipo, CrPreaConfigGuardarRequest request)
        {
            var estado = (request.estado ?? string.Empty).Trim().ToUpperInvariant();
            if (estado != "A" && estado != "I")
            {
                throw new InvalidOperationException("El estado indicado no es válido.");
            }

            if (tipo == TipoExam)
            {
                if (request.edad_min > request.edad_max)
                {
                    throw new InvalidOperationException("El rango de edad mínima no puede ser mayor al de edad máxima.");
                }

                if (request.monto_min > request.monto_max)
                {
                    throw new InvalidOperationException("El monto mínimo no puede ser mayor al monto máximo.");
                }

                if (string.IsNullOrWhiteSpace((request.rango_edad ?? string.Empty).Trim()))
                {
                    throw new InvalidOperationException("La descripción del rango de edad es requerida.");
                }

                if (string.IsNullOrWhiteSpace((request.descripcion_examenes ?? string.Empty).Trim()))
                {
                    throw new InvalidOperationException("La descripción de exámenes es requerida.");
                }

                return;
            }

            if (request.monto_min > request.monto_max)
            {
                throw new InvalidOperationException("El monto mínimo no puede ser mayor al monto máximo.");
            }
        }
        /// <summary>
        /// Valida la solicitud del avalúo CFIA.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateAvaluoCfiaRequest(CrPreaAvaluoCfiaGuardarRequest request)
        {
            if (request.valor_formula_crd_hip < 0)
            {
                throw new InvalidOperationException("Valor de la fórmula crédito hipotecario no es válido.");
            }

            if (request.valor_formula_aseccss < 0)
            {
                throw new InvalidOperationException("Valor de la fórmula interna no es válido.");
            }

            if (request.valor_porc_iva < 0)
            {
                throw new InvalidOperationException("Valor del IVA no es válido.");
            }

            if (request.monto_honorarios_min_iva < 0)
            {
                throw new InvalidOperationException("Valor para honorarios mínimos no es válido.");
            }

            if (request.valor_porc_iva > 13)
            {
                throw new InvalidOperationException("Valor del IVA no puede ser mayor al 13%.");
            }
        }
        /// <summary>
        /// Resuelve el nombre del SP de guardado según el tipo indicado.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string ResolveGuardarSp(string tipo)
        {
            return tipo switch
            {
                TipoCanc => "spCrd_Prea_Config_Hipoteca_Cancelacion_Add",
                TipoCons => "spCrd_Prea_Config_Hipoteca_Constitucion_Add",
                TipoTras => "spCrd_Prea_Config_Traspaso_Bienes_Muebles_Add",
                TipoExam => "spCrd_Prea_Config_Examen_Requisito_Add",
                _ => throw new InvalidOperationException(MensajeTipoInvalido)
            };
        }
        /// <summary>
        /// Resuelve el nombre del SP de eliminación según el tipo indicado.
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static string ResolveEliminarSp(string tipo)
        {
            return tipo switch
            {
                TipoCanc => "spCrd_Prea_Config_Hipoteca_Cancelacion_Del",
                TipoCons => "spCrd_Prea_Config_Hipoteca_Constitucion_Del",
                TipoTras => "spCrd_Prea_Config_Traspaso_Bienes_Muebles_Del",
                TipoExam => "spCrd_Prea_Config_Examen_Requisito_Del",
                _ => throw new InvalidOperationException(MensajeTipoInvalido)
            };
        }
        /// <summary>
        /// Construye los parámetros del SP de guardado según el tipo indicado.
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static object BuildGuardarParameters(string tipo,CrPreaConfigGuardarRequest request,string usuario)
        {
            return tipo switch
            {
                TipoCanc => new
                {
                    Id = request.id,
                    MontoMin = request.monto_min,
                    MontoMax = request.monto_max,
                    Gastos = request.gastos,
                    Honorarios = request.honorarios,
                    Estado = request.estado.Trim().ToUpperInvariant(),
                    Usuario = usuario
                },
                TipoCons => new
                {
                    Id = request.id,
                    MontoMin = request.monto_min,
                    MontoMax = request.monto_max,
                    Gastos = request.gastos,
                    Honorarios = request.honorarios,
                    Estado = request.estado.Trim().ToUpperInvariant(),
                    Usuario = usuario
                },
                TipoTras => new
                {
                    Id = request.id,
                    MontoMin = request.monto_min,
                    MontoMax = request.monto_max,
                    Gastos = request.gastos,
                    Honorarios = request.honorarios,
                    Impuesto = request.impuesto,
                    Estado = request.estado.Trim().ToUpperInvariant(),
                    Usuario = usuario
                },
                TipoExam => new
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
                },
                _ => throw new InvalidOperationException(MensajeTipoInvalido)
            };
        }
        /// <summary>
        /// Obtiene el primer valor string disponible según las claves indicadas.
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
        /// Obtiene el primer valor entero disponible según las claves indicadas.
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
        /// Obtiene el primer valor decimal disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static decimal GetDecimal(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            if (value == null)
            {
                return 0m;
            }

            return Convert.ToDecimal(value);
        }
        /// <summary>
        /// Obtiene el primer valor short disponible según las claves indicadas.
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
        /// Obtiene el primer valor fecha disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static DateTime? GetDate(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            if (value == null)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }
        /// <summary>
        /// Convierte la descripción de estado en su código estándar.
        /// </summary>
        /// <param name="estadoDesc"></param>
        /// <returns></returns>
        private static string ResolveEstado(string estadoDesc)
        {
            return estadoDesc.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? "A" : "I";
        }
    }
}