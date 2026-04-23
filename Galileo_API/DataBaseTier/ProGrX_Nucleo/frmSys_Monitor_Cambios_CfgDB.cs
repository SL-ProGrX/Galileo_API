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
            try
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
            catch (Exception ex)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = $"Error al obtener los módulos: {ex.Message}",
                    Result = null
                };
            }

        }


        public ErrorDto<List<DropDownListaGenericaModel>> Sys_MonitorCambiosCfg_Tablas_Obtener(int CodEmpresa)
        {
            try
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
            catch (Exception ex)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = $"Error al obtener las tablas: {ex.Message}",
                    Result = null
                };
            }
           
        }

        // <summary>
        /// Obtiene la bitácora de cambios de configuración según los filtros enviados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Filtros aplicados a la consulta de bitácora.</param>
        /// <returns>Listado de movimientos encontrados.</returns>
        public ErrorDto<List<MovimientoLogDto>> Sys_MonitorCambiosCfg_Bitacora_Obtener(int CodEmpresa, MonitorCambiosCfgFiltros filtros)
        {
            var response = new ErrorDto<List<MovimientoLogDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<MovimientoLogDto>()
            };

            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                const string procedure = "[spSEG_Bitacora_Consulta]";

                DateTime inicio;
                DateTime corte;

                if (!filtros.chkFechas)
                {
                    if (!filtros.chkHoras)
                    {
                        inicio = filtros.dtpInicio.Date.Add(filtros.dtpInicio.TimeOfDay);
                        corte = filtros.dtpCorte.Date.Add(filtros.dtpCorte.TimeOfDay);
                    }
                    else
                    {
                        inicio = filtros.dtpInicio.Date;
                        corte = filtros.dtpCorte.Date.AddDays(1).AddTicks(-1);
                    }
                }
                else
                {
                    inicio = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
                    corte = new DateTime(2100, 12, 30, 23, 59, 59, DateTimeKind.Unspecified);
                }

                var values = new
                {
                    Cliente = CodEmpresa,
                    FechaInicio = inicio,
                    FechaCorte = corte,
                    Usuario = string.IsNullOrWhiteSpace(filtros.usuario) ? null : filtros.usuario.Trim(),
                    Modulo = string.IsNullOrWhiteSpace(filtros.modulo) || filtros.modulo == "T" ? null : filtros.modulo.Trim(),
                    Movimiento = string.IsNullOrWhiteSpace(filtros.fuente) || filtros.fuente == "T" ? null : filtros.fuente.Trim(),
                    Detalle = string.IsNullOrWhiteSpace(filtros.detalle) ? null : filtros.detalle.Trim(),
                    AppName = string.IsNullOrWhiteSpace(filtros.appNombre) ? null : filtros.appNombre.Trim(),
                    AppVersion = string.IsNullOrWhiteSpace(filtros.appVersion) ? null : filtros.appVersion.Trim(),
                    LogEquipo = string.IsNullOrWhiteSpace(filtros.logEquipo) ? null : filtros.logEquipo.Trim(),
                    LogIP = string.IsNullOrWhiteSpace(filtros.logIP) ? null : filtros.logIP.Trim(),
                    EquipoMAC = string.IsNullOrWhiteSpace(filtros.mac) ? null : filtros.mac.Trim()
                };

                response.Result = connection
                    .Query<MovimientoLogDto>(procedure, values, commandType: System.Data.CommandType.StoredProcedure)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener la bitácora de movimientos: {ex.Message}";
                response.Result = null;
            }

            return response;
        }

    }
}
