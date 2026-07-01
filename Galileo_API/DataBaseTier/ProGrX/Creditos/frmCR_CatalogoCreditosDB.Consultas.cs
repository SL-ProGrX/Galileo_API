using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {

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
        /// Valida si una linea permite cambiar las marcas de retencion o poliza.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codigo">Codigo de linea de credito.</param>
        /// <returns>Verdadero si permite aplicar el cambio.</returns>
        public ErrorDto<bool> CrCatalogoCreditos_PermiteCambioRetencionPoliza(int codEmpresa, string codigo)
        {
            const string query = @"
                SELECT ISNULL(COUNT(1), 0)
                FROM reg_Creditos
                WHERE estado = 'A'
                  AND saldo > 0
                  AND codigo = @Codigo;";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new { Codigo = codigo.Trim().ToUpperInvariant() });

            if (respuesta.Code < 0)
            {
                return new ErrorDto<bool>
                {
                    Code = respuesta.Code,
                    Description = respuesta.Description,
                    Result = false
                };
            }

            return DbHelper.CreateOkResponse(respuesta.Result <= 1);
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
    }
}
