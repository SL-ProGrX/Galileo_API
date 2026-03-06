using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using System.Data.Common;
using System.Data; 
using static Galileo_API.Models.ProGrX_Procesos.FrmCcFndSolidarioModels; 

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCcFndSolidarioDB
    {
        private readonly PortalDB _portalDB;
        private readonly MProGrxMain mProGrxDll; 

        public FrmCcFndSolidarioDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FNDSolidario_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                        select 
                            cod_institucion as item,
                            rtrim(descripcion) as descripcion
                        from instituciones
                        where activa = 1
                          and cod_institucion in (1,2)
                        order by descripcion";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta la fecha del proceso anterior dado un proceso actual, si no encuentra un proceso anterior retorna el mismo proceso actual
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tx"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        private static decimal FxFechaProcesoAnterior(IDbConnection connection, IDbTransaction tx, decimal proceso)
        {
            const string sql = @"SELECT ISNULL(dbo.fxSIFPrmProcesoAnt(@Proceso), @Proceso);";
            return connection.ExecuteScalar<decimal>(sql, new { Proceso = proceso }, tx);
        }


        /// <summary>
        /// Consulta la fecha del proceso siguiente dado un proceso actual, si no encuentra un proceso siguiente retorna el mismo proceso actual
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tx"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        private static decimal FxFechaProcesoSiguiente(IDbConnection connection, IDbTransaction tx, decimal proceso)
        {
            const string sql = @"SELECT ISNULL(dbo.fxSIFPrmProcesoSig(@Proceso), @Proceso);";
            return connection.ExecuteScalar<decimal>(sql, new { Proceso = proceso }, tx);
        }

        private static DateTime FechaServidor(IDbConnection conn, IDbTransaction tx)
        {
            const string sql = @"SELECT GETDATE();";
            return conn.ExecuteScalar<DateTime>(sql, transaction: tx);
        }

        private static decimal FxFondoSolidario(decimal monto, decimal montoBase = 150m)
        {
            return (monto / 1000000m) * montoBase;
        }
        private enum FndsPasoTipo { Paso1, Paso2, Paso3 }

        public ErrorDto FrmCC_FNDSolidario_Ejecutar(
            int codEmpresa,
            string usuario,
            int codContabilidad,
            int codInstitucion)
        {

            var globales = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad)?.Result
                       ?? new Globales();

            if (globales.GlngFechaCR <= 0)
            {
                return DbHelper.ErrorResponse("No fue posible obtener el período (GlngFechaCR) para ejecutar el proceso.");
            }

            return globales.SysASEVersion
                ? FrmCC_FNDSolidario_Ejecutar_FNDS(codEmpresa, usuario, codInstitucion, globales)
                : FrmCC_FNDSolidario_Ejecutar_FBEN(codEmpresa, usuario, codContabilidad);
        }

        //sbFondoSolidario
        private ErrorDto FrmCC_FNDSolidario_Ejecutar_FNDS(int codEmpresa, string usuario, int codInstitucion, Globales globalesLocal)
        {
            return EjecutarEnTransaccion(codEmpresa, (connection, tx) =>
            {
               
                var fechaServidor = FechaServidor(connection, tx);
                var fechaProX = FxFechaProcesoSiguiente(connection, tx, globalesLocal.GlngFechaCR);

            
                const string codigo = "FNDS";

                Db_FNDS_CancelarSinCobertura(connection, tx, codInstitucion, codigo);
                Db_FNDS_InicializarCuotas(connection, tx, codInstitucion, codigo);

                var ctx = new FondoSolidarioContext
                {
                    CodEmpresa = codEmpresa,
                    CodInstitucion = codInstitucion,
                    Usuario = usuario,
                    GlngFechaCR = globalesLocal.GlngFechaCR,
                    FechaProcesoSiguiente = fechaProX,
                    FechaServidor = fechaServidor
                };


                var pasos = new[]
                              {
                                new FndsPasoConfig
                                {
                                    Rows = Db_FNDS_Paso_Listar(connection, tx, codInstitucion, FndsPasoTipo.Paso1),
                                    MontoBase = 150m,
                                    Garantia = "S",
                                    Actualizar = (conn, tx, id, monto) =>
                                        Db_FNDS_ActualizarPaso(conn, tx, id, monto, FndsUpdateTipo.Reemplazar)
                                },
                                new FndsPasoConfig
                                {
                                    Rows = Db_FNDS_Paso_Listar(connection, tx, codInstitucion, FndsPasoTipo.Paso2),
                                    MontoBase = 300m,
                                    Garantia = "Z",
                                    Actualizar = (conn, tx, id, monto) =>
                                        Db_FNDS_ActualizarPaso(conn, tx, id, monto, FndsUpdateTipo.SumarConSaldoMes)
                                },
                                new FndsPasoConfig
                                {
                                    Rows = Db_FNDS_Paso_Listar(connection, tx, codInstitucion, FndsPasoTipo.Paso3),
                                    MontoBase = 300m,
                                    Garantia = "Z",
                                    Actualizar = (conn, tx, id, monto) =>
                                        Db_FNDS_ActualizarPaso(conn, tx, id, monto, FndsUpdateTipo.SumarSinSaldoMes)
                                }
                            };

                foreach (var paso in pasos)
                    ProcesarPasoFnds(connection, tx, ctx, codigo, paso);


                // 6) Cancela por congelamiento (al final)
                Db_FNDS_CancelarPorCongelamiento(connection, tx, codInstitucion, codigo); 

                return DbHelper.OkResponse("Fondo Solidario Actualizado Satisfactoriamente...");
            });
        }
       
        
        private ErrorDto FrmCC_FNDSolidario_Ejecutar_FBEN(
            int codEmpresa,
            string usuario,
            int codContabilidad)
        {
            return EjecutarEnTransaccion(codEmpresa, (connection, tx) =>
            {
                
                var globalesDto = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad);
                var glngFechaCR = globalesDto?.Result?.GlngFechaCR ?? 0;

                if (glngFechaCR <= 0)
                {                    
                    return DbHelper.ErrorResponse("No fue posible obtener el período (GlngFechaCR) para ejecutar FBEN.");

                }
                 
                var fechaServidor = FechaServidor(connection, tx);

                var fechaProcesoSiguiente = FxFechaProcesoSiguiente(connection, tx, glngFechaCR);
                var fechaProcesoAnterior = FxFechaProcesoAnterior(connection, tx, glngFechaCR);
                
                const string codigo = "FBEN";
                const decimal monto = 800m;

                // 1) Excluyendo cuotas ex-socios
                Db_FBEN_ExcluirExSocios(connection, tx, codigo);

                // 2) Actualiza casos actuales
                Db_FBEN_ActualizarCasosActuales(connection, tx, codigo, monto);

                // 3) Procesando casos nuevos (insert)
                var ctx = new FondoSolidarioContext
                {
                    CodEmpresa = codEmpresa,
                    Usuario = usuario,
                    GlngFechaCR = glngFechaCR,
                    FechaProcesoSiguiente = fechaProcesoSiguiente,
                    FechaProcesoAnterior = fechaProcesoAnterior,
                    FechaServidor = fechaServidor
                };

                Db_FBEN_InsertarCasosNuevos(
                    connection, tx, ctx,
                    codigo, monto);

                // 4) Cancela sin aportes > 2 meses
                Db_FBEN_CancelarSinAporteMas2Meses(connection, tx, codigo);

                return DbHelper.OkResponse("Fondo de Beneficio Socual Actualizado Satisfactoriamente...");
            });
        }
        private static void Db_FBEN_ExcluirExSocios(IDbConnection conn, IDbTransaction tx, string codigo)
        {
            const string sql = @"
        update reg_creditos
        set estado = 'C', saldo = 0, cuota = 0
        where estado = 'A'
          and codigo = @Codigo
          and cedula in (select cedula from socios where estadoactual <> 'S')";

            conn.Execute(sql, new { Codigo = codigo }, transaction: tx);
        }

        private static void Db_FBEN_ActualizarCasosActuales(IDbConnection conn, IDbTransaction tx, string codigo, decimal monto)
        {
            const string sql = @"
        update reg_creditos
        set montoapr = @Monto,
            cuota = @Monto,
            saldo = @Monto
        where estado = 'A'
          and codigo = @Codigo
          and cedula in (select cedula from socios where estadoactual = 'S')";

            conn.Execute(sql, new { Codigo = codigo, Monto = monto }, transaction: tx);
        }
        private static void Db_FBEN_InsertarCasosNuevos(
            IDbConnection conn,
            IDbTransaction tx,
            FondoSolidarioContext ctx,
            string codigo,
            decimal monto)
        {
            const string sql = @"
        insert into reg_creditos
        (codigo,id_comite,cedula,montosol,montoapr,monto_girado,
         saldo,amortiza,interesc,saldo_mes,cuota,int,interesv,plazo,userrec,userres,
         userfor,usertesoreria,tesoreria,fechasol,fechares,fechaforp,fechaforf,
         fecha_calculo_int,garantia,primer_cuota,tdocumento,ndocumento,pagare,
         firma_deudor,premio,observacion,estado,prideduc,fecult,estadosol,documento_referido)
        select
         @Codigo,6,cedula,@Monto,@Monto,0,@Monto,0,0,@Monto,@Monto,0,0,999,
         @Usuario,@Usuario,@Usuario,@Usuario,
         @Fecha,@Fecha,@Fecha,@Fecha,@Fecha,@Fecha,
         'N','N','OT','',0,1,0,
         'Proceso Automatico Cuota Mantenimiento CR',
         'A',@FechaProcesoSiguiente,@FechaProcesoAnterior,'F','AUTOMATICO'
        from socios
        where estadoactual = 'S'
          and cedula not in(
                select cedula from reg_creditos
                where estado = 'A' and codigo = @Codigo
          )";

            conn.Execute(sql, new
            {
                Codigo = codigo.ToUpperInvariant(),
                Monto = monto,
                Usuario = ctx.Usuario.Trim(),
                Fecha = ctx.FechaServidor,
                ctx.FechaProcesoSiguiente,
                ctx.FechaProcesoAnterior
            }, transaction: tx);
        }

        private static void Db_FBEN_CancelarSinAporteMas2Meses(IDbConnection conn, IDbTransaction tx, string codigo)
        {
            const string sql = @"
        update reg_creditos
        set estado = 'C', saldo = 0
        where estado = 'A'
          and codigo = @Codigo
          and cedula in (
                select A.cedula
                from ahorro_consolidado A
                inner join socios S on A.cedula = S.cedula
                where S.estadoactual = 'S'
                  and datediff(month, A.fecAporte, dbo.MyGetdate()) > 2
          )";

            conn.Execute(sql, new { Codigo = codigo }, transaction: tx);
        }

        /*sbFondoSolidario (FNDS)*/
        private static void Db_FNDS_CancelarSinCobertura(IDbConnection conn, IDbTransaction tx, int codInstitucion, string codigo)
        {
            const string sql = @"
        update R set Estado = 'C'
        from reg_creditos R
        inner join catalogo C on R.codigo = C.codigo
        inner join Socios S on R.cedula = S.cedula
        where S.cod_institucion = @CodInstitucion
          and R.codigo = @Codigo
          and R.estado = 'A'
          and R.cedula not in(
                select Reg.Cedula
                from reg_creditos Reg
                inner join catalogo Cat on Reg.codigo = Cat.codigo
                where Cat.retencion = 'N'
                  and Cat.poliza = 'N'
                  and Cat.cobertura = 1
                  and Reg.garantia not in('H')
                  and Reg.saldo > 0
                  and Reg.estado = 'A'
                  and Reg.proceso <> 'J'
                group by Reg.cedula
          )";

            conn.Execute(sql, new { CodInstitucion = codInstitucion, Codigo = codigo }, transaction: tx);
        }

        private static void Db_FNDS_InicializarCuotas(IDbConnection conn, IDbTransaction tx, int codInstitucion, string codigo)
        {
            const string sql = @"
        update R
        set cuota = 0,
            saldo = 0,
            montoapr = 0,
            saldo_mes = 0
        from reg_creditos R
        inner join Socios S on R.cedula = S.cedula
        where R.estado = 'A'
          and R.codigo = @Codigo
          and S.cod_institucion = @CodInstitucion";

            conn.Execute(sql, new { Codigo = codigo, CodInstitucion = codInstitucion }, transaction: tx);
        }

        private enum FndsUpdateTipo
        {
            Reemplazar,
            SumarConSaldoMes,
            SumarSinSaldoMes
        }

        private static void Db_FNDS_ActualizarPaso(
            IDbConnection conn,
            IDbTransaction tx,
            int idSolicitud,
            decimal vFnd,
            FndsUpdateTipo tipo)
        {
            var sql = tipo switch
            {
                FndsUpdateTipo.Reemplazar => @"
            update reg_creditos
            set cuota = @Monto,
                saldo = @Monto,
                montoapr = @Monto,
                saldo_mes = @Monto
            where id_solicitud = @IdSolicitud",

                FndsUpdateTipo.SumarConSaldoMes => @"
            update reg_creditos
            set cuota = cuota + @Monto,
                saldo = saldo + @Monto,
                saldo_mes = saldo_mes + @Monto,
                montoapr = montoapr + @Monto
            where id_solicitud = @IdSolicitud",

                _ => @"
            update reg_creditos
            set cuota = cuota + @Monto,
                saldo = saldo + @Monto,
                montoapr = montoapr + @Monto
            where id_solicitud = @IdSolicitud"
            };

            conn.Execute(sql, new { Monto = vFnd, IdSolicitud = idSolicitud }, transaction: tx);
        }

        private static (int IdSolicitud, decimal Cuota)? Db_FNDS_ObtenerActivo(IDbConnection conn, IDbTransaction tx, string codigo, string cedula)
        {
            const string sql = @"
        select top 1 id_solicitud as IdSolicitud, cuota as Cuota
        from reg_creditos
        where codigo = @Codigo
          and cedula = @Cedula
          and estado = 'A'";

            var r = conn.QueryFirstOrDefault<(int IdSolicitud, decimal Cuota)>(
                sql, new { Codigo = codigo, Cedula = cedula }, transaction: tx);

            return r.IdSolicitud <= 0 ? null : r;
        }

        private static void Db_FNDS_CancelarPorCongelamiento(IDbConnection conn, IDbTransaction tx, int codInstitucion, string codigo)
        {
            const string sql = @"
        update R set estado = 'C'
        from reg_creditos R
        inner join Socios S on R.cedula = S.cedula
        where R.estado = 'A'
          and R.codigo = @Codigo
          and S.cod_institucion = @CodInstitucion
          and R.cedula in (
                select cedula
                from afi_congelar
                where estado = 'A'
                  and fecha_finaliza >= dbo.MyGetdate()
                  and per_cobro_fndSol = 0
          )";

            conn.Execute(sql, new { Codigo = codigo, CodInstitucion = codInstitucion }, transaction: tx);
        }
        private static void Db_FNDS_InsertarPaso(
             IDbConnection conn,
            IDbTransaction tx,
            FondoSolidarioContext ctx,
            string codigo,
            string cedula,
            decimal vFnd,
            string garantia)
        {
            const string sql = @"
        insert reg_creditos
        (id_comite,codigo,cedula,montosol,montoapr,plazo,int,interesv,
         saldo,interesc,amortiza,cuota,prideduc,fecult,estadosol,estado,
         fechasol,fechares,fechaforp,fechaforf,observacion,garantia,
         tdocumento,ndocumento,tesoreria,userrec,userfor,userres)
        values
        (1,@Codigo,@Cedula,@Monto,@Monto,999,0,0,
         @Monto,0,0,@Monto,@FechaProX,@GlngFechaCR,'F','A',
         @Fecha,@Fecha,@Fecha,@Fecha,
         @Obs,@Garantia,'OT','',@Fecha,
         @Usuario,@Usuario,@Usuario)";

            conn.Execute(sql, new
            {
                Codigo = codigo,
                Cedula = cedula.Trim(),
                Monto = vFnd,
                FechaProX = ctx.FechaProcesoSiguiente,   // o ctx.FechaProcesoSiguiente si aplica igual que vFechaProX
                ctx.GlngFechaCR,
                Fecha = ctx.FechaServidor,
                Obs = $"FONDO SOLIDARIO CREADO EL {ctx.FechaServidor:yyyy-MM-dd HH:mm:ss}",
                Garantia = garantia,
                Usuario = ctx.Usuario.Trim()
            }, transaction: tx);
        }

        private static void ProcesarPasoFnds(
                IDbConnection connection,
                IDbTransaction tx,
                FondoSolidarioContext ctx,
                string codigo,
                FndsPasoConfig config)
        {
            foreach (var (cedula, monto) in config.Rows)
            {
                var vFnd = FxFondoSolidario(monto, config.MontoBase);

                var activo = Db_FNDS_ObtenerActivo(connection, tx, codigo, cedula);

                if (activo is null)
                {
                    Db_FNDS_InsertarPaso(connection, tx, ctx, codigo, cedula, vFnd, config.Garantia);
                }
                else
                {
                    if (Math.Abs(vFnd - activo.Value.Cuota) > 1m)
                        config.Actualizar?.Invoke(connection, tx, activo.Value.IdSolicitud, vFnd);
                }
            }
        }

        private ErrorDto EjecutarEnTransaccion(int codEmpresa, Func<IDbConnection, IDbTransaction, ErrorDto> action)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            connection.Open();

            using var tx = connection.BeginTransaction();
            try
            {
                var result = action(connection, tx);
                if ((result.Code ?? 0) != 0)
                {
                    tx.Rollback();
                    return result;
                }

                tx.Commit();
                return result;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static IEnumerable<(string Cedula, decimal Monto)> Db_FNDS_Paso_Listar(
    IDbConnection conn,
    IDbTransaction tx,
    int codInstitucion,
    FndsPasoTipo tipo)
        {

            var garantiaFiltro = tipo switch
            {
                FndsPasoTipo.Paso1 => "R.garantia in ('A','N')",
                FndsPasoTipo.Paso2 => "R.garantia in ('F','X')",
                _ => "R.garantia not in ('H')"
            };

            var fechaFiltro = tipo switch
            {
                FndsPasoTipo.Paso1 or FndsPasoTipo.Paso2 => "R.fechaforp < @FechaCorte",
                _ => "R.fechaforp >= @FechaCorte"
            };
            var sql = $@"
                select R.cedula as Cedula, sum(R.montoapr) as Monto
                from reg_creditos R
                inner join Catalogo C on R.codigo = C.codigo
                inner join Socios S on R.cedula = S.cedula
                where C.retencion = 'N'
                  and C.poliza = 'N'
                  and C.cobertura = 1
                  and {garantiaFiltro}
                  and R.saldo > 0
                  and R.estado = 'A'
                  and R.proceso <> 'J'
                  and {fechaFiltro}
                  and S.cod_institucion = @CodInstitucion
                group by R.cedula";


               return conn.Query<(string Cedula, decimal Monto)>(
                    sql,
                    new { CodInstitucion = codInstitucion, FechaCorte = FechaCorteFnds },
                    transaction: tx);
        }
        private static readonly DateTime FechaCorteFnds =
    new(2004, 6, 1, 0, 0, 0, DateTimeKind.Unspecified);
    }
}
