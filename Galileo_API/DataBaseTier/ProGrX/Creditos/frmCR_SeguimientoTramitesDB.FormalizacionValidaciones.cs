using System.Data;
using System.Globalization;
using Dapper;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Reproduce fxVerificaFormalizacion del formulario VB6 y devuelve los mensajes encontrados.
        /// </summary>
        private List<string> Cr_SeguimientoTramites_Formalizacion_Validar(
            IDbConnection conn,
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            DateTime fechaSistema,
            decimal fechaCredito)
        {
            List<string> mensajes = Cr_SeguimientoTramites_Formalizacion_ValidacionesBasicas_Obtener(
                request,
                fechaSistema);

            Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Validar(
                conn,
                request,
                fechaCredito,
                mensajes);

            if (!string.Equals(
                    request.estado_solicitud.Trim(),
                    "R",
                    StringComparison.OrdinalIgnoreCase))
            {
                mensajes.Add("- Esta solicitud no se encuentra recibida...");
            }

            Cr_SeguimientoTramites_Formalizacion_Rangos_Validar(conn, request, mensajes);
            Cr_SeguimientoTramites_Formalizacion_Procedimiento_Validar(conn, request, mensajes);
            Cr_SeguimientoTramites_Formalizacion_Refundiciones_Validar(conn, request, mensajes);
            Cr_SeguimientoTramites_Formalizacion_Excedente_Validar(conn, request, mensajes);
            Cr_SeguimientoTramites_Formalizacion_Fondo_Validar(conn, request, mensajes);
            Cr_SeguimientoTramites_Formalizacion_Recurso_Validar(
                conn,
                codEmpresa,
                request,
                mensajes);

            return mensajes;
        }

        /// <summary>
        /// Verifica el disponible del recurso contra el monto a girar y el tope de retiros
        /// en caja, conservando el bloque final de fxVerificaFormalizacion del VB6.
        /// </summary>
        private void Cr_SeguimientoTramites_Formalizacion_Recurso_Validar(
            IDbConnection conn,
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            ICollection<string> mensajes)
        {
            // El VB6 solo evalúa este bloque cuando no se acumuló ningún mensaje previo.
            if (mensajes.Count > 0)
            {
                return;
            }

            decimal disponible = Cr_SeguimientoTramites_Formalizacion_Disponible_Obtener(
                conn,
                request.recurso,
                request.fecha_desembolso).disponible;

            var resumenResp = Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener(
                codEmpresa,
                Cr_SeguimientoTramites_Formalizacion_Resumen_Request_Crear(request));

            if (resumenResp.Code != 0 || resumenResp.Result is null)
            {
                mensajes.Add(
                    resumenResp.Description ?? "No fue posible calcular el resumen de la operación.");
                return;
            }

            decimal montoGiros = resumenResp.Result.monto_giros;
            if (montoGiros <= 0)
            {
                return;
            }

            if (disponible < montoGiros)
            {
                Cr_SeguimientoTramites_Formalizacion_Disponible_Mensajes_Agregar(
                    montoGiros,
                    disponible,
                    mensajes);
            }

            Cr_SeguimientoTramites_Formalizacion_RetiroCaja_Validar(
                conn,
                request,
                montoGiros,
                mensajes);
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Disponible_Mensajes_Agregar(
            decimal montoGiros,
            decimal disponible,
            ICollection<string> mensajes)
        {
            mensajes.Add(" - No Hay disponible en el Recurso, para desembolsar esta Operación...");
            mensajes.Add(string.Create(
                CultureInfo.InvariantCulture,
                $" - Monto a Girar : {montoGiros:N2} - Disponible :  {disponible:N2}"));
            mensajes.Add(string.Create(
                CultureInfo.InvariantCulture,
                $" - Monto Faltante para Girar: {montoGiros - disponible:N2}"));
        }

        private static void Cr_SeguimientoTramites_Formalizacion_RetiroCaja_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            decimal montoGiros,
            ICollection<string> mensajes)
        {
            if (!string.Equals(
                    request.emite_tipo.Trim(),
                    "RC",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string? valor = conn.QueryFirstOrDefault<string>(
                "select Valor from CAJAS_PARAMETROS where cod_parametro = '15';");

            if (!decimal.TryParse(
                    valor,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal maximo))
            {
                mensajes.Add(
                    "- No se ha configurado el Monto para Retiros de Efectivos en Cajas, Informe a su Administrador!");
                return;
            }

            if (maximo < montoGiros)
            {
                mensajes.Add(string.Create(
                    CultureInfo.InvariantCulture,
                    $"- El Monto Máximo para Retiros de Efectivos en Cajas es de {maximo:N2}, Informe a su Administrador!"));
            }
        }

        private static List<string> Cr_SeguimientoTramites_Formalizacion_ValidacionesBasicas_Obtener(
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            DateTime fechaSistema)
        {
            var mensajes = new List<string>();

            if (request.fecha_vence.HasValue
                && request.fecha_vence.Value.Date <= fechaSistema.Date)
            {
                mensajes.Add("La fecha de Vencimiento no puede ser igual o menor a la actual");
            }

            if (request.fecha_desembolso.Date < request.fecha_formalizacion.Date)
            {
                mensajes.Add(
                    "- La fecha del desembolsos no puede ser menor que la fecha de formalizacion");
            }

            if (request.tasa_facial < 0)
            {
                mensajes.Add("- Tasa Facial no es correcta!");
            }

            if (string.Equals(request.emite_tipo.Trim(), "TE", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(request.cuenta_bancaria))
            {
                mensajes.Add(
                    "- No se ha especificado una cuenta de ahorros para realizarle la transferencia electrónica...");
            }

            if (request.estado.Trim() is "A" or "C")
            {
                mensajes.Add("- Esta Operación ya fue procesada");
            }

            if (request.pagare < 0)
            {
                mensajes.Add("- # de Pagaré no es válido");
            }

            if (request.primer_deduccion_anio < fechaSistema.Year)
            {
                mensajes.Add("- El Año especificado no es válido");
            }

            if (request.primer_deduccion_mes is < 1 or > 12)
            {
                mensajes.Add("- El Mes para la primer deduccion no es válido");
            }

            return mensajes;
        }

        private static void Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            decimal fechaCredito,
            ICollection<string> mensajes)
        {
            decimal primerDeduccion = Cr_SeguimientoTramites_Formalizacion_PrimerDeduccion_Componer(
                request.primer_deduccion_anio,
                request.primer_deduccion_mes,
                request.primer_deduccion_quincena);

            decimal corte = Cr_SeguimientoTramites_Formalizacion_Corte_Obtener(
                conn,
                request,
                fechaCredito);

            if (!request.ind_deduce_planilla)
            {
                if (primerDeduccion < corte)
                {
                    mensajes.Add(
                        "- La primer deducción no es válida porque menor a la fecha de proceso actual");
                }

                return;
            }

            if (primerDeduccion > corte)
            {
                return;
            }

            // El VB6 tolera la igualdad cuando se deduce la primer cuota.
            if (primerDeduccion == corte && request.ind_primera_cuota)
            {
                return;
            }

            mensajes.Add(
                "- La primer deducción no es válida porque es igual o menor a la fecha de proceso actual");
        }

        private static decimal Cr_SeguimientoTramites_Formalizacion_Corte_Obtener(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            decimal fechaCredito)
        {
            if (!request.ind_deduce_planilla)
            {
                return fechaCredito;
            }

            const string sql = """
                select max(proceso)
                from PRM_BITACORA
                where COD_INSTITUCION = @DeductoraId
                  and GESTION = 'E'
                  and TRANSACCION = '02';
                """;

            return conn.QueryFirstOrDefault<decimal?>(
                sql,
                new { DeductoraId = request.deductora_id }) ?? fechaCredito;
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Rangos_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            ICollection<string> mensajes)
        {
            if (mensajes.Count > 0)
            {
                return;
            }

            string? mensaje = conn.QueryFirstOrDefault<string>(
                """
                exec spCrdFormaliza_Valida_Rangos
                    @Cedula, @Codigo, @Monto, @Tasa, @Plazo, @Destino, @Garantia, @Operacion;
                """,
                new
                {
                    Cedula = request.cedula.Trim(),
                    Codigo = request.codigo.Trim(),
                    Monto = request.monto,
                    Tasa = request.tasa,
                    Plazo = request.plazo,
                    Destino = request.destino.Trim(),
                    Garantia = request.garantia.Trim(),
                    Operacion = request.operacion
                });

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                mensajes.Add(mensaje.Trim());
            }
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Procedimiento_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            ICollection<string> mensajes)
        {
            object? fila = conn.QueryFirstOrDefault(
                "exec spCRDFormalizaValidacion @Operacion, @Usuario;",
                new
                {
                    Operacion = request.operacion,
                    Usuario = request.usuario.Trim()
                });

            if (fila is not IDictionary<string, object> columnas)
            {
                mensajes.Add("- No fue posible ejecutar la validación de formalización.");
                return;
            }

            Dictionary<string, int> banderas =
                Cr_SeguimientoTramites_Formalizacion_Banderas_Leer(columnas);

            foreach ((string columna, string mensaje) in
                Cr_SeguimientoTramites_Formalizacion_Banderas_Definir(request.codigo))
            {
                // Solo se evalúan las banderas que el procedimiento realmente devuelve:
                // su juego de columnas ha variado entre versiones de la base.
                if (banderas.TryGetValue(columna, out int valor) && valor == 0)
                {
                    mensajes.Add(mensaje);
                }
            }
        }

        private static Dictionary<string, int>
            Cr_SeguimientoTramites_Formalizacion_Banderas_Leer(
                IDictionary<string, object> columnas)
        {
            var banderas = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, object> columna in columnas)
            {
                if (columna.Value is null || columna.Value is DBNull)
                {
                    continue;
                }

                try
                {
                    banderas[columna.Key] = Convert.ToInt32(
                        columna.Value,
                        CultureInfo.InvariantCulture);
                }
                catch (Exception ex) when (ex is FormatException or InvalidCastException
                    or OverflowException)
                {
                    // Columnas no numéricas del procedimiento: no son banderas.
                }
            }

            return banderas;
        }

        private static IEnumerable<(string columna, string mensaje)>
            Cr_SeguimientoTramites_Formalizacion_Banderas_Definir(string codigo)
        {
            return
            [
                ("Nivel",
                    $"- No existe nivel de formalización de este usuario para la línea {codigo.Trim()}"),
                ("Refundicion", "- El saldo a refundir vario en la operación"),
                ("Bloqueo",
                    "- Esta persona se encuentra bloqueda, hasta mañana se le podran formalizar operaciones..."),
                ("GarAhorro", " - El Monto aprobado excede el porcentaje aprobado de sus ahorros"),
                ("MaxOperaciones",
                    "- No se permite sobrepasar el número máximo de operaciones en esta linea"),
                ("MaxLinea", "- No se permite sobrepasar el monto máximo de la línea"),
                ("MaxGarantia", "- No se permite sobrepasar el monto maximo de la línea x Garantía"),
                ("MaxGarantiaTotal", "- No se permite sobrepasar el monto máximo x Garantía"),
                ("Firmas", "- No se han registrado todas las firmas..."),
                ("LineaActiva", "- La línea de crédito no se encuentra Activa..."),
                ("DestinoActivo", "- El destino del crédito no se encuentra Activo..."),
                ("Cobertura", "- Cobertura de las Hipotecas es inferior al monto del crédito..."),
                ("Prendas", "- Cobertura de las Prendas es inferior al monto del crédito..."),
                ("EstadoPersona",
                    "- Esta Línea de Crédito no Admite El estado actual de la persona (verifique.!)"),
                ("CongeladoCredito",
                    "- La persona tiene un Proceso de Congelamiento de Cuentas (verifique.!)"),
                ("Requisitos", "- No se cumplieron los requisitos Obligatorios (verifique.!)"),
                ("BaseCalculo", "- No se ha establecido la Base de Calculo para Cuota Balloon!")
            ];
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Refundiciones_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            ICollection<string> mensajes)
        {
            int cambios = conn.QueryFirstOrDefault<int?>(
                "exec spCrdSGTRefundicionesValida @Operacion;",
                new { Operacion = request.operacion }) ?? 0;

            if (cambios > 0)
            {
                mensajes.Add(
                    $"- {cambios} Operación a Refundir a Cambiado su Estado ---> Actualice!");
            }
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Excedente_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            ICollection<string> mensajes)
        {
            string? codigoExcedente = conn.QueryFirstOrDefault<string>(
                "select ase_codigo from excedentes_parametros;");

            if (!string.Equals(
                    codigoExcedente?.Trim(),
                    request.codigo.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            CrSeguimientoTramitesFormalizacionExcedenteRaw? excedente =
                conn.QueryFirstOrDefault<CrSeguimientoTramitesFormalizacionExcedenteRaw>(
                    "exec spVoxExcedenteCredito @Cedula;",
                    new { Cedula = request.cedula.Trim() });

            if (request.monto > (excedente?.@base ?? 0m))
            {
                mensajes.Add(
                    "- Este es un prestamo sobre excedentes, y el monto aprobado sobrepasa la tabla autorizada...");
            }
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Fondo_Validar(
            IDbConnection conn,
            CrSeguimientoTramitesFormalizacionAplicarRequest request,
            ICollection<string> mensajes)
        {
            if (!string.Equals(request.garantia.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(request.fnd_garantia))
            {
                mensajes.Add(" - No existe un PLAN especificado para cobertura de esta garantía");
                return;
            }

            decimal disponible = conn.QueryFirstOrDefault<decimal?>(
                "exec spCRDGarantiaFNDCalculo @Cedula, @Garantia, @Contrato;",
                new
                {
                    Cedula = request.cedula.Trim(),
                    Garantia = request.fnd_garantia.Trim(),
                    Contrato = request.fnd_contrato
                }) ?? 0m;

            if (request.monto <= disponible)
            {
                return;
            }

            mensajes.Add(request.fnd_contrato > 0
                ? " - El Monto Solicitado excede la cobertura de su PLAN DE INVERSION..."
                : " - El Monto Solicitado excede la cobertura de sus PLANES de ahorros...");
        }

        /// <summary>
        /// Reproduce fxVerificaAnulacion del formulario VB6 y devuelve los mensajes encontrados.
        /// </summary>
        private static List<string> Cr_SeguimientoTramites_Formalizacion_Anulacion_Validar(
            IDbConnection conn,
            IDbTransaction transaction,
            CrSeguimientoTramitesFormalizacionAnularRequest request,
            int sysPlanPagos)
        {
            var mensajes = new List<string>();

            if (!string.Equals(
                    request.estado_solicitud.Trim(),
                    "F",
                    StringComparison.OrdinalIgnoreCase))
            {
                mensajes.Add(
                    "- Esta Operación no ha sido formalizada! Utilice el Estado de DENEGADA!");
                return mensajes;
            }

            CrSeguimientoTramitesFormalizacionAnulacionRaw raw =
                Cr_SeguimientoTramites_Formalizacion_Anulacion_Referencias_Obtener(
                    conn,
                    transaction,
                    request);

            if (raw.nivel == 0)
            {
                mensajes.Add(
                    $"- No existe nivel de anulación de este usuario para la línea.: {request.codigo.Trim()}");
            }

            if (Math.Abs(raw.meses_diferencia) > 0)
            {
                mensajes.Add("- Esta operación fue formalizada en un mes diferente...");
            }

            if (raw.desembolsos_tesoreria > 0)
            {
                mensajes.Add(
                    "- Existen solicitudes o documentos emitidos (Cheques/Transferencias) en Tesorería (Proceda a Anularlos)");
            }

            if (raw.retencion == 1)
            {
                mensajes.Add("- Este es un código de retención No se puede Anular...");
            }

            if (sysPlanPagos == 0)
            {
                Cr_SeguimientoTramites_Formalizacion_Anulacion_Movimientos_Validar(
                    conn,
                    transaction,
                    request,
                    raw.fechaforp,
                    mensajes);
            }

            return mensajes;
        }

        private static CrSeguimientoTramitesFormalizacionAnulacionRaw
            Cr_SeguimientoTramites_Formalizacion_Anulacion_Referencias_Obtener(
                IDbConnection conn,
                IDbTransaction transaction,
                CrSeguimientoTramitesFormalizacionAnularRequest request)
        {
            const string sql = """
                select
                    (
                        select isnull(count(1), 0)
                        from NIVEL_GRUPOS N
                        inner join nivel_miembros A on N.NV_COD_GRUPO = A.NV_COD_GRUPO
                        inner join nivel_derechos B on N.NV_COD_GRUPO = B.NV_COD_GRUPO
                        where A.nombre = @Usuario
                          and B.codigo = @Codigo
                          and N.nv_tipo = 'N'
                          and @Monto between nv_desde and nv_hasta
                    ) as nivel,
                    (
                        select isnull(
                            month(fechaforp) - month(dbo.MyGetdate())
                            + (year(fechaforp) - year(dbo.MyGetdate())), 0)
                        from reg_creditos
                        where id_solicitud = @Operacion
                    ) as meses_diferencia,
                    (
                        select isnull(count(1), 0)
                        from Tes_Transacciones
                        where op = @Operacion and estado <> 'A'
                    ) as desembolsos_tesoreria,
                    (
                        select case when isnull(max(retencion), 'N') = 'S' then 1 else 0 end
                        from catalogo
                        where codigo = @Codigo
                    ) as retencion,
                    (
                        select max(fechaforp)
                        from reg_creditos
                        where id_solicitud = @Operacion
                    ) as fechaforp;
                """;

            return conn.QueryFirst<CrSeguimientoTramitesFormalizacionAnulacionRaw>(
                sql,
                new
                {
                    Usuario = request.usuario.Trim(),
                    Codigo = request.codigo.Trim(),
                    Monto = request.monto,
                    Operacion = request.operacion
                },
                transaction);
        }

        private static void Cr_SeguimientoTramites_Formalizacion_Anulacion_Movimientos_Validar(
            IDbConnection conn,
            IDbTransaction transaction,
            CrSeguimientoTramitesFormalizacionAnularRequest request,
            DateTime? fechaFormalizacion,
            ICollection<string> mensajes)
        {
            const string sql = """
                select
                    (
                        select isnull(count(1), 0)
                        from creditos_dt
                        where id_solicitud = @Operacion and ncon <> @OperacionTexto
                    ) as movimientos,
                    (
                        select isnull(count(1), 0)
                        from MOROSIDAD
                        where id_solicitud = @Operacion
                    ) as morosidad,
                    (
                        select isnull(count(1), 0)
                        from creditos_dt
                        where fechas > @FechaFormalizacion
                          and id_solicitud <> @Operacion
                          and tcon not in ('3', 'FRM')
                          and ncon = @OperacionTexto
                    ) as refundiciones_posteriores,
                    (
                        select isnull(count(1), 0)
                        from morosidad
                        where Estado = 'C'
                          and fecUlt > @FechaFormalizacion
                          and id_solicitud <> @Operacion
                          and tcon not in ('3', 'FRM')
                          and ncon = @OperacionTexto
                    ) as mora_refundiciones_posteriores,
                    (
                        select isnull(count(1), 0)
                        from creditos_dt R
                        where R.tcon in ('3', 'FRM')
                          and R.ncon = @OperacionTexto
                          and exists (
                                select 1
                                from creditos_dt M
                                where M.id_solicitud = R.id_solicitud
                                  and M.consec > R.consec)
                    ) as refundiciones_movidas;
                """;

            CrSeguimientoTramitesFormalizacionMovimientosRaw raw =
                conn.QueryFirst<CrSeguimientoTramitesFormalizacionMovimientosRaw>(
                    sql,
                    new
                    {
                        Operacion = request.operacion,
                        OperacionTexto = request.operacion.ToString(
                            System.Globalization.CultureInfo.InvariantCulture),
                        FechaFormalizacion = fechaFormalizacion ?? DateTime.Today
                    },
                    transaction);

            if (raw.movimientos > 0 || raw.morosidad > 0)
            {
                mensajes.Add("- Existen movimientos a esta operación después de su formalización");
            }

            if (raw.refundiciones_movidas > 0)
            {
                mensajes.Add(
                    $"- Existen movimientos realizados a {raw.refundiciones_movidas} operación(es) posterior a su refundicion");
            }

            if (raw.refundiciones_posteriores > 0)
            {
                mensajes.Add(
                    "- Existen movimientos realizados a refundiciones posterior a la formalizacion");
            }

            if (raw.mora_refundiciones_posteriores > 0)
            {
                mensajes.Add(
                    "- Existen movimientos realizados a Mora de refundiciones posterior a la formalizacion");
            }
        }
    }
}
