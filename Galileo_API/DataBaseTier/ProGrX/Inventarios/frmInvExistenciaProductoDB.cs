using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvExistenciaProductoDB
    {
        private const string SpInvProcesoProd = "[spINVProcesoProd]";
        private const int SpMuestra = 1;

        private const string ErrorConsultarExistencia =
            "Ocurri&oacute; un error al consultar la existencia del producto.";

        private const string ErrorFechaServidor =
            "Error al obtener fecha del servidor.";

        private const string QueryFechaServidor =
            "SELECT dbo.MyGetdate() AS Fecha";

        private const string QueryBodegas = """
            SELECT
                B.cod_bodega,
                B.descripcion
            FROM pv_bodegas B
            ORDER BY B.cod_bodega
            """;

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="FrmInvExistenciaProductoDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvExistenciaProductoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> con la configuración actual.
        /// </summary>
        /// <returns>Acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la fecha del servidor (fxFechaServidor del VB6, usado en Form_Load).
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Fecha del servidor en Description con formato yyyy-MM-dd HH:mm:ss.</returns>
        public ErrorDto INV_ExistenciaProducto_FechaServidor_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<DateTime>(
                CreatePortalDb(),
                CodEmpresa,
                QueryFechaServidor,
                default);

            return result.Code == 0
                ? DbHelper.OkResponse(result.Result.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                : DbHelper.ErrorResponse(result.Description ?? ErrorFechaServidor, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene la existencia al corte del producto en cada bodega y el total general.
        /// Equivale a btnBuscar_Click (Index 0) de frmInvExistenciaProducto en VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consulta">Producto, fecha de corte (yyyy-MM-dd) y usuario.</param>
        /// <returns>Existencias por bodega y total.</returns>
        public ErrorDto<InvExistenciaProductoResultadoDto> INV_ExistenciaProducto_Consultar(
            int CodEmpresa,
            InvExistenciaProductoConsulta consulta)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var bodegas = connection
                    .Query<InvExistenciaProductoBodegaDto>(QueryBodegas)
                    .ToList();

                foreach (var bodega in bodegas)
                {
                    bodega.existencia = INV_ExistenciaProducto_Bodega_Existencia_Obtener(
                        connection,
                        consulta,
                        bodega.cod_bodega);
                }

                return new InvExistenciaProductoResultadoDto
                {
                    bodegas = bodegas,
                    total_existencia = bodegas.Sum(b => b.existencia)
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new InvExistenciaProductoResultadoDto())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarExistencia,
                    result.Code.GetValueOrDefault(-1),
                    new InvExistenciaProductoResultadoDto());
        }

        /// <summary>
        /// Calcula la existencia del producto en una bodega a la fecha de corte (fxInvProcesoProd del VB6).
        /// Si el SP falla retorna 0, igual que el manejo de error del VB6.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="consulta">Producto, fecha de corte y usuario.</param>
        /// <param name="codBodega">Código de la bodega.</param>
        /// <returns>Existencia calculada.</returns>
        private static decimal INV_ExistenciaProducto_Bodega_Existencia_Obtener(
            IDbConnection connection,
            InvExistenciaProductoConsulta consulta,
            string codBodega)
        {
            try
            {
                return connection.QueryFirstOrDefault<decimal>(
                    SpInvProcesoProd,
                    new
                    {
                        CodProd = consulta.cod_producto.Trim(),
                        Bodega = codBodega,
                        Fecha = consulta.fecha_corte,
                        Usuario = consulta.usuario,
                        Muestra = SpMuestra,
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch
            {
                return 0;
            }
        }
    }
}
