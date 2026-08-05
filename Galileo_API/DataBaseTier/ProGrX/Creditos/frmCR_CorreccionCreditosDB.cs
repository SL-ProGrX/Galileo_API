using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCorreccionCreditosDb
    {
        private const string EditorNumero = "numero";
        private const string EditorLista = "lista";

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;

        public FrmCrCorreccionCreditosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mRecibos = new MRecibos(config);
        }

        /// <summary>
        /// Obtiene la operacion activa y reproduce las opciones habilitadas por el VB6.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="operacion">Identificador de la operacion.</param>
        /// <param name="usuario">Usuario requerido para inicializar Globales.</param>
        /// <returns>Operacion y movimientos disponibles.</returns>
        public ErrorDto<CrCorreccionCreditosConsultaResponse> CR_CorreccionCreditos_Operacion_Obtener(
            int codEmpresa,
            int operacion,
            string usuario)
        {
            if (operacion <= 0)
                return CR_CorreccionCreditos_Consulta_Error("Debe indicar una operación válida.");

            var globalesResponse = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario ?? string.Empty);
            if (globalesResponse.Code != 0 || globalesResponse.Result is null)
                return CR_CorreccionCreditos_Consulta_Error(globalesResponse.Description ?? "No fue posible obtener Globales.");

            const string sql = @"
                select top 1
                    R.id_solicitud,
                    rtrim(isnull(R.cedula, '')) as cedula,
                    rtrim(isnull(S.nombre, '')) as nombre,
                    rtrim(isnull(R.codigo, '')) as codigo,
                    rtrim(isnull(C.descripcion, '')) as descripcion,
                    'ACTIVA' as estado_descripcion,
                    case when isnull(R.opex, 0) = 1 then 'OPEX' else '' end as opex_descripcion,
                    isnull(R.montoapr, 0) as montoapr,
                    isnull(R.saldo, 0) as saldo,
                    isnull(R.plazo, 0) as plazo,
                    case
                        when dbo.fxCrdPlazoRestante(R.plazo, R.prideduc, @FechaCredito) <= 0 then 1
                        else dbo.fxCrdPlazoRestante(R.plazo, R.prideduc, @FechaCredito)
                    end as plazo_restante,
                    isnull(R.interesv, R.[int]) as interes,
                    isnull(R.[int], 0) as tasa_original,
                    isnull(R.cuota, 0) as cuota,
                    isnull(convert(int, R.fecult), 0) as fecult,
                    isnull(convert(int, R.prideduc), 0) as prideduc,
                    rtrim(isnull(R.garantia, '')) as garantia,
                    rtrim(isnull(Gar.descripcion, '')) as garantia_descripcion,
                    rtrim(isnull(R.cod_destino, '')) as cod_destino,
                    rtrim(isnull(Des.descripcion, '')) as destino_descripcion,
                    rtrim(isnull(R.cod_grupo, '')) as cod_grupo,
                    rtrim(isnull(Rec.descripcion, '')) as recurso_descripcion,
                    rtrim(isnull(R.cod_oficina_r, '')) as cod_oficina_r,
                    rtrim(isnull(Ofi.descripcion, '')) as oficina_descripcion,
                    R.id_promotor,
                    rtrim(isnull(Eje.nombre, '')) as ejecutivo_descripcion,
                    rtrim(isnull(R.cod_actividad, '')) as cod_actividad,
                    isnull(R.dia_pago, 0) as dia_pago,
                    R.TBP_PuntosAdd as tbp_puntos_add,
                    convert(bit, case when isnull(R.LiqTasa, 0) <> 0 then 1 else 0 end) as liq_tasa,
                    convert(bit, case when C.retencion = 'S' or C.poliza = 'S' then 1 else 0 end) as retencion,
                    rtrim(isnull(R.base_calculo, '')) as base_calculo
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                inner join Socios S on R.cedula = S.cedula
                left join CRD_GARANTIA_TIPOS Gar on R.garantia = Gar.garantia
                left join Catalogo_destinos Des on R.cod_destino = Des.cod_destino
                left join Catalogo_grupos Rec on R.cod_grupo = Rec.cod_grupo
                left join SIF_Oficinas Ofi on R.cod_oficina_r = Ofi.cod_oficina
                left join Promotores Eje on R.id_promotor = Eje.id_promotor
                where R.estado = 'A'
                  and R.id_solicitud = @Operacion
                  and (@SysPlanPagos = 1 or R.proceso <> 'J');";

            var response = DbHelper.ExecuteSingleQuery<CrCorreccionCreditosOperacion>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    Operacion = operacion,
                    FechaCredito = globalesResponse.Result.GlngFechaCR,
                    SysPlanPagos = globalesResponse.Result.SysPlanPagos
                });

            if (response.Code != 0 || response.Result is null)
                return CR_CorreccionCreditos_Consulta_Error(response.Description ?? "La operación no se encontró o está cancelada.");

            response.Result.sys_plan_pagos = globalesResponse.Result.SysPlanPagos == 1;
            return DbHelper.CreateOkResponse(new CrCorreccionCreditosConsultaResponse
            {
                operacion = response.Result,
                movimientos = CR_CorreccionCreditos_Movimientos_Crear(response.Result)
            });
        }

        /// <summary>
        /// Obtiene el catalogo asociado al movimiento seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="movimiento">Identificador funcional del movimiento.</param>
        /// <param name="codigo">Linea de credito actual.</param>
        /// <returns>Lista de codigos y descripciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CorreccionCreditos_Catalogo_Obtener(
            int codEmpresa,
            int movimiento,
            string codigo)
        {
            if (movimiento == 14)
            {
                var dias = Enumerable.Range(1, 31)
                    .Select(dia => new DropDownListaGenericaModel { item = dia.ToString(), descripcion = dia.ToString() })
                    .ToList();
                return DbHelper.CreateOkResponse(dias);
            }

            var sql = movimiento switch
            {
                11 => @"select rtrim(G.garantia) as item, rtrim(G.descripcion) as descripcion
                        from CRD_GARANTIA_TIPOS G order by G.descripcion",
                12 => @"select rtrim(D.cod_destino) as item, rtrim(D.descripcion) as descripcion
                        from Catalogo_destinos D
                        inner join Catalogo_destinosAsg A on D.cod_destino = A.cod_destino
                        where A.codigo = @Codigo order by D.descripcion",
                13 => @"select rtrim(G.cod_grupo) as item, rtrim(G.descripcion) as descripcion
                        from Catalogo_grupos G
                        inner join Catalogo_AsignaGrp A on G.cod_grupo = A.cod_grupo
                        where A.codigo = @Codigo order by G.descripcion",
                16 => @"select rtrim(cod_oficina) as item, rtrim(descripcion) as descripcion
                        from SIF_Oficinas order by descripcion",
                18 => @"select rtrim(cod_actividad) as item, rtrim(descripcion) as descripcion
                        from AFI_ACTIVIDADES_ECO where activa = 1 order by descripcion",
                19 => @"select convert(varchar(20), id_promotor) as item, rtrim(nombre) as descripcion
                        from Promotores where estado = 1 order by nombre",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(sql))
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    "El movimiento no utiliza catálogo.",
                    result: []);

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = (codigo ?? string.Empty).Trim() });
        }

        /// <summary>
        /// Obtiene cuotas morosas o cargos registrados para seleccion individual.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="operacion">Identificador de la operacion.</param>
        /// <param name="movimiento">Movimiento 5 o 15.</param>
        /// <returns>Detalle homologado al recordset del formulario original.</returns>
        public ErrorDto<List<CrCorreccionCreditosDetalleSeleccion>> CR_CorreccionCreditos_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int movimiento)
        {
            if (operacion <= 0 || movimiento is not (5 or 15))
                return DbHelper.CreateErrorResponse<List<CrCorreccionCreditosDetalleSeleccion>>(
                    "Parámetros de consulta inválidos.",
                    result: []);

            var globales = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, string.Empty).Result;
            var usaPlanPagos = globales?.SysPlanPagos == 1;
            var sql = movimiento == 5
                ? CR_CorreccionCreditos_Mora_ConsultaCrear(usaPlanPagos)
                : CR_CorreccionCreditos_Cargos_ConsultaCrear(usaPlanPagos);

            return DbHelper.ExecuteListQuery<CrCorreccionCreditosDetalleSeleccion>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }

        /// <summary>
        /// Obtiene el proceso anterior o siguiente usado por los cambios de fecha del formulario.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="proceso">Proceso actual con formato AAAAMM.</param>
        /// <param name="direccion">Dirección -1 para anterior o 1 para siguiente.</param>
        /// <returns>Proceso encontrado con formato AAAAMM.</returns>
        public ErrorDto<int> CR_CorreccionCreditos_Proceso_Obtener(
            int codEmpresa,
            int proceso,
            int direccion)
        {
            if (direccion is not (-1 or 1))
                return DbHelper.CreateErrorResponse<int>("La dirección indicada no es válida.");

            var sql = direccion == -1
                ? "select convert(int, dbo.fxSIFPrmProcesoAnt(@Proceso))"
                : "select convert(int, dbo.fxSIFPrmProcesoSig(@Proceso))";
            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Proceso = proceso });
        }

        /// <summary>Crea los movimientos permitidos según las condiciones de la operación.</summary>
        /// <param name="operacion">Operación activa consultada.</param>
        /// <returns>Movimientos disponibles para la pantalla.</returns>
        private static List<CrCorreccionCreditosMovimiento> CR_CorreccionCreditos_Movimientos_Crear(CrCorreccionCreditosOperacion operacion)
        {
            var movimientos = new List<CrCorreccionCreditosMovimiento>();
            if (!operacion.retencion)
            {
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 0, "Cambio de Plazo", EditorNumero);
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 1, "Cambio de Tasa", "tasa");
            }

            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 2, "Cambio de Línea", "texto");
            if (!operacion.sys_plan_pagos && (!operacion.retencion || operacion.plazo < 900))
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 3, "Cambio de Monto", EditorNumero);
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 4, "Cambio de Cuota", EditorNumero);
            if (!operacion.retencion || !operacion.sys_plan_pagos)
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 5, "Elimina Cuotas en Mora", "seleccion");
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 6, "Cambio de último abono", EditorNumero);
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 7, "Cambio Primer Deducción", EditorNumero);

            if (!operacion.retencion)
            {
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 9, "Cambio de Fiadores", "modal");
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 10, "Elimina Intereses Moratorios", "accion");
            }

            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 11, "Cambio de Garantía", EditorLista);
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 12, "Cambio de Destino", EditorLista);
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 13, "Cambio de Recurso", EditorLista);
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 14, "Cambio de Día de Pago", EditorLista);
            if (!operacion.retencion)
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 15, "Elimina Cargos Registrados", "seleccion");
            CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 16, "Cambio de Oficina", EditorLista);

            if (!operacion.retencion && operacion.base_calculo == "04")
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 17, "Ajuste de Cuota Bullet/Ballon", "modal");
            if (!operacion.retencion)
            {
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 18, "Cambio de Actividad", EditorLista);
                CR_CorreccionCreditos_Movimiento_Agregar(movimientos, 19, "Cambio de Ejecutivo", EditorLista);
            }

            return movimientos;
        }

        /// <summary>Agrega una opción de movimiento a la lista de la pantalla.</summary>
        /// <param name="movimientos">Colección destino.</param>
        /// <param name="id">Identificador funcional VB6.</param>
        /// <param name="descripcion">Texto visible.</param>
        /// <param name="editor">Editor Angular requerido.</param>
        private static void CR_CorreccionCreditos_Movimiento_Agregar(
            ICollection<CrCorreccionCreditosMovimiento> movimientos,
            int id,
            string descripcion,
            string editor)
            => movimientos.Add(new CrCorreccionCreditosMovimiento
            {
                id = id,
                descripcion = descripcion,
                tipo_editor = editor
            });

        /// <summary>Crea la consulta parametrizada de cuotas en mora.</summary>
        /// <param name="usaPlanPagos">Indica si se usa el nuevo esquema de planes.</param>
        /// <returns>Consulta SQL correspondiente.</returns>
        private static string CR_CorreccionCreditos_Mora_ConsultaCrear(bool usaPlanPagos)
            => usaPlanPagos
                ? @"select ID_SEQ as id, convert(varchar(20), FECHA_PROCESO) as proceso,
                           FECHA_PAGO as fecha, '' as usuario, isnull(INTCOR,0) as int_cor,
                           isnull(INTMOR,0) as int_mor, isnull(PRINCIPAL,0) as principal,
                           isnull(CARGOS,0) as cargos, 0 as monto,
                           convert(varchar(20), isnull(MORA_DIAS,0)) as dias, '' as detalle,
                           ID_SEQ as id_mora, convert(bit,0) as seleccionado
                    from CRD_OPERACION_TRANSAC
                    where MORA_DIAS > 0 and ESTADO = 'A' and ID_SOLICITUD = @Operacion
                    order by FECHA_PROCESO desc"
                : @"select ID_MORO as id, convert(varchar(20), FECHAP) as proceso,
                           FECULT as fecha, '' as usuario, isnull(INTC,0) as int_cor,
                           isnull(INTM,0) as int_mor, isnull(AMORTIZA,0) as principal,
                           isnull(CARGO,0) as cargos, 0 as monto, 'N/A' as dias,
                           '' as detalle, ID_MORO as id_mora, convert(bit,0) as seleccionado
                    from morosidad
                    where estado = 'A' and id_solicitud = @Operacion
                    order by FECHAP desc";

        /// <summary>Crea la consulta parametrizada de cargos registrados.</summary>
        /// <param name="usaPlanPagos">Indica si se usa el nuevo esquema de planes.</param>
        /// <returns>Consulta SQL correspondiente.</returns>
        private static string CR_CorreccionCreditos_Cargos_ConsultaCrear(bool usaPlanPagos)
            => usaPlanPagos
                ? @"select C.LINEA as id, convert(varchar(20), M.FECHA_PROCESO) as proceso,
                           C.FECHA as fecha, rtrim(isnull(C.USUARIO,'')) as usuario,
                           0 as int_cor, 0 as int_mor, 0 as principal, 0 as cargos,
                           isnull(C.MONTO,0) as monto, '' as dias,
                           rtrim(isnull(C.DETALLE,'')) as detalle, M.ID_SEQ as id_mora,
                           convert(bit,0) as seleccionado
                    from CRD_OPERACION_TRANSAC M
                    inner join CRD_OPERACION_TRANSAC_CARGOS C
                      on M.ID_SOLICITUD = C.ID_SOLICITUD and M.ID_SEQ = C.ID_SEQ
                    where M.ESTADO = 'A' and C.MOV_MONTO = 0 and M.ID_SOLICITUD = @Operacion
                    order by C.LINEA desc"
                : @"select C.ID_CARGO as id, convert(varchar(20), M.FECHAP) as proceso,
                           C.FECHA as fecha, rtrim(isnull(C.USUARIO,'')) as usuario,
                           0 as int_cor, 0 as int_mor, 0 as principal, 0 as cargos,
                           isnull(C.MONTO,0) as monto, '' as dias,
                           rtrim(isnull(G.DESCRIPCION,'')) as detalle, C.ID_MORO as id_mora,
                           convert(bit,0) as seleccionado
                    from MOROSIDAD_CARGOS C
                    inner join CBR_GESTIONES G on C.COD_GESTION = G.COD_GESTION
                    inner join MOROSIDAD M on M.ID_MORO = C.ID_MORO
                    where M.ESTADO = 'A' and M.ID_SOLICITUD = @Operacion
                    order by C.ID_CARGO desc";

        /// <summary>Crea una respuesta de error para consultas del formulario.</summary>
        /// <param name="mensaje">Descripción del error.</param>
        /// <returns>Respuesta homologada.</returns>
        private static ErrorDto<CrCorreccionCreditosConsultaResponse> CR_CorreccionCreditos_Consulta_Error(string mensaje)
            => DbHelper.CreateErrorResponse<CrCorreccionCreditosConsultaResponse>(
                mensaje,
                -2,
                new CrCorreccionCreditosConsultaResponse());

        /// <summary>
        /// Genera y adjunta el comprobante PDF cuando el proceso produjo un documento.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecutó el proceso.</param>
        /// <param name="resultado">Resultado del proceso que contiene el documento.</param>
        private void CR_CorreccionCreditos_Reporte_Adjuntar(
            int codEmpresa,
            string usuario,
            CrCorreccionCreditosResultado resultado)
        {
            if (resultado.numero_documento <= 0 || string.IsNullOrWhiteSpace(resultado.tipo_documento))
                return;

            var impresion = _mRecibos.sbImprimeRecibo(
                codEmpresa,
                resultado.numero_documento.ToString(),
                resultado.tipo_documento,
                usuario,
                pFolder: "Creditos");
            if (impresion.Code == -1)
            {
                resultado.mensaje += $" No fue posible generar el recibo: {impresion.Description}";
                return;
            }

            resultado.reporte_resultado = impresion.Result?.ToString();
        }
    }
}
