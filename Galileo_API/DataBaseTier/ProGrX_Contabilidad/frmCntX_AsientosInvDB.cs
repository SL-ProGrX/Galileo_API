using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXAsientosInvDb
    {
        private const int vModulo = 20;
        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MSecurityMainDb _bitacoraDb;

        public FrmCntXAsientosInvDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _bitacoraDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene el período contable utilizado para inicializar
        /// el formulario de asientos de inventario periódico.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<DefMascarasDto>
            CntX_frmCntX_AsientosInv_Parametros_Obtener(
                int codEmpresa)
        {
            var parametros =
                _cntLinkDb.sbgCntParametros(
                    codEmpresa);

            if (parametros.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    parametros.Description
                        ?? "No fue posible obtener los par&aacute;metros contables.",
                    parametros.Code.GetValueOrDefault(-1),
                    new DefMascarasDto());
            }

            if (!parametros.Result.gPeriodoAnio.HasValue
                || !parametros.Result.gPeriodoMes.HasValue
                || parametros.Result.gPeriodoAnio.Value <= 0
                || parametros.Result.gPeriodoMes.Value
                    is < 1 or > 12)
            {
                return DbHelper.CreateErrorResponse(
                    "El per&iacute;odo contable no es v&aacute;lido.",
                    -2,
                    new DefMascarasDto());
            }

            return DbHelper.CreateOkResponse(
                parametros.Result);
        }

        /// <summary>
        /// Obtiene el encabezado y el detalle de un asiento
        /// de ajuste de inventario periodico.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="numAsiento"></param>
        /// <returns></returns>
        public ErrorDto<CntXAsientosInvResponse?>
            CntX_frmCntX_AsientosInv_Asiento_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? numAsiento)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvResponse?>(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(numAsiento))
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvResponse?>(
                    "El n&uacute;mero de asiento es requerido.",
                    -2,
                    null);
            }

            const string sqlEncabezado = """
                select top 1
                    cod_contabilidad,
                    rtrim(num_asiento) as num_asiento,
                    anio,
                    mes,
                    fecha_asiento,
                    isnull(rtrim(descripcion), '') as descripcion,
                    isnull(rtrim(notas), '') as notas
                from CntX_Inv_Asientos
                where cod_contabilidad = @CodContabilidad
                  and num_asiento = @NumAsiento;
                """;

            const string sqlDetalle = """
                select
                    rtrim(A.cod_cuenta) as cod_cuenta,
                    isnull(
                        rtrim(B.cod_cuenta_mask),
                        rtrim(A.cod_cuenta)
                    ) as cod_cuenta_mask,
                    isnull(rtrim(B.descripcion), '') as descripcion,
                    isnull(rtrim(A.documento), '') as documento,
                    isnull(rtrim(A.detalle), '') as detalle,
                    isnull(A.monto_debito, 0) as monto_debito,
                    isnull(A.monto_credito, 0) as monto_credito,
                    A.num_linea
                from CntX_Inv_Asientos_Detalle A
                inner join CntX_Cuentas B
                    on B.cod_contabilidad = A.cod_contabilidad
                   and B.cod_cuenta = A.cod_cuenta
                where A.cod_contabilidad = @CodContabilidad
                  and A.num_asiento = @NumAsiento
                order by A.num_linea;
                """;

            var consulta =
                DbHelper.WithConn<CntXAsientosInvResponse?>(
                    _portalDb,
                    codEmpresa,
                    connection =>
                    {
                        var parametros = new
                        {
                            CodContabilidad = codContabilidad,
                            NumAsiento = numAsiento.Trim()
                        };

                        var encabezado =
                            connection.QueryFirstOrDefault<
                                CntXAsientosInvData>(
                                sqlEncabezado,
                                parametros);

                        if (encabezado is null)
                        {
                            return null;
                        }

                        var detalle =
                            connection.Query<
                                CntXAsientosInvDetalleData>(
                                sqlDetalle,
                                parametros).ToList();

                        return new CntXAsientosInvResponse
                        {
                            asiento = encabezado,
                            detalle = detalle
                        };
                    });

            if (consulta.Result is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvResponse?>(
                    "El asiento indicado no existe.",
                    -2,
                    null);
            }

            return DbHelper.CreateOkResponse<
                CntXAsientosInvResponse?>(
                consulta.Result);
        }

        /// <summary>
        /// Obtiene los asientos correspondientes a un periodo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_AsientosInv_Asientos_Lista_Obtener(
                int codEmpresa,
                CntXAsientosInvListaRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos de consulta son requeridos.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            if (request.cod_contabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            if (!CntX_frmCntX_AsientosInv_Periodo_EsValido(
                    request.anio,
                    request.mes))
            {
                return DbHelper.CreateErrorResponse(
                    "El per&iacute;odo indicado no es v&aacute;lido.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = """
                select
                    rtrim(num_asiento) as item,
                    isnull(rtrim(descripcion), '') as descripcion
                from CntX_Inv_Asientos
                where cod_contabilidad = @CodContabilidad
                  and anio = @Anio
                  and mes = @Mes
                order by num_asiento;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodContabilidad =
                        request.cod_contabilidad,
                    Anio = request.anio,
                    Mes = request.mes
                });
        }

        /// <summary>
        /// Formatea una cuenta y verifica que acepte movimientos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<CntXAsientosInvCuentaData?>
            CntX_frmCntX_AsientosInv_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvCuentaData?>(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvCuentaData?>(
                    "Cuenta no es v&aacute;lida.",
                    -2,
                    null);
            }

            string cuentaFormateada =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    cuenta.Trim(),
                    0);

            if (string.IsNullOrWhiteSpace(cuentaFormateada))
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvCuentaData?>(
                    "Cuenta no es v&aacute;lida.",
                    -2,
                    null);
            }

            const string sql = """
                select top 1
                    rtrim(cod_cuenta) as cod_cuenta,
                    isnull(
                        rtrim(cod_cuenta_mask),
                        rtrim(cod_cuenta)
                    ) as cod_cuenta_mask,
                    isnull(rtrim(descripcion), '') as descripcion
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta
                  and acepta_movimientos = 1;
                """;

            var consulta =
                DbHelper.ExecuteSingleQuery<
                    CntXAsientosInvCuentaData?>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    null,
                    new
                    {
                        CodContabilidad = codContabilidad,
                        Cuenta = cuentaFormateada
                    });

            if (consulta.Result is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAsientosInvCuentaData?>(
                    "La cuenta no existe o no acepta movimientos.",
                    -2,
                    null);
            }

            return consulta;
        }

        /// <summary>
        /// Registra o modifica un asiento de inventario periodico.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CntX_frmCntX_AsientosInv_Guardar(
                int codEmpresa,
                CntXAsientosInvGuardarRequest request)
        {
            var validacion =
                CntX_frmCntX_AsientosInv_Guardar_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            var detallePreparado =
                CntX_frmCntX_AsientosInv_Detalle_Preparar(
                    codEmpresa,
                    request);

            if (detallePreparado.Result is null)
            {
                return DbHelper.ErrorResponse(
                    detallePreparado.Description
                        ?? "El detalle del asiento no es v&aacute;lido.",
                    detallePreparado.Code.GetValueOrDefault(-1));
            }

            using var connection =
                DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

            try
            {
                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    var asiento = request.asiento;

                    bool existe =
                        CntX_frmCntX_AsientosInv_Asiento_Existe(
                            connection,
                            transaction,
                            asiento.cod_contabilidad,
                            asiento.num_asiento);

                    if (!request.edita && existe)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "El n&uacute;mero de asiento ya existe.",
                            -2);
                    }

                    if (request.edita && !existe)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "El asiento que desea modificar no existe.",
                            -2);
                    }

                    if (request.edita)
                    {
                        CntX_frmCntX_AsientosInv_Encabezado_Actualizar(
                            connection,
                            transaction,
                            request);
                    }
                    else
                    {
                        CntX_frmCntX_AsientosInv_Encabezado_Insertar(
                            connection,
                            transaction,
                            request);
                    }

                    string numAsiento =
                        asiento.num_asiento.Trim();

                    CntX_frmCntX_AsientosInv_Detalle_Eliminar(
                        connection,
                        transaction,
                        asiento.cod_contabilidad,
                        numAsiento);

                    CntX_frmCntX_AsientosInv_Detalle_Insertar(
                        connection,
                        transaction,
                        asiento.cod_contabilidad,
                        numAsiento,
                        detallePreparado.Result);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                CntX_frmCntX_AsientosInv_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario.Trim(),
                    request.edita
                        ? "Modifica"
                        : "Registra",
                    CntX_frmCntX_AsientosInv_Bitacora_Detalle_Construir(
                        request.asiento.num_asiento,
                        request.asiento.cod_contabilidad));

                return DbHelper.OkResponse(
                    "Informaci&oacute;n guardada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un asiento de ajuste
        /// para inventario periodico.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CntX_frmCntX_AsientosInv_Eliminar(
                int codEmpresa,
                CntXAsientosInvEliminarRequest request)
        {
            var validacion =
                CntX_frmCntX_AsientosInv_Eliminar_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            using var connection =
                DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

            try
            {
                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    var parametros = new
                    {
                        CodContabilidad =
                            request.cod_contabilidad,
                        NumAsiento =
                            request.num_asiento.Trim()
                    };

                    if (!CntX_frmCntX_AsientosInv_Asiento_Existe(
                            connection,
                            transaction,
                            request.cod_contabilidad,
                            request.num_asiento))
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "El asiento indicado no existe.",
                            -2);
                    }

                    CntX_frmCntX_AsientosInv_Detalle_Eliminar(
                        connection,
                        transaction,
                        request.cod_contabilidad,
                        request.num_asiento.Trim());

                    const string sqlEncabezado = """
                        delete from CntX_Inv_Asientos
                        where cod_contabilidad = @CodContabilidad
                          and num_asiento = @NumAsiento;
                        """;

                    connection.Execute(
                        sqlEncabezado,
                        parametros,
                        transaction);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                CntX_frmCntX_AsientosInv_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario.Trim(),
                    "Elimina",
                    CntX_frmCntX_AsientosInv_Bitacora_Detalle_Construir(
                        request.num_asiento,
                        request.cod_contabilidad));

                return DbHelper.OkResponse(
                    "Informaci&oacute;n eliminada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool
            CntX_frmCntX_AsientosInv_Asiento_Existe(
                SqlConnection connection,
                SqlTransaction transaction,
                int codContabilidad,
                string numAsiento)
        {
            const string sql = """
                select count(1)
                from CntX_Inv_Asientos
                where cod_contabilidad = @CodContabilidad
                  and num_asiento = @NumAsiento;
                """;

            int cantidad =
                connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        CodContabilidad = codContabilidad,
                        NumAsiento = numAsiento.Trim()
                    },
                    transaction);

            return cantidad > 0;
        }

        private static void
            CntX_frmCntX_AsientosInv_Encabezado_Insertar(
                SqlConnection connection,
                SqlTransaction transaction,
                CntXAsientosInvGuardarRequest request)
        {
            const string sql = """
                insert into CntX_Inv_Asientos
                (
                    cod_contabilidad,
                    num_asiento,
                    anio,
                    mes,
                    fecha_asiento,
                    descripcion,
                    notas
                )
                values
                (
                    @CodContabilidad,
                    @NumAsiento,
                    @Anio,
                    @Mes,
                    @FechaAsiento,
                    @Descripcion,
                    @Notas
                );
                """;

            connection.Execute(
                sql,
                CntX_frmCntX_AsientosInv_Encabezado_Parametros_Crear(
                    request),
                transaction);
        }

        private static void
            CntX_frmCntX_AsientosInv_Encabezado_Actualizar(
                SqlConnection connection,
                SqlTransaction transaction,
                CntXAsientosInvGuardarRequest request)
        {
            const string sql = """
                update CntX_Inv_Asientos
                set descripcion = @Descripcion,
                    fecha_asiento = @FechaAsiento,
                    notas = @Notas
                where cod_contabilidad = @CodContabilidad
                  and num_asiento = @NumAsiento;
                """;

            connection.Execute(
                sql,
                CntX_frmCntX_AsientosInv_Encabezado_Parametros_Crear(
                    request),
                transaction);
        }

        private static void
            CntX_frmCntX_AsientosInv_Detalle_Eliminar(
                SqlConnection connection,
                SqlTransaction transaction,
                int codContabilidad,
                string numAsiento)
        {
            const string sql = """
                delete from CntX_Inv_Asientos_Detalle
                where cod_contabilidad = @CodContabilidad
                  and num_asiento = @NumAsiento;
                """;

            connection.Execute(
                sql,
                new
                {
                    CodContabilidad = codContabilidad,
                    NumAsiento = numAsiento.Trim()
                },
                transaction);
        }

        private static void
    CntX_frmCntX_AsientosInv_Detalle_Insertar(
        SqlConnection connection,
        SqlTransaction transaction,
        int codContabilidad,
        string numAsiento,
        IReadOnlyCollection<CntXAsientosInvDetalleData> detalle)
        {
            if (detalle.Count == 0)
            {
                return;
            }

            const string sql = """
            insert into CntX_Inv_Asientos_Detalle
            (
                num_asiento,
                cod_contabilidad,
                cod_unidad,
                num_linea,
                cod_cuenta,
                documento,
                detalle,
                monto_debito,
                monto_credito
            )
            values
            (
                @NumAsiento,
                @CodContabilidad,
                @CodUnidad,
                @NumLinea,
                @CodCuenta,
                @Documento,
                @Detalle,
                @MontoDebito,
                @MontoCredito
            );
            """;

            var parametros =
                detalle.Select(
                    linea => new
                    {
                        NumAsiento = numAsiento,
                        CodContabilidad =
                            codContabilidad,
                        CodUnidad =
                            "OC",
                        NumLinea =
                            linea.num_linea,
                        CodCuenta =
                            linea.cod_cuenta,
                        Documento =
                            linea.documento,
                        Detalle =
                            linea.detalle,
                        MontoDebito =
                            linea.monto_debito,
                        MontoCredito =
                            linea.monto_credito
                    });

            connection.Execute(
                sql,
                parametros,
                transaction);
        }

        private ErrorDto<List<CntXAsientosInvDetalleData>>
            CntX_frmCntX_AsientosInv_Detalle_Preparar(
                int codEmpresa,
                CntXAsientosInvGuardarRequest request)
        {
            var lineasFormateadas =
                CntX_frmCntX_AsientosInv_Detalle_Formatear(
                    codEmpresa,
                    request.detalle);

            if (lineasFormateadas.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    lineasFormateadas.Description
                        ?? "El detalle del asiento no es v&aacute;lido.",
                    lineasFormateadas.Code.GetValueOrDefault(-1),
                    new List<CntXAsientosInvDetalleData>());
            }

            if (lineasFormateadas.Result.Count == 0)
            {
                return DbHelper.CreateOkResponse(
                    new List<CntXAsientosInvDetalleData>());
            }

            string[] cuentas =
                lineasFormateadas.Result
                    .Select(item => item.Cuenta)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();

            var cuentasValidas =
                CntX_frmCntX_AsientosInv_Cuentas_Validas_Obtener(
                    codEmpresa,
                    request.asiento.cod_contabilidad,
                    cuentas);

            if (cuentasValidas.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    cuentasValidas.Description
                        ?? "No fue posible validar las cuentas.",
                    cuentasValidas.Code.GetValueOrDefault(-1),
                    new List<CntXAsientosInvDetalleData>());
            }

            return CntX_frmCntX_AsientosInv_Detalle_Construir(
                lineasFormateadas.Result,
                cuentasValidas.Result);
        }

        private ErrorDto<List<CntXAsientosInvLineaFormateada>>
            CntX_frmCntX_AsientosInv_Detalle_Formatear(
                int codEmpresa,
                IEnumerable<CntXAsientosInvDetalleData> detalle)
        {
            var resultado =
                new List<CntXAsientosInvLineaFormateada>();

            var cuentasFormateadas =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var linea in detalle)
            {
                if (string.IsNullOrWhiteSpace(
                        linea.cod_cuenta))
                {
                    continue;
                }

                string cuentaOriginal =
                    linea.cod_cuenta.Trim();

                if (!cuentasFormateadas.TryGetValue(
                        cuentaOriginal,
                        out string? cuenta))
                {
                    cuenta =
                        _cntLinkDb.fxgCntCuentaFormato(
                            codEmpresa,
                            false,
                            cuentaOriginal,
                            0);

                    if (string.IsNullOrWhiteSpace(cuenta))
                    {
                        return DbHelper.CreateErrorResponse(
                            $"La cuenta {cuentaOriginal} "
                            + "no es v&aacute;lida.",
                            -2,
                            new List<CntXAsientosInvLineaFormateada>());
                    }

                    cuentasFormateadas[cuentaOriginal] =
                        cuenta;
                }

                resultado.Add(
                    new CntXAsientosInvLineaFormateada(
                        linea,
                        cuenta));
            }

            return DbHelper.CreateOkResponse(resultado);
        }

        private ErrorDto<List<CntXAsientosInvCuentaData>>
            CntX_frmCntX_AsientosInv_Cuentas_Validas_Obtener(
                int codEmpresa,
                int codContabilidad,
                IReadOnlyCollection<string> cuentas)
        {
            if (cuentas.Count == 0)
            {
                return DbHelper.CreateOkResponse(
                    new List<CntXAsientosInvCuentaData>());
            }

            const string sql = """
                select
                    rtrim(cod_cuenta) as cod_cuenta,
                    isnull(
                        rtrim(cod_cuenta_mask),
                        rtrim(cod_cuenta)
                    ) as cod_cuenta_mask,
                    isnull(rtrim(descripcion), '') as descripcion
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta in @Cuentas
                  and acepta_movimientos = 1;
                """;

            return DbHelper.ExecuteListQuery<
                CntXAsientosInvCuentaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        CodContabilidad = codContabilidad,
                        Cuentas = cuentas
                    });
        }

        private static ErrorDto<List<CntXAsientosInvDetalleData>>
            CntX_frmCntX_AsientosInv_Detalle_Construir(
                IEnumerable<CntXAsientosInvLineaFormateada>
                    lineasFormateadas,
                IEnumerable<CntXAsientosInvCuentaData>
                    cuentasConsultadas)
        {
            var cuentasValidas =
                cuentasConsultadas.ToDictionary(
                    item => item.cod_cuenta,
                    StringComparer.OrdinalIgnoreCase);

            var resultado =
                new List<CntXAsientosInvDetalleData>();

            foreach (var item in lineasFormateadas)
            {
                if (!cuentasValidas.TryGetValue(
                        item.Cuenta,
                        out var cuentaValida))
                {
                    return DbHelper.CreateErrorResponse(
                        $"La cuenta "
                        + $"{item.Linea.cod_cuenta.Trim()} "
                        + "no existe o no acepta movimientos.",
                        -2,
                        new List<CntXAsientosInvDetalleData>());
                }

                decimal debito =
                    Math.Max(
                        0,
                        item.Linea.monto_debito);

                decimal credito =
                    Math.Max(
                        0,
                        item.Linea.monto_credito);

                if (debito > 0)
                {
                    credito = 0;
                }

                resultado.Add(
                    new CntXAsientosInvDetalleData
                    {
                        cod_cuenta = item.Cuenta,
                        cod_cuenta_mask =
                            cuentaValida.cod_cuenta_mask,
                        descripcion =
                            cuentaValida.descripcion,
                        documento =
                            item.Linea.documento
                            ?? string.Empty,
                        detalle =
                            item.Linea.detalle
                            ?? string.Empty,
                        monto_debito = debito,
                        monto_credito = credito,
                        num_linea =
                            resultado.Count + 1
                    });
            }

            return DbHelper.CreateOkResponse(resultado);
        }

        private static ErrorDto?
            CntX_frmCntX_AsientosInv_Guardar_Validar(
                CntXAsientosInvGuardarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del asiento son requeridos.",
                    -2);
            }

            if (request.asiento is null)
            {
                return DbHelper.ErrorResponse(
                    "El encabezado del asiento es requerido.",
                    -2);
            }

            if (request.asiento.cod_contabilidad <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(
                    request.asiento.num_asiento))
            {
                return DbHelper.ErrorResponse(
                    "El n&uacute;mero de asiento es requerido.",
                    -2);
            }

            if (request.asiento.num_asiento.Trim().Length > 15)
            {
                return DbHelper.ErrorResponse(
                    "El n&uacute;mero de asiento no puede "
                    + "superar 15 caracteres.",
                    -2);
            }

            if (!CntX_frmCntX_AsientosInv_Periodo_EsValido(
                    request.asiento.anio,
                    request.asiento.mes))
            {
                return DbHelper.ErrorResponse(
                    "El per&iacute;odo indicado no es v&aacute;lido.",
                    -2);
            }

            if (request.asiento.fecha_asiento == default)
            {
                return DbHelper.ErrorResponse(
                    "La fecha del asiento es requerida.",
                    -2);
            }

            if (request.asiento.fecha_asiento.Year
                    != request.asiento.anio
                || request.asiento.fecha_asiento.Month
                    != request.asiento.mes)
            {
                return DbHelper.ErrorResponse(
                    "La fecha del asiento no corresponde "
                    + "con el per&iacute;odo indicado.",
                    -2);
            }

            if ((request.asiento.descripcion
                    ?? string.Empty).Length > 60)
            {
                return DbHelper.ErrorResponse(
                    "La descripci&oacute;n no puede superar "
                    + "60 caracteres.",
                    -2);
            }

            if ((request.asiento.notas
                    ?? string.Empty).Length > 60)
            {
                return DbHelper.ErrorResponse(
                    "Las notas no pueden superar "
                    + "60 caracteres.",
                    -2);
            }

            request.detalle ??= [];

            return null;
        }

        private static ErrorDto?
            CntX_frmCntX_AsientosInv_Eliminar_Validar(
                CntXAsientosInvEliminarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del asiento son requeridos.",
                    -2);
            }

            if (request.cod_contabilidad <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(
                    request.num_asiento))
            {
                return DbHelper.ErrorResponse(
                    "El n&uacute;mero de asiento es requerido.",
                    -2);
            }

            if (request.num_asiento.Trim().Length > 15)
            {
                return DbHelper.ErrorResponse(
                    "El n&uacute;mero de asiento no puede "
                    + "superar 15 caracteres.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    -2);
            }

            return null;
        }

        private static object
            CntX_frmCntX_AsientosInv_Encabezado_Parametros_Crear(
                CntXAsientosInvGuardarRequest request)
        {
            return new
            {
                CodContabilidad =
                    request.asiento.cod_contabilidad,
                NumAsiento =
                    request.asiento.num_asiento.Trim(),
                Anio = request.asiento.anio,
                Mes = request.asiento.mes,
                FechaAsiento =
                    request.asiento.fecha_asiento.Date,
                Descripcion =
                    (request.asiento.descripcion
                        ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant(),
                Notas =
                    (request.asiento.notas
                        ?? string.Empty)
                    .Trim()
            };
        }

        private void
            CntX_frmCntX_AsientosInv_Bitacora_Registrar(
                int codEmpresa,
                string usuario,
                string movimiento,
                string detalle)
        {
            _bitacoraDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = vModulo
                });
        }

        private static string
            CntX_frmCntX_AsientosInv_Bitacora_Detalle_Construir(
                string numAsiento,
                int codContabilidad)
        {
            return $"Asiento INV.PER. : "
                + $"{numAsiento.Trim()} "
                + $"Conta.{codContabilidad}";
        }

        private static bool
            CntX_frmCntX_AsientosInv_Periodo_EsValido(
                int anio,
                int mes)
        {
            return anio is >= 1753 and <= 9999
                && mes is >= 1 and <= 12;
        }

        private sealed record CntXAsientosInvLineaFormateada(
            CntXAsientosInvDetalleData Linea,
            string Cuenta);
    }
}