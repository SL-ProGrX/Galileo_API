using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using System.Data;
using Galileo.Models.Security;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPControlReprogramacionDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;

        #region Helpers privados

        /// <summary>
        /// Crea una lista vacía de facturas para respuestas sin resultados.
        /// </summary>
        /// <returns>Lista de facturas inicializada.</returns>
        private static FacturaLista CrearFacturaListaVacia() => new()
        {
            Total = 0,
            Facturas = new List<Factura>()
        };

        /// <summary>
        /// Convierte el resultado de una consulta única en una respuesta estándar.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="result">Resultado devuelto por DbHelper.</param>
        /// <param name="errorMessage">Mensaje de error cuando la consulta falla.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar para consultas de una sola entidad.</returns>
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

        #endregion

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPControlReprogramacionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPControlReprogramacionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            DBBitacora = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad.
        /// </summary>
        /// <param name="data">Datos del movimiento a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtiene las facturas reprogramables de un proveedor con paginación y filtro opcional.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro opcional por código de factura.</param>
        /// <returns>Listado paginado de facturas.</returns>
        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = CrearFacturaListaVacia();

                var parametros = new DynamicParameters();
                parametros.Add("Cod_Proveedor", Cod_Proveedor);

                var totalQuery = "SELECT COUNT(*) from vCxP_ProgramacionPago where cod_proveedor = @Cod_Proveedor";
                respuesta.Total = connection.QueryFirstOrDefault<int>(totalQuery, parametros);

                if (Cod_Proveedor <= 0)
                {
                    return respuesta;
                }

                var builder = new StringBuilder(@"select cod_factura, cod_proveedor, total as total_factura, fecha, tipo
                                                 from vCxP_ProgramacionPago
                                                 WHERE cxp_estado = 'G'
                                                   AND cod_proveedor = @Cod_Proveedor");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    builder.Append(" and cod_factura LIKE @Filtro");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                builder.Append(" order by cod_factura");

                if (pagina.HasValue && paginacion.HasValue)
                {
                    builder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Facturas = connection.Query<Factura>(builder.ToString(), parametros).ToList();

                foreach (Factura ft in respuesta.Facturas)
                {
                    ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
                }

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearFacturaListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener facturas.", result.Code.GetValueOrDefault(-1), CrearFacturaListaVacia());
        }

        /// <summary>
        /// Obtiene el detalle de programación de una factura específica.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Detalle de programación encontrado.</returns>
        public ErrorDto<VCxpProgramacionPago> ProgramacionDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<VCxpProgramacionPago>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT *
                  FROM vCxP_ProgramacionPago
                  WHERE cxp_estado = 'G'
                    AND Cod_Factura = @Cod_Factura
                    AND Cod_Proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            return CrearRespuestaSingle(
                result,
                "Error al obtener el detalle de programación.",
                "No se encontró detalle de programación.");
        }

        /// <summary>
        /// Obtiene los montos acumulados programados de una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Montos acumulados del pago.</returns>
        public ErrorDto<Pago> PagoMontos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<Pago>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ISNULL(SUM(Monto), 0) as Monto,
                         ISNULL(SUM(IMPORTE_DIVISA_REAL), 0) as Importe_Real
                  FROM CxP_PagoPRov
                  WHERE Tesoreria IS NULL
                    AND Cod_Factura = @Cod_Factura
                    AND Cod_Proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            return CrearRespuestaSingle(
                result,
                "Error al obtener montos del pago.",
                "No se encontraron montos del pago.");
        }

        /// <summary>
        /// Ajusta el monto de una factura mediante procedimiento almacenado y registra bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del ajuste a realizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FacturaMonto_Ajuste(int CodEmpresa, AjusteFactura data)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var procedure = "[spCxP_AjusteMontoFactura]";
                var values = new
                {
                    Proveedor = data.Cod_Proveedor,
                    Factura = data.Cod_Factura,
                    Ajuste = data.Monto_Ajuste,
                };

                var code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                return new ErrorDto
                {
                    Code = code,
                    Description = "Ok"
                };
            });

            var respuesta = result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al ajustar monto de factura.", result.Code.GetValueOrDefault(-1));

            if (respuesta.Code == 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = data.Registro_Usuario,
                    DetalleMovimiento = "Ajuste Monto Factura: " + data.Cod_Factura + " [Prov." + data.Cod_Proveedor + "] Mnt.Ant.: " + data.Monto + " -> Mnt.Nv.: " + data.Monto_Ajuste,
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 30
                });
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene el pago máximo aplicado y el monto total pagado de una factura enviada a tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Detalle acumulado de pagos.</returns>
        public ErrorDto<FacturaDet> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<FacturaDet>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT ISNULL(MAX(Npago), 0) AS Pago,
                         ISNULL(SUM(Monto), 0) AS Monto
                  FROM CxP_PagoPRov
                  WHERE Tesoreria IS NOT NULL
                    AND Cod_Factura = @Cod_Factura
                    AND Cod_Proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            return CrearRespuestaSingle(
                result,
                "Error al obtener el detalle de factura.",
                "No se encontró detalle de factura.");
        }

        /// <summary>
        /// Obtiene los cargos adicionales acumulados a partir del segundo pago de una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de cargos adicionales.</returns>
        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<CargoAdicional>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT C.Cod_Cargo,
                         C.descripcion,
                         ISNULL(SUM(Monto), 0) AS Monto
                  FROM CxP_Cargos C
                  LEFT JOIN cxp_PagoProvCargos D ON C.cod_Cargo = D.cod_Cargo
                                               AND D.cod_Proveedor = @Cod_Proveedor
                                               AND D.cod_Factura = @Cod_Factura
                                               AND D.NPago > 1
                  GROUP BY C.Cod_Cargo, C.descripcion",
                new { Cod_Factura, Cod_Proveedor });
        }

        /// <summary>
        /// Obtiene los datos de la compra asociada a una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Datos de la compra.</returns>
        public ErrorDto<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<FacturaDatos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CxP_Estado,
                         Total,
                         Imp_ventas
                  FROM CPR_COMPRAS
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            return CrearRespuestaSingle(
                result,
                "Error al obtener datos de la compra.",
                "No se encontró información de la compra.");
        }

        /// <summary>
        /// Obtiene los datos de una factura en cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Datos de la factura.</returns>
        public ErrorDto<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<FacturaDatos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CxP_Estado,
                         Total,
                         0 AS Imp_ventas
                  FROM cxp_facturas
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            return CrearRespuestaSingle(
                result,
                "Error al obtener datos de la factura.",
                "No se encontró información de la factura.");
        }

        /// <summary>
        /// Elimina cargos y pagos a partir de un número de pago específico.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Pago">Número de pago desde el cual se eliminará.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CargosPagos_Borrar(int CodEmpresa, int Pago, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    @"DELETE cxp_pagoProvCargos
                      WHERE nPago >= @Pago
                        AND cod_factura = @Cod_Factura
                        AND cod_proveedor = @Cod_Proveedor",
                    new { Pago, Cod_Factura, Cod_Proveedor });

                connection.Execute(
                    @"DELETE cxp_pagoProv
                      WHERE nPago >= @Pago
                        AND cod_factura = @Cod_Factura
                        AND cod_proveedor = @Cod_Proveedor",
                    new { Pago, Cod_Factura, Cod_Proveedor });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar cargos y pagos.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}