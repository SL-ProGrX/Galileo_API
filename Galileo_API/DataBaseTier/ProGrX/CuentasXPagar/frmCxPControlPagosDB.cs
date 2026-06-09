using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPControlPagosDB
    {
        private readonly IConfiguration _config;
        private const string FechaVencimientoColumna = "C.Fecha_Vencimiento";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPControlPagosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPControlPagosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el detalle de pagos de cuentas por pagar según los filtros indicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="pagosParametros">Parámetros de filtrado.</param>
        /// <returns>Listado de pagos encontrados.</returns>
        public ErrorDto<List<ControlPagosData>> CxPControlPagos_Obtener(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosConsulta(pagosParametros);
                var query = ConstruirQueryControlPagosDetalle(pagosParametros);
                return connection.Query<ControlPagosData>(query, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<ControlPagosData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el control de pagos.", result.Code.GetValueOrDefault(-1), new List<ControlPagosData>());
        }

        /// <summary>
        /// Obtiene el resumen de pagos de cuentas por pagar agrupado por proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="pagosParametros">Parámetros de filtrado.</param>
        /// <returns>Listado resumen de pagos por proveedor.</returns>
        public ErrorDto<List<ControlPagosResumenData>> CxPCOntrolPagos_Resumen(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosConsulta(pagosParametros);
                var query = ConstruirQueryControlPagosResumen(pagosParametros);
                return connection.Query<ControlPagosResumenData>(query, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<ControlPagosResumenData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el resumen de control de pagos.", result.Code.GetValueOrDefault(-1), new List<ControlPagosResumenData>());
        }

        /// <summary>
        /// Crea los parámetros comunes para las consultas de control de pagos.
        /// </summary>
        /// <param name="pagosParametros">Parámetros recibidos del formulario.</param>
        /// <returns>Parámetros listos para Dapper.</returns>
        private static DynamicParameters CrearParametrosConsulta(CxPControlPagosParametros pagosParametros)
        {
            var parametros = new DynamicParameters();
            parametros.Add("FechaInicio", $"{pagosParametros.fechaInicio} 00:00:00");
            parametros.Add("FechaCorte", $"{pagosParametros.fechaCorte} 23:59:59");
            parametros.Add("TipoCancelacion", pagosParametros.tipo_Cancelacion);

            if (!pagosParametros.cboProveedor && pagosParametros.codProveedor != 0)
            {
                parametros.Add("CodProveedor", pagosParametros.codProveedor);
            }

            if (!string.IsNullOrWhiteSpace(pagosParametros.factura))
            {
                parametros.Add("Factura", $"%{pagosParametros.factura}%");
            }

            if (!string.IsNullOrWhiteSpace(pagosParametros.documento))
            {
                parametros.Add("Documento", $"%{pagosParametros.documento}%");
            }

            if (!string.IsNullOrWhiteSpace(pagosParametros.noSolicitud))
            {
                parametros.Add("NoSolicitud", pagosParametros.noSolicitud);
            }

            return parametros;
        }

        /// <summary>
        /// Construye la consulta detallada de control de pagos.
        /// </summary>
        /// <param name="pagosParametros">Parámetros de filtrado.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ConstruirQueryControlPagosDetalle(CxPControlPagosParametros pagosParametros)
        {
            var builder = new StringBuilder(@"select C.*,P.descripcion as Proveedor,B.descripcion as Banco,T.ndocumento
                                from cxp_PagoProv C inner join CxP_Proveedores P on P.cod_proveedor = C.cod_proveedor
                                left join Tes_Transacciones T on C.tesoreria = T.nsolicitud
                                left join Tes_Bancos B on T.id_banco = B.id_Banco
                                where ");

            builder.Append(ObtenerColumnaFecha(pagosParametros));
            builder.Append(@" between @FechaInicio and @FechaCorte
                                and C.Tipo_Cancelacion = @TipoCancelacion ");

            AgregarFiltrosComunes(builder, pagosParametros);
            return builder.ToString();
        }

        /// <summary>
        /// Construye la consulta resumen de control de pagos.
        /// </summary>
        /// <param name="pagosParametros">Parámetros de filtrado.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ConstruirQueryControlPagosResumen(CxPControlPagosParametros pagosParametros)
        {
            var builder = new StringBuilder(@"select count(*) as Pagos,sum(C.monto) as Monto,P.descripcion as Proveedor,P.cod_proveedor
                                     from cxp_PagoProv C inner join CxP_Proveedores P on P.cod_proveedor = C.cod_proveedor
                                     left join Tes_Transacciones T on C.tesoreria = T.nsolicitud
                                     left join Tes_Bancos B on T.id_banco = B.id_Banco
                                     where ");

            builder.Append(ObtenerColumnaFecha(pagosParametros));
            builder.Append(@" between @FechaInicio and @FechaCorte
                                           and C.Tipo_Cancelacion = @TipoCancelacion ");

            AgregarFiltrosComunes(builder, pagosParametros);
            builder.Append(" group by P.cod_proveedor,P.descripcion ");
            return builder.ToString();
        }

        /// <summary>
        /// Agrega al query los filtros comunes aplicables al detalle y al resumen.
        /// </summary>
        /// <param name="builder">Constructor del SQL.</param>
        /// <param name="pagosParametros">Parámetros de filtrado.</param>
        private static void AgregarFiltrosComunes(StringBuilder builder, CxPControlPagosParametros pagosParametros)
        {
            if (!pagosParametros.cboProveedor && pagosParametros.codProveedor != 0)
            {
                builder.Append(" and C.cod_proveedor = @CodProveedor");
            }

            switch (pagosParametros.cboEstado)
            {
                case "S":
                    builder.Append(" and C.tesoreria is null ");
                    break;
                case "E":
                    builder.Append(" and C.tesoreria is not null ");
                    break;
                case "P":
                    builder.Append(" and C.tesoreria is not null and C.TESORERIA_ESTADO = 'P' ");
                    break;
                case "C":
                    builder.Append(" and C.tesoreria is not null and C.TESORERIA_ESTADO in('I','T','E') ");
                    break;
                case "A":
                    builder.Append(" and C.tesoreria is not null and C.TESORERIA_ESTADO in('A','N') ");
                    break;
                case "T":
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(pagosParametros.factura))
            {
                builder.Append(" and C.cod_factura like @Factura");
            }

            if (!string.IsNullOrWhiteSpace(pagosParametros.documento))
            {
                builder.Append(" and T.Ndocumento like @Documento ");
            }

            if (!string.IsNullOrWhiteSpace(pagosParametros.noSolicitud))
            {
                builder.Append(" and C.Tesoreria = @NoSolicitud ");
            }
        }

        /// <summary>
        /// Obtiene la columna de fecha a usar según el filtro seleccionado.
        /// </summary>
        /// <param name="pagosParametros">Parámetros del formulario.</param>
        /// <returns>Nombre de columna SQL permitido.</returns>
        private static string ObtenerColumnaFecha(CxPControlPagosParametros pagosParametros)
        {
            return pagosParametros.cboFecha switch
            {
                "E" => "C.Fecha_Traslada",
                "V" => FechaVencimientoColumna,
                "C" => "T.Fecha_Emision",
                _ => FechaVencimientoColumna,
            };
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}