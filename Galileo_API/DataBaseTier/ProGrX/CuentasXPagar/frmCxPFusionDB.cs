using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPFusionDB
    {
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
                var respuesta = new CxpProveedoresDataLista
                {
                    Total = 0,
                    Proveedores = new List<CxpProveedorData>()
                };

                var parametros = new DynamicParameters();
                var condiciones = new List<string>
                {
                    "ESTADO = 'A'",
                    "fusion is null"
                };

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    condiciones.Add("(CAST(cod_proveedor AS varchar(50)) LIKE @Filtro OR descripcion LIKE @Filtro)");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                var whereClause = " WHERE " + string.Join(" AND ", condiciones);

                var totalQuery = "SELECT COUNT(cod_proveedor) from cxp_proveedores" + whereClause;
                respuesta.Total = connection.QueryFirstOrDefault<int>(totalQuery, parametros);

                var queryBuilder = new StringBuilder(@"SELECT COD_PROVEEDOR, DESCRIPCION 
                                                      FROM CXP_PROVEEDORES");
                queryBuilder.Append(whereClause);
                queryBuilder.Append(" ORDER BY DESCRIPCION");

                if (pagina.HasValue && paginacion.HasValue)
                {
                    queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Proveedores = connection.Query<CxpProveedorData>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CxpProveedoresDataLista { Total = 0, Proveedores = new List<CxpProveedorData>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener proveedores para fusión.", result.Code.GetValueOrDefault(-1), new CxpProveedoresDataLista { Total = 0, Proveedores = new List<CxpProveedorData>() });
        }

        /// <summary>
        /// Aplica la fusión de proveedores inactivando los fusionados y registrando la relación en la tabla de fusiones.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="proveedor">Proveedor principal que recibirá la fusión.</param>
        /// <param name="proveedores">Lista de proveedores a fusionar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Fusion_Aplicar(int CodCliente, int proveedor, List<CxpProveedorData> proveedores)
        {
            if (proveedores is null || proveedores.Count == 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar al menos un proveedor para fusionar.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                foreach (var codProveedor in proveedores.Select(item => item.Cod_Proveedor))
                {
                    connection.Execute(
                        @"update cxp_proveedores
                          set estado = 'I',
                              fusion = Getdate()
                          where cod_proveedor = @CodProveedor",
                        new { CodProveedor = codProveedor });

                    connection.Execute(
                        @"insert cxp_fusiones(cod_proveedor, cod_proveedor_fus)
                          values (@ProveedorPrincipal, @CodProveedor)",
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

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}