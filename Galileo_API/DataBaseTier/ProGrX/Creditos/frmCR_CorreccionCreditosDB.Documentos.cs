using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCorreccionCreditosDb
    {
        /// <summary>Aplica un cambio de monto y genera el documento contable equivalente al VB6.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio solicitado.</param>
        /// <param name="operacion">Datos actuales de la operación.</param>
        /// <param name="sysPlanPagos">Indicador del sistema de planes de pago.</param>
        /// <param name="fechaCredito">Proceso vigente del módulo de crédito.</param>
        /// <param name="sysDocVersion">Versión del sistema documental.</param>
        /// <param name="enlace">Enlace contable.</param>
        /// <param name="oficinaTitular">Oficina titular del usuario.</param>
        /// <returns>Resultado con el documento generado.</returns>
        private CrCorreccionCreditosResultado CR_CorreccionCreditos_Monto_Aplicar(
            int codEmpresa,
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            int sysPlanPagos,
            decimal fechaCredito,
            int sysDocVersion,
            int enlace,
            string oficinaTitular)
        {
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
            var cuentaDocumento = _mRecibos.FxDocumentoCuenta(codEmpresa, tipoDocumento);
            if (string.IsNullOrWhiteSpace(cuentaDocumento))
                throw new InvalidOperationException(
                    "No se puede realizar el movimiento porque no existe una cuenta contable válida para el documento.");

            var numeroDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, tipoDocumento);
            var cuota = MCobroDb.fxCalcula_Cuota(nuevoMonto, operacion.plazo, operacion.interes);
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(@"
                update reg_creditos
                   set montoapr=@NuevoMonto, cuota=@Cuota, saldo=@NuevoMonto-amortiza
                 where id_solicitud=@Operacion and estado='A';",
                new { NuevoMonto = nuevoMonto, Cuota = cuota, Operacion = request.operacion }, tx));

            var cuentas = CR_CorreccionCreditos_OperacionCuentas_Obtener(conn, tx, request.operacion);
            CR_CorreccionCreditos_DocumentoMonto_Insertar(
                conn, tx, request, operacion, cuentas, tipoDocumento,
                numeroDocumento, montoDiferencia, cuentaDocumento, debeHaber,
                enlace, oficinaTitular);

            var tipoMovimiento = sysDocVersion == 1 && tipoDocumento == "NC" ? "7" : tipoDocumento;
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
                    FechaCredito = fechaCredito,
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
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos actuales de la operación.</param>
        /// <param name="sysDocVersion">Versión del sistema documental.</param>
        /// <param name="enlace">Enlace contable.</param>
        /// <param name="oficinaTitular">Oficina titular del usuario.</param>
        /// <returns>Resultado con el documento generado.</returns>
        private CrCorreccionCreditosResultado CR_CorreccionCreditos_Oficina_Aplicar(
            int codEmpresa,
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            int sysDocVersion,
            int enlace,
            string oficinaTitular)
        {
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
            var numeroDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, tipoDocumento);
            var cuentas = CR_CorreccionCreditos_OperacionCuentas_Obtener(conn, tx, request.operacion);
            if (sysDocVersion == 2)
            {
                CR_CorreccionCreditos_DocumentoOficina_Insertar(
                    conn, tx, request, operacion, cuentas, oficina,
                    numeroDocumento, enlace, oficinaTitular);
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
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos de la operación.</param>
        /// <param name="cuentas">Cuentas contables de la operación.</param>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <param name="monto">Monto del ajuste.</param>
        /// <param name="cuentaDocumento">Cuenta contraparte.</param>
        /// <param name="debeHaber">Naturaleza del asiento principal.</param>
        /// <param name="enlace">Enlace contable.</param>
        /// <param name="oficinaTitular">Oficina titular.</param>
        private static void CR_CorreccionCreditos_DocumentoMonto_Insertar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            CrCorreccionCreditosOperacionCtasData cuentas,
            string tipoDocumento,
            long numeroDocumento,
            decimal monto,
            string cuentaDocumento,
            string debeHaber,
            int enlace,
            string oficinaTitular)
        {
            CR_CorreccionCreditos_Documento_Insertar(
                conn, tx, request, operacion, tipoDocumento, numeroDocumento, monto,
                oficinaTitular, "CRD012",
                "CAMBIO DE MONTO",
                $"DE {operacion.montoapr:N2}",
                $"A {request.valor_numerico:N2}",
                string.Empty,
                $"Divisa: {cuentas.cod_Divisa} / Tipo Cambio: {cuentas.TipoCambio}",
                string.Empty);
            CR_CorreccionCreditos_Asiento_Insertar(
                conn, tx, cuentas, tipoDocumento, numeroDocumento, monto,
                debeHaber, cuentas.ctaamortiza, enlace, cuentas.cod_unidad, cuentas.cod_centro_costo);
            CR_CorreccionCreditos_Asiento_Insertar(
                conn, tx, cuentas, tipoDocumento, numeroDocumento, monto,
                debeHaber == "D" ? "C" : "D", cuentaDocumento, enlace,
                cuentas.cod_unidad, cuentas.cod_centro_costo);
        }

        /// <summary>Registra el documento y los asientos del cambio de oficina.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos de la operación.</param>
        /// <param name="cuentas">Cuentas contables.</param>
        /// <param name="oficina">Nueva oficina.</param>
        /// <param name="numeroDocumento">Número del documento.</param>
        /// <param name="enlace">Enlace contable.</param>
        /// <param name="oficinaTitular">Oficina titular.</param>
        private static void CR_CorreccionCreditos_DocumentoOficina_Insertar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            CrCorreccionCreditosOperacionCtasData cuentas,
            CrCorreccionCreditosOficinaData oficina,
            long numeroDocumento,
            int enlace,
            string oficinaTitular)
        {
            CR_CorreccionCreditos_Documento_Insertar(
                conn, tx, request, operacion, "ND", numeroDocumento, cuentas.saldo,
                oficinaTitular, "CRD012",
                $"Saldo {cuentas.saldo:N2}",
                $"Oficina Actual {operacion.cod_oficina_r}_{operacion.oficina_descripcion}",
                $"Oficina Nueva {oficina.cod_oficina}_{oficina.descripcion}",
                string.Empty,
                $"Divisa: {cuentas.cod_Divisa} / Tipo Cambio: {cuentas.TipoCambio}",
                "Cambia de Oficina/Agencia");
            CR_CorreccionCreditos_Asiento_Insertar(
                conn, tx, cuentas, "ND", numeroDocumento, cuentas.saldo,
                "D", cuentas.ctaamortiza, enlace, oficina.cod_unidad, oficina.cod_centro_costo, false);
            CR_CorreccionCreditos_Asiento_Insertar(
                conn, tx, cuentas, "ND", numeroDocumento, cuentas.saldo,
                "C", cuentas.ctaamortiza, enlace, cuentas.cod_unidad, cuentas.cod_centro_costo, false);
        }

        /// <summary>Inserta el encabezado documental común de una corrección.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos de la operación.</param>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <param name="monto">Monto del documento.</param>
        /// <param name="oficinaTitular">Oficina titular.</param>
        /// <param name="concepto">Concepto funcional del documento.</param>
        /// <param name="linea1">Primera línea descriptiva.</param>
        /// <param name="linea2">Segunda línea descriptiva.</param>
        /// <param name="linea3">Tercera línea descriptiva.</param>
        /// <param name="linea4">Cuarta línea descriptiva.</param>
        /// <param name="linea7">Información de divisa y tipo de cambio.</param>
        /// <param name="linea10">Décima línea descriptiva.</param>
        private static void CR_CorreccionCreditos_Documento_Insertar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            string tipoDocumento,
            long numeroDocumento,
            decimal monto,
            string oficinaTitular,
            string concepto,
            string linea1,
            string linea2,
            string linea3,
            string linea4,
            string linea7,
            string linea10)
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
                    Documento = numeroDocumento,
                    Tipo = tipoDocumento,
                    Usuario = request.usuario,
                    operacion.cedula,
                    operacion.nombre,
                    Monto = monto,
                    Operacion = request.operacion.ToString(),
                    operacion.codigo,
                    Oficina = oficinaTitular,
                    Concepto = concepto,
                    Linea1 = linea1,
                    Linea2 = linea2,
                    Linea3 = linea3,
                    Linea4 = linea4,
                    Linea6 = $"Operación {request.operacion}..{operacion.codigo}..{operacion.opex_descripcion.ToUpperInvariant()}",
                    Linea7 = linea7,
                    Linea8 = operacion.descripcion,
                    Linea9 = $"Usuario {request.usuario}",
                    Linea10 = linea10,
                    Notas = request.notas
                }, tx);

        /// <summary>Registra una línea del asiento contable de la corrección.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="cuentas">Cuentas de la operación.</param>
        /// <param name="tipoDocumento">Tipo de documento.</param>
        /// <param name="numeroDocumento">Número de documento.</param>
        /// <param name="monto">Monto del asiento.</param>
        /// <param name="debeHaber">Naturaleza débito o crédito.</param>
        /// <param name="cuenta">Cuenta contable.</param>
        /// <param name="enlace">Enlace contable.</param>
        /// <param name="unidad">Unidad contable.</param>
        /// <param name="centroCosto">Centro de costo.</param>
        /// <param name="aplicarTipoCambio">Indica si el monto debe convertirse con el tipo de cambio de la operación.</param>
        private static void CR_CorreccionCreditos_Asiento_Insertar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosOperacionCtasData cuentas,
            string tipoDocumento,
            long numeroDocumento,
            decimal monto,
            string debeHaber,
            string cuenta,
            int enlace,
            string unidad,
            string centroCosto,
            bool aplicarTipoCambio = true)
        {
            if (string.IsNullOrWhiteSpace(cuenta))
                throw new InvalidOperationException("No se encontró una cuenta contable válida para el asiento.");
            conn.Execute(@"
                exec spSIFDocsAsiento @Tipo,@Documento,@Monto,@DebeHaber,@Divisa,@TipoCambio,
                     @Enlace,@Unidad,@CentroCosto,@Cuenta,@Operacion,@Codigo,'';",
                new
                {
                    Tipo = tipoDocumento,
                    Documento = numeroDocumento,
                    Monto = aplicarTipoCambio ? monto * cuentas.TipoCambio : monto,
                    DebeHaber = debeHaber,
                    Divisa = cuentas.cod_Divisa,
                    TipoCambio = aplicarTipoCambio ? cuentas.TipoCambio : 1,
                    Enlace = enlace,
                    Unidad = unidad,
                    CentroCosto = centroCosto,
                    Cuenta = cuenta,
                    Operacion = cuentas.id_solicitud,
                    Codigo = cuentas.Codigo
                }, tx);
        }

        private sealed class CrCorreccionCreditosOperacionCtasData
        {
            public int id_solicitud { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public decimal saldo { get; set; }
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
    }
}
