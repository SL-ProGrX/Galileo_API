using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public partial class FrmCxcCuentasDB
    {
        #region Facturas

        private static void AsignarTotalesFacturas<TItem>(
           CxCCuentasFacturasListaBase<TItem> result,
           List<TItem> lista,
           Func<TItem, decimal> selectorMonto,
           Func<TItem, decimal> selectorAdelanto)
        {
            result.lista = lista;
            result.casos = lista.Count;
            result.total = lista.Sum(selectorMonto);
            result.adelanto = lista.Sum(selectorAdelanto);
        }

        private static ErrorDto<CxCCuentasFacturaMantenimientoResult> CrearErrorFacturaMantenimiento(string mensaje)
        {
            return new ErrorDto<CxCCuentasFacturaMantenimientoResult>
            {
                Code = -1,
                Description = mensaje,
                Result = new CxCCuentasFacturaMantenimientoResult()
            };
        }

        private static string? ValidarOperacionFactura(long operacion, string estado, string autorizaEstado)
        {
            if (operacion <= 0)
            {
                return CxCCuentasConstantes.operacionRequerida;
            }

            if (estado is "A" or "D")
            {
                return "La operación no está pendiente o recibida, no pueden realizarse los cambios.";
            }

            if (autorizaEstado != "P")
            {
                return "La operación ya fue autorizada o denegada.";
            }

            return null;
        }

        private static string? ValidarRegistroFactura(
            CxCCuentasFacturaRegistraRequest request,
            string factura,
            string divisa,
            string facturaEstado,
            string adelantoTipo,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(factura) || request.importe <= 0)
            {
                return "El número de factura o el importe no es válido.";
            }

            if (string.IsNullOrWhiteSpace(divisa) ||
                string.IsNullOrWhiteSpace(facturaEstado) ||
                string.IsNullOrWhiteSpace(usuario))
            {
                return "Faltan datos requeridos para registrar la factura.";
            }

            if (request.tipo_cambio <= 0 || request.monto <= 0)
            {
                return "El tipo de cambio o el monto no es válido.";
            }

            if (adelantoTipo is not ("P" or "M"))
            {
                return "El tipo de adelanto no es válido.";
            }

            if (request.fecha_emision is null || request.fecha_pago is null)
            {
                return "Las fechas de emisión y pago son requeridas.";
            }

            return null;
        }

        private static string? ValidarEliminacionFactura(string factura, string usuario)
        {
            if (string.IsNullOrWhiteSpace(factura))
            {
                return "La factura es requerida.";
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CxCCuentasConstantes.usuarioRequerido;
            }

            return null;
        }

        private static string? ValidarVinculacionFactura(CxCCuentasFacturaVincularRequest request, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CxCCuentasConstantes.usuarioRequerido;
            }

            if (request.facturas is null || request.facturas.Count == 0)
            {
                return "Debe seleccionar al menos una factura.";
            }

            return null;
        }

        private static string? ValidarItemFacturaVincular(
            CxCCuentasFacturaVincularItem item,
            string factura,
            string divisa)
        {
            if (string.IsNullOrWhiteSpace(factura) || string.IsNullOrWhiteSpace(divisa))
            {
                return "Hay facturas seleccionadas con datos incompletos.";
            }

            if (item.importe <= 0 || item.tipo_cambio <= 0 || item.monto <= 0)
            {
                return "Hay facturas seleccionadas con importes inválidos.";
            }

            return null;
        }

        private ErrorDto<CxCCuentasFacturaMantenimientoResult> EjecutarFacturaMantenimiento(
            int codEmpresa,
            string mensajeGeneral,
            Func<SqlConnection, ErrorDto<CxCCuentasFacturaMantenimientoResult>> accion)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                return accion(conn);
            }
            catch (Exception ex)
            {
                return CrearErrorFacturaMantenimiento($"{mensajeGeneral} {ex.Message}");
            }
        }

        private static bool FacturaDisponibleParaOperacion(SqlConnection conn, long operacion, string factura)
        {
            const string sql = @"
                SELECT dbo.fxCxC_FacturaValida(@operacion, @factura) AS pass;";

            var pass = conn.QueryFirstOrDefault<int>(sql, new
            {
                operacion,
                factura
            });

            return pass != 0;
        }

        private static ErrorDto<CxCCuentasFacturaMantenimientoResult> EjecutarConsultaFacturaMantenimiento(
            SqlConnection conn,
            string sql,
            object parametros)
        {
            var data = conn.QueryFirstOrDefault<CxCCuentasFacturaMantenimientoResult>(sql, parametros);
            return DbHelper.CreateOkResponse(data ?? new CxCCuentasFacturaMantenimientoResult());
        }

        /// <summary>
        /// Obtiene las facturas registradas de una operación de CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="operacion">Número de operación.</param>
        /// <returns>Lista de facturas y totales.</returns>
        public ErrorDto<CxCCuentasFacturasLista> CxCCuentasFacturas_Obtener(int codEmpresa, long operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasLista>(CxCCuentasConstantes.operacionRequerida);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"exec spCxC_Operacion_Facturas @Operacion, 0;";
                var lista = conn.Query<CxCCuentasFacturasData>(sql, new { Operacion = operacion }).ToList();

                foreach (var item in lista)
                {
                    item.adelanto_tipo_desc = (item.adelanto_tipo ?? string.Empty).Trim().ToUpperInvariant() == "P"
                        ? "Porcentual"
                        : "Monto";
                }

                var response = DbHelper.CreateOkResponse(new CxCCuentasFacturasLista());
                AsignarTotalesFacturas(response.Result, lista, x => x.monto, x => x.adelanto_monto);
                return response;
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasLista>($"No fue posible consultar las facturas de la operación. {ex.Message}");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasLista>($"Error inesperado al consultar las facturas de la operación. {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene las facturas adelantadas pendientes para una cédula y pagador.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="cedulaPagador">Cédula del pagador.</param>
        /// <returns>Lista de facturas adelantadas y totales.</returns>
        public ErrorDto<CxCCuentasFacturasAdelantadasLista> CxCCuentasFacturasAdelantadas_Obtener(
            int codEmpresa,
            string cedula,
            string cedulaPagador)
        {
            var cedulaNormalizada = NormalizarTexto(cedula);

            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasAdelantadasLista>("La cédula es requerida.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"exec spCxC_Facturas_Adelantadas_Pendientes @Cedula, @Pagador;";
                var lista = conn.Query<CxCCuentasFacturasAdelantadasData>(sql, new
                {
                    Cedula = cedulaNormalizada,
                    Pagador = NormalizarTexto(cedulaPagador)
                }).ToList();

                var response = DbHelper.CreateOkResponse(new CxCCuentasFacturasAdelantadasLista());
                AsignarTotalesFacturas(response.Result, lista, x => x.monto, x => x.adelanto_monto);
                return response;
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasAdelantadasLista>($"No fue posible consultar las facturas adelantadas. {ex.Message}");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasFacturasAdelantadasLista>($"Error inesperado al consultar las facturas adelantadas. {ex.Message}");
            }
        }

        /// <summary>
        /// Registra una factura en la operación de CxC y recalcula sus totales.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos de la factura a registrar.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Registra(
            int codEmpresa,
            CxCCuentasFacturaRegistraRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var factura = NormalizarTexto(request.factura);
            var divisa = NormalizarTexto(request.divisa);
            var facturaEstado = NormalizarMayusculas(request.factura_estado);
            var adelantoTipo = NormalizarMayusculas(request.adelanto_tipo);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarRegistroFactura(request, factura, divisa, facturaEstado, adelantoTipo, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            if (!request.fecha_emision.HasValue || !request.fecha_pago.HasValue)
            {
                return CrearErrorFacturaMantenimiento("Las fechas de emisión y pago son requeridas.");
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible registrar la factura.",
                conn =>
                {
                    if (!FacturaDisponibleParaOperacion(conn, request.operacion, factura))
                    {
                        return CrearErrorFacturaMantenimiento("Esta factura ya ha sido utilizada anteriormente con este cliente.");
                    }

                    const string sqlRegistra = @"
                        exec spCxC_Operacion_Factura_Registra
                            @operacion,
                            @factura,
                            @divisa,
                            @factura_estado,
                            @importe,
                            @tipo_cambio,
                            @monto,
                            @adelanta,
                            @adelanto_tipo,
                            @adelanto,
                            @fecha_emision,
                            @fecha_pago,
                            @usuario,
                            'I',
                            1,
                            1,
                            0;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sqlRegistra, new
                    {
                        operacion = request.operacion,
                        factura,
                        divisa,
                        factura_estado = facturaEstado,
                        importe = request.importe,
                        tipo_cambio = request.tipo_cambio,
                        monto = request.monto,
                        adelanta = request.adelanta ? 1 : 0,
                        adelanto_tipo = adelantoTipo,
                        adelanto = request.adelanta ? request.adelanto : 0,
                        fecha_emision = request.fecha_emision.Value.ToString(CxCCuentasConstantes.fechaFormat),
                        fecha_pago = request.fecha_pago.Value.ToString(CxCCuentasConstantes.fechaFormat),
                        usuario
                    });
                });
        }

        /// <summary>
        /// Elimina una factura de la operación de CxC y recalcula sus totales.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos mínimos de la factura a eliminar.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Elimina(
            int codEmpresa,
            CxCCuentasFacturaEliminaRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var factura = NormalizarTexto(request.factura);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarEliminacionFactura(factura, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible eliminar la factura.",
                conn =>
                {
                    const string sql = @"
                        exec spCxC_Operacion_Factura_Registra
                            @operacion,
                            @factura,
                            '',
                            '',
                            0,
                            0,
                            0,
                            0,
                            'M',
                            0,
                            null,
                            null,
                            @usuario,
                            'E',
                            1,
                            1,
                            0;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sql, new
                    {
                        operacion = request.operacion,
                        factura,
                        usuario
                    });
                });
        }

        /// <summary>
        /// Vincula facturas adelantadas a una operación de CxC y recalcula sus totales.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos de las facturas a vincular.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_Vincular(
            int codEmpresa,
            CxCCuentasFacturaVincularRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarVinculacionFactura(request, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible vincular las facturas.",
                conn =>
                {
                    const string sqlRegistra = @"
                        exec spCxC_Operacion_Factura_Registra
                            @operacion,
                            @factura,
                            @divisa,
                            'T',
                            @importe,
                            @tipo_cambio,
                            @monto,
                            0,
                            'M',
                            @adelanto,
                            @fecha_emision,
                            @fecha_pago,
                            @usuario,
                            'I',
                            1,
                            0,
                            @operacion_origen;";

                    foreach (var item in request.facturas)
                    {
                        var factura = NormalizarTexto(item.factura);
                        var divisa = NormalizarTexto(item.divisa);

                        var mensajeItem = ValidarItemFacturaVincular(item, factura, divisa);
                        if (!string.IsNullOrWhiteSpace(mensajeItem))
                        {
                            return CrearErrorFacturaMantenimiento(mensajeItem);
                        }

                        conn.Execute(sqlRegistra, new
                        {
                            operacion = request.operacion,
                            factura,
                            divisa,
                            importe = item.importe,
                            tipo_cambio = item.tipo_cambio,
                            monto = item.monto,
                            adelanto = item.adelanto,
                            fecha_emision = item.fecha_emision?.ToString(CxCCuentasConstantes.fechaFormat),
                            fecha_pago = item.fecha_pago?.ToString(CxCCuentasConstantes.fechaFormat),
                            usuario,
                            operacion_origen = item.operacion_origen
                        });
                    }

                    const string sqlActualiza = @"
                        exec spCxC_Operacion_Facturas_Actualiza @operacion, 1, @usuario;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sqlActualiza, new
                    {
                        operacion = request.operacion,
                        usuario
                    });
                });
        }

        private sealed class CxCCuentasFacturaCargaFilaNormalizada
        {
            public string factura { get; init; } = string.Empty;
            public string divisa { get; init; } = string.Empty;
            public string factura_estado { get; init; } = string.Empty;
            public decimal importe { get; init; }
            public decimal tipo_cambio { get; init; }
            public decimal monto { get; init; }
            public int adelanta { get; init; }
            public string adelanto_tipo { get; init; } = "M";
            public decimal adelanto_monto { get; init; }
            public string fecha_emite { get; init; } = string.Empty;
            public string fecha_pago { get; init; } = string.Empty;
        }

        private static string? ValidarCargaArchivoFactura(
            CxCCuentasFacturaCargaRequest request,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return CxCCuentasConstantes.usuarioRequerido;
            }

            if (request.facturas is null || request.facturas.Count == 0)
            {
                return "Debe seleccionar al menos una factura.";
            }

            return null;
        }

        private static string? ValidarFilaFacturaCarga(
            CxCCuentasFacturaCargaItem item,
            string factura,
            string divisa)
        {
            if (string.IsNullOrWhiteSpace(factura) || string.IsNullOrWhiteSpace(divisa))
            {
                return "Hay filas del archivo con datos incompletos.";
            }

            if (item.importe <= 0 || item.tipo_cambio <= 0)
            {
                return "Hay filas del archivo con importes inválidos.";
            }

            return null;
        }

        private static CxCCuentasFacturaCargaFilaNormalizada NormalizarFilaFacturaCarga(CxCCuentasFacturaCargaItem item)
        {
            var factura = NormalizarTexto(item.factura);
            var divisa = NormalizarTexto(item.divisa);
            var facturaEstado = NormalizarMayusculas(item.estado);
            var adelantoBase = facturaEstado == "A" ? item.monto : 0;
            var adelantoMonto = item.adelanto > 0 ? item.adelanto : adelantoBase;
            var monto = item.importe * item.tipo_cambio;

            if (adelantoMonto > monto)
            {
                adelantoMonto = monto;
            }

            var adelanta = adelantoMonto > 0 || facturaEstado == "A";
            var adelantoTipo = facturaEstado == "A" && adelantoMonto == 0 ? "P" : "M";

            return new CxCCuentasFacturaCargaFilaNormalizada
            {
                factura = factura,
                divisa = divisa,
                factura_estado = facturaEstado,
                importe = item.importe,
                tipo_cambio = item.tipo_cambio,
                monto = monto,
                adelanta = adelanta ? 1 : 0,
                adelanto_tipo = adelantoTipo,
                adelanto_monto = adelantoMonto,
                fecha_emite = item.fecha_emite,
                fecha_pago = item.fecha_pago
            };
        }

        private static void RegistrarFilaFacturaCarga(
            SqlConnection conn,
            long operacion,
            string usuario,
            CxCCuentasFacturaCargaFilaNormalizada fila)
        {
            const string sqlRegistra = @"
                exec spCxC_Operacion_Factura_Registra
                    @Operacion,
                    @Factura,
                    @Divisa,
                    @Estado,
                    @Importe,
                    @TipoCambio,
                    @Monto,
                    @Adelanta,
                    @AdelantaTipo,
                    @AdelantaMonto,
                    @FechaEmite,
                    @FechaPago,
                    @Usuario,
                    'I',
                    0,
                    0,
                    0;";

            conn.Execute(sqlRegistra, new
            {
                Operacion = operacion,
                Factura = fila.factura,
                Divisa = fila.divisa,
                Estado = fila.factura_estado,
                Importe = fila.importe,
                TipoCambio = fila.tipo_cambio,
                Monto = fila.monto,
                Adelanta = fila.adelanta,
                AdelantaTipo = fila.adelanto_tipo,
                AdelantaMonto = fila.adelanto_monto,
                FechaEmite = fila.fecha_emite,
                FechaPago = fila.fecha_pago,
                Usuario = usuario
            });
        }

        /// <summary>
        /// Procesa un lote de facturas leído desde archivo y recalcula los totales de la operación.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos del lote de facturas.</param>
        /// <returns>Totales actualizados de la operación.</returns>
        public ErrorDto<CxCCuentasFacturaMantenimientoResult> CxCCuentasFactura_CargarArchivo(
            int codEmpresa,
            CxCCuentasFacturaCargaRequest request)
        {
            if (request is null)
            {
                return CrearErrorFacturaMantenimiento(CxCCuentasConstantes.solicitudRequerida);
            }

            var estado = NormalizarMayusculas(request.estado);
            var autorizaEstado = NormalizarMayusculas(request.autoriza_estado);
            var usuario = NormalizarTexto(request.usuario);

            var mensajeValidacion = ValidarOperacionFactura(request.operacion, estado, autorizaEstado)
                ?? ValidarCargaArchivoFactura(request, usuario);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                return CrearErrorFacturaMantenimiento(mensajeValidacion);
            }

            return EjecutarFacturaMantenimiento(
                codEmpresa,
                "No fue posible cargar el archivo de facturas.",
                conn =>
                {
                    foreach (var item in request.facturas)
                    {
                        var factura = NormalizarTexto(item.factura);
                        var divisa = NormalizarTexto(item.divisa);
                        var mensajeFila = ValidarFilaFacturaCarga(item, factura, divisa);

                        if (!string.IsNullOrWhiteSpace(mensajeFila))
                        {
                            return CrearErrorFacturaMantenimiento(mensajeFila);
                        }

                        var fila = NormalizarFilaFacturaCarga(item);
                        RegistrarFilaFacturaCarga(conn, request.operacion, usuario, fila);
                    }

                    const string sqlActualiza = @"
                        exec spCxC_Operacion_Facturas_Actualiza @operacion, 1;";

                    return EjecutarConsultaFacturaMantenimiento(conn, sqlActualiza, new
                    {
                        operacion = request.operacion
                    });
                });
        }

        #endregion
    }
}
