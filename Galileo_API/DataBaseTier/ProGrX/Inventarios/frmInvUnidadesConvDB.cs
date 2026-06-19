using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvUnidadesConvDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvUnidadesConvDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvUnidadesConvDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de equivalencias de unidades.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static UnidadesConvLista CrearListaVacia() => new()
        {
            total = 0,
            lista = new List<UnidadMedicionConvData>()
        };

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
        /// Valida si una equivalencia entre unidades ya existe.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="equivalencia">Datos de la equivalencia.</param>
        /// <returns>Cantidad de registros encontrados.</returns>
        private static int ContarEquivalencia(IDbConnection connection, UnidadMedicionConvData equivalencia)
        {
            return connection.ExecuteScalar<int>(
                @"SELECT COUNT(*)
                  FROM PV_UNIDADES_CONV
                  WHERE COD_UNIDAD = @cod_unidad
                    AND COD_UNIDAD_D = @cod_unidad_d",
                new
                {
                    equivalencia.cod_unidad,
                    equivalencia.cod_unidad_d
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene la lista de unidades activas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Listado de unidades activas.</returns>
        public ErrorDto<List<UnidadMedicionConv>> UnidadMedicion_Obtener(int CodCliente)
        {
            return DbHelper.ExecuteListQuery<UnidadMedicionConv>(
                CreatePortalDb(),
                CodCliente,
                "SELECT COD_UNIDAD AS ITEM, DESCRIPCION FROM PV_UNIDADES WHERE ACTIVO = 1");
        }

        /// <summary>
        /// Obtiene la lista de equivalencias entre unidades de medida según la unidad base especificada.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="cod_unidad">Unidad base.</param>
        /// <returns>Listado de equivalencias.</returns>
        public ErrorDto<UnidadesConvLista> UnidadConvLista_Obtener(int CodCliente, string cod_unidad)
        {
            var result = DbHelper.ExecuteListQuery<UnidadMedicionConvData>(
                CreatePortalDb(),
                CodCliente,
                "SELECT * FROM PV_UNIDADES_CONV WHERE COD_UNIDAD = @cod_unidad",
                new { cod_unidad });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(new UnidadesConvLista
                {
                    total = result.Result?.Count ?? 0,
                    lista = result.Result ?? new List<UnidadMedicionConvData>()
                })
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener equivalencias de unidades.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Guarda o actualiza una equivalencia entre unidades de medida.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="equivalencia">Datos de la equivalencia.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UnidadConv_Guardar(int CodCliente, UnidadMedicionConvData equivalencia)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodCliente, connection =>
            {
                int count = ContarEquivalencia(connection, equivalencia);

                if (count > 0)
                {
                    connection.Execute(
                        @"UPDATE PV_UNIDADES_CONV
                          SET FACTOR = @factor
                          WHERE COD_UNIDAD = @cod_unidad
                            AND COD_UNIDAD_D = @cod_unidad_d",
                        new
                        {
                            equivalencia.factor,
                            equivalencia.cod_unidad,
                            equivalencia.cod_unidad_d
                        });
                }
                else
                {
                    connection.Execute(
                        @"INSERT INTO PV_UNIDADES_CONV (COD_UNIDAD, COD_UNIDAD_D, FACTOR)
                          VALUES (@cod_unidad, @cod_unidad_d, @factor)",
                        new
                        {
                            equivalencia.cod_unidad,
                            equivalencia.cod_unidad_d,
                            equivalencia.factor
                        });
                }

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar la equivalencia de unidades.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una equivalencia entre unidades de medida según la unidad base y la unidad destino especificadas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="cod_unidad">Unidad base.</param>
        /// <param name="cod_unidad_d">Unidad destino.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UnidadConv_Eliminar(int CodCliente, string cod_unidad, string cod_unidad_d)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                @"DELETE FROM PV_UNIDADES_CONV
                  WHERE COD_UNIDAD = @cod_unidad
                    AND COD_UNIDAD_D = @cod_unidad_d",
                new
                {
                    cod_unidad,
                    cod_unidad_d
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar la equivalencia de unidades.");
        }

        #endregion
    }
}