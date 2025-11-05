using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using System.Data;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_Educacion_BitacoraDB
    {
        private readonly IConfiguration? _config;

        public frmSYS_Educacion_BitacoraDB(IConfiguration? config)
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
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro);

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
                {
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
                    if (filtro.pagina != null)
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

                var where = "";
                var parameters = new DynamicParameters();

                // Deserializa los filtros avanzados si existen
                dynamic filtrosAvanzados = filtros.parametros != null
                    ? JsonConvert.DeserializeObject<dynamic>(filtros.parametros.ToString())
                    : null;

                // Filtros por rango de a�o
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Ciclo_Anio_Inicio) && !string.IsNullOrEmpty((string?)filtrosAvanzados.Ciclo_Anio_Corte))
                {
                    where += " AND CICLO_ANIO BETWEEN @Ciclo_Anio_Inicio AND @Ciclo_Anio_Corte";
                    parameters.Add("@Ciclo_Anio_Inicio", (string)filtrosAvanzados.Ciclo_Anio_Inicio);
                    parameters.Add("@Ciclo_Anio_Corte", (string)filtrosAvanzados.Ciclo_Anio_Corte);
                }

                // Filtros por rango de fecha
                if (filtrosAvanzados != null && filtrosAvanzados.Registro_Fecha_Inicio != null && filtrosAvanzados.Registro_Fecha_Corte != null)
                {
                    DateTime? fechaInicio = filtrosAvanzados.Registro_Fecha_Inicio;
                    DateTime? fechaCorte = filtrosAvanzados.Registro_Fecha_Corte;
                    if (fechaInicio.HasValue && fechaCorte.HasValue)
                    {
                        where += " AND REGISTRO_FECHA BETWEEN @Registro_Fecha_Inicio AND @Registro_Fecha_Corte";
                        parameters.Add("@Registro_Fecha_Inicio", fechaInicio.Value.Date);
                        parameters.Add("@Registro_Fecha_Corte", fechaCorte.Value.Date.AddDays(1).AddSeconds(-1));
                    }
                }

                // Filtros exactos y por LIKE
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Ciclo))
                {
                    where += " AND REPLACE(CICLO, ' ', '') = @Ciclo";
                    parameters.Add("@Ciclo", (string)filtrosAvanzados.Ciclo);
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Registro_Usuario))
                {
                    where += " AND REGISTRO_USUARIO LIKE @Registro_Usuario";
                    parameters.Add("@Registro_Usuario", $"%{(string)filtrosAvanzados.Registro_Usuario}%");
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Cedula))
                {
                    where += " AND CEDULA LIKE @Cedula";
                    parameters.Add("@Cedula", $"%{(string)filtrosAvanzados.Cedula}%");
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Nombre))
                {
                    where += " AND NOMBRE LIKE @Nombre";
                    parameters.Add("@Nombre", $"%{(string)filtrosAvanzados.Nombre}%");
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Beneficiario_Id))
                {
                    where += " AND BENEFICIARIO_ID LIKE @Beneficiario_Id";
                    parameters.Add("@Beneficiario_Id", $"%{(string)filtrosAvanzados.Beneficiario_Id}%");
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Beneficiario))
                {
                    where += " AND BENEFICIARIO LIKE @Beneficiario";
                    parameters.Add("@Beneficiario", $"%{(string)filtrosAvanzados.Beneficiario}%");
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Universidad))
                {
                    where += " AND COD_UNIVERSIDAD = @Cod_Universidad";
                    parameters.Add("@Cod_Universidad", (string)filtrosAvanzados.Universidad);
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Nivel))
                {
                    where += " AND COD_NIVEL = @Cod_Nivel";
                    parameters.Add("@Cod_Nivel", (string)filtrosAvanzados.Nivel);
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Carrera))
                {
                    where += " AND COD_CARRERA = @Cod_Carrera";
                    parameters.Add("@Cod_Carrera", (string)filtrosAvanzados.Carrera);
                }
                if (filtrosAvanzados != null && !string.IsNullOrEmpty((string?)filtrosAvanzados.Especialidad))
                {
                    where += " AND COD_ESPECIALIDAD = @Cod_Especialidad";
                    parameters.Add("@Cod_Especialidad", (string)filtrosAvanzados.Especialidad);
                }

                // Filtro general (buscador en todos los campos relevantes)
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

                // Ordenamiento din�mico
                string orderBy = " ORDER BY Registro_Fecha DESC ";
                if (!string.IsNullOrEmpty(filtros.sortField))
                {
                    orderBy = $" ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} ";
                }

                // Paginaci�n
                string paginacion = "";
                if (filtros.paginacion > 0)
                {
                    paginacion = $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }

                // Consulta final
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
    }

}