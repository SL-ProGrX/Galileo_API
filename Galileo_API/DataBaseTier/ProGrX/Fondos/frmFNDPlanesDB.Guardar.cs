using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {
       
        private const string SqlExistePlan = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_Planes
                    WHERE cod_operadora = @codoperadora
                      AND cod_plan = @codplan;";

        private const string SqlUpdatePlan = @"
                    UPDATE dbo.FND_Planes SET
                        descripcion = @descripcion,
                        tipo_deduc = @tipodeduc,
                        porc_deduc = @porc_deduc,
                        notas = @notas,
                        plazo_minimo = @plazo_minimo,
                        monto_minimo = @monto_minimo,
                        cuenta_conta = @cuenta_conta,
                        cuenta_gasto = @cuenta_gasto,
                        codigo_ase = @codigo_ase,
                        estado = @estado,
                        sirve_garantia = @sirve_garantia,
                        calcula_rend = @calcula_rend,
                        cod_grupo = @codgrupo,
                        cuenta_maestra = @cuenta_maestra,
                        visible_ec = @visible_ec,
                        apl_liq_socio = @apl_liq_socio,
                        website = @website,
                        plazo_tipo = @plazo_tipo,
                        inversion_minimo = @inversion_minimo,
                        cod_moneda = @cod_moneda,
                        garantia_porc_disp = @garantia_porc_disp,
                        garantia_integrada = @garantia_integrada,
                        garantia_tasaad = @garantia_tasa_ad,
                        capitaliza_rendimientos = @capitaliza_rendimientos,
                        base_calculo = @base_calculo,
                        tasa_base = @tasa_base,
                        utiliza_tbp = @utiliza_tbp,
                        utiliza_tasa_fluctuante = @utiliza_tasa_fluctuante,
                        deducir_planilla = @deducir_planilla,
                        genera_mora = @genera_mora,
                        controla_saldo = @controla_saldo,
                        tipo_cdp = @tipo_cdp,
                        permite_mov_cajas = @permite_mov_cajas,
                        num_contratos_activos = @num_contratos_activos,
                        tasa_comision_aportes = @tasa_comision_aportes,
                        tasa_comision_rend = @tasa_comision_rend,
                        cuenta_comision_adm = @ctacomisionadm,
                        cuenta_ing_retiros = @ctaingretiros,
                        cuenta_gst_comision = @ctagstcomision,
                        comision_vta_monto = @comision_vta_monto,
                        comision_vta_inv = @comision_vta_inv,
                        cuenta_rendimiento = @ctarnd,
                        deduce_independiente = @deduce_independiente,
                        requiere_beneficiarios = @requiere_beneficiarios,
                        patrimonio_enlace = @patrimonio_enlace,
                        patrimonio_tipo = @patrimonio_tipo,
                        web_vence = @web_vence,
                        tasa_ajuste_vencimiento = @tasa_ajuste_vencimiento,
                        tasa_ajuste = @tasa_ajuste,
                        web_liquida = @web_liquida,
                        permite_giro_terceros = @permite_giro_terceros,
                        permite_retiros_cajas = @permite_retiros_cajas,
                        cuenta_impuestos = @ctaimpuesto,
                        impuesto_renta = @impuesto_renta,
                        sinpe_cuenta = @sinpe_cuenta,
                        sinpe_producto = dbo.fxFndSinpeProducto(@codplan),
                        mov_entre_fondos = @mov_entre_fondos,
                        forma_pago_pos = @forma_pago_pos,
                        mov_entre_fondos_terceros = @mov_entre_fondos_terceros,
                        renta_global = @renta_global,
                        apl_rend_automatico = @apl_rend_automatico,
                        permite_ret_parcial = @permite_ret_parcial,
                        patrimonio_unifica = @patrimonio_unifica,
                        vence_accion = @vence_accion,
                        vence_plan = @vence_plan,
                        vence_renueva = @vence_renueva,
                        cod_tipo_plan = @codtipoplan,
                        aplicar_tasa_cont_vencidos = @aplicar_tasa_cont_vencidos,
                        aplicar_en_procs_contrs_vencidos = @aplicar_en_procs_contrs_vencidos,
                        mov_sinpe_tipos = @mov_sinpe_tipos,
                        mov_sinpe = @mov_sinpe,
                        SINPE_PROC_ENLINEA = @sinpe_proc_enlinea,
                        SINPE_PROC_INTERNO = @sinpe_proc_interno,
                        sif_liquida = @sif_liquida,
                        pago_cupones = @pago_cupones,
                        IndAplicarAMora = @ind_aplicar_amora,
                        web_crear = @web_crear,
                        web_modifica_couta = @web_modifica_couta,
                        tasa_margen_negociacion = @tasa_margen_negociacion,
                        vence_notifica = @vence_notifica,
                        SubCuentasMax = @subcuentas_max,
                        web_valida_vence = 0
                    WHERE cod_operadora = @codoperadora
                      AND cod_plan = @codplan;";

        private const string SqlInsertPlan = @"
                    INSERT INTO dbo.FND_Planes
                    (
                        cod_operadora, cod_plan, descripcion, tipo_deduc, porc_deduc, notas,
                        plazo_minimo, monto_minimo, cuenta_conta, cuenta_gasto,
                        codigo_ase, estado, sirve_garantia, calcula_rend, consecutivo, rend_corte,
                        cod_grupo, cuenta_maestra, visible_ec, apl_liq_socio,
                        website, plazo_tipo, inversion_minimo, cod_moneda,
                        garantia_porc_disp, garantia_integrada, garantia_tasaad,
                        capitaliza_rendimientos, base_calculo, tasa_base,
                        utiliza_tbp, utiliza_tasa_fluctuante, deducir_planilla,
                        genera_mora, controla_saldo, tipo_cdp, permite_mov_cajas,
                        num_contratos_activos, tasa_comision_aportes,
                        tasa_comision_rend, cuenta_comision_adm, cuenta_ing_retiros,
                        cuenta_gst_comision, comision_vta_monto, comision_vta_inv,
                        cuenta_rendimiento, deduce_independiente, codigo_deduc,
                        requiere_beneficiarios, patrimonio_enlace, patrimonio_tipo,
                        web_vence, tasa_ajuste_vencimiento, tasa_ajuste, web_liquida,
                        permite_giro_terceros, permite_retiros_cajas, cuenta_impuestos,
                        impuesto_renta, sinpe_cuenta, sinpe_producto,
                        mov_entre_fondos, forma_pago_pos, mov_entre_fondos_terceros,
                        renta_global, apl_rend_automatico, permite_ret_parcial,
                        patrimonio_unifica, vence_accion, vence_plan,
                        vence_renueva, cod_tipo_plan,
                        aplicar_tasa_cont_vencidos, aplicar_en_procs_contrs_vencidos,
                        mov_sinpe_tipos, mov_sinpe, sif_liquida, pago_cupones,
                        IndAplicarAMora, web_crear, web_modifica_couta,
                        tasa_margen_negociacion, vence_notifica, SubCuentasMax,
                        SINPE_PROC_ENLINEA, SINPE_PROC_INTERNO,
                        forma_pago_servicios, vence_plazo, web_valida_vence
                    )
                    VALUES
                    (
                        @codoperadora, @codplan, @descripcion, @tipodeduc, @porc_deduc, @notas,
                        @plazo_minimo, @monto_minimo, @cuenta_conta, @cuenta_gasto,
                        @codigo_ase, @estado, @sirve_garantia, @calcula_rend, 0, dbo.MyGetdate(),
                        @codgrupo, @cuenta_maestra, @visible_ec, @apl_liq_socio,
                        @website, @plazo_tipo, @inversion_minimo, @cod_moneda,
                        @garantia_porc_disp, @garantia_integrada, @garantia_tasa_ad,
                        @capitaliza_rendimientos, @base_calculo, @tasa_base,
                        @utiliza_tbp, @utiliza_tasa_fluctuante, @deducir_planilla,
                        @genera_mora, @controla_saldo, @tipo_cdp, @permite_mov_cajas,
                        @num_contratos_activos, @tasa_comision_aportes,
                        @tasa_comision_rend, @ctacomisionadm, @ctaingretiros,
                        @ctagstcomision, @comision_vta_monto, @comision_vta_inv,
                        @ctarnd, @deduce_independiente, '',
                        @requiere_beneficiarios, @patrimonio_enlace, @patrimonio_tipo,
                        @web_vence, @tasa_ajuste_vencimiento, @tasa_ajuste, @web_liquida,
                        @permite_giro_terceros, @permite_retiros_cajas, @ctaimpuesto,
                        @impuesto_renta, @sinpe_cuenta, dbo.fxFndSinpeProducto(@codplan),
                        @mov_entre_fondos, @forma_pago_pos, @mov_entre_fondos_terceros,
                        @renta_global, @apl_rend_automatico, @permite_ret_parcial,
                        @patrimonio_unifica, @vence_accion, @vence_plan,
                        @vence_renueva, @codtipoplan,
                        @aplicar_tasa_cont_vencidos, @aplicar_en_procs_contrs_vencidos,
                        @mov_sinpe_tipos, @mov_sinpe, @sif_liquida, @pago_cupones,
                        @ind_aplicar_amora, @web_crear, @web_modifica_couta,
                        @tasa_margen_negociacion, @vence_notifica, @subcuentas_max,
                        @sinpe_proc_enlinea, @sinpe_proc_interno,
                        0, 0, 0
                    );";

        private const string SqlDeletePlan = @"
                    DELETE FROM dbo.FND_Planes
                    WHERE cod_operadora = @codoperadora
                      AND cod_plan = @codplan;";

        private const string SqlUpdateFechaCortePlan = @"
                    UPDATE dbo.FND_Planes
                    SET WEB_VENCE = @fecha
                    WHERE cod_operadora = @codoperadora
                      AND cod_plan = @codplan;";

        private const string SqlFechaServidor = "SELECT dbo.MyGetdate();";

        #region Guardar

        /// <summary>
        /// Inserta o actualiza la configuración de un plan de fondos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="dto">Datos del plan.</param>
        /// <returns>Plan guardado.</returns>
        public ErrorDto<FndPlanDto> Fnd_Plan_Guardar(int CodEmpresa, string usuario, FndPlanDto dto)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse("Los datos del plan son requeridos.", -2, CrearPlanVacio());
            }

            var existe = DbHelper.ExecuteSingleQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlExistePlan,
                0,
                CrearParametrosPlan(usuario, dto));

            if (existe.Code != 0)
            {
                return DbHelper.CreateErrorResponse(existe.Description ?? "Error al validar plan.", existe.Code.GetValueOrDefault(-1), dto);
            }

            var guardar = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                existe.Result > 0 ? SqlUpdatePlan : SqlInsertPlan,
                CrearParametrosPlan(usuario, dto));

            return new ErrorDto<FndPlanDto>
            {
                Code = guardar.Code,
                Description = guardar.Description,
                Result = dto
            };
        }


        /// <summary>
        /// Elimina un plan de fondos por operadora y código de plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="codoperadora">Código de operadora.</param>
        /// <param name="codplan">Código del plan.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto<FndPlanDto> Fnd_Plan_Eliminar(int CodEmpresa, string usuario, int codoperadora, string codplan)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                SqlDeletePlan,
                new
                {
                    codoperadora,
                    codplan = NormalizarTexto(codplan)
                });

            if (result.Code != 0)
            {
                var mensaje = (result.Description ?? string.Empty).Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
                    ? "No se pudo borrar porque el plan está siendo usado en otros registros."
                    : result.Description ?? "Error al eliminar plan.";

                return DbHelper.CreateErrorResponse(mensaje, result.Code.GetValueOrDefault(-1), CrearPlanVacio());
            }

            return result.Result == 0
                ? DbHelper.CreateErrorResponse("No se encontró el plan para eliminar.", -1, CrearPlanVacio())
                : DbHelper.CreateOkResponse(CrearPlanVacio());
        }

        /// <summary>
        /// Actualiza la fecha de corte web de un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="codoperadora">Código de operadora.</param>
        /// <param name="codplan">Código del plan.</param>
        /// <param name="fecha">Fecha de corte.</param>
        /// <returns>Indicador de éxito de la actualización.</returns>
        public ErrorDto<bool> Fnd_Plan_FechaCorte_Update(int CodEmpresa, string usuario, int codoperadora, string codplan, string fecha)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                SqlUpdateFechaCortePlan,
                new
                {
                    codoperadora,
                    codplan = NormalizarTexto(codplan),
                    fecha = NormalizarTexto(fecha)
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al actualizar fecha de corte.", result.Code.GetValueOrDefault(-1), false);
            }

            return result.Result == 0
                ? DbHelper.CreateErrorResponse("No se encontró el plan para actualizar la fecha de corte.", -1, false)
                : DbHelper.CreateOkResponse(true);
        }

        /// <summary>
        /// Obtiene la fecha actual del servidor de base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Fecha del servidor en formato yyyy-MM-dd HH:mm:ss.</returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlFechaServidor,
                DateTime.MinValue);

            return result.Code != 0
                ? DbHelper.ErrorResponse(result.Description ?? "Error al obtener fecha del servidor.", result.Code.GetValueOrDefault(-1))
                : DbHelper.OkResponse(result.Result.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// Crea los parámetros seguros para insertar o actualizar un plan.
        /// </summary>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="dto">Datos del plan.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosPlan(string usuario, FndPlanDto dto)
        {
            return new
            {
                usuario = NormalizarTexto(usuario),
                codoperadora = dto.codoperadora ?? 0,
                codplan = NormalizarTexto(dto.codplan).ToUpperInvariant(),
                descripcion = NormalizarTexto(dto.descripcion).ToUpperInvariant(),
                notas = NormalizarTexto(dto.notas),
                plazo_minimo = dto.plazo_minimo ?? 0,
                monto_minimo = dto.monto_minimo ?? 0,
                cuenta_conta = NormalizarCuentaContable(dto.ctaplan),
                cuenta_gasto = NormalizarCuentaContable(dto.ctagasto),
                dto.codigo_ase,
                estado = NormalizarTexto(dto.estado),
                sirve_garantia = dto.sirve_garantia ?? false,
                calcula_rend = dto.calcula_rend ?? false,
                dto.codgrupo,
                cuenta_maestra = dto.cuenta_maestra ?? false,
                visible_ec = dto.visible_ec ?? false,
                apl_liq_socio = dto.apl_liq_socio ?? false,
                website = dto.website ?? false,
                plazo_tipo = NormalizarTexto(dto.plazo_tipo),
                inversion_minimo = dto.inversion_minimo ?? 0,
                cod_moneda = NormalizarTexto(dto.cod_moneda),
                garantia_porc_disp = dto.garantia_porc_disp ?? 0,
                garantia_integrada = dto.garantia_integrada ?? false,
                garantia_tasa_ad = dto.garantia_tasa_ad ?? 0,
                capitaliza_rendimientos = dto.capitaliza_rendimientos ?? false,
                base_calculo = dto.base_calculo ?? 0,
                tasa_base = dto.tasa_base ?? 0,
                utiliza_tbp = dto.utiliza_tbp ?? false,
                utiliza_tasa_fluctuante = dto.utiliza_tasa_fluctuante ?? false,
                deducir_planilla = dto.deducir_planilla ?? false,
                genera_mora = dto.genera_mora ?? false,
                controla_saldo = dto.controla_saldo ?? false,
                tipo_cdp = dto.tipo_cdp ?? false,
                permite_mov_cajas = dto.permite_mov_cajas ?? false,
                num_contratos_activos = dto.num_contratos_activos ?? 0,
                tasa_comision_aportes = dto.tasa_comision_aportes ?? 0,
                tasa_comision_rend = dto.tasa_comision_rend ?? 0,
                ctacomisionadm = NormalizarCuentaContable(dto.ctacomisionadm),
                ctaingretiros = NormalizarCuentaContable(dto.ctaingretiros),
                ctagstcomision = NormalizarCuentaContable(dto.ctagstcomision),
                comision_vta_monto = dto.comision_vta_monto ?? 0,
                comision_vta_inv = dto.comision_vta_inv ?? 0,
                ctarnd = NormalizarCuentaContable(dto.ctarnd),
                deduce_independiente = dto.deduce_independiente ?? false,
                tipodeduc = NormalizarTexto(dto.tipodeduc),
                porc_deduc = dto.porc_deduc ?? 0,
                requiere_beneficiarios = dto.requiere_beneficiarios ?? false,
                patrimonio_enlace = dto.patrimonio_enlace ?? false,
                dto.patrimonio_tipo,
                dto.web_vence,
                tasa_ajuste_vencimiento = dto.tasa_ajuste_vencimiento ?? false,
                tasa_ajuste = dto.tasa_ajuste ?? 0,
                web_liquida = dto.web_liquida ?? false,
                permite_giro_terceros = dto.permite_giro_terceros ?? false,
                permite_retiros_cajas = dto.permite_retiros_cajas ?? false,
                ctaimpuesto = NormalizarCuentaContable(dto.ctaimpuesto),
                impuesto_renta = dto.impuesto_renta ?? 0,
                sinpe_cuenta = dto.sinpe_cuenta ?? false,
                mov_entre_fondos = dto.mov_entre_fondos ?? false,
                forma_pago_pos = dto.forma_pago_pos ?? false,
                mov_entre_fondos_terceros = dto.mov_entre_fondos_terceros ?? false,
                renta_global = dto.renta_global ?? false,
                apl_rend_automatico = dto.apl_rend_automatico ?? false,
                permite_ret_parcial = dto.permite_ret_parcial ?? false,
                patrimonio_unifica = dto.patrimonio_unifica ?? false,
                dto.vence_accion,
                dto.vence_plan,
                vence_renueva = dto.vence_renueva ?? false,
                codtipoplan = dto.codtipoplan ?? 0,
                aplicar_tasa_cont_vencidos = dto.aplicar_tasa_cont_vencidos ?? false,
                aplicar_en_procs_contrs_vencidos = dto.aplicar_en_procs_contrs_vencidos ?? false,
                dto.mov_sinpe_tipos,
                mov_sinpe = dto.mov_sinpe ?? false,
                sinpe_proc_enlinea = dto.sinpe_proc_enlinea ?? false,
                sinpe_proc_interno = dto.sinpe_proc_interno ?? false,
                sif_liquida = dto.sif_liquida ?? false,
                pago_cupones = dto.pago_cupones ?? false,
                web_crear = dto.web_crear ?? false,
                web_modifica_couta = dto.web_modifica_couta ?? false,
                tasa_margen_negociacion = dto.tasa_margen_negociacion ?? 0,
                vence_notifica = dto.vence_notifica ?? false,
                ind_aplicar_amora = dto.ind_aplicar_amora ?? false,
                subcuentas_max = dto.subcuentas_max ?? 0
            };
        }

        private static string NormalizarCuentaContable(string? cuenta)
        {
            var texto = NormalizarTexto(cuenta);
            return texto.Length == 0
                ? string.Empty
                : new string(texto.Where(char.IsLetterOrDigit).ToArray());
        }

        #endregion

    }
}
