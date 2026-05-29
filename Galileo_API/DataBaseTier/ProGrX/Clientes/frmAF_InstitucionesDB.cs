using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmAFInstitucionesDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;
        private readonly MCntLinkDB _mCntLink;

        private const string SqlInstitucionUpdate = @"
                    UPDATE dbo.instituciones
                    SET descripcion = @Descripcion,
                        Desc_Corta = @DescCorta,
                        Activa = @Activa,
                        Mora_Cierres = @MoraAutomatica,
                        DEDUCCION_PLANILLA = @DeducionPlanilla,
                        planilla = @PlanillaRecibe,
                        planilla_envio = @PlanillaEnvio,
                        cod_divisa = @Divisa,
                        FRECUENCIA = @TipoPago,
                        direccion = @Direccion,
                        cta_credito = @CtaCredito,
                        cta_obrero = @CtaObrero,
                        cta_patronal = @CtaPatronal,
                        cta_fondos = @CtaFondos,
                        cta_inconsistencia = @CtaInconsistencias,
                        TipoAsiento = @TipoAsiento,
                        codigo_aportes = @CodigoDeducAportes,
                        codigo_creditos = @CodigoDeducCreditos,
                        codigo_aportes_env = @CodigoEnvAportes,
                        codigo_creditos_env = @CodigoEnvCreditos,
                        codigo_inst_deduc = @CodInstDeduc,
                        porc_ahorro = @PorcentajeAhorro,
                        porc_aporte = @PorcentajeAporte,
                        IncInclusiones = @MovInclusion,
                        IncExclusiones = @MovExclusion,
                        IncModificaciones = @MovModificacion,
                        IncMantienen = @MovMantienen,
                        pr_genera = @GeneraDeducciones,
                        pr_carga = @CargaDeducciones,
                        pr_desgloza = @Desgloza,
                        pr_apAplica = @AhAplica,
                        pr_apInco = @AhInconsistencias,
                        pr_apDev = @AhDevoluciones,
                        pr_crAplica = @CRAplica,
                        pr_crInco = @CRInconsistencias,
                        pr_crMora = @CRRecalculaMora,
                        pr_cr_aplica_incon = @Inconsistencias,
                        fnd_ap_aplica = @Devoluciones,
                        fnd_cr_SOAplica = @FNDSocios,
                        fnd_cr_ExAplica = @FNDExSocios,
                        fnd_ap_plan = @DevPlan,
                        fnd_ap_planp = @DevPlanPat,
                        fnd_cr_soPlan = @PlanSocios,
                        fnd_cr_exPlan = @PlanExSocios,
                        fnd_ap_Operadora = @DevOp,
                        fnd_cr_SoOperadora = @OPSocios,
                        fnd_cr_exOperadora = @OPExSocios,
                        Compara_Indicador = @ChkCompara,
                        compara_valor = @Compara,
                        Historico_Cobro_Envio = @HistoricoCuotasEnviadas,
                        Tipo_Cobro_Mora = @CuotasMora,
                        TRANSITO_PLANILLAS_MES = @TransitoPlanillasMes,
                        TRANSITO_COMPARA = @TransitoCompra
                    WHERE cod_institucion = @CodInstitucion;";

        private const string SqlInstitucionInsert = @"
                    INSERT INTO dbo.instituciones
                    (
                        descripcion,
                        desc_Corta,
                        activa,
                        cod_divisa,
                        mora_cierres,
                        DEDUCCION_PLANILLA,
                        direccion,
                        planilla,
                        planilla_envio,
                        cta_credito,
                        cta_obrero,
                        cta_patronal,
                        cta_fondos,
                        cta_inconsistencia,
                        TipoAsiento,
                        porc_ahorro,
                        porc_aporte,
                        pr_fecha_corte,
                        pr_genera,
                        pr_carga,
                        pr_desgloza,
                        pr_apAplica,
                        pr_apDev,
                        pr_apInco,
                        pr_crAplica,
                        pr_crInco,
                        pr_crMora,
                        pr_cr_aplica_incon,
                        fnd_ap_aplica,
                        fnd_ap_operadora,
                        fnd_ap_plan,
                        fnd_ap_planp,
                        fnd_cr_soAplica,
                        fnd_cr_soOperadora,
                        fnd_cr_soPlan,
                        fnd_cr_exAplica,
                        fnd_cr_exOperadora,
                        fnd_cr_exPlan,
                        codigo_aportes,
                        codigo_creditos,
                        codigo_aportes_env,
                        codigo_creditos_env,
                        IND_CAMBIA_FECPRO,
                        compara_indicador,
                        compara_valor,
                        codigo_inst_deduc,
                        Historico_Cobro_Envio,
                        Tipo_Cobro_Mora,
                        IncInclusiones,
                        IncExclusiones,
                        IncModificaciones,
                        IncMantienen,
                        TRANSITO_PLANILLAS_MES,
                        TRANSITO_COMPARA,
                        FRECUENCIA
                    )
                    VALUES
                    (
                        @Descripcion,
                        @DescCorta,
                        @Activa,
                        @Divisa,
                        @MoraAutomatica,
                        @DeducionPlanilla,
                        @Direccion,
                        @PlanillaRecibe,
                        @PlanillaEnvio,
                        @CtaCredito,
                        @CtaObrero,
                        @CtaPatronal,
                        @CtaFondos,
                        @CtaInconsistencias,
                        @TipoAsiento,
                        @PorcentajeAhorro,
                        @PorcentajeAporte,
                        @FechaCorte,
                        @GeneraDeducciones,
                        @CargaDeducciones,
                        @Desgloza,
                        @AhAplica,
                        @AhDevoluciones,
                        @AhInconsistencias,
                        @CRAplica,
                        @CRInconsistencias,
                        @CRRecalculaMora,
                        @Inconsistencias,
                        @Devoluciones,
                        @DevOp,
                        @DevPlan,
                        @DevPlanPat,
                        @FNDSocios,
                        @OPSocios,
                        @PlanSocios,
                        @FNDExSocios,
                        @OPExSocios,
                        @PlanExSocios,
                        @CodigoDeducAportes,
                        @CodigoDeducCreditos,
                        @CodigoEnvAportes,
                        @CodigoEnvCreditos,
                        @CambiaFechaGeneral,
                        @ChkCompara,
                        @Compara,
                        @CodInstDeduc,
                        @HistoricoCuotasEnviadas,
                        @CuotasMora,
                        @MovInclusion,
                        @MovExclusion,
                        @MovModificacion,
                        @MovMantienen,
                        @TransitoPlanillasMes,
                        @TransitoCompra,
                        @TipoPago
                    );
                    SELECT ISNULL(MAX(cod_institucion), 0) AS Ultimo
                    FROM dbo.instituciones;";

        private const string SqlDepartamentoDefaultInsert = @"
                    INSERT INTO dbo.Afdepartamentos
                    (
                        cod_institucion,
                        cod_departamento,
                        descripcion
                    )
                    VALUES
                    (
                        @Codigo,
                        '',
                        'SIN IDENTIFICAR'
                    );";

        private const string SqlSeccionDefaultInsert = @"
                    INSERT INTO dbo.AfSecciones
                    (
                        cod_institucion,
                        cod_departamento,
                        cod_seccion,
                        descripcion
                    )
                    VALUES
                    (
                        @Codigo,
                        '',
                        '',
                        'SIN IDENTIFICAR'
                    );";

        public FrmAFInstitucionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
            _mCntLink = new MCntLinkDB(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Guardar información de la institución, ya sea nuevo o edición
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <param name="Usuario"></param>
        /// <param name="vEdita"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Guardar(int CodEmpresa, AfInstitucionDto Info, string Usuario, bool vEdita)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos de la institución son requeridos.", -2);
            }

            var cuentas = CrearCuentasInstitucion(CodEmpresa, Info);
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                GuardarInstitucion(connection, CodEmpresa, Info, Usuario, vEdita, cuentas));

            return CrearRespuestaGuardarInstitucion(result);
        }

        /// <summary>
        /// Ejecuta el guardado de institución usando una conexión abierta.
        /// </summary>
        private ErrorDto GuardarInstitucion(
            SqlConnection connection,
            int codEmpresa,
            AfInstitucionDto info,
            string usuario,
            bool edita,
            InstitucionCuentasData cuentas)
        {
            return edita
                ? ActualizarInstitucion(connection, codEmpresa, info, usuario, cuentas)
                : InsertarInstitucion(connection, codEmpresa, info, usuario, cuentas);
        }

        /// <summary>
        /// Actualiza la información de una institución existente.
        /// </summary>
        private ErrorDto ActualizarInstitucion(
            SqlConnection connection,
            int codEmpresa,
            AfInstitucionDto info,
            string usuario,
            InstitucionCuentasData cuentas)
        {
            connection.Execute(SqlInstitucionUpdate, CrearParametrosInstitucion(info, cuentas, true));
            RegistrarBitacora(codEmpresa, usuario, $"Institución No.{info.cod_institucion}", "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Inserta una institución nueva y crea los registros por omisión.
        /// </summary>
        private ErrorDto InsertarInstitucion(
            SqlConnection connection,
            int codEmpresa,
            AfInstitucionDto info,
            string usuario,
            InstitucionCuentasData cuentas)
        {
            var ultimo = connection.QueryFirstOrDefault<int>(SqlInstitucionInsert, CrearParametrosInstitucion(info, cuentas, false));
            CrearDepartamentosPorOmision(connection, ultimo);
            RegistrarBitacora(codEmpresa, usuario, $"Institución No.{ultimo}", "Registra - WEB");
            return new ErrorDto { Code = ultimo, Description = "Ok" };
        }

        /// <summary>
        /// Crea los departamentos y secciones por omisión para una institución nueva.
        /// </summary>
        private static void CrearDepartamentosPorOmision(SqlConnection connection, int codInstitucion)
        {
            var parametros = new { Codigo = codInstitucion };
            connection.Execute(SqlDepartamentoDefaultInsert, parametros);
            connection.Execute(SqlSeccionDefaultInsert, parametros);
        }

        /// <summary>
        /// Crea la respuesta estándar del guardado de institución.
        /// </summary>
        private static ErrorDto CrearRespuestaGuardarInstitucion(ErrorDto<ErrorDto> result)
        {
            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar institución.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea las cuentas contables formateadas de la institución.
        /// </summary>
        private InstitucionCuentasData CrearCuentasInstitucion(int codEmpresa, AfInstitucionDto info)
        {
            return new InstitucionCuentasData
            {
                CtaCredito = _mCntLink.fxgCntCuentaFormato(codEmpresa, false, info.cta_crd_mask),
                CtaObrero = _mCntLink.fxgCntCuentaFormato(codEmpresa, false, info.cta_obr_mask),
                CtaPatronal = _mCntLink.fxgCntCuentaFormato(codEmpresa, false, info.cta_pat_mask),
                CtaFondos = _mCntLink.fxgCntCuentaFormato(codEmpresa, false, info.cta_fnd_mask),
                CtaInconsistencias = _mCntLink.fxgCntCuentaFormato(codEmpresa, false, info.cta_inc_mask)
            };
        }

        /// <summary>
        /// Crea parámetros seguros para insertar o actualizar instituciones.
        /// </summary>
        private static object CrearParametrosInstitucion(AfInstitucionDto info, InstitucionCuentasData cuentas, bool incluyeCodigo)
        {
            return new
            {
                CodInstitucion = ObtenerCodigoInstitucion(info, incluyeCodigo),
                Descripcion = NormalizarTexto(info.descripcion),
                DescCorta = NormalizarTexto(info.desc_corta),
                Activa = ToSqlBit(info.activa),
                MoraAutomatica = ToSqlBit(info.mora_cierres),
                DeducionPlanilla = info.deduccion_planilla,
                PlanillaRecibe = info.planilla,
                PlanillaEnvio = info.planilla_envio,
                Divisa = NormalizarTexto(info.cod_divisa),
                TipoPago = info.frecuencia_id,
                Direccion = NormalizarTexto(info.direccion),
                cuentas.CtaCredito,
                cuentas.CtaObrero,
                cuentas.CtaPatronal,
                cuentas.CtaFondos,
                cuentas.CtaInconsistencias,
                TipoAsiento = NormalizarTexto(info.tipoasiento),
                CodigoDeducAportes = NormalizarTexto(info.codigo_aportes),
                CodigoDeducCreditos = NormalizarTexto(info.codigo_creditos),
                CodigoEnvAportes = NormalizarTexto(info.codigo_aportes_env),
                CodigoEnvCreditos = NormalizarTexto(info.codigo_creditos_env),
                CodInstDeduc = info.codigo_inst_deduc,
                PorcentajeAhorro = info.porc_ahorro,
                PorcentajeAporte = info.porc_aporte,
                MovInclusion = ToSqlBit(info.incinclusiones),
                MovExclusion = ToSqlBit(info.incexclusiones),
                MovModificacion = ToSqlBit(info.incmodificaciones),
                MovMantienen = ToSqlBit(info.incmantienen),
                GeneraDeducciones = ToSqlBit(info.pr_genera),
                CargaDeducciones = ToSqlBit(info.pr_carga),
                Desgloza = ToSqlBit(info.pr_desgloza),
                AhAplica = ToSqlBit(info.pr_apaplica),
                AhInconsistencias = ToSqlBit(info.pr_apinco),
                AhDevoluciones = ToSqlBit(info.pr_apdev),
                CRAplica = ToSqlBit(info.pr_craplica),
                CRInconsistencias = ToSqlBit(info.pr_crinco),
                CRRecalculaMora = ToSqlBit(info.pr_crmora),
                Inconsistencias = ToSqlBit(info.pr_cr_aplica_incon),
                Devoluciones = ToSqlBit(info.fnd_ap_aplica),
                FNDSocios = ToSqlBit(info.fnd_cr_soaplica),
                FNDExSocios = ToSqlBit(info.fnd_cr_exaplica),
                DevPlan = info.fnd_ap_plan,
                DevPlanPat = info.fnd_ap_planp,
                PlanSocios = info.fnd_cr_soplan,
                PlanExSocios = info.fnd_cr_explan,
                DevOp = info.fnd_ap_operadora,
                OPSocios = info.fnd_cr_sooperadora,
                OPExSocios = info.fnd_cr_exoperadora,
                ChkCompara = ToSqlBit(info.compara_indicador),
                Compara = info.compara_valor,
                HistoricoCuotasEnviadas = info.historico_cobro_envio,
                CuotasMora = info.tipo_cobro_mora,
                TransitoPlanillasMes = info.transito_planillas_mes,
                TransitoCompra = info.transito_compara,
                FechaCorte = info.pr_fecha_corte,
                CambiaFechaGeneral = info.ind_cambia_fecpro
            };
        }

        /// <summary>
        /// Obtiene el código de institución cuando corresponde a una actualización.
        /// </summary>
        private static int ObtenerCodigoInstitucion(AfInstitucionDto info, bool incluyeCodigo)
        {
            if (!incluyeCodigo)
            {
                return 0;
            }

            return info.cod_institucion;
        }

        /// <summary>
        /// Convierte un valor booleano a bit numérico para SQL Server.
        /// </summary>
        private static int ToSqlBit(bool valor)
        {
            return valor ? 1 : 0;
        }

        /// <summary>
        /// Cuentas contables formateadas para guardar institución.
        /// </summary>
        private sealed class InstitucionCuentasData
        {
            public string CtaCredito { get; init; } = string.Empty;
            public string CtaObrero { get; init; } = string.Empty;
            public string CtaPatronal { get; init; } = string.Empty;
            public string CtaFondos { get; init; } = string.Empty;
            public string CtaInconsistencias { get; init; } = string.Empty;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Registra una acción en bitácora.
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}