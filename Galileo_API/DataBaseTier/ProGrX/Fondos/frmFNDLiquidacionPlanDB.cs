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
        private readonly MProGrxMain mProGrx;


        public FrmFndLiquidacionPlanDB(IConfiguration? config)
        {
            _portalDb = new PortalDB(config!);
            mProGrx = new MProGrxMain(config!);
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
        /// Metodo que obtiene el siguiente o anterior plan para liquidación de planes según operadora, plan actual y dirección del scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlanActual"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> FND_LiquidacionPlan_Plan_Scroll_Obtener(
            int CodEmpresa,
            int codOperadora,
            string? codPlanActual,
            int scrollCode)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        select top 1
                            cod_plan as item,
                            descripcion
                        from fnd_planes
                        where cod_operadora = @codOperadora
                          and (
                                @codPlanActual is null
                                or @codPlanActual = ''
                                or (@scrollCode = 1 and cod_plan > @codPlanActual)
                                or (@scrollCode <> 1 and cod_plan < @codPlanActual)
                              )
                        order by
                            case when @scrollCode = 1 then cod_plan end asc,
                            case when @scrollCode <> 1 then cod_plan end desc";

                return conn.QueryFirstOrDefault<DropDownListaGenericaModel>(query, new
                {
                    codOperadora,
                    codPlanActual,
                    scrollCode
                }) ?? new DropDownListaGenericaModel();
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


        #region Buscar Plan Contratos

        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContratos_Buscar(
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
            if (string.Equals(filtro.cuentaFiltro, FndLiquidaPlanConst.vTodos, StringComparison.OrdinalIgnoreCase))
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
                string.Equals(filtro.cod_institucion, FndLiquidaPlanConst.vTodos, StringComparison.OrdinalIgnoreCase))
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
                string.Equals(filtro.estado, FndLiquidaPlanConst.vTodos, StringComparison.OrdinalIgnoreCase))
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
                string.Equals(filtro.creditos, FndLiquidaPlanConst.vTodos, StringComparison.OrdinalIgnoreCase))
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
            return string.Equals(filtro.tipoDocumento, "CK", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(filtro.proceso, "R", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsInterbancariaMismoBanco(FndLiquidacionPlanFiltrosData filtro)
        {
            return string.Equals(filtro.cuentaFiltro, "Interbancaria Mismo Banco", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldUseArchivoRef(FndLiquidacionPlanFiltrosData filtro)
        {
            return filtro.archivo != null && filtro.archivo != "";
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
                fecha_corte = row.fecha_corte,
                fecha_inicio = row.fecha_inicio,
                estadodesc = row.estadodesc ?? string.Empty
            };
        }

        #endregion

        /// <summary>
        /// Metodo que obtiene los catalogos para liquidación de planes, se le pasa el nombre del catalogo a consultar y devuelve la lista genérica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Catalogo_Buscar(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = @"select codigo as item, descripcion from catalogo";
                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        #region Liquidar Plan

        public ErrorDto<FndLiquidacionPlanLiquidarResult> FND_LiquidacionPlan_Liquidar(
            int codEmpresa,
            FndLiquidacionPlanLiquidarRequest request)
        {

            var globales = mProGrx.sbSifParametrosInicializa(codEmpresa, request.usuario, request.codContabilidad).Result;

            request.oficinaTitular = globales.GOficinaTitular;
            request.oficinaUnidad = globales.GOficinaUnidad;
            request.oficinaCentroCosto = globales.GOficinaCentroCosto;

            var mensajeValidacion = ValidarLiquidacion(request);
            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
                return DbHelper.CreateErrorResponse<FndLiquidacionPlanLiquidarResult>(mensajeValidacion);

            if (request.contratos.Count == 0)
                return DbHelper.CreateErrorResponse<FndLiquidacionPlanLiquidarResult>("Debe seleccionar al menos un contrato.");

            SqlTransaction? tx = null;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                tx = conn.BeginTransaction();

                int codOperadora = int.Parse(request.cod_operadora);
                var plan = ObtenerPlanInfo(conn, tx, codOperadora, request.cod_plan);
                var operadora = ObtenerOperadoraInfo(conn, tx, codOperadora);

                if (string.IsNullOrWhiteSpace(plan.cod_moneda))
                    throw new InvalidOperationException("No se encontró la configuración del plan.");

                string cuentaLiquidacion = EsRetener(request.proceso)
                    ? ObtenerCuentaRetencion(conn, tx, request.retencionCodigo)
                    : operadora.cta_retiros;

                if (string.IsNullOrWhiteSpace(cuentaLiquidacion))
                    throw new InvalidOperationException("No se encontró la cuenta de liquidación.");

                var correlativo = ObtenerDocumentoReferencia(conn, tx, codOperadora, request.cod_plan);
                string docRef = $"{request.cod_plan.Trim()}.{correlativo.consecutivo:000}_{correlativo.fecha:yyyy.MM.dd}";
                string procesoCodigo = ObtenerCodigoProceso(request.proceso);
                string tipoLiquidacion = ObtenerCodigoTipo(request.tipo);
                string bancoTipo = MFndFuncionesDb.fxTipoDocumento(request.tipoDocumento);
                string tipoDoc = "FLIQ";
                string concepto = "FND006";
                string notas = LimitarTexto(request.notas, 1000);

                foreach (var contrato in request.contratos)
                {
                    EjecutarLiquidacionComplementaria(
                        conn,
                        tx,
                        new
                        {
                            Operadora = codOperadora,
                            Plan = request.cod_plan.Trim(),
                            Contrato = contrato.cod_contrato,
                            Tipo = "L",
                            TipoDoc = tipoDoc,
                            Concepto = concepto,
                            DocRef = docRef,
                            AporteLiq = contrato.aportes,
                            RendiLiq = contrato.rendimiento,
                            Multa = request.multa,
                            Notas = notas,
                            Usuario = request.usuario.Trim(),
                            OficinaTitular = request.oficinaTitular.Trim(),
                            ProcesoCodigo = procesoCodigo,
                            RetencionCodigo = request.retencionCodigo ?? string.Empty,
                            CuentaLiquidacion = cuentaLiquidacion,
                            Banco = ParseBanco(contrato.bancofinal),
                            BancoTipo = bancoTipo,
                            CuentaAhorros = contrato.cuentafinal ?? string.Empty,
                            Origen = "ProGrX",
                            TipoLiquidacion = tipoLiquidacion,
                            FechaVence = request.fechaVence?.Date ?? correlativo.fecha.Date
                        });
                }

                CrearDocumentoGeneral(
                    conn,
                    tx,
                    codOperadora,
                    request,
                    plan,
                    operadora,
                    docRef,
                    tipoDoc,
                    concepto
                    );

                tx.Commit();

                return DbHelper.CreateOkResponse(new FndLiquidacionPlanLiquidarResult
                {
                    documentoReferencia = docRef,
                    fecha = correlativo.fecha,
                    contratosProcesados = request.contratos.Count,
                    totalAportes = request.contratos.Sum(x => x.aportes),
                    totalRendimientos = request.contratos.Sum(x => x.rendimiento),
                    totalGeneral = request.contratos.Sum(x => x.aportes + x.rendimiento)
                });
            }
            catch (Exception ex)
            {
                tx?.Rollback();
                return DbHelper.CreateErrorResponse<FndLiquidacionPlanLiquidarResult>($"Error al liquidar el plan: {ex.Message}");
            }
        }

        private static string? ValidarLiquidacion(FndLiquidacionPlanLiquidarRequest request)
        {
            if (request == null) return "La solicitud es requerida.";
            if (string.IsNullOrWhiteSpace(request.cod_operadora)) return "La operadora es requerida.";
            if (string.IsNullOrWhiteSpace(request.cod_plan)) return "El plan es requerido.";
            if (string.IsNullOrWhiteSpace(request.usuario)) return "El usuario es requerido.";
            if (string.IsNullOrWhiteSpace(request.oficinaTitular)) return "La oficina titular es requerida.";
           // if (string.IsNullOrWhiteSpace(request.oficinaUnidad)) return "La oficina unidad es requerida.";
            if (EsRetener(request.proceso) && string.IsNullOrWhiteSpace(request.retencionCodigo)) return "La retención es requerida.";
            return null;
        }

        private static bool EsRetener(string? proceso) =>
            ObtenerCodigoProceso(proceso) == "R";

        private static string ObtenerCodigoProceso(string? proceso) =>
            !string.IsNullOrWhiteSpace(proceso) && proceso.Trim().StartsWith("R", StringComparison.OrdinalIgnoreCase) ? "R" : "D";

        private static string ObtenerCodigoTipo(string? tipo) =>
            !string.IsNullOrWhiteSpace(tipo) && tipo.Trim().StartsWith("R", StringComparison.OrdinalIgnoreCase) ? "R" : "L";

        private static int ParseBanco(string? banco) =>
            int.TryParse(banco, out var value) ? value : 0;

        private static string LimitarTexto(string? valor, int longitud) =>
            string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim()[..Math.Min(valor.Trim().Length, longitud)];

        private static FndLiquidacionPlanInfoData ObtenerPlanInfo(SqlConnection conn, SqlTransaction tx, int codOperadora, string codPlan)
        {
            const string sql = @"
        select
            rtrim(descripcion) as descripcion,
            rtrim(cod_moneda) as cod_moneda,
            rtrim(cuenta_conta) as cuenta_conta,
            rtrim(cuenta_rendimiento) as cuenta_rendimiento,
            rtrim(cuenta_impuestos) as cuenta_impuestos
        from fnd_planes
        where cod_operadora = @codOperadora
          and cod_plan = @codPlan";

            return conn.QueryFirstOrDefault<FndLiquidacionPlanInfoData>(sql, new { codOperadora, codPlan }, tx)
                ?? new FndLiquidacionPlanInfoData();
        }

        private static FndLiquidacionPlanOperadoraData ObtenerOperadoraInfo(SqlConnection conn, SqlTransaction tx, int codOperadora)
        {
            const string sql = @"
        select
            rtrim(cta_retiros) as cta_retiros,
            rtrim(cta_ingresos) as cta_ingresos
        from fnd_operadoras
        where cod_operadora = @codOperadora";

            return conn.QueryFirstOrDefault<FndLiquidacionPlanOperadoraData>(sql, new { codOperadora }, tx)
                ?? new FndLiquidacionPlanOperadoraData();
        }

        private static string ObtenerCuentaRetencion(SqlConnection conn, SqlTransaction tx, string? retencionCodigo)
        {
            const string sql = @"
        select rtrim(cod_cuenta)
        from fnd_retencion_conceptos
        where retencion_codigo = @retencionCodigo";

            return conn.QueryFirstOrDefault<string>(sql, new { retencionCodigo }, tx) ?? string.Empty;
        }

        private static FndLiquidacionPlanDocumentoRefData ObtenerDocumentoReferencia(SqlConnection conn, SqlTransaction tx, int codOperadora, string codPlan)
        {
            const string sql = "exec spFndPlanIdLiqGen @codOperadora, @codPlan";
            return conn.QueryFirstOrDefault<FndLiquidacionPlanDocumentoRefData>(sql, new { codOperadora, codPlan }, tx)
                ?? throw new InvalidOperationException("No fue posible obtener el consecutivo de liquidación.");
        }

        private static void EjecutarLiquidacionComplementaria(SqlConnection conn, SqlTransaction tx, object parameters)
        {
            const string sql = @"
        exec spFndRetLiq_Masivo_Complemento
            @Operadora,
            @Plan,
            @Contrato,
            @Tipo,
            @TipoDoc,
            @Concepto,
            @DocRef,
            @AporteLiq,
            @RendiLiq,
            @Multa,
            @Notas,
            @Usuario,
            @OficinaTitular,
            @ProcesoCodigo,
            @RetencionCodigo,
            @CuentaLiquidacion,
            @Banco,
            @BancoTipo,
            @CuentaAhorros,
            @Origen,
            @TipoLiquidacion,
            @FechaVence";

            conn.Execute(sql, parameters, tx);
        }

        private static void CrearDocumentoGeneral(
            SqlConnection conn,
            SqlTransaction tx,
            int codOperadora,
            FndLiquidacionPlanLiquidarRequest request,
            FndLiquidacionPlanInfoData plan,
            FndLiquidacionPlanOperadoraData operadora,
            string docRef,
            string tipoDoc,
            string concepto
           )
        {
            var resumen = ObtenerResumenDocumento(conn, tx, codOperadora, request.cod_plan, docRef);
            string detalleAsiento = LimitarTexto($"Liquidacion general {request.cod_plan}", 30);

            foreach (var item in resumen)
            {
                InsertarDocumentoMaestro(conn, tx, request, plan, item, docRef, tipoDoc, concepto);

                EjecutarAsiento(conn, tx, tipoDoc, docRef, item.aporte, "D", plan.cod_moneda, request.enlace,
                    request.oficinaUnidad, string.Empty, item.cuenta_conta, item.cod_operadora, item.cod_plan, detalleAsiento);

                EjecutarAsiento(conn, tx, tipoDoc, docRef, item.rendimiento, "D", plan.cod_moneda, request.enlace,
                    request.oficinaUnidad, request.oficinaCentroCosto, item.cuenta_rendimiento, item.cod_operadora, item.cod_plan, detalleAsiento);

                EjecutarAsiento(conn, tx, tipoDoc, docRef, item.isr_monto, "C", plan.cod_moneda, request.enlace,
                    request.oficinaUnidad, request.oficinaCentroCosto, item.isr_cta, item.cod_operadora, item.cod_plan, detalleAsiento);

                EjecutarAsiento(conn, tx, tipoDoc, docRef, item.multa, "C", plan.cod_moneda, request.enlace,
                    request.oficinaUnidad, request.oficinaCentroCosto, operadora.cta_ingresos, item.cod_operadora, item.cod_plan, detalleAsiento);

                decimal neto = item.aporte + item.rendimiento - (item.multa + item.isr_monto);
                EjecutarAsiento(conn, tx, tipoDoc, docRef, neto, "C", plan.cod_moneda, request.enlace,
                    request.oficinaUnidad, string.Empty, item.cod_cuenta, item.cod_operadora, item.cod_plan, detalleAsiento);
            }
        }

        private static List<FndLiquidacionPlanDocumentoResumenData> ObtenerResumenDocumento(
            SqlConnection conn,
            SqlTransaction tx,
            int codOperadora,
            string codPlan,
            string docRef)
        {
            const string sql = @"
        select
            p.cod_operadora,
            p.cod_plan,
            p.cuenta_conta,
            p.cuenta_rendimiento,
            l.cod_cuenta,
            p.cuenta_impuestos as isr_cta,
            max(l.cod_contrato) as cod_contrato,
            isnull(sum(l.aportes_liq), 0) as aporte,
            isnull(sum(l.rendi_liq), 0) as rendimiento,
            isnull(sum(l.multa_retiro), 0) as multa,
            isnull(sum(l.isr_monto), 0) as isr_monto
        from fnd_liquidacion l
        inner join fnd_planes p
            on l.cod_operadora = p.cod_operadora
           and l.cod_plan = p.cod_plan
        where l.cod_operadora = @codOperadora
          and l.cod_plan = @codPlan
          and l.liq_plan = @docRef
        group by
            p.cod_operadora,
            p.cod_plan,
            p.cuenta_conta,
            p.cuenta_rendimiento,
            l.cod_cuenta,
            p.cuenta_impuestos";

            return conn.Query<FndLiquidacionPlanDocumentoResumenData>(sql, new { codOperadora, codPlan, docRef }, tx).ToList();
        }

        private static void InsertarDocumentoMaestro(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanLiquidarRequest request,
            FndLiquidacionPlanInfoData plan,
            FndLiquidacionPlanDocumentoResumenData item,
            string docRef,
            string tipoDoc,
            string concepto)
        {
            const string sql = @"
        insert into SIF_TRANSACCIONES
        (
            COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
            Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
            Referencia_01, Referencia_02, cod_oficina,
            linea1, linea2, linea3, linea4, linea5, linea6, linea7,
            detalle, documento
        )
        values
        (
            @DocRef, @TipoDoc, dbo.MyGetdate(), @Usuario,
            @ClienteIdentificacion, @ClienteNombre, @Concepto, @Monto, 'P',
            @Referencia01, @Referencia02, @CodOficina,
            @Linea1, @Linea2, @Linea3, @Linea4, @Linea5, @Linea6, @Linea7,
            @Detalle, @Documento
        )";

            decimal total = item.aporte + item.rendimiento;
            var fecha = DateTime.Now;
            conn.Execute(sql, new
            {
                DocRef = docRef,
                TipoDoc = tipoDoc,
                Usuario = request.usuario.Trim(),
                ClienteIdentificacion = request.cod_plan.Trim(),
                ClienteNombre = plan.descripcion,
                Concepto = concepto,
                Monto = total,
                Referencia01 = item.cod_operadora,
                Referencia02 = item.cod_plan,
                CodOficina = request.oficinaTitular.Trim(),
                Linea1 = "Aplica Liquidacion General",
                Linea2 = $"Aplicado por..:{request.usuario.Trim()}",
                Linea3 = $"El dia        :{fecha:dd/MM/yyyy}",
                Linea4 = string.Empty,
                Linea5 = $"Aportes Liq.:{item.aporte:Standard}",
                Linea6 = $"Rendim. Liq.:{item.rendimiento:Standard}",
                Linea7 = $"Total.  Liq.:{total:Standard}",
                Detalle = LimitarTexto(request.notas, 1000),
                Documento = docRef
            }, tx);
        }

        private static void EjecutarAsiento(
            SqlConnection conn,
            SqlTransaction tx,
            string tipoDocumento,
            string numDocumento,
            decimal monto,
            string debeHaber,
            string codDivisa,
            int enlace,
            string codUnidad,
            string codCentroCosto,
            string codCuenta,
            string referencia1,
            string referencia2,
            string detalle)
        {
            if (monto <= 0 || string.IsNullOrWhiteSpace(codCuenta))
                return;

            const string sql = @"
        exec spSIFDocsAsiento
            @TipoDocumento,
            @NumDocumento,
            @Monto,
            @DebeHaber,
            @CodDivisa,
            @Factor,
            @Enlace,
            @CodUnidad,
            @CodCentroCosto,
            @CodCuenta,
            @Referencia1,
            @Referencia2,
            @Detalle";

            conn.Execute(sql, new
            {
                TipoDocumento = tipoDocumento,
                NumDocumento = numDocumento,
                Monto = monto,
                DebeHaber = debeHaber,
                CodDivisa = codDivisa,
                Factor = 1,
                Enlace = enlace,
                CodUnidad = codUnidad,
                CodCentroCosto = codCentroCosto,
                CodCuenta = codCuenta,
                Referencia1 = referencia1,
                Referencia2 = referencia2,
                Detalle = detalle
            }, tx);
        }

        #endregion


        #region Load Archivo

        public ErrorDto<int> FND_LiquidacionPlan_ArchivoRef_Cargar(
                int codEmpresa,
                FndArchivoRefCargaRequest request)
        {
            if (request?.lineas == null || request.lineas.Count == 0)
                return DbHelper.CreateErrorResponse<int>("No se recibieron líneas para cargar.");

            var cedulas = request.lineas
                .Select(x => (x.cedula ?? string.Empty).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (cedulas.Count == 0)
                return DbHelper.CreateErrorResponse<int>("No se encontraron identificaciones válidas.");

            SqlTransaction? tx = null;

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                tx = conn.BeginTransaction();

                conn.Execute("delete FND_ARCHIVO_REF", transaction: tx);

                const string sql = "exec spFnd_Archivo_Ref @Cedula, 0, @EsPrimera";

                for (int i = 0; i < cedulas.Count; i++)
                {
                    conn.Execute(sql, new
                    {
                        Cedula = cedulas[i],
                        EsPrimera = i == 0 ? 1 : 0
                    }, tx);
                }

                tx.Commit();
                return DbHelper.CreateOkResponse(cedulas.Count);
            }
            catch (Exception ex)
            {
                tx?.Rollback();
                return DbHelper.CreateErrorResponse<int>($"Error al cargar archivo de referencia: {ex.Message}");
            }
        }

        #endregion

    }
}
