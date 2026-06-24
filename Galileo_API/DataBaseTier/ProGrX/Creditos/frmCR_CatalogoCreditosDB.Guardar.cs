using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {

        /// <summary>
        /// Guarda una linea del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_Guardar(int codEmpresa, CrCatalogoCreditoGuardarRequest request)
        {
            NormalizarRequest(request);

            if (string.IsNullOrWhiteSpace(request.codigo) || string.IsNullOrWhiteSpace(request.descripcion))
            {
                return new ErrorDto { Code = -1, Description = "Codigo y descripcion son requeridos." };
            }

            if (request.codigo.Length > 4)
            {
                return new ErrorDto { Code = -1, Description = "Codigo corriente invalido." };
            }

            var existe = CrCatalogoCreditos_Existe(codEmpresa, request.codigo);
            var respuesta = existe
                ? CrCatalogoCreditos_Actualizar(codEmpresa, request)
                : CrCatalogoCreditos_Insertar(codEmpresa, request);

            if (respuesta.Code < 0)
                return respuesta;

            var respuestaCph = CrCatalogoCreditos_Cph_Guardar(codEmpresa, request);
            if (respuestaCph.Code < 0)
                return respuestaCph;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                existe ? "Modifica - WEB" : "Registra - WEB",
                $"Linea de Credito : {request.codigo}");

            return new ErrorDto { Code = 0, Description = "Informacion guardada satisfactoriamente..." };
        }


        /// <summary>
        /// Guarda la ficha tecnica del producto en linea para Web/App.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_PeL_Guardar(int codEmpresa, CrCatalogoCreditoPeLGuardarRequest request)
        {
            NormalizarPeLRequest(request);

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto { Code = -1, Description = "Debe consultar una linea de credito." };
            }

            const string query = @"
                EXEC spCrd_Catalogo_PeL_Guarda
                    @Codigo,
                    @DescripcionLinea,
                    @UsoDestinoLinea,
                    @ColorCaja,
                    @LogoUrl,
                    @EtiquetaAprobacion,
                    @EtiquetaMontoMax,
                    @EtiquetaPlazoTasa,
                    @EtiquetaDeposito,
                    @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    DescripcionLinea = request.df_descripcion_linea,
                    UsoDestinoLinea = request.df_uso_destino_linea,
                    ColorCaja = request.df_color_caja,
                    LogoUrl = request.df_logo_url,
                    EtiquetaAprobacion = request.df_etiqueta_aprobacion,
                    EtiquetaMontoMax = request.df_etiqueta_monto_max,
                    EtiquetaPlazoTasa = request.df_etiqueta_plazo_tasa,
                    EtiquetaDeposito = request.df_etiqueta_deposito,
                    Usuario = request.usuario
                });
        }


        /// <summary>
        /// Elimina una linea del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            const string query = "DELETE catalogo WHERE codigo = @Codigo;";
            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = codigo.Trim().ToUpperInvariant() });

            if (respuesta.Code < 0)
                return respuesta;

            RegistrarBitacora(codEmpresa, usuario, "Borra - WEB", $"Codigo = {codigo.Trim().ToUpperInvariant()}");
            return respuesta;
        }

        /// <summary>
        /// Verifica si existe una linea del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private bool CrCatalogoCreditos_Existe(int codEmpresa, string codigo)
        {
            const string query = "SELECT COUNT(1) FROM catalogo WHERE codigo = @Codigo;";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new { Codigo = codigo.Trim().ToUpperInvariant() });

            return respuesta.Code >= 0 && respuesta.Result > 0;
        }

        /// <summary>
        /// Inserta una linea basica del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto CrCatalogoCreditos_Insertar(int codEmpresa, CrCatalogoCreditoGuardarRequest request)
        {
            const string query = @"
                INSERT INTO catalogo (
                    codigo, codigoa, descripcion, notas, activo, linea_interna,
                    deduc_codigo_alter, filtra_refundibles, Permite_PersonaEnCbrJud,
                    convenio, poliza, refunde, retencion, aceptarefun, primer_cuota,
                    pidecheque, retencion_muestra_saldo, cobertura, genera_mora,
                    movcajas, tramite, requisitos_tipo, id_comite, cod_institucion,
                    divisaid, tramitedias, operaciones_activas, membresia_meses,
                    refunde_porc, refunde_tipo, porc_cargo_cancelacion, anticipo_meses,
                    liq_tipoaumento, liq_valor, base_calculo, cobro_tipo_aplicacion,
                    FechaCorteAlterna, fechacorte, tasa_destino, tbp_utiliza,
                    tbp_adicional, tasa_mora_tipo, tasa_mora_add, TASA_FIJA_X_TBP,
                    TASA_FIJA_X_TBP_PUNTOS_ADD, PLAZO_TASA_FIJA, Oficina_Linea,
                    Oficina, website, visible_ec,
                    forma_pago_pos, forma_pago_web, auto_gestion_lmax, giro_max_transac,
                    giro_automatico, giro_monto_base, giro_minimo, auto_gestion_tipo,
                    refunde_auto, refunde_aumenta_base, IND_NOTIFICA_CLI_FORMALIZA,
                    IND_NOTIFICA_CLI_CANCELA, IND_MOV_APLICA_BONIF, IND_PAGO_OP_APLICACION,
                    IND_READECUA, IND_MONTO_MAX, ID_REQ_SUPERVISION, MONTO_SUPERVISION,
                    PORC_ANTICIPO_EXT, IND_EDAD_PENSION_EST, IND_EDAD_PENSION_FOR,
                    MOV_SINPE, MOV_SINPE_TIPOS, Reserva_Aplica, Reserva_Facial_Flat,
                    Reserva_Mora_Apl, Reserva_Codigo, Reserva_Monto_Minimo, Revolutiva,
                    Revolutiva_Tope_Retiros, Revolutiva_Estudio, Revolutiva_Plan_Ahorro_Utiliza,
                    Revolutiva_Plan_Ahorro
                )
                VALUES (
                    @codigo, @codigoa, @descripcion, @notas, @activo, @linea_interna,
                    @deduc_codigo_alter, @filtra_refundibles, @permite_persona_en_cbr_jud,
                    @convenio, @poliza, @refunde, @retencion, @aceptarefun, @primer_cuota,
                    @pidecheque, @retencion_muestra_saldo, @cobertura, @genera_mora,
                    @movcajas, @tramite, @requisitos_tipo, @id_comite, @cod_institucion,
                    @divisaid, @tramitedias, @operaciones_activas, @membresia_meses,
                    @refunde_porc, @refunde_tipo, @porc_cargo_cancelacion, @anticipo_meses,
                    @liq_tipoaumento, @liq_valor, @base_calculo, @cobro_tipo_aplicacion,
                    @fecha_corte_alterna, @fechacorte, @tasa_destino, @tbp_utiliza,
                    @tbp_adicional, @tasa_mora_tipo, @tasa_mora_add, @tasa_fija_x_tbp,
                    @tasa_fija_x_tbp_puntos_add, @plazo_tasa_fija, @oficina_linea,
                    NULLIF(@oficina, ''), @website, @visible_ec,
                    @forma_pago_pos, @forma_pago_web, @auto_gestion_lmax, @giro_max_transac,
                    @giro_automatico, @giro_monto_base, @giro_minimo, @auto_gestion_tipo,
                    @refunde_auto, @refunde_aumenta_base, @ind_notifica_cli_formaliza,
                    @ind_notifica_cli_cancela, @ind_mov_aplica_bonif, @ind_pago_op_aplicacion,
                    @ind_readecua, @ind_monto_max, @id_req_supervision, @monto_supervision,
                    @porc_anticipo_ext, @ind_edad_pension_est, @ind_edad_pension_for,
                    @mov_sinpe, @mov_sinpe_tipos, @reserva_aplica, @reserva_facial_flat,
                    @reserva_mora_apl, NULLIF(@reserva_codigo, ''), @reserva_monto_minimo,
                    @revolutiva, @revolutiva_tope_retiros, @revolutiva_estudio,
                    @revolutiva_plan_ahorro_utiliza, NULLIF(@revolutiva_plan_ahorro, '')
                );";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, request);
        }


        /// <summary>
        /// Guarda las opciones CPH de la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto CrCatalogoCreditos_Cph_Guardar(int codEmpresa, CrCatalogoCreditoGuardarRequest request)
        {
            const string query = @"
                EXEC spCrd_Catalogo_CPH @Codigo, 1, @Cph1, @Usuario;
                EXEC spCrd_Catalogo_CPH @Codigo, 2, @Cph2, @Usuario;
                EXEC spCrd_Catalogo_CPH @Codigo, 3, @Cph3, @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Cph1 = request.cph1 ? 1 : 0,
                    Cph2 = request.cph2 ? 1 : 0,
                    Cph3 = request.cph3 ? 1 : 0,
                    Usuario = request.usuario
                });
        }


        /// <summary>
        /// Actualiza una linea basica del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto CrCatalogoCreditos_Actualizar(int codEmpresa, CrCatalogoCreditoGuardarRequest request)
        {
            const string query = @"
                UPDATE catalogo
                SET codigoa = @codigoa,
                    descripcion = @descripcion,
                    notas = @notas,
                    activo = @activo,
                    linea_interna = @linea_interna,
                    deduc_codigo_alter = @deduc_codigo_alter,
                    filtra_refundibles = @filtra_refundibles,
                    Permite_PersonaEnCbrJud = @permite_persona_en_cbr_jud,
                    convenio = @convenio,
                    poliza = @poliza,
                    refunde = @refunde,
                    retencion = @retencion,
                    aceptarefun = @aceptarefun,
                    primer_cuota = @primer_cuota,
                    pidecheque = @pidecheque,
                    retencion_muestra_saldo = @retencion_muestra_saldo,
                    cobertura = @cobertura,
                    genera_mora = @genera_mora,
                    movcajas = @movcajas,
                    tramite = @tramite,
                    requisitos_tipo = @requisitos_tipo,
                    id_comite = @id_comite,
                    cod_institucion = @cod_institucion,
                    divisaid = @divisaid,
                    tramitedias = @tramitedias,
                    operaciones_activas = @operaciones_activas,
                    membresia_meses = @membresia_meses,
                    refunde_porc = @refunde_porc,
                    refunde_tipo = @refunde_tipo,
                    porc_cargo_cancelacion = @porc_cargo_cancelacion,
                    anticipo_meses = @anticipo_meses,
                    liq_tipoaumento = @liq_tipoaumento,
                    liq_valor = @liq_valor,
                    base_calculo = @base_calculo,
                    cobro_tipo_aplicacion = @cobro_tipo_aplicacion,
                    FechaCorteAlterna = @fecha_corte_alterna,
                    fechacorte = @fechacorte,
                    tasa_destino = @tasa_destino,
                    tbp_utiliza = @tbp_utiliza,
                    tbp_adicional = @tbp_adicional,
                    tasa_mora_tipo = @tasa_mora_tipo,
                    tasa_mora_add = @tasa_mora_add,
                    TASA_FIJA_X_TBP = @tasa_fija_x_tbp,
                    TASA_FIJA_X_TBP_PUNTOS_ADD = @tasa_fija_x_tbp_puntos_add,
                    PLAZO_TASA_FIJA = @plazo_tasa_fija,
                    Oficina_Linea = @oficina_linea,
                    Oficina = NULLIF(@oficina, ''),
                    website = @website,
                    visible_ec = @visible_ec,
                    forma_pago_pos = @forma_pago_pos,
                    forma_pago_web = @forma_pago_web,
                    auto_gestion_lmax = @auto_gestion_lmax,
                    giro_max_transac = @giro_max_transac,
                    giro_automatico = @giro_automatico,
                    giro_monto_base = @giro_monto_base,
                    giro_minimo = @giro_minimo,
                    auto_gestion_tipo = @auto_gestion_tipo,
                    refunde_auto = @refunde_auto,
                    refunde_aumenta_base = @refunde_aumenta_base,
                    IND_NOTIFICA_CLI_FORMALIZA = @ind_notifica_cli_formaliza,
                    IND_NOTIFICA_CLI_CANCELA = @ind_notifica_cli_cancela,
                    IND_MOV_APLICA_BONIF = @ind_mov_aplica_bonif,
                    IND_PAGO_OP_APLICACION = @ind_pago_op_aplicacion,
                    IND_READECUA = @ind_readecua,
                    IND_MONTO_MAX = @ind_monto_max,
                    ID_REQ_SUPERVISION = @id_req_supervision,
                    MONTO_SUPERVISION = @monto_supervision,
                    PORC_ANTICIPO_EXT = @porc_anticipo_ext,
                    IND_EDAD_PENSION_EST = @ind_edad_pension_est,
                    IND_EDAD_PENSION_FOR = @ind_edad_pension_for,
                    MOV_SINPE = @mov_sinpe,
                    MOV_SINPE_TIPOS = @mov_sinpe_tipos,
                    Reserva_Aplica = @reserva_aplica,
                    Reserva_Facial_Flat = @reserva_facial_flat,
                    Reserva_Mora_Apl = @reserva_mora_apl,
                    Reserva_Codigo = NULLIF(@reserva_codigo, ''),
                    Reserva_Monto_Minimo = @reserva_monto_minimo,
                    Revolutiva = @revolutiva,
                    Revolutiva_Tope_Retiros = @revolutiva_tope_retiros,
                    Revolutiva_Estudio = @revolutiva_estudio,
                    Revolutiva_Plan_Ahorro_Utiliza = @revolutiva_plan_ahorro_utiliza,
                    Revolutiva_Plan_Ahorro = NULLIF(@revolutiva_plan_ahorro, '')
                WHERE codigo = @codigo;";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, request);
        }
    }
}
