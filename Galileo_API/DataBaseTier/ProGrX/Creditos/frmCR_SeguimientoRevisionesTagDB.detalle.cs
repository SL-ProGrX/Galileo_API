using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using Galileo_API.Models.ProGrX.Credito.Galileo_API.Models.ProGrX.Credito;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoRevisionesTagDB
    {
        private const string validaCedula = "No se encontró la cédula asociada a la operación indicada.";


        /// <summary>
        /// Obtiene el encabezado y montos principales del detalle de la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Detalle principal del crédito.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagDetalleCreditoResponse> Cr_SeguimientoRevisionesTag_DetalleCredito_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDetalleCreditoResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
                        select
                            rtrim(R.cedula) as cedula,
                            rtrim(S.nombre) as nombre,
                            R.ID_SOLICITUD as id_solicitud,
                            rtrim(G.DESCRIPCION) as garantia,
                            isnull(R.MONTOSOL, 0) as montosol,
                            isnull(R.MONTOAPR, 0) as montoapr,
                            isnull(R.CUOTA, 0) as cuota,
                            isnull(R.MONTO_GIRADO, 0) as monto_girado,
                            isnull(dbo.fxCrdDesembolsosOperacion(R.ID_SOLICITUD), 0) as montodesembolsos,
                            isnull(dbo.fxCrdRefundicionesOperacion(R.ID_SOLICITUD), 0) as montorefundicion,
                            isnull(dbo.fxCrdRefundicionesCuotaOperacion(R.ID_SOLICITUD), 0) as refundicionescuota,
                            cast(0 as decimal(18, 2)) as desembolsoscuota
                        ) as desembolsoscuota
                        from REG_CREDITOS R
                        inner join SOCIOS S on S.cedula = R.cedula
                        inner join AFI_ESTADOS_PERSONA E on S.ESTADOACTUAL = E.COD_ESTADO
                        inner join CRD_GARANTIA_TIPOS G on R.GARANTIA = G.GARANTIA
                        where R.ID_SOLICITUD = @id_solicitud
                        """;

                var result = conn.QueryFirstOrDefault<CrSeguimientoRevisionesTagDetalleCreditoResponse>(
                    sql,
                    new { id_solicitud = request.id_solicitud });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDetalleCreditoResponse>(
                        "No se encontró información para la operación indicada.");
                }

                result.total_cuotas = result.refundicionescuota + result.desembolsoscuota;
                result.diferencia_cuota = result.cuota - result.total_cuotas;

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDetalleCreditoResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la información de patrimonio del asociado para la operación seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Detalle de patrimonio y cálculos derivados.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagPatrimonioResponse> Cr_SeguimientoRevisionesTag_Patrimonio_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagPatrimonioResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sqlCedula = """
                    select rtrim(CEDULA)
                    from REG_CREDITOS
                    where ID_SOLICITUD = @id_solicitud
                    """;

                var cedula = conn.QueryFirstOrDefault<string>(
                    sqlCedula,
                    new { id_solicitud = request.id_solicitud });

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagPatrimonioResponse>(
                        validaCedula);
                }

                const string sqlPorcentajeObrero = """
                    select isnull(PORCENTAJE, 0)
                    from CRD_GARANTIAS_PATRIMONIO
                    where TIPO = 'OBR'
                    """;

                const string sqlPorcentajeCapitalizacion = """
                    select isnull(PORCENTAJE, 0)
                    from CRD_GARANTIAS_PATRIMONIO
                    where TIPO = 'CAP'
                    """;

                var porcObrero = conn.QueryFirstOrDefault<decimal>(sqlPorcentajeObrero);
                var porcCapitalizacion = conn.QueryFirstOrDefault<decimal>(sqlPorcentajeCapitalizacion);

                const string sqlPatrimonio = """
                    select
                        isnull(A.APORTE, 0) as patronal,
                        isnull(A.AHORRO, 0) as aporte_obrero,
                        isnull(A.CAPITALIZA, 0) as capitalizacion,
                        A.FECAPORTE as fecha_corte,
                        (isnull(sum(F.APORTES), 0) + isnull(sum(F.RENDIMIENTO), 0)) as ahorros_extra
                    from AHORRO_CONSOLIDADO A
                    left join FND_CONTRATOS F
                        on A.CEDULA = F.CEDULA
                       and F.ESTADO = 'A'
                    where A.CEDULA = @cedula
                    group by
                        A.APORTE,
                        A.AHORRO,
                        A.CAPITALIZA,
                        A.FECAPORTE
                    """;

                var result = conn.QueryFirstOrDefault<CrSeguimientoRevisionesTagPatrimonioResponse>(
                    sqlPatrimonio,
                    new { cedula = cedula.Trim() }) ?? new CrSeguimientoRevisionesTagPatrimonioResponse();

                const string sqlSaldoPrestamos = """
                    select isnull(sum(SALDO), 0)
                    from REG_CREDITOS
                    where CEDULA = @cedula
                      and SALDO > 0
                      and PROCESO = 'N'
                      and ESTADO = 'A'
                      and GARANTIA = 'A'
                    """;

                result.saldo_prestamos = conn.QueryFirstOrDefault<decimal>(
                    sqlSaldoPrestamos,
                    new { cedula = cedula.Trim() });

                result.total =
                    result.patronal +
                    result.aporte_obrero +
                    result.ahorros_extra +
                    result.capitalizacion;

                result.ahorros_fecha =
                    result.aporte_obrero +
                    result.capitalizacion;

                result.disponible_bruto =
                    (result.aporte_obrero * (porcObrero / 100m)) +
                    (result.capitalizacion * (porcCapitalizacion / 100m));

                result.disponible =
                    result.disponible_bruto -
                    result.saldo_prestamos;

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagPatrimonioResponse>(ex.Message);
            }
        }

        private string Cr_SeguimientoRevisionesTag_ObtenerCedulaOperacion(
            DbConnection conn,
            long idSolicitud)
        {
            const string sql = """
        select rtrim(CEDULA)
        from REG_CREDITOS
        where ID_SOLICITUD = @id_solicitud
        """;

            return conn.QueryFirstOrDefault<string>(
                sql,
                new { id_solicitud = idSolicitud }) ?? string.Empty;
        }

        private string Cr_SeguimientoRevisionesTag_ObtenerPreanalisisOperacion(
            DbConnection conn,
            long idSolicitud)
        {
            const string sql = """
        select isnull(COD_PREANALISIS, 0)
        from CRD_PREA_PREANALISIS
        where TIPO_PREANALISIS = 'E'
          and ID_SOLICITUD = @id_solicitud
        """;

            return conn.QueryFirstOrDefault<string>(
                sql,
                new { id_solicitud = idSolicitud }) ?? string.Empty;
        }


        /// <summary>
        /// Obtiene el resumen y la lista de deudas activas de la persona asociada a la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Resumen y lista de deudas activas.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagDeudasResponse> Cr_SeguimientoRevisionesTag_Deudas_Obtener(
    int codEmpresa,
    CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDeudasResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var cedula = Cr_SeguimientoRevisionesTag_ObtenerCedulaOperacion(conn, request.id_solicitud);

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDeudasResponse>(
                        validaCedula);
                }

                var response = new CrSeguimientoRevisionesTagDeudasResponse();
                var cedulaNormalizada = cedula.Trim();

                Cr_SeguimientoRevisionesTag_Deudas_CargarResumen(conn, cedulaNormalizada, response);
                Cr_SeguimientoRevisionesTag_Deudas_CargarDeducciones(conn, request.id_solicitud, response);
                response.lista = Cr_SeguimientoRevisionesTag_Deudas_CargarLista(conn, cedulaNormalizada);

                return DbHelper.CreateOkResponse(response);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagDeudasResponse>(ex.Message);
            }
        }

        private static void Cr_SeguimientoRevisionesTag_Deudas_CargarResumen(
    DbConnection conn,
    string cedula,
    CrSeguimientoRevisionesTagDeudasResponse response)
        {
            const string sqlResumen = """
        select
            isnull(sum(R.SALDO), 0) as total_saldo,
            isnull(sum(R.CUOTA), 0) as total_cuota
        from REG_CREDITOS R
        where R.SALDO > 0
          and R.ESTADO = 'A'
          and R.CEDULA = @cedula
        """;

            var resumen = conn.QueryFirstOrDefault(sqlResumen, new { cedula });

            if (resumen == null)
            {
                return;
            }

            response.total_saldo = resumen.total_saldo ?? 0;
            response.total_cuota = resumen.total_cuota ?? 0;
        }

        private void Cr_SeguimientoRevisionesTag_Deudas_CargarDeducciones(
            DbConnection conn,
            long idSolicitud,
            CrSeguimientoRevisionesTagDeudasResponse response)
        {
            var codPreanalisis = Cr_SeguimientoRevisionesTag_ObtenerPreanalisisOperacion(conn, idSolicitud);

            if (string.IsNullOrWhiteSpace(codPreanalisis) || codPreanalisis == "0")
            {
                return;
            }

            const string sqlDeducciones = """
        select isnull(sum(CUOTA_MENSUAL), 0)
        from CRD_PREA_DETALLE_DEDUC
        where COD_PREANALISIS = @cod_preanalisis
        """;

            response.deducciones = conn.QueryFirstOrDefault<decimal>(
                sqlDeducciones,
                new { cod_preanalisis = codPreanalisis });
        }

        private List<CrSeguimientoRevisionesTagDeudaRow> Cr_SeguimientoRevisionesTag_Deudas_CargarLista(
            DbConnection conn,
            string cedula)
        {
            const string sqlLista = """
        exec spSIFEstadoCreditos @cedula
        """;

            var rows = conn.Query(sqlLista, new { cedula }).ToList();
            var lista = new List<CrSeguimientoRevisionesTagDeudaRow>();

            foreach (var row in rows)
            {
                lista.Add(Cr_SeguimientoRevisionesTag_Deudas_MapearRow(row));
            }

            return lista;
        }

        private CrSeguimientoRevisionesTagDeudaRow Cr_SeguimientoRevisionesTag_Deudas_MapearRow(dynamic row)
        {
            return new CrSeguimientoRevisionesTagDeudaRow
            {
                seleccionado = false,
                semaforo = Cr_SeguimientoRevisionesTag_Deudas_ResolverSemaforo(row),
                operacion = (row.id_solicitud ?? string.Empty).ToString().Trim(),
                linea = (row.codigo ?? string.Empty).ToString().Trim(),
                plazo = row.plazo ?? 0,
                monto = row.MontoApr ?? 0,
                saldo = row.Saldo ?? 0,
                cuota = row.Cuota ?? 0,
                primero = Cr_SeguimientoRevisionesTag_Deudas_FormatearPrimerMovimiento(row.prideduc),
                mora = (row.MoraPrincipal ?? 0) + (row.MoraInt ?? 0),
                garantia = (row.Garantia ?? string.Empty).ToString().Trim(),
            };
        }

        private static string Cr_SeguimientoRevisionesTag_Deudas_ResolverSemaforo(dynamic row)
        {
            decimal moraCuota = row.MoraCuota ?? 0;
            string procesoCod = (row.ProcesoCod ?? string.Empty).ToString().Trim();
            string estado = (row.Estado ?? string.Empty).ToString().Trim();
            string referencia = (row.Referencia ?? string.Empty).ToString().Trim();
            decimal indicadorCbr = row.IndicadorCbr ?? 0;

            if (moraCuota > 0 && procesoCod != "J")
            {
                return "rojo";
            }

            if (procesoCod == "J")
            {
                return "judicial";
            }

            if (!string.IsNullOrWhiteSpace(referencia) && moraCuota == 0)
            {
                return "amarillo";
            }

            if (indicadorCbr > 0)
            {
                return "reversado";
            }

            if (estado.StartsWith('C'))
            {
                return "cancelado";
            }

            return "verde";
        }

        private static string Cr_SeguimientoRevisionesTag_Deudas_FormatearPrimerMovimiento(dynamic priDeduc)
        {
            if (priDeduc == null)
            {
                return string.Empty;
            }

            return Convert.ToDecimal(priDeduc).ToString("0000-00");
        }

        /// <summary>
        /// Obtiene el resumen y la lista de fianzas activas asociadas a la persona de la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Resumen y lista de fianzas activas.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagFianzasResponse> Cr_SeguimientoRevisionesTag_Fianzas_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagFianzasResponse>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var cedula = Cr_SeguimientoRevisionesTag_ObtenerCedulaOperacion(conn, request.id_solicitud);

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagFianzasResponse>(
                        validaCedula);
                }

                var response = new CrSeguimientoRevisionesTagFianzasResponse();

                const string sqlResumen = """
            select
                isnull(sum(R.MONTOAPR), 0) as monto,
                isnull(sum(R.SALDO), 0) as saldo,
                isnull(sum(R.CUOTA), 0) as cuota
            from REG_CREDITOS R
            where R.SALDO > 0
              and R.ESTADO = 'A'
              and R.ID_SOLICITUD in
              (
                  select ID_SOLICITUD
                  from FIADORES
                  where CEDULAF = @cedula
                    and FIRMA = 'S'
              )
            """;

                var resumen = conn.QueryFirstOrDefault(sqlResumen, new { cedula = cedula.Trim() });

                if (resumen != null)
                {
                    response.monto = resumen.monto ?? 0;
                    response.saldo = resumen.saldo ?? 0;
                    response.cuota = resumen.cuota ?? 0;
                }

                const string sqlLista = """
            select 
                cast(R.ID_SOLICITUD as varchar(50)) as operacion,
                rtrim(R.CODIGO) as linea,
                rtrim(R.CEDULA) as cedula_deudor,
                rtrim(S.NOMBRE) as nombre,
                isnull(R.MONTOAPR, 0) as monto,
                isnull(R.SALDO, 0) as saldo,
                isnull(R.CUOTA, 0) as cuota
            from REG_CREDITOS R
            inner join FIADORES F on R.ID_SOLICITUD = F.ID_SOLICITUD
            inner join SOCIOS S on R.CEDULA = S.CEDULA
            where R.SALDO > 0
              and R.ESTADO = 'A'
              and F.CEDULAF = @cedula
              and F.FIRMA = 'S'
            GROUP by R.ID_SOLICITUD, R.CODIGO, R.CEDULA,S.NOMBRE, R.MONTOAPR, R.SALDO, R.CUOTA 
            order by R.ID_SOLICITUD
            """;

                response.lista = conn.Query<CrSeguimientoRevisionesTagFianzaRow>(
                    sqlLista,
                    new { cedula = cedula.Trim() }).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagFianzasResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de refundiciones asociadas a la operación seleccionada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Lista de refundiciones.</returns>
        public ErrorDto<List<CrSeguimientoRevisionesTagRefundicionRow>> Cr_SeguimientoRevisionesTag_Refundiciones_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagRefundicionRow>>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sql = """
                select
                    cast(R.ID_SOLICITUD as varchar(50)) as operacion,
                    rtrim(R.CODIGO) as linea,
                    isnull(R.PLAZO, 0) as plazo,
                    isnull(R.MONTOAPR, 0) as monto,
                    isnull(RE.MONTO, 0) as refundicion,
                    isnull(R.CUOTA, 0) as cuota,
                    isnull(rtrim(R.GARANTIA), '') as garantia
                from REG_CREDITOS R
                inner join REFUNDICIONES RE on R.ID_SOLICITUD = RE.ID_SOLICITUD
                where RE.ID_SOLICITUDR = @id_solicitud
                order by R.ID_SOLICITUD
                """;

                var lista = conn.Query<CrSeguimientoRevisionesTagRefundicionRow>(
                    sql,
                    new { id_solicitud = request.id_solicitud }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagRefundicionRow>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de desembolsos asociados al preanálisis de la operación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Lista de desembolsos.</returns>
        public ErrorDto<List<CrSeguimientoRevisionesTagDesembolsoRow>> Cr_SeguimientoRevisionesTag_Desembolsos_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagDesembolsoRow>>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var codPreanalisis = Cr_SeguimientoRevisionesTag_ObtenerPreanalisisOperacion(conn, request.id_solicitud);

                if (string.IsNullOrWhiteSpace(codPreanalisis) || codPreanalisis == "0")
                {
                    return DbHelper.CreateOkResponse(new List<CrSeguimientoRevisionesTagDesembolsoRow>());
                }

                const string sql = """
                        select
                            rtrim(DESCRIPCION) as concepto,
                            isnull(MONTO, 0) as monto,
                            isnull(CUOTA, 0) as cuota
                        from CRD_PREA_DETALLE_DESEMBOLSOS
                        where COD_PREANALISIS = @cod_preanalisis
                        order by DESCRIPCION
            """;

                var lista = conn.Query<CrSeguimientoRevisionesTagDesembolsoRow>(
                    sql,
                    new { cod_preanalisis = codPreanalisis }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagDesembolsoRow>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle del fiador o deudor mostrado en la pestaña de información personal.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con cédula, tipo e identificación de operación.</param>
        /// <returns>Detalle del fiador o deudor.</returns>
        public ErrorDto<CrSeguimientoRevisionesTagFiadorResponse> Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagFiadorRequest request)
        {
            if (request == null || request.id_solicitud <= 0 || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagFiadorResponse>(
                    "Debe indicar un fiador/deudor válido.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var codPreanalisis = Cr_SeguimientoRevisionesTag_ObtenerPreanalisisOperacion(conn, request.id_solicitud);

               

                var sql = """
            select
                dbo.fxEC_Membresia(S.CEDULA, dbo.MyGetdate()) as membresia,
                S.FECHAINGRESO as fechaingreso,
                I.DESCRIPCION as institucion,
                isnull(P.SALARIO_LIQUIDO, 0) as salario_liquido,
                isnull(P.LIQUIDEZ_SIMPLE, 0) as liquidez_simple,
                isnull(P.LIQUIDEZ_CFIANZAS, 0) as liquidez_cfianzas,
                PR.DESCRIPCION as provincia,
                C.DESCRIPCION as canton,
                D.DESCRIPCION as distrito,
                isnull(S.DIRECCION, '') as direccion,
                isnull(P.DEVENGADO_MES, 0) as devengado_mes,
                isnull(P.COD_PREANALISIS, '') as cod_preanalisis
            from SOCIOS S
            inner join INSTITUCIONES I on S.COD_INSTITUCION = I.COD_INSTITUCION
            left join PROVINCIAS PR on S.PROVINCIA = PR.PROVINCIA
            left join CANTONES C
                on S.CANTON = C.CANTON
               and S.PROVINCIA = C.PROVINCIA
            left join DISTRITOS D
                on S.DISTRITO = D.DISTRITO
               and S.CANTON = D.CANTON
               and S.PROVINCIA = D.PROVINCIA
            left join CRD_PREA_PREANALISIS P on S.CEDULA = P.CEDULA
            where S.CEDULA = @cedula
            """;

                if (!string.IsNullOrWhiteSpace(codPreanalisis))
                {
                    if ((request.tipo ?? string.Empty).Trim().Equals("Fiador", StringComparison.OrdinalIgnoreCase))
                    {
                        sql += """
                 and P.COD_PREANALISIS_REF = @cod_preanalisis
                """;
                    }
                    else
                    {
                        sql += """
                 and P.COD_PREANALISIS = @cod_preanalisis
                """;
                    }
                }

                var row = conn.QueryFirstOrDefault(
                    sql,
                    new
                    {
                        cedula = request.cedula.Trim(),
                        cod_preanalisis = codPreanalisis
                    });

                if (row == null)
                {
                    return DbHelper.CreateOkResponse(new CrSeguimientoRevisionesTagFiadorResponse());
                }

                var response = new CrSeguimientoRevisionesTagFiadorResponse
                {
                    membresia = row.membresia ?? string.Empty,
                    fechaingreso = row.fechaingreso,
                    institucion = row.institucion ?? string.Empty,
                    salario_liquido = row.salario_liquido ?? 0,
                    liquidez_simple = row.liquidez_simple ?? 0,
                    liquidez_cfianzas = row.liquidez_cfianzas ?? 0,
                    provincia = row.provincia ?? string.Empty,
                    canton = row.canton ?? string.Empty,
                    distrito = row.distrito ?? string.Empty,
                    direccion = row.direccion ?? string.Empty,
                    cod_preanalisis = row.cod_preanalisis ?? string.Empty
                };

                decimal devengadoMes = row.devengado_mes ?? 0;

                if (devengadoMes != 0)
                {
                    response.liquidez_simple_porc = (response.liquidez_simple / devengadoMes) * 100m;
                    response.liquidez_cfianzas_porc = (response.liquidez_cfianzas / devengadoMes) * 100m;
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrSeguimientoRevisionesTagFiadorResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la clasificación de preanálisis asociada al fiador o deudor seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con cédula, tipo e identificación de operación.</param>
        /// <returns>Clasificación calculada por preanálisis.</returns>
        public ErrorDto<List<CrSeguimientoRevisionesTagClasificacionRow>> Cr_SeguimientoRevisionesTag_FiadorClasificacion_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagFiadorRequest request)
        {
            if (request == null || request.id_solicitud <= 0 || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagClasificacionRow>>(
                    "Debe indicar un fiador/deudor válido.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var fiadorResponse = Cr_SeguimientoRevisionesTag_FiadorDetalle_Obtener(codEmpresa, request);

                if (fiadorResponse.Code == -1)
                {
                    return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagClasificacionRow>>(
                        fiadorResponse.Description ?? "No fue posible obtener el detalle del fiador.");
                }

                var detalle = fiadorResponse.Result ?? new CrSeguimientoRevisionesTagFiadorResponse();

                if (string.IsNullOrWhiteSpace(detalle.cod_preanalisis))
                {
                    return DbHelper.CreateOkResponse(new List<CrSeguimientoRevisionesTagClasificacionRow>());
                }

                const string sql = """
            exec spCRDPreaClasificacion @cedula, @liquidez_cfianzas_porc, @expediente
            """;

                var lista = new List<CrSeguimientoRevisionesTagClasificacionRow>();

                using var multi = conn.QueryMultiple(
                    sql,
                    new
                    {
                        cedula = request.cedula.Trim(),
                        liquidez_cfianzas_porc = detalle.liquidez_cfianzas_porc,
                        expediente = detalle.cod_preanalisis
                    });

                while (!multi.IsConsumed)
                {
                    var resultset = multi.Read().ToList();

                    foreach (var row in resultset)
                    {
                        var item = new CrSeguimientoRevisionesTagClasificacionRow
                        {
                            descripcion = row.CODIGO?.ToString() ?? string.Empty,
                            valor = row.COLOR?.ToString() ?? string.Empty,
                            observacion = row.RAZON?.ToString() ?? string.Empty
                        };

                        lista.Add(item);
                    }
                }

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagClasificacionRow>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de personas mostradas en la pestaña de información personal.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Solicitud con la operación seleccionada.</param>
        /// <returns>Lista de deudor y fiadores.</returns>
        public ErrorDto<List<CrSeguimientoRevisionesTagPersonaRow>> Cr_SeguimientoRevisionesTag_Personas_Obtener(
            int codEmpresa,
            CrSeguimientoRevisionesTagDetalleRequest request)
        {
            if (request == null || request.id_solicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagPersonaRow>>(
                    validaSolicitud);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var cedula = Cr_SeguimientoRevisionesTag_ObtenerCedulaOperacion(conn, request.id_solicitud);

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagPersonaRow>>(
                        validaCedula);
                }

                const string sql = """
            select
                X.cedula,
                X.nombre,
                X.estado,
                X.calidad,
                X.est_lab
            from
            (
                select
                    0 as orden,
                    rtrim(S.CEDULA) as cedula,
                    rtrim(S.NOMBRE) as nombre,
                    'Deudor' as estado,
                    rtrim(EST.DESCRIPCION) as calidad,
                    isnull(rtrim(EL.DESCRIPCION), '') as est_lab
                from SOCIOS S
                inner join AFI_ESTADOS_PERSONA EST
                    on S.ESTADOACTUAL = EST.COD_ESTADO
                left join AFI_ESTADO_LABORAL EL
                    on S.ESTADOLABORAL = EL.ESTADO_LABORAL
                where S.CEDULA = @cedula

                union all

                select
                    1 as orden,
                    rtrim(F.CEDULAF) as cedula,
                    rtrim(S.NOMBRE) as nombre,
                    'Fiador' as estado,
                    rtrim(EST.DESCRIPCION) as calidad,
                    isnull(rtrim(EL.DESCRIPCION), '') as est_lab
                from FIADORES F
                inner join SOCIOS S
                    on F.CEDULAF = S.CEDULA
                inner join AFI_ESTADOS_PERSONA EST
                    on S.ESTADOACTUAL = EST.COD_ESTADO
                left join AFI_ESTADO_LABORAL EL
                    on S.ESTADOLABORAL = EL.ESTADO_LABORAL
                where F.ID_SOLICITUD = @id_solicitud
            ) X
            order by X.orden, X.cedula
            """;

                var lista = conn.Query<CrSeguimientoRevisionesTagPersonaRow>(
                    sql,
                    new
                    {
                        cedula = cedula.Trim(),
                        id_solicitud = request.id_solicitud
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrSeguimientoRevisionesTagPersonaRow>>(ex.Message);
            }
        }

    }
}