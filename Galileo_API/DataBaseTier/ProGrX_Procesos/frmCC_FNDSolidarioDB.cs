using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.FrmCcFndSolidarioModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public sealed class FrmCcFndSolidarioDB
    {
        private const int TiempoEsperaSegundos = 5000;

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        private static readonly DateTime FechaCorteFnds =
            new(2004, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);

        private const string SqlFndsPaso1 = """
            select
                R.cedula as Cedula,
                sum(R.montoapr) as Monto
            from reg_creditos R
            inner join Catalogo C
                on R.codigo = C.codigo
            inner join Socios S
                on R.cedula = S.cedula
            where C.retencion = 'N'
              and C.poliza = 'N'
              and C.cobertura = 1
              and R.garantia in ('A', 'N')
              and R.saldo > 0
              and R.estado = 'A'
              and R.proceso <> 'J'
              and R.fechaforp < @FechaCorte
              and S.cod_institucion = @CodInstitucion
            group by R.cedula;
            """;

        private const string SqlFndsPaso2 = """
            select
                R.cedula as Cedula,
                sum(R.montoapr) as Monto
            from reg_creditos R
            inner join Catalogo C
                on R.codigo = C.codigo
            inner join Socios S
                on R.cedula = S.cedula
            where C.retencion = 'N'
              and C.poliza = 'N'
              and C.cobertura = 1
              and R.garantia in ('F', 'X')
              and R.saldo > 0
              and R.estado = 'A'
              and R.proceso <> 'J'
              and R.fechaforp < @FechaCorte
              and S.cod_institucion = @CodInstitucion
            group by R.cedula;
            """;

        private const string SqlFndsPaso3 = """
            select
                R.cedula as Cedula,
                sum(R.montoapr) as Monto
            from reg_creditos R
            inner join Catalogo C
                on R.codigo = C.codigo
            inner join Socios S
                on R.cedula = S.cedula
            where C.retencion = 'N'
              and C.poliza = 'N'
              and C.cobertura = 1
              and R.garantia not in ('H')
              and R.saldo > 0
              and R.estado = 'A'
              and R.proceso <> 'J'
              and R.fechaforp >= @FechaCorte
              and S.cod_institucion = @CodInstitucion
            group by R.cedula;
            """;

        private enum FndsPasoTipo
        {
            Paso1,
            Paso2,
            Paso3,
        }

        private enum FndsUpdateTipo
        {
            Reemplazar,
            SumarConSaldoMes,
            SumarSinSaldoMes,
        }

        private enum TipoFechaProceso
        {
            Anterior,
            Siguiente,
        }

        public FrmCcFndSolidarioDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene las instituciones habilitadas para ejecutar el proceso
        /// de Fondo Solidario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FNDSolidario_Instituciones_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    const string sql = """
                        select
                            cod_institucion as item,
                            rtrim(descripcion) as descripcion
                        from instituciones
                        where activa = 1
                          and cod_institucion in (1, 2)
                        order by descripcion;
                        """;

                    return connection
                        .Query<DropDownListaGenericaModel>(
                            sql)
                        .ToList();
                });
        }

        /// <summary>
        /// Ejecuta el proceso de Fondo Solidario o Fondo de Beneficio Social
        /// segun la configuración de la empresa.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto FrmCC_FNDSolidario_Ejecutar(
            int codEmpresa,
            string usuario,
            int codContabilidad,
            int codInstitucion)
        {
            try
            {
                string usuarioNormalizado =
                    usuario?.Trim() ?? string.Empty;

                ErrorDto? validacion = ValidarParametros(
                    codEmpresa,
                    usuarioNormalizado,
                    codContabilidad);

                if (validacion is not null)
                {
                    return validacion;
                }

                var globalesResponse =
                    _mProGrxMain.sbSifParametrosInicializa(
                        codEmpresa,
                        usuarioNormalizado,
                        codContabilidad);

                if (globalesResponse is null)
                {
                    return DbHelper.ErrorResponse(
                        "No se obtuvo respuesta al cargar los par&aacute;metros del sistema.");
                }

                int codigoRespuesta = globalesResponse.Code ?? 0;

                if (codigoRespuesta != 0)
                {
                    return DbHelper.ErrorResponse(
                        globalesResponse.Description ??
                            "Ocurri&oacute; un error al cargar los par&aacute;metros del sistema.",
                        codigoRespuesta == -2 ? -2 : -1);
                }

                Globales? globales = globalesResponse.Result;

                if (globales is null)
                {
                    return DbHelper.ErrorResponse(
                        "No fue posible obtener los par&aacute;metros necesarios para ejecutar el proceso.",
                        -2);
                }

                if (globales.GlngFechaCR <= 0)
                {
                    return DbHelper.ErrorResponse(
                        "No fue posible obtener el per&iacute;odo de cr&eacute;ditos para ejecutar el proceso.",
                        -2);
                }

                if (
                    globales.SysASEVersion &&
                    codInstitucion != 1 &&
                    codInstitucion != 2)
                {
                    return DbHelper.ErrorResponse(
                        "Debe seleccionar una instituci&oacute;n v&aacute;lida.",
                        -2);
                }

                return globales.SysASEVersion
                    ? FrmCC_FNDSolidario_Ejecutar_FNDS(
                        codEmpresa,
                        usuarioNormalizado,
                        codInstitucion,
                        globales)
                    : FrmCC_FNDSolidario_Ejecutar_FBEN(
                        codEmpresa,
                        usuarioNormalizado,
                        globales);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static ErrorDto? ValidarParametros(
            int codEmpresa,
            string usuario,
            int codContabilidad)
        {
            if (codEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    "No fue posible determinar la empresa de la sesi&oacute;n actual.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "No fue posible determinar el usuario de la sesi&oacute;n actual.",
                    -2);
            }

            if (codContabilidad <= 0)
            {
                return DbHelper.ErrorResponse(
                    "No fue posible determinar la contabilidad de la sesi&oacute;n actual.",
                    -2);
            }

            return null;
        }

        private ErrorDto FrmCC_FNDSolidario_Ejecutar_FNDS(
            int codEmpresa,
            string usuario,
            int codInstitucion,
            Globales globales)
        {
            return EjecutarEnTransaccion(
                codEmpresa,
                (connection, transaction) =>
                {
                    DateTime fechaServidor = FechaServidor(
                        connection,
                        transaction);

                    decimal fechaProcesoSiguiente = FxFechaProceso(
                        connection,
                        transaction,
                        globales.GlngFechaCR,
                        TipoFechaProceso.Siguiente);

                    const string codigo = "FNDS";

                    Db_FNDS_CancelarSinCobertura(
                        connection,
                        transaction,
                        codInstitucion,
                        codigo);

                    Db_FNDS_InicializarCuotas(
                        connection,
                        transaction,
                        codInstitucion,
                        codigo);

                    var contexto = new FondoSolidarioContext
                    {
                        CodEmpresa = codEmpresa,
                        CodInstitucion = codInstitucion,
                        Usuario = usuario,
                        GlngFechaCR = globales.GlngFechaCR,
                        FechaProcesoSiguiente = fechaProcesoSiguiente,
                        FechaServidor = fechaServidor,
                    };

                    FndsPasoConfig[] pasos =
                    [
                        new()
                        {
                            Rows = Db_FNDS_Paso_Listar(
                                connection,
                                transaction,
                                codInstitucion,
                                FndsPasoTipo.Paso1),
                            MontoBase = 150m,
                            Garantia = "S",
                            Actualizar =
                                (
                                    currentConnection,
                                    currentTransaction,
                                    idSolicitud,
                                    monto
                                ) =>
                                    Db_FNDS_ActualizarPaso(
                                        currentConnection,
                                        currentTransaction,
                                        idSolicitud,
                                        monto,
                                        FndsUpdateTipo.Reemplazar),
                        },
                        new()
                        {
                            Rows = Db_FNDS_Paso_Listar(
                                connection,
                                transaction,
                                codInstitucion,
                                FndsPasoTipo.Paso2),
                            MontoBase = 300m,
                            Garantia = "Z",
                            Actualizar =
                                (
                                    currentConnection,
                                    currentTransaction,
                                    idSolicitud,
                                    monto
                                ) =>
                                    Db_FNDS_ActualizarPaso(
                                        currentConnection,
                                        currentTransaction,
                                        idSolicitud,
                                        monto,
                                        FndsUpdateTipo.SumarConSaldoMes),
                        },
                        new()
                        {
                            Rows = Db_FNDS_Paso_Listar(
                                connection,
                                transaction,
                                codInstitucion,
                                FndsPasoTipo.Paso3),
                            MontoBase = 300m,
                            Garantia = "Z",
                            Actualizar =
                                (
                                    currentConnection,
                                    currentTransaction,
                                    idSolicitud,
                                    monto
                                ) =>
                                    Db_FNDS_ActualizarPaso(
                                        currentConnection,
                                        currentTransaction,
                                        idSolicitud,
                                        monto,
                                        FndsUpdateTipo.SumarSinSaldoMes),
                        },
                    ];

                    foreach (FndsPasoConfig paso in pasos)
                    {
                        ProcesarPasoFnds(
                            connection,
                            transaction,
                            contexto,
                            codigo,
                            paso);
                    }

                    Db_FNDS_CancelarPorCongelamiento(
                        connection,
                        transaction,
                        codInstitucion,
                        codigo);

                    return DbHelper.OkResponse(
                        "Fondo Solidario actualizado satisfactoriamente.");
                });
        }

        private ErrorDto FrmCC_FNDSolidario_Ejecutar_FBEN(
            int codEmpresa,
            string usuario,
            Globales globales)
        {
            return EjecutarEnTransaccion(
                codEmpresa,
                (connection, transaction) =>
                {
                    DateTime fechaServidor = FechaServidor(
                        connection,
                        transaction);

                    decimal fechaProcesoSiguiente = FxFechaProceso(
                        connection,
                        transaction,
                        globales.GlngFechaCR,
                        TipoFechaProceso.Siguiente);

                    decimal fechaProcesoAnterior = FxFechaProceso(
                        connection,
                        transaction,
                        globales.GlngFechaCR,
                        TipoFechaProceso.Anterior);

                    const string codigo = "FBEN";
                    const decimal monto = 800m;

                    Db_FBEN_ExcluirExSocios(
                        connection,
                        transaction,
                        codigo);

                    Db_FBEN_ActualizarCasosActuales(
                        connection,
                        transaction,
                        codigo,
                        monto);

                    var contexto = new FondoSolidarioContext
                    {
                        CodEmpresa = codEmpresa,
                        Usuario = usuario,
                        GlngFechaCR = globales.GlngFechaCR,
                        FechaProcesoSiguiente = fechaProcesoSiguiente,
                        FechaProcesoAnterior = fechaProcesoAnterior,
                        FechaServidor = fechaServidor,
                    };

                    Db_FBEN_InsertarCasosNuevos(
                        connection,
                        transaction,
                        contexto,
                        codigo,
                        monto);

                    Db_FBEN_CancelarSinAporteMas2Meses(
                        connection,
                        transaction,
                        codigo);

                    return DbHelper.OkResponse(
                        "Fondo de Beneficio Social actualizado satisfactoriamente.");
                });
        }

        private static decimal FxFechaProceso(
            IDbConnection connection,
            IDbTransaction transaction,
            decimal proceso,
            TipoFechaProceso tipo)
        {
            const string sqlAnterior = """
                select isnull(
                    dbo.fxSIFPrmProcesoAnt(@Proceso),
                    @Proceso
                );
                """;

            const string sqlSiguiente = """
                select isnull(
                    dbo.fxSIFPrmProcesoSig(@Proceso),
                    @Proceso
                );
                """;

            string sql = tipo switch
            {
                TipoFechaProceso.Anterior => sqlAnterior,
                TipoFechaProceso.Siguiente => sqlSiguiente,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(tipo),
                    tipo,
                    "El tipo de fecha de proceso no es v&aacute;lido."),
            };

            return connection.ExecuteScalar<decimal>(
                sql,
                new { Proceso = proceso },
                transaction,
                TiempoEsperaSegundos);
        }

        private static DateTime FechaServidor(
            IDbConnection connection,
            IDbTransaction transaction)
        {
            const string sql = "select getdate();";

            return connection.ExecuteScalar<DateTime>(
                sql,
                transaction: transaction,
                commandTimeout: TiempoEsperaSegundos);
        }

        private static decimal FxFondoSolidario(
            decimal monto,
            decimal montoBase)
        {
            return monto / 1000000m * montoBase;
        }

        private static void Db_FBEN_ExcluirExSocios(
            IDbConnection connection,
            IDbTransaction transaction,
            string codigo)
        {
            const string sql = """
                update reg_creditos
                set estado = 'C',
                    saldo = 0,
                    cuota = 0
                where estado = 'A'
                  and codigo = @Codigo
                  and cedula in (
                        select cedula
                        from socios
                        where estadoactual <> 'S'
                  );
                """;

            connection.Execute(
                sql,
                new { Codigo = codigo },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FBEN_ActualizarCasosActuales(
            IDbConnection connection,
            IDbTransaction transaction,
            string codigo,
            decimal monto)
        {
            const string sql = """
                update reg_creditos
                set montoapr = @Monto,
                    cuota = @Monto,
                    saldo = @Monto
                where estado = 'A'
                  and codigo = @Codigo
                  and cedula in (
                        select cedula
                        from socios
                        where estadoactual = 'S'
                  );
                """;

            connection.Execute(
                sql,
                new
                {
                    Codigo = codigo,
                    Monto = monto,
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FBEN_InsertarCasosNuevos(
            IDbConnection connection,
            IDbTransaction transaction,
            FondoSolidarioContext contexto,
            string codigo,
            decimal monto)
        {
            const string sql = """
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
                    documento_referido
                )
                select
                    @Codigo,
                    6,
                    cedula,
                    @Monto,
                    @Monto,
                    0,
                    @Monto,
                    0,
                    0,
                    @Monto,
                    @Monto,
                    0,
                    0,
                    999,
                    @Usuario,
                    @Usuario,
                    @Usuario,
                    @Usuario,
                    @Fecha,
                    @Fecha,
                    @Fecha,
                    @Fecha,
                    @Fecha,
                    @Fecha,
                    'N',
                    'N',
                    'OT',
                    '',
                    0,
                    1,
                    0,
                    'Proceso Automatico Cuota Mantenimiento CR',
                    'A',
                    @FechaProcesoSiguiente,
                    @FechaProcesoAnterior,
                    'F',
                    'AUTOMATICO'
                from socios
                where estadoactual = 'S'
                  and cedula not in (
                        select cedula
                        from reg_creditos
                        where estado = 'A'
                          and codigo = @Codigo
                  );
                """;

            connection.Execute(
                sql,
                new
                {
                    Codigo = codigo.ToUpperInvariant(),
                    Monto = monto,
                    Usuario = contexto.Usuario.Trim(),
                    Fecha = contexto.FechaServidor,
                    contexto.FechaProcesoSiguiente,
                    contexto.FechaProcesoAnterior,
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FBEN_CancelarSinAporteMas2Meses(
            IDbConnection connection,
            IDbTransaction transaction,
            string codigo)
        {
            const string sql = """
                update reg_creditos
                set estado = 'C',
                    saldo = 0
                where estado = 'A'
                  and codigo = @Codigo
                  and cedula in (
                        select A.cedula
                        from ahorro_consolidado A
                        inner join socios S
                            on A.cedula = S.cedula
                        where S.estadoactual = 'S'
                          and datediff(
                                month,
                                A.fecAporte,
                                getdate()
                              ) > 2
                  );
                """;

            connection.Execute(
                sql,
                new { Codigo = codigo },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FNDS_CancelarSinCobertura(
            IDbConnection connection,
            IDbTransaction transaction,
            int codInstitucion,
            string codigo)
        {
            const string sql = """
                update R
                set estado = 'C'
                from reg_creditos R
                inner join catalogo C
                    on R.codigo = C.codigo
                inner join Socios S
                    on R.cedula = S.cedula
                where S.cod_institucion = @CodInstitucion
                  and R.codigo = @Codigo
                  and R.estado = 'A'
                  and R.cedula not in (
                        select Reg.cedula
                        from reg_creditos Reg
                        inner join catalogo Cat
                            on Reg.codigo = Cat.codigo
                        where Cat.retencion = 'N'
                          and Cat.poliza = 'N'
                          and Cat.cobertura = 1
                          and Reg.garantia not in ('H')
                          and Reg.saldo > 0
                          and Reg.estado = 'A'
                          and Reg.proceso <> 'J'
                        group by Reg.cedula
                  );
                """;

            connection.Execute(
                sql,
                new
                {
                    CodInstitucion = codInstitucion,
                    Codigo = codigo,
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FNDS_InicializarCuotas(
            IDbConnection connection,
            IDbTransaction transaction,
            int codInstitucion,
            string codigo)
        {
            const string sql = """
                update R
                set cuota = 0,
                    saldo = 0,
                    montoapr = 0,
                    saldo_mes = 0
                from reg_creditos R
                inner join Socios S
                    on R.cedula = S.cedula
                where R.estado = 'A'
                  and R.codigo = @Codigo
                  and S.cod_institucion = @CodInstitucion;
                """;

            connection.Execute(
                sql,
                new
                {
                    Codigo = codigo,
                    CodInstitucion = codInstitucion,
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FNDS_ActualizarPaso(
            IDbConnection connection,
            IDbTransaction transaction,
            int idSolicitud,
            decimal montoFondo,
            FndsUpdateTipo tipo)
        {
            const string sqlReemplazar = """
                update reg_creditos
                set cuota = @Monto,
                    saldo = @Monto,
                    montoapr = @Monto,
                    saldo_mes = @Monto
                where id_solicitud = @IdSolicitud;
                """;

            const string sqlSumarConSaldoMes = """
                update reg_creditos
                set cuota = cuota + @Monto,
                    saldo = saldo + @Monto,
                    saldo_mes = saldo_mes + @Monto,
                    montoapr = montoapr + @Monto
                where id_solicitud = @IdSolicitud;
                """;

            const string sqlSumarSinSaldoMes = """
                update reg_creditos
                set cuota = cuota + @Monto,
                    saldo = saldo + @Monto,
                    montoapr = montoapr + @Monto
                where id_solicitud = @IdSolicitud;
                """;

            string sql = tipo switch
            {
                FndsUpdateTipo.Reemplazar =>
                    sqlReemplazar,

                FndsUpdateTipo.SumarConSaldoMes =>
                    sqlSumarConSaldoMes,

                FndsUpdateTipo.SumarSinSaldoMes =>
                    sqlSumarSinSaldoMes,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(tipo),
                    tipo,
                    "El tipo de actualizaci&oacute;n no es v&aacute;lido."),
            };

            connection.Execute(
                sql,
                new
                {
                    Monto = montoFondo,
                    IdSolicitud = idSolicitud,
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static (int IdSolicitud, decimal Cuota)?
            Db_FNDS_ObtenerActivo(
                IDbConnection connection,
                IDbTransaction transaction,
                string codigo,
                string cedula)
        {
            const string sql = """
                select top 1
                    id_solicitud as IdSolicitud,
                    cuota as Cuota
                from reg_creditos
                where codigo = @Codigo
                  and cedula = @Cedula
                  and estado = 'A';
                """;

            var resultado = connection
                .QueryFirstOrDefault<(int IdSolicitud, decimal Cuota)>(
                    sql,
                    new
                    {
                        Codigo = codigo,
                        Cedula = cedula,
                    },
                    transaction,
                    TiempoEsperaSegundos);

            return resultado.IdSolicitud <= 0
                ? null
                : resultado;
        }

        private static void Db_FNDS_CancelarPorCongelamiento(
            IDbConnection connection,
            IDbTransaction transaction,
            int codInstitucion,
            string codigo)
        {
            const string sql = """
                update R
                set estado = 'C'
                from reg_creditos R
                inner join Socios S
                    on R.cedula = S.cedula
                where R.estado = 'A'
                  and R.codigo = @Codigo
                  and S.cod_institucion = @CodInstitucion
                  and R.cedula in (
                        select cedula
                        from afi_congelar
                        where estado = 'A'
                          and fecha_finaliza >= getdate()
                          and per_cobro_fndSol = 0
                  );
                """;

            connection.Execute(
                sql,
                new
                {
                    Codigo = codigo,
                    CodInstitucion = codInstitucion,
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void Db_FNDS_InsertarPaso(
            IDbConnection connection,
            IDbTransaction transaction,
            FondoSolidarioContext contexto,
            string codigo,
            string cedula,
            decimal montoFondo,
            string garantia)
        {
            const string sql = """
                insert reg_creditos
                (
                    id_comite,
                    codigo,
                    cedula,
                    montosol,
                    montoapr,
                    plazo,
                    int,
                    interesv,
                    saldo,
                    interesc,
                    amortiza,
                    cuota,
                    prideduc,
                    fecult,
                    estadosol,
                    estado,
                    fechasol,
                    fechares,
                    fechaforp,
                    fechaforf,
                    observacion,
                    garantia,
                    tdocumento,
                    ndocumento,
                    tesoreria,
                    userrec,
                    userfor,
                    userres
                )
                values
                (
                    1,
                    @Codigo,
                    @Cedula,
                    @Monto,
                    @Monto,
                    999,
                    0,
                    0,
                    @Monto,
                    0,
                    0,
                    @Monto,
                    @FechaProX,
                    @GlngFechaCR,
                    'F',
                    'A',
                    @Fecha,
                    @Fecha,
                    @Fecha,
                    @Fecha,
                    @Observacion,
                    @Garantia,
                    'OT',
                    '',
                    @Fecha,
                    @Usuario,
                    @Usuario,
                    @Usuario
                );
                """;

            string fechaDescripcion =
                contexto.FechaServidor?.ToString(
                    "yyyy-MM-dd HH:mm:ss") ??
                string.Empty;

            connection.Execute(
                sql,
                new
                {
                    Codigo = codigo,
                    Cedula = cedula.Trim(),
                    Monto = montoFondo,
                    FechaProX = contexto.FechaProcesoSiguiente,
                    contexto.GlngFechaCR,
                    Fecha = contexto.FechaServidor,
                    Observacion =
                        $"FONDO SOLIDARIO CREADO EL {fechaDescripcion}",
                    Garantia = garantia,
                    Usuario = contexto.Usuario.Trim(),
                },
                transaction,
                TiempoEsperaSegundos);
        }

        private static void ProcesarPasoFnds(
            IDbConnection connection,
            IDbTransaction transaction,
            FondoSolidarioContext contexto,
            string codigo,
            FndsPasoConfig configuracion)
        {
            if (configuracion.Actualizar is null)
            {
                throw new InvalidOperationException(
                    "No se configur&oacute; la operaci&oacute;n de actualizaci&oacute;n del Fondo Solidario.");
            }

            foreach (
                (string cedula, decimal monto) in
                configuracion.Rows)
            {
                decimal montoFondo = FxFondoSolidario(
                    monto,
                    configuracion.MontoBase);

                var fondoActivo = Db_FNDS_ObtenerActivo(
                    connection,
                    transaction,
                    codigo,
                    cedula);

                if (fondoActivo is null)
                {
                    Db_FNDS_InsertarPaso(
                        connection,
                        transaction,
                        contexto,
                        codigo,
                        cedula,
                        montoFondo,
                        configuracion.Garantia);

                    continue;
                }

                if (
                    Math.Abs(
                        montoFondo -
                        fondoActivo.Value.Cuota
                    ) <= 1m)
                {
                    continue;
                }

                configuracion.Actualizar(
                    connection,
                    transaction,
                    fondoActivo.Value.IdSolicitud,
                    montoFondo);
            }
        }

        private ErrorDto EjecutarEnTransaccion(
            int codEmpresa,
            Func<
                IDbConnection,
                IDbTransaction,
                ErrorDto
            > accion)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    ErrorDto resultado = accion(
                        connection,
                        transaction);

                    if ((resultado.Code ?? 0) != 0)
                    {
                        transaction.Rollback();
                        return resultado;
                    }

                    transaction.Commit();
                    return resultado;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private static IEnumerable<(string Cedula, decimal Monto)>
            Db_FNDS_Paso_Listar(
                IDbConnection connection,
                IDbTransaction transaction,
                int codInstitucion,
                FndsPasoTipo tipo)
        {
            string sql = tipo switch
            {
                FndsPasoTipo.Paso1 => SqlFndsPaso1,
                FndsPasoTipo.Paso2 => SqlFndsPaso2,
                FndsPasoTipo.Paso3 => SqlFndsPaso3,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(tipo),
                    tipo,
                    "El paso del Fondo Solidario no es v&aacute;lido."),
            };

            return connection.Query<(string Cedula, decimal Monto)>(
                sql,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaCorte = FechaCorteFnds,
                },
                transaction: transaction,
                commandTimeout: TiempoEsperaSegundos);
        }
    }
}