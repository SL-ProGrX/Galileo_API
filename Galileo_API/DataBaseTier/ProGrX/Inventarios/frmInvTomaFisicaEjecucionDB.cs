using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvTomaFisicaEjecucionDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTomaFisicaEjecucionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTomaFisicaEjecucionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Ajusta la existencia del producto restando la existencia física procesada.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="producto">Producto a ajustar.</param>
        private static void ActualizarExistenciaProducto(IDbConnection connection, ProductosTomaFisica producto)
        {
            connection.Execute(
                @"UPDATE PV_PRODUCTOS
                  SET Existencia = Existencia - @Existencia_Fisica
                  WHERE cod_producto = @Cod_Producto",
                new
                {
                    Cod_Producto = producto.cod_producto,
                    Existencia_Fisica = producto.existencia_fisica
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene las entradas de la tabla pv_entrada_salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de entradas.</returns>
        public ErrorDto<List<EntradasTomaFisicaDto>> Obtener_Entradas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<EntradasTomaFisicaDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(cod_entsal) AS Codigo,
                         RTRIM(descripcion) AS Descripcion
                  FROM pv_entrada_salida
                  WHERE tipo = 'E'");
        }

        /// <summary>
        /// Obtiene las salidas de la tabla pv_entrada_salida para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de salidas.</returns>
        public ErrorDto<List<SalidasTomaFisicaDto>> Obtener_Salidas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<SalidasTomaFisicaDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT RTRIM(cod_entsal) AS Codigo,
                         RTRIM(descripcion) AS Descripcion
                  FROM pv_entrada_salida
                  WHERE tipo = 'S'");
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Procesa la toma física de inventario actualizando estado, datos de aplicación y existencias.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma física.</param>
        /// <param name="usuario">Usuario que procesa.</param>
        /// <param name="cod_entrada">Código de causa de entrada.</param>
        /// <param name="cod_salida">Código de causa de salida.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ProcesarTomaFisica(int CodEmpresa, int consecutivo, string usuario, string cod_entrada, string cod_salida)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    @"UPDATE PV_INVTOMAFISICA
                      SET ESTADO = 'P',
                          FECHA_APLICA = GETDATE(),
                          USER_APLICA = @Usuario,
                          CAUSA_ENTRADA = @Cod_Entrada,
                          CAUSA_SALIDA = @Cod_Salida,
                          COD_ENTRADAG = 0,
                          COD_SALIDAG = 0
                      WHERE CONSECUTIVO = @Consecutivo",
                    new
                    {
                        Usuario = usuario,
                        Cod_Entrada = cod_entrada,
                        Cod_Salida = cod_salida,
                        Consecutivo = consecutivo
                    });

                var productos = connection.Query<ProductosTomaFisica>(
                    @"SELECT tomaFisicaDetalle.COD_PRODUCTO,
                             tomaFisicaDetalle.EXISTENCIA_FISICA
                      FROM pv_InvTomaFisica AS tomaFisica
                      INNER JOIN pv_invTF_Detalle AS tomaFisicaDetalle
                        ON tomaFisicaDetalle.consecutivo = tomaFisica.consecutivo
                      WHERE tomaFisica.consecutivo = @Consecutivo",
                    new { Consecutivo = consecutivo }).ToList();

                foreach (var producto in productos)
                {
                    ActualizarExistenciaProducto(connection, producto);
                }

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al procesar la toma física.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}