using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvBodegasDB
    {
        private readonly IConfiguration _config;

        private const string MensajeOk = "Ok";
        private const string ErrorDesplazamiento = "Error al obtener el desplazamiento de bodega.";
        private const string SinDesplazamiento = "No se encontró otra bodega para el desplazamiento solicitado.";
        private const string ErrorBodegaConsecutivo = "Error al obtener la bodega por consecutivo.";
        private const string SinBodega = "No se encontró la bodega indicada.";
        private const string ErrorInsertarBodega = "Error al insertar la bodega.";
        private const string ErrorActualizarBodega = "Error al actualizar la bodega.";
        private const string ErrorEliminarBodega = "Error al eliminar la bodega.";
        private const string ErrorActualizarPermisos = "Error al actualizar los permisos de la bodega.";
        private const string ErrorBodegaExistente = "Ya existe el numero de Bodega";
        private const string QueryObtenerBodegas = "select * from PV_BODEGAS";
        private const string QueryExisteBodega = "SELECT COUNT(*) FROM PV_BODEGAS WHERE cod_bodega = @CodBodega";
        private const string QueryBodegaPorCodigo = @"SELECT *
                  FROM PV_BODEGAS
                  WHERE COD_BODEGA = @CodBodega";
        private const string QueryEliminarPermisosBodega = "DELETE FROM PV_BODEGAS_PERMISOS WHERE COD_BODEGA = @CodBodega";

        private const string QueryEliminarBodega = "DELETE FROM PV_BODEGAS WHERE COD_BODEGA = @CodBodega";

        private static readonly string[] CamposInsertarBodega =
        {
            nameof(BodegasDto.Cod_Bodega),
            nameof(BodegasDto.Descripcion),
            nameof(BodegasDto.Observacion),
            nameof(BodegasDto.Estado),
            "Fecha_Inclusion",
            nameof(BodegasDto.Permite_Entradas),
            nameof(BodegasDto.Permite_Salidas),
            nameof(BodegasDto.Cod_Cuenta),
            nameof(BodegasDto.Cod_Cta_Ingresostf),
            nameof(BodegasDto.Cod_Cta_Gastostf),
            nameof(BodegasDto.Utiliza_Permisos)
        };

        private static readonly string[] ColumnasInsertarBodega =
        {
            "cod_bodega",
            "descripcion",
            "observacion",
            "estado",
            "fecha_inclusion",
            "permite_entradas",
            "permite_salidas",
            "cod_cuenta",
            "cod_cta_ingresosTF",
            "cod_cta_gastosTF",
            "UTILIZA_PERMISOS"
        };

        private static readonly string[] CamposActualizarBodega =
        {
            nameof(BodegasDto.Observacion),
            nameof(BodegasDto.Cod_Cuenta),
            nameof(BodegasDto.Cod_Cta_Gastostf),
            nameof(BodegasDto.Cod_Cta_Ingresostf),
            nameof(BodegasDto.Permite_Entradas),
            nameof(BodegasDto.Permite_Salidas),
            nameof(BodegasDto.Utiliza_Permisos),
            nameof(BodegasDto.Estado),
            nameof(BodegasDto.Descripcion)
        };

        private static readonly string[] ColumnasActualizarBodega =
        {
            "observacion",
            "cod_cuenta",
            "cod_cta_gastoSTF",
            "cod_cta_ingresostf",
            "permite_entradas",
            "permite_salidas",
            "utiliza_permisos",
            "estado",
            "descripcion"
        };

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvBodegasDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvBodegasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta estándar para operaciones de consulta única.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="errorMessage">Mensaje cuando ocurre un error.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar para una sola entidad.</returns>
        private static ErrorDto<T> CrearRespuestaSingle<T>(ErrorDto<T?> result, string errorMessage, string notFoundMessage)
            where T : class
        {
            if (result.Code != 0)
            {
                return new ErrorDto<T>
                {
                    Code = result.Code,
                    Description = result.Description ?? errorMessage,
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<T>
                {
                    Code = -2,
                    Description = notFoundMessage,
                    Result = null
                };
        }

        /// <summary>
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una respuesta estándar para resultados generados dentro de WithConn.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="errorMessage">Mensaje de error estándar.</param>
        /// <returns>Respuesta estándar.</returns>
        private static ErrorDto CrearRespuestaWithConn(ErrorDto<ErrorDto> result, string errorMessage)
        {
            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea los parámetros comunes para una bodega.
        /// </summary>
        /// <param name="CodBodega">Código de bodega.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosBodega(string CodBodega) => new
        {
            CodBodega
        };

        /// <summary>
        /// Crea los parámetros para insertar una bodega.
        /// </summary>
        /// <param name="data">Datos de la bodega.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static DynamicParameters CrearParametrosInsertarBodega(BodegasDto data)
        {
            var parametros = new DynamicParameters();
            var tipo = typeof(BodegasDto);

            foreach (var campo in CamposInsertarBodega)
            {
                parametros.Add(campo, campo == "Fecha_Inclusion" ? DateTime.Now : tipo.GetProperty(campo)?.GetValue(data));
            }

            return parametros;
        }

        /// <summary>
        /// Crea los parámetros para actualizar una bodega.
        /// </summary>
        /// <param name="data">Datos de la bodega.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static DynamicParameters CrearParametrosActualizarBodega(BodegasDto data)
        {
            var parametros = new DynamicParameters();
            var tipo = typeof(BodegasDto);

            foreach (var campo in CamposActualizarBodega)
            {
                parametros.Add(campo, tipo.GetProperty(campo)?.GetValue(data));
            }

            parametros.Add(nameof(BodegasDto.Cod_Bodega), data.Cod_Bodega);
            return parametros;
        }

        /// <summary>
        /// Crea la consulta para insertar una bodega.
        /// </summary>
        /// <returns>Consulta SQL de inserción.</returns>
        private static string CrearConsultaInsertarBodega()
        {
            var columnas = string.Join(", ", ColumnasInsertarBodega);
            var valores = string.Join(", ", CamposInsertarBodega.Select(campo => $"@{campo}"));
            return $"INSERT INTO pv_bodegas ({columnas}) VALUES ({valores})";
        }

        /// <summary>
        /// Crea la consulta para actualizar una bodega.
        /// </summary>
        /// <returns>Consulta SQL de actualización.</returns>
        private static string CrearConsultaActualizarBodega()
        {
            var asignaciones = ColumnasActualizarBodega
                .Zip(CamposActualizarBodega, (columna, campo) => $"{columna} = @{campo}");

            return $"UPDATE pv_bodegas SET {string.Join(", ", asignaciones)} WHERE cod_bodega = @Cod_Bodega";
        }

        /// <summary>
        /// Crea los parámetros para actualizar permisos de bodega.
        /// </summary>
        /// <param name="request">Datos de permisos.</param>
        /// <param name="codBodega">Código de bodega.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosPermisos(PermisosBodegasDto request, string codBodega) => new
        {
            Modifica = request.E_Modifica ? 1 : 0,
            Autoriza = request.E_Autoriza ? 1 : 0,
            Procesa = request.E_Procesa ? 1 : 0,
            Autorizador = request.Nombre,
            cod_bodega = codBodega
        };

        /// <summary>
        /// Crea una respuesta de operación correcta.
        /// </summary>
        /// <returns>Respuesta Ok.</returns>
        private static ErrorDto CrearOk() => new()
        {
            Code = 0,
            Description = MensajeOk
        };

        /// <summary>
        /// Ejecuta un procedimiento almacenado que devuelve un código entero y lo transforma en <see cref="ErrorDto"/>.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="procedure">Nombre del procedimiento almacenado.</param>
        /// <param name="values">Parámetros del procedimiento.</param>
        /// <param name="errorMessage">Mensaje de error estándar.</param>
        /// <returns>Respuesta con el código devuelto por el procedimiento.</returns>
        private ErrorDto EjecutarProcedimientoConCodigo(int CodEmpresa, string procedure, object values, string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var code = connection.QueryFirstOrDefault<int>(procedure, values, commandType: CommandType.StoredProcedure);
                return new ErrorDto
                {
                    Code = code,
                    Description = MensajeOk
                };
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene los permisos de los usuarios para una bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodBodega">Código de la bodega.</param>
        /// <returns>Listado de permisos por usuario.</returns>
        public ErrorDto<List<PermisosBodegasDto>> Autorizador_ObtenerTodos(int CodEmpresa, string CodBodega)
        {
            return DbHelper.ExecuteListQuery<PermisosBodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT 
                        U.Nombre,
                        U.DESCRIPCION,
                        C.COD_BODEGA,
                        ISNULL(C.E_PROCESA, 0) AS E_Procesa,
                        ISNULL(C.E_MODIFICA, 0) AS E_Modifica,
                        ISNULL(C.E_AUTORIZA, 0) AS E_Autoriza
                  FROM usuarios U
                  LEFT JOIN PV_BODEGAS_PERMISOS C
                    ON U.nombre = C.usuario
                   AND C.cod_bodega = @CodBodega
                  WHERE U.estado = 'A'
                  ORDER BY U.nombre ASC;",
                CrearParametrosBodega(CodBodega));
        }

        /// <summary>
        /// Obtiene el listado de bodegas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<BodegasDto>> Bodegas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<BodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryObtenerBodegas);
        }

        /// <summary>
        /// Obtiene la bodega anterior o siguiente según el desplazamiento indicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Código actual de bodega.</param>
        /// <param name="tipo">Dirección del desplazamiento: asc o desc.</param>
        /// <returns>Bodega encontrada para el desplazamiento.</returns>
        public ErrorDto<BodegasDto> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            string query;
            object parametros;

            if (tipo == "desc")
            {
                if (consecutivo == 0)
                {
                    query = @"select Top 1 *
                              from PV_BODEGAS
                              order by COD_BODEGA desc";
                    parametros = new { };
                }
                else
                {
                    query = @"select Top 1 *
                              from PV_BODEGAS
                              where COD_BODEGA < @Consecutivo
                              order by COD_BODEGA desc";
                    parametros = new { Consecutivo = consecutivo };
                }
            }
            else
            {
                query = @"select Top 1 *
                          from PV_BODEGAS
                          where COD_BODEGA > @Consecutivo
                          order by COD_BODEGA asc";
                parametros = new { Consecutivo = consecutivo };
            }

            var result = DbHelper.ExecuteSingleQuery<BodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                parametros);

            return CrearRespuestaSingle(result, ErrorDesplazamiento, SinDesplazamiento);
        }

        /// <summary>
        /// Obtiene una bodega por su consecutivo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Código de la bodega.</param>
        /// <returns>Bodega encontrada.</returns>
        public ErrorDto<BodegasDto> bodegaConsecutivo_Obtener(int CodEmpresa, string consecutivo)
        {
            var result = DbHelper.ExecuteSingleQuery<BodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryBodegaPorCodigo,
                null,
                CrearParametrosBodega(consecutivo));

            return CrearRespuestaSingle(result, ErrorBodegaConsecutivo, SinBodega);
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Inserta una nueva bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto bodega_Insertar(int CodEmpresa, BodegasDto data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(
                    QueryExisteBodega,
                    CrearParametrosBodega(data.Cod_Bodega));

                if (existe >= 1)
                {
                    return DbHelper.ErrorResponse(ErrorBodegaExistente, -1);
                }

                connection.Execute(
                    CrearConsultaInsertarBodega(),
                    CrearParametrosInsertarBodega(data));

                return CrearOk();
            });

            return CrearRespuestaWithConn(result, ErrorInsertarBodega);
        }

        /// <summary>
        /// Actualiza una bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto bodega_Actualizar(int CodEmpresa, BodegasDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                CrearConsultaActualizarBodega(),
                CrearParametrosActualizarBodega(data));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorActualizarBodega);
        }

        /// <summary>
        /// Elimina una bodega y sus permisos asociados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_bodega">Código de la bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto bodega_Eliminar(int CodEmpresa, string cod_bodega)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    QueryEliminarPermisosBodega,
                    CrearParametrosBodega(cod_bodega));

                connection.Execute(
                    QueryEliminarBodega,
                    CrearParametrosBodega(cod_bodega));

                return CrearOk();
            });

            return CrearRespuestaWithConn(result, ErrorEliminarBodega);
        }

        /// <summary>
        /// Actualiza los permisos de una bodega para un usuario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de permisos.</param>
        /// <param name="cod_bodega">Código de la bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto permisosBodega_Actualizar(int CodEmpresa, PermisosBodegasDto request, string cod_bodega)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_PermisosBodegas_Actualizar]",
                CrearParametrosPermisos(request, cod_bodega),
                ErrorActualizarPermisos);
        }

        #endregion
    }
}