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

        /// <summary>
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
                var (inicio, corte) = ObtenerRangoFechasBitacora(filtros);
                var values = CrearParametrosBitacora(CodEmpresa, filtros, inicio, corte);

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

        /// <summary>
        /// Calcula el rango de fechas a consultar en bitácora según la configuración de filtros.
        /// </summary>
        /// <param name="filtros">Filtros de consulta de bitácora.</param>
        /// <returns>Tupla con fecha de inicio y fecha de corte.</returns>
        private static (DateTime inicio, DateTime corte) ObtenerRangoFechasBitacora(MonitorCambiosCfgFiltros filtros)
        {
            if (filtros.chkFechas)
            {
                return (
                    new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                    new DateTime(2100, 12, 30, 23, 59, 59, DateTimeKind.Unspecified)
                );
            }

            if (!filtros.chkHoras)
            {
                return (
                    filtros.dtpInicio.Date.Add(filtros.dtpInicio.TimeOfDay),
                    filtros.dtpCorte.Date.Add(filtros.dtpCorte.TimeOfDay)
                );
            }

            return (
                filtros.dtpInicio.Date,
                filtros.dtpCorte.Date.AddDays(1).AddTicks(-1)
            );
        }

        /// <summary>
        /// Crea el objeto de parámetros requerido por el procedimiento de consulta de bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros enviados por la pantalla.</param>
        /// <param name="inicio">Fecha inicial calculada.</param>
        /// <param name="corte">Fecha final calculada.</param>
        /// <returns>Objeto anónimo con los parámetros del procedimiento almacenado.</returns>
        private static object CrearParametrosBitacora(
            int codEmpresa,
            MonitorCambiosCfgFiltros filtros,
            DateTime inicio,
            DateTime corte)
        {
            return new
            {
                Cliente = codEmpresa,
                FechaInicio = inicio,
                FechaCorte = corte,
                Usuario = NormalizarTexto(filtros.usuario),
                Modulo = NormalizarTextoTodosComoNull(filtros.modulo),
                Movimiento = NormalizarTextoTodosComoNull(filtros.fuente),
                Detalle = NormalizarTexto(filtros.detalle),
                AppName = NormalizarTexto(filtros.appNombre),
                AppVersion = NormalizarTexto(filtros.appVersion),
                LogEquipo = NormalizarTexto(filtros.logEquipo),
                LogIP = NormalizarTexto(filtros.logIP),
                EquipoMAC = NormalizarTexto(filtros.mac)
            };
        }

        /// <summary>
        /// Normaliza un texto para parámetros opcionales.
        /// Devuelve null cuando el valor viene vacío.
        /// </summary>
        /// <param name="valor">Texto a normalizar.</param>
        /// <returns>Texto limpio o null.</returns>
        private static string? NormalizarTexto(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        /// <summary>
        /// Normaliza un texto para filtros donde el valor "T" representa todos.
        /// Devuelve null cuando el valor viene vacío o es "T".
        /// </summary>
        /// <param name="valor">Texto a normalizar.</param>
        /// <returns>Texto limpio o null.</returns>
        private static string? NormalizarTextoTodosComoNull(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            var texto = valor.Trim();
            return texto == "T" ? null : texto;
        }
    }
}
