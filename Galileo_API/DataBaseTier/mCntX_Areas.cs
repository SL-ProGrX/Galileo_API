using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Extensions.Configuration;

namespace Galileo_API.DataBaseTier
{
    public class mCntX_Areas
    {
        private readonly PortalDB _portalDb;

        public mCntX_Areas(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Mayoriza los saldos temporales de un area hacia sus cuentas madre.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codArea"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> sbCntX_Areas_Mayorizar(int codEmpresa, int codContabilidad, int codArea, string usuario)
        {
            const string sqlConsulta = @"
                select R.saldo_inicial as SaldoInicial,
                       R.total_debitos as TotalDebitos,
                       R.total_creditos as TotalCreditos,
                       R.csaldo_inicial as CSaldoInicial,
                       R.ctotal_debitos as CTotalDebitos,
                       R.ctotal_creditos as CTotalCreditos,
                       rtrim(isnull(C.cuenta_madre, '')) as CuentaMadre
                from CntX_Acceso_Historico R
                inner join CntX_Cuentas C
                  on R.cod_contabilidad = C.cod_contabilidad
                 and R.cod_cuenta = C.cod_cuenta
                where R.cod_contabilidad = @CodContabilidad
                  and R.cod_area = @CodArea
                  and R.usuario = @Usuario
                order by R.cod_cuenta desc";

            const string sqlActualiza = @"
                update CntX_Acceso_Historico
                set saldo_inicial = saldo_inicial + @SaldoInicial,
                    total_debitos = total_debitos + @TotalDebitos,
                    total_creditos = total_creditos + @TotalCreditos,
                    csaldo_inicial = csaldo_inicial + @CSaldoInicial,
                    ctotal_debitos = ctotal_debitos + @CTotalDebitos,
                    ctotal_creditos = ctotal_creditos + @CTotalCreditos
                where cod_contabilidad = @CodContabilidad
                  and cod_area = @CodArea
                  and usuario = @Usuario
                  and cod_cuenta = @CuentaMadre";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var registros = conn.Query<CntXAreaMayorizarRow>(sqlConsulta, new
                {
                    CodContabilidad = codContabilidad,
                    CodArea = codArea,
                    Usuario = usuario
                });

                foreach (var registro in registros)
                {
                    conn.Execute(sqlActualiza, new
                    {
                        CodContabilidad = codContabilidad,
                        CodArea = codArea,
                        Usuario = usuario,
                        registro.CuentaMadre,
                        registro.SaldoInicial,
                        registro.TotalDebitos,
                        registro.TotalCreditos,
                        registro.CSaldoInicial,
                        registro.CTotalDebitos,
                        registro.CTotalCreditos
                    });
                }

                return true;
            });
        }

        /// <summary>
        /// Genera el balance de comprobacion temporal para un area.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <param name="codArea"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> sbCntX_Areas_Balance_Comprobacion(int codEmpresa, int codContabilidad, int anio, int mes, int codArea, string usuario)
        {
            const string sqlBorra = @"
                delete CntX_Acceso_Historico
                where cod_contabilidad = @CodContabilidad
                  and cod_area = @CodArea
                  and usuario = @Usuario";

            const string sqlInsertaMovimientos = @"
                insert into CntX_Acceso_Historico
                    (usuario, cod_contabilidad, cod_area, cod_cuenta, saldo_inicial, total_debitos,
                     total_creditos, csaldo_inicial, ctotal_debitos, ctotal_creditos)
                select @Usuario,
                       @CodContabilidad,
                       @CodArea,
                       C.cod_cuenta,
                       isnull(M.saldo_inicial, 0),
                       isnull(M.total_debitos, 0),
                       isnull(M.total_creditos, 0),
                       0,
                       0,
                       0
                from CntX_Mov_Cuentas_Detallado M
                inner join CntX_Cuentas C
                  on M.cod_contabilidad = C.cod_contabilidad
                 and M.cod_cuenta = C.cod_cuenta
                inner join CntX_Area_Cuentas A
                  on M.cod_contabilidad = A.cod_contabilidad
                 and A.cod_cuenta = M.cod_cuenta
                where A.cod_contabilidad = @CodContabilidad
                  and A.cod_area = @CodArea
                  and M.anio = @Anio
                  and M.mes = @Mes
                  and C.acepta_movimientos = 1";

            const string sqlInsertaCuentasMadre = @"
                insert into CntX_Acceso_Historico
                    (usuario, cod_contabilidad, cod_area, cod_cuenta, saldo_inicial, total_debitos,
                     total_creditos, csaldo_inicial, ctotal_debitos, ctotal_creditos)
                select @Usuario,
                       @CodContabilidad,
                       @CodArea,
                       C.cod_cuenta,
                       0,
                       0,
                       0,
                       0,
                       0,
                       0
                from CntX_Cuentas C
                inner join CntX_Area_Cuentas A
                  on C.cod_contabilidad = A.cod_contabilidad
                 and A.cod_cuenta = C.cod_cuenta
                where A.cod_contabilidad = @CodContabilidad
                  and A.cod_area = @CodArea
                  and C.acepta_movimientos = 0";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Open();
                using var transaction = conn.BeginTransaction();
                var parametros = new
                {
                    CodContabilidad = codContabilidad,
                    Anio = anio,
                    Mes = mes,
                    CodArea = codArea,
                    Usuario = usuario
                };

                try
                {
                    conn.Execute(sqlBorra, parametros, transaction);
                    conn.Execute(sqlInsertaMovimientos, parametros, transaction);
                    conn.Execute(sqlInsertaCuentasMadre, parametros, transaction);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        /// <summary>
        /// Carga los saldos comparativos del area en la tabla temporal.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <param name="codArea"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> sbCntX_Areas_Balance_Compara(int codEmpresa, int codContabilidad, int anio, int mes, int codArea, string usuario)
        {
            const string sql = @"
                update H
                set csaldo_inicial = M.saldo_inicial,
                    ctotal_debitos = M.total_debitos,
                    ctotal_creditos = M.total_creditos
                from CntX_Acceso_Historico H
                inner join (
                    select C.cod_cuenta,
                           isnull(M.saldo_inicial, 0) as saldo_inicial,
                           isnull(M.total_debitos, 0) as total_debitos,
                           isnull(M.total_creditos, 0) as total_creditos
                    from CntX_Mov_Cuentas_Detallado M
                    inner join CntX_Cuentas C
                      on M.cod_contabilidad = C.cod_contabilidad
                     and M.cod_cuenta = C.cod_cuenta
                    inner join CntX_Area_Cuentas A
                      on M.cod_contabilidad = A.cod_contabilidad
                     and A.cod_cuenta = M.cod_cuenta
                    where A.cod_contabilidad = @CodContabilidad
                      and A.cod_area = @CodArea
                      and M.anio = @Anio
                      and M.mes = @Mes
                      and C.acepta_movimientos = 1
                ) M
                  on H.cod_cuenta = M.cod_cuenta
                where H.cod_contabilidad = @CodContabilidad
                  and H.usuario = @Usuario";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new
                {
                    CodContabilidad = codContabilidad,
                    Anio = anio,
                    Mes = mes,
                    CodArea = codArea,
                    Usuario = usuario
                });
                return true;
            });
        }

        /// <summary>
        /// Calcula la utilidad mensual y acumulada del area.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codArea"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CntXAreaUtilidadDto> sbCntX_Areas_Utilidad(int codEmpresa, int codContabilidad, int codArea, string usuario)
        {
            const string sql = @"
                select
                    isnull(sum(case when T.clasificacion in ('I', 'V') then A.total_debitos + A.total_creditos else 0 end), 0)
                    - isnull(sum(case when T.clasificacion = 'G' then A.total_debitos + A.total_creditos else 0 end), 0) as AreaUTMes,
                    isnull(sum(case when T.clasificacion in ('I', 'V') then A.saldo_inicial + A.total_debitos + A.total_creditos else 0 end), 0)
                    - isnull(sum(case when T.clasificacion = 'G' then A.saldo_inicial + A.total_debitos + A.total_creditos else 0 end), 0) as AreaUTAcumulada,
                    isnull(sum(case when T.clasificacion in ('I', 'V') then A.ctotal_debitos + A.ctotal_creditos else 0 end), 0)
                    - isnull(sum(case when T.clasificacion = 'G' then A.ctotal_debitos + A.ctotal_creditos else 0 end), 0) as AreaUTCMes,
                    isnull(sum(case when T.clasificacion in ('I', 'V') then A.csaldo_inicial + A.ctotal_debitos + A.ctotal_creditos else 0 end), 0)
                    - isnull(sum(case when T.clasificacion = 'G' then A.csaldo_inicial + A.ctotal_debitos + A.ctotal_creditos else 0 end), 0) as AreaUTCAcumulada
                from CntX_Acceso_Historico A
                inner join CntX_Cuentas C
                  on A.cod_contabilidad = C.cod_contabilidad
                 and A.cod_cuenta = C.cod_cuenta
                inner join CntX_Tipos_Cuentas T
                  on A.cod_contabilidad = T.cod_contabilidad
                 and C.tipo_cuenta = T.tipo_cuenta
                where A.cod_contabilidad = @CodContabilidad
                  and A.usuario = @Usuario
                  and A.cod_area = @CodArea
                  and T.clasificacion in ('I', 'V', 'G')
                  and C.acepta_movimientos = 1";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CntXAreaUtilidadDto>(sql, new
                {
                    CodContabilidad = codContabilidad,
                    CodArea = codArea,
                    Usuario = usuario
                }) ?? new CntXAreaUtilidadDto());
        }

    }
}
