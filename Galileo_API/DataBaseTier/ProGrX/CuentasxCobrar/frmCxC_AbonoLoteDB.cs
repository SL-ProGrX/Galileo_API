using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Security;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public sealed class FrmCxCAbonoLoteDb
    {
        private const int ModuloCuentasPorCobrar = 31;
        private const string ConceptoAbonoOrdinario = "CRD001";
        private const string MovimientoHaber = "H";
        private const string MovimientoBitacora = "Registra - WEB";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MRecibos _mRecibos;

        /// <summary>
        /// Inicializa el acceso a datos del formulario con sus dependencias.
        /// </summary>
        /// <param name="config">Configuración de conexiones y servicios.</param>
        public FrmCxCAbonoLoteDb(IConfiguration config)
            : this(
                new PortalDB(config),
                new MSecurityMainDb(config),
                new MRecibos(config))
        {
        }

        /// <summary>
        /// Inicializa el acceso a datos con dependencias proporcionadas.
        /// </summary>
        /// <param name="portalDb">Acceso común a consultas del portal.</param>
        /// <param name="securityMainDb">Acceso a seguridad y bitácora.</param>
        /// <param name="mRecibos">Servicio de generación de recibos.</param>
        public FrmCxCAbonoLoteDb(
            PortalDB portalDb,
            MSecurityMainDb securityMainDb,
            MRecibos mRecibos)
        {
            _portalDb = portalDb;
            _securityMainDb = securityMainDb;
            _mRecibos = mRecibos;
        }

        /// <summary>
        /// Obtiene los clientes que poseen cuentas pendientes.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Clientes con cuentas pendientes.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_Clientes_Obtener(int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(Per.Cedula) AS item,
                    RTRIM(Per.Nombre) AS descripcion
                FROM vCxC_Cuentas_Pendientes_Rsm Ft
                INNER JOIN CxC_Personas Per
                    ON Ft.Cedula = Per.Cedula
                GROUP BY
                    Per.Cedula,
                    Per.Nombre
                ORDER BY
                    Per.Nombre,
                    Per.Cedula;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el derecho configurado para aplicar abonos por lote.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario de la sesión.</param>
        /// <returns>Valor del derecho configurado.</returns>
        public int CxC_AbonoLote_Derecho_Aplicar_Obtener(
            int codEmpresa,
            string usuario)
        {
            return _securityMainDb.Derecho(new ParametrosAccesoDto
            {
                EmpresaId = codEmpresa,
                Usuario = CxC_AbonoLote_Texto_Normalizar(usuario),
                Modulo = ModuloCuentasPorCobrar,
                FormName = "frmCxC_AbonoLote",
                Boton = "Aplicar"
            });
        }

        /// <summary>
        /// Obtiene las divisas disponibles para las cuentas pendientes
        /// de un cliente.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cedula">Identificación del cliente.</param>
        /// <returns>Divisas disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_Divisas_Obtener(
                int codEmpresa,
                string cedula)
        {
            const string sql = """
                SELECT
                    RTRIM(Cod_Divisa) AS item,
                    RTRIM(Cod_Divisa) AS descripcion
                FROM vCxC_Cuentas_Pendientes_Rsm
                WHERE Cedula = @cedula
                GROUP BY Cod_Divisa
                ORDER BY Cod_Divisa;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cedula = CxC_AbonoLote_Texto_Normalizar(cedula)
                });
        }

        /// <summary>
        /// Obtiene las facturas pendientes de un cliente y una divisa.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de consulta.</param>
        /// <returns>Facturas pendientes.</returns>
        public ErrorDto<List<CxCFactPendienteCancelacionData>>
            CxC_AbonoLote_Facturas_Obtener(
                int codEmpresa,
                CxCAbonoLoteFacturasRequest request)
        {
            var error = CxC_AbonoLote_FacturasFiltro_Validar(request);

            if (!string.IsNullOrWhiteSpace(error))
            {
                return DbHelper.CreateErrorResponse(
                    error,
                    -2,
                    new List<CxCFactPendienteCancelacionData>());
            }

            const string sql = """
                SELECT
                    Operacion AS operacion,
                    RTRIM(Cod_Factura) AS cod_factura,
                    Monto AS monto,
                    RTRIM(Cod_Divisa) AS cod_divisa,
                    Fecha_Pago AS fecha_pago,
                    Importe AS importe,
                    Fecha_Emision AS fecha_emision,
                    Activa_Fecha AS activa_fecha,
                    RTRIM(Cedula) AS cedula,
                    RTRIM(Nombre) AS nombre
                FROM vCxC_Cuentas_Pendientes_Detalle
                WHERE Cedula = @cedula
                  AND Cod_Divisa = @codDivisa
                  AND (
                        @codFactura = ''
                        OR Cod_Factura LIKE '%' + @codFactura + '%'
                  )
                ORDER BY
                    Nombre,
                    Cod_Factura,
                    Fecha_Pago;
                """;

            return DbHelper.ExecuteListQuery<CxCFactPendienteCancelacionData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cedula = CxC_AbonoLote_Texto_Normalizar(request.cedula),
                    codDivisa = CxC_AbonoLote_Texto_Normalizar(
                        request.cod_divisa),
                    codFactura = CxC_AbonoLote_Texto_Normalizar(
                        request.cod_factura)
                });
        }

        /// <summary>
        /// Obtiene los tipos de documento autorizados para la caja.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="caja">Código de caja.</param>
        /// <returns>Tipos de documento disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_TiposDocumento_Obtener(
                int codEmpresa,
                string caja)
        {
            const string sql = """
                SELECT
                    RTRIM(C.tipo_documento) AS item,
                    RTRIM(D.Descripcion) AS descripcion
                FROM SIF_DOCUMENTOS D
                INNER JOIN CAJAS_DOCUMENTOS C
                    ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                WHERE C.cod_caja = @caja
                  AND D.Tipo_Movimiento IN ('A', 'C')
                ORDER BY C.tipo_documento;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    caja = CxC_AbonoLote_Texto_Normalizar(caja)
                });
        }

        /// <summary>
        /// Aplica los abonos ordinarios de las facturas seleccionadas y
        /// genera el documento y asiento correspondiente al lote.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Información del lote.</param>
        /// <returns>Documento generado y total aplicado.</returns>
        public ErrorDto<CxCAbonoLoteAplicarData>
            CxC_AbonoLote_Aplicar(
                int codEmpresa,
                CxCAbonoLoteAplicarRequest request)
        {
            var validacion = CxC_AbonoLote_Aplicacion_Validar(request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return CxC_AbonoLote_Error_Crear(validacion, -2);
            }

            if (CxC_AbonoLote_Derecho_Aplicar_Obtener(
                    codEmpresa,
                    request.usuario) <= 0)
            {
                return CxC_AbonoLote_Error_Crear(
                    "El usuario no tiene permiso para aplicar abonos por lote.",
                    -2);
            }

            try
            {
                var tipoDocumento = CxC_AbonoLote_Texto_Normalizar(
                    request.tipo_documento);

                var numeroDocumento = _mRecibos
                    .FxDocumentoConsecutivo(codEmpresa, tipoDocumento)
                    .ToString();

                if (string.IsNullOrWhiteSpace(numeroDocumento)
                    || numeroDocumento == "0")
                {
                    return CxC_AbonoLote_Error_Crear(
                        "No fue posible obtener el consecutivo del documento.",
                        -2);
                }

                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                connection.Open();

                var resultado = CxC_AbonoLote_Transaccion_Ejecutar(
                    connection,
                    request,
                    tipoDocumento,
                    numeroDocumento);

                CxC_AbonoLote_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario,
                    request.cedula);

                return DbHelper.CreateOkResponse(
                    resultado,
                    $"Abono aplicado con el documento {numeroDocumento}.");
            }
            catch (Exception ex) when (
                ex is DbException
                or DataException
                or InvalidOperationException
                or SecurityException
                or ArgumentException
                or OverflowException)
            {
                return CxC_AbonoLote_Error_Crear(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta el lote completo dentro de una única transacción SQL.
        /// </summary>
        /// <param name="connection">Conexión abierta a la empresa.</param>
        /// <param name="request">Datos del lote que se debe aplicar.</param>
        /// <param name="tipoDocumento">Tipo del documento generado.</param>
        /// <param name="numeroDocumento">Consecutivo del documento.</param>
        /// <returns>Resultado consolidado del lote aplicado.</returns>
        private static CxCAbonoLoteAplicarData
            CxC_AbonoLote_Transaccion_Ejecutar(
                SqlConnection connection,
                CxCAbonoLoteAplicarRequest request,
                string tipoDocumento,
                string numeroDocumento)
        {
            using var transaction = connection.BeginTransaction();

            var fechaServidor = connection.QuerySingle<DateTime>(
                "SELECT GETDATE();",
                transaction: transaction);

            foreach (var factura in request.facturas)
            {
                CxC_AbonoLote_FacturaActual_Validar(
                    connection,
                    transaction,
                    request,
                    factura);
            }

            foreach (var factura in request.facturas)
            {
                CxC_AbonoLote_AbonoOrdinario_Ejecutar(
                    connection,
                    transaction,
                    request,
                    factura,
                    tipoDocumento,
                    numeroDocumento,
                    fechaServidor);
            }

            CxC_AbonoLote_Asiento_Ejecutar(
                connection,
                transaction,
                request,
                tipoDocumento,
                numeroDocumento);

            transaction.Commit();

            return new CxCAbonoLoteAplicarData
            {
                tipo_documento = tipoDocumento,
                numero_documento = numeroDocumento,
                total_aplicado = CxC_AbonoLote_Monto_Redondear(
                    request.facturas.Sum(factura => factura.monto)),
                cantidad_facturas = request.facturas.Count
            };
        }

        /// <summary>
        /// Confirma que una factura siga pendiente y conserve su monto.
        /// </summary>
        /// <param name="connection">Conexión utilizada por el lote.</param>
        /// <param name="transaction">Transacción activa del lote.</param>
        /// <param name="request">Datos generales del lote.</param>
        /// <param name="factura">Factura que se debe validar.</param>
        private static void CxC_AbonoLote_FacturaActual_Validar(
            SqlConnection connection,
            SqlTransaction transaction,
            CxCAbonoLoteAplicarRequest request,
            CxCAbonoLoteDetalleRequest factura)
        {
            const string sql = """
                SELECT TOP (1)
                    Operacion AS operacion,
                    RTRIM(Cod_Factura) AS cod_factura,
                    Monto AS monto
                FROM vCxC_Cuentas_Pendientes_Detalle
                WHERE Operacion = @operacion
                  AND Cod_Factura = @codFactura
                  AND Cedula = @cedula
                  AND Cod_Divisa = @codDivisa;
                """;

            var facturaActual =
                connection.QueryFirstOrDefault<CxCAbonoLoteFacturaActualData>(
                    sql,
                    new
                    {
                        operacion = factura.operacion,
                        codFactura = CxC_AbonoLote_Texto_Normalizar(
                            factura.cod_factura),
                        cedula = CxC_AbonoLote_Texto_Normalizar(
                            request.cedula),
                        codDivisa = CxC_AbonoLote_Texto_Normalizar(
                            request.cod_divisa)
                    },
                    transaction);

            if (facturaActual is null)
            {
                throw new InvalidOperationException(
                    $"La factura {factura.cod_factura} ya no se encuentra pendiente.");
            }

            if (CxC_AbonoLote_Monto_Redondear(facturaActual.monto)
                != CxC_AbonoLote_Monto_Redondear(factura.monto))
            {
                throw new InvalidOperationException(
                    $"El monto pendiente de la factura "
                    + $"{facturaActual.cod_factura} fue modificado. "
                    + "Actualice la lista antes de aplicar el abono.");
            }
        }

        /// <summary>
        /// Ejecuta el abono ordinario de una factura seleccionada.
        /// </summary>
        /// <param name="connection">Conexión utilizada por el lote.</param>
        /// <param name="transaction">Transacción activa del lote.</param>
        /// <param name="request">Datos generales del lote.</param>
        /// <param name="factura">Factura que se debe abonar.</param>
        /// <param name="tipoDocumento">Tipo del documento generado.</param>
        /// <param name="numeroDocumento">Consecutivo del documento.</param>
        /// <param name="fechaServidor">Fecha transaccional del servidor.</param>
        private static void CxC_AbonoLote_AbonoOrdinario_Ejecutar(
            SqlConnection connection,
            SqlTransaction transaction,
            CxCAbonoLoteAplicarRequest request,
            CxCAbonoLoteDetalleRequest factura,
            string tipoDocumento,
            string numeroDocumento,
            DateTime fechaServidor)
        {
            const string sql = """
                EXEC spCxC_AbonoOrdinario
                    @operacion,
                    @concepto,
                    @usuario,
                    @tipoDocumento,
                    @numeroDocumento,
                    @abono,
                    @fecha,
                    @caja,
                    @movimiento,
                    @reversa;
                """;

            connection.Execute(
                sql,
                new
                {
                    operacion = factura.operacion,
                    concepto = ConceptoAbonoOrdinario,
                    usuario = CxC_AbonoLote_Texto_Normalizar(
                        request.usuario),
                    tipoDocumento,
                    numeroDocumento,
                    abono = CxC_AbonoLote_Monto_Redondear(factura.monto),
                    fecha = fechaServidor,
                    caja = CxC_AbonoLote_Texto_Normalizar(request.caja),
                    movimiento = MovimientoHaber,
                    reversa = 0
                },
                transaction);
        }

        /// <summary>
        /// Genera el asiento contable correspondiente al lote aplicado.
        /// </summary>
        /// <param name="connection">Conexión utilizada por el lote.</param>
        /// <param name="transaction">Transacción activa del lote.</param>
        /// <param name="request">Datos generales del lote.</param>
        /// <param name="tipoDocumento">Tipo del documento generado.</param>
        /// <param name="numeroDocumento">Consecutivo del documento.</param>
        private static void CxC_AbonoLote_Asiento_Ejecutar(
            SqlConnection connection,
            SqlTransaction transaction,
            CxCAbonoLoteAplicarRequest request,
            string tipoDocumento,
            string numeroDocumento)
        {
            const string sql = """
                EXEC spCxC_Abonos_Lote_Asiento
                    @tipoDocumento,
                    @numeroDocumento,
                    @caja,
                    @apertura,
                    @tiquete,
                    @usuario,
                    @sesionId;
                """;

            connection.Execute(
                sql,
                new
                {
                    tipoDocumento,
                    numeroDocumento,
                    caja = CxC_AbonoLote_Texto_Normalizar(request.caja),
                    request.apertura,
                    tiquete = CxC_AbonoLote_Texto_Normalizar(
                        request.tiquete),
                    usuario = CxC_AbonoLote_Texto_Normalizar(
                        request.usuario),
                    sesionId = request.sesion_id
                },
                transaction);
        }

        /// <summary>
        /// Registra en bitácora la cancelación de cuentas del cliente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que aplicó el lote.</param>
        /// <param name="cedula">Identificación del cliente.</param>
        private void CxC_AbonoLote_Bitacora_Registrar(
            int codEmpresa,
            string usuario,
            string cedula)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = CxC_AbonoLote_Texto_Normalizar(usuario)
                    .ToUpperInvariant(),
                Movimiento = MovimientoBitacora,
                Modulo = ModuloCuentasPorCobrar,
                DetalleMovimiento =
                    "Registra cancelación de cuentas > Cliente Id: "
                    + CxC_AbonoLote_Texto_Normalizar(cedula)
            });
        }

        /// <summary>
        /// Valida los filtros requeridos para consultar facturas.
        /// </summary>
        /// <param name="request">Filtros enviados por la pantalla.</param>
        /// <returns>Mensaje de validación o texto vacío.</returns>
        private static string
            CxC_AbonoLote_FacturasFiltro_Validar(
                CxCAbonoLoteFacturasRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "La identificación del cliente es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_divisa))
            {
                return "La divisa es requerida.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Valida los datos y totales requeridos para aplicar el lote.
        /// </summary>
        /// <param name="request">Datos del lote que se debe validar.</param>
        /// <returns>Mensaje de validación o texto vacío.</returns>
        private static string CxC_AbonoLote_Aplicacion_Validar(
            CxCAbonoLoteAplicarRequest request)
        {
            var camposRequeridos = new[]
            {
                (
                    valor: request.caja,
                    mensaje: "El código de caja es requerido."
                ),
                (
                    valor: request.tiquete,
                    mensaje: "El tiquete de caja es requerido."
                ),
                (
                    valor: request.tipo_documento,
                    mensaje: "El tipo de documento es requerido."
                ),
                (
                    valor: request.usuario,
                    mensaje: "El usuario es requerido."
                ),
                (
                    valor: request.cedula,
                    mensaje: "La identificación del cliente es requerida."
                ),
                (
                    valor: request.cod_divisa,
                    mensaje: "La divisa es requerida."
                )
            };

            var campoInvalido = camposRequeridos.FirstOrDefault(
                campo => string.IsNullOrWhiteSpace(campo.valor));

            if (!string.IsNullOrWhiteSpace(campoInvalido.mensaje))
            {
                return campoInvalido.mensaje;
            }

            if (request.apertura <= 0)
            {
                return "La apertura de caja no es válida.";
            }

            if (request.sesion_id < 0)
            {
                return "La sesión de caja no es válida.";
            }

            if (request.facturas.Count == 0)
            {
                return "Debe seleccionar al menos una factura.";
            }

            var facturaInvalida = request.facturas.FirstOrDefault(
                factura =>
                    factura.operacion <= 0
                    || string.IsNullOrWhiteSpace(factura.cod_factura)
                    || factura.monto <= 0);

            if (facturaInvalida is not null)
            {
                return "Una de las facturas seleccionadas no es válida.";
            }

            var existeDuplicada = request.facturas
                .GroupBy(
                    factura =>
                        $"{factura.operacion}|"
                        + CxC_AbonoLote_Texto_Normalizar(
                            factura.cod_factura),
                    StringComparer.OrdinalIgnoreCase)
                .Any(grupo => grupo.Count() > 1);

            if (existeDuplicada)
            {
                return "Existen facturas duplicadas en la selección.";
            }

            var totalFacturas = CxC_AbonoLote_Monto_Redondear(
                request.facturas.Sum(factura => factura.monto));

            if (totalFacturas <= 0)
            {
                return "El total de las facturas debe ser mayor que cero.";
            }

            if (CxC_AbonoLote_Monto_Redondear(request.total_detallado)
                != totalFacturas)
            {
                return "No se ha recaudado el total a cancelar de "
                    + "las facturas seleccionadas.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Construye una respuesta de error estándar para la aplicación.
        /// </summary>
        /// <param name="mensaje">Detalle del error.</param>
        /// <param name="codigo">Código funcional del error.</param>
        /// <returns>Respuesta de error normalizada.</returns>
        private static ErrorDto<CxCAbonoLoteAplicarData>
            CxC_AbonoLote_Error_Crear(
                string mensaje,
                int codigo = -1)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                codigo,
                new CxCAbonoLoteAplicarData());
        }

        /// <summary>
        /// Normaliza texto opcional para enviarlo a base de datos.
        /// </summary>
        /// <param name="valor">Texto que se debe normalizar.</param>
        /// <returns>Texto sin espacios laterales y nunca nulo.</returns>
        private static string CxC_AbonoLote_Texto_Normalizar(
            string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        /// <summary>
        /// Redondea un monto a la precisión monetaria del formulario.
        /// </summary>
        /// <param name="valor">Monto que se debe redondear.</param>
        /// <returns>Monto redondeado a dos decimales.</returns>
        private static decimal CxC_AbonoLote_Monto_Redondear(
            decimal valor)
        {
            return Math.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        private sealed class CxCAbonoLoteFacturaActualData
        {
            public int operacion { get; init; } = 0;

            public string cod_factura { get; init; } = string.Empty;

            public decimal monto { get; init; } = 0;
        }
    }
}
