using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPolizasRegistroDb
    {
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
                    "Debe indicar la operaci&oacute;n.",
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
                    "No se encontr&oacute; la operaci&oacute;n o no aplica para p&oacute;lizas.",
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
                    "No se encontr&oacute; la p&oacute;liza seleccionada.",
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

            int polizaPagosNum = 0;

            if (detalle.pago_fecha.HasValue && detalle.cobertura_vence.HasValue)
            {
                int divisorFrecuencia = CrPolizasRegistro_FrecuenciaPagosDivisor_Obtener(
                    MapearFrecuencia(detalle.pago_frecuencia));

                int mesesVigenciaPago = CrPolizasRegistro_DiferenciaMeses_Obtener(
                    detalle.pago_fecha.Value,
                    detalle.cobertura_vence.Value) + 1;

                if (mesesVigenciaPago <= 0)
                {
                    mesesVigenciaPago = 1;
                }

                polizaPagosNum = mesesVigenciaPago / divisorFrecuencia;
                if (polizaPagosNum <= 0)
                {
                    polizaPagosNum = 1;
                }
            }

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
                poliza_pagos_num = polizaPagosNum,
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
        /// Obtiene los acreedores asociados a la poliza.
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
                    rtrim(Acr.cod_acreedor) as cod_acreedor,
                    rtrim(isnull(Acr.identificacion, '')) as identificacion,
                    rtrim(isnull(Acr.nombre, '')) as nombre,
                    Apl.registro_fecha,
                    rtrim(isnull(Apl.registro_usuario, '')) as registro_usuario,
                    cast(case when Apl.registro_fecha is null then 0 else 1 end as bit) as checked_item
                from CRD_POLIZAS_ACREEDORES Acr
                inner join CRD_POLIZAS_ACREEDOR_ASG Asg
                    on Acr.cod_acreedor = Asg.cod_acreedor
                left join CRD_OPERACION_POLIZAS_ACREEDORES Apl
                    on Acr.cod_acreedor = Apl.cod_acreedor
                   and Apl.id_solicitud = @Operacion
                   and Apl.num_poliza = @NumPoliza
                where Acr.activo = 1
                  and Asg.cod_poliza =
                    (
                        select top 1 cod_poliza
                        from CRD_OPERACION_POLIZAS
                        where id_solicitud = @Operacion
                          and num_poliza = @NumPoliza
                    )
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
        /// Obtiene los destinos permitidos para la linea.
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
                inner join CATALOGO_DESTINOS_POLIZAS P
                    on D.cod_destino = P.cod_destino
                where P.codigo = @Codigo
                order by D.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = (codigo ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Obtiene las garantias permitidas para la linea.
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
                    rtrim(G.garantia) as item,
                    rtrim(G.descripcion) as descripcion
                from CRD_GARANTIA_TIPOS G
                inner join CRD_GARANTIA_TIPOS_POLIZA P
                    on G.garantia = P.garantia
                where P.codigo = @Codigo
                order by G.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = (codigo ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Obtiene las cuotas disponibles del plan de pagos.
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
                    convert(varchar(20), id_seq) + ' - ' +
                    convert(varchar(10), fecha_proceso, 103) as descripcion
                from CRD_OPERACION_TRANSAC
                where id_solicitud = @Operacion
                  and estado = 'A'
                order by id_seq;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }

        /// <summary>
        /// Obtiene los beneficiarios asociados a la poliza.
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
                from CRD_OPERACION_POLIZAS_BENEFICIARIOS
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
                    resp.Description ?? "No fue posible obtener los beneficiarios.",
                    resp.Code.GetValueOrDefault(-1),
                    new List<CrPolizasRegistroBeneficiarioItem>());
            }

            foreach (var item in resp.Result ?? new List<CrPolizasRegistroBeneficiarioItem>())
            {
                item.parentesco = MAfilicacionDB.fxParentesco(item.parentesco);
            }

            return DbHelper.CreateOkResponse(resp.Result ?? new List<CrPolizasRegistroBeneficiarioItem>());
        }
    }
}