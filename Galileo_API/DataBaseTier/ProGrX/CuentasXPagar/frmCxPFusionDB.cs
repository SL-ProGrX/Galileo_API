using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPFusionDB
    {
        #region Constructor y helper

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPFusionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPFusionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una respuesta vacía para el listado de proveedores.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static CxpProveedoresDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Proveedores = new List<CxpProveedorData>()
        };

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene el listado paginado de proveedores activos no fusionados.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro opcional por código o descripción.</param>
        /// <returns>Listado paginado de proveedores disponibles para fusión.</returns>
        public ErrorDto<CxpProveedoresDataLista> Proveedores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                var filtroTexto = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro.Trim()}%";

                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(cod_proveedor)
                      FROM cxp_proveedores
                      WHERE ESTADO = 'A'
                        AND fusion IS NULL
                        AND (
                            @Filtro IS NULL
                            OR CAST(cod_proveedor AS varchar(50)) LIKE @Filtro
                            OR descripcion LIKE @Filtro
                        )",
                    new { Filtro = filtroTexto });

                if (pagina.HasValue && paginacion.HasValue)
                {
                    respuesta.Proveedores = connection.Query<CxpProveedorData>(
                        @"SELECT COD_PROVEEDOR, DESCRIPCION
                          FROM CXP_PROVEEDORES
                          WHERE ESTADO = 'A'
                            AND fusion IS NULL
                            AND (
                                @Filtro IS NULL
                                OR CAST(cod_proveedor AS varchar(50)) LIKE @Filtro
                                OR descripcion LIKE @Filtro
                            )
                          ORDER BY DESCRIPCION
                          OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                        new
                        {
                            Filtro = filtroTexto,
                            Offset = pagina.Value,
                            Fetch = paginacion.Value
                        }).ToList();
                }
                else
                {
                    respuesta.Proveedores = connection.Query<CxpProveedorData>(
                        @"SELECT COD_PROVEEDOR, DESCRIPCION
                          FROM CXP_PROVEEDORES
                          WHERE ESTADO = 'A'
                            AND fusion IS NULL
                            AND (
                                @Filtro IS NULL
                                OR CAST(cod_proveedor AS varchar(50)) LIKE @Filtro
                                OR descripcion LIKE @Filtro
                            )
                          ORDER BY DESCRIPCION",
                        new { Filtro = filtroTexto }).ToList();
                }

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener proveedores para fusión.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Aplica la fusión de proveedores inactivando los fusionados y registrando la relación en la tabla de fusiones.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="proveedor">Proveedor principal que recibirá la fusión.</param>
        /// <param name="proveedores">Lista de proveedores a fusionar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fusion_Aplicar(int CodCliente, int proveedor, List<CxpProveedorData> proveedores)
        {
            if (proveedor <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un proveedor principal válido.", -2);
            }

            if (proveedores is null || proveedores.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos un proveedor para fusionar.", -2);
            }

            var proveedoresFusion = proveedores
                .Select(item => int.TryParse(item.Cod_Proveedor, out var codProveedor) ? codProveedor : 0)
                .Where(codProveedor => codProveedor > 0 && codProveedor != proveedor)
                .Distinct()
                .ToList();

            if (proveedoresFusion.Count == 0)
            {
                return DbHelper.ErrorResponse("No existen proveedores válidos para aplicar la fusión.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                foreach (var codProveedor in proveedoresFusion)
                {
                    connection.Execute(
                        @"UPDATE cxp_proveedores
                          SET estado = 'I',
                              fusion = GETDATE()
                          WHERE cod_proveedor = @CodProveedor",
                        new { CodProveedor = codProveedor });

                    connection.Execute(
                        @"INSERT cxp_fusiones(cod_proveedor, cod_proveedor_fus)
                          VALUES (@ProveedorPrincipal, @CodProveedor)",
                        new
                        {
                            ProveedorPrincipal = proveedor,
                            CodProveedor = codProveedor
                        });
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Proveedores fusionados correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al aplicar la fusión de proveedores.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}