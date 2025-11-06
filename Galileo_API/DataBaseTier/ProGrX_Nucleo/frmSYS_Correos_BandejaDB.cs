using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Nucleo.FrmSysCorreosBandejaModels;
namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_Correos_BandejaDB
    {
        private readonly IConfiguration? _config;
        public frmSYS_Correos_BandejaDB(IConfiguration? config)
        {
            _config = config;

        }
        /// <summary>
        /// Obtiene una lista de bandeja de correos con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="para_Buscar"></param>
        /// <param name="asunto_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysCorreosBandejaLista> Correos_Bandeja_Lista_Obtener(
            int CodEmpresa,
            string para_Buscar,
            string asunto_Buscar,
            string fecha_Inicio,
            string fecha_Fin,
            FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var dto = new ErrorDto<SysCorreosBandejaLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysCorreosBandejaLista { total = 0, lista = new() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);


                // Igualamos a la técnica de "Resumen": temp table + select con nombres EXACTOS al POCO
                var sql = @"
                DECLARE @tmp TABLE(
                  ID_EMAIL     INT,
                  COD_SMTP     VARCHAR(50),
                  PARA         VARCHAR(500),
                  ASUNTO       VARCHAR(500),
                  EstadoDesc   VARCHAR(100),
                  FECHA        DATETIME,
                  FECHA_ENVIO  DATETIME,
                  Usuario      VARCHAR(50),
                  Anio         INT,
                  MesId        INT,
                  Mes          VARCHAR(20)
                );

                INSERT INTO @tmp
                EXEC dbo.spSys_Mail_Consulta_General
                     @parametro_para,
                     @parametro_asunto,
                     @fecha_inicio_consulta,
                     @fecha_fin_consulta,
                     @tipo_resultado;

                SELECT
                  ID_EMAIL     AS IdEmail,
                  COD_SMTP     AS CodSmtp,
                  PARA         AS Para,
                  ASUNTO       AS Asunto,
                  EstadoDesc   AS EstadoDesc,
                  FECHA        AS Fecha,
                  FECHA_ENVIO  AS FechaEnvio,
                  Usuario      AS Usuario,
                  Anio         AS Anio,
                  MesId        AS MesId,
                  Mes          AS Mes
                FROM @tmp;";

                var args = new
                {
                    parametro_para = (para_Buscar ?? "").Trim(),
                    parametro_asunto = (asunto_Buscar ?? "").Trim(),
                    fecha_inicio_consulta = (fecha_Inicio ?? "").Trim(),
                    fecha_fin_consulta = (fecha_Fin ?? "").Trim(),
                    tipo_resultado = "D"
                };

                var all = connection.Query<SysCorreosBandejaData>(sql, args, commandTimeout: 60).ToList();

                // --- Filtro texto ---
                var texto = (filtros?.filtro ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    var q = texto.ToUpperInvariant();
                    all = all.Where(x =>
                    (x.Para ?? "").ToUpperInvariant().Contains(q) ||
                    (x.Asunto ?? "").ToUpperInvariant().Contains(q) ||
                    (x.CodSmtp ?? "").ToUpperInvariant().Contains(q) ||
                    (x.EstadoDesc ?? "").ToUpperInvariant().Contains(q) ||
                    (x.Usuario ?? "").ToUpperInvariant().Contains(q) ||
                    (x.Mes ?? "").ToUpperInvariant().Contains(q) ||
                    (x.Anio?.ToString() ?? "").Contains(q) ||
                    (x.MesId?.ToString() ?? "").Contains(q)
                    ).ToList();
                }

                // --- Ordenamiento (1=ASC, 0=DESC) ---
                string campo = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                int orden = filtros?.sortOrder ?? 1;

                Func<SysCorreosBandejaData, object?> key = campo switch
                {
                    "id_mail" or "id_email" or "idemail" => x => x.IdEmail,
                    "cuenta" or "cod_smtp" or "codsmtp" => x => x.CodSmtp,
                    "para" => x => x.Para,
                    "asunto" => x => x.Asunto,
                    "estado" or "estadodesc" => x => x.EstadoDesc,
                    "fecha" => x => x.Fecha ?? DateTime.MinValue,
                    "fecha_envio" or "fechaenvio" => x => x.FechaEnvio ?? DateTime.MinValue,
                    "usuario" => x => x.Usuario,
                    "anio" => x => x.Anio,
                    "mesid" => x => x.MesId,
                    "mes" => x => x.Mes,
                    _ => x => x.Fecha ?? DateTime.MinValue
                };

                all = (orden == 0) ? all.OrderByDescending(key).ToList()
                                   : all.OrderBy(key).ToList();

                // --- Paginación ---
                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int take = Math.Max(1, filtros?.paginacion ?? 30);

                dto.Result.total = all.Count;
                dto.Result.lista = all.Skip(offset).Take(take).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1;
                dto.Description = ex.Message;
                dto.Result.total = 0;
                dto.Result.lista = null;
            }

            return dto;
        }



        /// <summary>
        /// Obtiene una lista de bandeja de correos sin paginación y con filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="para_Buscar"></param>
        /// <param name="asunto_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="filtro_Global"></param>
        /// <returns></returns>
        public ErrorDto<List<SysCorreosBandejaData>> Correos_Bandeja_Obtener(
                   int CodEmpresa,
                   string para_Buscar,
                   string asunto_Buscar,
                   string fecha_Inicio,
                   string fecha_Fin,
                   string filtro_Global)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var dto = new ErrorDto<List<SysCorreosBandejaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var sql = @"
                DECLARE @tmp TABLE(
                  ID_EMAIL     INT,
                  COD_SMTP     VARCHAR(50),
                  PARA         VARCHAR(500),
                  ASUNTO       VARCHAR(500),
                  EstadoDesc   VARCHAR(100),
                  FECHA        DATETIME,
                  FECHA_ENVIO  DATETIME,
                  Usuario      VARCHAR(50),
                  Anio         INT,
                  MesId        INT,
                  Mes          VARCHAR(20)
                );

                INSERT INTO @tmp
                EXEC dbo.spSys_Mail_Consulta_General
                     @parametro_para,
                     @parametro_asunto,
                     @fecha_inicio_consulta,
                     @fecha_fin_consulta,
                     @tipo_resultado;

                SELECT
                  ID_EMAIL     AS IdEmail,
                  COD_SMTP     AS CodSmtp,
                  PARA         AS Para,
                  ASUNTO       AS Asunto,
                  EstadoDesc   AS EstadoDesc,
                  FECHA        AS Fecha,
                  FECHA_ENVIO  AS FechaEnvio,
                  Usuario      AS Usuario,
                  Anio         AS Anio,
                  MesId        AS MesId,
                  Mes          AS Mes
                FROM @tmp;";

                var args = new
                {
                    parametro_para = (para_Buscar ?? "").Trim(),
                    parametro_asunto = (asunto_Buscar ?? "").Trim(),
                    fecha_inicio_consulta = (fecha_Inicio ?? "").Trim(),
                    fecha_fin_consulta = (fecha_Fin ?? "").Trim(),
                    tipo_resultado = "D"
                };

                var datos = connection.Query<SysCorreosBandejaData>(sql, args, commandTimeout: 60).ToList();

                var texto = (filtro_Global ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    var q = texto.ToUpperInvariant();
                    datos = datos.Where(x =>
                        (x.Para ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Asunto ?? "").ToUpperInvariant().Contains(q) ||
                        (x.CodSmtp ?? "").ToUpperInvariant().Contains(q) ||
                        (x.EstadoDesc ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Usuario ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Mes ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Anio?.ToString() ?? "").Contains(q) ||
                        (x.MesId?.ToString() ?? "").Contains(q)
                    ).ToList();
                }

                dto.Result = datos;
            }
            catch (Exception ex)
            {
                dto.Code = -1;
                dto.Description = ex.Message;
                dto.Result = null;
            }

            return dto;
        }
        /// <summary>
        /// Obtiene una lista de bandeja de correos resumen con paginación y con filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="para_Buscar"></param>
        /// <param name="asunto_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysCorreosBandejaResumenLista> Correos_Bandeja_Resumen_Lista_Obtener(int CodEmpresa, string para_Buscar, string asunto_Buscar, string fecha_Inicio, string fecha_Fin, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var dto = new ErrorDto<SysCorreosBandejaResumenLista> { Code = 0, Description = "Ok", Result = new SysCorreosBandejaResumenLista { total = 0, lista = new() } };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var sql = @"
            exec dbo.spSys_Mail_Consulta_General
                 @parametro_para,
                 @parametro_asunto,
                 @fecha_inicio_consulta,
                 @fecha_fin_consulta,
                 @tipo_resultado";
                var args = new
                {
                    parametro_para = (para_Buscar ?? "").Trim(),
                    parametro_asunto = (asunto_Buscar ?? "").Trim(),
                    fecha_inicio_consulta = (fecha_Inicio ?? "").Trim(),
                    fecha_fin_consulta = (fecha_Fin ?? "").Trim(),
                    tipo_resultado = "R"
                };

                var all = connection.Query<SysCorreosBandejaResumenData>(sql, args).ToList();

                var texto = (filtros?.filtro ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    var q = texto.ToUpperInvariant();
                    all = all.Where(x =>
                        (x.Cod_Smtp ?? "").ToUpperInvariant().Contains(q) ||
                        (x.EstadoDesc ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Mes ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Anio?.ToString() ?? "").Contains(q) ||
                        (x.MesId?.ToString() ?? "").Contains(q) ||
                        (x.Correos.ToString()).Contains(q)
                    ).ToList();
                }

                string campo = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                int orden = filtros?.sortOrder ?? 1; // 1=ASC, 0=DESC
                Func<SysCorreosBandejaResumenData, object?> key = campo switch
                {
                    "cod_smtp" => x => x.Cod_Smtp,
                    "correos" => x => x.Correos,
                    "estadodesc" => x => x.EstadoDesc,
                    "anio" => x => x.Anio,
                    "mesid" => x => x.MesId,
                    "mes" => x => x.Mes,
                    _ => x => x.Cod_Smtp
                };
                all = (orden == 0) ? all.OrderByDescending(key).ToList() : all.OrderBy(key).ToList();

                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int take = Math.Max(1, filtros?.paginacion ?? 30);

                dto.Result.total = all.Count;
                dto.Result.lista = all.Skip(offset).Take(take).ToList();
            }
            catch (Exception ex)
            {
                dto.Code = -1; dto.Description = ex.Message; dto.Result.total = 0; dto.Result.lista = null;
            }
            return dto;
        }
        /// <summary>
        /// Obtiene una lista de bandeja de correos resumen sin paginación y con filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="para_Buscar"></param>
        /// <param name="asunto_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="filtro_Global"></param>
        /// <returns></returns>
        public ErrorDto<List<SysCorreosBandejaResumenData>> Correos_Bandeja_Resumen_Obtener(int CodEmpresa, string para_Buscar, string asunto_Buscar, string fecha_Inicio, string fecha_Fin, string filtro_Global)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var dto = new ErrorDto<List<SysCorreosBandejaResumenData>> { Code = 0, Description = "Ok", Result = new() };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var sql = @"
            exec dbo.spSys_Mail_Consulta_General
                 @parametro_para,
                 @parametro_asunto,
                 @fecha_inicio_consulta,
                 @fecha_fin_consulta,
                 @tipo_resultado";
                var args = new
                {
                    parametro_para = (para_Buscar ?? "").Trim(),
                    parametro_asunto = (asunto_Buscar ?? "").Trim(),
                    fecha_inicio_consulta = (fecha_Inicio ?? "").Trim(),
                    fecha_fin_consulta = (fecha_Fin ?? "").Trim(),
                    tipo_resultado = "R"
                };

                var datos = connection.Query<SysCorreosBandejaResumenData>(sql, args).ToList();

                var texto = (filtro_Global ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    var q = texto.ToUpperInvariant();
                    datos = datos.Where(x =>
                        (x.Cod_Smtp ?? "").ToUpperInvariant().Contains(q) ||
                        (x.EstadoDesc ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Mes ?? "").ToUpperInvariant().Contains(q) ||
                        (x.Anio?.ToString() ?? "").Contains(q) ||
                        (x.MesId?.ToString() ?? "").Contains(q) ||
                        (x.Correos.ToString()).Contains(q)
                    ).ToList();
                }

                dto.Result = datos;
            }
            catch (Exception ex)
            {
                dto.Code = -1; dto.Description = ex.Message; dto.Result = null;
            }
            return dto;
        }


    }
}
