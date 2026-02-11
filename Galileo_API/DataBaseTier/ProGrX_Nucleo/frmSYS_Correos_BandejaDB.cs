using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.DataBaseTier;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysCorreosBandejaDB
    {
        private readonly PortalDB _portalDb;
        public FrmSysCorreosBandejaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        private const int DefaultTimeout = 60;

        private const string SqlDetalle = @"
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

        private const string SqlResumen = @"
            exec dbo.spSys_Mail_Consulta_General
                 @parametro_para,
                 @parametro_asunto,
                 @fecha_inicio_consulta,
                 @fecha_fin_consulta,
                 @tipo_resultado";

        private static object BuildArgs(string para_Buscar, string asunto_Buscar, string fecha_Inicio, string fecha_Fin, string tipo)
            => new
            {
                parametro_para = (para_Buscar ?? "").Trim(),
                parametro_asunto = (asunto_Buscar ?? "").Trim(),
                fecha_inicio_consulta = (fecha_Inicio ?? "").Trim(),
                fecha_fin_consulta = (fecha_Fin ?? "").Trim(),
                tipo_resultado = tipo
            };

        private static List<SysCorreosBandejaData> QueryDetalle(SqlConnection connection, object args)
            => connection.Query<SysCorreosBandejaData>(SqlDetalle, args, commandTimeout: DefaultTimeout).ToList();

        private static List<SysCorreosBandejaResumenData> QueryResumen(SqlConnection connection, object args)
            => connection.Query<SysCorreosBandejaResumenData>(SqlResumen, args, commandTimeout: DefaultTimeout).ToList();

        private static List<SysCorreosBandejaData> ApplyFiltroDetalle(List<SysCorreosBandejaData> datos, string? texto)
        {
            var t = (texto ?? "").Trim();
            if (string.IsNullOrWhiteSpace(t))
                return datos;

            var q = t.ToUpperInvariant();
            return datos.Where(x =>
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

        private static List<SysCorreosBandejaResumenData> ApplyFiltroResumen(List<SysCorreosBandejaResumenData> datos, string? texto)
        {
            var t = (texto ?? "").Trim();
            if (string.IsNullOrWhiteSpace(t))
                return datos;

            var q = t.ToUpperInvariant();
            return datos.Where(x =>
                (x.Cod_Smtp ?? "").ToUpperInvariant().Contains(q) ||
                (x.EstadoDesc ?? "").ToUpperInvariant().Contains(q) ||
                (x.Mes ?? "").ToUpperInvariant().Contains(q) ||
                (x.Anio?.ToString() ?? "").Contains(q) ||
                (x.MesId?.ToString() ?? "").Contains(q) ||
                (x.Correos.ToString()).Contains(q)
            ).ToList();
        }

        private static List<SysCorreosBandejaData> ApplyOrdenDetalle(List<SysCorreosBandejaData> all, FiltrosLazyLoadData filtros)
        {
            string campo = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
            int orden = filtros?.sortOrder ?? 1; // 1=ASC, 0=DESC

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

            return (orden == 0) ? all.OrderByDescending(key).ToList()
                               : all.OrderBy(key).ToList();
        }

        private static List<SysCorreosBandejaResumenData> ApplyOrdenResumen(List<SysCorreosBandejaResumenData> all, FiltrosLazyLoadData filtros)
        {
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

            return (orden == 0) ? all.OrderByDescending(key).ToList()
                               : all.OrderBy(key).ToList();
        }

        private static (int total, List<T> lista) ApplyPaginacion<T>(List<T> all, FiltrosLazyLoadData filtros, int defaultTake = 30)
        {
            int offset = Math.Max(0, filtros?.pagina ?? 0);
            int take = Math.Max(1, filtros?.paginacion ?? defaultTake);
            return (all.Count, all.Skip(offset).Take(take).ToList());
        }

        private static ErrorDto<SysCorreosBandejaLista> FailDetalle(ErrorDto<SysCorreosBandejaLista> dto, string msg)
        {
            dto.Code = -1;
            dto.Description = msg;
            if (dto.Result != null)
            {
                dto.Result.total = 0;
                dto.Result.lista = null;
            }
            return dto;
        }

        private static ErrorDto<SysCorreosBandejaResumenLista> FailResumen(ErrorDto<SysCorreosBandejaResumenLista> dto, string msg)
        {
            dto.Code = -1;
            dto.Description = msg;
            if (dto.Result != null)
            {
                dto.Result.total = 0;
                dto.Result.lista = new List<SysCorreosBandejaResumenData>();
            }
            return dto;
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
            var dto = new ErrorDto<SysCorreosBandejaLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysCorreosBandejaLista { total = 0, lista = new() }
            };

            var db = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                var args = BuildArgs(para_Buscar, asunto_Buscar, fecha_Inicio, fecha_Fin, "D");

                var all = QueryDetalle(connection, args);
                all = ApplyFiltroDetalle(all, filtros?.filtro);
                all = ApplyOrdenDetalle(all, filtros ?? new FiltrosLazyLoadData());

                var safeFiltros = filtros ?? new FiltrosLazyLoadData();
                var page = ApplyPaginacion(all, safeFiltros);
                return page;
            });

            if (db.Code != 0)
                return FailDetalle(dto, db.Description ?? "Error desconocido");

            dto.Result.total = db.Result.total;
            dto.Result.lista = db.Result.lista;
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
            var dto = new ErrorDto<List<SysCorreosBandejaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new()
            };

            var db = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                var args = BuildArgs(para_Buscar, asunto_Buscar, fecha_Inicio, fecha_Fin, "D");

                var datos = QueryDetalle(connection, args);
                datos = ApplyFiltroDetalle(datos, filtro_Global);
                return datos;
            });

            if (db.Code != 0)
            {
                dto.Code = -1;
                dto.Description = db.Description;
                dto.Result = null;
                return dto;
            }

            dto.Result = db.Result ?? new();
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
            var dto = new ErrorDto<SysCorreosBandejaResumenLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysCorreosBandejaResumenLista { total = 0, lista = new() }
            };

            var db = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                var args = BuildArgs(para_Buscar, asunto_Buscar, fecha_Inicio, fecha_Fin, "R");

                var all = QueryResumen(connection, args);
                all = ApplyFiltroResumen(all, filtros?.filtro);
                all = ApplyOrdenResumen(all, filtros ?? new FiltrosLazyLoadData());

                var safeFiltros = filtros ?? new FiltrosLazyLoadData();
                var page = ApplyPaginacion(all, safeFiltros);
                return page;
            });

            if (db.Code != 0)
                return FailResumen(dto, db.Description ?? "Error desconocido");

            dto.Result.total = db.Result.total;
            dto.Result.lista = db.Result.lista;
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
            var dto = new ErrorDto<List<SysCorreosBandejaResumenData>> { Code = 0, Description = "Ok", Result = new() };

            var db = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                var args = BuildArgs(para_Buscar, asunto_Buscar, fecha_Inicio, fecha_Fin, "R");

                var datos = QueryResumen(connection, args);
                datos = ApplyFiltroResumen(datos, filtro_Global);
                return datos;
            });

            if (db.Code != 0)
            {
                dto.Code = -1;
                dto.Description = db.Description;
                dto.Result = null;
                return dto;
            }

            dto.Result = db.Result ?? new();
            return dto;
        }
    }
}