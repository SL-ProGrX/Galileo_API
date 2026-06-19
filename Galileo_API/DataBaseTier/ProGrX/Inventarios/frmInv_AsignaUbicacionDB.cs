using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvAsignaUbicacionDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvAsignaUbicacionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvAsignaUbicacionDB(IConfiguration config)
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
        /// Crea una respuesta estándar para operaciones de actualización.
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
        /// Crea los parámetros comunes para una asignación de ubicación.
        /// </summary>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosAsignacion(int CodAsignaUbicacion) => new
        {
            CodAsignaUbicacion
        };

        /// <summary>
        /// Crea los parámetros comunes para una asignación y su línea.
        /// </summary>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <param name="Linea">Línea del detalle.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosAsignacionLinea(int CodAsignaUbicacion, int Linea) => new
        {
            CodAsignaUbicacion,
            Linea
        };

        /// <summary>
        /// Obtiene el siguiente consecutivo disponible para asignación de ubicación.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Consecutivo formateado a 10 caracteres.</returns>
        private static string ObtenerSiguienteConsecutivo(IDbConnection connection)
        {
            var consecutivo = connection.QueryFirstOrDefault<int>(
                "select isnull(max(COD_ASIGNAUBICACION),0)+1 as Ultimo from INV_UBICACIONES");

            return consecutivo.ToString().PadLeft(10, '0');
        }

        /// <summary>
        /// Inserta el detalle de productos asociados a una asignación de ubicación.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="CodAsignaUbicacion">Código de asignación.</param>
        /// <param name="producLineas">Líneas de productos a registrar.</param>
        private static void InsertarDetalleUbicacion(IDbConnection connection, int CodAsignaUbicacion, List<AsignaUbicacionDetalleDto> producLineas)
        {
            var linea = 0;
            foreach (AsignaUbicacionDetalleDto item in producLineas)
            {
                linea++;
                connection.Execute(
                    @"insert INV_UBICACIONES_DETALLE(linea, COD_ASIGNAUBICACION, COD_PRODUCTO, CANTIDAD, UBICACION)
                      values(@Linea, @CodAsignaUbicacion, @Cod_Producto, @Cantidad, @Ubicacion)",
                    new
                    {
                        Linea = linea,
                        CodAsignaUbicacion,
                        Cod_Producto = item.cod_producto,
                        Cantidad = item.existencia,
                        Ubicacion = item.ubicacion
                    });
            }
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene las ubicaciones del producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <returns>Cabecera de la asignación de ubicación.</returns>
        public ErrorDto<AsignaUbicacionDto> InvUbicaciones_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            var result = DbHelper.ExecuteSingleQuery<AsignaUbicacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT COD_ASIGNAUBICACION,
                         ESTADO,
                         COD_BODEGA,
                         RESPONSABLE,
                         FECHA,
                         NOTAS
                  FROM INV_UBICACIONES
                  WHERE COD_ASIGNAUBICACION = @CodAsignaUbicacion",
                null,
                CrearParametrosAsignacion(CodAsignaUbicacion));

            return CrearRespuestaSingle(
                result,
                "Error al obtener la ubicación de inventario.",
                "No se encontró la asignación de ubicación.");
        }

        /// <summary>
        /// Obtiene la línea del producto en inventario para su ubicación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <returns>Listado de productos asociados a la ubicación.</returns>
        public ErrorDto<List<AsignaUbicacionDetalleDto>> InvUbicacionProduc_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            return DbHelper.ExecuteListQuery<AsignaUbicacionDetalleDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select D.COD_ASIGNAUBICACION,
                         D.linea,
                         D.cod_producto AS cod_producto,
                         P.Descripcion,
                         D.Cantidad as Existencia,
                         D.UBICACION
                  from INV_UBICACIONES_DETALLE D
                  inner join pv_productos P on D.cod_producto = P.cod_producto
                  where D.COD_ASIGNAUBICACION = @CodAsignaUbicacion
                  order by D.Linea",
                CrearParametrosAsignacion(CodAsignaUbicacion));
        }

        /// <summary>
        /// Obtiene una asignación de ubicación anterior o siguiente según el desplazamiento indicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="scrollValue">Dirección del desplazamiento.</param>
        /// <param name="CodAsignaUbicacion">Código actual de asignación.</param>
        /// <returns>Asignación encontrada para el desplazamiento.</returns>
        public ErrorDto<AsignaUbicacionDto> InvUbicacion_scroll(int CodEmpresa, int scrollValue, int? CodAsignaUbicacion)
        {
            const string query = @"select Top 1 COD_ASIGNAUBICACION
                                   from INV_UBICACIONES
                                   where ((@ScrollValue = 1 and COD_ASIGNAUBICACION > @CodAsignaUbicacion)
                                       or (@ScrollValue <> 1 and COD_ASIGNAUBICACION < @CodAsignaUbicacion))
                                   order by
                                       case when @ScrollValue = 1 then COD_ASIGNAUBICACION end asc,
                                       case when @ScrollValue <> 1 then COD_ASIGNAUBICACION end desc";

            var result = DbHelper.ExecuteSingleQuery<AsignaUbicacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                new
                {
                    ScrollValue = scrollValue,
                    CodAsignaUbicacion
                });

            return CrearRespuestaSingle(
                result,
                "Error al obtener el desplazamiento de asignación de ubicación.",
                "No se encontró otra asignación para el desplazamiento solicitado.");
        }

        /// <summary>
        /// Obtiene la lista de tareas de ubicaciones de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de asignaciones de ubicación.</returns>
        public ErrorDto<List<AsignaUbicacionDto>> InvAsignaUbicacion_Lista(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AsignaUbicacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT U.*, 
                         B.DESCRIPCION AS descripcion_bodega
                  FROM INV_UBICACIONES U
                  LEFT JOIN PV_BODEGAS B ON U.COD_BODEGA = B.COD_BODEGA");
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Inserta una nueva ubicación de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la asignación.</param>
        /// <returns>Resultado de la operación con el consecutivo generado en la descripción.</returns>
        public ErrorDto InvAsignaUbicacion_Insertar(int CodEmpresa, AsignaUbicacionDto request)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var consecutivo = ObtenerSiguienteConsecutivo(connection);
                connection.Execute(
                    @"INSERT INTO INV_UBICACIONES(cod_asignaubicacion, cod_bodega, notas, estado, responsable, cod_unidad, fecha, genera_user, fecha_user)
                      VALUES(@Cod_AsignaUbicacion, @Cod_Bodega, @Notas, @Estado, @Responsable, @Cod_Unidad, getdate(), @Genera_User, getdate())",
                    new
                    {
                        Cod_AsignaUbicacion = consecutivo,
                        Cod_Bodega = request.cod_bodega,
                        Notas = request.notas,
                        Estado = request.estado,
                        Responsable = request.responsable,
                        Cod_Unidad = request.cod_unidad,
                        Genera_User = request.genera_user
                    });

                return consecutivo;
            });

            return result.Code == 0
                ? new ErrorDto { Code = 0, Description = result.Result ?? string.Empty }
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la asignación de ubicación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza una ubicación de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la asignación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvAsignaUbicacion_Actualizar(int CodEmpresa, AsignaUbicacionDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE INV_UBICACIONES
                  SET cod_bodega = @Cod_Bodega,
                      fecha_user = GETDATE(),
                      documento = @Documento,
                      notas = @Notas,
                      cod_unidad = @Cod_Unidad,
                      responsable = @Responsable
                  WHERE COD_ASIGNAUBICACION = @CodAsignaUbicacion",
                new
                {
                    CodAsignaUbicacion = request.cod_asignaubicacion,
                    Cod_Bodega = request.cod_bodega,
                    Documento = request.documento,
                    Notas = request.notas,
                    Cod_Unidad = request.cod_unidad,
                    Responsable = request.responsable
                });

            return CrearRespuestaNonQuery(result, "Registro actualizado correctamente", "Error al actualizar la asignación de ubicación.");
        }

        /// <summary>
        /// Elimina una ubicación de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvAsignaUbicacion_Eliminar(int CodEmpresa, int CodAsignaUbicacion)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "delete INV_UBICACIONES_DETALLE where COD_ASIGNAUBICACION = @CodAsignaUbicacion",
                    CrearParametrosAsignacion(CodAsignaUbicacion));

                connection.Execute(
                    "delete INV_UBICACIONES where COD_ASIGNAUBICACION = @CodAsignaUbicacion",
                    CrearParametrosAsignacion(CodAsignaUbicacion));

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar la asignación de ubicación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta una nueva ubicación de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <param name="producLineas">Líneas de productos a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvAsignaUbicacionProduc_Insertar(int CodEmpresa, int CodAsignaUbicacion, List<AsignaUbicacionDetalleDto> producLineas)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "delete INV_UBICACIONES_DETALLE where COD_ASIGNAUBICACION = @CodAsignaUbicacion",
                    CrearParametrosAsignacion(CodAsignaUbicacion));

                InsertarDetalleUbicacion(connection, CodAsignaUbicacion, producLineas);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Informacion guardada satisfactoriamente...")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar el detalle de la ubicación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Cambia de estado la tarea de ubicación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codigoAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <param name="Usuario">Usuario que autoriza.</param>
        /// <param name="Estado">Estado a aplicar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvAsignacionUbicacion_CerrarOrden_Finalizar(int CodEmpresa, int codigoAsignaUbicacion, string Usuario, string Estado)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"update INV_UBICACIONES
                  set estado = @Estado,
                      Autoriza_user = @Autoriza_User,
                      autoriza_fecha = getdate()
                  where COD_ASIGNAUBICACION = @CodAsignaUbicacion",
                new
                {
                    CodAsignaUbicacion = codigoAsignaUbicacion,
                    Autoriza_User = Usuario,
                    Estado
                });

            return CrearRespuestaNonQuery(result, "Registro actualizado correctamente", "Error al actualizar el estado de la asignación de ubicación.");
        }

        /// <summary>
        /// Elimina un producto de la ubicación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodAsignaUbicacion">Código de asignación de ubicación.</param>
        /// <param name="Linea">Línea del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvAsignaUbicacionProduc_Eliminar(int CodEmpresa, int CodAsignaUbicacion, int Linea)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"delete INV_UBICACIONES_DETALLE
                  where COD_ASIGNAUBICACION = @CodAsignaUbicacion
                    and linea = @Linea",
                CrearParametrosAsignacionLinea(CodAsignaUbicacion, Linea));

            return CrearRespuestaNonQuery(result, "Registro eliminado correctamente", "Error al eliminar el producto de la ubicación.");
        }

        #endregion
    }
}