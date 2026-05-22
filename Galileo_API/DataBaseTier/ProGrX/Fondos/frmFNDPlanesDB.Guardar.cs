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
                        codigo_deduc = @tipodeduc,
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
                        sinpe_producto = @sinpe_producto,
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
                        sif_liquida = @sif_liquida,
                        pago_cupones = @pago_cupones,
                        IndAplicarAMora = @genera_mora,
                        web_crear = @web_crear,
                        web_modifica_couta = @web_modifica_couta,
                        tasa_margen_negociacion = @tasa_margen_negociacion,
                        vence_notifica = @vence_notifica,
                        SubCuentasMax = @subcuentas_max
                    WHERE cod_operadora = @codoperadora
                      AND cod_plan = @codplan;";

        private const string SqlInsertPlan = @"
                    INSERT INTO dbo.FND_Planes
                    (
                        cod_operadora, cod_plan, descripcion, notas,
                        plazo_minimo, monto_minimo, cuenta_conta, cuenta_gasto,
                        codigo_ase, estado, sirve_garantia, calcula_rend, ulttasa,
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
                        tasa_margen_negociacion, vence_notifica, SubCuentasMax
                    )
                    VALUES
                    (
                        @codoperadora, @codplan, @descripcion, @notas,
                        @plazo_minimo, @monto_minimo, @cuenta_conta, @cuenta_gasto,
                        @codigo_ase, @estado, @sirve_garantia, @calcula_rend, @ulttasa,
                        @codgrupo, @cuenta_maestra, @visible_ec, @apl_liq_socio,
                        @website, @plazo_tipo, @inversion_minimo, @cod_moneda,
                        @garantia_porc_disp, @garantia_integrada, @garantia_tasa_ad,
                        @capitaliza_rendimientos, @base_calculo, @tasa_base,
                        @utiliza_tbp, @utiliza_tasa_fluctuante, @deducir_planilla,
                        @genera_mora, @controla_saldo, @tipo_cdp, @permite_mov_cajas,
                        @num_contratos_activos, @tasa_comision_aportes,
                        @tasa_comision_rend, @ctacomisionadm, @ctaingretiros,
                        @ctagstcomision, @comision_vta_monto, @comision_vta_inv,
                        @ctarnd, @deduce_independiente, @tipodeduc,
                        @requiere_beneficiarios, @patrimonio_enlace, @patrimonio_tipo,
                        @web_vence, @tasa_ajuste_vencimiento, @tasa_ajuste, @web_liquida,
                        @permite_giro_terceros, @permite_retiros_cajas, @ctaimpuesto,
                        @impuesto_renta, @sinpe_cuenta, @sinpe_producto,
                        @mov_entre_fondos, @forma_pago_pos, @mov_entre_fondos_terceros,
                        @renta_global, @apl_rend_automatico, @permite_ret_parcial,
                        @patrimonio_unifica, @vence_accion, @vence_plan,
                        @vence_renueva, @codtipoplan,
                        @aplicar_tasa_cont_vencidos, @aplicar_en_procs_contrs_vencidos,
                        @mov_sinpe_tipos, @mov_sinpe, @sif_liquida, @pago_cupones,
                        @genera_mora, @web_crear, @web_modifica_couta,
                        @tasa_margen_negociacion, @vence_notifica, @subcuentas_max
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
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al eliminar plan.", result.Code.GetValueOrDefault(-1), CrearPlanVacio());
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
                dto.codoperadora,
                codplan = NormalizarTexto(dto.codplan),
                descripcion = NormalizarTexto(dto.descripcion),
                notas = NormalizarTexto(dto.notas),
                dto.plazo_minimo,
                dto.monto_minimo,
                cuenta_conta = NormalizarCuentaContable(dto.ctaplan),
                cuenta_gasto = NormalizarCuentaContable(dto.ctagasto),
                dto.codigo_ase,
                estado = NormalizarTexto(dto.estado),
                dto.sirve_garantia,
                dto.calcula_rend,
                dto.ulttasa,
                dto.codgrupo,
                cuenta_maestra = dto.cuenta_maestra,
                dto.visible_ec,
                dto.apl_liq_socio,
                dto.website,
                plazo_tipo = NormalizarTexto(dto.plazo_tipo),
                inversion_minimo = dto.inversion_minimo,
                cod_moneda = NormalizarTexto(dto.cod_moneda),
                dto.garantia_porc_disp,
                dto.garantia_integrada,
                garantia_tasa_ad = dto.garantia_tasa_ad,
                dto.capitaliza_rendimientos,
                dto.base_calculo,
                dto.tasa_base,
                dto.utiliza_tbp,
                dto.utiliza_tasa_fluctuante,
                dto.deducir_planilla,
                dto.genera_mora,
                dto.controla_saldo,
                dto.tipo_cdp,
                dto.permite_mov_cajas,
                dto.num_contratos_activos,
                dto.tasa_comision_aportes,
                dto.tasa_comision_rend,
                ctacomisionadm = NormalizarCuentaContable(dto.ctacomisionadm),
                ctaingretiros = NormalizarCuentaContable(dto.ctaingretiros),
                ctagstcomision = NormalizarCuentaContable(dto.ctagstcomision),
                dto.comision_vta_monto,
                dto.comision_vta_inv,
                ctarnd = NormalizarCuentaContable(dto.ctarnd),
                dto.deduce_independiente,
                tipodeduc = dto.tipodeduc,
                dto.requiere_beneficiarios,
                dto.patrimonio_enlace,
                dto.patrimonio_tipo,
                dto.web_vence,
                dto.tasa_ajuste_vencimiento,
                dto.tasa_ajuste,
                dto.web_liquida,
                dto.permite_giro_terceros,
                dto.permite_retiros_cajas,
                ctaimpuesto = NormalizarCuentaContable(dto.ctaimpuesto),
                dto.impuesto_renta,
                sinpe_cuenta = dto.sinpe_cuenta,
                sinpe_producto = NormalizarTexto(dto.sinpe_producto),
                dto.mov_entre_fondos,
                dto.forma_pago_pos,
                dto.mov_entre_fondos_terceros,
                dto.renta_global,
                dto.apl_rend_automatico,
                dto.permite_ret_parcial,
                dto.patrimonio_unifica,
                dto.vence_accion,
                dto.vence_plan,
                dto.vence_renueva,
                dto.codtipoplan,
                dto.aplicar_tasa_cont_vencidos,
                dto.aplicar_en_procs_contrs_vencidos,
                dto.mov_sinpe_tipos,
                dto.mov_sinpe,
                dto.sif_liquida,
                dto.pago_cupones,
                dto.web_crear,
                dto.web_modifica_couta,
                dto.tasa_margen_negociacion,
                dto.vence_notifica,
                dto.subcuentas_max
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
