using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPTiposProvDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helper

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPTiposProvDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPTiposProvDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene las clasificaciones de proveedores registradas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Listado de clasificaciones de proveedores.</returns>
        public ErrorDto<List<TiposProveedorDto>> ObtenerClasificacionProveedores(int CodCliente)
        {
            return DbHelper.ExecuteListQuery<TiposProveedorDto>(
                CreatePortalDb(),
                CodCliente,
                @"SELECT cod_clasificacion AS CodClasificacion,
                         descripcion AS Descripcion,
                         NIT_Codigo AS NitCodigo,
                         Activo
                  FROM cxp_prov_clas
                  ORDER BY cod_clasificacion");
        }

        /// <summary>
        /// Obtiene los proveedores activos.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Listado de proveedores activos.</returns>
        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodCliente)
        {
            return DbHelper.ExecuteListQuery<Proveedor>(
                CreatePortalDb(),
                CodCliente,
                "SELECT COD_PROVEEDOR, DESCRIPCION FROM CXP_PROVEEDORES WHERE ESTADO = 'A' ORDER BY COD_PROVEEDOR");
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza un tipo de proveedor.
        /// </summary>
        /// <param name="request">Datos del tipo de proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProveedor_Actualizar(TiposProveedorDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                request.CodEmpresa,
                @"UPDATE cxp_prov_clas
                  SET descripcion = @Descripcion,
                      nit_codigo = @NitCodigo,
                      Activo = @Activo
                  WHERE cod_clasificacion = @CodClasificacion",
                new
                {
                    request.CodClasificacion,
                    request.Descripcion,
                    request.NitCodigo,
                    request.Activo
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Tipo proveedor actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar tipo de proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un tipo de proveedor.
        /// </summary>
        /// <param name="request">Datos del tipo de proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProveedor_Eliminar(TiposProveedorDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                request.CodEmpresa,
                "DELETE cxp_prov_clas WHERE cod_clasificacion = @CodClasificacion",
                new
                {
                    request.CodClasificacion
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Tipo proveedor eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo tipo de proveedor.
        /// </summary>
        /// <param name="request">Datos del tipo de proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TipoProveedor_Insertar(TiposProveedorDto request)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), request.CodEmpresa, connection =>
                connection.Query<int>(
                    "[spCxP_W_TipoProveeedor_Agregar]",
                    new
                    {
                        Cod_Clasificacion = request.CodClasificacion,
                        Descripcion = request.Descripcion,
                        Nit_Codigo = request.NitCodigo,
                        Activo = request.Activo,
                    },
                    commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault());

            return result.Code == 0
                ? DbHelper.OkResponse("Tipo Proveedor agregado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al agregar tipo de proveedor.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}