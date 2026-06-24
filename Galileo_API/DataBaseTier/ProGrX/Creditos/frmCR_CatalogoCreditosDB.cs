using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoCreditosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoCreditosDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoCreditosDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene las lineas del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="soloAutoGestion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoData>> CrCatalogoCreditos_Obtener(int codEmpresa, bool soloAutoGestion)
        {
            const string query = @"
                SELECT
                    codigo,
                    ISNULL(codigoa, '') AS codigoa,
                    descripcion,
                    ISNULL(notas, '') AS notas,
                    activo,
                    linea_interna,
                    deduc_codigo_alter,
                    filtra_refundibles,
                    Permite_PersonaEnCbrJud AS permite_persona_en_cbr_jud,
                    convenio,
                    poliza,
                    refunde,
                    retencion,
                    aceptarefun,
                    primer_cuota,
                    pidecheque,
                    retencion_muestra_saldo,
                    cobertura,
                    genera_mora,
                    movcajas,
                    tramite,
                    requisitos_tipo,
                    ISNULL(id_comite, 0) AS id_comite,
                    ISNULL(cod_institucion, '') AS cod_institucion,
                    '' AS divisaid,
                    ISNULL(tramitedias, 0) AS tramitedias,
                    ISNULL(operaciones_activas, 0) AS operaciones_activas,
                    ISNULL(membresia_meses, 0) AS membresia_meses,
                    ISNULL(refunde_porc, 0) AS refunde_porc,
                    ISNULL(refunde_tipo, 'P') AS refunde_tipo,
                    ISNULL(porc_cargo_cancelacion, 0) AS porc_cargo_cancelacion,
                    ISNULL(anticipo_meses, 0) AS anticipo_meses,
                    ISNULL(liq_tipoaumento, 'F') AS liq_tipoaumento,
                    ISNULL(liq_valor, 0) AS liq_valor,
                    ISNULL(base_calculo, '') AS base_calculo,
                    ISNULL(cobro_tipo_aplicacion, 'V') AS cobro_tipo_aplicacion,
                    CASE
                        WHEN UPPER(CONVERT(varchar(5), ISNULL(FechaCorteAlterna, 'N'))) IN ('S', '1', 'TRUE')
                            THEN CONVERT(bit, 1)
                        ELSE CONVERT(bit, 0)
                    END AS fecha_corte_alterna,
                    fechacorte,
                    ISNULL(tasa_destino, 0) AS tasa_destino,
                    ISNULL(tbp_utiliza, 0) AS tbp_utiliza,
                    ISNULL(tbp_adicional, 0) AS tbp_adicional,
                    ISNULL(tasa_mora_tipo, 'N/A') AS tasa_mora_tipo,
                    ISNULL(tasa_mora_add, 0) AS tasa_mora_add,
                    ISNULL(TASA_FIJA_X_TBP, 0) AS tasa_fija_x_tbp,
                    ISNULL(TASA_FIJA_X_TBP_PUNTOS_ADD, 0) AS tasa_fija_x_tbp_puntos_add,
                    ISNULL(PLAZO_TASA_FIJA, 0) AS plazo_tasa_fija,
                    0 AS oficina_linea,
                    '' AS oficina,
                    '' AS oficina_desc,
                    ISNULL(website, 0) AS website,
                    ISNULL(visible_ec, 0) AS visible_ec,
                    ISNULL(forma_pago_pos, 0) AS forma_pago_pos,
                    ISNULL(forma_pago_web, 0) AS forma_pago_web,
                    ISNULL(auto_gestion_lmax, 0) AS auto_gestion_lmax,
                    ISNULL(giro_max_transac, 0) AS giro_max_transac,
                    ISNULL(giro_automatico, 0) AS giro_automatico,
                    ISNULL(giro_monto_base, 0) AS giro_monto_base,
                    ISNULL(giro_minimo, 0) AS giro_minimo,
                    ISNULL(auto_gestion_tipo, 'C') AS auto_gestion_tipo,
                    ISNULL(refunde_auto, 0) AS refunde_auto,
                    ISNULL(refunde_aumenta_base, 0) AS refunde_aumenta_base
                FROM catalogo
                WHERE (@SoloAutoGestion = 0 OR website = 1)
                ORDER BY codigo;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoData>(
                _portalDb,
                codEmpresa,
                query,
                new { SoloAutoGestion = soloAutoGestion ? 1 : 0 });
        }

        /// <summary>
        /// Consulta una linea especifica del catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCreditoData?> CrCatalogoCreditos_Consultar(int codEmpresa, string codigo)
        {
            const string query = "EXEC spCrd_Catalogo_Consulta @Codigo;";

            var respuesta = DbHelper.ExecuteSingleQuery<CrCatalogoCreditoData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { Codigo = codigo.Trim().ToUpperInvariant() });

            if (respuesta.Code >= 0 && respuesta.Result is not null)
            {
                CrCatalogoCreditos_Cph_Obtener(codEmpresa, respuesta.Result);
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene las cuentas contables asociadas a una linea de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoCuentaData>> CrCatalogoCreditos_Cuentas_Obtener(int codEmpresa, string codigo)
        {
            const string query = @"
                SELECT
                    C.rubro,
                    ISNULL(C.cuenta, '') AS cuenta,
                    ISNULL(C.descripcion, '') AS descripcion,
                    C.es_titulo
                FROM vCrd_Catalogo_Cuentas V
                CROSS APPLY (VALUES
                    ('NORMAL', '', '', 1),
                    ('   Principal', V.ctaNamort_Mask, V.ctaNamort_Desc, 0),
                    ('   Int.Corriente', V.ctaNintC_Mask, V.ctaNintC_Desc, 0),
                    ('   Int.Moratorio', V.ctaNintM_Mask, V.ctaNintM_Desc, 0),
                    ('OPEX', '', '', 1),
                    ('   Principal', V.CtaOamort_Mask, V.CtaOamort_Desc, 0),
                    ('   Int.Corriente', V.ctaOintC_Mask, V.ctaOintC_Desc, 0),
                    ('   Int.Moratorio', V.ctaOintM_Mask, V.ctaOintM_Desc, 0),
                    ('CBR.JUD.', '', '', 1),
                    ('   Principal', V.ctacamort_Mask, V.CtaCamort_Desc, 0),
                    ('   Int.Corriente', V.ctacintc_Mask, V.ctaCintC_Desc, 0),
                    ('   Int.Moratorio', V.ctacintm_Mask, V.ctaCintM_Desc, 0),
                    ('COMPLEMENTARIAS', '', '', 1),
                    ('   Cancelacion Anticipada', V.CTA_CARGOS_ANTICIPO_Mask, V.CTA_CARGOS_ANTICIPO_Desc, 0),
                    ('   Imp.Valor Agregado', V.CTA_IVA_Mask, V.CTA_IVA_Desc, 0),
                    ('   Prod.Acum.Cartera', V.CTA_CAR_PRODUCTO_Mask, V.CTA_CAR_PRODUCTO_Desc, 0),
                    ('   Prod.Acum.Efectos (+/-)', V.CTA_PROD_ACUM_Mask, V.CTA_PROD_ACUM_Desc, 0),
                    ('   Int.Cbr. x Adelantado', V.CTA_INT_ADELANTADO_Mask, V.CTA_INT_ADELANTADO_Desc, 0),
                    ('Registra Produto en Suspenso', CASE WHEN V.PS_REGISTRA = 1 THEN 'Si' ELSE 'No' END, '', 1),
                    ('   Prod.Susp.Deudora', V.CTA_PS_DEUDORA_Mask, V.CTA_PS_DEUDORA_Desc, 0),
                    ('   Prod.Susp.Acreedora', V.CTA_PS_ACREADORA_Mask, V.CTA_PS_ACREADORA_Desc, 0),
                    ('PUENTE', '', '', 1),
                    ('   Cierre Formalizacion', V.ctapuente_Mask, V.ctapuente_Desc, 0)
                ) C(rubro, cuenta, descripcion, es_titulo)
                WHERE V.codigo = @Codigo;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoCuentaData>(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = codigo.Trim().ToUpperInvariant() });
        }

        /// <summary>
        /// Obtiene las asignaciones disponibles de la linea de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCreditoAsignacionesData> CrCatalogoCreditos_Asignaciones_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCatalogoCreditoAsignacionesData>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            const string destinosQuery = @"
                SELECT
                    R.cod_destino AS destino,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM Catalogo_Destinos R
                LEFT JOIN catalogo_destinosAsg A
                    ON R.cod_destino = A.cod_destino
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_destino;";

            const string cargosQuery = @"
                SELECT
                    R.cod_cargo AS cargo,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CASE WHEN R.tipo = 'P' THEN 'Porcentual' ELSE 'Monto' END AS tipo,
                    ISNULL(R.valor, 0) AS valor,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM Cargos_Adicionales R
                LEFT JOIN Cargos_asignacion A
                    ON R.cod_cargo = A.cod_cargo
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_cargo;";

            const string requisitosQuery = @"
                SELECT
                    R.cod_requisito AS requisito,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, ISNULL(A.opcional, 0)) AS opcional,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM Requisitos_Adicionales R
                LEFT JOIN Requisitos_asignacion A
                    ON R.cod_requisito = A.cod_requisito
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_requisito;";

            const string recursosQuery = @"
                SELECT
                    G.cod_grupo AS recurso,
                    ISNULL(G.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM catalogo_grupos G
                LEFT JOIN catalogo_asignaGrp A
                    ON G.cod_grupo = A.cod_grupo
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, G.cod_grupo;";

            const string carteraQuery = @"
                SELECT
                    R.cod_clasificacion AS cartera,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM CBR_CLASIFICACION_CARTERA R
                LEFT JOIN CBR_CLASIFICACION_DETALLE A
                    ON R.cod_clasificacion = A.cod_clasificacion
                    AND A.codigo = @Codigo
                ORDER BY asignado DESC, R.cod_clasificacion;";

            const string refundiblesQuery = @"
                SELECT
                    R.codigo,
                    ISNULL(R.descripcion, '') AS descripcion,
                    CONVERT(bit, CASE WHEN A.codigo IS NULL THEN 0 ELSE 1 END) AS refunde
                FROM CATALOGO R
                LEFT JOIN CRD_CATALOGO_REFUNDIBLES A
                    ON R.codigo = A.cod_refundible
                    AND A.codigo = @Codigo
                ORDER BY refunde DESC, R.codigo;";

            var parametros = new { Codigo = codigo };
            var destinos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionDestinoData>(_portalDb, codEmpresa, destinosQuery, parametros);
            if (destinos.Code < 0) return ErrorAsignaciones(destinos.Description);

            var cargos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionCargoData>(_portalDb, codEmpresa, cargosQuery, parametros);
            if (cargos.Code < 0) return ErrorAsignaciones(cargos.Description);

            var requisitos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionRequisitoData>(_portalDb, codEmpresa, requisitosQuery, parametros);
            if (requisitos.Code < 0) return ErrorAsignaciones(requisitos.Description);

            var recursos = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionRecursoData>(_portalDb, codEmpresa, recursosQuery, parametros);
            if (recursos.Code < 0) return ErrorAsignaciones(recursos.Description);

            var cartera = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionCarteraData>(_portalDb, codEmpresa, carteraQuery, parametros);
            if (cartera.Code < 0) return ErrorAsignaciones(cartera.Description);

            var refundibles = DbHelper.ExecuteListQuery<CrCatalogoCreditoAsignacionRefundibleData>(_portalDb, codEmpresa, refundiblesQuery, parametros);
            if (refundibles.Code < 0) return ErrorAsignaciones(refundibles.Description);

            return new ErrorDto<CrCatalogoCreditoAsignacionesData>
            {
                Code = 0,
                Description = "OK",
                Result = new CrCatalogoCreditoAsignacionesData
                {
                    destinos = destinos.Result ?? [],
                    cargos = cargos.Result ?? [],
                    requisitos = requisitos.Result ?? [],
                    recursos = recursos.Result ?? [],
                    cartera = cartera.Result ?? [],
                    refundibles = refundibles.Result ?? []
                }
            };
        }

        /// <summary>
        /// Obtiene la lista de adjuntos disponibles para solicitudes en linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoAdjuntoData>> CrCatalogoCreditos_Adjuntos_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<CrCatalogoCreditoAdjuntoData>>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito.",
                    Result = []
                };
            }

            const string query = @"
                SELECT
                    R.COD_ADJUNTO AS id,
                    ISNULL(R.DESCRIPCION, '') AS descripcion,
                    CONVERT(bit, ISNULL(A.opcional, 0)) AS opcional,
                    CONVERT(bit, CASE WHEN A.COD_ADJUNTO IS NULL THEN 0 ELSE 1 END) AS asignado
                FROM CRD_ADJUNTOS_TIPOS R
                LEFT JOIN CRD_CATALOGO_ADJUNTOS A
                    ON R.COD_ADJUNTO = A.COD_ADJUNTO
                    AND A.CODIGO = @Codigo
                ORDER BY asignado DESC, R.COD_ADJUNTO;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoAdjuntoData>(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = codigo });
        }

        /// <summary>
        /// Guarda una asignacion de destinos, cargos, requisitos, recursos, cartera o refundibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_Asignacion_Guardar(int codEmpresa, CrCatalogoCreditoAsignacionGuardarRequest request)
        {
            NormalizarAsignacionRequest(request);

            if (string.IsNullOrWhiteSpace(request.codigo) || string.IsNullOrWhiteSpace(request.codigo_asignacion))
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y el codigo de asignacion." };
            }

            var query = request.tipo switch
            {
                "destinos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM catalogo_DestinosAsg WHERE codigo = @Codigo AND cod_destino = @CodigoAsignacion)
                        INSERT catalogo_DestinosAsg(codigo, cod_destino) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE catalogo_DestinosAsg WHERE codigo = @Codigo AND cod_destino = @CodigoAsignacion;",
                "cargos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM cargos_asignacion WHERE codigo = @Codigo AND cod_cargo = @CodigoAsignacion)
                        INSERT cargos_asignacion(codigo, cod_cargo) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE cargos_asignacion WHERE codigo = @Codigo AND cod_cargo = @CodigoAsignacion;",
                "requisitos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM requisitos_asignacion WHERE codigo = @Codigo AND cod_requisito = @CodigoAsignacion)
                            INSERT requisitos_asignacion(codigo, cod_requisito, opcional) VALUES(@Codigo, @CodigoAsignacion, @Opcional);
                        ELSE
                            UPDATE requisitos_asignacion SET opcional = @Opcional WHERE codigo = @Codigo AND cod_requisito = @CodigoAsignacion;"
                    : @"DELETE requisitos_asignacion WHERE codigo = @Codigo AND cod_requisito = @CodigoAsignacion;",
                "recursos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM catalogo_asignaGrp WHERE codigo = @Codigo AND cod_grupo = @CodigoAsignacion)
                        INSERT catalogo_asignaGrp(codigo, cod_grupo) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE catalogo_asignaGrp WHERE codigo = @Codigo AND cod_grupo = @CodigoAsignacion;",
                "cartera" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM CBR_CLASIFICACION_DETALLE WHERE codigo = @Codigo AND cod_clasificacion = @CodigoAsignacion)
                        INSERT CBR_CLASIFICACION_DETALLE(codigo, cod_clasificacion) VALUES(@Codigo, @CodigoAsignacion);"
                    : @"DELETE CBR_CLASIFICACION_DETALLE WHERE codigo = @Codigo AND cod_clasificacion = @CodigoAsignacion;",
                "refundibles" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CATALOGO_REFUNDIBLES WHERE codigo = @Codigo AND cod_refundible = @CodigoAsignacion)
                        INSERT CRD_CATALOGO_REFUNDIBLES(codigo, cod_refundible, registro_fecha, registro_usuario)
                        VALUES(@Codigo, @CodigoAsignacion, dbo.mygetdate(), @Usuario);"
                    : @"DELETE CRD_CATALOGO_REFUNDIBLES WHERE codigo = @Codigo AND cod_refundible = @CodigoAsignacion;",
                "adjuntos" => request.asignado
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CATALOGO_ADJUNTOS WHERE codigo = @Codigo AND COD_ADJUNTO = @CodigoAsignacion)
                            INSERT CRD_CATALOGO_ADJUNTOS(codigo, COD_ADJUNTO, opcional, REGISTRO_USUARIO, REGISTRO_FECHA)
                            VALUES(@Codigo, @CodigoAsignacion, @Opcional, @Usuario, dbo.mygetdate());
                        ELSE
                            UPDATE CRD_CATALOGO_ADJUNTOS SET opcional = @Opcional WHERE codigo = @Codigo AND COD_ADJUNTO = @CodigoAsignacion;"
                    : @"DELETE CRD_CATALOGO_ADJUNTOS WHERE codigo = @Codigo AND COD_ADJUNTO = @CodigoAsignacion;",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto { Code = -1, Description = "Tipo de asignacion invalido." };
            }

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    CodigoAsignacion = request.codigo_asignacion,
                    Opcional = request.opcional ? 1 : 0,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.asignado ? "Registra - WEB" : "Borrar - WEB",
                    $"Catalogo Creditos > {request.tipo}: {request.codigo_asignacion} a la Linea: {request.codigo}");
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene los rangos base de monto, plazo y garantias de la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCreditoRangosBaseData> CrCatalogoCreditos_RangosBase_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCatalogoCreditoRangosBaseData>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            const string rangosQuery = @"
                SELECT
                    consec,
                    ISNULL(de, 0) AS de,
                    ISNULL(hasta, 0) AS hasta,
                    ISNULL(plazo, 0) AS plazo,
                    ISNULL(intc_soc, 0) AS intc_soc,
                    ISNULL(intm_soc, 0) AS intm_soc,
                    ISNULL(intc_nsoc, 0) AS intc_nsoc,
                    ISNULL(intm_nsoc, 0) AS intm_nsoc
                FROM Rangos
                WHERE codigo = @Codigo
                ORDER BY consec;";

            const string plazosQuery = @"
                SELECT
                    consec,
                    ISNULL(desde, 0) AS desde,
                    ISNULL(hasta, 0) AS hasta,
                    ISNULL(tasa, 0) AS tasa
                FROM Rangos_plazo
                WHERE codigo = @Codigo
                ORDER BY consec;";

            const string garantiasQuery = @"
                SELECT
                    G.garantia,
                    ISNULL(G.descripcion, '') AS descripcion,
                    ISNULL(A.utiliza_tasa_garantia, 0) AS utiliza_tasa_garantia,
                    ISNULL(A.tasa_garantia, 0) AS tasa_garantia,
                    ISNULL(A.utiliza_tasa_piso, 0) AS utiliza_tasa_piso,
                    ISNULL(A.tasa_piso, 0) AS tasa_piso,
                    ISNULL(A.utiliza_tasa_techo, 0) AS utiliza_tasa_techo,
                    ISNULL(A.tasa_techo, 0) AS tasa_techo,
                    ISNULL(A.utiliza_maximos, 0) AS utiliza_maximos,
                    ISNULL(A.max_monto, 0) AS max_monto,
                    ISNULL(A.liquidez_minima, 0) AS liquidez_minima
                FROM crd_garantia_Tipos G
                INNER JOIN crd_catalogo_garantias A
                    ON G.garantia = A.garantia
                WHERE A.codigo = @Codigo
                ORDER BY G.garantia;";

            var parametros = new { Codigo = codigo };
            var rangos = DbHelper.ExecuteListQuery<CrCatalogoCreditoRangoBaseData>(_portalDb, codEmpresa, rangosQuery, parametros);
            if (rangos.Code < 0) return ErrorRangosBase(rangos.Description);

            var plazos = DbHelper.ExecuteListQuery<CrCatalogoCreditoRangoPlazoData>(_portalDb, codEmpresa, plazosQuery, parametros);
            if (plazos.Code < 0) return ErrorRangosBase(plazos.Description);

            var garantias = DbHelper.ExecuteListQuery<CrCatalogoCreditoRangoGarantiaData>(_portalDb, codEmpresa, garantiasQuery, parametros);
            if (garantias.Code < 0) return ErrorRangosBase(garantias.Description);

            return new ErrorDto<CrCatalogoCreditoRangosBaseData>
            {
                Code = 0,
                Description = "OK",
                Result = new CrCatalogoCreditoRangosBaseData
                {
                    rangos = rangos.Result ?? [],
                    tasasPlazos = plazos.Result ?? [],
                    garantias = garantias.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda un rango base por monto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrCatalogoCreditos_RangoBase_Guardar(int codEmpresa, CrCatalogoCreditoRangoBaseGuardarRequest request)
        {
            NormalizarRangoRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto<int> { Code = -1, Description = "Debe consultar una linea de credito." };
            }

            const string query = @"
                IF ISNULL(@Consec, 0) = 0
                BEGIN
                    INSERT INTO Rangos(codigo, de, hasta, plazo, intc_soc, intm_soc, intc_nsoc, intm_nsoc)
                    VALUES(@Codigo, @De, @Hasta, @Plazo, @IntcSoc, @IntmSoc, @IntcNsoc, @IntmNsoc);

                    SELECT ISNULL(MAX(consec), 0)
                    FROM Rangos
                    WHERE codigo = @Codigo;
                END
                ELSE
                BEGIN
                    UPDATE Rangos
                    SET de = @De,
                        hasta = @Hasta,
                        plazo = @Plazo,
                        intc_soc = @IntcSoc,
                        intm_soc = @IntmSoc,
                        intc_nsoc = @IntcNsoc,
                        intm_nsoc = @IntmNsoc
                    WHERE consec = @Consec;

                    SELECT @Consec;
                END";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new
                {
                    Codigo = request.codigo,
                    Consec = request.rango.consec,
                    De = request.rango.de,
                    Hasta = request.rango.hasta,
                    Plazo = request.rango.plazo,
                    IntcSoc = request.rango.intc_soc,
                    IntmSoc = request.rango.intm_soc,
                    IntcNsoc = request.rango.intc_nsoc,
                    IntmNsoc = request.rango.intm_nsoc
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.rango.consec == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Rango para el Codigo: {request.codigo} ID:{respuesta.Result}");
            }

            return respuesta;
        }

        /// <summary>
        /// Guarda un rango de tasa por plazo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrCatalogoCreditos_RangoPlazo_Guardar(int codEmpresa, CrCatalogoCreditoRangoPlazoGuardarRequest request)
        {
            NormalizarRangoRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto<int> { Code = -1, Description = "Debe consultar una linea de credito." };
            }

            const string query = @"
                IF ISNULL(@Consec, 0) = 0
                BEGIN
                    INSERT INTO Rangos_Plazo(codigo, desde, hasta, tasa)
                    VALUES(@Codigo, @Desde, @Hasta, @Tasa);

                    SELECT ISNULL(MAX(consec), 0)
                    FROM Rangos_Plazo
                    WHERE codigo = @Codigo;
                END
                ELSE
                BEGIN
                    UPDATE Rangos_Plazo
                    SET desde = @Desde,
                        hasta = @Hasta,
                        tasa = @Tasa
                    WHERE consec = @Consec;

                    SELECT @Consec;
                END";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new
                {
                    Codigo = request.codigo,
                    Consec = request.rango.consec,
                    Desde = request.rango.desde,
                    Hasta = request.rango.hasta,
                    Tasa = request.rango.tasa
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.rango.consec == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Rango Plazo para el Codigo: {request.codigo} ID:{respuesta.Result}");
            }

            return respuesta;
        }

        /// <summary>
        /// Guarda la configuracion de tasas y maximos por garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_RangoGarantia_Guardar(int codEmpresa, CrCatalogoCreditoRangoGarantiaGuardarRequest request)
        {
            NormalizarRangoGarantiaRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || string.IsNullOrWhiteSpace(request.garantia.garantia))
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y la garantia." };
            }

            const string query = @"
                UPDATE crd_catalogo_garantias
                SET utiliza_tasa_garantia = @UtilizaTasaGarantia,
                    tasa_garantia = @TasaGarantia,
                    utiliza_tasa_piso = @UtilizaTasaPiso,
                    tasa_piso = @TasaPiso,
                    utiliza_tasa_techo = @UtilizaTasaTecho,
                    tasa_techo = @TasaTecho,
                    utiliza_maximos = @UtilizaMaximos,
                    max_monto = @MaxMonto,
                    liquidez_minima = @LiquidezMinima,
                    actualiza_fecha = dbo.MyGetdate(),
                    actualiza_usuario = @Usuario
                WHERE codigo = @Codigo
                    AND garantia = @Garantia;";

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Garantia = request.garantia.garantia,
                    UtilizaTasaGarantia = request.garantia.utiliza_tasa_garantia ? 1 : 0,
                    TasaGarantia = request.garantia.tasa_garantia,
                    UtilizaTasaPiso = request.garantia.utiliza_tasa_piso ? 1 : 0,
                    TasaPiso = request.garantia.tasa_piso,
                    UtilizaTasaTecho = request.garantia.utiliza_tasa_techo ? 1 : 0,
                    TasaTecho = request.garantia.tasa_techo,
                    UtilizaMaximos = request.garantia.utiliza_maximos ? 1 : 0,
                    MaxMonto = request.garantia.max_monto,
                    LiquidezMinima = request.garantia.liquidez_minima,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica - WEB",
                    $"Garantia: {request.garantia.descripcion} Linea: {request.codigo}");
            }

            return respuesta;
        }

        /**
         * Obtiene los rangos por liquidez de la linea.
         * @param codEmpresa Codigo de empresa.
         * @param codigo Codigo de linea de credito.
         */
        public ErrorDto<CrCatalogoCreditoRangosLiquidezData> CrCatalogoCreditos_RangosLiquidez_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCatalogoCreditoRangosLiquidezData>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            var ensure = AsegurarTablasRangosLiquidez(codEmpresa);
            if (ensure.Code < 0)
            {
                return ErrorRangosLiquidez(ensure.Description);
            }

            const string bonoQuery = @"
                SELECT id,
                    pago_inicial,
                    pago_final,
                    puntos_bonificacion
                FROM CRD_CATALOGO_LIQUIDEZ_BONO
                WHERE codigo = @Codigo
                ORDER BY id;";

            const string capacidadQuery = @"
                SELECT id,
                    capacidad_inicio,
                    capacidad_corte,
                    porc_giro_maximo,
                    porcentaje_olgura
                FROM CRD_CATALOGO_LIQUIDEZ_CAPACIDAD
                WHERE codigo = @Codigo
                ORDER BY id;";

            var parametros = new { Codigo = codigo };
            var bono = DbHelper.ExecuteListQuery<CrCatalogoCreditoLiquidezBonoData>(_portalDb, codEmpresa, bonoQuery, parametros);
            if (bono.Code < 0) return ErrorRangosLiquidez(bono.Description);

            var capacidad = DbHelper.ExecuteListQuery<CrCatalogoCreditoLiquidezCapacidadData>(_portalDb, codEmpresa, capacidadQuery, parametros);
            if (capacidad.Code < 0) return ErrorRangosLiquidez(capacidad.Description);

            return new ErrorDto<CrCatalogoCreditoRangosLiquidezData>
            {
                Code = 0,
                Description = "OK",
                Result = new CrCatalogoCreditoRangosLiquidezData
                {
                    bono = bono.Result ?? [],
                    capacidad = capacidad.Result ?? []
                }
            };
        }

        /**
         * Guarda un rango por liquidez de bono.
         * @param codEmpresa Codigo de empresa.
         * @param request Datos del rango.
         */
        public ErrorDto CrCatalogoCreditos_LiquidezBono_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezBonoGuardarRequest request)
        {
            NormalizarLiquidezBonoRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || request.rango.id <= 0)
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y el rango." };
            }

            var ensure = AsegurarTablasRangosLiquidez(codEmpresa);
            if (ensure.Code < 0)
            {
                return ensure;
            }

            const string query = @"
                IF EXISTS (SELECT 1 FROM CRD_CATALOGO_LIQUIDEZ_BONO WHERE codigo = @Codigo AND id = @Id)
                BEGIN
                    UPDATE CRD_CATALOGO_LIQUIDEZ_BONO
                    SET pago_inicial = @PagoInicial,
                        pago_final = @PagoFinal,
                        puntos_bonificacion = @PuntosBonificacion,
                        modifica_fecha = dbo.MyGetdate(),
                        modifica_usuario = @Usuario
                    WHERE codigo = @Codigo
                        AND id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO CRD_CATALOGO_LIQUIDEZ_BONO(
                        codigo, id, pago_inicial, pago_final, puntos_bonificacion, registro_fecha, registro_usuario)
                    VALUES(
                        @Codigo, @Id, @PagoInicial, @PagoFinal, @PuntosBonificacion, dbo.MyGetdate(), @Usuario);
                END";

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Id = request.rango.id,
                    PagoInicial = request.rango.pago_inicial,
                    PagoFinal = request.rango.pago_final,
                    PuntosBonificacion = request.rango.puntos_bonificacion,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica - WEB",
                    $"Rango Liquidez Bono Linea: {request.codigo} ID:{request.rango.id}");
            }

            return respuesta;
        }

        /**
         * Guarda un rango por liquidez de capacidad de pago.
         * @param codEmpresa Codigo de empresa.
         * @param request Datos del rango.
         */
        public ErrorDto CrCatalogoCreditos_LiquidezCapacidad_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezCapacidadGuardarRequest request)
        {
            NormalizarLiquidezCapacidadRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || request.rango.id <= 0)
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y el rango." };
            }

            var ensure = AsegurarTablasRangosLiquidez(codEmpresa);
            if (ensure.Code < 0)
            {
                return ensure;
            }

            const string query = @"
                IF EXISTS (SELECT 1 FROM CRD_CATALOGO_LIQUIDEZ_CAPACIDAD WHERE codigo = @Codigo AND id = @Id)
                BEGIN
                    UPDATE CRD_CATALOGO_LIQUIDEZ_CAPACIDAD
                    SET capacidad_inicio = @CapacidadInicio,
                        capacidad_corte = @CapacidadCorte,
                        porc_giro_maximo = @PorcGiroMaximo,
                        porcentaje_olgura = @PorcentajeOlgura,
                        modifica_fecha = dbo.MyGetdate(),
                        modifica_usuario = @Usuario
                    WHERE codigo = @Codigo
                        AND id = @Id;
                END
                ELSE
                BEGIN
                    INSERT INTO CRD_CATALOGO_LIQUIDEZ_CAPACIDAD(
                        codigo, id, capacidad_inicio, capacidad_corte, porc_giro_maximo, porcentaje_olgura, registro_fecha, registro_usuario)
                    VALUES(
                        @Codigo, @Id, @CapacidadInicio, @CapacidadCorte, @PorcGiroMaximo, @PorcentajeOlgura, dbo.MyGetdate(), @Usuario);
                END";

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Id = request.rango.id,
                    CapacidadInicio = request.rango.capacidad_inicio,
                    CapacidadCorte = request.rango.capacidad_corte,
                    PorcGiroMaximo = request.rango.porc_giro_maximo,
                    PorcentajeOlgura = request.rango.porcentaje_olgura,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica - WEB",
                    $"Rango Liquidez Capacidad Linea: {request.codigo} ID:{request.rango.id}");
            }

            return respuesta;
        }

        /// <summary>
        /// Obtiene los comites de estudio de credito configurables por linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoComiteEstudioData>> CrCatalogoCreditos_ComitesEstudio_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<CrCatalogoCreditoComiteEstudioData>>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            const string query = "EXEC spCRD_ComitesPreanalisis_Consulta @Codigo;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoComiteEstudioData>(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = codigo });
        }

        /// <summary>
        /// Guarda el porcentaje de extras por comite para estudio de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrCatalogoCreditos_ComiteEstudio_Guardar(int codEmpresa, CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            NormalizarComiteEstudioRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || request.comite.id_comite <= 0)
            {
                return new ErrorDto<int>
                {
                    Code = -1,
                    Description = "Debe indicar la linea y el comite."
                };
            }

            const string query = "EXEC spCrd_ComitesPreanalisis_Add @Id, @Codigo, @IdComite, @Porcentaje, @Usuario;";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                request.comite.id,
                new
                {
                    Id = request.comite.id,
                    Codigo = request.codigo,
                    IdComite = request.comite.id_comite,
                    Porcentaje = request.comite.porcentaje,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.comite.id == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Config: Porc. Extras [Linea: {request.codigo}, Id Reg: {respuesta.Result}...Comite: {request.comite.comite}] Porc: {request.comite.porcentaje:N2}");
            }

            return respuesta;
        }

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
        /// Valida si existe la linea de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private bool CrCatalogoCreditos_Existe(int codEmpresa, string codigo)
        {
            const string query = "SELECT ISNULL(COUNT(*), 0) FROM catalogo WHERE codigo = @Codigo;";
            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new { Codigo = codigo.Trim().ToUpperInvariant() });

            return respuesta.Result > 0;
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
        /// Carga las opciones CPH asociadas a la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="data"></param>
        private void CrCatalogoCreditos_Cph_Obtener(int codEmpresa, CrCatalogoCreditoData data)
        {
            const string query = @"
                SELECT COD_CPH
                FROM CRD_FORMULARIO_CPH
                WHERE COD_LINEA = @Codigo;";

            var respuesta = DbHelper.ExecuteListQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = data.codigo.Trim().ToUpperInvariant() });

            if (respuesta.Code < 0 || respuesta.Result is null)
                return;

            data.cph1 = respuesta.Result.Contains(1);
            data.cph2 = respuesta.Result.Contains(2);
            data.cph3 = respuesta.Result.Contains(3);
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

        private static void NormalizarRequest(CrCatalogoCreditoGuardarRequest request)
        {
            request.codigo = request.codigo.Trim().ToUpperInvariant();
            request.codigoa = request.codigoa?.Trim().ToUpperInvariant() ?? string.Empty;
            request.descripcion = request.descripcion?.Trim().ToUpperInvariant() ?? string.Empty;
            request.notas = request.notas?.Trim() ?? string.Empty;
            request.oficina = request.oficina?.Trim().ToUpperInvariant() ?? string.Empty;
            request.oficina_desc = request.oficina_desc?.Trim() ?? string.Empty;
            request.reserva_codigo = request.reserva_codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.reserva_plan_desc = request.reserva_plan_desc?.Trim() ?? string.Empty;
            request.revolutiva_plan_ahorro = request.revolutiva_plan_ahorro?.Trim().ToUpperInvariant() ?? string.Empty;
            request.plan_ahorro_desc = request.plan_ahorro_desc?.Trim() ?? string.Empty;
            request.convenio = NormalizarSiNo(request.convenio);
            request.poliza = NormalizarSiNo(request.poliza);
            request.refunde = NormalizarSiNo(request.refunde);
            request.retencion = NormalizarSiNo(request.retencion);
            request.aceptarefun = NormalizarSiNo(request.aceptarefun);
            request.primer_cuota = NormalizarSiNo(request.primer_cuota);
            request.pidecheque = NormalizarSiNo(request.pidecheque);
            request.tramite = string.IsNullOrWhiteSpace(request.tramite) ? "C" : request.tramite.Trim().ToUpperInvariant()[..1];
            request.requisitos_tipo = string.IsNullOrWhiteSpace(request.requisitos_tipo) ? "L" : request.requisitos_tipo.Trim().ToUpperInvariant()[..1];
            request.refunde_tipo = string.IsNullOrWhiteSpace(request.refunde_tipo) ? "P" : request.refunde_tipo.Trim().ToUpperInvariant()[..1];
            request.liq_tipoaumento = string.IsNullOrWhiteSpace(request.liq_tipoaumento) ? "F" : request.liq_tipoaumento.Trim().ToUpperInvariant()[..1];
            request.cobro_tipo_aplicacion = string.IsNullOrWhiteSpace(request.cobro_tipo_aplicacion) ? "V" : request.cobro_tipo_aplicacion.Trim().ToUpperInvariant()[..1];
            request.tasa_mora_tipo = string.IsNullOrWhiteSpace(request.tasa_mora_tipo) ? "N/A" : request.tasa_mora_tipo.Trim().ToUpperInvariant();
            request.auto_gestion_tipo = string.IsNullOrWhiteSpace(request.auto_gestion_tipo) ? "C" : request.auto_gestion_tipo.Trim().ToUpperInvariant()[..1];
            request.mov_sinpe_tipos = request.mov_sinpe_tipos <= 0 ? 3 : request.mov_sinpe_tipos;
        }

        private static void NormalizarPeLRequest(CrCatalogoCreditoPeLGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.df_descripcion_linea = request.df_descripcion_linea?.Trim() ?? string.Empty;
            request.df_uso_destino_linea = request.df_uso_destino_linea?.Trim() ?? string.Empty;
            request.df_logo_url = request.df_logo_url?.Trim() ?? string.Empty;
            request.df_etiqueta_aprobacion = request.df_etiqueta_aprobacion?.Trim() ?? string.Empty;
            request.df_etiqueta_monto_max = request.df_etiqueta_monto_max?.Trim() ?? string.Empty;
            request.df_etiqueta_plazo_tasa = request.df_etiqueta_plazo_tasa?.Trim() ?? string.Empty;
            request.df_etiqueta_deposito = request.df_etiqueta_deposito?.Trim() ?? string.Empty;
            request.df_color_caja = string.IsNullOrWhiteSpace(request.df_color_caja)
                ? "#415CBF"
                : request.df_color_caja.Trim().ToUpperInvariant();
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }

        private static void NormalizarAsignacionRequest(CrCatalogoCreditoAsignacionGuardarRequest request)
        {
            request.tipo = request.tipo?.Trim().ToLowerInvariant() ?? string.Empty;
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.codigo_asignacion = request.codigo_asignacion?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }

        private static void NormalizarRangoRequest(CrCatalogoCreditoRangoBaseGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }

        private static void NormalizarRangoRequest(CrCatalogoCreditoRangoPlazoGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }

        private static void NormalizarRangoGarantiaRequest(CrCatalogoCreditoRangoGarantiaGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
            request.garantia.garantia = request.garantia.garantia?.Trim().ToUpperInvariant() ?? string.Empty;
        }

        private static void NormalizarLiquidezBonoRequest(CrCatalogoCreditoLiquidezBonoGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }

        private static void NormalizarLiquidezCapacidadRequest(CrCatalogoCreditoLiquidezCapacidadGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
        }

        private static void NormalizarComiteEstudioRequest(CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;
            request.comite.linea = request.comite.linea?.Trim().ToUpperInvariant() ?? string.Empty;
            request.comite.comite = request.comite.comite?.Trim() ?? string.Empty;
        }

        private static string NormalizarSiNo(string? valor)
        {
            return string.Equals(valor?.Trim(), "S", StringComparison.OrdinalIgnoreCase) ? "S" : "N";
        }

        private static ErrorDto<CrCatalogoCreditoAsignacionesData> ErrorAsignaciones(string? descripcion)
        {
            return new ErrorDto<CrCatalogoCreditoAsignacionesData>
            {
                Code = -1,
                Description = descripcion ?? "Ocurrio un error al obtener asignaciones de la linea."
            };
        }

        private static ErrorDto<CrCatalogoCreditoRangosBaseData> ErrorRangosBase(string? descripcion)
        {
            return new ErrorDto<CrCatalogoCreditoRangosBaseData>
            {
                Code = -1,
                Description = descripcion ?? "Ocurrio un error al obtener rangos base de la linea."
            };
        }

        private static ErrorDto<CrCatalogoCreditoRangosLiquidezData> ErrorRangosLiquidez(string? descripcion)
        {
            return new ErrorDto<CrCatalogoCreditoRangosLiquidezData>
            {
                Code = -1,
                Description = descripcion ?? "Ocurrio un error al obtener rangos por liquidez de la linea."
            };
        }

        private ErrorDto AsegurarTablasRangosLiquidez(int codEmpresa)
        {
            const string query = @"
                IF OBJECT_ID('dbo.CRD_CATALOGO_LIQUIDEZ_BONO', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.CRD_CATALOGO_LIQUIDEZ_BONO(
                        codigo varchar(4) NOT NULL,
                        id int NOT NULL,
                        pago_inicial decimal(18, 4) NULL,
                        pago_final decimal(18, 4) NULL,
                        puntos_bonificacion decimal(18, 4) NULL,
                        registro_fecha datetime NULL,
                        registro_usuario varchar(50) NULL,
                        modifica_fecha datetime NULL,
                        modifica_usuario varchar(50) NULL,
                        CONSTRAINT PK_CRD_CATALOGO_LIQUIDEZ_BONO PRIMARY KEY(codigo, id)
                    );
                END

                IF OBJECT_ID('dbo.CRD_CATALOGO_LIQUIDEZ_CAPACIDAD', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.CRD_CATALOGO_LIQUIDEZ_CAPACIDAD(
                        codigo varchar(4) NOT NULL,
                        id int NOT NULL,
                        capacidad_inicio decimal(18, 4) NULL,
                        capacidad_corte decimal(18, 4) NULL,
                        porc_giro_maximo decimal(18, 4) NULL,
                        porcentaje_olgura decimal(18, 4) NULL,
                        registro_fecha datetime NULL,
                        registro_usuario varchar(50) NULL,
                        modifica_fecha datetime NULL,
                        modifica_usuario varchar(50) NULL,
                        CONSTRAINT PK_CRD_CATALOGO_LIQUIDEZ_CAPACIDAD PRIMARY KEY(codigo, id)
                    );
                END";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { });
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
