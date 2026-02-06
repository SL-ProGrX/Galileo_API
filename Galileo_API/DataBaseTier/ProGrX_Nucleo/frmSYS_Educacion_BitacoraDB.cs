using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Data;
using Galileo.Models;
using System.Diagnostics.CodeAnalysis;

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

                // Total (mantiene comportamiento anterior: total sin filtro)
                const string queryTotal = @"SELECT COUNT(*) FROM vSys_Padron_Nacional";
                response.Result.total = connection.Query<int>(queryTotal).FirstOrDefault();

                var search = filtro.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var sortField = (filtro.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtro.sortOrder; // 0=DESC, 1=ASC

                var offset = filtro.pagina;
                var fetch = filtro.paginacion;
                if (fetch <= 0)
                {
                    fetch = int.MaxValue;
                }

                const string query = @"SELECT Identificacion, Nombre
                                       FROM vSys_Padron_Nacional
                                       WHERE (@search IS NULL
                                              OR Identificacion LIKE @search
                                              OR Nombre LIKE @search)
                                       ORDER BY
                                            -- ASC
                                            CASE WHEN @sortOrder = 1 AND @sortField = 'identificacion' THEN Identificacion END ASC,
                                            CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN Nombre END ASC,

                                            -- DESC
                                            CASE WHEN @sortOrder = 0 AND @sortField = 'identificacion' THEN Identificacion END DESC,
                                            CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN Nombre END DESC,

                                            -- Fallback
                                            Identificacion ASC
                                       OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                response.Result.lista = connection.Query<SysPadronData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
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
        // --------------------------- Helpers & DTO for SYS_Educacion_Obtener ---------------------------
        [SuppressMessage("Major Code Smell", "S3459", Justification = "Properties are populated via Json.NET deserialization.")]
        [SuppressMessage("Major Code Smell", "S1144", Justification = "Setters are used by Json.NET during deserialization.")]
        private sealed class EducacionAvanzadosDto
        {
            public string? Ciclo_Anio_Inicio { get; set; }
            public string? Ciclo_Anio_Corte { get; set; }
            public DateTime? Registro_Fecha_Inicio { get; set; }
            public DateTime? Registro_Fecha_Corte { get; set; }

            public string? Ciclo { get; set; }
            public string? Registro_Usuario { get; set; }
            public string? Cedula { get; set; }
            public string? Nombre { get; set; }
            public string? Beneficiario_Id { get; set; }
            public string? Beneficiario { get; set; }

            public string? Universidad { get; set; }
            public string? Nivel { get; set; }
            public string? Carrera { get; set; }
            public string? Especialidad { get; set; }
        }

        private static EducacionAvanzadosDto? ParseEducacionAvanzados(object? parametros)
        {
            if (parametros == null) return null;

            var parametrosStr = parametros.ToString();
            if (string.IsNullOrWhiteSpace(parametrosStr)) return null;

            try
            {
                return JsonConvert.DeserializeObject<EducacionAvanzadosDto>(parametrosStr);
            }
            catch
            {
                // Si el JSON viene mal, ignoramos y seguimos sin filtros avanzados.
                return null;
            }
        }

        private static string? LikeOrNull(string? value)
        {
            var v = value?.Trim();
            return string.IsNullOrWhiteSpace(v) ? null : $"%{v}%";
        }

        private static string? TrimOrNull(string? value)
        {
            var v = value?.Trim();
            return string.IsNullOrWhiteSpace(v) ? null : v;
        }

        private static DateTime? StartOfDay(DateTime? dt) => dt?.Date;
        private static DateTime? EndOfDay(DateTime? dt) => dt?.Date.AddDays(1).AddSeconds(-1);


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

                var adv = ParseEducacionAvanzados(filtros?.parametros);

                var cicloAnioInicio = TrimOrNull(adv?.Ciclo_Anio_Inicio);
                var cicloAnioCorte = TrimOrNull(adv?.Ciclo_Anio_Corte);

                // Normaliza fechas para rango inclusivo
                DateTime? fIni = StartOfDay(adv?.Registro_Fecha_Inicio);
                DateTime? fFin = EndOfDay(adv?.Registro_Fecha_Corte);

                // Campos avanzados
                var ciclo = TrimOrNull(adv?.Ciclo)?.Replace(" ", string.Empty);
                var registroUsuarioLike = LikeOrNull(adv?.Registro_Usuario);
                var cedulaLike = LikeOrNull(adv?.Cedula);
                var nombreLike = LikeOrNull(adv?.Nombre);
                var beneficiarioIdLike = LikeOrNull(adv?.Beneficiario_Id);
                var beneficiarioLike = LikeOrNull(adv?.Beneficiario);

                var codUniversidad = TrimOrNull(adv?.Universidad);
                var codNivel = TrimOrNull(adv?.Nivel);
                var codCarrera = TrimOrNull(adv?.Carrera);
                var codEspecialidad = TrimOrNull(adv?.Especialidad);

                // Filtro general
                var searchLike = LikeOrNull(filtros?.filtro);

                // Orden/paginación
                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    fetch = int.MaxValue;
                }

                const string query = @"
                    SELECT Cedula, Nombre, Registro_Fecha, Registro_Usuario,
                           Universidad, Nivel, Carrera, Especialidad,
                           Ciclo, Ciclo_Anio, Beneficiario_Id, Beneficiario, Parentesco
                    FROM vSys_Educacion_Log
                    WHERE 1=1
                      AND (@Ciclo_Anio_Inicio IS NULL OR @Ciclo_Anio_Corte IS NULL OR Ciclo_Anio BETWEEN @Ciclo_Anio_Inicio AND @Ciclo_Anio_Corte)
                      AND (@Registro_Fecha_Inicio IS NULL OR @Registro_Fecha_Corte IS NULL OR Registro_Fecha BETWEEN @Registro_Fecha_Inicio AND @Registro_Fecha_Corte)

                      AND (@Ciclo IS NULL OR REPLACE(Ciclo, ' ', '') = @Ciclo)
                      AND (@Registro_Usuario IS NULL OR Registro_Usuario LIKE @Registro_Usuario)
                      AND (@Cedula IS NULL OR Cedula LIKE @Cedula)
                      AND (@Nombre IS NULL OR Nombre LIKE @Nombre)
                      AND (@Beneficiario_Id IS NULL OR Beneficiario_Id LIKE @Beneficiario_Id)
                      AND (@Beneficiario IS NULL OR Beneficiario LIKE @Beneficiario)

                      AND (@Cod_Universidad IS NULL OR Cod_Universidad = @Cod_Universidad)
                      AND (@Cod_Nivel IS NULL OR Cod_Nivel = @Cod_Nivel)
                      AND (@Cod_Carrera IS NULL OR Cod_Carrera = @Cod_Carrera)
                      AND (@Cod_Especialidad IS NULL OR Cod_Especialidad = @Cod_Especialidad)

                      AND (
                            @search IS NULL
                            OR Cedula LIKE @search
                            OR Nombre LIKE @search
                            OR Registro_Usuario LIKE @search
                            OR Universidad LIKE @search
                            OR Nivel LIKE @search
                            OR Carrera LIKE @search
                            OR Especialidad LIKE @search
                            OR Ciclo LIKE @search
                            OR Ciclo_Anio LIKE @search
                            OR Beneficiario_Id LIKE @search
                            OR Beneficiario LIKE @search
                            OR Parentesco LIKE @search
                          )
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cedula' THEN Cedula END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'nombre' THEN Nombre END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha' THEN Registro_Fecha END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'registro_usuario' THEN Registro_Usuario END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'universidad' THEN Universidad END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'nivel' THEN Nivel END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'carrera' THEN Carrera END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'especialidad' THEN Especialidad END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'ciclo' THEN Ciclo END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'ciclo_anio' THEN Ciclo_Anio END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'beneficiario_id' THEN Beneficiario_Id END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'beneficiario' THEN Beneficiario END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'parentesco' THEN Parentesco END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cedula' THEN Cedula END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'nombre' THEN Nombre END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha' THEN Registro_Fecha END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'registro_usuario' THEN Registro_Usuario END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'universidad' THEN Universidad END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'nivel' THEN Nivel END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'carrera' THEN Carrera END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'especialidad' THEN Especialidad END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'ciclo' THEN Ciclo END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'ciclo_anio' THEN Ciclo_Anio END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'beneficiario_id' THEN Beneficiario_Id END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'beneficiario' THEN Beneficiario END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'parentesco' THEN Parentesco END DESC,

                        -- Fallback (equivalente al orden default anterior)
                        Registro_Fecha DESC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                result.Result = connection.Query<SysEducacionLogData>(query, new
                {
                    Ciclo_Anio_Inicio = cicloAnioInicio,
                    Ciclo_Anio_Corte = cicloAnioCorte,
                    Registro_Fecha_Inicio = fIni,
                    Registro_Fecha_Corte = fFin,

                    Ciclo = ciclo,
                    Registro_Usuario = registroUsuarioLike,
                    Cedula = cedulaLike,
                    Nombre = nombreLike,
                    Beneficiario_Id = beneficiarioIdLike,
                    Beneficiario = beneficiarioLike,

                    Cod_Universidad = codUniversidad,
                    Cod_Nivel = codNivel,
                    Cod_Carrera = codCarrera,
                    Cod_Especialidad = codEspecialidad,

                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
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