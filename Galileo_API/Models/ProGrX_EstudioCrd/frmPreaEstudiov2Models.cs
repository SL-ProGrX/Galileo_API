namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    #region Carga / Scroll

    public class FrmPreaEstudiov2CargaRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ScrollRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int scroll_code { get; set; }
    }

    public class FrmPreaEstudiov2ScrollResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2CargaResponse
    {
        public FrmPreaEstudiov2EstadoDto estado { get; set; } = new();
        public FrmPreaEstudiov2EncabezadoDto encabezado { get; set; } = new();
        public FrmPreaEstudiov2CreditoDto credito { get; set; } = new();
        public FrmPreaEstudiov2SalariosDto salarios { get; set; } = new();
        public FrmPreaEstudiov2ResumenDto resumen { get; set; } = new();
        /// <summary>
        /// Solo los combos en cascada que dependen de la línea del expediente
        /// (sbSTCargaCboDestinos / sbSTCargaCboGarantiav2). Los catálogos estáticos
        /// —líneas, tipos de salario, comités, deducciones, etiquetas, tipos de extra,
        /// fondos, oficinas, ejecutivos— ya no viajan aquí: VB6 los llena una sola vez en
        /// Form_Load, no en sbLigarDatos, y Angular los pide con
        /// Prea_frmPreaEstudiov2_Catalogos_Consultar al abrir la pantalla.
        /// </summary>
        public FrmPreaEstudiov2DestinosGarantiasResponse catalogos { get; set; } = new();

        /// <summary>
        /// Sub-expedientes (fiadores) del expediente principal. VB6 los recarga dentro de
        /// la misma carga (sbLlenarComboFiltrado sobre cboSubExpediente), así que viajan
        /// con la respuesta en vez de costar una segunda llamada HTTP.
        /// </summary>
        public List<string> sub_expedientes { get; set; } = [];

        /// <summary>
        /// Comité resolutivo asignado (VB6: rs!ID_COMITE, dentro del mismo
        /// recordset de spCRDPreaPREANALISIS_T).
        /// </summary>
        public string comite_resolutivo { get; set; } = string.Empty;

        /// <summary>
        /// Parámetros globales de la empresa (VB6: sbInicializaGlobales, lee
        /// CRD_PREA_PARAMETROS COD_PARAMETRO='17' y '22'). No son propios del
        /// expediente, son configuración general del sistema.
        /// </summary>
        public decimal salario_minimo_inembargable { get; set; }
        public decimal salario_normativa { get; set; }

        /// <summary>
        /// Edad Máxima Permitida (años), parámetro global de la empresa (VB6:
        /// GlobalEdadMaximaPermitidaHombre/Mujeres, CRD_PREA_PARAMETROS COD_PARAMETRO
        /// '01'/'02'). Usado para calcular txtPlMax.
        /// </summary>
        public int edad_maxima_hombres { get; set; }
        public int edad_maxima_mujeres { get; set; }
    }

    public class FrmPreaEstudiov2EstadoDto
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string estado_v2 { get; set; } = string.Empty;
        public string estado_v2_desc { get; set; } = string.Empty;
        public bool editable { get; set; }
        public bool tiene_alerta { get; set; }
        public string mensaje_alerta { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2EncabezadoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public int edad { get; set; }
        public string estado_persona { get; set; } = string.Empty;
        public string clasificacion_crediticia { get; set; } = string.Empty;
        public int edad_aplica { get; set; }
        public string edad_justificacion { get; set; } = string.Empty;

        /// <summary>txtObservaciones (optObservacion(0), CRD_PREA_PREANALISIS.OBSERVACION_ANALISTA).</summary>
        public string observacion_analista { get; set; } = string.Empty;
        /// <summary>txtObservaciones (optObservacion(1), CRD_PREA_PREANALISIS.OBSERVACION_COMITE).</summary>
        public string observacion_comite { get; set; } = string.Empty;
        /// <summary>txtObservaciones (optObservacion(2), CRD_PREA_PREANALISIS.OBSERVACION_JD).</summary>
        public string observacion_jd { get; set; } = string.Empty;

        /// <summary>lblRegistro(0) en VB6: "R-US: " &amp; !Usuario (frmPreaEstudiov2.frm ~línea 10707).</summary>
        public string registro_usuario { get; set; } = string.Empty;
        /// <summary>lblRegistro(1) en VB6: "R-FE: " &amp; Format(!FECHA_CREACION, "dd-mm-yyyy") (~línea 10708).</summary>
        public DateTime? registro_fecha { get; set; }
    }

    /// <summary>
    /// VB6: sbCalcularCuota / chkPolizaX_Click (frmPreaEstudiov2.frm). Recalcula
    /// Cuota, montos de pólizas y Compromiso cuando el usuario cambia Monto/Plazo/
    /// Tasa/Monto Construcción o marca/desmarca una póliza.
    /// </summary>
    public class FrmPreaEstudiov2CreditoRecalcularRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int plazo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal tasa { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_construccion { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_vida { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_incendio { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_prenda { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_desempleo { get; set; }

        /// <summary>
        /// "monto" cuando el disparo equivale al Case "txtMonto" de VB6 (sbCalcularCuota,
        /// frmPreaEstudiov2.frm ~línea 14879) — el único caso que re-deriva Tasa/Plazo desde
        /// el catálogo (dbo.fxCrdCatalogoRango) y aplica bono de membresía. Cualquier otro
        /// valor (o vacío) se comporta como los casos "txtPlazo"/"txtTasa": solo recalcula
        /// Cuota/Pólizas/Compromiso con los valores tal cual llegan, sin tocar Tasa/Plazo.
        /// </summary>
        public string origen { get; set; } = string.Empty;

        public string linea { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;

        /// <summary>Estado del expediente (columna Estado, un carácter). El bono de
        /// membresía solo aplica cuando es 'R' o 'P' (VB6: sbCalcularCuota, línea 14895).</summary>
        public string estado { get; set; } = string.Empty;
    }

    /// <summary>Resultado del recálculo: Tasa/Plazo (si aplicó catálogo/bono), Cuota,
    /// montos de pólizas y Compromiso.</summary>
    public class FrmPreaEstudiov2CreditoRecalculoResponse
    {
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal cuota { get; set; }
        public decimal compromiso { get; set; }
        public decimal monto_poliza_vida { get; set; }
        public decimal monto_poliza_incendio { get; set; }
        public decimal monto_poliza_prenda { get; set; }
        public decimal monto_poliza_desempleo { get; set; }
        /// <summary>true si Póliza de Incendio se marcó automáticamente por tener
        /// Monto Construcción &gt; 0 (VB6: sbCalculaPolizaDeIncendio ~línea 14999).</summary>
        public bool poliza_incendio { get; set; }
        /// <summary>Puntos de bono de membresía aplicados a la Tasa (0 si no aplicó).
        /// VB6: clsMensajes.TASA_PTS_BONO.</summary>
        public decimal tasa_pts_bono { get; set; }
    }

    public class FrmPreaEstudiov2CreditoDto
    {
        public string linea { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public int fiadores { get; set; }
        public string contrato { get; set; } = string.Empty;
        public string no_op_crm { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        /// <summary>mFrecuenciaPago en VB6. Se carga desde Instituciones.Frecuencia
        /// por cédula del socio; si no existe, usa "M".</summary>
        public string frecuencia_pago { get; set; } = "M";
        public decimal cuota { get; set; }
        public decimal monto_construccion { get; set; }
        public bool poliza_vida { get; set; }
        public bool poliza_incendio { get; set; }
        public bool poliza_prenda { get; set; }
        public bool poliza_desempleo { get; set; }
        public bool primera_cuota { get; set; }
        public decimal monto_poliza_vida { get; set; }
        public decimal monto_poliza_incendio { get; set; }
        public decimal monto_poliza_prenda { get; set; }
        public decimal monto_poliza_desempleo { get; set; }
        public decimal compromiso { get; set; }
        public string asignado_operacion { get; set; } = string.Empty;

        /// <summary>cboCPH en VB6 (COD_FORMULARIO_CPH). No tiene catálogo propio —
        /// sbCboAsignaDato solo restaura el valor ya guardado, sin llenar una lista
        /// de opciones en ningún punto del .frm. Se expone como valor simple.</summary>
        public string cph { get; set; } = string.Empty;

        /// <summary>txtPrendaValor en VB6 (columna MONTO_VALOR_VEHICULO). Distinto de
        /// monto_poliza_prenda (que en realidad corresponde a APL_POLIZA_VEHICULO /
        /// Monto_Poliza_Vehiculo — el nombreo heredado en este DTO es engañoso, ver
        /// nota en Prea_frmPreaEstudiov2_Cargar; no se renombra en este pase para no
        /// romper otros consumidores, queda documentado para la pestaña de Garantía).</summary>
        public decimal valor_prenda { get; set; }

        /// <summary>txtEjecutivo en VB6 (Id_Promotor / PromotorDesc). Restaurado en
        /// sbLigarDatos: "[" &amp; Id_Promotor &amp; "] .. " &amp; PromotorDesc.</summary>
        public string id_promotor { get; set; } = string.Empty;
        public string promotor_desc { get; set; } = string.Empty;

        /// <summary>txtOficina en VB6. Texto = OFICINA (descripción de SIF_OFICINAS,
        /// sbLigarDatos frm ~10713); Tag = COD_OFICINA (código, se reenvía al guardar
        /// y en btnOficinaCambia_Click vía spCrdPreaAsignaOficina).</summary>
        public string oficina { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;

        /// <summary>txtCFIA_Avaluo en VB6 (columna MONTO_AVALUO_CFIA).</summary>
        public decimal monto_avaluo_cfia { get; set; }

        /// <summary>txtDiasIntereses en VB6 (columna DIAS_INTERES_GASTOS_OP). Se
        /// pasa directo sin cálculo en VB6. Solo lectura por ahora — el guardado
        /// vive dentro de los ~60 parámetros posicionales de spCrdPreaPreanalisisModifica
        /// que aún no se han auditado uno a uno (ver frmPreaEstudiov2DB.guardar.cs).</summary>
        public int dias_interes_gastos_op { get; set; }

        /// <summary>
        /// COD_CAPACIDAD/ENDEUDAMIENTO/GARANTIA/HISTORIAL/MORA. VB6 (sbLigarDatos,
        /// frmPreaEstudiov2.frm ~línea 10671-10675): se leen del recordset hacia
        /// clsMensajes. LigarDatosClasificacion (~línea 12041-12071), que en teoría los
        /// recalcularía desde una grilla visual (vGrid), es código muerto en el VB6 actual:
        /// sbClasificacion_CargaGrid (~línea 18602-18627) tiene la llamada que llena vGrid
        /// comentada y termina con "vGrid.Visible = False", por lo que vGrid.MaxRows nunca
        /// se establece y el bucle de LigarDatosClasificacion no itera. En la práctica estos
        /// 5 campos son puro passthrough: se cargan y se reenvían sin cambios al guardar
        /// (sbEstudio_Guarda_Modifica, ~línea 12219-12283, params pCal_Capacidad/
        /// Endeudamiento/Garantia/Historial/Mora). Se exponen aquí solo de lectura — el
        /// guardado, igual que dias_interes_gastos_op, depende de auditar el signature real
        /// de spCrdPreaPreanalisisModifica antes de reenviarlos sin adivinar nombres de
        /// parámetro.
        /// </summary>
        public string cod_capacidad { get; set; } = string.Empty;
        public string cod_endeudamiento { get; set; } = string.Empty;
        public string cod_garantia_clasificacion { get; set; } = string.Empty;
        public string cod_historial { get; set; } = string.Empty;
        public string cod_mora { get; set; } = string.Empty;

        /// <summary>
        /// txtCIC_Puntaje en VB6 (columna PUNTOS_CIC_DEUDOR). El tooltip "Presione F4
        /// para Consultar" está heredado de la plantilla de control (XtremeSuiteControls.FlatEdit)
        /// pero no tiene ningún KeyDown handler propio en frmPreaEstudiov2.frm — no hay
        /// grep de "txtCIC_Puntaje_KeyDown" ni SP de consulta CIC en este .frm. Es texto
        /// libre, pasa directo a sbEstudio_Guarda_Modifica (línea ~12289).
        /// </summary>
        public string cic_puntaje { get; set; } = string.Empty;

        /// <summary>txtCIC_NivelHistorico en VB6 (columna NIVEL_COMPORTAMIENTO_HIST).
        /// Mismo patrón que cic_puntaje: texto libre, sin consulta F4 real.</summary>
        public string cic_nivel_historico { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2SalariosDto
    {
        public string tipo_salario { get; set; } = string.Empty;
        public DateTime? corte_colilla { get; set; }
        public decimal salario_devengado { get; set; }
        public decimal salario_mensual { get; set; }
        public decimal salario_constancia { get; set; }
        public decimal salario_orden_patronal { get; set; }
        public decimal ingreso_privado { get; set; }
        public decimal ingreso_privado_porc { get; set; }
        public int componente_adicional_id { get; set; }
        public decimal componente_adicional_porc { get; set; }
        /// <summary>txtS_ComponenteAdicional en VB6 (EXTRAS_FIJAS): base editable, txtCompAdicional = base * porc / 100.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal componente_adicional_base { get; set; }
        public decimal componentes_adicionales { get; set; }
        public decimal total_extras { get; set; }
        public List<FrmPreaEstudiov2SalarioDetalleDto> tabla_salarios { get; set; } = [];
        public List<FrmPreaEstudiov2ExtraDto> extras { get; set; } = [];
        public List<FrmPreaEstudiov2IncapacidadDto> incapacidades { get; set; } = [];
    }

    public class FrmPreaEstudiov2SalarioDetalleDto
    {
        public int orden { get; set; }
        public DateTime? fecha { get; set; }
        public decimal salario_s { get; set; }
        public int mes { get; set; }
        public decimal salario_rh { get; set; }
        public decimal ca { get; set; }
    }

    public class FrmPreaEstudiov2ExtraDto
    {
        public int idx { get; set; }
        public string cod_extras { get; set; } = string.Empty;
        public string tipo_extra { get; set; } = string.Empty;
        public decimal monto { get; set; }
    }

    public class FrmPreaEstudiov2IncapacidadDto
    {
        public int orden { get; set; }
        public DateTime? desde { get; set; }
        public DateTime? hasta { get; set; }
        public int dias { get; set; }
    }

    /// <summary>
    /// Pestaña "Resumen" (tcMain.Item(6)). Todos los campos vienen directamente del
    /// mismo recordset de spCRDPreaPREANALISIS_T usado para cargar el resto del
    /// formulario (frmPreaEstudiov2.frm, sbLigarDatos ~línea 10788-10963), salvo
    /// donde se indique lo contrario. No existe SP propio para esta pestaña.
    /// </summary>
    public class FrmPreaEstudiov2ResumenDto
    {
        /// <summary>rs!SALARIO_REAL.</summary>
        public decimal salario_real { get; set; }
        /// <summary>rs!TOTAL_CARGA_CCSS (txtTotal_Cargas_CCSS).</summary>
        public decimal cargas { get; set; }
        /// <summary>rs!CARGA_CCSS (lblCargaCCSS).</summary>
        public decimal carga_ccss { get; set; }
        /// <summary>rs!CARGA_ASOCIACION (lblCargaAsociacion).</summary>
        public decimal carga_asociacion { get; set; }
        /// <summary>rs!CARGA_FRAP (lblCargaFrap).</summary>
        public decimal carga_frap { get; set; }
        /// <summary>rs!CARGA_IMPUESTO_SALARIO (lblCargaImpSalario).</summary>
        public decimal carga_impuesto_salario { get; set; }
        /// <summary>rs!PTS_EXTRA_FRAP (txtFrapPorc).</summary>
        public decimal pts_extra_frap { get; set; }
        /// <summary>CRD_PREA_PARAMETROS '09' (GlobalPorcFRAPFAP), usado por el recálculo local.</summary>
        public decimal porc_frap_fap { get; set; }
        /// <summary>chkCargaAsociacion.Tag = "S" cuando CARGA_ASOCIACION es mayor a 0.</summary>
        public bool aplica_carga_asociacion { get; set; }
        /// <summary>chkCargaFrap.Tag = "S" cuando CARGA_FRAP es mayor a 0.</summary>
        public bool aplica_carga_frap { get; set; }
        /// <summary>rs!PORCENTAJE_LIBRE (txtPorcSobreSalario).</summary>
        public decimal porc_sobre_salario { get; set; }
        /// <summary>rs!DEDUCCIONES.</summary>
        public decimal deducciones { get; set; }
        /// <summary>rs!CRD_TRANSITO_CANCELADOS.</summary>
        public decimal creditos_cancelados { get; set; }
        /// <summary>rs!CRD_TRANSITO_XCOBRAR.</summary>
        public decimal creditos_por_cobrar { get; set; }
        /// <summary>rs!SALARIO_LIQUIDO.</summary>
        public decimal salario_liquido { get; set; }
        /// <summary>rs!REFUNDICIONES.</summary>
        public decimal refundiciones { get; set; }
        /// <summary>rs!REFUNDICIONES_CUOTA (txtRefundiciones.ToolTipText en VB6).</summary>
        public decimal refundiciones_cuota { get; set; }
        /// <summary>rs!DESEMBOLSOS.</summary>
        public decimal desembolsos { get; set; }
        /// <summary>rs!DESEMBOLSOS_CUOTA (txtDesembolsos.ToolTipText en VB6).</summary>
        public decimal desembolsos_cuota { get; set; }
        /// <summary>rs!LIQUIDO_TOTAL (txtTotalLiquido).</summary>
        public decimal total_liquido_persona { get; set; }
        /// <summary>rs!LIQUIDO_TOTAL_GRUPO.</summary>
        public decimal total_liquido_grupo { get; set; }
        /// <summary>rs!FIANZAS.</summary>
        public decimal fianzas { get; set; }
        /// <summary>rs!MONTO_COMISION (txtComisiones).</summary>
        public decimal comisiones { get; set; }
        /// <summary>rs!Monto_Interes (txtIntereses).</summary>
        public decimal intereses { get; set; }
        /// <summary>txtPSD = Monto * GlobalPorcPSD / 100, solo para expediente principal.</summary>
        public decimal psd { get; set; }
        /// <summary>txtMontoGirar = Monto menos refundiciones, desembolsos, P.S.D.,
        /// intereses, comisiones y primera cuota cuando aplica.</summary>
        public decimal monto_girar { get; set; }
        /// <summary>
        /// txtCuotaDiferencia = Abs(Cuota - (REFUNDICIONES_CUOTA + DESEMBOLSOS_CUOTA)).
        /// Calculado aquí porque en VB6 no viene como columna propia, se deriva de
        /// otras dos que sí son columnas reales (frmPreaEstudiov2.frm línea ~10120).
        /// </summary>
        public decimal diferencia_cuota { get; set; }

        /// <summary>rs!SALARIO_USURA (txtSalarioMinInembargableEstudio). Valor propio
        /// del expediente — distinto del parámetro global salario_minimo_inembargable.</summary>
        public decimal salario_minimo_estudio { get; set; }
        /// <summary>rs!SALARIO_NORMATIVA (txtSalarioNormativaEstudio). Valor propio
        /// del expediente — distinto del parámetro global salario_normativa.</summary>
        public decimal salario_normativa_estudio { get; set; }

        /// <summary>rs!LIQUIDEZ_SIMPLE.</summary>
        public decimal liquidez_sin_fianzas { get; set; }
        /// <summary>rs!PORC_LIQ_SIN_FIANZA.</summary>
        public decimal liquidez_sin_fianzas_porc { get; set; }
        /// <summary>rs!LIQUIDEZ_CFIANZAS.</summary>
        public decimal liquidez_con_fianzas { get; set; }
        /// <summary>rs!PORC_LIQ_CON_FIANZA.</summary>
        public decimal liquidez_con_fianzas_porc { get; set; }
        /// <summary>rs!LIQUIDEZ_SFIANZAS_CA.</summary>
        public decimal liquidez_sin_fianzas_comp { get; set; }
        /// <summary>rs!PORC_LIQ_SIN_FIANZA_CA.</summary>
        public decimal liquidez_sin_fianzas_comp_porc { get; set; }
        /// <summary>rs!LIQUIDEZ_CFIANZAS_CA.</summary>
        public decimal liquidez_con_fianzas_comp { get; set; }
        /// <summary>rs!PORC_LIQ_CON_FIANZA_CA.</summary>
        public decimal liquidez_con_fianzas_comp_porc { get; set; }
    }

    /// <summary>
    /// Catálogos que NO dependen del expediente ni de la línea. VB6 los carga una sola vez
    /// en Form_Load (sbCargarCombos / sbCargaCboComites / cboFondo / lookups F4); antes se
    /// reconsultaban en cada carga de expediente.
    /// </summary>
    public class FrmPreaEstudiov2CatalogosResponse
    {
        public List<FrmPreaEstudiov2DropdownDto> lineas { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> tipos_id { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> divisas { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> tipos_documento { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> tipos_salario { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> componentes_adicionales { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> comites { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> deducciones { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> etiquetas { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> tipos_extra { get; set; } = [];

        /// <summary>cboFondo en VB6 (Form_Load, línea ~11460-11462): EXEC spCRDGarantiaFND,
        /// cargado una sola vez al abrir el formulario (catálogo estático, no depende de
        /// cédula ni línea). Columnas verificadas por convención de sbCbo_Llena_New: el SP
        /// debe devolver IdX/ItmX (mProGrX_Dlls.bas línea ~1563).</summary>
        public List<FrmPreaEstudiov2DropdownDto> fondos { get; set; } = [];

        /// <summary>Oficinas para el lookup del botón Cambiar (btnOficinaCambia).
        /// VB6: txtOficina_KeyDown (F4) -> select Cod_Oficina, Descripcion from
        /// SIF_OFICINAS + " and Estado = 1", orden por Descripcion.</summary>
        public List<FrmPreaEstudiov2DropdownDto> oficinas { get; set; } = [];

        /// <summary>Ejecutivos colocadores (promotores) para el lookup del botón Cambiar.
        /// VB6: txtEjecutivo_KeyDown (F4) -> select ID_PROMOTOR as 'Id.', Nombre, Usuario
        /// from promotores + " and Estado = 1", orden por ID_PROMOTOR.</summary>
        public List<FrmPreaEstudiov2EjecutivoDto> ejecutivos { get; set; } = [];
    }

    /// <summary>
    /// Ejecutivo colocador para el lookup del botón Cambiar (btnOficinaCambia).
    /// VB6: txtEjecutivo_KeyDown (F4) -> "select ID_PROMOTOR as 'Id.',Nombre, Usuario
    /// from promotores" con Filtro " and Estado = 1", Columna/Orden = ID_PROMOTOR.
    /// </summary>
    public class FrmPreaEstudiov2EjecutivoDto
    {
        public string id_promotor { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2DestinosGarantiasResponse
    {
        public List<FrmPreaEstudiov2DropdownDto> destinos { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> garantias { get; set; } = [];
    }

    /// <summary>
    /// VB6: cboGarantia_Click (frmPreaEstudiov2.frm ~línea 14292-14355), casos F01 (Sobre
    /// Ahorros: dbo.fxCrdGarantiaPatMnt) y F06 (Adelanto de Salario:
    /// dbo.fxCrdDisponibleAdelantoSalario_Estudio). Los demás formularios no calculan monto
    /// aquí (F02/F03/F07 no tocan txtMonto; F05 se resuelve aparte vía cboFondo).
    /// </summary>
    public class FrmPreaEstudiov2GarantiaMontoRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string formulario { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2GarantiaMontoResponse
    {
        public decimal monto { get; set; }
    }

    /// <summary>
    /// VB6: cboFondo_Click / cboFondoContrato_Click (frmPreaEstudiov2.frm ~línea 14121-14235).
    /// cod_contrato es opcional: cuando se omite (cambio de Fondo), además de recalcular
    /// también se reconstruye la lista de contratos (fnd_contratos) y se preselecciona el
    /// primero, igual que VB6. Cuando se envía (cambio de Contrato), solo se recalcula.
    /// </summary>
    public class FrmPreaEstudiov2FondoCalcularRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string cod_fondo { get; set; } = string.Empty;
        public string? cod_contrato { get; set; }
    }

    public class FrmPreaEstudiov2FondoCalcularResponse
    {
        public decimal monto { get; set; }
        public bool aplica_tasa { get; set; }
        public decimal tasa { get; set; }
        public bool aplica_plazo { get; set; }
        public int plazo { get; set; }
        /// <summary>Solo se llena cuando la request no trae cod_contrato (cambio de Fondo).</summary>
        public List<FrmPreaEstudiov2DropdownDto> contratos { get; set; } = [];
        /// <summary>cod_contrato preseleccionado (primero de la lista), igual que VB6.</summary>
        public string cod_contrato_seleccionado { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sub-expedientes (fiadores) ligados a un expediente principal.
    /// VB6: cboSubExpediente, poblado con clsEntidad.fxTraerFiltrado("SubExpediente", padre)
    /// -&gt; EXEC spCRDPreaPREANALISIS_TXSubExpediente '&lt;padre&gt;'.
    /// No incluye al expediente principal (ese se agrega en el propio Angular).
    /// </summary>
    public class FrmPreaEstudiov2SubExpedientesResponse
    {
        public List<string> sub_expedientes { get; set; } = [];
    }

    /// <summary>
    /// Genera/valida el número de un nuevo sub-expediente (fiador) para un expediente
    /// dado. VB6: EXEC spCrd_Prea_Expediente_Numero '&lt;expediente&gt;', 'S'.
    /// </summary>
    public class FrmPreaEstudiov2SubExpedienteGenerarResponse
    {
        public string expediente { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public bool exito { get; set; }
    }

    public class FrmPreaEstudiov2DropdownDto
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        /// <summary>Solo poblado para el catálogo de garantías (CRD_GARANTIA_TIPOS.FORMULARIO,
        /// F01-F07). VB6: cboGarantia_Click consulta esto por separado; aquí se trae junto
        /// con el resto del catálogo para evitar un roundtrip extra.</summary>
        public string? formulario { get; set; }

        /// <summary>Solo poblado para el catálogo de tipos de salario.
        /// VB6: frmPreaEstudiov2.frm línea 10466 — indica si el campo Base (EXTRAS_FIJAS)
        /// es editable (1) o solo lectura (0).</summary>
        public bool? modifica_extras_fijas { get; set; }
    }

    #endregion

    #region Guardar

    public class FrmPreaEstudiov2GuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public string linea { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int fiadores { get; set; }
        public string contrato { get; set; } = string.Empty;
        public string no_op_crm { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal tasa { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int plazo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal cuota { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_construccion { get; set; }
        public string tipo_salario { get; set; } = string.Empty;
        public DateTime? corte_colilla { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_devengado { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_mensual { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_constancia { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_orden_patronal { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal ingreso_privado { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal ingreso_privado_porc { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int componente_adicional_id { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal componente_adicional_porc { get; set; }
        /// <summary>txtS_ComponenteAdicional en VB6 (SP: @EXTRAS_FIJAS).</summary>
        public decimal componente_adicional_base { get; set; } = 0;
        public string notas_cumplimiento { get; set; } = string.Empty;

        /// <summary>'E' expediente principal, 'S' sub-expediente (fiador). VB6:
        /// fxValidaDatos, línea ~11648 (fxSelectItemSubExpediente).</summary>
        public string tipo_preanalisis { get; set; } = "E";

        /// <summary>Código del expediente padre cuando tipo_preanalisis = 'S'.
        /// VB6: clsMensajes.cod_preanalisis_ref.</summary>
        public string cod_preanalisis_ref { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_vida { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_incendio { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_vehiculo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool poliza_desempleo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool primera_cuota { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_poliza_vida { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_poliza_incendio { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_poliza_vehiculo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_poliza_prenda { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_poliza_desempleo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal valor_prenda { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal compromiso { get; set; }
        public string cph { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int edad_aplica { get; set; }
        public string edad_justificacion { get; set; } = string.Empty;
        public string clasificacion_crediticia { get; set; } = string.Empty;

        // ---- Fase 2 del diccionario de campos (frmPreaEstudiov2_diccionario_campos.md):
        // parámetros de spCrdPreaPreanalisisModifica con fuente YA confirmada en Angular
        // (resumen/credito/salariosForm/observaciones), agregados para dejar de mandarlos
        // en 0/vacío. Los marcados ⚠️/❌/❓ en el diccionario quedan para fases siguientes.
        /// <summary>@27 REBAJO_EXTRAS (txtT_Extras). VB6: salariosForm.total_extras.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal total_extras { get; set; }
        /// <summary>@35 DEDUCCIONES. Fuente: resumen.deducciones (solo lectura).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal deducciones { get; set; }
        /// <summary>@36 CRD_TRANSITO_CANCELADOS. Fuente: resumen.creditos_cancelados.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal creditos_cancelados { get; set; }
        /// <summary>@37 CRD_TRANSITO_XCOBRAR. Fuente: resumen.creditos_por_cobrar.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal creditos_por_cobrar { get; set; }
        /// <summary>@38 SALARIO_LIQUIDO. Fuente: resumen.salario_liquido.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_liquido { get; set; }
        /// <summary>@39 REFUNDICIONES. Fuente: resumen.refundiciones.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal refundiciones { get; set; }
        /// <summary>@40 REFUNDICIONES_CUOTA. Fuente: resumen.refundiciones_cuota.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal refundiciones_cuota { get; set; }
        /// <summary>@41 DESEMBOLSOS. Fuente: resumen.desembolsos.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal desembolsos { get; set; }
        /// <summary>@42 DESEMBOLSOS_CUOTA. Fuente: resumen.desembolsos_cuota.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal desembolsos_cuota { get; set; }
        /// <summary>@44 LIQUIDEZ_SIMPLE. Fuente: resumen.liquidez_sin_fianzas.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_sin_fianzas { get; set; }
        /// <summary>@45 FIANZAS. Fuente: resumen.fianzas.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal fianzas { get; set; }
        /// <summary>@46 LIQUIDEZ_CFIANZAS. Fuente: resumen.liquidez_con_fianzas.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_con_fianzas { get; set; }
        /// <summary>@47 OBSERVACION_ANALISTA. Fuente: this.observacionAnalista.</summary>
        public string observacion_analista { get; set; } = string.Empty;
        /// <summary>@48 OBSERVACION_COMITE. Fuente: this.observacionComite.</summary>
        public string observacion_comite { get; set; } = string.Empty;
        /// <summary>@49 OBSERVACION_JD. Fuente: this.observacionJd.</summary>
        public string observacion_jd { get; set; } = string.Empty;
        /// <summary>@53 COD_ENDEUDAMIENTO. Fuente: credito.cod_endeudamiento.</summary>
        public string cod_endeudamiento { get; set; } = string.Empty;
        /// <summary>@54 COD_HISTORIAL. Fuente: credito.cod_historial.</summary>
        public string cod_historial { get; set; } = string.Empty;
        /// <summary>@55 COD_MORA. Fuente: credito.cod_mora.</summary>
        public string cod_mora { get; set; } = string.Empty;
        /// <summary>@56 COD_CAPACIDAD. Fuente: credito.cod_capacidad.</summary>
        public string cod_capacidad { get; set; } = string.Empty;
        /// <summary>@57 SALARIO_REAL (default NULL en el SP). Fuente: resumen.salario_real.</summary>
        public decimal? salario_real { get; set; }
        /// <summary>@60 GARANTIA_FND (default NULL). Fuente: this.fondoSeleccionado.</summary>
        public string? garantia_fondo { get; set; }
        /// <summary>@69 PORC_LIQ_CON_FIANZA. Fuente: resumen.liquidez_con_fianzas_porc.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_con_fianzas_porc { get; set; }
        /// <summary>@70 PORC_LIQ_SIN_FIANZA. Fuente: resumen.liquidez_sin_fianzas_porc.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_sin_fianzas_porc { get; set; }
        /// <summary>@74 MONTO_PORC_COMPONENTE_AD. Fuente: salariosForm.componentes_adicionales (base * porc / 100, ya calculado en Cargar).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal componentes_adicionales { get; set; }
        /// <summary>@78 PUNTOS_CIC_DEUDOR. Fuente: credito.cic_puntaje.</summary>
        public string cic_puntaje { get; set; } = string.Empty;
        /// <summary>@79 NIVEL_COMPORTAMIENTO_HIST. Fuente: credito.cic_nivel_historico.</summary>
        public string cic_nivel_historico { get; set; } = string.Empty;
        /// <summary>@81 DIAS_INTERES_GASTOS_OP. Fuente: credito.dias_interes_gastos_op.</summary>
        public decimal? dias_interes_gastos_op { get; set; }
        /// <summary>@87 PORC_LIQ_SIN_FIANZA_CA. Fuente: resumen.liquidez_sin_fianzas_comp_porc.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_sin_fianzas_comp_porc { get; set; }
        /// <summary>@88 PORC_LIQ_CON_FIANZA_CA. Fuente: resumen.liquidez_con_fianzas_comp_porc.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_con_fianzas_comp_porc { get; set; }
        /// <summary>@89 LIQUIDEZ_SFIANZAS_CA. Fuente: resumen.liquidez_sin_fianzas_comp.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_sin_fianzas_comp { get; set; }
        /// <summary>@90 LIQUIDEZ_CFIANZAS_CA. Fuente: resumen.liquidez_con_fianzas_comp.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal liquidez_con_fianzas_comp { get; set; }
        /// <summary>@94 MONTO_VALOR_VEHICULO. Ya existe como valor_prenda arriba (txtPrendaValor) — no duplicar.</summary>
        /// <summary>@100 MONTO_INTERES. Fuente: resumen.intereses.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal intereses { get; set; }
        /// <summary>@101 MONTO_COMISION. Fuente: resumen.comisiones.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal comisiones { get; set; }

        // ---- Fase 3 del diccionario: ambigüedades resueltas leyendo sbEstudio_Guarda_Modifica
        // (frmPreaEstudiov2.frm líneas 12185-12298) directamente ----
        /// <summary>@52 COD_GARANTIA = clsMensajes.COD_GARANTIA (columna COD_GARANTIA cargada en
        /// sbLigarDatos, línea 10673) — es DISTINTO de @51 GARANTIA (cboGarantia.ItemData).
        /// Fuente Angular: credito.cod_garantia_clasificacion (ya mapeado 1:1 desde la misma
        /// columna COD_GARANTIA en ConstruirCredito).</summary>
        public string cod_garantia_clasificacion { get; set; } = string.Empty;
        /// <summary>@93 MONTO_POLIZA_VEHICULO = txtPolizaPrenda.Text (distinto de @94
        /// MONTO_VALOR_VEHICULO = txtPrendaValor.Text = valor_prenda). Sin control propio en
        /// Angular todavía — se envía 0 documentado, no inventado (mismo campo que ya existía
        /// como monto_poliza_prenda, hardcodeado a 0 en el request).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto_poliza_prenda_vehiculo { get; set; }
        /// <summary>@25 ID_SOLICITUD = txtAsignado.Text. Fuente Angular: credito.asignado_operacion
        /// (ya mapeado 1:1 desde la columna ID_SOLICITUD en ConstruirCredito).</summary>
        public string asignado_operacion { get; set; } = string.Empty;
        /// <summary>@97 ID_PROMOTOR = txtEjecutivo.Tag. Fuente Angular: credito.id_promotor.</summary>
        public string id_promotor { get; set; } = string.Empty;
        /// <summary>@24 ESTADO = lblEstado.Tag. Fuente Angular: this.estadoCodigo.</summary>
        public string estado { get; set; } = string.Empty;
        /// <summary>@105 SALARIO_NORMATIVA = txtSalarioNormativa.Text (CRD_PREA_PARAMETROS '22').
        /// Fuente Angular: this.salarioNormativa (property de página, NO resumen.salario_normativa_estudio,
        /// que es un campo distinto).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_normativa { get; set; }
        /// <summary>@43 LIQUIDO_TOTAL = txtTotalLiquido.Text, cargado desde la columna LIQUIDO_TOTAL
        /// (confirmado: distinto de LIQUIDO_TOTAL_GRUPO). Fuente Angular: resumen.total_liquido_persona
        /// (mapeado 1:1 desde esa misma columna en Prea_frmPreaEstudiov2_Cargar).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal total_liquido_persona { get; set; }

        // ---- Fase 4 del diccionario: 11 parámetros sin control propio en Angular, resueltos
        // leyendo sbEstudio_Guarda_Modifica línea por línea (frmPreaEstudiov2.frm 12185-12298). ----
        /// <summary>@59 NSUB_EXP = cboCantidadFiadores.Text (línea 12284) — en VB6 esta posición
        /// realmente recibe el conteo de fiadores, NO un conteo de sub-expedientes pese al nombre
        /// del parámetro. Fuente Angular: el campo `fiadores` ya existente arriba.</summary>
        /// <summary>@75 APL_IND_COMPONENTE: en VB6 = 0 si txtS_ComponenteAdicionalPorc.Text = 0,
        /// si no 1 (líneas 12256-12260). Se deriva en el API desde componente_adicional_porc,
        /// no requiere campo propio.</summary>
        /// <summary>@84 IND_TIPO_SALARIO_EXT: en VB6, default 0; 1 si chkS_Constancia está marcado;
        /// 2 si chkS_OrdenPatronal está marcado (el segundo pisa al primero, líneas 12246-12253).
        /// Fuente Angular: salariosForm.ind_salario_constancia / ind_salario_orden_patronal.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public bool ind_salario_constancia { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool ind_salario_orden_patronal { get; set; }

        // ---- Fase 5 del diccionario: últimos 3 params ❓ resueltos leyendo sbEstudio_Guarda_Modifica ----
        /// <summary>@103 REFUNDICIONES_MORA = txtR_TotalMora.Text. Fuente Angular: total_mora del
        /// tab Refundiciones (propiedad de página, actualizada por TabRefundicionesComponent).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal refundiciones_mora { get; set; }
        /// <summary>@104 SALARIO_USURA = txtSalarioMinimoInembargable.Text (VB6 reutiliza el
        /// control del salario mínimo inembargable para este parámetro, no hay txtSalarioUsura
        /// propio). Fuente Angular: this.salarioMinimoInembargable.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public decimal salario_minimo_inembargable { get; set; }
        /// <summary>@65 PTS_EXTRA_FAP = txtFrapPorc.Text en VB6.</summary>
        public decimal? pts_extra_frap { get; set; }
        /// <summary>chkCargaAsociacion.Value en VB6. Null conserva compatibilidad con clientes viejos.</summary>
        public bool? aplica_carga_asociacion { get; set; }
        /// <summary>chkCargaFrap.Value en VB6. Null conserva compatibilidad con clientes viejos.</summary>
        public bool? aplica_carga_frap { get; set; }
    }

    public class FrmPreaEstudiov2GuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Salarios

    public class FrmPreaEstudiov2SalariosGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo_salario { get; set; } = string.Empty;
        public DateTime? corte_colilla { get; set; }
        public decimal salario_devengado { get; set; }
        public decimal salario_mensual { get; set; }
        public decimal salario_constancia { get; set; }
        public decimal salario_orden_patronal { get; set; }
        public decimal ingreso_privado { get; set; }
        public decimal ingreso_privado_porc { get; set; }
        public int componente_adicional_id { get; set; }
        public decimal componente_adicional_porc { get; set; }
    }

    /// <summary>
    /// Guarda la grilla "Tabla de Salarios" (gSalarios en VB6). VB6: sbSalarios_Guardar
    /// (frmPreaEstudiov2.frm línea ~13737) — elimina los registros y los reconstruye desde cero.
    /// </summary>
    public class FrmPreaEstudiov2TablaSalariosGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public List<FrmPreaEstudiov2SalarioDetalleDto> tabla_salarios { get; set; } = [];
    }

    public class FrmPreaEstudiov2OficinaEjecutivoCambiarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string id_promotor { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ExtraGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        /// <summary>IdX del extra. 0 (o vacío) para insertar, &gt; 0 para modificar.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public int idx { get; set; }
        public string cod_extras { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
    }

    public class FrmPreaEstudiov2ExtraBorrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int idx { get; set; }
    }

    #endregion

    #region Deducciones

    public class FrmPreaEstudiov2DeduccionesResponse
    {
        public List<FrmPreaEstudiov2DeduccionesDetalleDto> deducciones { get; set; } = [];
        public decimal total_colilla { get; set; }
        public decimal total_mensual { get; set; }
    }

    /// <summary>
    /// VB6: sbDeducciones_Load (frmPreaEstudiov2.frm línea ~17202), columnas reales de
    /// spCrdPreaConsultaDeducciones: IdX (clave para borrar), Id_Deduccion (CellTag, no
    /// usado para borrar), Tipo, Descripcion, CUOTA_COLILLA, CUOTA_MENSUAL.
    /// </summary>
    public class FrmPreaEstudiov2DeduccionesDetalleDto
    {
        public string id_x { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal cuota_colilla { get; set; }
        public decimal cuota_mensual { get; set; }
    }

    public class FrmPreaEstudiov2DeduccionesAgregarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;

        /// <summary>cboDeduccion.ItemData en VB6 — id del tipo de deducción seleccionado.</summary>
        public string cod_deduccion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
    }

    public class FrmPreaEstudiov2DeduccionesBorrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string id_x { get; set; } = string.Empty;
    }

    #endregion

    #region Créditos (tránsito)

    public class FrmPreaEstudiov2CreditosResponse
    {
        public List<FrmPreaEstudiov2CreditoTransitoDto> cancelados { get; set; } = [];
        public List<FrmPreaEstudiov2CreditoTransitoDto> por_cobrar { get; set; } = [];
        public decimal total_cancelados { get; set; }
        public decimal total_por_cobrar { get; set; }
    }

    public class FrmPreaEstudiov2CreditoTransitoDto
    {
        public int id_solicitud { get; set; }
        public string detalle { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal cuota { get; set; }
    }

    public class FrmPreaEstudiov2CreditoTransitoRegistrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        /// <summary>'C' = Cancelados, 'A' = Por Cobrar (gCuotasCancela/gCuotasCobrar en VB6).</summary>
        public string tipo { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public decimal cuota { get; set; } = 0;
    }

    public class FrmPreaEstudiov2CreditoTransitoBorrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        /// <summary>'C' = Cancelados, 'A' = Por Cobrar. VB6 elimina el grupo completo, no una fila.</summary>
        public string tipo { get; set; } = string.Empty;
    }

    /// <summary>Elimina UNA cuota en tránsito por su id_solicitud (borrado individual de fila).</summary>
    public class FrmPreaEstudiov2CreditoTransitoBorrarFilaRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        /// <summary>Identificador de la cuota en tránsito a eliminar (id_solicitud de CRD_PREA_DETALLE_CUOTAS_EN_TRANSITO).</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public int id_solicitud { get; set; }
    }

    #endregion

    #region Refundiciones

    public class FrmPreaEstudiov2RefundicionesResponse
    {
        public List<FrmPreaEstudiov2RefundicionDto> refundiciones { get; set; } = [];
        public decimal total_cuotas { get; set; }
        public decimal total_refunde { get; set; }
        public decimal total_mora { get; set; }
        /// <summary>Fecha del servidor SQL (dbo.MyGetdate). VB6: dtpR_Formaliza.</summary>
        public DateTime fecha_servidor { get; set; }
    }

    public class FrmPreaEstudiov2RefundicionDto
    {
        /// <summary>Clave real usada por VB6 en el UPDATE de Aplica/Apl_Mora.</summary>
        public int id_solicitud { get; set; }
        public string descripcion { get; set; } = string.Empty;
        /// <summary>Saldo a refundir.</summary>
        public decimal saldo { get; set; }
        /// <summary>Tasa de interés.</summary>
        public decimal Interes { get; set; }
        public decimal cuota { get; set; }
        /// <summary>Interés corriente.</summary>
        public decimal IntCor { get; set; }
        /// <summary>Interés devuelto.</summary>
        public decimal IntDevueltos { get; set; }
        public decimal Cargos { get; set; }
        /// <summary>Mora principal (monto a refundir).</summary>
        public decimal Mora_principal { get; set; }
        /// <summary>Mora intereses.</summary>
        public decimal Mora_intereses { get; set; }
        /// <summary>Columna real CRD_PREA_REFUNDICIONES.Aplica.</summary>
        public bool Aplica { get; set; }
        /// <summary>Columna real CRD_PREA_REFUNDICIONES.Apl_Mora.</summary>
        public bool Apl_Mora { get; set; }
        /// <summary>Código de garantía (F, H, etc).</summary>
        public string garantia { get; set; } = string.Empty;
        /// <summary>Descripción de garantía.</summary>
        public string Gdescripcion { get; set; } = string.Empty;
        /// <summary>Porcentaje.</summary>
        public decimal Porcentaje { get; set; }
        /// <summary>Monto aprobado.</summary>
        public decimal MontoApr { get; set; }
        /// <summary>Código de línea.</summary>
        public string Codigo { get; set; } = string.Empty;
        /// <summary>Mora IVA.</summary>
        public decimal mora_iva { get; set; }
        /// <summary>Fecha de cálculo.</summary>
        public DateTime? FechaCalculo { get; set; }
        /// <summary>No bloqueado.</summary>
        public int NO_BLOQUEO { get; set; }
    }

    public class FrmPreaEstudiov2RefundicionToggleRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int id_solicitud { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool aplica { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool apl_mora { get; set; }
    }

    public class FrmPreaEstudiov2RefundicionesActualizarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    #endregion

    #region Fianzas

    public class FrmPreaEstudiov2FianzasResponse
    {
        public List<FrmPreaEstudiov2FianzaDto> fianzas { get; set; } = [];
        public decimal total_saldos { get; set; }
        public decimal total_cuotas { get; set; }
    }

    public class FrmPreaEstudiov2FianzaDto
    {
        /// <summary>Clave real usada por VB6 en el UPDATE de Aplica/Cancela_Mora (id_solicitud).</summary>
        public int id_solicitud { get; set; }
        public int orden { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        /// <summary>Cantidad de fiadores; SP: nfiadores. VB6 divide saldo/cuota/monto_mora entre este valor.</summary>
        public int fiadores { get; set; }
        /// <summary>Cantidad de cuotas en mora; SP: Mora_Cuotas.</summary>
        public int mora_cuotas { get; set; }
        /// <summary>Monto en mora; SP: Mora_Monto.</summary>
        public decimal monto_mora { get; set; }
        /// <summary>Columna real CRD_PREA_DETALLE_FIANZAS.Aplica.</summary>
        public bool aplica { get; set; }
        /// <summary>Columna real CRD_PREA_DETALLE_FIANZAS.Cancela_Mora.</summary>
        public bool cancela_mora { get; set; }
        /// <summary>Monto aprobado inicial; SP: MontoApr.</summary>
        public decimal montoApr { get; set; }
        /// <summary>Porcentaje cancelado; SP: Porcentaje.</summary>
        public decimal porcentaje { get; set; }
        /// <summary>Clasificación crediticia; SP: Clasificacion.</summary>
        public string clasificacion { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2FianzaToggleRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int id_solicitud { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool aplica { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool cancela_mora { get; set; }
    }

    public class FrmPreaEstudiov2FianzasActualizarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    #endregion

    #region Desembolsos

    public class FrmPreaEstudiov2DesembolsosResponse
    {
        public List<FrmPreaEstudiov2DesembolsoDto> desembolsos { get; set; } = [];
        public List<FrmPreaEstudiov2DropdownDto> bancos { get; set; } = [];
    }

    public class FrmPreaEstudiov2DesembolsoAcreedorDto
    {
        public string id { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string nombre_giro { get; set; } = string.Empty;
        public int modifica { get; set; }
    }

    public class FrmPreaEstudiov2DesembolsoDto
    {
        /// <summary>CRD_PREA_DETALLE_DESEMBOLSOS.IdX (clave real usada por spCrdPreaEliminarDesembolsos).</summary>
        public int id_desembolso { get; set; }
        public string cod_acredor { get; set; } = string.Empty;
        public int ordinario { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public decimal monto { get; set; }
    }

    /// <summary>
    /// Fiel a VB6 sbDesembolso_Guardar (frmPreaEstudiov2.frm línea ~13091), que llama
    /// exec spCrdPreaGuardaDesembolsos con 16 parámetros posicionales (firma confirmada
    /// en el comentario del propio código fuente VB6).
    /// </summary>
    public class FrmPreaEstudiov2DesembolsoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        /// <summary>txtDS_Descripcion.Tag: código del acreedor/concepto seleccionado en la lista.</summary>
        public string cod_acreedor { get; set; } = string.Empty;
        /// <summary>cboD_Ordinario = "Sí"/"No".</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public bool ordinario { get; set; }
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public decimal cuota { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public decimal monto { get; set; }
        /// <summary>TipoGiro: fxTipoDocumento(cboTipoDocumento.Text), ej. 'TE'/'TS'.</summary>
        public string tipo_giro { get; set; } = string.Empty;
        /// <summary>txtIdentificación.Text.</summary>
        public string cedula_destino { get; set; } = string.Empty;
        /// <summary>cboTipoId.ItemData.</summary>
        [System.Text.Json.Serialization.JsonRequired]
        public int tipo_cedula { get; set; }
        /// <summary>cboCuenta.ItemData.</summary>
        public string cuenta { get; set; } = string.Empty;
        /// <summary>cboDivisa.ItemData.</summary>
        public string cod_divisa { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        /// <summary>cboBanco.ItemData.</summary>
        public string cod_banco { get; set; } = string.Empty;
    }

    #endregion

    #region Historial

    public class FrmPreaEstudiov2HistorialResponse
    {
        public List<FrmPreaEstudiov2HistorialDto> ejecutivos { get; set; } = [];
        public List<FrmPreaEstudiov2HistorialDto> general { get; set; } = [];
    }

    public class FrmPreaEstudiov2HistorialDto
    {
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string accion { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string cod_etiqueta { get; set; } = string.Empty;
        public string codigo_etiqueta { get; set; } = string.Empty;
        public string etiqueta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string usuario_registra_1 { get; set; } = string.Empty;
        public string usuario_registra_2 { get; set; } = string.Empty;
        public string usuario_revision { get; set; } = string.Empty;
    }

    #endregion

    #region Adjuntos

    public class FrmPreaEstudiov2AdjuntoDto
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_adjunto { get; set; }
        public string nombre_archivo { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2AdjuntoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string nombre_archivo { get; set; } = string.Empty;
        public string contenido_base64 { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2AdjuntoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public int id_adjunto { get; set; } = 0;
        public List<int> ids_adjuntos { get; set; } = [];
    }

    public class FrmPreaEstudiov2AdjuntoDescargaDto
    {
        public byte[] contenido { get; set; } = [];
        public string nombre_archivo { get; set; } = string.Empty;
    }

    #endregion

    #region Resolución

    public class FrmPreaEstudiov2ResolucionResponse
    {
        public string acta { get; set; } = string.Empty;
        public string acta_sesion { get; set; } = string.Empty;
        public DateTime? acta_fecha { get; set; }
        public List<FrmPreaEstudiov2ResolucionDetalleDto> detalle { get; set; } = [];
    }

    public class FrmPreaEstudiov2ResolucionDetalleDto
    {
        public string estado { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    #endregion

    #region Observaciones

    public class FrmPreaEstudiov2ObservacionesRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
    }

    #endregion

    #region Comité

    /// <summary>
    /// Fiel a VB6 btnEtiqueta_Click (frmPreaEstudiov2.frm línea ~13247):
    /// exec spCrdPreaAgregaEtiqueta '&lt;expediente&gt;', '&lt;etiqueta&gt;', '&lt;nota&gt;', '&lt;usuario&gt;'.
    /// La nota debe tener al menos 50 caracteres (validación de VB6).
    /// </summary>
    public class FrmPreaEstudiov2EtiquetaAgregarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_etiqueta { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ComiteAsignarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
        /// <summary>Texto del comité seleccionado (cboComite.Text), usado en la bitácora.</summary>
        public string comite_desc { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ComiteAsignarResponse
    {
        public string comite { get; set; } = string.Empty;
        public bool asignado { get; set; }
        public string mensaje { get; set; } = string.Empty;
        /// <summary>'E' Ejecutivo, 'M' Mancomunado, u otro devuelto por spCrdPrea_Comite_Asigna_Valida.</summary>
        public string tipo_aprobacion { get; set; } = string.Empty;
    }

    #endregion

    #region Copiar

    public class FrmPreaEstudiov2CopiarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis_origen { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2CopiarResponse
    {
        public string cod_preanalisis_nuevo { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Solicitar

    public class FrmPreaEstudiov2SolicitarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2SolicitarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Incapacidades

    public class FrmPreaEstudiov2IncapacidadGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public List<FrmPreaEstudiov2IncapacidadItemRequest> incapacidades { get; set; } = [];
    }

    public class FrmPreaEstudiov2IncapacidadItemRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime desde { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime hasta { get; set; }
    }

    #endregion

    #region Hipotecario

    /// <summary>
    /// Fiel a VB6 btnHipotecario_Click Case 1 "Avalúos CFIA" (frmPreaEstudiov2.frm línea ~13418):
    /// exec spCrdPreaSumarAvaluoCFIA '&lt;expediente&gt;', '&lt;usuario&gt;'.
    /// </summary>
    public class FrmPreaEstudiov2HipotecarioSumarAvaluoRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fiel a VB6 btnHipotecario_Click Case 4 "Cambio de Estado" (frmPreaEstudiov2.frm línea ~13466):
    /// valida comité asignado (dbo.fxValidaAsignacionComite, spCrdPrea_Comite_Asigna_Valida) y,
    /// si la garantía es Hipotecaria ('H'), ejecuta spCRDPreaEstadoHipotecarioAprob.
    /// </summary>
    public class FrmPreaEstudiov2HipotecarioCambiarEstadoRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public string cod_garantia { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2HipotecarioResponse
    {
        public decimal monto_avaluo_cfia { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

    #region Abandonar

    public class FrmPreaEstudiov2AbandonarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2AbandonarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = "B";
        public string estado_desc { get; set; } = "Abandonado";
        public string mensaje { get; set; } = "Se ha ABANDONADO el expediente correctamente.";
    }

    #endregion

    #region Prendario

    /// <summary>
    /// Consulta la información del tab Prendario (sbPrendario_Load en VB6):
    /// log de exámenes (spCrd_Prea_Examenes_Log) y datos de la prenda
    /// (spCrd_Prea_Prenda_Datos: Prenda_Poliza, Prenda_Monto, ESTADO_EXAMENES).
    /// </summary>
    public class FrmPreaEstudiov2PrendarioConsultarRequest
    {
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2ExamenPrendaDto
    {
        public int id_nota { get; set; }
        public string nota { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2PrendarioConsultarResponse
    {
        public List<FrmPreaEstudiov2ExamenPrendaDto> examenes { get; set; } = new();
        public decimal valor_prenda { get; set; }
        public decimal monto_poliza_prenda { get; set; }
        public string estado_examenes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Aplica un estado a los exámenes de prenda (btnP_Examenes_Click en VB6):
    /// exec spCRD_PreaAplicaEstadoExamenes '&lt;expediente&gt;', '&lt;estado&gt;', '&lt;usuario&gt;', '&lt;nota&gt;'.
    /// El estado es E/R/A (Enviados/Recibidos/Aprobados).
    /// </summary>
    public class FrmPreaEstudiov2PrendarioEstadoRequest
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2PrendarioEstadoResponse
    {
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion
}
