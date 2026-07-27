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
        /// Crea los parámetros comunes para consultas por factura y proveedor.
        /// </summary>
        /// <param name="Cod_Factura">Código de la factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosFactura(string Cod_Factura, int Cod_Proveedor) => new
        {
            Cod_Factura,
            Cod_Proveedor
        };

        /// <summary>
        /// Asigna la llave compuesta a una colección de facturas.
        /// </summary>
        /// <param name="facturas">Listado de facturas.</param>
        private static void AsignarDataKeys(IEnumerable<Factura> facturas)
        {
            foreach (Factura ft in facturas)
            {
                ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
            }
        }

        /// <summary>
        /// Obtiene los datos de factura desde la tabla indicada usando un mismo formato de respuesta.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="query">Consulta SQL a ejecutar.</param>
        /// <param name="Cod_Factura">Código de la factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar con datos de factura.</returns>
        private ErrorDto<FacturaDatos> ObtenerFacturaDatosComun(int CodEmpresa, string query, string Cod_Factura, int Cod_Proveedor, string errorMessage, string notFoundMessage)
        {
            var result = DbHelper.ExecuteSingleQuery<FacturaDatos>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                null,
                CrearParametrosFactura(Cod_Factura, Cod_Proveedor));

            return CrearRespuestaSingle(result, errorMessage, notFoundMessage);
        }

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

                if (Cod_Proveedor <= 0)
                {
                    return respuesta;
                }

                var totalBuilder = new StringBuilder(@"SELECT COUNT(*)
                                                       FROM vCxP_ProgramacionPago
                                                       WHERE cxp_estado = 'G'
                                                         AND cod_proveedor = @Cod_Proveedor");
                var builder = new StringBuilder(@"select cod_factura, cod_proveedor, total as total_factura, fecha, tipo
                                                 from vCxP_ProgramacionPago
                                                 WHERE cxp_estado = 'G'
                                                   AND cod_proveedor = @Cod_Proveedor");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    totalBuilder.Append(" and cod_factura LIKE @Filtro");
                    builder.Append(" and cod_factura LIKE @Filtro");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                respuesta.Total = connection.QueryFirstOrDefault<int>(totalBuilder.ToString(), parametros);
                builder.Append(" order by cod_factura");

                if (pagina.HasValue && paginacion.HasValue)
                {
                    builder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
                    parametros.Add("Offset", pagina.Value);
                    parametros.Add("Fetch", paginacion.Value);
                }

                respuesta.Facturas = connection.Query<Factura>(builder.ToString(), parametros).ToList();

                AsignarDataKeys(respuesta.Facturas);

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
                CrearParametrosFactura(Cod_Factura, Cod_Proveedor));

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
                CrearParametrosFactura(Cod_Factura, Cod_Proveedor));

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
                    DetalleMovimiento = "Ajuste Monto Factura: " + data.Cod_Factura + " [Prov." + data.Cod_Proveedor + "] Mnt.Ant.: " + data.Monto + " -> Mnt.Nv.: " + (data.Monto + data.Monto_Ajuste),
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
                         ISNULL(SUM(Monto), 0) AS Monto,
                         dbo.MyGetdate() AS Fecha_Servidor
                  FROM CxP_PagoPRov
                  WHERE Tesoreria IS NOT NULL
                    AND Cod_Factura = @Cod_Factura
                    AND Cod_Proveedor = @Cod_Proveedor",
                null,
                CrearParametrosFactura(Cod_Factura, Cod_Proveedor));

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
                                               AND D.NPago > ISNULL((
                                                   SELECT MAX(P.NPago)
                                                   FROM CxP_PagoProv P
                                                   WHERE P.Tesoreria IS NOT NULL
                                                     AND P.cod_Proveedor = @Cod_Proveedor
                                                     AND P.cod_Factura = @Cod_Factura
                                               ), 0)
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
            return ObtenerFacturaDatosComun(
                CodEmpresa,
                @"SELECT CxP_Estado,
                         Total,
                         Imp_ventas
                  FROM CPR_COMPRAS
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                Cod_Factura,
                Cod_Proveedor,
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
            return ObtenerFacturaDatosComun(
                CodEmpresa,
                @"SELECT CxP_Estado,
                         Total,
                         0 AS Imp_ventas
                  FROM cxp_facturas
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                Cod_Factura,
                Cod_Proveedor,
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
                    CrearParametrosPago(Pago, Cod_Factura, Cod_Proveedor));

                connection.Execute(
                    @"DELETE cxp_pagoProv
                      WHERE nPago >= @Pago
                        AND cod_factura = @Cod_Factura
                        AND cod_proveedor = @Cod_Proveedor",
                    CrearParametrosPago(Pago, Cod_Factura, Cod_Proveedor));

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro eliminado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar cargos y pagos.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Sustituye la programación pendiente de una factura dentro de una única transacción.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Pagos y cargos de la nueva programación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Reprogramacion_Aplicar(int CodEmpresa, ReprogramacionAplicar data)
        {
            if (data.Pagos.Count == 0 || data.Cod_Proveedor <= 0 || string.IsNullOrWhiteSpace(data.Cod_Factura))
            {
                return DbHelper.ErrorResponse("La reprogramación no contiene pagos válidos.", -1);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Open();
                using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    var parametrosFactura = CrearParametrosFactura(data.Cod_Factura, data.Cod_Proveedor);
                    var totalFactura = connection.QueryFirstOrDefault<decimal?>(
                        @"SELECT Total
                          FROM vCxP_ProgramacionPago
                          WHERE CxP_Estado = 'G'
                            AND Cod_Factura = @Cod_Factura
                            AND Cod_Proveedor = @Cod_Proveedor",
                        parametrosFactura,
                        transaction);

                    if (!totalFactura.HasValue)
                    {
                        throw new InvalidOperationException("La factura no existe en la programación.");
                    }

                    var pagosTranscurridos = connection.QuerySingle<FacturaDet>(
                        @"SELECT ISNULL(MAX(NPago), 0) AS Pago,
                                 ISNULL(SUM(Monto), 0) AS Monto
                          FROM CxP_PagoProv
                          WHERE Tesoreria IS NOT NULL
                            AND Cod_Factura = @Cod_Factura
                            AND Cod_Proveedor = @Cod_Proveedor",
                        parametrosFactura,
                        transaction);

                    var siguientePago = pagosTranscurridos.Pago + 1;
                    var pagosOrdenados = data.Pagos.OrderBy(pago => pago.NPago).ToList();
                    if (pagosOrdenados[0].NPago != siguientePago ||
                        pagosOrdenados.Where((pago, index) => pago.NPago != siguientePago + index).Any() ||
                        pagosOrdenados.Any(pago => pago.Monto < 0 ||
                                                  pago.Cod_Proveedor != data.Cod_Proveedor ||
                                                  !string.Equals(pago.Cod_Factura, data.Cod_Factura, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("La numeración o los datos de los pagos no son válidos.");
                    }

                    var numerosPago = pagosOrdenados.Select(pago => pago.NPago).ToHashSet();
                    if (data.Cargos.Any(cargo => cargo.Monto < 0 ||
                                                 cargo.Cod_Proveedor != data.Cod_Proveedor ||
                                                 !numerosPago.Contains(cargo.NPago) ||
                                                 !string.Equals(cargo.Cod_Factura, data.Cod_Factura, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("Los datos de los cargos no son válidos.");
                    }

                    var saldo = totalFactura.Value - pagosTranscurridos.Monto;
                    var pendiente = saldo - pagosOrdenados.Sum(pago => pago.Monto);
                    if (pendiente >= 1)
                    {
                        throw new InvalidOperationException("Los montos distribuidos dejan un saldo pendiente.");
                    }

                    var cargosPorPago = data.Cargos
                        .GroupBy(cargo => cargo.NPago)
                        .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(cargo => cargo.Monto));
                    if (pagosOrdenados.Any(pago => pago.Monto - cargosPorPago.GetValueOrDefault(pago.NPago) < 0))
                    {
                        throw new InvalidOperationException("Los cargos son mayores que el monto de uno de los pagos.");
                    }

                    connection.Execute(
                        @"DELETE cxp_pagoProvCargos
                          WHERE nPago >= @Pago
                            AND cod_factura = @Cod_Factura
                            AND cod_proveedor = @Cod_Proveedor",
                        CrearParametrosPago(siguientePago, data.Cod_Factura, data.Cod_Proveedor),
                        transaction);

                    connection.Execute(
                        @"DELETE cxp_pagoProv
                          WHERE nPago >= @Pago
                            AND cod_factura = @Cod_Factura
                            AND cod_proveedor = @Cod_Proveedor",
                        CrearParametrosPago(siguientePago, data.Cod_Factura, data.Cod_Proveedor),
                        transaction);

                    const string insertarPago = @"INSERT cxp_pagoProv(
                            npago, cod_proveedor, cod_factura, fecha_vencimiento, monto, frecuencia,
                            tipo_transac, apl_cargo_flotante, pago_anticipado, forma_pago,
                            importe_divisa_real, tipo_cambio, cod_divisa)
                        VALUES(
                            @NPago, @Cod_Proveedor, @Cod_Factura, @Fecha_Vencimiento, @Monto, @Frecuencia,
                            @Tipo, @Apl_Cargo_Flotante, @Pago_Anticipado, @Forma_Pago,
                            @Importe_Divisa_Real, @Tipo_Cambio, @Cod_Divisa)";
                    connection.Execute(insertarPago, pagosOrdenados, transaction);

                    if (data.Cargos.Count > 0)
                    {
                        const string insertarCargo = @"INSERT cxp_PagoProvCargos(
                                Npago, Cod_factura, cod_proveedor, cod_cargo, monto, registro_fecha,
                                registro_usuario, cod_divisa, tipo_cambio, tipo_cargo, tipo_proceso)
                            VALUES(
                                @NPago, @Cod_Factura, @Cod_Proveedor, @Cod_Cargo, @Monto, dbo.MyGetdate(),
                                @Registro_Usuario, @Cod_Divisa, @Tipo_Cambio, @Tipo_Cargo, @Tipo_Proceso)";
                        connection.Execute(insertarCargo, data.Cargos, transaction);
                    }

                    connection.Execute(
                        @"UPDATE cxp_pagoProv
                             SET Tesoreria = 0,
                                 fecha_traslada = dbo.MyGetdate(),
                                 user_traslada = @Registro_Usuario
                          WHERE cod_proveedor = @Cod_Proveedor
                            AND cod_factura = @Cod_Factura
                            AND Npago IN(
                                SELECT P.npago
                                FROM cxp_pagoProv P
                                INNER JOIN cxp_PagoprovCargos C ON P.cod_proveedor = C.cod_proveedor
                                                               AND P.cod_factura = C.cod_factura
                                                               AND P.npago = C.npago
                                WHERE P.cod_proveedor = @Cod_Proveedor
                                  AND P.cod_factura = @Cod_Factura
                                GROUP BY P.npago, P.cod_proveedor, P.cod_factura, P.monto
                                HAVING P.Monto = ISNULL(SUM(C.Monto), 0))",
                        new
                        {
                            data.Registro_Usuario,
                            data.Cod_Proveedor,
                            data.Cod_Factura
                        },
                        transaction);

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Reprogramación aplicada satisfactoriamente.")
                : DbHelper.ErrorResponse(result.Description ?? "Error al aplicar la reprogramación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea los parámetros comunes para operaciones por pago, factura y proveedor.
        /// </summary>
        /// <param name="Pago">Número de pago.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosPago(int Pago, string Cod_Factura, int Cod_Proveedor) => new
        {
            Pago,
            Cod_Factura,
            Cod_Proveedor
        };

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
