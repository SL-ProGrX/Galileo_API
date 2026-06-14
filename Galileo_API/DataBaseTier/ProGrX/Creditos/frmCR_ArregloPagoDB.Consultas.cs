using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrArregloPagoDb
    {
        /// <summary>
        /// Obtiene los tipos de documento de caja y los parametros base del formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="caja"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoCajaInicialData> Cr_ArregloPago_CajaInicial_Obtener(
            int codEmpresa,
            string caja,
            string usuario)
        {
            caja = NormalizarTexto(caja);
            usuario = NormalizarTexto(usuario);

            if (string.IsNullOrWhiteSpace(caja))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la caja.",
                    -2,
                    new CrArregloPagoCajaInicialData());
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    usuarioInvalido,
                    -2,
                    new CrArregloPagoCajaInicialData());
            }

            var globalesResp = ObtenerGlobales(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? paramGlobalesNulos,
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoCajaInicialData());
            }

            const string sql = @"
                select
                    rtrim(C.tipo_documento) as item,
                    rtrim(D.descripcion) as descripcion
                from SIF_DOCUMENTOS D
                inner join CAJAS_DOCUMENTOS C
                    on D.tipo_documento = C.tipo_documento
                where C.cod_caja = @Caja
                  and D.tipo_movimiento in ('A','C')
                order by C.tipo_documento;";

            var tiposDocResp = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Caja = caja });

            if (tiposDocResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    tiposDocResp.Description ?? "No fue posible obtener los tipos de documento.",
                    tiposDocResp.Code.GetValueOrDefault(-1),
                    new CrArregloPagoCajaInicialData());
            }

            return DbHelper.CreateOkResponse(new CrArregloPagoCajaInicialData
            {
                tipos_documento = tiposDocResp.Result ?? new List<DropDownListaGenericaModel>(),
                fecha_servidor = globalesResp.Result.fxFechaServidor ?? DateTime.Now,
                sys_plan_pagos = globalesResp.Result.SysPlanPagos == 1
            });
        }

        /// <summary>
        /// Obtiene la operacion activa y el estado mostrado por el formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrArregloPagoOperacionData?> Cr_ArregloPago_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario,
            bool tipoIntereses = false)
        {
            usuario = NormalizarTexto(usuario);

            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    "Debe indicar una operacion valida.",
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    usuarioInvalido,
                    -2,
                    null);
            }

            var globalesResp = ObtenerGlobales(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    globalesResp.Description ?? paramGlobalesNulos,
                    globalesResp.Code.GetValueOrDefault(-1),
                    null);
            }

            const string sql = @"
                select top 1
                    R.id_solicitud as operacion,
                    rtrim(R.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    rtrim(R.codigo) as codigo,
                    rtrim(C.descripcion) as linea_desc,
                    rtrim(isnull(R.proceso, '')) as proceso,
                    isnull(R.opex, 0) as opex,
                    cast(case when (C.retencion = 'S' or C.poliza = 'S') then 1 else 0 end as bit) as retencion,
                    isnull(R.montoapr, 0) as monto,
                    isnull(R.saldo, 0) as saldo,
                    isnull(R.plazo, 0) as plazo,
                    isnull(isnull(R.interesv, R.[int]), 0) as tasa,
                    isnull(R.cuota, 0) as cuota,
                    rtrim(isnull(R.cod_divisa, 'COL')) as divisa,
                    Getdate() as fecha_servidor,
                    dbo.fxSIFCorteAFecha(isnull(R.fecult, R.prideduc)) as fecha_ult_mov,
                    isnull(R.prideduc, 0) as prideduc,
                    isnull(V.amortiza, 0) as amortiza,
                    isnull(V.intC, 0) as int_cor,
                    isnull(V.intM, 0) as int_mor,
                    isnull(V.cargos, 0) as cargos,
                    cast(0 as decimal(16,2)) as polizas,
                    cast(0 as bit) as sys_plan_pagos,
                    cast(0 as int) as mora_count,
                    cast(0 as decimal(16,2)) as cargos_intereses,
                    cast(0 as decimal(16,2)) as deuda,
                    cast(0 as decimal(16,2)) as total_pagar
                from reg_creditos R
                inner join socios S
                    on R.cedula = S.cedula
                inner join catalogo C
                    on R.codigo = C.codigo
                   and C.retencion = 'N'
                   and C.poliza = 'N'
                left join vista_morosidad V
                    on R.id_solicitud = V.id_solicitud
                where R.id_solicitud = @Operacion
                  and R.estado = 'A';";

            var response = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                (CrArregloPagoOperacionData?)null,
                new { Operacion = operacion });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    response.Description ?? "No fue posible obtener la operacion.",
                    response.Code.GetValueOrDefault(-1),
                    null);
            }

            if (response.Result is null)
            {
                return DbHelper.CreateErrorResponse<CrArregloPagoOperacionData?>(
                    "No se encontr&oacute; registro de la operaci&oacute;n activa o no es un cr&eacute;dito.",
                    -2,
                    null);
            }

            var resultado = response.Result;
            var globales = globalesResp.Result;
            resultado.sys_plan_pagos = globales.SysPlanPagos == 1;
            resultado.fecha_servidor = globales.fxFechaServidor ?? resultado.fecha_servidor;

            if (resultado.sys_plan_pagos)
            {
                const string sqlCancelacion = @"
                    exec spCrdPlanPagosInfoCancelacion @OperacionId, @FechaCancelacion;";

                var cancelacionResp = DbHelper.ExecuteSingleQuery<CrArregloPagoOperacionData>(
                    _portalDb,
                    codEmpresa,
                    sqlCancelacion,
                    null,
                    new
                    {
                        OperacionId = operacion,
                        FechaCancelacion = resultado.fecha_servidor.Date
                    });

                if (cancelacionResp.Code == 0 && cancelacionResp.Result is not null)
                {
                    resultado.int_cor = cancelacionResp.Result.int_cor;
                    resultado.int_mor = cancelacionResp.Result.int_mor;
                    resultado.cargos = cancelacionResp.Result.cargos;
                    resultado.polizas = cancelacionResp.Result.polizas;
                    resultado.amortiza = cancelacionResp.Result.amortiza;
                }
            }
            else
            {
                const string sqlInteres = @"
                    select isnull(dbo.fxCRDCalculoIntCorte(@Operacion, Getdate()), 0);";

                var interesResp = DbHelper.ExecuteSingleQuery<decimal>(
                    _portalDb,
                    codEmpresa,
                    sqlInteres,
                    0,
                    new { Operacion = operacion });

                if (interesResp.Code == 0)
                {
                    resultado.int_cor = interesResp.Result - resultado.int_mor;
                }
            }

            resultado.tipo_intereses = tipoIntereses;

            resultado.mora = SbCargaMora(
                codEmpresa,
                operacion,
                resultado.sys_plan_pagos,
                resultado.fecha_servidor,
                tipoIntereses);

            resultado.mora_count = resultado.mora.Count;

            decimal totalIntCor = 0;
            decimal totalIntMor = 0;
            decimal totalCargos = 0;
            decimal totalPolizas = 0;
            decimal totalPrincipal = 0;

            foreach (var item in resultado.mora)
            {
                totalIntCor += item.int_c;
                totalIntMor += item.int_m;
                totalCargos += item.cargo;
                totalPolizas += item.poliza;
                totalPrincipal += item.amortiza;
            }

            resultado.int_cor = totalIntCor;
            resultado.int_mor = totalIntMor;
            resultado.cargos = totalCargos;
            resultado.polizas = totalPolizas;
            resultado.amortiza = totalPrincipal;

            resultado.cargos_intereses =
                resultado.int_cor +
                resultado.int_mor +
                resultado.cargos +
                resultado.polizas;

            resultado.deuda =
                resultado.saldo +
                resultado.int_cor +
                resultado.int_mor +
                resultado.cargos +
                resultado.polizas;

            resultado.total_pagar =
                resultado.int_cor +
                resultado.int_mor +
                resultado.cargos +
                resultado.polizas +
                resultado.amortiza;

            return DbHelper.CreateOkResponse<CrArregloPagoOperacionData?>(resultado);
        }

        private List<CrArregloPagoMoraData> SbCargaMora(
            int codEmpresa,
            int operacion,
            bool sysPlanPagos,
            DateTime fechaServidor,
            bool tipoIntereses)
        {
            if (sysPlanPagos)
            {
                if (tipoIntereses)
                {
                    DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        "exec spCrdPlanPagosProyectaCuota @Operacion, @Fecha, 1;",
                        new
                        {
                            Operacion = operacion,
                            Fecha = fechaServidor.Date
                        });

                    DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        @"
                        update CRD_OPERACION_TRANSAC_CAL
                        set principal = 0
                        where id_solicitud = @Operacion
                          and id_seq in
                          (
                              select max(id_seq)
                              from CRD_OPERACION_TRANSAC_CAL
                              where id_solicitud = @Operacion
                          );",
                        new { Operacion = operacion });

                    const string sqlCal = @"
                    select
                        Det.id_seq as id_moro,
                        Det.id_solicitud,
                        Det.fecha_proceso as fecha_p,
                        Det.intcor as int_c,
                        Det.intmor as int_m,
                        Det.cargos as cargo,
                        Det.poliza as poliza,
                        Det.principal as amortiza,
                        Det.intcor + Det.intmor + Det.principal + Det.poliza + Det.cargos as cuota_morosa,
                        rtrim(Det.estado) as estado,
                        cast(0 as decimal(16,2)) as ab_int_c,
                        cast(0 as decimal(16,2)) as ab_int_m,
                        cast(0 as decimal(16,2)) as ab_amortiza,
                        cast(0 as decimal(16,2)) as ab_cargo,
                        cast(0 as decimal(16,2)) as ab_poliza
                    from CRD_OPERACION_TRANSAC_CAL Det
                    inner join REG_CREDITOS Reg
                        on Det.id_solicitud = Reg.id_solicitud
                    where Reg.proceso <> 'J'
                      and Det.estado = 'A'
                      and Det.id_solicitud = @Operacion
                    order by Det.fecha_proceso, Det.id_seq;";

                    return DbHelper.ExecuteListQuery<CrArregloPagoMoraData>(
                        _portalDb,
                        codEmpresa,
                        sqlCal,
                        new { Operacion = operacion }).Result ?? new List<CrArregloPagoMoraData>();
                }

                const string sql = @"
                select
                    Det.id_seq as id_moro,
                    Det.id_solicitud,
                    Det.fecha_proceso as fecha_p,
                    Det.intcor as int_c,
                    Det.intmor as int_m,
                    Det.cargos as cargo,
                    Det.poliza as poliza,
                    Det.principal as amortiza,
                    Det.intcor + Det.intmor + Det.principal + Det.poliza + Det.cargos as cuota_morosa,
                    rtrim(Det.estado) as estado,
                    cast(0 as decimal(16,2)) as ab_int_c,
                    cast(0 as decimal(16,2)) as ab_int_m,
                    cast(0 as decimal(16,2)) as ab_amortiza,
                    cast(0 as decimal(16,2)) as ab_cargo,
                    cast(0 as decimal(16,2)) as ab_poliza
                from CRD_OPERACION_TRANSAC Det
                inner join REG_CREDITOS Reg
                    on Det.id_solicitud = Reg.id_solicitud
                where Reg.proceso <> 'J'
                  and Det.estado = 'A'
                  and Det.id_solicitud = @Operacion
                  and Det.fecha_corte <= @Fecha
                order by Det.fecha_proceso, Det.id_seq;";

                return DbHelper.ExecuteListQuery<CrArregloPagoMoraData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Operacion = operacion,
                        Fecha = fechaServidor.Date
                    }).Result ?? new List<CrArregloPagoMoraData>();
            }

            const string sqlMora = @"
            select
                id_moro,
                id_solicitud,
                fechap as fecha_p,
                intc as int_c,
                intm as int_m,
                isnull(cargo, 0) as cargo,
                cast(0 as decimal(16,2)) as poliza,
                amortiza,
                cuota_morosa,
                rtrim(estado) as estado,
                abintc as ab_int_c,
                abintm as ab_int_m,
                isnull(abamortiza, 0) as ab_amortiza,
                isnull(abcargo, 0) as ab_cargo,
                cast(0 as decimal(16,2)) as ab_poliza
            from MOROSIDAD
            where id_solicitud = @Operacion
              and estado = 'A'
            order by fechap;";

            return DbHelper.ExecuteListQuery<CrArregloPagoMoraData>(
                _portalDb,
                codEmpresa,
                sqlMora,
                new { Operacion = operacion }).Result ?? new List<CrArregloPagoMoraData>();
        }
    }
}