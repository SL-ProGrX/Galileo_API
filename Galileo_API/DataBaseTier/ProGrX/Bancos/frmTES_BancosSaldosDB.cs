using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Newtonsoft.Json;


namespace Galileo_API.DataBaseTier
{
    public class FrmTesBancosSaldosDB
    {

        private readonly PortalDB _portalDB;

        public FrmTesBancosSaldosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener lista de grupos bancarios para el dropdown
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Grupos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(cod_grupo) as item, rtrim(Descripcion) as descripcion 
                        from TES_BANCOS_GRUPOS where Activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtener lista de cuentas bancarios para el dropdown
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Cuentas_Obtener(int CodEmpresa, string CodGrupo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select id_banco as item,rtrim(descripcion) as descripcion
                        from Tes_Bancos where monitoreo = 1 and cod_Grupo = @grupo";

                return conn.Query<DropDownListaGenericaModel>(query, new { grupo = CodGrupo }).ToList();
            });
        }

        /// <summary>
        /// Obtener lista de bancos, indicando cuales tienen monitoreo activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Monitoreo_Obtener(int CodEmpresa, string CodGrupo, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<TesBancosSaldosMonitoreoDto>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                // Paginación: offset = filas a saltar (si pagina es pageNumber, te digo abajo cómo)
                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0;

                const string sqlCount = @"
SELECT COUNT(1)
FROM Tes_Bancos
WHERE cod_grupo = @grupo
  AND (
        @filtro IS NULL
     OR CAST(id_banco AS NVARCHAR(50)) LIKE @like
     OR descripcion LIKE @like
     OR cta LIKE @like
  );";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    grupo = CodGrupo,
                    filtro = hasFiltro ? texto : null,
                    like
                });

                var sqlList = @"
SELECT
    id_banco,
    descripcion,
    cta,
    ISNULL(monitoreo, 0) AS monitoreo
FROM Tes_Bancos
WHERE cod_grupo = @grupo
  AND (
        @filtro IS NULL
     OR CAST(id_banco AS NVARCHAR(50)) LIKE @like
     OR descripcion LIKE @like
     OR cta LIKE @like
  )
ORDER BY monitoreo DESC, descripcion";

                if (usarPaginacion)
                {
                    sqlList += @"
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";
                }

                response.Result.lista = conn.Query<TesBancosSaldosMonitoreoDto>(sqlList, new
                {
                    grupo = CodGrupo,
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Actualizar monitoreo (activo o inactivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="Monitoreo"></param>
        /// <returns></returns>
        public ErrorDto TES_BancosSaldos_Monitoreo_Actualizar(int CodEmpresa, string Banco, bool Monitoreo)
        {

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = @"update Tes_Bancos set monitoreo = @monitoreo where id_banco = @banco";
                conn.Execute(query, new
                {
                    banco = Banco,
                    monitoreo = Monitoreo ? 1 : 0
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Obtener historico de un banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Historico_Obtener(int CodEmpresa, int Banco, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<TesBancosSaldosHistoricoDto>()
                }
            };

            try
            {
                // --- filtros de fechas ---
                var parametros = filtros.parametros?.ToString() ?? string.Empty;
                var filtrosFechas = JsonConvert.DeserializeObject<HistoricoFiltros>(parametros)
                                  ?? new HistoricoFiltros();

                var fechaInicio = filtrosFechas.inicio.Date;
                var fechaCorte = filtrosFechas.corte.Date.AddDays(1).AddTicks(-1);
                var filtrarFechas = !filtrosFechas.todas_fechas;

                // --- filtro texto ---
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                // --- paginación ---
                var offset = filtros.pagina;       // si esto es pageNumber, abajo te digo ajuste
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0;

                // --- ORDER BY seguro (whitelist) ---
                var sortField = (filtros.sortField ?? string.Empty).Trim();
                var orderByField = sortField switch
                {
                    "idx" => "idx",
                    "id_banco" => "id_banco",
                    "usuario" => "usuario",
                    "inicio" => "inicio",
                    "corte" => "corte",
                    _ => "idx"
                };

                // Tu código: (sortOrder == 0 ? "DESC" : "ASC")
                var direction = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // --- COUNT con mismos filtros ---
                const string sqlCount = @"
SELECT COUNT(1)
FROM TES_BANCOS_CIERRES
WHERE id_banco = @banco
  AND (
        @filtro IS NULL
     OR CAST(id_banco AS NVARCHAR(50)) LIKE @like
     OR CAST(idx AS NVARCHAR(50)) LIKE @like
     OR usuario LIKE @like
  )
  AND (
        @filtrarFechas = 0
     OR (INICIO >= @fechaInicio AND CORTE <= @fechaCorte)
  );";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    banco = Banco,
                    filtro = hasFiltro ? texto : null,
                    like,
                    filtrarFechas = filtrarFechas ? 1 : 0,
                    fechaInicio,
                    fechaCorte
                });

                // --- LISTA con mismos filtros ---
                var sqlList = $@"
SELECT *
FROM TES_BANCOS_CIERRES
WHERE id_banco = @banco
  AND (
        @filtro IS NULL
     OR CAST(id_banco AS NVARCHAR(50)) LIKE @like
     OR CAST(idx AS NVARCHAR(50)) LIKE @like
     OR usuario LIKE @like
  )
  AND (
        @filtrarFechas = 0
     OR (INICIO >= @fechaInicio AND CORTE <= @fechaCorte)
  )
ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlList += @"
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";
                }

                response.Result.lista = conn.Query<TesBancosSaldosHistoricoDto>(sqlList, new
                {
                    banco = Banco,
                    filtro = hasFiltro ? texto : null,
                    like,
                    filtrarFechas = filtrarFechas ? 1 : 0,
                    fechaInicio,
                    fechaCorte,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Obtener cierres bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDto<TesBancosSaldosCierresDto> TES_BancosSaldos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TesBancosSaldosCierresDto>
            {
                Code = 0,
                Result = new TesBancosSaldosCierresDto()
            };
            try
            {
                response.Result.inicio = DateTime.Today;
                response.Result.corte = DateTime.Today;

                var query = @"select corte,saldo_final,saldo_minimo from TES_BANCOS_CIERRES 
                        where idX = (select max(idX) from TES_BANCOS_CIERRES where id_banco = @Banco)";

                var cierre = conn.QueryFirstOrDefault<TesBancosSaldosCierresDto>(query, new { Banco });


                if (cierre != null)
                {
                    response.Result.inicio = (cierre.corte).AddDays(1);
                    response.Result.corte = (cierre.corte).AddDays(1);
                    response.Result.inicio_habilitado = false;
                    response.Result.saldo_inicial = cierre.saldo_final;
                    response.Result.saldo_minimo = cierre.saldo_minimo;
                }
                else
                {
                    response.Result.inicio_habilitado = true;
                    response.Result.saldo_inicial = 0;
                    response.Result.saldo_minimo = 0;
                }
                response.Result.id_banco = Banco;
                response = TES_BancosSaldos_Movimientos_Obtener(CodEmpresa, response.Result);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener movimientos bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto<TesBancosSaldosCierresDto> TES_BancosSaldos_Movimientos_Obtener(int CodEmpresa, TesBancosSaldosCierresDto datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TesBancosSaldosCierresDto>
            {
                Code = 0,
                Result = new TesBancosSaldosCierresDto()
            };
            try
            {
                var FechaInicio = datos.inicio.Date;
                var FechaCorte = datos.corte.Date.AddDays(1).AddTicks(-1);
                string cuentaSql = "SELECT ctaConta FROM Tes_Bancos WHERE id_Banco = @banco";
                string vCuenta = conn.QueryFirstOrDefault<string>(cuentaSql, new { banco = datos.id_banco }) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(vCuenta))
                {
                    response.Code = -1;
                    response.Description = "Cuenta contable no encontrada.";
                    return response;
                }

                //Saca Debitos y Créditos de las Cuentas Bancarias
                decimal vDebitos = 0, vCreditos = 0;

                //Emisiones de Documentos
                string sqlEmisiones = @"SELECT D.debehaber AS Movimiento, SUM(D.monto) AS Total
                        FROM Tes_Transacciones C INNER JOIN Tes_Trans_Asiento D ON C.nsolicitud = D.nsolicitud
                        WHERE C.fecha_emision BETWEEN @inicio AND @corte 
                          AND C.estado IN ('I','T','A')
                          AND D.cuenta_contable = @cuenta
                        GROUP BY D.debehaber";
                var emisiones = conn.Query(sqlEmisiones, new { inicio = FechaInicio, corte = FechaCorte, cuenta = vCuenta });

                foreach (var mov in emisiones)
                {
                    if (mov.Movimiento == "D")
                    {
                        vCreditos = (decimal)mov.Total;
                    }
                    else
                    {
                        vDebitos = (decimal)mov.Total;
                    }
                }

                //Anulaciones de Documentos
                string sqlAnulaciones = @"SELECT D.debehaber AS Movimiento, SUM(D.monto) AS Total
                        FROM Tes_Transacciones C
                        INNER JOIN Tes_Trans_Asiento D ON C.nsolicitud = D.nsolicitud
                        WHERE C.fecha_anula BETWEEN @inicio AND @corte
                          AND C.estado = 'A'
                          AND D.cuenta_contable = @cuenta
                        GROUP BY D.debehaber";
                var anulaciones = conn.Query(sqlAnulaciones, new { inicio = FechaInicio, corte = FechaCorte, cuenta = vCuenta });

                foreach (var mov in anulaciones)
                {
                    if (mov.Movimiento == "D")
                    {
                        vDebitos = vDebitos + (decimal)mov.Total;
                    }
                    else
                    {
                        vCreditos = vCreditos + (decimal)mov.Total;
                    }
                }

                response.Result = datos;
                response.Result.total_debitos = vDebitos;
                response.Result.total_creditos = vCreditos;
                response.Result.saldo_final = datos.saldo_inicial - vDebitos + vCreditos;
                response.Result.ajuste = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Actualizar cierres bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto TES_BancosSaldos_Cierres_Actualizar(int CodEmpresa, string Usuario, TesBancosSaldosCierresDto datos)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {

                string fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(datos.inicio, "yyyy-MM-dd 00:00:00.000") ?? "";
                string fechaFin = MProGrXAuxiliarDB.validaFechaGlobal(datos.corte, "yyyy-MM-dd 23:59:59") ?? "";
                string fechaFin2 = MProGrXAuxiliarDB.validaFechaGlobal(datos.corte, "yyyy-MM-dd 00:00:00.000") ?? "";
                DateTime vFechaIni = Convert.ToDateTime(fechaIni);
                DateTime vFechaFin = Convert.ToDateTime(fechaFin);

                if (vFechaIni > vFechaFin)
                {
                    response.Code = -1;
                    response.Description = "La fecha de corte no puede ser menor a la de inicio, verifique...";
                    return response;
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                string query = @"
                    INSERT TES_BANCOS_CIERRES (
                        id_banco, fecha, usuario, inicio, corte, saldo_inicial,
                        total_debitos, total_creditos, saldo_final, ajuste, saldo_minimo, TIPO_CIERRE
                    )
                    VALUES (
                        @Banco, GETDATE(), @Usuario, @Inicio, @Corte, @SaldoInicial,
                        @Debitos, @Creditos, @SaldoFinal, @Ajuste, @SaldoMinimo, @cierreTipo
                    )";

                decimal saldoFinal = datos.saldo_final - datos.total_debitos + datos.total_creditos;

                conn.Execute(query, new
                {
                    Banco = datos.id_banco,
                    Usuario = Usuario,
                    Inicio = fechaIni,
                    Corte = fechaFin2,
                    SaldoInicial = datos.saldo_inicial,
                    Debitos = datos.total_debitos,
                    Creditos = datos.total_creditos,
                    SaldoFinal = saldoFinal,
                    Ajuste = datos.ajuste,
                    SaldoMinimo = datos.saldo_minimo,
                    cierreTipo = datos.tipo_cierre
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
