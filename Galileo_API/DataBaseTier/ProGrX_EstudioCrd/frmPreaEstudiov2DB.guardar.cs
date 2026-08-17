using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Guarda el preanálisis (nuevo expediente, nuevo sub-expediente, o modificación).
        /// VB6: fxGuardar (frmPreaEstudiov2.frm, línea ~12313):
        ///   1. fxValidaDatos (validaciones en memoria, replicadas en BL/aquí donde aplica).
        ///   2. EXEC spCrdFormaliza_Valida_Rangos (no bloqueante; su Mensaje se guarda en
        ///      CUMPLIMIENTO_NOTAS, sobreescribiendo lo que el usuario haya escrito).
        ///   3. Según modo: sbEstudio_Guarda_Nuevo (spCrdPreaPreanalisisNuevo) o
        ///      sbEstudio_Guarda_Modifica (spCrdPreaPreanalisisModifica).
        ///   4. Si es nuevo: inicializa Refundiciones/Fianzas/CreditosTransito/ImportCreditosVigentes.
        ///   5. Fix final: UPDATE CUMPLIMIENTO_NOTAS/MONTO_POLIZA_DESEMPLEO/APL_POLIZA_DESEMPLEO.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2GuardarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2GuardarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2GuardarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var usuario = request.usuario?.Trim() ?? string.Empty;
                var codPreanalisisActual = request.cod_preanalisis?.Trim() ?? string.Empty;
                var esNuevo = string.IsNullOrEmpty(codPreanalisisActual);

                // VB6: spCrdFormaliza_Valida_Rangos '<cedula>','<linea>',<monto>,<tasa>,<plazo>,'<destino>','<garantia>',0
                // No bloquea el guardado; su mensaje reemplaza CUMPLIMIENTO_NOTAS.
                var notasCumplimiento = ValidarRangosFormalizacion(connection, request);

                string codPreanalisisResultado;

                if (esNuevo)
                {
                    codPreanalisisResultado = GuardarNuevo(connection, usuario, request);

                    if (!string.IsNullOrEmpty(codPreanalisisResultado))
                    {
                        InicializarTablasHijasNuevoExpediente(connection, codPreanalisisResultado);
                    }
                }
                else
                {
                    // VB6: sbEstudio_Guarda_Modifica (spCrdPreaPreanalisisModifica, ~60 parámetros
                    // posicionales). La mayoría de esos parámetros pertenecen a datos de otras
                    // pestañas (Deducciones, Refundiciones, Desembolsos, Fianzas, liquidez, CIC,
                    // etc.) que Angular aún no consolida en un solo request y que se auditarán
                    // pestaña por pestaña. Por ahora se mantiene la llamada existente (parámetros
                    // nombrados, solo los campos que si están confirmados) — pendiente de
                    // completar cuando se audite cada pestaña restante.
                    codPreanalisisResultado = GuardarModifica(connection, usuario, request, notasCumplimiento);
                }

                // VB6: FIX TEMPORAL DE COLUMNAS NUEVAS (fxGuardar, al final, ambos modos).
                if (!string.IsNullOrEmpty(codPreanalisisResultado))
                {
                    AplicarFixColumnasFinal(connection, codPreanalisisResultado, notasCumplimiento, request);
                }

                response.Result = new FrmPreaEstudiov2GuardarResponse
                {
                    cod_preanalisis = codPreanalisisResultado,
                    estado = "G",
                    estado_desc = esNuevo ? "Registrado" : "Actualizado",
                    mensaje = esNuevo
                        ? "La información fue registrada correctamente."
                        : "La información fue actualizada correctamente."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2GuardarResponse();
                return response;
            }
        }

        /// <summary>
        /// EXEC spCrdFormaliza_Valida_Rangos '&lt;cedula&gt;','&lt;linea&gt;',&lt;monto&gt;,
        /// &lt;tasa&gt;,&lt;plazo&gt;,'&lt;destino&gt;','&lt;garantia&gt;',0
        /// (fxGuardar, línea ~12333). No bloquea el guardado — el campo Mensaje resultante
        /// reemplaza lo que hubiera en CUMPLIMIENTO_NOTAS.
        /// </summary>
        private static string ValidarRangosFormalizacion(IDbConnection connection, FrmPreaEstudiov2GuardarRequest request)
        {
            try
            {
                const string sql = @"EXEC spCrdFormaliza_Valida_Rangos
                    @Cedula, @Linea, @Monto, @Tasa, @Plazo, @Destino, @Garantia, 0";
                var row = connection.QueryFirstOrDefault(sql, new
                {
                    Cedula = request.cedula?.Trim() ?? string.Empty,
                    Linea = request.linea?.Trim() ?? string.Empty,
                    request.monto,
                    request.tasa,
                    request.plazo,
                    Destino = request.destino?.Trim() ?? string.Empty,
                    Garantia = request.garantia?.Trim() ?? string.Empty
                }) as IDictionary<string, object>;
                if (row is null)
                {
                    return string.Empty;
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return GetString(dict, "Mensaje");
            }
            catch
            {
                // No bloqueante en VB6 tampoco: si falla, simplemente no hay mensaje de validación.
                return string.Empty;
            }
        }

        /// <summary>
        /// EXEC spCrdPreaPreanalisisNuevo con los 43 parámetros posicionales confirmados en
        /// sbEstudio_Guarda_Nuevo (frmPreaEstudiov2.frm, línea ~12156). Simplificaciones
        /// documentadas (no inventadas, son el comportamiento por defecto real de VB6 cuando
        /// el dato no está disponible en el formulario):
        ///   - TASA_PTS_BONO: VB6 lo inicializa en 0 (línea 10658) y solo lo recalcula tras
        ///     validar membresía de socio (lógica de bono no trazada aún) — se usa 0.
        ///   - pOficina: EXEC sbSIFOficinasUsuario '&lt;usuario&gt;' (campo Titular), igual que
        ///     VB6 al iniciar sesión (mProGrX_Main.bas).
        /// </summary>
        private static string GuardarNuevo(
            IDbConnection connection,
            string usuario,
            FrmPreaEstudiov2GuardarRequest request)
        {
            var esSubExpediente = string.Equals(request.tipo_preanalisis, "S", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(request.cod_preanalisis_ref);

            var tipoExpediente = esSubExpediente ? "S" : "E";
            var expedienteRef = esSubExpediente ? request.cod_preanalisis_ref.Trim() : null;

            var oficina = ObtenerOficinaTitular(connection, usuario);
            var edad = CalcularEdad(request.fecha_nacimiento);
            var cph = string.IsNullOrWhiteSpace(request.cph) ? "0" : request.cph.Trim();
            const int tasaPtsBono = 0; // ver nota en el comentario del método

            var fechaNacimiento = request.fecha_nacimiento.HasValue
                ? request.fecha_nacimiento.Value.ToString("yyyy-MM-dd")
                : string.Empty;

            const string sql = @"EXEC spCrdPreaPreanalisisNuevo
                @TipoSalario, @TipoExpediente, @ExpedienteRef, @Usuario, @Cedula,
                @Linea, @Destino, @Nombre, @Sexo, @FechaNacimiento, @PolizaVida,
                @PolizaIncendio, @PrimeraCuota, @Monto, @Tasa, @Plazo, @Cuota,
                @MontoPolizaVida, @MontoPolizaIncendio, @Compromiso, NULL,
                @Garantia, @Garantia, @Fiadores, NULL, @Oficina, @TasaPtsBono,
                NULL, @Edad, @EdadAplica, @EdadJustificacion, @Plazo, 0, 0, 0,
                @Cph, 1, @MontoConstruccion, @PolizaVehiculo, @MontoPolizaPrenda,
                @ValorPrenda, @ClasificacionCrediticia, @NoOpCrm";
            var row = connection.QueryFirstOrDefault(sql, new
            {
                TipoSalario = request.tipo_salario?.Trim() ?? string.Empty,
                TipoExpediente = tipoExpediente,
                ExpedienteRef = expedienteRef,
                Usuario = usuario,
                Cedula = request.cedula?.Trim() ?? string.Empty,
                Linea = request.linea?.Trim() ?? string.Empty,
                Destino = request.destino?.Trim() ?? string.Empty,
                Nombre = request.nombre?.Trim() ?? string.Empty,
                Sexo = string.IsNullOrEmpty(request.sexo) ? string.Empty : request.sexo.Trim().Substring(0, 1),
                FechaNacimiento = fechaNacimiento,
                PolizaVida = BoolToInt(request.poliza_vida),
                PolizaIncendio = BoolToInt(request.poliza_incendio),
                PrimeraCuota = BoolToInt(request.primera_cuota),
                request.monto,
                request.tasa,
                request.plazo,
                request.cuota,
                MontoPolizaVida = request.monto_poliza_vida,
                MontoPolizaIncendio = request.monto_poliza_incendio,
                request.compromiso,
                Garantia = request.garantia?.Trim() ?? string.Empty,
                request.fiadores,
                Oficina = oficina,
                TasaPtsBono = tasaPtsBono,
                Edad = edad,
                EdadAplica = request.edad_aplica,
                EdadJustificacion = request.edad_justificacion ?? string.Empty,
                Cph = cph,
                MontoConstruccion = request.monto_construccion,
                PolizaVehiculo = BoolToInt(request.poliza_vehiculo),
                MontoPolizaPrenda = request.monto_poliza_prenda,
                ValorPrenda = request.valor_prenda,
                ClasificacionCrediticia = request.clasificacion_crediticia ?? string.Empty,
                NoOpCrm = request.no_op_crm?.Trim() ?? string.Empty
            }) as IDictionary<string, object>;
            if (row is null)
            {
                return string.Empty;
            }

            var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
            return GetString(dict, "cod_preanalisis");
        }

        /// <summary>
        /// EXEC spCrdPreaPreanalisisModifica — FASE 2 del diccionario de campos
        /// (frmPreaEstudiov2_diccionario_campos.md): agrega los parámetros cuya fuente ya está
        /// confirmada en Angular (resumen/credito/salariosForm/observaciones), sin tocar los
        /// marcados ⚠️/❌/❓ en el diccionario (esos quedan para fases 3-5, requieren decisión
        /// o verificación adicional antes de mapear).
        ///
        /// El SP real (firma confirmada por el usuario, no adivinada) declara ~108
        /// parámetros con estos nombres exactos (p.ej. @COD_PREANALISIS, @USUARIO, @EXTRAS_FIJAS...);
        /// la versión anterior de este método usaba alias inventados (@Expediente, @Usuario...) que
        /// NO coinciden con ningún parámetro real del SP, así que cada llamada a "Modificar" (editar
        /// un expediente existente) fallaba en tiempo de ejecución con
        /// "Procedure expects parameter '@COD_PREANALISIS', which was not supplied" (capturado
        /// silenciosamente por el catch de Prea_frmPreaEstudiov2_Guardar). Ese fix + esta fase 2
        /// suman ~64 de los 108 parámetros del SP (ver diccionario para el detalle exacto).
        ///
        /// Pendiente real (fases 3-5 del diccionario): ~17 parámetros con ambigüedad de fuente o
        /// tipo (p.ej. @52 COD_GARANTIA vs @51 GARANTIA, @93 vs @94 póliza/valor vehículo, @43
        /// LIQUIDO_TOTAL con dos candidatos, conversiones int/numeric), ~21 sin ningún control
        /// migrado en Angular (cargas CCSS/asociación/FRAP, frecuencia de pago, tasa estresada,
        /// mejora de cuota, etc. — requieren decisión de negocio: ¿migrar UI o default
        /// documentado?), y 5 que necesitan verificarse contra el .frm antes de mapear
        /// (@6 FECHA_CREACION, @59 NSUB_EXP, @82 CODPRM_COMPAD, @103 REFUNDICIONES_MORA,
        /// @104 SALARIO_USURA). No se adivinan esos valores.
        ///
        /// También se detectó que @FIADORES y @CONTRATO (los alias @Fiadores/@Contrato previos)
        /// NO existen como tales en el SP real — se omiten aquí en vez de adivinar a qué parámetro
        /// corresponden (candidatos posibles no confirmados: GARANTIA_FND_CONTRATO para contrato).
        /// </summary>
        private static string GuardarModifica(
            IDbConnection connection,
            string usuario,
            FrmPreaEstudiov2GuardarRequest request,
            string notasCumplimiento)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@COD_PREANALISIS", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@TIPO_PREANALISIS", string.IsNullOrEmpty(request.tipo_preanalisis) ? "E" : request.tipo_preanalisis.Trim(), DbType.String);
            parameters.Add("@COD_PREANALISIS_REF", string.IsNullOrWhiteSpace(request.cod_preanalisis_ref) ? null : request.cod_preanalisis_ref.Trim(), DbType.String);
            parameters.Add("@USUARIO", usuario, DbType.String);
            parameters.Add("@CEDULA", request.cedula?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@NOMBRE", request.nombre?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@SEXO", request.sexo?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@FECHA_NACIMIENTO", request.fecha_nacimiento, DbType.Date);
            parameters.Add("@COD_LINEA", request.linea?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@COD_DESTINO", request.destino?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@GARANTIA", request.garantia?.Trim() ?? string.Empty, DbType.String);
            // Fase 3: @COD_GARANTIA = clsMensajes.COD_GARANTIA (columna COD_GARANTIA), NO el mismo
            // valor que @GARANTIA (cboGarantia.ItemData) — confirmado leyendo sbEstudio_Guarda_Modifica.
            parameters.Add("@COD_GARANTIA", string.IsNullOrWhiteSpace(request.cod_garantia_clasificacion) ? null : request.cod_garantia_clasificacion.Trim(), DbType.String);
            parameters.Add("@APL_POLIZA_VIDA", BoolToInt(request.poliza_vida), DbType.Int32);
            parameters.Add("@APL_POLIZA_INCENDIO", BoolToInt(request.poliza_incendio), DbType.Int32);
            parameters.Add("@APL_PRIMER_CUOTA", BoolToInt(request.primera_cuota), DbType.Int32);
            parameters.Add("@FECHA_CORTE_COLIILA", request.corte_colilla, DbType.Date);
            parameters.Add("@MONTO", request.monto, DbType.Decimal);
            parameters.Add("@TASA", request.tasa, DbType.Decimal);
            parameters.Add("@PLAZO", request.plazo, DbType.Int32);
            parameters.Add("@CUOTA", request.cuota, DbType.Decimal);
            parameters.Add("@MONTO_POLIZA_VIDA", request.monto_poliza_vida, DbType.Decimal);
            parameters.Add("@MONTO_POLIZA_INCENDIO", request.monto_poliza_incendio, DbType.Decimal);
            parameters.Add("@COMPROMISO", request.compromiso, DbType.Decimal);
            parameters.Add("@SALARIO_DEVENGADO_COLILLA", request.salario_devengado, DbType.Decimal);
            parameters.Add("@EXTRAS_FIJAS", request.componente_adicional_base, DbType.Decimal);
            parameters.Add("@SALARIO_CONSTANCIA", request.salario_constancia, DbType.Decimal);
            parameters.Add("@SALARIO_ORDEN_PATRONAL", request.salario_orden_patronal, DbType.Decimal);
            parameters.Add("@MONTO_ACT_PRIVADAS", request.ingreso_privado, DbType.Decimal);
            parameters.Add("@ID_COMPONENTE_AD", request.componente_adicional_id, DbType.Int32);
            parameters.Add("@PORCENTAJE_COMPONENTE_AD", request.componente_adicional_porc, DbType.Decimal);
            parameters.Add("@DEVENGADO_MES", request.salario_mensual, DbType.Decimal);
            parameters.Add("@MONTO_CONSTRUCCION", request.monto_construccion, DbType.Decimal);
            parameters.Add("@TIPO_SALARIO", request.tipo_salario?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@NUM_OPORT_CRM", string.IsNullOrWhiteSpace(request.no_op_crm) ? null : request.no_op_crm.Trim(), DbType.String);
            parameters.Add("@CUMPLIMIENTO_NOTAS", notasCumplimiento, DbType.String);

            // ---- Fase 2: parámetros con fuente ya confirmada en Angular (ver diccionario) ----
            parameters.Add("@REBAJO_EXTRAS", request.total_extras, DbType.Decimal);
            parameters.Add("@DEDUCCIONES", request.deducciones, DbType.Decimal);
            parameters.Add("@CRD_TRANSITO_CANCELADOS", request.creditos_cancelados, DbType.Decimal);
            parameters.Add("@CRD_TRANSITO_XCOBRAR", request.creditos_por_cobrar, DbType.Decimal);
            parameters.Add("@SALARIO_LIQUIDO", request.salario_liquido, DbType.Decimal);
            parameters.Add("@REFUNDICIONES", request.refundiciones, DbType.Decimal);
            parameters.Add("@REFUNDICIONES_CUOTA", request.refundiciones_cuota, DbType.Decimal);
            parameters.Add("@DESEMBOLSOS", request.desembolsos, DbType.Decimal);
            parameters.Add("@DESEMBOLSOS_CUOTA", request.desembolsos_cuota, DbType.Decimal);
            parameters.Add("@LIQUIDEZ_SIMPLE", request.liquidez_sin_fianzas, DbType.Decimal);
            parameters.Add("@FIANZAS", request.fianzas, DbType.Decimal);
            parameters.Add("@LIQUIDEZ_CFIANZAS", request.liquidez_con_fianzas, DbType.Decimal);
            parameters.Add("@OBSERVACION_ANALISTA", request.observacion_analista ?? string.Empty, DbType.String);
            parameters.Add("@OBSERVACION_COMITE", request.observacion_comite ?? string.Empty, DbType.String);
            parameters.Add("@OBSERVACION_JD", request.observacion_jd ?? string.Empty, DbType.String);
            parameters.Add("@COD_ENDEUDAMIENTO", request.cod_endeudamiento ?? string.Empty, DbType.String);
            parameters.Add("@COD_HISTORIAL", request.cod_historial ?? string.Empty, DbType.String);
            parameters.Add("@COD_MORA", request.cod_mora ?? string.Empty, DbType.String);
            parameters.Add("@COD_CAPACIDAD", request.cod_capacidad ?? string.Empty, DbType.String);
            parameters.Add("@SALARIO_REAL", request.salario_real, DbType.Decimal);
            parameters.Add("@GARANTIA_FND", string.IsNullOrWhiteSpace(request.garantia_fondo) ? null : request.garantia_fondo.Trim(), DbType.String);
            parameters.Add("@EDAD_PERSONA", CalcularEdad(request.fecha_nacimiento), DbType.Int32);
            parameters.Add("@PORC_LIQ_CON_FIANZA", request.liquidez_con_fianzas_porc, DbType.Decimal);
            parameters.Add("@PORC_LIQ_SIN_FIANZA", request.liquidez_sin_fianzas_porc, DbType.Decimal);
            parameters.Add("@MONTO_PORC_COMPONENTE_AD", request.componentes_adicionales, DbType.Decimal);
            parameters.Add("@PUNTOS_CIC_DEUDOR", request.cic_puntaje ?? string.Empty, DbType.String);
            parameters.Add("@NIVEL_COMPORTAMIENTO_HIST", request.cic_nivel_historico ?? string.Empty, DbType.String);
            parameters.Add("@DIAS_INTERES_GASTOS_OP", request.dias_interes_gastos_op, DbType.Decimal);
            parameters.Add("@PORC_LIQ_SIN_FIANZA_CA", request.liquidez_sin_fianzas_comp_porc, DbType.Decimal);
            parameters.Add("@PORC_LIQ_CON_FIANZA_CA", request.liquidez_con_fianzas_comp_porc, DbType.Decimal);
            parameters.Add("@LIQUIDEZ_SFIANZAS_CA", request.liquidez_sin_fianzas_comp, DbType.Decimal);
            parameters.Add("@LIQUIDEZ_CFIANZAS_CA", request.liquidez_con_fianzas_comp, DbType.Decimal);
            parameters.Add("@APLICA_POLIZA_VEHICULO", BoolToInt(request.poliza_vehiculo), DbType.Int16);
            parameters.Add("@MONTO_VALOR_VEHICULO", request.valor_prenda, DbType.Decimal);
            parameters.Add("@MONTO_INTERES", request.intereses, DbType.Decimal);
            parameters.Add("@MONTO_COMISION", request.comisiones, DbType.Decimal);
            parameters.Add("@APL_POLIZA_DESEMPLEO", BoolToInt(request.poliza_desempleo), DbType.Int16);
            parameters.Add("@MONTO_POLIZA_DESEMPLEO", request.monto_poliza_desempleo, DbType.Decimal);

            // ---- Fase 3: ambigüedades resueltas leyendo sbEstudio_Guarda_Modifica directamente ----
            parameters.Add("@MONTO_POLIZA_VEHICULO", request.monto_poliza_prenda_vehiculo, DbType.Decimal);
            parameters.Add("@LIQUIDO_TOTAL", request.total_liquido_persona, DbType.Decimal);
            // COD_OFICINA (#61) en VB6 = GLOBALES.gOficinaTitular (oficina del USUARIO logueado,
            // no del expediente) — mismo criterio que GuardarNuevo.
            parameters.Add("@COD_OFICINA", ObtenerOficinaTitular(connection, usuario), DbType.String);
            parameters.Add("@ID_SOLICITUD", int.TryParse(request.asignado_operacion, out var idSolicitud) ? idSolicitud : (int?)null, DbType.Int32);
            // GARANTIA_FND_CONTRATO (#63) = cboFondoContrato.ItemData cuando la garantía es de
            // Fondos ("Y" en VB6); Angular usa credito.contrato para ese mismo combo.
            parameters.Add("@GARANTIA_FND_CONTRATO", int.TryParse(request.contrato, out var garantiaFndContrato) ? garantiaFndContrato : (int?)null, DbType.Int32);
            parameters.Add("@COD_FORMULARIO_CPH", short.TryParse(request.cph, out var cph) ? cph : (short?)null, DbType.Int16);
            parameters.Add("@ID_PROMOTOR", decimal.TryParse(request.id_promotor, out var idPromotor) ? idPromotor : (decimal?)null, DbType.Decimal);
            parameters.Add("@ESTADO", request.estado?.Trim() ?? string.Empty, DbType.String);
            // INDICADOR_EDITABLE (#83) en VB6: Select Case lblEstado.Tag / "R","P" -> 1 / Else -> 0
            // (líneas 12239-12244) — se deriva de @ESTADO, no de un control aparte.
            var estadoTrim = request.estado?.Trim() ?? string.Empty;
            var indicadorEditable = estadoTrim == "R" || estadoTrim == "P" ? 1 : 0;
            parameters.Add("@INDICADOR_EDITABLE", indicadorEditable, DbType.Boolean);
            // COD_ESTADO_V2 (#86) en VB6 es un literal fijo "RECI" (línea 12237), no viene de
            // ningún control — se replica igual, no es un dato inventado.
            parameters.Add("@COD_ESTADO_V2", "RECI", DbType.String);
            parameters.Add("@SALARIO_NORMATIVA", request.salario_normativa, DbType.Decimal);
            // CODPRM_COMPAD (#82) y NUM_OPORT_CRM (#96) usan la MISMA fuente en VB6 (txtCRM.Text,
            // líneas 12289 y 12295) — mismo patrón que CATEGORIA_PERSONA/COD_CATEGORIA_ASOCIADO.
            parameters.Add("@CODPRM_COMPAD", string.IsNullOrWhiteSpace(request.no_op_crm) ? null : request.no_op_crm.Trim(), DbType.String);

            // ---- Fase 4: 11 parámetros sin control propio en Angular, resueltos leyendo
            // sbEstudio_Guarda_Modifica línea por línea (frmPreaEstudiov2.frm 12185-12298).
            // Los 5 primeros son literales fijos en VB6 (no vienen de ningún control) — se
            // replican igual, no son valores inventados.
            parameters.Add("@CONDICION_EMBARGO", (short)0, DbType.Int16); // VB6: literal "0" (línea 12287)
            parameters.Add("@UN_PAGO_X_MES", (short)1, DbType.Int16); // VB6: literal "1" (línea 12287)
            parameters.Add("@TASA_ESTRESADA", (short)0, DbType.Int16); // VB6: literal "0" (línea 12287)
            parameters.Add("@PTS_TASA_ESTRESADA", 0m, DbType.Decimal); // VB6: literal "0" (línea 12287)
            parameters.Add("@MONTO_LIQ_FIADOR_EXT", 0m, DbType.Decimal); // VB6: pMontoLiq_Fiador_Ext = 0 (línea 12236)
            // NSUB_EXP (#59) en VB6 = cboCantidadFiadores.Text (línea 12284) — confirmado que esta
            // posición recibe el conteo de fiadores, no un conteo real de sub-expedientes.
            parameters.Add("@NSUB_EXP", (short)request.fiadores, DbType.Int16);
            // APL_IND_COMPONENTE (#75) en VB6: 0 si el % del componente adicional es 0, si no 1
            // (líneas 12256-12260) — se deriva del mismo dato que ya viaja en @PORCENTAJE_COMPONENTE_AD.
            parameters.Add("@APL_IND_COMPONENTE", request.componente_adicional_porc == 0m ? 0 : 1, DbType.Int32);
            // IND_TIPO_SALARIO_EXT (#84) en VB6: default 0; 1 si chkS_Constancia; 2 si
            // chkS_OrdenPatronal pisa al anterior (líneas 12246-12253).
            var indTipoSalarioExt = 0;
            if (request.ind_salario_constancia) indTipoSalarioExt = 1;
            if (request.ind_salario_orden_patronal) indTipoSalarioExt = 2;
            parameters.Add("@IND_TIPO_SALARIO_EXT", indTipoSalarioExt, DbType.Int32);
            // PTS_EXTRA_FAP (#65) = txtFrapPorc.Text, control en vivo sin control equivalente en
            // Angular hoy; el SP lo declara con default NULL, así que se omite en vez de adivinar.
            parameters.Add("@PTS_EXTRA_FAP", (int?)null, DbType.Int32);
            // MONTO_MEJORA_CUOTA (#99) = txtCuotaDiferencia.Text, control en vivo sin equivalente
            // en Angular hoy; el SP lo exige sin default, así que se documenta en 0 (igual patrón
            // ya usado para monto_poliza_prenda). MEJORA_CUOTA (#102) se deriva EXACTO de ese mismo
            // valor en VB6 ("S" si < 0, si no "N", líneas 12230-12234): con 0 documentado, la regla
            // de VB6 da "N" de forma determinística, no es un valor inventado aparte.
            parameters.Add("@MONTO_MEJORA_CUOTA", 0m, DbType.Decimal);
            parameters.Add("@MEJORA_CUOTA", "N", DbType.String);

            // ---- Fase 5: últimos 3 params ❓ resueltos leyendo sbEstudio_Guarda_Modifica ----
            // FECHA_CREACION (#6) en VB6 es el literal "Null" (línea 12270) — no se reescribe al
            // modificar; el SP preserva/deriva la fecha de creación original server-side.
            parameters.Add("@FECHA_CREACION", (DateTime?)null, DbType.DateTime);
            parameters.Add("@REFUNDICIONES_MORA", request.refundiciones_mora, DbType.Decimal);
            // SALARIO_USURA (#104) reutiliza en VB6 el control del salario mínimo inembargable.
            parameters.Add("@SALARIO_USURA", request.salario_minimo_inembargable, DbType.Decimal);

            // ---- Fase 6: últimos 5 params, motor de cargas sociales (sbCalcula_Cargas) ----
            var (aplicaCargaAsociacion, aplicaCargaFrap) = ObtenerAplicaCargas(connection, request.cod_preanalisis?.Trim() ?? string.Empty);
            var cargas = CalcularCargas(connection, request.salario_mensual, aplicaCargaAsociacion, aplicaCargaFrap);
            parameters.Add("@TOTAL_CARGA_CCSS", cargas.TotalCargaCcss, DbType.Decimal);
            parameters.Add("@CARGA_CCSS", cargas.CargaCcss, DbType.Decimal);
            parameters.Add("@CARGA_ASOCIACION", cargas.CargaAsociacion, DbType.Decimal);
            parameters.Add("@CARGA_FRAP", cargas.CargaFrap, DbType.Decimal);
            parameters.Add("@CARGA_IMPUESTO_SALARIO", cargas.CargaImpuestoSalario, DbType.Decimal);

            var codPreanalisis = connection.QueryFirstOrDefault<string>(
                "spCrdPreaPreanalisisModifica",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return codPreanalisis ?? request.cod_preanalisis?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// VB6 (fxGuardar, solo para NuevoRegistro, línea ~12376): inicializa las tablas
        /// hijas del expediente recién creado.
        /// </summary>
        private static void InicializarTablasHijasNuevoExpediente(IDbConnection connection, string codPreanalisis)
        {
            var parameters = new { Expediente = codPreanalisis };

            try { connection.Execute("EXEC spCRDPreaRefundiciones @Expediente, 'I'", parameters); } catch { /* no bloqueante, igual que VB6 (solo MsgBox) */ }
            try { connection.Execute("EXEC spCRDPreaFianzas @Expediente, 'I'", parameters); } catch { /* no bloqueante */ }
            try { connection.Execute("EXEC spCRDPreaCreditosTransito @Expediente, 'I', 0", parameters); } catch { /* no bloqueante */ }

            // VB6: m_NumPagos por defecto es 2 (línea 15393); solo cambia tras cargar datos
            // de frecuencia de pago del socio, que Angular no rastrea todavía.
            try { connection.Execute("EXEC spCRDPreaImportCreditosVigentes @Expediente, 2", parameters); } catch { /* no bloqueante */ }
        }

        /// <summary>
        /// VB6 (fxGuardar, "FIX TEMPORAL DE COLUMNAS NUEVAS", línea ~12421-12432): se ejecuta
        /// siempre, tanto para nuevo como para modificar.
        /// </summary>
        private static void AplicarFixColumnasFinal(
            IDbConnection connection,
            string codPreanalisis,
            string notasCumplimiento,
            FrmPreaEstudiov2GuardarRequest request)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Notas", notasCumplimiento ?? string.Empty, DbType.String);
                parameters.Add("@MontoPolizaDesempleo", request.monto_poliza_desempleo, DbType.Decimal);
                parameters.Add("@AplPolizaDesempleo", BoolToInt(request.poliza_desempleo), DbType.Int32);
                parameters.Add("@Expediente", codPreanalisis, DbType.String);

                connection.Execute(
                    @"UPDATE CRD_PREA_PREANALISIS
                      SET CUMPLIMIENTO_NOTAS = @Notas,
                          MONTO_POLIZA_DESEMPLEO = @MontoPolizaDesempleo,
                          APL_POLIZA_DESEMPLEO = @AplPolizaDesempleo
                      WHERE cod_Preanalisis = @Expediente",
                    parameters);
            }
            catch
            {
                // No bloqueante, igual que VB6.
            }
        }

        /// <summary>EXEC sbSIFOficinasUsuario '&lt;usuario&gt;' -> campo Titular. VB6:
        /// GLOBALES.gOficinaTitular, asignado al iniciar sesión (mProGrX_Main.bas línea 2887).</summary>
        private static string ObtenerOficinaTitular(IDbConnection connection, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return string.Empty;
            }

            try
            {
                const string sql = "EXEC sbSIFOficinasUsuario @Usuario";
                var row = connection.QueryFirstOrDefault(sql, new { Usuario = usuario }) as IDictionary<string, object>;
                if (row is null)
                {
                    return string.Empty;
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return GetString(dict, "Titular");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static int CalcularEdad(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue)
            {
                return 0;
            }

            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Value.Year;
            if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
            {
                edad--;
            }

            return edad;
        }

        private static int BoolToInt(bool valor) => valor ? 1 : 0;

        private static string Dec(decimal valor) => valor.ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Borra un expediente. VB6 (sbBorrar, frmPreaEstudiov2.frm línea ~12087):
        ///   clsEntidad.tablaName = "spCRDPreaPREANALISIS"
        ///   clsEntidad.fxRemover(cod_preanalisis, cod_preanalisis_ref)
        ///   -&gt; EXEC spCRDPreaPREANALISIS_B '&lt;cod_preanalisis&gt;', &lt;cod_preanalisis_ref o Null&gt;
        /// Solo permitido si el expediente está en modo edición (VB6: m_ventanaEnModo =
        /// ModificarRegistro) — validación replicada en BL.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Borrar(
            int codEmpresa, string codPreanalisis, string codPreanalisisRef)
        {
            var result = new ErrorDto<FrmPreaEstudiov2GuardarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2GuardarResponse()
            };

            var expediente = (codPreanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(expediente))
            {
                result.Code = -1;
                result.Description = "Debe indicar el expediente a borrar.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCRDPreaPREANALISIS_B @Expediente, @ExpedienteRef";
                connection.Execute(sql, new
                {
                    Expediente = expediente,
                    ExpedienteRef = string.IsNullOrWhiteSpace(codPreanalisisRef)
                        ? null
                        : codPreanalisisRef.Trim()
                });

                result.Result.cod_preanalisis = expediente;
                result.Result.mensaje = "La información fue borrada correctamente.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}
