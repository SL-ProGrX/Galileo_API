using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Data;
using Galileo.Models;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysEducacionBitacoraDB
    {
        private readonly IConfiguration _config;

        public FrmSysEducacionBitacoraDB(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Obtiene una lista de datos de educaci�n
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="valor"></param>
        /// <returns></returns>
        public ErrorDto<List<SysEducacionListData>> SYS_Educacion_Combo_Obtener(int CodEmpresa, string tipo, string valor)
        {
            // Inicializa la cadena de conexi�n y el resultado.
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysEducacionListData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysEducacionListData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var procedure = "spSys_Educacion_List";
                var parameters = new
                {
                    Tipo = tipo,
                    Codigo = string.IsNullOrWhiteSpace(valor) ? null : valor
                };
                // Ejecuta el procedimiento almacenado y mapea el resultado al modelo.
                result.Result = connection.Query<SysEducacionListData>(
                    procedure,
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                // En caso de error, retorna el mensaje y c�digo correspondiente.
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene una lista paginada del padr�n nacional, aplicando filtros de b�squeda y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltro"></param>
        /// <returns></returns>
        public ErrorDto<SysPadronLista> SYS_Padron_Obtener(int CodEmpresa, string jfiltro)
        {
            // Deserializa los filtros recibidos en formato JSON.
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ?? new FiltrosLazyLoadData();

            var response = new ErrorDto<SysPadronLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysPadronLista()
            };
            response.Result.total = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                    // Obtiene el total de registros para paginaci�n.
                    var queryTotal = "SELECT count(*) FROM vSys_Padron_Nacional";
                    response.Result.total = connection.Query<int>(queryTotal).FirstOrDefault();

                    // Construye el filtro de b�squeda si se proporciona.
                    string where = "";
                    if (!string.IsNullOrEmpty(filtro.filtro))
                    {
                        where = $@"WHERE (Identificacion LIKE '%{filtro.filtro}%' OR Nombre LIKE '%{filtro.filtro}%')";
                    }

                    // Define el campo de ordenamiento por defecto si no se especifica.
                    if (string.IsNullOrEmpty(filtro.sortField))
                        filtro.sortField = "Identificacion";

                    // Aplica paginaci�n si corresponde.
                    string paginacion = "";
                    if (filtro.pagina > 0)
                    {
                        paginacion = $" OFFSET {filtro.pagina} ROWS FETCH NEXT {filtro.paginacion} ROWS ONLY ";
                    }

                    // Ejecuta la consulta con filtros, orden y paginaci�n.
                    var query = $@"SELECT Identificacion, Nombre FROM vSys_Padron_Nacional
                                   {where}
                                   ORDER BY {filtro.sortField} {(filtro.sortOrder == 0 ? "DESC" : "ASC")}
                                   {paginacion}";

                    response.Result.lista = connection.Query<SysPadronData>(query).ToList();
            }
            catch (Exception ex)
            {
                // En caso de error, retorna el mensaje y c�digo correspondiente.
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtiene registros de educaci�n con lazy loading, paginaci�n, ordenamiento y filtros avanzados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysEducacionLogData>> SYS_Educacion_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysEducacionLogData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysEducacionLogData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parameters = new DynamicParameters();
                string where = BuildEducacionWhereClause(filtros, parameters);

                string sortDirection = filtros.sortOrder == 0 ? "DESC" : "ASC";
                string orderBy = !string.IsNullOrEmpty(filtros.sortField)
                    ? $" ORDER BY {filtros.sortField} {sortDirection} "
                    : " ORDER BY Registro_Fecha DESC ";

                string paginacion = filtros.paginacion > 0
                    ? $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY "
                    : "";

                string query = $@"
            SELECT Cedula, Nombre, Registro_Fecha, Registro_Usuario,
                   Universidad, Nivel, Carrera, Especialidad,
                   Ciclo, Ciclo_Anio, Beneficiario_Id, Beneficiario, Parentesco
            FROM vSys_Educacion_Log
            WHERE 1=1 {where}
            {orderBy}
            {paginacion}";

                result.Result = connection.Query<SysEducacionLogData>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        private static string BuildEducacionWhereClause(FiltrosLazyLoadData filtros, DynamicParameters parameters)
        {
            var where = "";
            dynamic? filtrosAvanzados = null;
            if (filtros.parametros != null)
            {
                var parametrosStr = filtros.parametros?.ToString();
                if (!string.IsNullOrWhiteSpace(parametrosStr))
                {
                    filtrosAvanzados = JsonConvert.DeserializeObject<dynamic>(parametrosStr) ?? new System.Dynamic.ExpandoObject();
                }
                else
                {
                    filtrosAvanzados = null;
                }
            }

            if (filtrosAvanzados != null)
            {
                where += BuildCicloAnioWhere(filtrosAvanzados, parameters);
                where += BuildRegistroFechaWhere(filtrosAvanzados, parameters);
                where += BuildAdvancedLikeWhere(filtrosAvanzados, parameters);
            }

            if (!string.IsNullOrWhiteSpace(filtros.filtro))
            {
                where += @" AND (
                Cedula LIKE @filtro OR
                Nombre LIKE @filtro OR
                Registro_Usuario LIKE @filtro OR
                Universidad LIKE @filtro OR
                Nivel LIKE @filtro OR
                Carrera LIKE @filtro OR
                Especialidad LIKE @filtro OR
                Ciclo LIKE @filtro OR
                Ciclo_Anio LIKE @filtro OR
                Beneficiario_Id LIKE @filtro OR
                Beneficiario LIKE @filtro OR
                Parentesco LIKE @filtro
            )";
                parameters.Add("@filtro", $"%{filtros.filtro}%");
            }

            return where;
        }

        private static string BuildCicloAnioWhere(dynamic filtrosAvanzados, DynamicParameters parameters)
        {
            if (!string.IsNullOrEmpty((string?)filtrosAvanzados.Ciclo_Anio_Inicio) && !string.IsNullOrEmpty((string?)filtrosAvanzados.Ciclo_Anio_Corte))
            {
                parameters.Add("@Ciclo_Anio_Inicio", (string)filtrosAvanzados.Ciclo_Anio_Inicio);
                parameters.Add("@Ciclo_Anio_Corte", (string)filtrosAvanzados.Ciclo_Anio_Corte);
                return " AND CICLO_ANIO BETWEEN @Ciclo_Anio_Inicio AND @Ciclo_Anio_Corte";
            }
            return "";
        }

        private static string BuildRegistroFechaWhere(dynamic filtrosAvanzados, DynamicParameters parameters)
        {
            if (filtrosAvanzados.Registro_Fecha_Inicio != null && filtrosAvanzados.Registro_Fecha_Corte != null)
            {
                DateTime? fechaInicio = filtrosAvanzados.Registro_Fecha_Inicio;
                DateTime? fechaCorte = filtrosAvanzados.Registro_Fecha_Corte;
                if (fechaInicio.HasValue && fechaCorte.HasValue)
                {
                    parameters.Add("@Registro_Fecha_Inicio", fechaInicio.Value.Date);
                    parameters.Add("@Registro_Fecha_Corte", fechaCorte.Value.Date.AddDays(1).AddSeconds(-1));
                    return " AND REGISTRO_FECHA BETWEEN @Registro_Fecha_Inicio AND @Registro_Fecha_Corte";
                }
            }
            return "";
        }

        private static string BuildAdvancedLikeWhere(dynamic filtrosAvanzados, DynamicParameters parameters)
        {
            var where = "";
            var likeFields = new (string Field, string Param, string Condition)[]
            {
                ("Ciclo", "@Ciclo", " AND REPLACE(CICLO, ' ', '') = @Ciclo"),
                ("Registro_Usuario", "@Registro_Usuario", " AND REGISTRO_USUARIO LIKE @Registro_Usuario"),
                ("Cedula", "@Cedula", " AND CEDULA LIKE @Cedula"),
                ("Nombre", "@Nombre", " AND NOMBRE LIKE @Nombre"),
                ("Beneficiario_Id", "@Beneficiario_Id", " AND BENEFICIARIO_ID LIKE @Beneficiario_Id"),
                ("Beneficiario", "@Beneficiario", " AND BENEFICIARIO LIKE @Beneficiario"),
                ("Universidad", "@Cod_Universidad", " AND COD_UNIVERSIDAD = @Cod_Universidad"),
                ("Nivel", "@Cod_Nivel", " AND COD_NIVEL = @Cod_Nivel"),
                ("Carrera", "@Cod_Carrera", " AND COD_CARRERA = @Cod_Carrera"),
                ("Especialidad", "@Cod_Especialidad", " AND COD_ESPECIALIDAD = @Cod_Especialidad")
            };

            foreach (var (field, param, condition) in likeFields)
            {
                var value = filtrosAvanzados[field];
                string? valueStr = value as string;
                if (!string.IsNullOrEmpty(valueStr))
                {
                    if (condition.Contains("LIKE"))
                    {
                        parameters.Add(param, $"%{valueStr}%");
                    }
                    else
                    {
                        parameters.Add(param, valueStr);
                    }
                    where += condition;
                }
            }
            return where;
        }
    }

}