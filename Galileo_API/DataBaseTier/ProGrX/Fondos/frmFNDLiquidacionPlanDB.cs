using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ProGrX.Fondos;
using System.Data;
using System.Text;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndLiquidacionPlanDB
    {
        private readonly PortalDB _portalDb;

        public FrmFndLiquidacionPlanDB(IConfiguration? config)
        {
            _portalDb = new PortalDB(config!);
        }

        /// <summary>
        /// Método que devuelve listas genéricas para Liquidación de Planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="catalogo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Listas(int CodEmpresa, string usuario, string catalogo)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = "";

                switch (catalogo.ToUpper())
                {
                    case "OPERADORA":
                        query = $@"select rtrim(descripcion) as 'descripcion',cod_operadora as 'item' from FND_Operadoras";
                        break;
                    case "RETENCION":
                        query = $@"select rtrim(RETENCION_CODIGO) as 'item' , RTRIM(DESCRIPCION) as 'descripcion' From FND_RETENCION_CONCEPTOS  Where ACTIVO = 1 ";
                        break;
                    case "ESTADOS":
                        query = $@"select rtrim(COD_ESTADO) as 'item' , RTRIM(DESCRIPCION) as 'descripcion' From AFI_ESTADOS_PERSONA  Where ACTIVO = 1";
                        break;
                    case "BANCO":
                        query = $@"select B.id_banco as 'item',
                                          rtrim(B.descripcion) as 'descripcion'
                                   from tes_banco_asg T
                                   inner join Tes_Bancos B on T.id_banco = B.id_banco
                                   where T.nombre = @usuario
                                     and B.Estado = 'A'";
                        break;

                    case "INSTITUCION":
                        query = @"select rtrim(descripcion) as 'descripcion',
                                         cod_institucion as 'item'
                                  from instituciones
                                  where Activa = 1";
                        break;
                }

                return conn.Query<DropDownListaGenericaModel>(query,new { usuario = usuario }).ToList();
            });
        }

        /// <summary>
        /// Método que obtiene los planes para liquidación de planes según operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_buscar(int CodEmpresa, int codOperadora)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = @"select cod_plan as item,descripcion from fnd_planes where Cod_operadora =  @codOperadora";
                return conn.Query<DropDownListaGenericaModel>(query, new { codOperadora }).ToList();
            });
        }

        /// <summary>
        /// Método que obtiene las operadoras para liquidación de planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = $@"select descripcion,cod_operadora as 'item' from FND_Operadoras";
                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContartos_Buscar(
            int CodEmpresa,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (filtro == null)
                return DbHelper.CreateErrorResponse<List<FndConsultaPlanRowDto>>("Los filtros son requeridos.");

            if (string.IsNullOrWhiteSpace(filtro.cod_operadora))
                return DbHelper.CreateErrorResponse<List<FndConsultaPlanRowDto>>("La operadora es requerida.");

            if (string.IsNullOrWhiteSpace(filtro.cod_plan))
                return DbHelper.CreateErrorResponse<List<FndConsultaPlanRowDto>>("El plan es requerido.");

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                if (!ShouldUseArchivoRef(filtro))
                {
                    LimpiarArchivoRef(connection);
                }

                var context = BuildConsultaPlanQuery(connection, filtro);

                var rows = connection.Query<FndConsultaPlanDbRow>(
                    context.Sql,
                    context.Parameters,
                    commandType: CommandType.Text
                ).ToList();

                var result = rows.Select(MapConsultaPlanRow).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FndConsultaPlanRowDto>>($"Error al consultar contratos: {ex.Message}");
            }
        }
        /// <summary>
        /// Contexto interno para armar query
        /// </summary>
        private sealed class ConsultaPlanQueryContext
        {
            public string Sql { get; set; } = string.Empty;
            public DynamicParameters Parameters { get; set; } = new();
        }

        /// <summary>
        /// Método que construye el SQL
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        private ConsultaPlanQueryContext BuildConsultaPlanQuery(
            SqlConnection connection,
            FndLiquidacionPlanFiltrosData filtro)
        {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();

            bool esChequeORetener = EsChequeORetener(filtro);

            if (esChequeORetener)
            {
                BuildQueryChequeORetener(sql, parameters, filtro);
            }
            else
            {
                BuildQueryDesembolso(connection, sql, parameters, filtro);
            }

            AddFiltrosComunes(sql, parameters, filtro);

            return new ConsultaPlanQueryContext
            {
                Sql = sql.ToString(),
                Parameters = parameters
            };
        }

        /// <summary>
        /// cheque o retener
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="filtro"></param>
        private static void BuildQueryChequeORetener(
            StringBuilder sql,
            DynamicParameters parameters,
            FndLiquidacionPlanFiltrosData filtro)
        {
            sql.AppendLine(@"
                    select
                        F.cod_contrato,
                        F.Cedula,
                        S.Nombre,
                        F.Estado,
                        F.Plazo,
                        F.Monto,
                        F.Aportes,
                        F.Rendimiento,
                        F.Fecha_Corte,
                        F.Fecha_Inicio,
                        '' as CuentaAhorroX,
                        @BancoX as BancoX,
                        Est.Descripcion as EstadoDesc
                    from Fnd_Contratos F
                    inner join Socios S
                        on F.Cedula = S.Cedula
                    inner join AFI_ESTADOS_PERSONA Est
                        on S.estadoActual = Est.cod_Estado
                    where F.Cod_Operadora = @CodOperadora
                      and F.Cod_Plan = @CodPlan
                      and F.Estado <> 'L'");

            parameters.Add("@CodOperadora", filtro.cod_operadora);
            parameters.Add("@CodPlan", filtro.cod_plan);
            parameters.Add("@BancoX", filtro.id_banco);
        }

        /// <summary>
        /// desembolso normal
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="filtro"></param>
        private static void BuildQueryDesembolso(
                SqlConnection connection,
                StringBuilder sql,
                DynamicParameters parameters,
                FndLiquidacionPlanFiltrosData filtro)
        {
            string grupoBancario = ObtenerGrupoBancario(connection, (int)filtro.id_banco!);

            sql.AppendLine(@"
                    select
                        F.cod_contrato,
                        F.Cedula,
                        S.Nombre,
                        F.Estado,
                        F.Plazo,
                        F.Monto,
                        F.Aportes,
                        F.Rendimiento,
                        F.Fecha_Corte,
                        F.Fecha_Inicio,
                        dbo.fxSys_Cuentas_Bancarias(F.cedula, B.id_Banco, 0) as CuentaAhorroX,
                        B.id_Banco as BancoX,
                        B.descripcion as BancoDesc,
                        Est.Descripcion as EstadoDesc
                    from Fnd_Contratos F
                    inner join Socios S
                        on F.Cedula = S.Cedula
                    inner join Fnd_Planes Pln
                        on F.cod_Operadora = Pln.Cod_Operadora
                       and F.cod_Plan = Pln.cod_Plan
                    inner join AFI_ESTADOS_PERSONA Est
                        on S.estadoActual = Est.cod_Estado
                    inner join Tes_Bancos B
                        on B.id_Banco = @BancoId");

            parameters.Add("@BancoId", filtro.id_banco);
            parameters.Add("@CodOperadora", filtro.cod_operadora);
            parameters.Add("@CodPlan", filtro.cod_plan);

            AddJoinCuentaFiltro(sql, filtro);

            sql.AppendLine(@"
where F.Cod_Operadora = @CodOperadora
  and F.Cod_Plan = @CodPlan
  and F.Estado <> 'L'
  and dbo.fxSys_Cuentas_Bancarias(F.cedula, B.id_Banco, 0) <> ''");

            if (EsInterbancariaMismoBanco(filtro))
            {
                sql.AppendLine(@"
  and substring(dbo.fxSys_Cuentas_Bancarias(F.cedula, B.id_Banco, 0), 1, 10) like @GrupoBancario");
                parameters.Add("@GrupoBancario", $"%{grupoBancario}%");
            }
        }


        /// <summary>
        /// Join por tipo de cuenta
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="filtro"></param>
        private static void AddJoinCuentaFiltro(StringBuilder sql, FndLiquidacionPlanFiltrosData filtro)
        {
            if (string.Equals(filtro.cuentaFiltro, "TODOS", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.Equals(filtro.cuentaFiltro, "Cuenta Interna", StringComparison.OrdinalIgnoreCase))
            {
                sql.AppendLine(@"
                    inner join vSys_Personas_Cuenta_Bancaria_Local Cta
                        on F.cedula = Cta.Identificacion
                       and Cta.cod_Banco = B.cod_Grupo
                       and Cta.cod_Divisa = Pln.Cod_Moneda");
                                    return;
                                }

               sql.AppendLine(@"
                    inner join vSys_Personas_Cuenta_Bancaria_Interbancaria Cta
                        on F.cedula = Cta.Identificacion
                       and Cta.cod_Banco = B.cod_Grupo
                       and Cta.cod_Divisa = Pln.Cod_Moneda");
        }

        private static void AddFiltrosComunes(
             StringBuilder sql,
            DynamicParameters parameters,
            FndLiquidacionPlanFiltrosData filtro)
        {
            AddFiltroInstitucion(sql, parameters, filtro);
            AddFiltroLinea(sql, parameters, filtro);
            AddFiltroFechas(sql, parameters, filtro);
            AddFiltroEstadoPersona(sql, parameters, filtro);
            AddFiltroMontos(sql, parameters, filtro);
            AddFiltroSinMovimiento(sql, parameters, filtro);
            AddFiltroRendimientoSinAporte(sql, filtro);
            AddFiltroMensualidad(sql, filtro);
            AddFiltroCreditos(sql, filtro);
            AddFiltroArchivo(sql, filtro);
        }

        private static void AddFiltroInstitucion(
                StringBuilder sql,
                DynamicParameters parameters,
                FndLiquidacionPlanFiltrosData filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro.cod_institucion) ||
                string.Equals(filtro.cod_institucion, "TODOS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            sql.AppendLine("and S.cod_institucion = @CodInstitucion");
            parameters.Add("@CodInstitucion", filtro.cod_institucion);
        }

        private static void AddFiltroLinea(
                    StringBuilder sql,
                    DynamicParameters parameters,
                    FndLiquidacionPlanFiltrosData filtro)
                        {
                            if (filtro.chkLineas || string.IsNullOrWhiteSpace(filtro.cod_linea))
                                return;

                            sql.AppendLine(@"
                and F.cedula in
                (
                    select cedula
                    from reg_creditos
                    where estado in ('A', 'C')
                      and codigo = @CodLinea
                )");
            parameters.Add("@CodLinea", filtro.cod_linea);
        }

        private static void AddFiltroFechas(
            StringBuilder sql,
            DynamicParameters parameters,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (filtro.chkFechas || filtro.dtpInicio == null || filtro.dtpCorte == null)
                return;

            sql.AppendLine(@"
                and F.Fecha_Inicio between @FechaInicio and @FechaCorte");

            parameters.Add("@FechaInicio", filtro.dtpInicio.Value.Date);
            parameters.Add("@FechaCorte", filtro.dtpCorte.Value.Date.AddDays(1).AddTicks(-1));
        }

        private static void AddFiltroEstadoPersona(
            StringBuilder sql,
            DynamicParameters parameters,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro.estado) ||
                string.Equals(filtro.estado, "TODOS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (filtro.chkEstadoPersonaDiferente)
            {
                sql.AppendLine("and S.EstadoActual not in @EstadoPersona");
            }
            else
            {
                sql.AppendLine("and S.EstadoActual in @EstadoPersona");
            }

            parameters.Add("@EstadoPersona", new[] { filtro.estado });
        }

        private static void AddFiltroMontos(
            StringBuilder sql,
            DynamicParameters parameters,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (filtro.chkFondosCero)
            {
                sql.AppendLine("and (F.Aportes + F.Rendimiento) = 0");
                return;
            }

            if (filtro.chkMontos)
                return;

            sql.AppendLine("and (F.Aportes + F.Rendimiento) between @MontoInicio and @MontoCorte");
            parameters.Add("@MontoInicio", filtro.txtMntInicio);
            parameters.Add("@MontoCorte", filtro.txtMntCorte);
        }

        private static void AddFiltroSinMovimiento(
            StringBuilder sql,
            DynamicParameters parameters,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (!filtro.chkContratosSinMovAportes)
                return;

            sql.AppendLine(@"
                and datediff(
                        month,
                        dbo.fxFndFechaUltAporte(F.cod_operadora, F.cod_plan, F.cod_contrato),
                        dbo.MyGetdate()
                    ) > @MesesSinMovimiento");

            parameters.Add("@MesesSinMovimiento", filtro.contratosSinMovMeses);
        }

        private static void AddFiltroRendimientoSinAporte(
            StringBuilder sql,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (!filtro.chkRndSinAporte)
                return;

            sql.AppendLine("and F.Aportes = 0 and F.Rendimiento > 0");
        }

        private static void AddFiltroMensualidad(
            StringBuilder sql,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (!filtro.chkMensualidad)
                return;

            sql.AppendLine("and F.Monto = 0 and isnull(F.Inversion, 0) = 0");
        }

        private static void AddFiltroArchivo(
            StringBuilder sql,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (!ShouldUseArchivoRef(filtro))
                return;

            sql.AppendLine("and S.cedula in (select cedula from FND_ARCHIVO_REF)");
        }

        private static void AddFiltroCreditos(
            StringBuilder sql,
            FndLiquidacionPlanFiltrosData filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro.creditos) ||
                string.Equals(filtro.creditos, "TODOS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string filtroSql = filtro.creditos switch
            {
                "Persona -> Con créditos activos" => @"
                    and S.cedula in
                    (
                        select V.cedula
                        from reg_creditos V
                        inner join Catalogo C
                            on V.codigo = C.codigo
                           and C.retencion = 'N'
                           and C.poliza = 'N'
                        where V.saldo > 0
                          and V.estado = 'A'
                        group by V.cedula
                    )",

                                    "Persona -> Con créditos en Mora" => @"
                    and S.cedula in
                    (
                        select V.cedula
                        from vista_morosidad V
                        inner join Catalogo C
                            on V.codigo = C.codigo
                           and C.retencion = 'N'
                           and C.poliza = 'N'
                        group by V.cedula
                    )",

                                    "Persona -> Sin créditos activos" => @"
                    and S.cedula not in
                    (
                        select cedula
                        from reg_creditos
                        where saldo > 0
                          and estado = 'A'
                        group by cedula
                    )",

                                    "Persona -> Sin créditos en Mora" => @"
                    and S.cedula not in
                    (
                        select V.cedula
                        from vista_morosidad V
                        inner join Catalogo C
                            on V.codigo = C.codigo
                           and C.retencion = 'N'
                           and C.poliza = 'N'
                        group by V.cedula
                    )",

                                    "Persona -> En Cobro Jud y/o Traspaso" => @"
                    and S.cedula in
                    (
                        select cedula
                        from reg_creditos
                        where saldo > 0
                          and estado = 'A'
                          and proceso <> 'N'
                        group by cedula
                    )",

                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(filtroSql))
            {
                sql.AppendLine(filtroSql);
            }
        }

        private static bool EsChequeORetener(FndLiquidacionPlanFiltrosData filtro)
        {
            return string.Equals(filtro.tipoDocumento, "Cheque", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(filtro.proceso, "Retener", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsInterbancariaMismoBanco(FndLiquidacionPlanFiltrosData filtro)
        {
            return string.Equals(filtro.cuentaFiltro, "Interbancaria Mismo Banco", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldUseArchivoRef(FndLiquidacionPlanFiltrosData filtro)
        {
            return filtro.archivo != null;
        }

        private static void LimpiarArchivoRef(SqlConnection connection)
        {
            connection.Execute("delete FND_ARCHIVO_REF");
        }

        private static string ObtenerGrupoBancario(SqlConnection connection, int bancoId)
        {
            const string sql = "select dbo.fxTes_BancoSFN(@BancoId)";
            return connection.QueryFirstOrDefault<string>(sql, new { BancoId = bancoId }) ?? string.Empty;
        }

        private FndConsultaPlanRowDto MapConsultaPlanRow(FndConsultaPlanDbRow row)
        {
            return new FndConsultaPlanRowDto
            {
                marcas = true,
                cod_contrato = row.cod_contrato,
                cedula = row.cedula ?? string.Empty,
                nombre = row.nombre ?? string.Empty,
                aportes = row.aportes,
                rendimiento = row.rendimiento,
                bancofinal = row.bancox,
                cuentafinal = row.cuentaahorrox ?? string.Empty,
                fechafinal = row.fecha_corte?.ToString("yyyy/MM/dd") ?? string.Empty,
                estadodesc = row.estadodesc ?? string.Empty
            };
        }
    }
}
