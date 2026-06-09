using Dapper;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmCxPControlProgramacionDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPControlProgramacionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPControlProgramacionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la programación de pagos de facturas según filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="pagina">Fila inicial de paginación.</param>
        /// <param name="paginacion">Cantidad de filas por página.</param>
        /// <param name="filtro">Filtro libre de búsqueda.</param>
        /// <param name="param">Parámetros adicionales de consulta.</param>
        /// <returns>Lista paginada de facturas programadas para pago.</returns>
        public ErrorDto<ProgramacionPagoLista> PagosFactura_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro, ConsultaPagosParam param)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new ProgramacionPagoLista
                {
                    Total = 0,
                    FacturasPago = new List<ProgramacionPagoDto>()
                };

                var parametros = CrearParametrosProgramacion(filtro, param, pagina, paginacion);
                var totalQuery = ConstruirQueryProgramacionTotal(param, filtro);
                respuesta.Total = connection.QueryFirstOrDefault<int>(totalQuery, parametros);

                var detalleQuery = ConstruirQueryProgramacionDetalle(param, filtro, pagina, paginacion);
                respuesta.FacturasPago = connection.Query<ProgramacionPagoDto>(detalleQuery, parametros).ToList();

                foreach (ProgramacionPagoDto ft in respuesta.FacturasPago)
                {
                    ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
                }

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new ProgramacionPagoLista { Total = 0, FacturasPago = new List<ProgramacionPagoDto>() })
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener pagos programados.", result.Code.GetValueOrDefault(-1), new ProgramacionPagoLista { Total = 0, FacturasPago = new List<ProgramacionPagoDto>() });
        }

        /// <summary>
        /// Obtiene el listado de cargos adicionales activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cargos adicionales.</returns>
        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CargoAdicional>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_Cargo, descripcion, 0 AS Monto FROM cxp_cargos WHERE Activo = 1");
        }

        /// <summary>
        /// Obtiene el detalle de saldos de una factura y proveedor.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Información de saldos de la factura.</returns>
        public ErrorDto<SaldosInformacion> DetalleSaldos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<SaldosInformacion>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT C.CREDITO_PLAZO,
                         dbo.fxCxPSaldoCorte(C.cod_proveedor, Getdate()) AS SALDO,
                         ISNULL(SUM(P.monto), 0) AS SaldoFactura
                  FROM CXP_PROVEEDORES C
                  LEFT JOIN cxp_pagoprov P ON C.cod_proveedor = P.cod_proveedor
                                           AND P.cod_factura = @Cod_Factura
                                           AND P.tesoreria IS NULL
                  WHERE C.cod_proveedor = @Cod_Proveedor
                  GROUP BY C.CREDITO_PLAZO, C.cod_proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<SaldosInformacion>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener el detalle de saldos.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<SaldosInformacion>
                {
                    Code = -2,
                    Description = "No se encontró información de saldos.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene la información de la compra asociada a una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Datos de la compra consultada.</returns>
        public ErrorDto<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<FacturaDatos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CxP_Estado, Total, Imp_ventas
                  FROM CPR_COMPRAS
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<FacturaDatos>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener datos de la compra.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<FacturaDatos>
                {
                    Code = -2,
                    Description = "No se encontró información de la compra.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene la información de una factura de cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Datos de la factura consultada.</returns>
        public ErrorDto<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<FacturaDatos>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT CxP_Estado,
                         Total,
                         ISNULL(impuesto_Ventas, 0) AS Imp_ventas
                  FROM cxp_facturas
                  WHERE cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                null,
                new { Cod_Factura, Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<FacturaDatos>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener datos de la factura.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<FacturaDatos>
                {
                    Code = -2,
                    Description = "No se encontró información de la factura.",
                    Result = null
                };
        }

        /// <summary>
        /// Obtiene el detalle de pagos asociados a una factura.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Listado de detalles de pago.</returns>
        public ErrorDto<List<DetallePago>> DetallePagos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            return DbHelper.ExecuteListQuery<DetallePago>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.NPago,
                         P.Cod_Factura,
                         P.Cod_Proveedor,
                         ISNULL(SUM(C.monto), 0) AS Cargo,
                         P.Monto,
                         (P.monto - ISNULL(SUM(C.monto), 0)) AS Neto,
                         ISNULL(P.Tesoreria, 0) AS Tesoreria,
                         P.fecha_Vencimiento,
                         P.importe_divisa_real,
                         P.cod_divisa,
                         P.tipo_Cambio,
                         P.forma_pago
                  FROM cxp_pagoprov P
                  LEFT JOIN cxp_pagoProvCargos C ON P.npago = C.npago
                                               AND P.cod_factura = C.cod_factura
                                               AND P.cod_proveedor = C.cod_proveedor
                  WHERE P.cod_factura = @Cod_Factura
                    AND P.cod_proveedor = @Cod_Proveedor
                  GROUP BY P.NPago,
                           P.Cod_Factura,
                           P.Cod_Proveedor,
                           P.Monto,
                           P.Tesoreria,
                           P.fecha_Vencimiento,
                           P.importe_divisa_real,
                           P.cod_divisa,
                           P.tipo_Cambio,
                           P.forma_pago
                  ORDER BY P.NPago",
                new { Cod_Factura, Cod_Proveedor });
        }

        /// <summary>
        /// Obtiene el detalle de la solicitud de tesorería asociada a un pago.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Tesoreria">Número de solicitud de tesorería.</param>
        /// <returns>Detalle de la solicitud de tesorería.</returns>
        public ErrorDto<TesoreriaDetalle> TesoreriaDetalle_Obtener(int CodEmpresa, int Tesoreria)
        {
            var result = DbHelper.ExecuteSingleQuery<TesoreriaDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT C.estado,
                         C.tipo,
                         B.descripcion,
                         C.beneficiario,
                         C.monto
                  FROM Tes_Transacciones AS C
                  INNER JOIN Tes_Bancos AS B ON C.id_banco = B.id_banco
                  WHERE C.Nsolicitud = @Tesoreria",
                null,
                new { Tesoreria });

            if (result.Code != 0)
            {
                return new ErrorDto<TesoreriaDetalle>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener detalle de tesorería.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<TesoreriaDetalle>
                {
                    Code = -2,
                    Description = "No se encontró información de tesorería.",
                    Result = null
                };
        }

        /// <summary>
        /// Actualiza los saldos del proveedor luego de registrar un pago.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Saldo">Monto a rebajar.</param>
        /// <param name="Tipo_Cambio">Tipo de cambio aplicado.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto SaldosProveedor_Actualizar(int CodEmpresa, decimal Saldo, decimal Tipo_Cambio, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"update cxp_proveedores
                     set saldo = isnull(saldo, 0) - @Saldo,
                         SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) - @Saldo / @Tipo_Cambio
                  where cod_proveedor = @Cod_Proveedor",
                new { Saldo, Tipo_Cambio, Cod_Proveedor });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar saldo del proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el estado de una factura a generada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FacturaEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"update cxp_facturas
                     set cxp_estado = 'G'
                  where cod_factura = @Cod_Factura
                    and cod_proveedor = @Cod_Proveedor",
                new { Cod_Factura, Cod_Proveedor });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el estado de la factura.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el estado de una compra a generada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CompraEstado_Actualizar(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"update CPR_COMPRAS
                     set cxp_estado = 'G'
                  where cod_factura = @Cod_Factura
                    and cod_proveedor = @Cod_Proveedor",
                new { Cod_Factura, Cod_Proveedor });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el estado de la compra.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo pago programado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del pago a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Pago_Insertar(int CodEmpresa, DetallePago data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT cxp_pagoProv(npago, cod_proveedor, cod_factura, fecha_vencimiento, monto, frecuencia, tipo_transac,
                                      apl_cargo_flotante, pago_anticipado, forma_pago, importe_divisa_real, tipo_cambio, cod_divisa)
                  VALUES(@NPago, @Cod_Proveedor, @Cod_Factura, @Fecha_Vencimiento, @Monto, @Frecuencia, @Tipo,
                         @Apl_Cargo_Flotante, @Pago_Anticipado, @Forma_Pago, @Importe_Divisa_Real, @Tipo_Cambio, @Cod_Divisa)",
                new
                {
                    data.NPago,
                    data.Cod_Proveedor,
                    data.Cod_Factura,
                    data.Fecha_Vencimiento,
                    data.Monto,
                    data.Frecuencia,
                    data.Tipo,
                    data.Apl_Cargo_Flotante,
                    data.Pago_Anticipado,
                    data.Forma_Pago,
                    data.Importe_Divisa_Real,
                    data.Tipo_Cambio,
                    data.Cod_Divisa
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro guardado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el pago.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un cargo asociado a un pago programado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del cargo a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PagoProvCargo_Insertar(int CodEmpresa, PagoProvCargo data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT cxp_PagoProvCargos(Npago, Cod_factura, cod_proveedor, cod_cargo, monto, registro_fecha,
                                            registro_usuario, cod_divisa, tipo_cambio, tipo_cargo, tipo_proceso)
                  VALUES(@NPago, @Cod_Factura, @Cod_Proveedor, @Cod_Cargo, @Monto, @Registro_Fecha,
                         @Registro_Usuario, @Cod_Divisa, @Tipo_Cambio, @Tipo_Cargo, @Tipo_Proceso)",
                new
                {
                    data.NPago,
                    data.Cod_Factura,
                    data.Cod_Proveedor,
                    data.Cod_Cargo,
                    data.Monto,
                    Registro_Fecha = DateTime.Now,
                    data.Registro_Usuario,
                    data.Cod_Divisa,
                    data.Tipo_Cambio,
                    data.Tipo_Cargo,
                    data.Tipo_Proceso
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Registro guardado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar cargo del pago.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el monto disponible neto de un pago programado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="NPago">Número de pago.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Monto neto disponible.</returns>
        public ErrorDto<Disponible> Disponible_Obtener(int CodEmpresa, int NPago, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteSingleQuery<Disponible>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT P.Npago,
                         (P.Monto - ISNULL(SUM(C.monto), 0)) AS Neto
                  FROM cxp_pagoProv P
                  LEFT JOIN cxp_pagoProvCargos C ON P.npago = C.npago
                                               AND P.cod_factura = C.cod_factura
                                               AND P.cod_proveedor = C.cod_proveedor
                  WHERE P.npago = @NPago
                    AND P.cod_factura = @Cod_Factura
                    AND P.cod_proveedor = @Cod_Proveedor
                  GROUP BY P.NPago, P.Monto",
                null,
                new { NPago, Cod_Factura, Cod_Proveedor });

            if (result.Code != 0)
            {
                return new ErrorDto<Disponible>
                {
                    Code = result.Code,
                    Description = result.Description ?? "Error al obtener el monto disponible.",
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<Disponible>
                {
                    Code = -2,
                    Description = "No se encontró información disponible.",
                    Result = null
                };
        }

        /// <summary>
        /// Actualiza pagos completados dejando la solicitud de tesorería en cero y registrando usuario y fecha.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Cod_Factura">Código de factura.</param>
        /// <param name="Cod_Proveedor">Código del proveedor.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PagoProv_Actualizar(int CodEmpresa, string Usuario, string Cod_Factura, int Cod_Proveedor)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_pagoProv
                     SET Tesoreria = 0,
                         fecha_traslada = @Fecha_Traslada,
                         user_traslada = @Usuario
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
                    Fecha_Traslada = DateTime.Now,
                    Usuario,
                    Cod_Proveedor,
                    Cod_Factura
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar pagos del proveedor.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza la fecha de vencimiento de un pago programado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del pago con la nueva fecha de vencimiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FechaVencimiento_Actualizar(int CodEmpresa, DetallePago data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE cxp_pagoprov
                     SET fecha_vencimiento = @Fecha_Vencimiento
                  WHERE npago = @NPago
                    AND cod_factura = @Cod_Factura
                    AND cod_proveedor = @Cod_Proveedor",
                new
                {
                    data.Fecha_Vencimiento,
                    data.NPago,
                    data.Cod_Factura,
                    data.Cod_Proveedor
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar la fecha de vencimiento.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea los parámetros de la consulta de programación de pagos.
        /// </summary>
        /// <param name="filtro">Filtro libre de búsqueda.</param>
        /// <param name="param">Parámetros adicionales de consulta.</param>
        /// <param name="pagina">Fila inicial de paginación.</param>
        /// <param name="paginacion">Cantidad de filas por página.</param>
        /// <returns>Parámetros listos para Dapper.</returns>
        private static DynamicParameters CrearParametrosProgramacion(string? filtro, ConsultaPagosParam param, int? pagina, int? paginacion)
        {
            var parametros = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(param.Estado))
            {
                parametros.Add("Estado", param.Estado);
            }

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                parametros.Add("Filtro", $"%{filtro.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(param.Forma_Pago) && param.Forma_Pago != "T")
            {
                parametros.Add("Forma_Pago", param.Forma_Pago);
            }

            if (pagina.HasValue && paginacion.HasValue)
            {
                parametros.Add("Offset", pagina.Value);
                parametros.Add("Fetch", paginacion.Value);
            }

            return parametros;
        }

        /// <summary>
        /// Construye la consulta de total para la programación de pagos.
        /// </summary>
        /// <param name="param">Parámetros adicionales de consulta.</param>
        /// <param name="filtro">Filtro libre de búsqueda.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ConstruirQueryProgramacionTotal(ConsultaPagosParam param, string? filtro)
        {
            var builder = new StringBuilder("SELECT COUNT(cod_factura) FROM vCxP_ProgramacionPago");
            AgregarWhereProgramacion(builder, param, filtro);
            return builder.ToString();
        }

        /// <summary>
        /// Construye la consulta de detalle para la programación de pagos.
        /// </summary>
        /// <param name="param">Parámetros adicionales de consulta.</param>
        /// <param name="filtro">Filtro libre de búsqueda.</param>
        /// <param name="pagina">Fila inicial de paginación.</param>
        /// <param name="paginacion">Cantidad de filas por página.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ConstruirQueryProgramacionDetalle(ConsultaPagosParam param, string? filtro, int? pagina, int? paginacion)
        {
            var builder = new StringBuilder(@"SELECT cod_proveedor, cod_Factura, total, CxP_Estado, fecha, tipo, fecha_ingreso,
                                                    Proveedor, forma_pago, cod_divisa, tipo_cambio, Vence, IMPORTE_DIVISA_REAL
                                             FROM vCxP_ProgramacionPago");

            AgregarWhereProgramacion(builder, param, filtro);
            builder.Append(" ORDER BY FECHA desc");

            if (pagina.HasValue && paginacion.HasValue)
            {
                builder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Agrega los filtros dinámicos permitidos al query de programación de pagos.
        /// </summary>
        /// <param name="builder">Constructor SQL.</param>
        /// <param name="param">Parámetros adicionales de consulta.</param>
        /// <param name="filtro">Filtro libre de búsqueda.</param>
        private static void AgregarWhereProgramacion(StringBuilder builder, ConsultaPagosParam param, string? filtro)
        {
            var condiciones = new List<string>();

            if (!string.IsNullOrWhiteSpace(param.Estado))
            {
                condiciones.Add("cxp_estado = @Estado");
            }

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                condiciones.Add("(cod_factura LIKE @Filtro OR cod_proveedor LIKE @Filtro OR proveedor LIKE @Filtro OR cod_divisa LIKE @Filtro)");
            }

            if (!string.IsNullOrWhiteSpace(param.Forma_Pago) && param.Forma_Pago != "T")
            {
                condiciones.Add("forma_pago = @Forma_Pago");
            }

            if (condiciones.Count > 0)
            {
                builder.Append(" WHERE ");
                builder.Append(string.Join(" AND ", condiciones));
            }
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}