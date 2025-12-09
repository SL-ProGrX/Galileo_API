using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosJustificacionesDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        // SELECT base para detalle de justificación (reutilizado en Obtener y Scroll)
        private const string JustificacionDetalleSelectSql = @"
            SELECT
                j.COD_JUSTIFICACION                                                  AS cod_justificacion,
                ISNULL(j.TIPO,'')                                                    AS tipo,
                ISNULL(j.DESCRIPCION,'')                                             AS descripcion,
                ISNULL(j.TIPO_ASIENTO,'')                                            AS tipo_asiento,
                ISNULL(j.COD_CUENTA_01,'')                                           AS cod_cuenta_01,
                ISNULL(j.COD_CUENTA_02,'')                                           AS cod_cuenta_02,
                ISNULL(j.COD_CUENTA_03,'')                                           AS cod_cuenta_03,
                ISNULL(j.COD_CUENTA_04,'')                                           AS cod_cuenta_04,
                ISNULL(j.ESTADO,'')                                                  AS estado,
                ISNULL(j.REGISTRO_USUARIO,'')                                        AS registro_usuario,
                ISNULL(CONVERT(varchar(19), j.REGISTRO_FECHA,120),'')                AS registro_fecha,
                ISNULL(j.MODIFICA_USUARIO,'')                                        AS modifica_usuario,
                ISNULL(CONVERT(varchar(19), j.MODIFICA_FECHA,120),'')                AS modifica_fecha,

                -- Decorados
                ISNULL(ta.DESCRIPCION,'')                                            AS tipo_asiento_desc,

                ISNULL(c1.COD_CUENTA_MASK,'')                                        AS cod_cuenta_01_mask,
                ISNULL(c1.DESCRIPCION,'')                                            AS cod_cuenta_01_desc,

                ISNULL(c2.COD_CUENTA_MASK,'')                                        AS cod_cuenta_02_mask,
                ISNULL(c2.DESCRIPCION,'')                                            AS cod_cuenta_02_desc,

                ISNULL(c3.COD_CUENTA_MASK,'')                                        AS cod_cuenta_03_mask,
                ISNULL(c3.DESCRIPCION,'')                                            AS cod_cuenta_03_desc,

                ISNULL(c4.COD_CUENTA_MASK,'')                                        AS cod_cuenta_04_mask,
                ISNULL(c4.DESCRIPCION,'')                                            AS cod_cuenta_04_desc
            FROM dbo.ACTIVOS_JUSTIFICACIONES j
            LEFT JOIN dbo.CNTX_TIPOS_ASIENTOS ta  ON ta.TIPO_ASIENTO = j.TIPO_ASIENTO
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c1 ON c1.COD_CUENTA = j.COD_CUENTA_01
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c2 ON c2.COD_CUENTA = j.COD_CUENTA_02
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c3 ON c3.COD_CUENTA = j.COD_CUENTA_03
            LEFT JOIN dbo.vCNTX_CUENTAS_LOCAL c4 ON c4.COD_CUENTA = j.COD_CUENTA_04";

        public FrmActivosJustificacionesDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        #region Helpers privados

        private static string NormalizeCode(string? value) =>
            (value ?? string.Empty).ToUpper();

        private void RegistrarBitacora(int CodEmpresa, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static object BuildCuentasParams(ActivosJustificacionesData data, bool esInsert)
        {
            return esInsert
                ? new
                {
                    cod = NormalizeCode(data.cod_justificacion),
                    descripcion = data.descripcion?.ToUpper(),
                    tipo = NormalizeCode(data.tipo),
                    tipo_asiento = data.tipo_asiento?.ToUpper(),
                    cta1 = string.IsNullOrWhiteSpace(data.cod_cuenta_01) ? null : NormalizeCode(data.cod_cuenta_01),
                    cta2 = string.IsNullOrWhiteSpace(data.cod_cuenta_02) ? null : NormalizeCode(data.cod_cuenta_02),
                    cta3 = string.IsNullOrWhiteSpace(data.cod_cuenta_03) ? null : NormalizeCode(data.cod_cuenta_03),
                    cta4 = string.IsNullOrWhiteSpace(data.cod_cuenta_04) ? null : NormalizeCode(data.cod_cuenta_04),
                    reg_usuario = string.IsNullOrWhiteSpace(data.registro_usuario) ? null : data.registro_usuario
                }
                : new
                {
                    cod = NormalizeCode(data.cod_justificacion),
                    descripcion = data.descripcion?.ToUpper(),
                    tipo = NormalizeCode(data.tipo),
                    tipo_asiento = data.tipo_asiento?.ToUpper(),
                    cta1 = string.IsNullOrWhiteSpace(data.cod_cuenta_01) ? null : NormalizeCode(data.cod_cuenta_01),
                    cta2 = string.IsNullOrWhiteSpace(data.cod_cuenta_02) ? null : NormalizeCode(data.cod_cuenta_02),
                    cta3 = string.IsNullOrWhiteSpace(data.cod_cuenta_03) ? null : NormalizeCode(data.cod_cuenta_03),
                    cta4 = string.IsNullOrWhiteSpace(data.cod_cuenta_04) ? null : NormalizeCode(data.cod_cuenta_04),
                    mod_usuario = string.IsNullOrWhiteSpace(data.modifica_usuario) ? null : data.modifica_usuario
                };
        }

        #endregion

        /// <summary>
        /// Obtener lista de justificaciones (paginado + filtro).
        /// </summary>
        public ErrorDto<ActivosJustificacionesLista> Activos_JustificacionesLista_Obtener(
            int CodEmpresa,
            string filtros)
        {
            var response = DbHelper.CreateOkResponse(new ActivosJustificacionesLista
            {
                total = 0,
                lista = new List<ActivosJustificacionesData>()
            });

            ActivosJustificacionesFiltros vfiltro;

            try
            {
                vfiltro = JsonConvert.DeserializeObject<ActivosJustificacionesFiltros>(filtros ?? "{}")
                          ?? new ActivosJustificacionesFiltros();
            }
            catch
            {
                vfiltro = new ActivosJustificacionesFiltros();
            }

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();

                string? filtroLike = string.IsNullOrWhiteSpace(vfiltro.filtro)
                    ? null
                    : $"%{vfiltro.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                bool usarPaginacion = vfiltro.pagina.HasValue && vfiltro.paginacion.HasValue;
                if (usarPaginacion)
                {
                    p.Add("@offset", vfiltro.pagina!.Value, DbType.Int32);
                    p.Add("@rows", vfiltro.paginacion!.Value, DbType.Int32);
                }

                const string whereSql = @"
                    WHERE (@filtro IS NULL
                           OR COD_JUSTIFICACION LIKE @filtro
                           OR DESCRIPCION       LIKE @filtro)";

                string countSql = $"SELECT COUNT(*) FROM ACTIVOS_JUSTIFICACIONES {whereSql}";
                if (response.Result != null)
                    response.Result.total = 0;
                if (connection != null && response.Result != null)
                {
                    response.Result.total = connection.QueryFirstOrDefault<int?>(countSql, p) ?? 0;
                }

                string dataSql = $@"
                    SELECT 
                        COD_JUSTIFICACION AS cod_justificacion, 
                        DESCRIPCION       AS descripcion
                    FROM ACTIVOS_JUSTIFICACIONES
                    {whereSql}
                    ORDER BY COD_JUSTIFICACION";

                if (usarPaginacion)
                {
                    dataSql += @"
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY";
                }

                if (connection != null)
                {
                    if (response.Result != null)
                        response.Result.lista = connection.Query<ActivosJustificacionesData>(dataSql, p).ToList();
                }
                else
                {
                    if (response.Result != null)
                        response.Result.lista = new List<ActivosJustificacionesData>();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                if (response.Result != null)
                {
                    response.Result.total = 0;
                    response.Result.lista = [];
                }
            }

            return response;
        }

        /// <summary>
        /// Verifica si una justificación ya existe en la base de datos.
        /// </summary>
        public ErrorDto Activos_JustificacionesExiste_Obtener(int CodEmpresa, string cod_justificacion)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    SELECT COUNT(*) 
                    FROM dbo.ACTIVOS_JUSTIFICACIONES 
                    WHERE UPPER(COD_JUSTIFICACION) = @cod";

                int result = connection.QueryFirstOrDefault<int>(query, new { cod = NormalizeCode(cod_justificacion) });

                (resp.Code, resp.Description) =
                    (result == 0)
                        ? (0, "JUSTIFICACION: Libre!")
                        : (-2, "JUSTIFICACION: Ocupado!");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los detalles de una justificación específica.
        /// </summary>
        public ErrorDto<ActivosJustificacionesData> Activos_Justificaciones_Obtener(
            int CodEmpresa,
            string cod_justificacion)
        {
            var resp = DbHelper.CreateOkResponse(new ActivosJustificacionesData());

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                string query = $@"
                    {JustificacionDetalleSelectSql}
                    WHERE j.COD_JUSTIFICACION = @cod;";

                resp.Result = connection.QueryFirstOrDefault<ActivosJustificacionesData>(
                    query,
                    new { cod = NormalizeCode(cod_justificacion) });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "Justificación no encontrada.";
                }
                else
                {
                    resp.Description = "Ok";
                }
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
        /// Navegación (scroll) entre justificaciones.
        /// </summary>
        public ErrorDto<ActivosJustificacionesData> Activos_Justificacion_Scroll(int CodEmpresa,int scroll,string? cod_justificacion)
        {
            var resp = DbHelper.CreateOkResponse(new ActivosJustificacionesData());

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var sql = (scroll == 1)
                    ? JustificacionScrollNextSql   // > ASC
                    : JustificacionScrollPrevSql;  // < DESC

                resp.Result = connection.QueryFirstOrDefault<ActivosJustificacionesData>(
                    sql,
                    new { cod = NormalizeCode(cod_justificacion) });

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "No se encontraron más resultados.";
                }
                else
                {
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        private const string JustificacionScrollNextSql = @"
        " + JustificacionDetalleSelectSql + @"
        WHERE j.COD_JUSTIFICACION > @cod
        ORDER BY j.COD_JUSTIFICACION ASC;";

        private const string JustificacionScrollPrevSql = @"
        " + JustificacionDetalleSelectSql + @"
        WHERE j.COD_JUSTIFICACION < @cod
        ORDER BY j.COD_JUSTIFICACION DESC;";



        /// <summary>
        /// Guarda (inserta o actualiza) una justificación en la base de datos.
        /// </summary>
        public ErrorDto Activos_Justificaciones_Guardar(int CodEmpresa, ActivosJustificacionesData data)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                if (data == null)
                    return new ErrorDto { Code = -1, Description = "Datos de justificación no proporcionados." };

                var errores = new List<string>();
                if (string.IsNullOrWhiteSpace(data.cod_justificacion))
                    errores.Add("No ha indicado el código de justificación.");
                if (string.IsNullOrWhiteSpace(data.descripcion))
                    errores.Add("No ha indicado la descripción de la justificación.");

                if (errores.Count > 0)
                {
                    resp.Code = -1;
                    resp.Description = string.Join(" | ", errores);
                    return resp;
                }

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string qExiste = @"
                    SELECT COUNT(1)
                    FROM   dbo.ACTIVOS_JUSTIFICACIONES
                    WHERE  COD_JUSTIFICACION = @cod";

                int existe = connection.QueryFirstOrDefault<int>(
                    qExiste,
                    new { cod = NormalizeCode(data.cod_justificacion) });

                if (data.isNew)
                {
                    resp = (existe > 0)
                        ? new ErrorDto
                        {
                            Code = -2,
                            Description = $"La justificación {NormalizeCode(data.cod_justificacion)} ya existe."
                        }
                        : Activos_Justificaciones_Insertar(CodEmpresa, data);
                }
                else
                {
                    resp = (existe == 0)
                        ? new ErrorDto
                        {
                            Code = -2,
                            Description = $"La justificación {NormalizeCode(data.cod_justificacion)} no existe."
                        }
                        : Activos_Justificaciones_Actualizar(CodEmpresa, data);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private ErrorDto Activos_Justificaciones_Insertar(int CodEmpresa, ActivosJustificacionesData data)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                const string query = @"
                    INSERT INTO dbo.ACTIVOS_JUSTIFICACIONES
                        (COD_JUSTIFICACION, DESCRIPCION, TIPO, TIPO_ASIENTO,
                         COD_CUENTA_01, COD_CUENTA_02, COD_CUENTA_03, COD_CUENTA_04,
                         REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                    VALUES
                        (@cod, @descripcion, @tipo, @tipo_asiento,
                         @cta1, @cta2, @cta3, @cta4,
                         SYSDATETIME(), @reg_usuario, NULL, NULL)";

                var dbResp = DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    query,
                    BuildCuentasParams(data, esInsert: true));

                resp.Code = dbResp.Code;
                resp.Description = dbResp.Description;

                if (resp.Code == 0)
                {
                    RegistrarBitacora(
                        CodEmpresa,
                        data.registro_usuario ?? string.Empty,
                        $"Justificación: {data.cod_justificacion} - {data.descripcion}",
                        "Registra - WEB");

                    resp.Description = "Justificación Ingresada Satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private ErrorDto Activos_Justificaciones_Actualizar(int CodEmpresa, ActivosJustificacionesData data)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                const string query = @"
                    UPDATE dbo.ACTIVOS_JUSTIFICACIONES
                    SET DESCRIPCION      = @descripcion,
                        TIPO             = @tipo,
                        TIPO_ASIENTO     = @tipo_asiento,
                        COD_CUENTA_01    = @cta1,
                        COD_CUENTA_02    = @cta2,
                        COD_CUENTA_03    = @cta3,
                        COD_CUENTA_04    = @cta4,
                        MODIFICA_USUARIO = @mod_usuario,
                        MODIFICA_FECHA   = SYSDATETIME()
                    WHERE COD_JUSTIFICACION = @cod";

                var dbResp = DbHelper.ExecuteNonQuery(
                    _portalDB,
                    CodEmpresa,
                    query,
                    BuildCuentasParams(data, esInsert: false));

                resp.Code = dbResp.Code;
                resp.Description = dbResp.Description;

                if (resp.Code == 0)
                {
                    RegistrarBitacora(
                        CodEmpresa,
                        data.modifica_usuario ?? string.Empty,
                        $"Justificación: {data.cod_justificacion} - {data.descripcion}",
                        "Modifica - WEB");

                    resp.Description = "Justificación Actualizada Satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto Activos_Justificaciones_Eliminar(int CodEmpresa, string usuario, string cod_justificacion)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"
                    DELETE FROM dbo.ACTIVOS_JUSTIFICACIONES 
                    WHERE  COD_JUSTIFICACION = @cod_justificacion";

                int rows = connection.Execute(
                    query,
                    new { cod_justificacion = NormalizeCode(cod_justificacion) });

                if (rows == 0)
                {
                    resp.Code = -2;
                    resp.Description = $"La justificación {NormalizeCode(cod_justificacion)} no existe.";
                    return resp;
                }

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Justificación: {cod_justificacion}",
                    "Elimina - WEB");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTipos_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(
                new List<DropDownListaGenericaModel>
                {
                    new() { item = "A", descripcion = "Adiciones y Mejoras" },
                    new() { item = "R", descripcion = "Retiros (Salidas)" },
                    new() { item = "V", descripcion = "Revaluaciones" },
                    new() { item = "D", descripcion = "Deterioros y Desvalorizaciones" },
                    new() { item = "M", descripcion = "Mantenimiento" }
                });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_JustificacionesTiposAsientos_Obtener(
            int CodEmpresa,
            int contabilidad)
        {
            const string query = @"
                SELECT Tipo_Asiento AS item,
                       descripcion
                FROM   CNTX_TIPOS_ASIENTOS
                WHERE  cod_contabilidad = @contabilidad 
                  AND  ACTIVO = 1
                ORDER BY descripcion ASC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                CodEmpresa,
                query,
                new { contabilidad });
        }
    }
}