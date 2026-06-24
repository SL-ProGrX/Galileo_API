using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPolizasRegistroDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSeguimientoDB _seguimientoDb;
        private readonly MCobroDb _cobroDb;
        private readonly MProGrxMain _mainDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCrPolizasRegistroDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _seguimientoDb = new MSeguimientoDB(config);
            _cobroDb = new MCobroDb(config);
            _mainDb = new MProGrxMain(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las polizas configuradas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroPolizaLineaItem>> CrPolizasRegistro_PolizaLinea_Obtener(int codEmpresa)
        {
            const string sql = @"
            select
                rtrim(cod_poliza) as item,
                rtrim(descripcion) as descripcion,
                isnull(integra_plan_pagos, 0) as integra_plan_pagos
            from CRD_CATALOGO_POLIZAS
            order by descripcion;";

            return DbHelper.ExecuteListQuery<CrPolizasRegistroPolizaLineaItem>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene la operacion anterior o siguiente segun navegacion del formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="direccion"></param>
        /// <returns></returns>
        public ErrorDto<int> CrPolizasRegistro_Operacion_Navegar_Obtener(
            int codEmpresa,
            int operacion,
            int direccion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion.",
                    -2,
                    0);
            }

            if (direccion != 1 && direccion != -1)
            {
                return DbHelper.CreateErrorResponse(
                    "La direccion de navegacion no es valida.",
                    -2,
                    0);
            }

            string comparador = direccion == 1 ? ">" : "<";
            string orden = direccion == 1 ? "asc" : "desc";

            string sql = $@"
            select top 1
                R.id_solicitud
            from reg_creditos R
            inner join catalogo C
                on R.codigo = C.codigo
            where (C.retencion = 'N' or C.poliza = 'N')
              and R.id_solicitud {comparador} @Operacion
            order by R.id_solicitud {orden};";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Operacion = operacion });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? "No fue posible navegar la operacion.",
                    resp.Code.GetValueOrDefault(-1),
                    0);
            }

            if (resp.Result <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontro otra operacion para navegar.",
                    -2,
                    0);
            }

            return DbHelper.CreateOkResponse(resp.Result);
        }

        /// <summary>
        /// Carga la operacion madre.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroOperacionData> CrPolizasRegistro_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion.",
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
                    resp.Description ?? "No fue posible cargar la operacion.",
                    resp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroOperacionData());
            }

            if (resp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontro la operacion o no aplica para polizas.",
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
        /// Lista las polizas registradas para una operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroListadoItem>> CrPolizasRegistro_Lista_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion.",
                    -2,
                    new List<CrPolizasRegistroListadoItem>());
            }

            const string sql = @"
                select
                    Pol.id_solicitud,
                    Pol.num_poliza,
                    rtrim(Pol.cod_poliza) as cod_poliza,
                    rtrim(Cat.descripcion) as poliza_descripcion,
                    case when isnull(Cat.integra_plan_pagos,0) = 1 then 'Integrado' else 'Retencion' end as integra_plan_pagos,
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
                    resp.Description ?? "No fue posible obtener la lista de polizas.",
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
        /// Carga el detalle general de una poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="num_poliza"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroFormData> CrPolizasRegistro_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y el numero de poliza.",
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
                isnull(Pol.id_solicitud_poliza, 0) as id_solicitud_poliza,
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
                cast(Reg.prideduc as int) as prideduc
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
                    resp.Description ?? "No fue posible cargar el detalle de la poliza.",
                    resp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroFormData());
            }

            if (resp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontro la poliza seleccionada.",
                    -2,
                    new CrPolizasRegistroFormData());
            }

            var detalle = resp.Result;

            var destinosResp = CrPolizasRegistro_Destinos_Obtener(codEmpresa, detalle.codigo);
            if (destinosResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    destinosResp.Description ?? "No fue posible cargar los destinos de la poliza.",
                    destinosResp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroFormData());
            }

            var garantiasResp = CrPolizasRegistro_Garantias_Obtener(codEmpresa, detalle.codigo);
            if (garantiasResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    garantiasResp.Description ?? "No fue posible cargar las garantias de la poliza.",
                    garantiasResp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroFormData());
            }

            int priDeduc = detalle.prideduc ?? 0;
            if (priDeduc <= 0)
            {
                priDeduc = Convert.ToInt32(_seguimientoDb.fxPrimerDeduccion(codEmpresa));
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
                poliza_operacion = detalle.id_solicitud_poliza > 0
                    ? detalle.id_solicitud_poliza.ToString()
                    : string.Empty,

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
                anio = CrPolizasRegistro_PriDeduc_Anio_Obtener(priDeduc),
                mes = CrPolizasRegistro_PriDeduc_Mes_Obtener(priDeduc),

                destinos = destinosResp.Result ?? new(),
                garantias = garantiasResp.Result ?? new()
            });
        }

        /// <summary>
        /// Obtiene las operaciones de retencion asociadas a la operacion principal.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_OperacionPoliza_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = @"
            select distinct
                convert(varchar(20), isnull(Pol.id_solicitud_poliza, 0)) as item,
                convert(varchar(20), isnull(Pol.id_solicitud_poliza, 0)) as descripcion
            from CRD_OPERACION_POLIZAS Pol
            inner join CRD_CATALOGO_POLIZAS Cat
                on Pol.cod_poliza = Cat.cod_poliza
            where Pol.id_solicitud = @Operacion
              and isnull(Cat.integra_plan_pagos, 0) = 0
              and isnull(Pol.id_solicitud_poliza, 0) > 0
            order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }

        /// <summary>
        /// Obtiene los pagos de una poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="num_poliza"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroPagoItem>> CrPolizasRegistro_Pagos_Obtener(
            int codEmpresa, int operacion, int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y el numero de poliza.",
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
        /// Obtiene la recaudacion de una poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="num_poliza"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroRecaudacionItem>> CrPolizasRegistro_Recaudacion_Obtener(
            int codEmpresa, int operacion, int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y el numero de poliza.",
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
        /// Obtiene los acreedores disponibles y aplicados para la poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="num_poliza"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroAcreedorItem>> CrPolizasRegistro_Acreedores_Obtener(
            int codEmpresa, int operacion, int num_poliza)
        {
            if (operacion <= 0 || num_poliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y el numero de poliza.",
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
        /// Obtiene los destinos permitidos para la linea de credito asociada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
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
        /// Obtiene las garantias permitidas para la linea de credito asociada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
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
        /// Obtiene las cuotas del plan de pagos disponibles para polizas integradas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PlanPagos_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion.",
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

        /// <summary>
        /// Obtener los beneficiarios de una poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="numPoliza"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPolizasRegistroBeneficiarioItem>> CrPolizasRegistro_Beneficiarios_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
        {
            if (operacion <= 0 || numPoliza <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y el numero de poliza.",
                    -2,
                    new List<CrPolizasRegistroBeneficiarioItem>());
            }

            const string sql = @"
                select
                    rtrim(convert(varchar(20), isnull(id_beneficiario, 0))) as id_beneficiario,
                    rtrim(isnull(nombre, '')) as nombre,
                    FechaNac as fecha_nac,
                    rtrim(convert(varchar(20), isnull(parentesco, ''))) as parentesco,
                    isnull(porcentaje, 0) as porcentaje
                from CRD_OPERACION_POLIZAS_BENEFIARIOS
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza
                order by id_beneficiario;";

            var resp = DbHelper.ExecuteListQuery<CrPolizasRegistroBeneficiarioItem>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = operacion,
                    NumPoliza = numPoliza
                });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? "No fue posible cargar los beneficiarios.",
                    resp.Code.GetValueOrDefault(-1),
                    new List<CrPolizasRegistroBeneficiarioItem>());
            }

            var data = (resp.Result ?? new List<CrPolizasRegistroBeneficiarioItem>())
                .Select(x =>
                {
                    x.parentesco = MAfilicacionDB.fxParentesco(x.parentesco);
                    return x;
                })
                .ToList();

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Aplica acreedor de la poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CrPolizasRegistro_Acreedor_Aplicar(
            int codEmpresa,
            CrPolizasRegistroAcreedorAplicarRequest request)
        {
            if (request.operacion <= 0 || request.num_poliza <= 0 || string.IsNullOrWhiteSpace(request.cod_acreedor))
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del acreedor no son validos.",
                    -2,
                    false);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                const string sqlDelete = @"
                delete from CRD_OPERACION_POLIZAS_ACREEDORES
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza
                  and cod_acreedor = @CodAcreedor;";

                conn.Execute(sqlDelete, new
                {
                    Operacion = request.operacion,
                    NumPoliza = request.num_poliza,
                    CodAcreedor = request.cod_acreedor.Trim()
                }, tx);

                if (request.checked_item)
                {
                    const string sqlInsert = @"
                    insert into CRD_OPERACION_POLIZAS_ACREEDORES
                    (
                        num_poliza,
                        cod_acreedor,
                        codigo,
                        id_solicitud,
                        registro_fecha,
                        registro_usuario
                    )
                    values
                    (
                        @NumPoliza,
                        @CodAcreedor,
                        @Codigo,
                        @Operacion,
                        Getdate(),
                        @Usuario
                    );";

                    conn.Execute(sqlInsert, new
                    {
                        NumPoliza = request.num_poliza,
                        CodAcreedor = request.cod_acreedor.Trim(),
                        Codigo = (request.codigo ?? string.Empty).Trim(),
                        Operacion = request.operacion,
                        Usuario = (request.usuario ?? string.Empty).Trim()
                    }, tx);
                }

                tx.Commit();
                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(
                    $"No fue posible aplicar el acreedor. {ex.Message}",
                    -1,
                    false);
            }
        }

        /// <summary>
        /// Obtiene los datos de detalle del plan de pagos para polizas integradas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroPlanPagoDetalleData> CrPolizasRegistro_PlanPago_Detalle_Obtener(
            int codEmpresa,
            CrPolizasRegistroPlanPagoDetalleRequest request)
        {
            if (request.operacion <= 0 || request.id_seq <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y la linea inicial del plan.",
                    -2,
                    new CrPolizasRegistroPlanPagoDetalleData());
            }

            if (!request.poliza_fecha_pago.HasValue ||
                !request.poliza_cobertura_inicio.HasValue ||
                !request.poliza_cobertura_corte.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar las fechas de pago y cobertura.",
                    -2,
                    new CrPolizasRegistroPlanPagoDetalleData());
            }

            const string sqlMesesPendientes = @"
            select isnull(
                dbo.fxCrdPolizaMesesPendientes(
                    @Operacion,
                    @IdSeq,
                    @FechaInicio,
                    @FechaCorte
                ),
                0
            ) as meses;";

            var mesesPendientesResp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlMesesPendientes,
                0,
                new
                {
                    Operacion = request.operacion,
                    IdSeq = request.id_seq,
                    FechaInicio = request.poliza_cobertura_inicio.Value,
                    FechaCorte = request.poliza_cobertura_corte.Value
                });

            if (mesesPendientesResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    mesesPendientesResp.Description ?? "No fue posible calcular los meses pendientes del plan.",
                    mesesPendientesResp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroPlanPagoDetalleData());
            }

            int mesesPendientes = mesesPendientesResp.Result;
            if (mesesPendientes <= 0)
            {
                mesesPendientes = 1;
            }

            int divisorFrecuencia = CrPolizasRegistro_FrecuenciaPagosDivisor_Obtener(request.poliza_pago_frecuencia);
            int mesesVigenciaPago = CrPolizasRegistro_DiferenciaMeses_Obtener(
                request.poliza_fecha_pago.Value,
                request.poliza_cobertura_corte.Value) + 1;

            if (mesesVigenciaPago <= 0)
            {
                mesesVigenciaPago = 1;
            }

            int pagosNum = mesesVigenciaPago / divisorFrecuencia;
            if (pagosNum <= 0)
            {
                pagosNum = 1;
            }

            int coberturaMeses = CrPolizasRegistro_DiferenciaMeses_Obtener(
                request.poliza_cobertura_inicio.Value,
                request.poliza_cobertura_corte.Value) + 1;

            if (coberturaMeses <= 0)
            {
                coberturaMeses = 1;
            }

            decimal pagoMonto = request.poliza_monto / pagosNum;
            decimal cuota = request.poliza_monto / mesesPendientes;
            decimal cuotaRestoPlazo = request.poliza_monto / coberturaMeses;

            return DbHelper.CreateOkResponse(new CrPolizasRegistroPlanPagoDetalleData
            {
                poliza_cobertura_meses = coberturaMeses,
                poliza_pagos_num = pagosNum,
                poliza_pago_monto = pagoMonto,
                poliza_cuota = cuota,
                poliza_ctas_deduce = mesesPendientes,
                poliza_cuota_resto_plazo = cuotaRestoPlazo,
                id_seq = request.id_seq
            });
        }

        /// <summary>
        /// Guarda los datos de la poliza integrada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrPolizasRegistro_PolizaIntegrada_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaIntegradaGuardarRequest request)
        {
            if (request.operacion <= 0 || string.IsNullOrWhiteSpace(request.poliza_linea))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y la linea de poliza.",
                    -2,
                    0);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                int numPoliza = request.poliza_id > 0
                    ? request.poliza_id
                    : CrPolizasRegistro_NumeroPolizaSiguiente_Obtener(codEmpresa, request.operacion);

                int seqCorte = request.poliza_plan + request.poliza_ctas_deduce;
                string frecuenciaId = MapearFrecuenciaId(request.poliza_pago_frecuencia);
                string estadoId = CrPolizasRegistro_EstadoPoliza_Obtener(request.poliza_estado);

                if (request.poliza_id <= 0)
                {
                    const string sqlInsert = @"
                insert into CRD_OPERACION_POLIZAS
                (
                    id_solicitud_poliza,
                    cod_poliza,
                    id_solicitud,
                    codigo,
                    cuota,
                    registro_fecha,
                    registro_usuario,
                    estado,
                    num_poliza,
                    monto,
                    cobertura_inicio,
                    cobertura_vence,
                    pago_frecuencia,
                    pago_fecha,
                    pago_monto,
                    pago_realizado,
                    pago_saldo,
                    pago_ultimo,
                    recaudado_monto,
                    recaudado_corte,
                    recaudado_saldo,
                    num_seq_inicio,
                    num_ctas_deduce,
                    num_seq_corte,
                    num_contrato,
                    deduce_plazo_credito,
                    cuota_rst_plan
                )
                values
                (
                    0,
                    @PolizaLinea,
                    @Operacion,
                    @Codigo,
                    @Cuota,
                    Getdate(),
                    @Usuario,
                    @Estado,
                    @NumPoliza,
                    @Monto,
                    @CoberturaInicio,
                    @CoberturaCorte,
                    @Frecuencia,
                    @FechaPago,
                    @PagoMonto,
                    0,
                    @Monto,
                    null,
                    0,
                    Getdate(),
                    @Monto,
                    @Plan,
                    @CtasDeduce,
                    @SeqCorte,
                    @Contrato,
                    @PlazoCredito,
                    @CuotaRestoPlazo
                );";

                    conn.Execute(sqlInsert, new
                    {
                        PolizaLinea = request.poliza_linea.Trim(),
                        Operacion = request.operacion,
                        Codigo = request.codigo.Trim(),
                        Cuota = request.poliza_cuota,
                        Usuario = request.usuario.Trim(),
                        Estado = estadoId,
                        NumPoliza = numPoliza,
                        Monto = request.poliza_monto,
                        CoberturaInicio = request.poliza_cobertura_inicio,
                        CoberturaCorte = request.poliza_cobertura_corte,
                        Frecuencia = frecuenciaId,
                        FechaPago = request.poliza_fecha_pago,
                        PagoMonto = request.poliza_pago_monto,
                        Plan = request.poliza_plan,
                        CtasDeduce = request.poliza_ctas_deduce,
                        SeqCorte = seqCorte,
                        Contrato = request.poliza_contrato.Trim(),
                        PlazoCredito = request.poliza_plazo_credito ? 1 : 0,
                        CuotaRestoPlazo = request.poliza_cuota_resto_plazo
                    }, tx);
                }
                else
                {
                    const string sqlUpdate = @"
            update CRD_OPERACION_POLIZAS
               set estado = @Estado,
                   cuota = @Cuota,
                   monto = @Monto,
                   cobertura_inicio = @CoberturaInicio,
                   cobertura_vence = @CoberturaCorte,
                   deduce_plazo_credito = @PlazoCredito,
                   cuota_rst_plan = @CuotaRestoPlazo,
                   num_seq_inicio = @Plan,
                   num_ctas_deduce = @CtasDeduce,
                   num_seq_corte = @SeqCorte,
                   pago_frecuencia = @Frecuencia,
                   pago_fecha = @FechaPago,
                   pago_monto = @PagoMonto,
                   num_contrato = @Contrato
             where id_solicitud = @Operacion
               and num_poliza = @NumPoliza;";

                    conn.Execute(sqlUpdate, new
                    {
                        Estado = estadoId,
                        Cuota = request.poliza_cuota,
                        Monto = request.poliza_monto,
                        CoberturaInicio = request.poliza_cobertura_inicio,
                        CoberturaCorte = request.poliza_cobertura_corte,
                        PlazoCredito = request.poliza_plazo_credito ? 1 : 0,
                        CuotaRestoPlazo = request.poliza_cuota_resto_plazo,
                        Plan = request.poliza_plan,
                        CtasDeduce = request.poliza_ctas_deduce,
                        SeqCorte = seqCorte,
                        Frecuencia = frecuenciaId,
                        FechaPago = request.poliza_fecha_pago,
                        PagoMonto = request.poliza_pago_monto,
                        Contrato = request.poliza_contrato.Trim(),
                        Operacion = request.operacion,
                        NumPoliza = numPoliza
                    }, tx);
                }

                conn.Execute(
                    "exec spCrdPolizaRegistroDetalle @Operacion,@NumPoliza,@Usuario;",
                    new
                    {
                        Operacion = request.operacion,
                        NumPoliza = numPoliza,
                        Usuario = request.usuario.Trim()
                    },
                    tx);

                tx.Commit();

                return DbHelper.CreateOkResponse(numPoliza);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(
                    $"No fue posible guardar la poliza integrada. {ex.Message}",
                    -1,
                    0);
            }
        }

        /// <summary>
        /// Guarda los datos de la poliza de retencion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrPolizasRegistro_PolizaRetencion_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaRetencionGuardarRequest request)
        {
            if (request.operacion <= 0 || string.IsNullOrWhiteSpace(request.poliza_linea))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y la linea de poliza.",
                    -2,
                    0);
            }

            int priDeduc = CrPolizasRegistro_PriDeduc_Crear(request.anio, request.mes);
            if (priDeduc <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La primer deduccion no es valida.",
                    -2,
                    0);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                var operacionBaseResp = CrPolizasRegistro_OperacionBase_Obtener(codEmpresa, request.operacion);
                if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
                {
                    return DbHelper.CreateErrorResponse(
                        operacionBaseResp.Description ?? "No se encontro la operacion base.",
                        -2,
                        0);
                }

                var operacionBase = operacionBaseResp.Result;

                var lineaDataResp = CrPolizasRegistro_PolizaRetencionData_Obtener(codEmpresa, request.poliza_linea);
                if (lineaDataResp.Code != 0 || lineaDataResp.Result is null || string.IsNullOrWhiteSpace(lineaDataResp.Result.codigo_retencion))
                {
                    return DbHelper.CreateErrorResponse(
                        lineaDataResp.Description ?? "No se encontro la definicion de la linea de poliza.",
                        -2,
                        0);
                }

                var lineaData = lineaDataResp.Result;

                int comite = MCredito.fxCrdIdComiteLinea(_portalDb, codEmpresa, operacionBase.codigo);
                int numPoliza = CrPolizasRegistro_NumeroPolizaSiguiente_Obtener(codEmpresa, request.operacion);
                decimal fechaProcesoAnterior = _cobroDb.fxFechaProcesoAnterior(codEmpresa, priDeduc);
                DateTime fechaServidor = conn.QueryFirst<DateTime>("select Getdate();", transaction: tx);

                const string sqlInsertOperacion = @"
                insert into reg_creditos
                (
                    codigo,
                    id_comite,
                    cedula,
                    montosol,
                    montoapr,
                    monto_girado,
                    saldo,
                    amortiza,
                    interesc,
                    saldo_mes,
                    cuota,
                    int,
                    interesv,
                    plazo,
                    userrec,
                    userres,
                    userfor,
                    usertesoreria,
                    tesoreria,
                    fechasol,
                    fechares,
                    fechaforp,
                    fechaforf,
                    fecha_calculo_int,
                    garantia,
                    primer_cuota,
                    tdocumento,
                    ndocumento,
                    pagare,
                    firma_deudor,
                    premio,
                    observacion,
                    estado,
                    prideduc,
                    fecult,
                    estadosol,
                    documento_referido,
                    cod_destino
                )
                values
                (
                    @CodigoPolizaRet,
                    @Comite,
                    @Cedula,
                    @Monto,
                    @Monto,
                    0,
                    @Monto,
                    0,
                    0,
                    0,
                    @Monto,
                    @Monto,
                    0,
                    0,
                    @Plazo,
                    @Usuario,
                    @Usuario,
                    @Usuario,
                    @Usuario,
                    0,
                    @FechaServidor,
                    @FechaServidor,
                    @FechaServidor,
                    @FechaServidor,
                    @FechaServidor,
                    @Garantia,
                    'N',
                    'OT',
                    '',
                    0,
                    1,
                    0,
                    @Observacion,
                    'A',
                    @PriDeduc,
                    @Fecult,
                    'F',
                    @Documento,
                    @CodDestino
                );";

                conn.Execute(sqlInsertOperacion, new
                {
                    CodigoPolizaRet = lineaData.codigo_retencion.Trim().ToUpperInvariant(),
                    Comite = comite,
                    Cedula = operacionBase.cedula.Trim(),
                    Monto = request.monto,
                    Plazo = request.plazo,
                    Usuario = request.usuario.Trim(),
                    FechaServidor = fechaServidor,
                    Garantia = request.garantia.Trim(),
                    Observacion = request.observaciones.Trim().ToUpperInvariant(),
                    PriDeduc = priDeduc,
                    Fecult = fechaProcesoAnterior,
                    Documento = request.documento.Trim(),
                    CodDestino = string.IsNullOrWhiteSpace(request.destino) ? null : request.destino.Trim()
                }, tx);

                int nuevaOperacion = CrPolizasRegistro_UltimaOperacion_Obtener(codEmpresa, operacionBase.cedula);

                const string sqlInsertPoliza = @"
                insert into CRD_OPERACION_POLIZAS
                (
                    id_solicitud_poliza,
                    cod_poliza,
                    num_poliza,
                    id_solicitud,
                    codigo,
                    cuota,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @OperacionPoliza,
                    @CodigoPolizaRet,
                    @NumPoliza,
                    @OperacionMadre,
                    @CodigoMadre,
                    @Monto,
                    Getdate(),
                    @Usuario
                );";

                conn.Execute(sqlInsertPoliza, new
                {
                    OperacionPoliza = nuevaOperacion,
                    CodigoPolizaRet = lineaData.codigo_retencion.Trim().ToUpperInvariant(),
                    NumPoliza = numPoliza,
                    OperacionMadre = request.operacion,
                    CodigoMadre = operacionBase.codigo.Trim(),
                    Monto = request.monto,
                    Usuario = request.usuario.Trim()
                }, tx);

                int sysPlanPagos = _mainDb.sbSifParametrosInicializa(codEmpresa, request.usuario.Trim()).Result?.SysPlanPagos ?? 0;
                if (sysPlanPagos == 1)
                {
                    conn.Execute(
                        "exec spCrdPlanPagos @Operacion;",
                        new { Operacion = nuevaOperacion },
                        tx);
                }

                tx.Commit();

                MCredito.SbBitacoraCredito(
                    _portalDb,
                    codEmpresa,
                    new MCredito.CrBitacoraCreditoRequest
                    {
                        usuario = request.usuario.Trim(),
                        movimiento = "08",
                        detalle = $"Op: {nuevaOperacion} - Monto {request.monto} - Plazo: {request.plazo}",
                        tipo = "R",
                        operacion = nuevaOperacion,
                        codigo = operacionBase.codigo.Trim()
                    });

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = request.usuario.Trim(),
                    Movimiento = "Registra - WEB",
                    DetalleMovimiento = $"Retencion en la OP : {nuevaOperacion}",
                    Modulo = 10
                });

                return DbHelper.CreateOkResponse(nuevaOperacion);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(
                    $"No fue posible guardar la poliza de retencion. {ex.Message}",
                    -1,
                    0);
            }
        }

        private static int CrPolizasRegistro_PriDeduc_Anio_Obtener(int prideduc)
        {
            string valor = prideduc.ToString();
            if (valor.Length < 6)
            {
                return 0;
            }

            return int.TryParse(valor[..4], out int anio) ? anio : 0;
        }

        private static string CrPolizasRegistro_PriDeduc_Mes_Obtener(int prideduc)
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
        private sealed class CrPolizasRegistroOperacionBaseData
        {
            public string cedula { get; set; } = string.Empty;
            public string codigo { get; set; } = string.Empty;
        }

        private sealed class CrPolizasRegistroPolizaRetencionData
        {
            public string codigo_retencion { get; set; } = string.Empty;
            public string codigo_cargo { get; set; } = string.Empty;
        }

        private int CrPolizasRegistro_NumeroPolizaSiguiente_Obtener(int codEmpresa, int operacion)
        {
            const string sql = @"
            select isnull(max(num_poliza), 0) + 1
            from CRD_OPERACION_POLIZAS
            where id_solicitud = @Operacion;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                1,
                new { Operacion = operacion }).Result;
        }

        private int CrPolizasRegistro_UltimaOperacion_Obtener(int codEmpresa, string cedula)
        {
            const string sql = @"
            select isnull(max(id_solicitud), 0)
            from reg_creditos
            where cedula = @Cedula;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Cedula = (cedula ?? string.Empty).Trim() }).Result;
        }

        private ErrorDto<CrPolizasRegistroOperacionBaseData?> CrPolizasRegistro_OperacionBase_Obtener(
            int codEmpresa,
            int operacion)
        {
            const string sql = @"
            select top 1
                rtrim(isnull(cedula, '')) as cedula,
                rtrim(isnull(codigo, '')) as codigo
            from reg_creditos
            where id_solicitud = @Operacion;";

            return DbHelper.ExecuteSingleQuery<CrPolizasRegistroOperacionBaseData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion });
        }

        private ErrorDto<CrPolizasRegistroPolizaRetencionData?> CrPolizasRegistro_PolizaRetencionData_Obtener(
            int codEmpresa,
            string polizaLinea)
        {
            const string sql = @"
            select top 1
                rtrim(isnull(CODIGO_RETENCION, '')) as codigo_retencion,
                rtrim(isnull(CODIGO_CARGO, '')) as codigo_cargo
            from CRD_CATALOGO_POLIZAS
            where COD_POLIZA = @PolizaLinea;";

            return DbHelper.ExecuteSingleQuery<CrPolizasRegistroPolizaRetencionData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { PolizaLinea = (polizaLinea ?? string.Empty).Trim() });
        }

        private static string MapearFrecuenciaId(string frecuencia)
        {
            return (frecuencia ?? string.Empty).Trim() switch
            {
                "Mensual" => "M",
                "Trimestral" => "T",
                "Semestral" => "S",
                "Anual" => "A",
                "Indefinida" => "I",
                _ => "M"
            };
        }

        private static int CrPolizasRegistro_FrecuenciaPagosDivisor_Obtener(string frecuencia)
        {
            return (frecuencia ?? string.Empty).Trim() switch
            {
                "Mensual" => 1,
                "Trimestral" => 4,
                "Semestral" => 2,
                "Anual" => 1,
                "Indefinida" => 1,
                _ => 1
            };
        }

        private static int CrPolizasRegistro_DiferenciaMeses_Obtener(DateTime fechaInicio, DateTime fechaFin)
        {
            return ((fechaFin.Year - fechaInicio.Year) * 12) + fechaFin.Month - fechaInicio.Month;
        }

        private static int CrPolizasRegistro_PriDeduc_Crear(int anio, string mes)
        {
            int mesNumero = CrPolizasRegistro_MesNumero_Obtener(mes);
            if (anio <= 0 || mesNumero <= 0)
            {
                return 0;
            }

            return Convert.ToInt32($"{anio}{mesNumero:00}");
        }

        private static int CrPolizasRegistro_MesNumero_Obtener(string mes)
        {
            return (mes ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "ENERO" => 1,
                "FEBRERO" => 2,
                "MARZO" => 3,
                "ABRIL" => 4,
                "MAYO" => 5,
                "JUNIO" => 6,
                "JULIO" => 7,
                "AGOSTO" => 8,
                "SEPTIEMBRE" => 9,
                "OCTUBRE" => 10,
                "NOVIEMBRE" => 11,
                "DICIEMBRE" => 12,
                _ => 0
            };
        }

        private static string CrPolizasRegistro_EstadoPoliza_Obtener(string estado)
        {
            return (estado ?? string.Empty).Trim().StartsWith("A", StringComparison.OrdinalIgnoreCase)
                ? "A"
                : "I";
        }
    }
}