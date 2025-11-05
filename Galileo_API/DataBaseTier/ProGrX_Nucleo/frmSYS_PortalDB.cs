using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Nucleo.frmSYS_PortalModels;
namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_PortalDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly mSecurityMainDb _Security_MainDB;
        public frmSYS_PortalDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }
        /// <summary>
        /// Obtiene la información de mensajes para el primer tab desde vSys_Notificaciones_Cfg.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<Sys_MensajesPortal_Lista> Sys_MensajesPortal_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<Sys_MensajesPortal_Lista>
            {
                Code = 0,
                Description = "OK",
                Result = new Sys_MensajesPortal_Lista()
            };

            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(connStr);
                string filtroTxt = filtros?.filtro?.Trim() ?? string.Empty;
                var p = new DynamicParameters();

                string where = "";
                if (!string.IsNullOrEmpty(filtroTxt))
                {
                    string like = $"%{filtroTxt.Replace("%", "[%]").Replace("_", "[_]").Replace("'", "''")}%";
                    p.Add("like", like);

                    where = @"
                WHERE (  COD_NOTIFICA LIKE @like
                      OR TITULO       LIKE @like
                      OR SMTP_ID      LIKE @like
                      OR TIPO         LIKE @like
                      OR ISNULL(Tipo_Desc,'') LIKE @like )";
                }
                string MapSort(string? f) => (f ?? "").ToLowerInvariant() switch
                {
                    "codigo" => "COD_NOTIFICA",
                    "titulo" => "TITULO",
                    "smtp_id" => "SMTP_ID",
                    "tipo_formato_cod" => "TIPO",
                    "tipo_formato_desc" => "Tipo_Desc",
                    "activa" => "Activa",
                    _ => "COD_NOTIFICA"
                };
                string orderCol = MapSort(filtros?.sortField);
                string orderDir = (filtros?.sortOrder ?? 1) == 1 ? "ASC" : "DESC";
                int take = filtros?.paginacion ?? 30;
                int off = filtros?.pagina ?? 0;
                if (off < 0) off = 0;

                p.Add("off", off);
                p.Add("take", take);
                string sqlTotal = $@"
            SELECT COUNT(*)
            FROM vSys_Notificaciones_Cfg
            {where};";

                result.Result.total = cn.ExecuteScalar<int>(sqlTotal, p);
                string sql = $@"
            SELECT
                RTRIM(COD_NOTIFICA)                 AS codigo,
                RTRIM(TITULO)                       AS titulo,
                RTRIM(SMTP_ID)                      AS smtp_id,
                RTRIM(TIPO)                         AS tipo_formato_cod,
                RTRIM(ISNULL(Tipo_Desc,''))         AS tipo_formato_desc,
                CAST(Activa AS bit)                 AS activa
            FROM vSys_Notificaciones_Cfg
            {where}
            ORDER BY {orderCol} {orderDir}
            OFFSET @off ROWS FETCH NEXT @take ROWS ONLY;";

                result.Result.lista = cn.Query<Sys_MensajesPortal_ListaItem>(sql, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }


        /// <summary>
        /// Obtiene una lista de mensajes  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<Sys_MensajesPortal_ListaItem>> Sys_MensajesPortal_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var result = new ErrorDTO<List<Sys_MensajesPortal_ListaItem>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<Sys_MensajesPortal_ListaItem>()
            };

            try
            {
                using var connection = new SqlConnection(connStr);
                string where = "";
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    where = " WHERE ( COD_NOTIFICA LIKE '%" + filtros.filtro + "%' " +
                            " OR TITULO LIKE '%" + filtros.filtro + "%' " +
                            " OR SMTP_ID LIKE '%" + filtros.filtro + "%' " +
                            " OR TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR ISNULL(Tipo_Desc,'') LIKE '%" + filtros.filtro + "%' ) ";
                }
                if (string.IsNullOrWhiteSpace(filtros?.sortField))
                    filtros!.sortField = "COD_NOTIFICA";

                string sortDir = (filtros!.sortOrder ?? 1) == 0 ? "DESC" : "ASC";

                string query = $@"
            SELECT
                RTRIM(COD_NOTIFICA)                 AS codigo,
                RTRIM(TITULO)                       AS titulo,
                RTRIM(SMTP_ID)                      AS smtp_id,
                RTRIM(TIPO)                         AS tipo_formato_cod,
                RTRIM(ISNULL(Tipo_Desc,''))         AS tipo_formato_desc,
                CAST(Activa AS bit)                 AS activa
            FROM vSys_Notificaciones_Cfg
            {where}
            ORDER BY {filtros.sortField} {sortDir};";

                result.Result = connection.Query<Sys_MensajesPortal_ListaItem>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Obtiene una lista de detalles de los mensajes por código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDTO<Sys_MensajesPortal_DetalleModel> Sys_MensajesPortal_Detalle_Obtener(int CodEmpresa, string codigo)
        {
            var r = new ErrorDTO<Sys_MensajesPortal_DetalleModel> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);

                var sql = @"
                    SELECT TOP (1)
                        RTRIM(COD_NOTIFICA)         AS codigo,
                        RTRIM(TITULO)               AS titulo,
                        RTRIM(SMTP_ID)              AS smtp_id,
                        RTRIM(TIPO)                 AS tipo_formato_cod,
                        RTRIM(ISNULL(Tipo_Desc,'')) AS tipo_formato_desc,
                        CAST(Activa AS bit)         AS activa,

                        RTRIM(ISNULL(PIE_01,''))    AS pie_01,
                        RTRIM(ISNULL(PIE_02,''))    AS pie_02,

                        RTRIM(ISNULL(IMAGEN_LOCATE,'')) AS imagen_ruta,
                        TRY_CAST(NULLIF(RTRIM(ISNULL(IMAGEN_W,'')),'') AS int) AS imagen_ancho,
                        TRY_CAST(NULLIF(RTRIM(ISNULL(IMAGEN_H,'')),'') AS int) AS imagen_alto,

                        RTRIM(ISNULL(P_PROCEDIMIENTO,'')) AS procedimiento,

                        P_ACTIVACION                AS activacion,
                        RTRIM(ISNULL(Activacion_Desc,'')) AS activacion_desc,

                        P_ACTIVA_FECHA              AS fecha_especifica,
                        P_ACTIVA_DIA                AS dia_del_mes,
                        P_ACTIVA_DIA_FREQ           AS frecuencia_n_dias,
                        P_ACTIVA_DIA_FREQ_INICIA    AS frecuencia_inicio,
                        RTRIM(ISNULL(P_ACTIVA_EVENTO,'')) AS evento_codigo,

                        RTRIM(ISNULL(REGISTRO_USUARIO,'')) AS registro_usuario,
                        REGISTRO_FECHA              AS registro_fecha,
                        RTRIM(ISNULL(MODIFICA_USUARIO,'')) AS modifica_usuario,
                        MODIFICA_FECHA              AS modifica_fecha
                    FROM vSys_Notificaciones_Cfg
                    WHERE COD_NOTIFICA = @codigo
                    ORDER BY COD_NOTIFICA;";

                var dto = cn.QueryFirstOrDefault<Sys_MensajesPortal_DetalleModel>(sql, new { codigo });

                if (dto != null)
                {
                    dto.imagen_ancho = dto.imagen_ancho == 0 ? 600 : dto.imagen_ancho;
                    dto.imagen_alto = dto.imagen_alto == 0 ? 300 : dto.imagen_alto;
                }

                r.Result = dto;
                r.Description = dto == null ? "No existe notificación" : "OK";
                r.Code = dto == null ? 1 : 0;
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }
            return r;
        }

        /// <summary>
        /// Método para guardar la información de un mensaje.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO Sys_MensajesPortal_Mensaje_Guardar(int CodEmpresa, Sys_MensajesPortal_DetalleModel dto, string usuario)
        {
            var r = new ErrorDTO { Code = 0, Description = "OK" };
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            if (dto == null || string.IsNullOrWhiteSpace(dto.codigo))
            {
                r.Code = -1;
                r.Description = "Datos insuficientes para guardar la notificación.";
                return r;
            }

            // -------- Normalización / defaults --------
            dto.imagen_ancho = dto.imagen_ancho <= 0 ? 600 : dto.imagen_ancho;
            dto.imagen_alto = dto.imagen_alto <= 0 ? 300 : dto.imagen_alto;

            switch (char.ToUpperInvariant(dto.activacion))
            {
                case 'M': dto.fecha_especifica = null; dto.dia_del_mes = null; dto.frecuencia_n_dias = null; dto.frecuencia_inicio = null; dto.evento_codigo = null; break;
                case 'F': dto.dia_del_mes = null; dto.frecuencia_n_dias = null; dto.frecuencia_inicio = null; dto.evento_codigo = null; break;
                case 'D':
                    if (dto.dia_del_mes is < 1 or > 32) dto.dia_del_mes = 1;
                    dto.fecha_especifica = null; dto.frecuencia_n_dias = null; dto.frecuencia_inicio = null; dto.evento_codigo = null; break;
                case 'C':
                    if (dto.frecuencia_n_dias is null or < 1) dto.frecuencia_n_dias = 7;
                    dto.fecha_especifica = null; dto.dia_del_mes = null; dto.evento_codigo = null; break;
                case 'E':
                    dto.evento_codigo = string.IsNullOrWhiteSpace(dto.evento_codigo) ? "N/A" : dto.evento_codigo.Trim();
                    dto.fecha_especifica = null; dto.dia_del_mes = null; dto.frecuencia_n_dias = null; dto.frecuencia_inicio = null; break;
                default:
                    dto.activacion = 'M';
                    dto.fecha_especifica = null; dto.dia_del_mes = null; dto.frecuencia_n_dias = null; dto.frecuencia_inicio = null; dto.evento_codigo = null; break;
            }

            const string insertSql = @"
                INSERT INTO SYS_NOTIFICACIONES_CFG
                (
                  COD_NOTIFICA, TITULO, SMTP_ID, TIPO, ACTIVA,
                  PIE_01, PIE_02,
                  P_ACTIVACION, P_PROCEDIMIENTO, P_ACTIVA_FECHA, P_ACTIVA_DIA,
                  P_ACTIVA_DIA_FREQ, P_ACTIVA_DIA_FREQ_INICIA, P_ACTIVA_EVENTO,
                  REGISTRO_FECHA, REGISTRO_USUARIO,
                  IMAGEN_LOCATE, IMAGEN_W, IMAGEN_H
                )
                VALUES
                (
                  @codigo, @titulo, @smtp_id, @tipo_formato_cod, @activa,
                  @pie_01, @pie_02,
                  @activacion, @procedimiento, @fecha_especifica, @dia_del_mes,
                  @frecuencia_n_dias, @frecuencia_inicio, @evento_codigo,
                  GETDATE(), @usuario,
                  @imagen_ruta, @imagen_ancho, @imagen_alto
                );";

                            const string updateSql = @"
                UPDATE SYS_NOTIFICACIONES_CFG
                SET TITULO            = @titulo,
                    SMTP_ID           = @smtp_id,
                    TIPO              = @tipo_formato_cod,
                    ACTIVA            = @activa,
                    PIE_01            = @pie_01,
                    PIE_02            = @pie_02,
                    P_ACTIVACION      = @activacion,
                    P_PROCEDIMIENTO   = @procedimiento,
                    P_ACTIVA_FECHA    = @fecha_especifica,
                    P_ACTIVA_DIA      = @dia_del_mes,
                    P_ACTIVA_DIA_FREQ = @frecuencia_n_dias,
                    P_ACTIVA_DIA_FREQ_INICIA = @frecuencia_inicio,
                    P_ACTIVA_EVENTO   = @evento_codigo,
                    IMAGEN_LOCATE     = @imagen_ruta,
                    IMAGEN_W          = @imagen_ancho,
                    IMAGEN_H          = @imagen_alto,
                    MODIFICA_FECHA    = dbo.mygetdate(),
                    MODIFICA_USUARIO  = @usuario
                WHERE COD_NOTIFICA = @codigo;";

            try
            {
                using var cn = new SqlConnection(connStr);
                cn.Open();
                using var tx = cn.BeginTransaction();
                int exists = cn.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM SYS_NOTIFICACIONES_CFG WHERE COD_NOTIFICA=@codigo",
                    new { codigo = dto.codigo }, tx);
                var p = new DynamicParameters();
                p.Add("codigo", dto.codigo);
                p.Add("titulo", dto.titulo);
                p.Add("smtp_id", dto.smtp_id);
                p.Add("tipo_formato_cod", dto.tipo_formato_cod);
                p.Add("activa", dto.activa);

                p.Add("pie_01", dto.pie_01 ?? "");
                p.Add("pie_02", dto.pie_02 ?? "");

                p.Add("imagen_ruta", dto.imagen_ruta ?? "");
                p.Add("imagen_ancho", dto.imagen_ancho);
                p.Add("imagen_alto", dto.imagen_alto);

                p.Add("procedimiento", dto.procedimiento ?? "");

                p.Add("activacion", dto.activacion);
                p.Add("fecha_especifica", dto.fecha_especifica);
                p.Add("dia_del_mes", dto.dia_del_mes);
                p.Add("frecuencia_n_dias", dto.frecuencia_n_dias);
                p.Add("frecuencia_inicio", dto.frecuencia_inicio);
                p.Add("evento_codigo", dto.evento_codigo ?? "");

                p.Add("usuario", usuario);

                if (exists == 0)
                    cn.Execute(insertSql, p, tx);
                else
                    cn.Execute(updateSql, p, tx);

                tx.Commit();

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = (exists == 0 ? "Registra - WEB" : "Modifica - WEB"),
                    DetalleMovimiento = $"Notificación: {dto.codigo} - {dto.titulo}"
                });
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }

            return r;
        }
        /// <summary>
        /// Método para eliminar la notificación o mensjae por codigo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO Sys_MensajesPortal_Mensaje_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            var r = new ErrorDTO { Code = 0, Description = "OK" };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                cn.Execute("DELETE FROM SYS_NOTIFICACIONES_CFG WHERE COD_NOTIFICA=@codigo", new { codigo });
                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Mensaje : {codigo}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }
            return r;
        }

        /// <summary>
        /// Método para obtener el catalogo de smtp.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<Sys_MensajesPortal_SmtpDto>> Sys_MensajesPortal_Smtps_Obtener(int CodEmpresa)
        {
            var r = new ErrorDTO<List<Sys_MensajesPortal_SmtpDto>> { Code = 0, Result = new() };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"EXEC spSys_SMTPs_AUT_Lista;";
                r.Result = cn.Query(sql).Select(row => new Sys_MensajesPortal_SmtpDto
                {
                    codigo = (row.COD_SMTP as string ?? "").Trim(),
                    descripcion = row.DESCRIPCION is string d ? d : ""
                }).ToList();

                if (r.Result.Count == 0)
                    r.Result.Add(new Sys_MensajesPortal_SmtpDto { codigo = "CNF01", descripcion = "" });

                r.Description = "OK";
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
                r.Result = null;
            }
            return r;
        }

        /// <summary>
        /// Método para obtener el catalogo de formatos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<Sys_MensajesPortal_FormatoDto>> Sys_MensajesPortal_Formatos_Obtener(int CodEmpresa)
        {
            var r = new ErrorDTO<List<Sys_MensajesPortal_FormatoDto>> { Code = 0, Result = new() };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"
                    SELECT DISTINCT
                        RTRIM(TIPO)        AS codigo,
                        RTRIM(Tipo_Desc)   AS descripcion
                    FROM vSys_Notificaciones_Cfg
                    WHERE TIPO IS NOT NULL
                    ORDER BY descripcion;";

                r.Result = cn.Query<Sys_MensajesPortal_FormatoDto>(sql).ToList();
                r.Description = "OK";
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
                r.Result = null;
            }
            return r;
        }

        /// <summary>
        /// Método para obtener el catalogo de activaciones.
        /// </summary>
        /// <returns></returns>
        public ErrorDTO<List<Sys_MensajesPortal_ActivacionDto>> Sys_MensajesPortal_Activaciones_Obtener()
        {
            var r = new ErrorDTO<List<Sys_MensajesPortal_ActivacionDto>>
            {
                Code = 0,
                Result = new List<Sys_MensajesPortal_ActivacionDto>
                {
                    new() { codigo = 'M', descripcion = "Manual" },
                    new() { codigo = 'F', descripcion = "Fecha" },
                    new() { codigo = 'D', descripcion = "Día del Mes" },
                    new() { codigo = 'C', descripcion = "Cada N días" },
                    new() { codigo = 'E', descripcion = "Evento" },
                },
                Description = "OK"
            };
            return r;
        }

        /// <summary>
        /// Método para obtener el catalogo de eventos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<Sys_MensajesPortal_EventoDto>> Sys_MensajesPortal_Eventos_Obtener(int CodEmpresa)
        {
            var r = new ErrorDTO<List<Sys_MensajesPortal_EventoDto>> { Code = 0, Result = new() };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"
                    IF OBJECT_ID('SYS_EVENTOS_NOTIF') IS NOT NULL
                    SELECT RTRIM(COD_EVENTO) AS codigo, RTRIM(DESCRIPCION) AS descripcion
                    FROM SYS_EVENTOS_NOTIF
                    ORDER BY DESCRIPCION;";

                var rows = cn.Query<Sys_MensajesPortal_EventoDto>(sql).ToList();
                r.Result = (rows?.Count > 0)
                    ? rows
                    : new List<Sys_MensajesPortal_EventoDto>
                    {
                        new() { codigo = "BEN", descripcion = "Aprobación de Beneficio y Ayuda Social" },
                        new() { codigo = "EST", descripcion = "Aprobación de Estudio de Crédito" },
                        new() { codigo = "CRD", descripcion = "Aprobación de Crédito" },
                        new() { codigo = "ABO", descripcion = "Aplicación de Abonos" },
                        new() { codigo = "PLA", descripcion = "Aplicación de Deducciones" },
                        new() { codigo = "BAN", descripcion = "Emisión de Pago en Bancos" },
                        new() { codigo = "LIQ", descripcion = "Liquidación de la Persona" },
                        new() { codigo = "RET", descripcion = "Retiros de Ahorros" },
                        new() { codigo = "FIA", descripcion = "Registro de Cobro a Fiadores" },
                        new() { codigo = "CBJ", descripcion = "Registro de Cobro Judicial" },
                        new() { codigo = "INC", descripcion = "Registro de Incobrables" }
                    };

                r.Description = "OK";
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
                r.Result = null;
            }
            return r;
        }

        /// <summary>
        /// Método para obtener la información de portal para ese tab.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<Sys_MensajesPortal_PreferenciasModel> Sys_MensajesPortal_Portal_Obtener(int CodEmpresa)
        {
            var r = new ErrorDTO<Sys_MensajesPortal_PreferenciasModel> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"
                    SELECT TOP (1)
                        RTRIM(ISNULL(LOGO_WEB_SITE,'')) AS logo_url,
                        ISNULL(LOGO_ALTO,  0)           AS logo_alto,
                        ISNULL(LOGO_ANCHO, 0)           AS logo_ancho,
                        RTRIM(ISNULL(COLOR_SET,''))     AS color_set_hex
                    FROM SIF_EMPRESA
                    ORDER BY ID_EMPRESA;";

                r.Result = cn.QueryFirstOrDefault<Sys_MensajesPortal_PreferenciasModel>(sql);
                r.Description = r.Result == null ? "No existe SIF_EMPRESA" : "OK";
                r.Code = r.Result == null ? 1 : 0;
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }
            return r;
        }

        /// <summary>
        /// Método para guardar la información de portal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO Sys_MensajesPortal_Portal_Guardar(int CodEmpresa, Sys_MensajesPortal_PreferenciasModel dto, string usuario)
        {
            var r = new ErrorDTO { Code = 0, Description = "OK" };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                cn.Execute(@"
                    UPDATE SIF_EMPRESA
                    SET LOGO_WEB_SITE = @logo_url,
                        LOGO_ALTO     = @logo_alto,
                        LOGO_ANCHO    = @logo_ancho,
                        COLOR_SET     = @color_set_hex;",
                    new
                    {
                        dto.logo_url,
                        dto.logo_alto,
                        dto.logo_ancho,
                        dto.color_set_hex
                    });
                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = "Actualiza - WEB",
                    DetalleMovimiento = $"Portal: Preferencias (Logo/Color) actualizadas"
                });
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }
            return r;
        }

    }
}
