using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysMonitorCambiosCfgDB
    {
        private readonly PortalDB _portalDB;
        private readonly IConfiguration _config;

        public FrmSysMonitorCambiosCfgDB(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para obtener nombre de la empresa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<string> Sys_GetNomCortoEmpresa_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "select PAG_NOMCORTO from SIF_EMPRESA";

                return conn.Query<string>(query).FirstOrDefault() ?? string.Empty;
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Modulos_Obtener(int CodEmpresa)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

            const string query = "exec spSEG_Modulos_Consulta";

            var modulos = connection.Query<MonitorCambiosCfgModulosDto>(query).ToList();

            var response = modulos.Select(m => new DropDownListaGenericaModel
            {
                item = m.modulo,
                descripcion = m.nombre
                 
            }).ToList();

            response.Add(new DropDownListaGenericaModel
            {
                item = "T",
                descripcion = "[TODOS]"
            });

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = response
            };
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Tablas_Obtener(int CodEmpresa)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

            const string query = "select TableName as 'item', TableDesc as 'descripcion'  From Sys_Conf_Monitor_Tables";

            var response = connection.Query<DropDownListaGenericaModel>(query).ToList();

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = response
            };
        }

        public ErrorDto<List<MovimientoLogDto>> Sys_MonitorCambiosCfg_Bitacora_Obtener(int CodEmpresa, MonitorCambiosCfgFiltros filtros)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));


            const string query = $@"exec spSEG_Bitacora_Consulta 
                                                    @Cliente,
                                                    @FechaInicio,
                                                    @FechaCorte,
                                                    @Usuario,
                                                    @Modulo,
                                                    @Movimiento,
                                                    @Detalle,
                                                    @AppName,
                                                    @AppVersion,
                                                    @LogEquipo,
                                                    @LogIP,
                                                    @EquipoMAC";

            DateTime inicio;
            DateTime corte;

            if (!filtros.chkFechas) // vbUnchecked
            {
                if (!filtros.chkHoras) // vbUnchecked
                {
                    inicio = filtros.dtpInicio.Date.Add(filtros.dtpInicio.TimeOfDay);
                    corte = filtros.dtpCorte.Date.Add(filtros.dtpCorte.TimeOfDay);
                }
                else
                {
                    inicio = filtros.dtpInicio.Date; // 00:00:00
                    corte = filtros.dtpCorte.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
            }
            else
            {
                inicio = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                corte = new DateTime(2100, 12, 30, 23, 59, 59, DateTimeKind.Unspecified);
            }

            // === 2) Parámetros opcionales (Null si vacío / [TODOS]) ===
            string? usuario = string.IsNullOrWhiteSpace(filtros.usuario) ? null : filtros.usuario.Trim();
            string? moduloId = (filtros.modulo == "T") ? null : filtros.modulo;
            string? fuente = (filtros.fuente == "T") ? null : filtros.fuente!.Trim();
            string? detalle = string.IsNullOrWhiteSpace(filtros.detalle) ? null : filtros.detalle.Trim();
            string? appNombre = string.IsNullOrWhiteSpace(filtros.appNombre) ? null : filtros.appNombre.Trim();
            string? appVersion = string.IsNullOrWhiteSpace(filtros.appVersion) ? null : filtros.appVersion.Trim();
            string? logEquipo = string.IsNullOrWhiteSpace(filtros.logEquipo) ? null : filtros.logEquipo.Trim();
            string? logIP = string.IsNullOrWhiteSpace(filtros.logIP) ? null : filtros.logIP.Trim();
            string? mac = string.IsNullOrWhiteSpace(filtros.mac) ? null : filtros.mac.Trim();

            // === 3) Dapper parameters (evita SQL injection y SonarQube feliz) ===
            var p = new DynamicParameters();
            p.Add("@Cliente", CodEmpresa);
            p.Add("@FechaInicio", inicio);
            p.Add("@FechaCorte", corte);
            p.Add("@Usuario", usuario);
            p.Add("@Modulo", moduloId);
            p.Add("@Movimiento", fuente);
            p.Add("@Detalle", detalle);
            p.Add("@AppName", appNombre);
            p.Add("@AppVersion", appVersion);
            p.Add("@LogEquipo", logEquipo);
            p.Add("@LogIP", logIP);
            p.Add("@EquipoMAC", mac);

            // Si tu SP espera el orden exacto y no nombres (raro, pero pasa),
            // esto igual funciona en SQL Server porque mapea por nombre.

            var data = connection.Query<MovimientoLogDto>(query, p, commandType: System.Data.CommandType.StoredProcedure).ToList();

            return new ErrorDto<List<MovimientoLogDto>>
            {
                Code = 0,
                Description = "OK",
                Result = data
            };

        }

    }
}
