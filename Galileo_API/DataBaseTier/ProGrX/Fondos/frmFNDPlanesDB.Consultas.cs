using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndPlanesDb
    {
        private const string SpPlanesListas = "spFND_Planes_Listas";
        private const string SpPlanesPlazosConsulta = "spFnd_Planes_Plazos_Consulta";

        private const string SqlPlanesEstados = @"
                    SELECT
                        P.COD_ESTADO,
                        P.DESCRIPCION,
                        CASE WHEN F.cod_plan IS NULL THEN 0 ELSE 1 END AS Asignado
                    FROM dbo.AFI_ESTADOS_PERSONA P
                    LEFT JOIN dbo.FND_PLANES_ESTADOS E
                        ON P.COD_ESTADO = E.COD_ESTADO
                       AND E.cod_plan = @CodPlan
                       AND E.cod_operadora = @CodOperadora
                    LEFT JOIN dbo.FND_PLANES F
                        ON E.cod_plan = F.cod_plan
                       AND E.cod_operadora = F.Cod_Operadora
                    ORDER BY
                        Asignado DESC,
                        P.DESCRIPCION;";

        private const string SqlPlanDetalle = @"
                    SELECT *,
                        LTRIM(RTRIM(cod_moneda)) AS cod_moneda
                    FROM dbo.vFnd_Planes
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan;";

        private const string SqlPlanUltimaTasa = @"
                    SELECT TOP 1
                        CORTE,
                        TASA
                    FROM dbo.vFnd_Plan_Ultima_Tasa
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan
                    ORDER BY CORTE DESC;";

        private const string SqlPlanesScroll = @"
                    SELECT LTRIM(RTRIM(cod_plan))
                    FROM dbo.vFnd_Planes
                    ORDER BY cod_plan;";

        private const string SqlHistorialRend = @"
                    SELECT
                        corte,
                        tasa,
                        tcp,
                        usuario,
                        fecha_sys
                    FROM dbo.FND_HISTORIAL_REND
                    WHERE cod_plan = @CodPlan
                    ORDER BY IDx DESC;";

        #region Consultas

        /// <summary>
        /// Obtiene las listas necesarias para los combos del formulario de planes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listas de tipos de plan, grupos, divisas, operadoras, líneas y planes.</returns>
        public ErrorDto<FndPlanesCombosDto> FND_Planes_Combos_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                using var multi = connection.QueryMultiple(
                    SpPlanesListas,
                    commandType: CommandType.StoredProcedure);

                return new FndPlanesCombosDto
                {
                    TiposPlan = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Grupos = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Divisas = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Operadoras = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Lineas = multi.Read<DropDownListaGenericaModel>().ToList(),
                    Planes = multi.Read<DropDownListaGenericaModel>().ToList()
                };
            });

            return new ErrorDto<FndPlanesCombosDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndPlanesCombosDto()
            };
        }

        /// <summary>
        /// Obtiene los estados de persona asociados o disponibles para un plan.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codOperadora">Código de operadora.</param>
        /// <param name="codPlan">Código del plan.</param>
        /// <returns>Listado de estados con indicador de asignación.</returns>
        public ErrorDto<List<PlanEstadoDto>> Fnd_Planes_Estados_Obtener(int codEmpresa, int codOperadora, string codPlan)
        {
            return DbHelper.ExecuteListQuery<PlanEstadoDto>(
                CreatePortalDb(),
                codEmpresa,
                SqlPlanesEstados,
                new
                {
                    CodPlan = NormalizarTexto(codPlan),
                    CodOperadora = codOperadora
                });
        }

        /// <summary>
        /// Obtiene los plazos configurados para un plan.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codOperadora">Código de operadora.</param>
        /// <param name="codPlan">Código del plan.</param>
        /// <returns>Listado de plazos configurados para el plan.</returns>
        public ErrorDto<List<PlanPlazoDto>> Fnd_Planes_Plazos_Obtener(int codEmpresa, int codOperadora, string codPlan)
        {
            const string sql = $@"
                    EXEC {SpPlanesPlazosConsulta}
                        @Operadora,
                        @Plan,
                        'T';";

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<PlanPlazoDto>(
                    sql,
                    new
                    {
                        Operadora = codOperadora,
                        Plan = NormalizarTexto(codPlan)
                    }).ToList());

            return new ErrorDto<List<PlanPlazoDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<PlanPlazoDto>()
            };
        }

        /// <summary>
        /// Obtiene el detalle de un plan y su última tasa registrada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codOperadora">Código de operadora.</param>
        /// <param name="codPlan">Código del plan.</param>
        /// <returns>Datos del plan solicitado.</returns>
        public ErrorDto<FndPlanDto> Fnd_Planes_Obtener(int codEmpresa, int codOperadora, string codPlan)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                var parametros = new
                {
                    CodOperadora = codOperadora,
                    CodPlan = NormalizarTexto(codPlan)
                };

                var plan = connection.QueryFirstOrDefault<FndPlanDto>(SqlPlanDetalle, parametros);
                if (plan is null)
                {
                    return null;
                }

                var ultima = connection.QueryFirstOrDefault<PlanUltimaTasaDto>(SqlPlanUltimaTasa, parametros);
                AplicarUltimaTasa(plan, ultima);

                return plan;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener el plan.",
                    result.Code.GetValueOrDefault(-1),
                    CrearPlanVacio());
            }

            if (result.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró el plan solicitado.",
                    -1,
                    CrearPlanVacio());
            }

            return DbHelper.CreateOkResponse(result.Result);
        }


        /// <summary>
        /// Obtiene el plan anterior o siguiente según el código de desplazamiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="plan">Código del plan actual.</param>
        /// <param name="scrollCode">Código de desplazamiento: 1 para siguiente, otro valor para anterior.</param>
        /// <returns>Datos del plan encontrado.</returns>
        public ErrorDto<FndPlanDto> AF_Plan_Scroll_Obtener(int CodEmpresa, string plan, int scrollCode)
        {
            try
            {
                var planesResult = DbHelper.ExecuteListQuery<string>(
                    CreatePortalDb(),
                    CodEmpresa,
                    SqlPlanesScroll);

                if (planesResult.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        planesResult.Description ?? "Error al consultar planes.",
                        planesResult.Code.GetValueOrDefault(-1),
                        CrearPlanVacio());
                }

                var planes = (planesResult.Result ?? new List<string>())
                    .Select(NormalizarTexto)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (planes.Count == 0)
                {
                    return DbHelper.CreateErrorResponse("No hay planes configurados.", -1, CrearPlanVacio());
                }

                var nextPlan = ObtenerPlanScroll(planes, plan, scrollCode);
                if (string.IsNullOrWhiteSpace(nextPlan))
                {
                    return DbHelper.CreateErrorResponse("No hay más registros.", -1, CrearPlanVacio());
                }

                return Fnd_Planes_Obtener(CodEmpresa, 1, nextPlan);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, CrearPlanVacio());
            }
        }


        /// <summary>
        /// Obtiene el historial de rendimientos registrado para un plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodPlan">Código del plan.</param>
        /// <returns>Listado histórico de rendimientos del plan.</returns>
        public ErrorDto<List<FndHistorialRendDto>> Fnd_Historial_Rend_Obtener(int CodEmpresa, string CodPlan)
        {
            return DbHelper.ExecuteListQuery<FndHistorialRendDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlHistorialRend,
                new { CodPlan = NormalizarTexto(CodPlan) });
        }

        #endregion

        /// <summary>
        /// Crea una instancia vacía de plan cumpliendo los miembros requeridos del DTO.
        /// </summary>
        /// <returns>Plan vacío para respuestas de error o sin datos.</returns>
        private static FndPlanDto CrearPlanVacio()
        {
            return new FndPlanDto
            {
                codplan = string.Empty,
                descripcion = string.Empty,
                notas = string.Empty,
                estado = string.Empty,
                cod_moneda = string.Empty,
                plazo_tipo = string.Empty,
                codtipoplan = 0,
                porc_deduc = 0,
                plazo_minimo = 0,
                monto_minimo = 0,
                inversion_minimo = 0,
                deducir_planilla = false,
                genera_mora = false,
                cdp = false,
                controla_saldo = false,
                cuenta_maestra = false,
                subcuentas_max = 0,
                tasa_margen_negociacion = 0,
                requiere_beneficiarios = false,
                deduce_independiente = false,
                calcula_rend = false,
                base_calculo = 0,
                tasa_base = 0,
                tasa_fluctuante = false,
                codoperadora = 0,
                capitaliza_rend = false,
                utiliza_tbp = false,
                sirve_garantia = false,
                garantia_porc_disp = 0,
                garantia_tasa_ad = 0,
                garantia_integrada = false,
                mov_cajas = false,
                retiros_cajas = false,
                giro_terceros = false,
                website = false,
                web_liquida = false,
                renta_global = false,
                mov_sinpe = false,
                num_contratos_activos = 0,
                contratos_activos_vb6 = 0,
                tipo_cdp = false,
                pago_cupones = false,
                sinpe_cuenta = false,
                apl_rend_automatico = false,
                utiliza_tasa_fluctuante = false,
                capitaliza_rendimientos = false,
                tasa_ajuste_vencimiento = false,
                tasa_ajuste = 0,
                permite_mov_cajas = false,
                forma_pago_pos = false,
                permite_retiros_cajas = false,
                permite_giro_terceros = false,
                mov_entre_fondos = false,
                mov_entre_fondos_terceros = false,
                apl_liq_socio = false,
                liq_desde_ahorros = false,
                permite_retiros_terceros = false,
                visible_ec = false,
                web_crear = false,
                web_modifica_couta = false,
                permite_ret_parcial = false,
                patrimonio_enlace = false,
                patrimonio_unifica = false,
                tasa_comision_aportes = 0,
                impuesto_rendimientos = 0,
                tasa_comision_rend = 0,
                comision_vta_inv = 0,
                comision_vta_monto = 0,
                sif_liquida = false,
                impuesto_renta = 0,
                aplicar_tasa_cont_vencidos = false,
                aplicar_en_procs_contrs_vencidos = false,
                vence_renueva = false,
                vence_notifica = false
            };
        }

        /// <summary>
        /// Aplica al plan el resumen de la última tasa registrada.
        /// </summary>
        /// <param name="plan">Plan a completar.</param>
        /// <param name="ultima">Última tasa encontrada.</param>
        private static void AplicarUltimaTasa(FndPlanDto plan, PlanUltimaTasaDto? ultima)
        {
            var tasaTexto = "%";
            var fechaTexto = string.Empty;

            if (ultima is not null)
            {
                plan.ultima_tasa_vb6 = ultima.CORTE;

                if (ultima.TASA > 0)
                {
                    tasaTexto = $"{ultima.TASA:0.##}%";
                }

                fechaTexto = ultima.CORTE.ToString("dd/MM/yyyy");
            }

            plan.resumen_cont_tasa = $"Cont: {plan.consecutivo}  Ult.Tasa: {tasaTexto} {fechaTexto}";
        }

        /// <summary>
        /// Calcula el código del plan anterior o siguiente dentro de una lista ordenada.
        /// </summary>
        /// <param name="planes">Lista de planes ordenados.</param>
        /// <param name="plan">Plan actual o prefijo de búsqueda.</param>
        /// <param name="scrollCode">Código de desplazamiento.</param>
        /// <returns>Código del plan encontrado o cadena vacía.</returns>
        private static string ObtenerPlanScroll(List<string> planes, string plan, int scrollCode)
        {
            var planSeguro = NormalizarTexto(plan);
            var index = planes.IndexOf(planSeguro);

            if (index == -1)
            {
                index = planes.FindIndex(p => p.StartsWith(planSeguro, StringComparison.OrdinalIgnoreCase));
            }

            if (index == -1)
            {
                index = 0;
            }

            if (scrollCode == 1)
            {
                return index < planes.Count - 1 ? planes[index + 1] : string.Empty;
            }

            return index > 0 ? planes[index - 1] : string.Empty;
        }

        /// <summary>
        /// Representa la última tasa registrada de un plan.
        /// </summary>
        private sealed class PlanUltimaTasaDto
        {
            /// <summary>Fecha de corte.</summary>
            public DateTime CORTE = DateTime.MinValue;

            /// <summary>Tasa registrada.</summary>
            public decimal TASA = 0m;
        }
    }
}
