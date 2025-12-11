using System.Data;
using System.Globalization;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        private const string ColNumPlaca = "A.NUM_PLACA";
        private const string TipoActivoParam = "@tipo_activo";
        private const string FiltroParam = "@filtro";

        private const string MsgOk = "Ok";
        private const string MsgPolizaLibre = "POLIZA: Libre";
        private const string MsgPolizaOcupada = "POLIZA: Ocupado";
        private const string MsgDebeIndicarPoliza = "Debe indicar la póliza.";
        private const string MsgDatosInsuficientes = "Datos insuficientes para la operación.";
        private const string MsgPolizaNoEncontrada = "Póliza no encontrada.";
        private const string MsgDatosNoProporcionados = "Datos de póliza no proporcionados.";
        private const string MsgPolizaInsertOk = "Póliza Ingresada Satisfactoriamente!";
        private const string MsgPolizaUpdateOk = "Póliza Actualizada Satisfactoriamente!";

        // Formatos de fecha permitidos
        private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd/MM/yyyy" };
        private static readonly CultureInfo DateCulture = CultureInfo.InvariantCulture;
        private const DateTimeStyles DateStyles = DateTimeStyles.None;

        // WHERE común para lista de pólizas (count y página)
        private const string FiltroPolizasWhere = @"
            WHERE
                (@filtro IS NULL OR
                 COD_POLIZA             LIKE @filtro OR
                 DESCRIPCION            LIKE @filtro OR
                 ISNULL(NUM_POLIZA,'')  LIKE @filtro OR
                 ISNULL(DOCUMENTO,'')   LIKE @filtro)";

        // Query base para asignación de activos
        private const string QueryActivosAsignacionBase = @"
            FROM dbo.ACTIVOS_PRINCIPAL A
            LEFT JOIN dbo.ACTIVOS_POLIZAS_ASG X
                   ON X.NUM_PLACA = A.NUM_PLACA
                  AND X.COD_POLIZA = @p
            WHERE
                (@tipo_activo IS NULL OR A.TIPO_ACTIVO = @tipo_activo)
                AND
                (
                    @filtro IS NULL OR
                    A.NUM_PLACA LIKE @filtro OR
                    A.NOMBRE    LIKE @filtro
                )";

        public FrmActivosPolizasDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }


        #region Helpers comunes

        private static string NormalizeCodPoliza(string? cod)
            => (cod ?? string.Empty).Trim().ToUpperInvariant();

        private static void AddFiltroTexto(DynamicParameters p, string paramName, string? valor)
        {
            var texto = (valor ?? string.Empty).Trim();
            p.Add(paramName, string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%");
        }

        private static void AddTipoActivo(DynamicParameters p, string? tipo_activo)
        {
            p.Add(TipoActivoParam, string.IsNullOrWhiteSpace(tipo_activo) ? null : tipo_activo);
        }

        private static int ObtenerSortIndex(string? sortFieldRaw)
        {
            var sortFieldNorm = (sortFieldRaw ?? ColNumPlaca).Trim().ToUpperInvariant();

            return sortFieldNorm switch
            {
                ColNumPlaca or "NUM_PLACA" => 1,
                "A.NOMBRE" or "NOMBRE" => 2,
                "A.ESTADO" or "ESTADO" => 3,
                _ => 1
            };
        }

        /// <summary>
        /// Intenta parsear una fecha y agrega mensaje de error si el formato no es válido.
        /// </summary>
        private static bool TryParseFecha(
            string? fechaStr,
            string nombreCampo,
            List<string> errores,
            out DateTime? fecha)
        {
            fecha = null;

            if (string.IsNullOrWhiteSpace(fechaStr))
                return true;

            if (DateTime.TryParseExact(fechaStr, DateFormats, DateCulture, DateStyles, out var f))
            {
                fecha = f;
                return true;
            }

            errores.Add($"La {nombreCampo} no tiene un formato válido.");
            return false;
        }

        /// <summary>
        /// Valida fechas y devuelve fi/fv y una lista de errores (si los hay).
        /// </summary>
        private static (DateTime? fi, DateTime? fv, List<string> errores) ValidarYObtenerFechas(ActivosPolizasData data)
        {
            var errores = new List<string>();

            var okInicio = TryParseFecha(data.fecha_inicio, "fecha de inicio", errores, out var fi);
            var okVence = TryParseFecha(data.fecha_vence, "fecha de vencimiento", errores, out var fv);

            if (okInicio && okVence && fi.HasValue && fv.HasValue && fv < fi)
                errores.Add("La fecha de vencimiento no puede ser menor a la inicial.");

            return (fi, fv, errores);
        }

        /// <summary>
        /// Obtiene fi/fv sin generar errores (se asume que ya fueron validadas antes).
        /// </summary>
        private static (DateTime? fi, DateTime? fv) ObtenerFechasParaPersistencia(ActivosPolizasData data)
        {
            DateTime? fi = null;
            DateTime? fv = null;

            if (!string.IsNullOrWhiteSpace(data.fecha_inicio) &&
                DateTime.TryParseExact(data.fecha_inicio, DateFormats, DateCulture, DateStyles, out var dti))
            {
                fi = dti;
            }

            if (!string.IsNullOrWhiteSpace(data.fecha_vence) &&
                DateTime.TryParseExact(data.fecha_vence, DateFormats, DateCulture, DateStyles, out var dtv))
            {
                fv = dtv;
            }

            return (fi, fv);
        }

        private static IEnumerable<ActivosPolizasAsignacionItem> ObtenerActivosAsignacion(
            IDbConnection cn,
            DynamicParameters p,
            bool paginar,
            int offset = 0,
            int rows = 0,
            int? sortIndex = null,
            int? sortDir = null)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                SELECT 
                    A.NUM_PLACA AS num_placa,
                    A.NOMBRE    AS nombre,
                    A.ESTADO    AS estado,
                    IIF(X.COD_POLIZA IS NULL, 0, 1) AS asignado
            ");
            sb.Append(QueryActivosAsignacionBase);

            if (paginar)
            {
                sb.Append(@"
                    ORDER BY
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1 THEN A.NUM_PLACA
                                WHEN 2 THEN A.NOMBRE
                                WHEN 3 THEN A.ESTADO
                            END
                        END ASC,
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1 THEN A.NUM_PLACA
                                WHEN 2 THEN A.NOMBRE
                                WHEN 3 THEN A.ESTADO
                            END
                        END DESC
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;");

                p.Add("@offset", offset);
                p.Add("@rows", rows);
                p.Add("@sortIndex", sortIndex ?? 1);
                p.Add("@sortDir", sortDir ?? 0);
            }
            else
            {
                sb.Append(" ORDER BY A.NUM_PLACA;");
            }

            return cn.Query<ActivosPolizasAsignacionItem>(sb.ToString(), p);
        }

        private ErrorDto EjecutarAsignacionMasiva(
            int CodEmpresa,
            string usuario,
            string cod_poliza,
            List<string> placas,
            string sql,
            string movimientoBitacora,
            string detalleAccion)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza) || placas == null || placas.Count == 0)
                    return new ErrorDto { Code = -1, Description = MsgDatosInsuficientes };

                using var connection = _portalDB.CreateConnection(CodEmpresa);
                connection.Open();
                using var tx = connection.BeginTransaction();

                var cod = NormalizeCodPoliza(cod_poliza);

                foreach (var pl in placas)
                {
                    connection.Execute(sql, new { p = cod, pl, u = usuario }, tx);
                }

                tx.Commit();

                RegistrarBitacoraPoliza(
                    CodEmpresa,
                    usuario,
                    cod,
                    descripcion: null,
                    movimiento: movimientoBitacora,
                    detalleExtra: $"{detalleAccion} {placas.Count} activo(s)"
                );
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private void RegistrarBitacoraPoliza(
            int CodEmpresa,
            string? usuario,
            string cod_poliza,
            string? descripcion,
            string movimiento,
            string? detalleExtra = null)
        {
            var detalle = $"Póliza: {cod_poliza}";

            if (!string.IsNullOrWhiteSpace(descripcion))
                detalle += $" - {descripcion}";

            if (!string.IsNullOrWhiteSpace(detalleExtra))
                detalle += $" - {detalleExtra}";

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static ErrorDto<T> CrearErrorDebeIndicarPoliza<T>()
        {
            return new ErrorDto<T>
            {
                Code = -1,
                Description = MsgDebeIndicarPoliza,
                Result = default!
            };
        }

        #endregion


        #region Lista de pólizas

        /// <summary>
        /// Obtiene la lista de pólizas con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPolizasLista> Activos_PolizasLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosPolizasFiltros>(filtros);

            var response = new ErrorDto<ActivosPolizasLista>
            {
                Code = 0,
                Description = MsgOk,
                Result = new ActivosPolizasLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                AddFiltroTexto(p, FiltroParam, vfiltro?.filtro);

                int pagina = vfiltro?.pagina ?? 0;
                int paginacion = vfiltro?.paginacion ?? 50;

                p.Add("@offset", pagina);
                p.Add("@rows", paginacion);

                var sqlCount = $"SELECT COUNT(*) FROM ACTIVOS_POLIZAS {FiltroPolizasWhere};";
                response.Result.total = connection.QueryFirstOrDefault<int>(sqlCount, p);

                var sqlPage = $@"
                    SELECT 
                        COD_POLIZA  AS cod_poliza,
                        DESCRIPCION AS descripcion
                    FROM ACTIVOS_POLIZAS
                    {FiltroPolizasWhere}
                    ORDER BY COD_POLIZA
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                response.Result.lista = connection.Query<ActivosPolizasData>(sqlPage, p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = new List<ActivosPolizasData>();
            }

            return response;
        }


        /// <summary>
        /// Elimina una póliza si no tiene activos asignados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_poliza"></param>
        /// <returns></returns>
        public ErrorDto Activos_Polizas_Eliminar(int codEmpresa, string usuario, string cod_poliza)
        {
            var resp = DbHelper.CreateOkResponse();

            // Normalizamos el código de póliza
            string codigo = cod_poliza.ToUpperInvariant();

            // 1. Validar si la póliza tiene activos asignados
            const string sqlCountAsg = @"
        SELECT COUNT(*) 
        FROM dbo.ACTIVOS_POLIZAS_ASG 
        WHERE COD_POLIZA = @cod;";

            var countResult = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                sqlCountAsg,
                defaultValue: 0,
                parameters: new { cod = codigo }
            );

            if (countResult.Code != 0)
            {
                // Error en la consulta de conteo
                resp.Code = countResult.Code;
                resp.Description = countResult.Description;
                return resp;
            }

            if (countResult.Result > 0)
            {
                resp.Code = -2;
                resp.Description = "La póliza tiene activos asignados. Debe desasignarlos antes de eliminar.";
                return resp;
            }

            // 2. Eliminar la póliza
            const string sqlDelete = @"
        DELETE FROM dbo.ACTIVOS_POLIZAS 
        WHERE COD_POLIZA = @cod_poliza;";

            var deleteResult = DbHelper.ExecuteNonQueryWithResult(
                _portalDB,
                codEmpresa,
                sqlDelete,
                new { cod_poliza = codigo }
            );

            if (deleteResult.Code != 0)
            {
                resp.Code = deleteResult.Code;
                resp.Description = deleteResult.Description;
                return resp;
            }

            if (deleteResult.Result == 0)
            {
                resp.Code = -2;
                resp.Description = $"La póliza {codigo} no existe.";
                return resp;
            }

            // 3. Registrar en bitácora
            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Póliza: {codigo}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                return resp;
            }

            return resp;
        }


        /// <summary>
        /// Lista los tipos de pólizas disponibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Polizas_Tipos_Listar(int codEmpresa)
        {
            const string sql = @"
        SELECT 
            TIPO_POLIZA AS item,
            DESCRIPCION AS descripcion
        FROM dbo.ACTIVOS_POLIZAS_TIPOS
        ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql
            );
        }


        /// <summary>
        /// Lista los tipos de activos disponibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipo_Activo_Listar(int codEmpresa)
        {
            const string sql = @"
        SELECT 
            TIPO_ACTIVO AS item,
            DESCRIPCION AS descripcion
        FROM dbo.ACTIVOS_TIPO_ACTIVO
        ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,   // ya existente
                codEmpresa,
                sql
            );
        }

        #endregion


        #region Validaciones y obtención de póliza

        /// <summary>
        /// Valida si una póliza existe o no.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_poliza"></param>
        /// <returns></returns>
        public ErrorDto Activos_PolizasExiste_Obtener(int CodEmpresa, string cod_poliza)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT COUNT(*) 
                    FROM dbo.ACTIVOS_POLIZAS 
                    WHERE UPPER(COD_POLIZA) = @cod;";

                int result = connection.QueryFirstOrDefault<int>(
                    query,
                    new { cod = NormalizeCodPoliza(cod_poliza) });

                (resp.Code, resp.Description) =
                    result == 0
                        ? (0, MsgPolizaLibre)
                        : (-2, MsgPolizaOcupada);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Obtiene los datos de una póliza.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_poliza"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPolizasData?> Activos_Polizas_Obtener(int CodEmpresa, string cod_poliza)
        {
            const string query = @"
                SELECT
                    p.COD_POLIZA                                         AS cod_poliza,
                    ISNULL(p.TIPO_POLIZA,'')                              AS tipo_poliza,
                    ISNULL(p.DESCRIPCION,'')                              AS descripcion,
                    ISNULL(p.OBSERVACION,'')                              AS observacion,
                    ISNULL(CONVERT(varchar(10), p.FECHA_INICIO,23),'')    AS fecha_inicio,
                    ISNULL(CONVERT(varchar(10), p.FECHA_VENCE,23),'')     AS fecha_vence,
                    ISNULL(p.MONTO,0)                                     AS monto,
                    ISNULL(p.NUM_POLIZA,'')                               AS num_poliza,
                    ISNULL(p.DOCUMENTO,'')                                AS documento,
                    CASE WHEN p.FECHA_VENCE < GETDATE() 
                         THEN 'VENCIDA' ELSE 'ACTIVA' END                 AS estado,
                    ISNULL(t.DESCRIPCION,'')                              AS tipo_poliza_desc,
                    ISNULL(p.REGISTRO_USUARIO,'')                         AS registro_usuario,
                    ISNULL(CONVERT(varchar(19), p.REGISTRO_FECHA,120),'') AS registro_fecha,
                    ISNULL(p.MODIFICA_USUARIO,'')                         AS modifica_usuario,
                    ISNULL(CONVERT(varchar(19), p.MODIFICA_FECHA,120),'') AS modifica_fecha
                FROM dbo.ACTIVOS_POLIZAS p
                LEFT JOIN dbo.ACTIVOS_POLIZAS_TIPOS t ON t.TIPO_POLIZA = p.TIPO_POLIZA
                WHERE p.COD_POLIZA = @cod;";

            var result = DbHelper.ExecuteSingleQuery<ActivosPolizasData?>(
                _portalDB,
                CodEmpresa,
                query,
                defaultValue: null,
                parameters: new { cod = NormalizeCodPoliza(cod_poliza) }
            );

            if (result.Code == 0 && result.Result == null)
            {
                result.Code = -2;
                result.Description = MsgPolizaNoEncontrada;
            }
            else if (result.Code == 0)
            {
                result.Description = MsgOk;
            }

            return result;
        }


        /// <summary>
        /// Guarda (inserta o actualiza) una póliza.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Activos_Polizas_Guardar(int CodEmpresa, ActivosPolizasData data)
        {
            var resp = new ErrorDto { Code = 0, Description = string.Empty };

            try
            {
                var validacion = ValidarDatosPoliza(data);
                if (validacion.Code != 0)
                    return validacion;

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string qExiste = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_POLIZAS
                    WHERE COD_POLIZA = @cod;";

                var cod = NormalizeCodPoliza(data.cod_poliza);
                int existe = connection.QueryFirstOrDefault<int>(qExiste, new { cod });

                if (data.isNew)
                {
                    if (existe > 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La póliza {cod} ya existe.";
                    }
                    else
                    {
                        resp = Activos_Polizas_Insertar(CodEmpresa, data);
                    }
                }
                else
                {
                    if (existe == 0)
                    {
                        resp.Code = -2;
                        resp.Description = $"La póliza {cod} no existe.";
                    }
                    else
                    {
                        resp = Activos_Polizas_Actualizar(CodEmpresa, data);
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        /// <summary>
        /// Valida los datos de la póliza (incluyendo fechas).
        /// </summary>
        private ErrorDto ValidarDatosPoliza(ActivosPolizasData data)
        {
            if (data == null)
                return new ErrorDto { Code = -1, Description = MsgDatosNoProporcionados };

            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(data.cod_poliza))
                errores.Add("No ha indicado el código de la póliza.");

            if (string.IsNullOrWhiteSpace(data.descripcion))
                errores.Add("No ha indicado la descripción de la póliza.");

            if (string.IsNullOrWhiteSpace(data.tipo_poliza))
                errores.Add("No ha indicado el tipo de póliza.");

            var (_, _, erroresFechas) = ValidarYObtenerFechas(data);
            errores.AddRange(erroresFechas);

            if (errores.Count > 0)
                return new ErrorDto { Code = -1, Description = string.Join(" | ", errores) };

            return new ErrorDto { Code = 0, Description = MsgOk };
        }

        private object BuildPolizaDbParamsForInsert(ActivosPolizasData data, DateTime? fi, DateTime? fv)
        {
            return new
            {
                cod = NormalizeCodPoliza(data.cod_poliza),
                tipo = data.tipo_poliza.ToUpperInvariant(),
                descripcion = data.descripcion?.ToUpperInvariant(),
                observacion = string.IsNullOrWhiteSpace(data.observacion) ? null : data.observacion,
                fi,
                fv,
                monto = data.monto,
                num_poliza = string.IsNullOrWhiteSpace(data.num_poliza) ? null : data.num_poliza,
                documento = string.IsNullOrWhiteSpace(data.documento) ? null : data.documento,
                reg_usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? null : data.registro_usuario
            };
        }

        private object BuildPolizaDbParamsForUpdate(ActivosPolizasData data, DateTime? fi, DateTime? fv)
        {
            return new
            {
                cod = NormalizeCodPoliza(data.cod_poliza),
                tipo = data.tipo_poliza.ToUpperInvariant(),
                descripcion = data.descripcion?.ToUpperInvariant(),
                observacion = string.IsNullOrWhiteSpace(data.observacion) ? null : data.observacion,
                fi,
                fv,
                monto = data.monto,
                num_poliza = string.IsNullOrWhiteSpace(data.num_poliza) ? null : data.num_poliza,
                documento = string.IsNullOrWhiteSpace(data.documento) ? null : data.documento,
                mod_usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? null : data.modifica_usuario
            };
        }

        private ErrorDto Activos_Polizas_Insertar(int CodEmpresa, ActivosPolizasData data)
        {
            const string query = @"
                INSERT INTO dbo.ACTIVOS_POLIZAS
                    (COD_POLIZA, TIPO_POLIZA, DESCRIPCION, OBSERVACION, 
                     FECHA_INICIO, FECHA_VENCE, MONTO, NUM_POLIZA, DOCUMENTO,
                     REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                VALUES
                    (@cod, @tipo, @descripcion, @observacion,
                     @fi, @fv, @monto, @num_poliza, @documento,
                     SYSDATETIME(), @reg_usuario, NULL, NULL);";

            var (fi, fv) = ObtenerFechasParaPersistencia(data);

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                BuildPolizaDbParamsForInsert(data, fi, fv)
            );

            if (result.Code == 0)
            {
                RegistrarBitacoraPoliza(
                    CodEmpresa,
                    data.registro_usuario,
                    NormalizeCodPoliza(data.cod_poliza),
                    data.descripcion,
                    movimiento: "Registra - WEB"
                );

                result.Description = MsgPolizaInsertOk;
            }

            return result;
        }

        private ErrorDto Activos_Polizas_Actualizar(int CodEmpresa, ActivosPolizasData data)
        {
            const string query = @"
                UPDATE dbo.ACTIVOS_POLIZAS
                   SET TIPO_POLIZA      = @tipo,
                       DESCRIPCION      = @descripcion,
                       OBSERVACION      = @observacion,
                       FECHA_INICIO     = @fi,
                       FECHA_VENCE      = @fv,
                       MONTO            = @monto,
                       NUM_POLIZA       = @num_poliza,
                       DOCUMENTO        = @documento,
                       MODIFICA_USUARIO = @mod_usuario,
                       MODIFICA_FECHA   = SYSDATETIME()
                 WHERE COD_POLIZA = @cod;";

            var (fi, fv) = ObtenerFechasParaPersistencia(data);

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                BuildPolizaDbParamsForUpdate(data, fi, fv)
            );

            if (result.Code == 0)
            {
                RegistrarBitacoraPoliza(
                    CodEmpresa,
                    data.modifica_usuario,
                    NormalizeCodPoliza(data.cod_poliza),
                    data.descripcion,
                    movimiento: "Modifica - WEB"
                );

                result.Description = MsgPolizaUpdateOk;
            }

            return result;
        }

        #endregion


        #region Asignación de activos

        /// <summary>
        /// Lista de activos para asignación de una póliza con lazy loading.
        /// </summary>
        public ErrorDto<ActivosPolizasLista> Activos_Polizas_Asignacion_Listar(
            int CodEmpresa,
            string cod_poliza,
            string? tipo_activo,
            FiltrosLazyLoadData filtros)
        {
            if (string.IsNullOrWhiteSpace(cod_poliza))
                return CrearErrorDebeIndicarPoliza<ActivosPolizasLista>();

            var resp = new ErrorDto<ActivosPolizasLista>
            {
                Code = 0,
                Description = MsgOk,
                Result = new ActivosPolizasLista()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@p", NormalizeCodPoliza(cod_poliza));

                AddTipoActivo(p, tipo_activo);
                AddFiltroTexto(p, FiltroParam, filtros?.filtro);

                var queryTotal = "SELECT COUNT(1) " + QueryActivosAsignacionBase + ";";
                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal, p);

                int sortIndex = ObtenerSortIndex(filtros?.sortField);
                int sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1;

                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;

                var filas = ObtenerActivosAsignacion(
                    cn,
                    p,
                    paginar: true,
                    offset: pagina,
                    rows: paginacion,
                    sortIndex: sortIndex,
                    sortDir: sortDir
                ).ToList();

                // Se mantiene la lógica original de colocar el JSON en Description
                resp.Description = JsonConvert.SerializeObject(filas);
                resp.Result.lista = filas.Select(f => new ActivosPolizasData
                {
                    cod_poliza = NormalizeCodPoliza(cod_poliza),
                    descripcion = f.nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = new List<ActivosPolizasData>();
            }

            return resp;
        }

        /// <summary>
        /// Lista de activos sin paginación (export).
        /// </summary>
        public ErrorDto<List<ActivosPolizasAsignacionItem>> Activos_Polizas_Asignacion_Listar_Export(
            int CodEmpresa,
            string cod_poliza,
            string? tipo_activo,
            FiltrosLazyLoadData filtros)
        {
            if (string.IsNullOrWhiteSpace(cod_poliza))
                return CrearErrorDebeIndicarPoliza<List<ActivosPolizasAsignacionItem>>();

            var p = new DynamicParameters();
            p.Add("@p", NormalizeCodPoliza(cod_poliza));

            AddTipoActivo(p, tipo_activo);
            AddFiltroTexto(p, FiltroParam, filtros?.filtro);

            var query = @"
                SELECT 
                    A.NUM_PLACA AS num_placa,
                    A.NOMBRE    AS nombre,
                    A.ESTADO    AS estado,
                    IIF(X.COD_POLIZA IS NULL, 0, 1) AS asignado
            " + QueryActivosAsignacionBase + @"
                ORDER BY A.NUM_PLACA;";

            return DbHelper.ExecuteListQuery<ActivosPolizasAsignacionItem>(
                _portalDB,
                CodEmpresa,
                query,
                p
            );
        }

        /// <summary>
        /// Asigna placas a la póliza.
        /// </summary>
        public ErrorDto Activos_Polizas_Asignar(
            int CodEmpresa,
            string usuario,
            string cod_poliza,
            List<string> placas)
        {
            const string insert = @"
                IF NOT EXISTS(SELECT 1 FROM dbo.ACTIVOS_POLIZAS_ASG WHERE COD_POLIZA=@p AND NUM_PLACA=@pl)
                INSERT INTO dbo.ACTIVOS_POLIZAS_ASG (COD_POLIZA, NUM_PLACA, REGISTRO_FECHA, REGISTRO_USUARIO)
                VALUES (@p, @pl, SYSDATETIME(), @u);";

            return EjecutarAsignacionMasiva(
                CodEmpresa,
                usuario,
                cod_poliza,
                placas,
                insert,
                movimientoBitacora: "Asigna - WEB",
                detalleAccion: "Asigna"
            );
        }

        /// <summary>
        /// Desasigna placas de la póliza.
        /// </summary>
        public ErrorDto Activos_Polizas_Desasignar(
            int CodEmpresa,
            string usuario,
            string cod_poliza,
            List<string> placas)
        {
            const string delete = @"
                DELETE FROM dbo.ACTIVOS_POLIZAS_ASG
                WHERE COD_POLIZA=@p AND NUM_PLACA=@pl;";

            return EjecutarAsignacionMasiva(
                CodEmpresa,
                usuario,
                cod_poliza,
                placas,
                delete,
                movimientoBitacora: "Desasigna - WEB",
                detalleAccion: "Desasigna"
            );
        }

        #endregion
    }
}