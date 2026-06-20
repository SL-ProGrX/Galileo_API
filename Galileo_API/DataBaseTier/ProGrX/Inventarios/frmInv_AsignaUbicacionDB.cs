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
        /// Crea los parámetros para insertar una asignación de ubicación.
        /// </summary>
        /// <param name="request">Datos de la asignación.</param>
        /// <param name="consecutivo">Consecutivo generado.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosInsertar(AsignaUbicacionDto request, string consecutivo) => new
        {
            Cod_AsignaUbicacion = consecutivo,
            Cod_Bodega = request.cod_bodega,
            Notas = request.notas,
            Estado = request.estado,
            Responsable = request.responsable,
            Cod_Unidad = request.cod_unidad,
            Genera_User = request.genera_user
        };

        /// <summary>
        /// Crea los parámetros para actualizar una asignación de ubicación.
        /// </summary>
        /// <param name="request">Datos de la asignación.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosActualizar(AsignaUbicacionDto request) => new
        {
            CodAsignaUbicacion = request.cod_asignaubicacion,
            Cod_Bodega = request.cod_bodega,
            Documento = request.documento,
            Notas = request.notas,
            Cod_Unidad = request.cod_unidad,
            Responsable = request.responsable
        };

        /// <summary>
        /// Crea los parámetros para cerrar o finalizar una asignación de ubicación.
        /// </summary>
        /// <param name="codigoAsignaUbicacion">Código de asignación.</param>
        /// <param name="usuario">Usuario que autoriza.</param>
        /// <param name="estado">Estado a aplicar.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosCerrarOrden(int codigoAsignaUbicacion, string usuario, string estado) => new
        {
            CodAsignaUbicacion = codigoAsignaUbicacion,
            Autoriza_User = usuario,
            Estado = estado
        };

        /// <summary>
        /// Crea los parámetros para insertar una línea de detalle de ubicación.
        /// </summary>
        /// <param name="linea">Número de línea.</param>
        /// <param name="codAsignaUbicacion">Código de asignación.</param>
        /// <param name="item">Detalle del producto.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosDetalle(int linea, int codAsignaUbicacion, AsignaUbicacionDetalleDto item) => new
        {
            Linea = linea,
            CodAsignaUbicacion = codAsignaUbicacion,
            Cod_Producto = item.cod_producto,
            Cantidad = item.existencia,
            Ubicacion = item.ubicacion
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
                    CrearParametrosDetalle(linea, CodAsignaUbicacion, item));
            }
        }

        #endregion

        #region Consultas

        public ErrorDto<AsignaUbicacionDto?> InvUbicaciones_Obtener(int CodEmpresa, int CodAsignaUbicacion) =>
            DbHelper.ExecuteSingleQuery<AsignaUbicacionDto>(
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

        public ErrorDto<AsignaUbicacionDto?> InvUbicacion_scroll(int CodEmpresa, int scrollValue, int? CodAsignaUbicacion) =>
            DbHelper.ExecuteSingleQuery<AsignaUbicacionDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select Top 1 COD_ASIGNAUBICACION
                                   from INV_UBICACIONES
                                   where ((@ScrollValue = 1 and COD_ASIGNAUBICACION > @CodAsignaUbicacion)
                                       or (@ScrollValue <> 1 and COD_ASIGNAUBICACION < @CodAsignaUbicacion))
                                   order by
                                       case when @ScrollValue = 1 then COD_ASIGNAUBICACION end asc,
                                       case when @ScrollValue <> 1 then COD_ASIGNAUBICACION end desc",
                null,
                new
                {
                    ScrollValue = scrollValue,
                    CodAsignaUbicacion
                });

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
                    CrearParametrosInsertar(request, consecutivo));

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
                CrearParametrosActualizar(request));

            return result.Code == 0
                ? DbHelper.OkResponse("Registro actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar la asignación de ubicación.", result.Code.GetValueOrDefault(-1));
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
                CrearParametrosCerrarOrden(codigoAsignaUbicacion, Usuario, Estado));

            return result.Code == 0
                ? DbHelper.OkResponse("Registro actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el estado de la asignación de ubicación.", result.Code.GetValueOrDefault(-1));
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

            return result.Code == 0
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar el producto de la ubicación.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}