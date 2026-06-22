using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPolizasRegistroDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrPolizasRegistroDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las pólizas configuradas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PolizaLinea_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(cod_poliza) as item,
                    rtrim(descripcion) as descripcion
                from CRD_CATALOGO_POLIZAS
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Carga la operación madre.
        /// </summary>
        public ErrorDto<CrPolizasRegistroOperacionData> CrPolizasRegistro_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación.",
                    -2,
                    new CrPolizasRegistroOperacionData());
            }

            const string sql = @"
                select top 1
                    R.id_solicitud,
                    R.cedula,
                    S.nombre,
                    R.codigo,
                    C.descripcion
                from reg_creditos R
                inner join socios S on R.cedula = S.cedula
                inner join catalogo C on R.codigo = C.codigo
                where R.id_solicitud = @Operacion
                  and (C.retencion = 'N' or C.poliza = 'N');";

            var resp = DbHelper.ExecuteSingleQuery<CrPolizasRegistroOperacionBase>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? "No fue posible cargar la operación.",
                    resp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroOperacionData());
            }

            if (resp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró la operación o no aplica para pólizas.",
                    -2,
                    new CrPolizasRegistroOperacionData());
            }

            return DbHelper.CreateOkResponse(new CrPolizasRegistroOperacionData
            {
                operacion = resp.Result.id_solicitud,
                cedula = resp.Result.cedula,
                nombre = resp.Result.nombre,
                codigo = resp.Result.codigo,
                descripcion = resp.Result.descripcion
            });
        }

        /// <summary>
        /// Lista las pólizas registradas para una operación.
        /// </summary>
        public ErrorDto<List<CrPolizasRegistroListadoItem>> CrPolizasRegistro_Lista_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación.",
                    -2,
                    new List<CrPolizasRegistroListadoItem>());
            }

            const string sql = @"
                select
                    Pol.id_solicitud,
                    Pol.num_poliza,
                    rtrim(Pol.cod_poliza) as cod_poliza,
                    rtrim(Cat.descripcion) as poliza_descripcion,
                    case when isnull(Cat.integra_plan_pagos,0) = 1 then 'Integrado' else 'Retención' end as integra_plan_pagos,
                    case when Pol.estado = 'A' then 'Activa' else 'Inactiva' end as estado,
                    Pol.cuota,
                    Pol.monto,
                    isnull(Pol.id_solicitud_poliza,0) as id_solicitud_poliza
                from CRD_OPERACION_POLIZAS Pol
                inner join CRD_CATALOGO_POLIZAS Cat on Pol.cod_poliza = Cat.cod_poliza
                where Pol.id_solicitud = @Operacion
                order by Pol.num_poliza;";

            var resp = DbHelper.ExecuteListQuery<CrPolizasRegistroListaBase>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? "No fue posible obtener la lista de pólizas.",
                    resp.Code.GetValueOrDefault(-1),
                    new List<CrPolizasRegistroListadoItem>());
            }

            List<CrPolizasRegistroListadoItem> result = (resp.Result ?? new())
                .Select(x => new CrPolizasRegistroListadoItem
                {
                    id_solicitud = x.id_solicitud,
                    num_poliza = x.num_poliza,
                    cod_poliza = x.cod_poliza,
                    poliza_descripcion = x.poliza_descripcion,
                    tipo_registro = x.integra_plan_pagos,
                    estado = x.estado,
                    cuota = x.cuota,
                    monto = x.monto
                })
                .ToList();

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Carga el detalle general de una póliza.
        /// </summary>
        /// <summary>
        /// Carga el detalle general de una póliza.
        /// </summary>
        public ErrorDto<CrPolizasRegistroFormData> CrPolizasRegistro_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación y el número de póliza.",
                    -2,
                    new CrPolizasRegistroFormData());
            }

            const string sql = @"
            select top 1
                rtrim(Pol.cod_poliza) as cod_poliza,
                rtrim(Cat.descripcion) as poliza_descripcion,
                isnull(Pol.estado, '') as estado,
                isnull(Pol.num_contrato, '') as num_contrato,
                isnull(Pol.monto, 0) as monto,
                isnull(Pol.cuota, 0) as cuota,
                isnull(Pol.pago_monto, 0) as pago_monto,
                isnull(Pol.cuota_rst_plan, 0) as cuota_rst_plan,
                isnull(Pol.deduce_plazo_credito, 0) as deduce_plazo_credito,
                Pol.pago_fecha,
                Pol.cobertura_inicio,
                Pol.cobertura_vence,
                isnull(Pol.pago_frecuencia, 'M') as pago_frecuencia,
                isnull(Pol.num_seq_inicio, 0) as num_seq_inicio,
                isnull(Pol.num_ctas_deduce, 0) as num_ctas_deduce,
                Pol.recaudado_corte,
                isnull(Pol.recaudado_saldo, 0) as recaudado_saldo,

                rtrim(isnull(Reg.codigo, '')) as codigo,
                isnull(Reg.cod_destino, 0) as cod_destino,
                rtrim(isnull(Cd.descripcion, '')) as destino,
                rtrim(isnull(Reg.garantia, '')) as garantia_codigo,
                rtrim(isnull(Gt.descripcion, '')) as garantia,
                isnull(Reg.documento_referido, '') as documento,
                isnull(Reg.plazo, 0) as plazo,
                isnull(Reg.cuota, 0) as monto_base,
                isnull(Reg.observacion, '') as observacion,
                Reg.fechaforp,
                isnull(Reg.amortiza, 0) as pagado,
                isnull(Reg.cuotas_planilla, 0) + isnull(Reg.cuotas_directas, 0) as plazo_transcurrido,
                cast(isnull(Reg.prideduc, dbo.fxPrimerDeduccion()) as bigint) as prideduc
            from reg_creditos Reg
            inner join catalogo C
                on Reg.codigo = C.codigo
            inner join CRD_OPERACION_POLIZAS Pol
                on Reg.id_solicitud = Pol.id_solicitud_poliza
            inner join CRD_CATALOGO_POLIZAS Cat
                on Pol.cod_poliza = Cat.cod_poliza
            left join CRD_GARANTIA_TIPOS Gt
                on Reg.garantia = Gt.garantia
            left join CATALOGO_DESTINOS Cd
                on Reg.cod_destino = Cd.cod_destino
            where Reg.estado in ('A', 'C')
              and C.poliza = 'S'
              and Pol.id_solicitud = @Operacion
              and Pol.num_poliza = @NumPoliza;";

            var resp = DbHelper.ExecuteSingleQuery<CrPolizasRegistroDetalleBase>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    Operacion = operacion,
                    NumPoliza = num_poliza
                });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? "No fue posible cargar el detalle de la póliza.",
                    resp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroFormData());
            }

            if (resp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró la póliza seleccionada.",
                    -2,
                    new CrPolizasRegistroFormData());
            }

            var detalle = resp.Result;

            var destinosResp = CrPolizasRegistro_Destinos_Obtener(codEmpresa, detalle.codigo);
            if (destinosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    destinosResp.Description ?? "No fue posible cargar los destinos de la póliza.",
                    destinosResp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroFormData());
            }

            var garantiasResp = CrPolizasRegistro_Garantias_Obtener(codEmpresa, detalle.codigo);
            if (garantiasResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    garantiasResp.Description ?? "No fue posible cargar las garantías de la póliza.",
                    garantiasResp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroFormData());
            }

            decimal proyectado = detalle.plazo >= 999
                ? detalle.monto_base
                : detalle.monto_base * detalle.plazo;

            decimal pendiente = detalle.plazo >= 999
                ? detalle.monto_base
                : proyectado - detalle.pagado;

            return DbHelper.CreateOkResponse(new CrPolizasRegistroFormData
            {
                poliza_linea = detalle.cod_poliza,
                poliza_id = num_poliza,
                poliza_contrato = detalle.num_contrato,
                poliza_estado = detalle.estado == "A" ? "Activa" : "Inactiva",
                poliza_monto = detalle.monto,
                poliza_cuota = detalle.cuota,
                poliza_pago_monto = detalle.pago_monto,
                poliza_cuota_resto_plazo = detalle.cuota_rst_plan,
                poliza_plazo_credito = detalle.deduce_plazo_credito == 1,
                poliza_fecha_pago = detalle.pago_fecha,
                poliza_cobertura_inicio = detalle.cobertura_inicio,
                poliza_cobertura_corte = detalle.cobertura_vence,
                poliza_pago_frecuencia = MapearFrecuencia(detalle.pago_frecuencia),
                poliza_ctas_deduce = detalle.num_ctas_deduce,
                poliza_pagos_num = 1,
                poliza_cobertura_meses = detalle.cobertura_inicio.HasValue && detalle.cobertura_vence.HasValue
                    ? ((detalle.cobertura_vence.Value.Year - detalle.cobertura_inicio.Value.Year) * 12)
                        + detalle.cobertura_vence.Value.Month - detalle.cobertura_inicio.Value.Month + 1
                    : 0,

                recaudado_saldo = detalle.recaudado_saldo,

                destino = detalle.cod_destino > 0 ? detalle.cod_destino.ToString() : string.Empty,
                garantia = detalle.garantia_codigo,
                documento = detalle.documento,
                plazo = detalle.plazo,
                monto = detalle.monto_base,
                observaciones = detalle.observacion,
                estado = detalle.estado == "A" ? "Activa" : "Inactiva",
                fecha = detalle.fechaforp.HasValue
                    ? detalle.fechaforp.Value.ToString("dd/MM/yyyy")
                    : string.Empty,
                plazo_transcurrido = detalle.plazo_transcurrido,
                proyectado = proyectado,
                pagado = detalle.pagado,
                pendiente = pendiente,
                anio = CrPolizasRegistro_PriDeduc_Anio_Obtener(detalle.prideduc),
                mes = CrPolizasRegistro_PriDeduc_Mes_Obtener(detalle.prideduc),

                destinos = destinosResp.Result ?? new(),
                garantias = garantiasResp.Result ?? new()
            });
        }

        /// <summary>
        /// Obtiene los pagos de una póliza.
        /// </summary>
        public ErrorDto<List<CrPolizasRegistroPagoItem>> CrPolizasRegistro_Pagos_Obtener(
            int codEmpresa, int operacion, int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación y el número de póliza.",
                    -2,
                    new List<CrPolizasRegistroPagoItem>());
            }

            const string sql = @"
                select
                    PAGO_FECHA as fecha,
                    PAGO_MONTO as monto,
                    PAGO_SALDO as saldo,
                    isnull(convert(varchar(200), PAGO_ULTIMO), '') as observacion
                from CRD_OPERACION_POLIZAS
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza;";

            return DbHelper.ExecuteListQuery<CrPolizasRegistroPagoItem>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = operacion,
                    NumPoliza = num_poliza
                });
        }

        /// <summary>
        /// Obtiene la recaudación de una póliza.
        /// </summary>
        public ErrorDto<List<CrPolizasRegistroRecaudacionItem>> CrPolizasRegistro_Recaudacion_Obtener(
            int codEmpresa, int operacion, int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación y el número de póliza.",
                    -2,
                    new List<CrPolizasRegistroRecaudacionItem>());
            }

            const string sql = @"
                select
                    RECAUDADO_CORTE as fecha,
                    RECAUDADO_MONTO as monto,
                    RECAUDADO_SALDO as saldo,
                    '' as usuario
                from CRD_OPERACION_POLIZAS
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza;";

            return DbHelper.ExecuteListQuery<CrPolizasRegistroRecaudacionItem>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = operacion,
                    NumPoliza = num_poliza
                });
        }

        /// <summary>
        /// Obtiene los acreedores disponibles y aplicados para la póliza.
        /// </summary>
        public ErrorDto<List<CrPolizasRegistroAcreedorItem>> CrPolizasRegistro_Acreedores_Obtener(
            int codEmpresa, int operacion, int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación y el número de póliza.",
                    -2,
                    new List<CrPolizasRegistroAcreedorItem>());
            }

            const string sql = @"
                select
                    Acr.cod_acreedor,
                    Acr.identificacion,
                    Acr.nombre,
                    Apl.registro_fecha,
                    isnull(Apl.registro_usuario,'') as registro_usuario,
                    case when Apl.registro_fecha is null then cast(0 as bit) else cast(1 as bit) end as checked_item
                from CRD_POLIZAS_ACREEDORES Acr
                inner join CRD_POLIZAS_ACREEDOR_ASG Asg
                    on Acr.cod_acreedor = Asg.cod_acreedor
                inner join CRD_OPERACION_POLIZAS Pol
                    on Pol.id_solicitud = @Operacion
                   and Pol.num_poliza = @NumPoliza
                left join CRD_OPERACION_POLIZAS_ACREEDORES Apl
                    on Acr.cod_acreedor = Apl.cod_acreedor
                   and Apl.id_solicitud = Pol.id_solicitud
                   and Apl.num_poliza = Pol.num_poliza
                where Asg.cod_poliza = Pol.cod_poliza
                  and Acr.activo = 1
                order by Apl.registro_fecha desc, Acr.cod_acreedor;";

            return DbHelper.ExecuteListQuery<CrPolizasRegistroAcreedorItem>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = operacion,
                    NumPoliza = num_poliza
                });
        }

        /// <summary>
        /// Obtiene los destinos permitidos para la línea de crédito asociada.
        /// </summary>
        private ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_Destinos_Obtener(
            int codEmpresa,
            string codigo)
        {
            const string sql = @"
        select
            convert(varchar(20), D.cod_destino) as item,
            rtrim(D.descripcion) as descripcion
        from CATALOGO_DESTINOS D
        where D.cod_destino in (
            select A.cod_destino
            from CATALOGO_DESTINOSASG A
            where A.codigo = @Codigo
        )
        order by D.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });
        }

        /// <summary>
        /// Obtiene las garantías permitidas para la línea de crédito asociada.
        /// </summary>
        private ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_Garantias_Obtener(
            int codEmpresa,
            string codigo)
        {
            const string sql = @"
        select
            rtrim(T.garantia) as item,
            rtrim(T.descripcion) as descripcion
        from CRD_CATALOGO_GARANTIAS C
        inner join CRD_GARANTIA_TIPOS T
            on C.garantia = T.garantia
        where C.codigo = @Codigo
        order by T.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo });
        }

        /// <summary>
        /// Obtiene las cuotas del plan de pagos disponibles para pólizas integradas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PlanPagos_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = @"
            select
                convert(varchar(20), id_seq) as item,
                'No.Cuota: ' + convert(varchar(10), num_cuota)
                + '   Fecha Pago: ' + convert(varchar(10), fecha_pago, 103)
                + ' (' + convert(varchar(10), fecha_inicio, 103)
                + ' : ' + convert(varchar(10), fecha_corte, 103) + ')' as descripcion
            from CRD_OPERACION_PLAN_PAGOS
            where id_solicitud = @Operacion
              and estado not in ('C')
              and num_cuota > 0
              and num_cuota_madre = 0
            order by id_seq;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }

        private static int CrPolizasRegistro_PriDeduc_Anio_Obtener(long prideduc)
        {
            string valor = prideduc.ToString();
            if (valor.Length < 6)
            {
                return 0;
            }

            return int.TryParse(valor[..4], out int anio) ? anio : 0;
        }

        private static string CrPolizasRegistro_PriDeduc_Mes_Obtener(long prideduc)
        {
            string valor = prideduc.ToString();
            if (valor.Length < 6 || !int.TryParse(valor.Substring(4, 2), out int mes))
            {
                return string.Empty;
            }

            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => string.Empty
            };
        }

        private static string MapearFrecuencia(string frecuencia)
        {
            return frecuencia switch
            {
                "M" => "Mensual",
                "T" => "Trimestral",
                "S" => "Semestral",
                "A" => "Anual",
                "I" => "Indefinida",
                _ => "Mensual"
            };
        }
    }
}