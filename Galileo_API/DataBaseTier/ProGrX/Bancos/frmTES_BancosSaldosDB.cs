using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.TES;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_BancosSaldosDB
    {
        private readonly IConfiguration? _config;

        public frmTES_BancosSaldosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener lista de grupos bancarios para el dropdown
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Grupos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rtrim(cod_grupo) as item, rtrim(Descripcion) as descripcion 
                        from TES_BANCOS_GRUPOS where Activo = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
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
        /// Obtener lista de cuentas bancarios para el dropdown
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosSaldos_Cuentas_Obtener(int CodEmpresa, string CodGrupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select id_banco as item,rtrim(descripcion) as descripcion
                        from Tes_Bancos where monitoreo = 1 and cod_Grupo = @grupo";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, 
                        new { grupo = CodGrupo }).ToList();
                }
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
        /// Obtener lista de bancos, indicando cuales tienen monitoreo activo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodGrupo"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_BancosSaldos_Monitoreo_Obtener(int CodEmpresa, string CodGrupo, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    query = @"select COUNT(id_banco) from Tes_Bancos where cod_grupo = @grupo";
                    response.Result.total = connection.Query<int>(query, new { grupo = CodGrupo}).FirstOrDefault();

                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " and ( id_banco LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR cta LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select id_banco,descripcion,cta,isnull(monitoreo,0) as monitoreo from Tes_Bancos
                        where cod_grupo = @grupo
                        {filtros.filtro} 
                        order by monitoreo desc, descripcion 
                         {paginaActual}
                         {paginacionActual} ";

                    response.Result.lista = connection.Query<TesBancosSaldosMonitoreoDto>(query, new { grupo = CodGrupo }).ToList();
                }
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"update Tes_Bancos set monitoreo = @monitoreo where id_banco = @banco";
                    connection.Execute(query, new
                    {
                        banco = Banco,
                        monitoreo = Monitoreo ? 1 : 0
                    });
                }
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            HistoricoFiltros filtrosFechas = JsonConvert.DeserializeObject<HistoricoFiltros>((string)filtros.parametros);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            var fechaInicio = filtrosFechas.inicio.Date;
            var fechaCorte = filtrosFechas.corte.Date.AddDays(1).AddTicks(-1);
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    query = @"select COUNT(idx) from TES_BANCOS_CIERRES where id_banco = @banco";
                    response.Result.total = connection.Query<int>(query, new { banco = Banco }).FirstOrDefault();

                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " and ( id_banco LIKE '%" + filtros.filtro + "%' " +
                            " OR idx LIKE '%" + filtros.filtro + "%' " +
                            " OR usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "idx";
                    }

                    if (!filtrosFechas.todas_fechas)
                    {
                        filtros.filtro += " AND INICIO >= '"+ fechaInicio + "' AND CORTE <= '"+ fechaCorte +"' ";
                    }

                    query = $@"select * from TES_BANCOS_CIERRES 
                        where id_banco = @banco
                        {filtros.filtro} 
                        order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                         {paginaActual}
                         {paginacionActual} ";

                    response.Result.lista = connection.Query<TesBancosSaldosHistoricoDto>(query, new { banco = Banco }).ToList();
                }
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
        /// Obtener cierres bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDto<TesBancosSaldosCierresDto> TES_BancosSaldos_Cierres_Obtener(int CodEmpresa, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesBancosSaldosCierresDto>
            {
                Code = 0,
                Result = new TesBancosSaldosCierresDto()
            };
            try
            {
                response.Result.inicio = DateTime.Today;
                response.Result.corte = DateTime.Today;

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select corte,saldo_final,saldo_minimo from TES_BANCOS_CIERRES 
                        where idX = (select max(idX) from TES_BANCOS_CIERRES where id_banco = @Banco)";
                    
                    var cierre = connection.QueryFirstOrDefault<TesBancosSaldosCierresDto>(query, new { Banco });

                    if (cierre != null)
                    {
                        response.Result.inicio = ((DateTime)cierre.corte).AddDays(1); 
                        response.Result.inicio_habilitado = false;
                        response.Result.saldo_inicial = (decimal)cierre.saldo_final;
                        response.Result.saldo_minimo = (decimal)cierre.saldo_minimo;
                    }
                    else
                    {
                        response.Result.inicio_habilitado = true;
                        response.Result.saldo_inicial = 0;
                        response.Result.saldo_minimo = 0;
                    }
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesBancosSaldosCierresDto>
            {
                Code = 0,
                Result = new TesBancosSaldosCierresDto()
            };
            try
            {
                var FechaInicio = datos.inicio.Date;
                var FechaCorte = datos.corte.Date.AddDays(1).AddTicks(-1);
                using var connection = new SqlConnection(stringConn);
                {
                    string cuentaSql = "SELECT ctaConta FROM Tes_Bancos WHERE id_Banco = @banco";
                    string vCuenta = connection.QueryFirstOrDefault<string>(cuentaSql, new { banco = datos.id_banco });

                    if (string.IsNullOrWhiteSpace(vCuenta))
                    {
                        response.Code = -1;
                        response.Description = "Cuenta contable no encontrada.";
                        return response;
                    }

                    //Saca Debitos y Creditos de las Cuentas Bancarias
                    decimal vDebitos = 0, vCreditos = 0;

                    //Emisiones de Documentos
                    string sqlEmisiones = @"SELECT D.debehaber AS Movimiento, SUM(D.monto) AS Total
                        FROM Tes_Transacciones C INNER JOIN Tes_Trans_Asiento D ON C.nsolicitud = D.nsolicitud
                        WHERE C.fecha_emision BETWEEN @inicio AND @corte 
                          AND C.estado IN ('I','T','A')
                          AND D.cuenta_contable = @cuenta
                        GROUP BY D.debehaber";
                    var emisiones = connection.Query(sqlEmisiones, new { inicio = FechaInicio, corte = FechaCorte, cuenta = vCuenta });

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
                    var anulaciones = connection.Query(sqlAnulaciones, new { inicio = FechaInicio, corte = FechaCorte, cuenta = vCuenta });

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
                if (datos.inicio > datos.corte)
                {
                    response.Code = -1;
                    response.Description = "La fecha de corte no puede ser menor a la de inicio, verifique...";
                    return response;
                }

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                {
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

                    connection.Execute(query, new
                    {
                        Banco = datos.id_banco,
                        Usuario = Usuario,
                        Inicio = datos.inicio,
                        Corte = datos.corte,
                        SaldoInicial = datos.saldo_inicial,
                        Debitos = datos.total_debitos,
                        Creditos = datos.total_creditos,
                        SaldoFinal = saldoFinal,
                        Ajuste = datos.ajuste,
                        SaldoMinimo = datos.saldo_minimo,
                        cierreTipo = datos.tipo_cierre
                    });
                }        
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