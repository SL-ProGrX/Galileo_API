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

        private const string _numplaca       = "A.NUM_PLACA";
        private const string TipoActivoParam = "@tipo_activo";
        private const string _filtro         = "@filtro";

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
            var sortFieldNorm = (sortFieldRaw ?? _numplaca).Trim().ToUpperInvariant();

            return sortFieldNorm switch
            {
                _numplaca or "NUM_PLACA" => 1,
                "A.NOMBRE" or "NOMBRE"   => 2,
                "A.ESTADO" or "ESTADO"   => 3,
                _                        => 1
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
            var okVence  = TryParseFecha(data.fecha_vence,  "fecha de vencimiento", errores, out var fv);

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
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_poliza) || placas == null || placas.Count == 0)
                    return new ErrorDto { Code = -1, Description = "Datos insuficientes para la operación." };

                using var connection = _portalDB.CreateConnection(CodEmpresa);
                connection.Open();
                using var tx = connection.BeginTransaction();

                foreach (var pl in placas)
                {
                    connection.Execute(sql, new { p = cod_poliza.ToUpper(), pl, u = usuario }, tx);
                }

                tx.Commit();

                RegistrarBitacoraPoliza(
                    CodEmpresa,
                    usuario,
                    cod_poliza,
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
                EmpresaId         = CodEmpresa,
                Usuario           = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento        = movimiento,
                Modulo            = vModulo
            });
        }

        private static ErrorDto<T> CrearErrorDebeIndicarPoliza<T>()
        {
            return new ErrorDto<T>
            {
                Code        = -1,
                Description = "Debe indicar la póliza.",
                Result      = default!
            };
        }

        #endregion

        #region Lista de pólizas

        /// <summary>
        /// Obtener lista de pólizas (paginada y con filtro).
        /// </summary>
        public ErrorDto<ActivosPolizasLista> Activos_PolizasLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosPolizasFiltros>(filtros);

            var response = new ErrorDto<ActivosPolizasLista>
            {
                Code        = 0,
                Description = "Ok",
                Result      = new ActivosPolizasLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                AddFiltroTexto(p, _filtro, vfiltro?.filtro);

                int pagina     = vfiltro?.pagina     ?? 0;
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

        #endregion

        #region Validaciones y obtención de póliza

        /// <summary>
        /// Verifica si una póliza ya existe.
        /// </summary>
        public ErrorDto Activos_PolizasExiste_Obtener(int CodEmpresa, string cod_poliza)
        {
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT COUNT(*) 
                    FROM dbo.ACTIVOS_POLIZAS 
                    WHERE UPPER(COD_POLIZA) = @cod;";

                int result = connection.QueryFirstOrDefault<int>(query, new { cod = (cod_poliza ?? string.Empty).ToUpper() });

                (resp.Code, resp.Description) =
                    (result == 0) ? (0, "POLIZA: Libre") : (-2, "POLIZA: Ocupado");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los detalles de una póliza.
        /// </summary>
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
                parameters: new { cod = (cod_poliza ?? string.Empty).ToUpper() }
            );

            if (result.Code == 0 && result.Result == null)
            {
                result.Code = -2;
                result.Description = "Póliza no encontrada.";
            }
            else if (result.Code == 0)
            {
                result.Description = "Ok";
            }

            return result;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una póliza.
        /// </summary>
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

                int existe = connection.QueryFirstOrDefault<int>(qExiste, new { cod = data.cod_poliza.ToUpper() });

                if (data.isNew)
                {
                    if (existe > 0)
                        resp = new ErrorDto { Code = -2, Description = $"La póliza {data.cod_poliza.ToUpper()} ya existe." };
                    else
                        resp = Activos_Polizas_Insertar(CodEmpresa, data);
                }
                else
                {
                    if (existe == 0)
                        resp = new ErrorDto { Code = -2, Description = $"La póliza {data.cod_poliza.ToUpper()} no existe." };
                    else
                        resp = Activos_Polizas_Actualizar(CodEmpresa, data);
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
                return new ErrorDto { Code = -1, Description = "Datos de póliza no proporcionados." };

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

            return new ErrorDto { Code = 0, Description = "Ok" };
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
                new
                {
                    cod = data.cod_poliza.ToUpper(),
                    tipo = data.tipo_poliza.ToUpper(),
                    descripcion = data.descripcion?.ToUpper(),
                    observacion = string.IsNullOrWhiteSpace(data.observacion) ? null : data.observacion,
                    fi,
                    fv,
                    monto = data.monto,
                    num_poliza = string.IsNullOrWhiteSpace(data.num_poliza) ? null : data.num_poliza,
                    documento = string.IsNullOrWhiteSpace(data.documento) ? null : data.documento,
                    reg_usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? null : data.registro_usuario
                }
            );

            if (result.Code == 0)
            {
                RegistrarBitacoraPoliza(
                    CodEmpresa,
                    data.registro_usuario,
                    data.cod_poliza,
                    data.descripcion,
                    movimiento: "Registra - WEB"
                );

                result.Description = "Póliza Ingresada Satisfactoriamente!";
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
                new
                {
                    cod = data.cod_poliza.ToUpper(),
                    tipo = data.tipo_poliza.ToUpper(),
                    descripcion = data.descripcion?.ToUpper(),
                    observacion = string.IsNullOrWhiteSpace(data.observacion) ? null : data.observacion,
                    fi,
                    fv,
                    monto = data.monto,
                    num_poliza = string.IsNullOrWhiteSpace(data.num_poliza) ? null : data.num_poliza,
                    documento = string.IsNullOrWhiteSpace(data.documento) ? null : data.documento,
                    mod_usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? null : data.modifica_usuario
                }
            );

            if (result.Code == 0)
            {
                RegistrarBitacoraPoliza(
                    CodEmpresa,
                    data.modifica_usuario,
                    data.cod_poliza,
                    data.descripcion,
                    movimiento: "Modifica - WEB"
                );

                result.Description = "Póliza Actualizada Satisfactoriamente!";
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
                Code        = 0,
                Description = "Ok",
                Result      = new ActivosPolizasLista()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@p", cod_poliza.ToUpper());

                AddTipoActivo(p, tipo_activo);
                AddFiltroTexto(p, _filtro, filtros?.filtro);

                // Usamos la misma base de query que en el listado para evitar duplicación
                var queryTotal = "SELECT COUNT(1) " + QueryActivosAsignacionBase + ";";
                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal, p);

                int sortIndex = ObtenerSortIndex(filtros?.sortField);
                int sortDir   = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1;

                int pagina     = filtros?.pagina     ?? 0;
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

                resp.Description = JsonConvert.SerializeObject(filas);
                resp.Result.lista = filas.Select(f => new ActivosPolizasData
                {
                    cod_poliza  = cod_poliza.ToUpper(),
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
            p.Add("@p", cod_poliza.ToUpper());

            AddTipoActivo(p, tipo_activo);
            AddFiltroTexto(p, _filtro, filtros?.filtro);

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
        public ErrorDto Activos_Polizas_Asignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
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
        public ErrorDto Activos_Polizas_Desasignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
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