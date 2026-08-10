using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCorreccionCreditosDb
    {
        /// <summary>Aplica un cambio de monto y genera el documento contable equivalente al VB6.</summary>
        /// <param name="contexto">Contexto transaccional y funcional del cambio.</param>
        /// <returns>Resultado con el documento generado.</returns>
        private CrCorreccionCreditosResultado CR_CorreccionCreditos_Monto_Aplicar(
            CrCorreccionCreditosCambioContext contexto)
        {
            var conn = contexto.Conn;
            var tx = contexto.Tx;
            var request = contexto.Request;
            var operacion = contexto.Operacion;
            var sysPlanPagos = contexto.SysPlanPagos;
            if (sysPlanPagos == 1)
                throw new InvalidOperationException("El cambio de monto no está disponible con plan de pagos.");

            var nuevoMonto = request.valor_numerico ?? -1;
            if (nuevoMonto <= 0)
                throw new InvalidOperationException("El nuevo monto debe ser mayor que cero.");
            if (nuevoMonto == operacion.montoapr)
                throw new InvalidOperationException("El nuevo monto debe ser diferente al monto actual.");

            if (operacion.retencion)
                return CR_CorreccionCreditos_MontoRetencion_Aplicar(conn, tx, request, operacion, nuevoMonto);

            var tipoDocumento = nuevoMonto > operacion.montoapr ? "ND" : "NC";
            var debeHaber = tipoDocumento == "ND" ? "D" : "H";
            var montoDiferencia = Math.Abs(nuevoMonto - operacion.montoapr);
            var cuentaDocumento = _mRecibos.FxDocumentoCuenta(contexto.CodEmpresa, tipoDocumento);
            if (string.IsNullOrWhiteSpace(cuentaDocumento))
                throw new InvalidOperationException(
                    "No se puede realizar el movimiento porque no existe una cuenta contable válida para el documento.");

            var numeroDocumento = _mRecibos.FxDocumentoConsecutivo(contexto.CodEmpresa, tipoDocumento);
            var cuota = MCobroDb.fxCalcula_Cuota(nuevoMonto, operacion.plazo, operacion.interes);
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(@"
                update reg_creditos
                   set montoapr=@NuevoMonto, cuota=@Cuota, saldo=@NuevoMonto-amortiza
                 where id_solicitud=@Operacion and estado='A';",
                new { NuevoMonto = nuevoMonto, Cuota = cuota, Operacion = request.operacion }, tx));

            var cuentas = CR_CorreccionCreditos_OperacionCuentas_Obtener(conn, tx, request.operacion);
            CR_CorreccionCreditos_DocumentoMonto_Insertar(
                contexto, cuentas, tipoDocumento, numeroDocumento,
                montoDiferencia, cuentaDocumento, debeHaber);

            var tipoMovimiento = contexto.SysDocVersion == 1 && tipoDocumento == "NC" ? "7" : tipoDocumento;
            conn.Execute(@"
                insert creditos_dt
                    (codigo,id_solicitud,cuota,abono,intcp,amortiza,fechas,fechap,
                     tcon,ncon,estado,cod_concepto,usuario,cod_caja)
                values
                    (@Codigo,@Operacion,0,0,0,@Monto,dbo.MyGetdate(),@FechaCredito,
                     @TipoMovimiento,@Documento,'A','CRD012',@Usuario,'');",
                new
                {
                    Codigo = operacion.codigo,
                    Operacion = request.operacion,
                    Monto = montoDiferencia,
                    FechaCredito = contexto.FechaCredito,
                    TipoMovimiento = tipoMovimiento,
                    Documento = numeroDocumento,
                    Usuario = request.usuario
                }, tx);

            return new CrCorreccionCreditosResultado
            {
                mensaje = $"Cambio realizado satisfactoriamente. Se generó {tipoDocumento} #{numeroDocumento}.",
                tipo_documento = tipoDocumento,
                numero_documento = Convert.ToInt32(numeroDocumento)
            };
        }

        /// <summary>Aplica el cambio de monto especial para líneas de retención.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos actuales de la operación.</param>
        /// <param name="nuevoMonto">Nuevo monto total solicitado.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_MontoRetencion_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            decimal nuevoMonto)
        {
            if (nuevoMonto < operacion.saldo)
                throw new InvalidOperationException(
                    "El nuevo monto es inferior al saldo de la operación de retención.");

            var montoPeriodo = nuevoMonto / Math.Max(operacion.plazo, 1);
            var saldo = nuevoMonto - operacion.amortiza;
            var cuota = operacion.plazo_restante <= 0
                ? saldo
                : saldo / operacion.plazo_restante;
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(@"
                update reg_creditos
                   set montoapr=@MontoPeriodo, cuota=@Cuota, saldo=@Saldo
                 where id_solicitud=@Operacion and estado='A';",
                new { MontoPeriodo = montoPeriodo, Cuota = cuota, Saldo = saldo, Operacion = request.operacion }, tx));
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Aplica el cambio de oficina y registra el asiento entre oficinas.</summary>
        /// <param name="contexto">Contexto transaccional y funcional del cambio.</param>
        /// <returns>Resultado con el documento generado.</returns>
        private CrCorreccionCreditosResultado CR_CorreccionCreditos_Oficina_Aplicar(
            CrCorreccionCreditosCambioContext contexto)
        {
            var conn = contexto.Conn;
            var tx = contexto.Tx;
            var request = contexto.Request;
            var operacion = contexto.Operacion;
            var nuevaOficina = request.valor.Trim();
            if (string.IsNullOrWhiteSpace(nuevaOficina) || nuevaOficina == operacion.cod_oficina_r)
                throw new InvalidOperationException("Debe seleccionar una oficina diferente a la actual.");

            var oficina = conn.QueryFirstOrDefault<CrCorreccionCreditosOficinaData>(@"
                select top 1 rtrim(cod_oficina) as cod_oficina,
                       rtrim(descripcion) as descripcion,
                       rtrim(cod_unidad) as cod_unidad,
                       rtrim(cod_centro_costo) as cod_centro_costo
                  from SIF_OFICINAS where cod_oficina=@Oficina;",
                new { Oficina = nuevaOficina }, tx)
                ?? throw new InvalidOperationException("La oficina seleccionada no existe.");

            var tipoDocumento = "ND";
            var numeroDocumento = _mRecibos.FxDocumentoConsecutivo(contexto.CodEmpresa, tipoDocumento);
            var cuentas = CR_CorreccionCreditos_OperacionCuentas_Obtener(conn, tx, request.operacion);
            if (contexto.SysDocVersion == 2)
            {
                CR_CorreccionCreditos_DocumentoOficina_Insertar(
                    contexto, cuentas, oficina, numeroDocumento);
            }

            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(@"
                update reg_creditos set cod_oficina_r=@Oficina
                 where id_solicitud=@Operacion and estado='A';",
                new { Oficina = nuevaOficina, Operacion = request.operacion }, tx));
            return new CrCorreccionCreditosResultado
            {
                mensaje = $"Cambio realizado satisfactoriamente. Se generó ND #{numeroDocumento}.",
                tipo_documento = tipoDocumento,
                numero_documento = Convert.ToInt32(numeroDocumento)
            };
        }

        /// <summary>Obtiene las cuentas contables vinculadas a una operación.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="operacion">Identificador de la operación.</param>
        /// <returns>Cuentas y datos monetarios de la operación.</returns>
        private static CrCorreccionCreditosOperacionCtasData CR_CorreccionCreditos_OperacionCuentas_Obtener(
            IDbConnection conn,
            IDbTransaction tx,
            int operacion)
            => conn.QueryFirstOrDefault<CrCorreccionCreditosOperacionCtasData>(
                "exec spCrdOperacionCtas @Operacion",
                new { Operacion = operacion }, tx)
                ?? throw new InvalidOperationException("No fue posible obtener las cuentas de la operación.");

        /// <summary>Registra el documento y los asientos del cambio de monto.</summary>
        /// <param name="contexto">Contexto transaccional y funcional del cambio.</param>
        /// <param name="cuentas">Cuentas contables de la operación.</param>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <param name="monto">Monto del ajuste.</param>
        /// <param name="cuentaDocumento">Cuenta contraparte.</param>
        /// <param name="debeHaber">Naturaleza del asiento principal.</param>
        private static void CR_CorreccionCreditos_DocumentoMonto_Insertar(
            CrCorreccionCreditosCambioContext contexto,
            CrCorreccionCreditosOperacionCtasData cuentas,
            string tipoDocumento,
            long numeroDocumento,
            decimal monto,
            string cuentaDocumento,
            string debeHaber)
        {
            var request = contexto.Request;
            var operacion = contexto.Operacion;
            CR_CorreccionCreditos_Documento_Insertar(
                contexto.Conn,
                contexto.Tx,
                new CrCorreccionCreditosDocumentoData
                {
                    Request = request,
                    Operacion = operacion,
                    TipoDocumento = tipoDocumento,
                    NumeroDocumento = numeroDocumento,
                    Monto = monto,
                    OficinaTitular = contexto.OficinaTitular,
                    Concepto = "CRD012",
                    Linea1 = "CAMBIO DE MONTO",
                    Linea2 = $"DE {operacion.montoapr:N2}",
                    Linea3 = $"A {request.valor_numerico:N2}",
                    Linea7 = $"Divisa: {cuentas.cod_Divisa} / Tipo Cambio: {cuentas.TipoCambio}"
                });
            CR_CorreccionCreditos_Asiento_Insertar(
                contexto.Conn, contexto.Tx, new CrCorreccionCreditosAsientoData
                {
                    Cuentas = cuentas, TipoDocumento = tipoDocumento, NumeroDocumento = numeroDocumento,
                    Monto = monto, DebeHaber = debeHaber, Cuenta = cuentas.ctaamortiza,
                    Enlace = contexto.Enlace, Unidad = cuentas.cod_unidad, CentroCosto = cuentas.cod_centro_costo
                });
            CR_CorreccionCreditos_Asiento_Insertar(
                contexto.Conn, contexto.Tx, new CrCorreccionCreditosAsientoData
                {
                    Cuentas = cuentas, TipoDocumento = tipoDocumento, NumeroDocumento = numeroDocumento,
                    Monto = monto, DebeHaber = debeHaber == "D" ? "C" : "D", Cuenta = cuentaDocumento,
                    Enlace = contexto.Enlace, Unidad = cuentas.cod_unidad, CentroCosto = cuentas.cod_centro_costo
                });
        }

        /// <summary>Registra el documento y los asientos del cambio de oficina.</summary>
        /// <param name="contexto">Contexto transaccional y funcional del cambio.</param>
        /// <param name="cuentas">Cuentas contables.</param>
        /// <param name="oficina">Nueva oficina.</param>
        /// <param name="numeroDocumento">Número del documento.</param>
        private static void CR_CorreccionCreditos_DocumentoOficina_Insertar(
            CrCorreccionCreditosCambioContext contexto,
            CrCorreccionCreditosOperacionCtasData cuentas,
            CrCorreccionCreditosOficinaData oficina,
            long numeroDocumento)
        {
            var operacion = contexto.Operacion;
            CR_CorreccionCreditos_Documento_Insertar(
                contexto.Conn,
                contexto.Tx,
                new CrCorreccionCreditosDocumentoData
                {
                    Request = contexto.Request,
                    Operacion = operacion,
                    TipoDocumento = "ND",
                    NumeroDocumento = numeroDocumento,
                    Monto = cuentas.saldo,
                    OficinaTitular = contexto.OficinaTitular,
                    Concepto = "CRD012",
                    Linea1 = $"Saldo {cuentas.saldo:N2}",
                    Linea2 = $"Oficina Actual {operacion.cod_oficina_r}_{operacion.oficina_descripcion}",
                    Linea3 = $"Oficina Nueva {oficina.cod_oficina}_{oficina.descripcion}",
                    Linea7 = $"Divisa: {cuentas.cod_Divisa} / Tipo Cambio: {cuentas.TipoCambio}",
                    Linea10 = "Cambia de Oficina/Agencia"
                });
            CR_CorreccionCreditos_Asiento_Insertar(
                contexto.Conn, contexto.Tx, new CrCorreccionCreditosAsientoData
                {
                    Cuentas = cuentas, TipoDocumento = "ND", NumeroDocumento = numeroDocumento,
                    Monto = cuentas.saldo, DebeHaber = "D", Cuenta = cuentas.ctaamortiza,
                    Enlace = contexto.Enlace, Unidad = oficina.cod_unidad, CentroCosto = oficina.cod_centro_costo,
                    AplicarTipoCambio = false
                });
            CR_CorreccionCreditos_Asiento_Insertar(
                contexto.Conn, contexto.Tx, new CrCorreccionCreditosAsientoData
                {
                    Cuentas = cuentas, TipoDocumento = "ND", NumeroDocumento = numeroDocumento,
                    Monto = cuentas.saldo, DebeHaber = "C", Cuenta = cuentas.ctaamortiza,
                    Enlace = contexto.Enlace, Unidad = cuentas.cod_unidad, CentroCosto = cuentas.cod_centro_costo,
                    AplicarTipoCambio = false
                });
        }

        /// <summary>Inserta el encabezado documental común de una corrección.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="documento">Datos del encabezado documental.</param>
        private static void CR_CorreccionCreditos_Documento_Insertar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosDocumentoData documento)
            => conn.Execute(@"
                insert SIF_TRANSACCIONES
                    (COD_TRANSACCION,TIPO_DOCUMENTO,REGISTRO_FECHA,REGISTRO_USUARIO,
                     Cliente_IDENTIFICACION,CLIENTE_NOMBRE,cod_concepto,monto,estado,
                     Referencia_01,Referencia_02,Referencia_03,cod_oficina,
                     linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,linea9,linea10,
                     detalle,documento)
                values
                    (@Documento,@Tipo,dbo.MyGetdate(),@Usuario,@Cedula,@Nombre,@Concepto,@Monto,'P',
                     @Operacion,@Codigo,'',@Oficina,@Linea1,@Linea2,@Linea3,@Linea4,'',
                     @Linea6,@Linea7,@Linea8,@Linea9,@Linea10,@Notas,'');",
                new
                {
                    Documento = documento.NumeroDocumento,
                    Tipo = documento.TipoDocumento,
                    Usuario = documento.Request.usuario,
                    documento.Operacion.cedula,
                    documento.Operacion.nombre,
                    documento.Monto,
                    Operacion = documento.Request.operacion.ToString(),
                    documento.Operacion.codigo,
                    Oficina = documento.OficinaTitular,
                    documento.Concepto,
                    documento.Linea1,
                    documento.Linea2,
                    documento.Linea3,
                    documento.Linea4,
                    Linea6 = $"Operación {documento.Request.operacion}..{documento.Operacion.codigo}..{documento.Operacion.opex_descripcion.ToUpperInvariant()}",
                    documento.Linea7,
                    Linea8 = documento.Operacion.descripcion,
                    Linea9 = $"Usuario {documento.Request.usuario}",
                    documento.Linea10,
                    Notas = documento.Request.notas
                }, tx);

        /// <summary>Registra una línea del asiento contable de la corrección.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="asiento">Datos de la línea contable.</param>
        private static void CR_CorreccionCreditos_Asiento_Insertar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAsientoData asiento)
        {
            if (string.IsNullOrWhiteSpace(asiento.Cuenta))
                throw new InvalidOperationException("No se encontró una cuenta contable válida para el asiento.");
            conn.Execute(@"
                exec spSIFDocsAsiento @Tipo,@Documento,@Monto,@DebeHaber,@Divisa,@TipoCambio,
                     @Enlace,@Unidad,@CentroCosto,@Cuenta,@Operacion,@Codigo,'';",
                new
                {
                    Tipo = asiento.TipoDocumento,
                    Documento = asiento.NumeroDocumento,
                    Monto = asiento.AplicarTipoCambio ? asiento.Monto * asiento.Cuentas.TipoCambio : asiento.Monto,
                    asiento.DebeHaber,
                    Divisa = asiento.Cuentas.cod_Divisa,
                    TipoCambio = asiento.AplicarTipoCambio ? asiento.Cuentas.TipoCambio : 1,
                    asiento.Enlace,
                    asiento.Unidad,
                    CentroCosto = asiento.CentroCosto,
                    Cuenta = asiento.Cuenta,
                    Operacion = asiento.Cuentas.id_solicitud,
                    Codigo = asiento.Cuentas.Codigo
                }, tx);
        }

        private sealed class CrCorreccionCreditosOperacionCtasData
        {
            public int id_solicitud { get; set; } = default;
            public string Codigo { get; set; } = string.Empty;
            public decimal saldo { get; set; } = default;
            public string cod_Divisa { get; set; } = string.Empty;
            public decimal TipoCambio { get; set; } = 1;
            public string cod_unidad { get; set; } = string.Empty;
            public string cod_centro_costo { get; set; } = string.Empty;
            public string ctaamortiza { get; set; } = string.Empty;
        }

        private sealed class CrCorreccionCreditosOficinaData
        {
            public string cod_oficina { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public string cod_unidad { get; set; } = string.Empty;
            public string cod_centro_costo { get; set; } = string.Empty;
        }

        private sealed class CrCorreccionCreditosDocumentoData
        {
            public required CrCorreccionCreditosAplicarRequest Request { get; init; }
            public required CrCorreccionCreditosOperacionBase Operacion { get; init; }
            public required string TipoDocumento { get; init; }
            public required long NumeroDocumento { get; init; }
            public required decimal Monto { get; init; }
            public required string OficinaTitular { get; init; }
            public required string Concepto { get; init; }
            public string Linea1 { get; init; } = string.Empty;
            public string Linea2 { get; init; } = string.Empty;
            public string Linea3 { get; init; } = string.Empty;
            public string Linea4 { get; init; } = string.Empty;
            public string Linea7 { get; init; } = string.Empty;
            public string Linea10 { get; init; } = string.Empty;
        }

        private sealed class CrCorreccionCreditosAsientoData
        {
            public required CrCorreccionCreditosOperacionCtasData Cuentas { get; init; }
            public required string TipoDocumento { get; init; }
            public required long NumeroDocumento { get; init; }
            public required decimal Monto { get; init; }
            public required string DebeHaber { get; init; }
            public required string Cuenta { get; init; }
            public required int Enlace { get; init; }
            public required string Unidad { get; init; }
            public required string CentroCosto { get; init; }
            public bool AplicarTipoCambio { get; init; } = true;
        }
    }
}
