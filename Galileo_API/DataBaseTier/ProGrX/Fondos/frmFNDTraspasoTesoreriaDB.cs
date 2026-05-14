using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndTraspasoTesoreriaDb
    {
        private readonly IConfiguration _config;

        private const string SpTesTokenConsulta = "spTes_Token_Consulta";
        private const string AppProductNameParam = "AppProductName";
        private const string AppProductNameDot = "App.ProductName";

        private const string SqlBancos = @"
                    SELECT
                        id_banco AS item,
                        RTRIM(descripcion) + '  ' + RTRIM(Cta) AS descripcion
                    FROM dbo.Tes_Bancos
                    WHERE estado = 'A';";

        private const string SqlConceptosRetencion = @"
                    SELECT
                        RTRIM(RETENCION_CODIGO) AS item,
                        RTRIM(descripcion) + ' [' + RTRIM(COD_CUENTA) + ']' AS descripcion
                    FROM dbo.FND_RETENCION_CONCEPTOS
                    WHERE ACTIVO = 1;";

        public FrmFndTraspasoTesoreriaDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de bancos activos para dropdown.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_Bancos_Obtener(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlBancos);
        }

        /// <summary>
        /// Obtiene la lista de conceptos de retención activos para dropdown.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_ConceptosRetencion_Obtener(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlConceptosRetencion);
        }

        /// <summary>
        /// Consulta los tokens de tesorería según los parámetros enviados.
        /// </summary>
        /// <param name="param">Parámetros de consulta de token.</param>
        /// <returns>ErrorDto con la lista de tokens.</returns>
        public ErrorDto<List<TesTokenConsultaResult>> Tes_Token_Consulta(TesTokenConsultaParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new List<TesTokenConsultaResult>());
            }

            return DbHelper.ExecuteStoredProcedureList<TesTokenConsultaResult>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa),
                SpTesTokenConsulta,
                new
                {
                    Token = NormalizarTexto(param.Token),
                    Estado = NormalizarTexto(param.Estado),
                    Usuario = NormalizarTexto(param.Usuario),
                    param.Top
                });
        }

        private static string ObtenerEstadoWhere(string estado)
        {
            return estado?.StartsWith('P') == true
                ? "L.Traspaso_tesoreria is Null"
                : "L.Traspaso_tesoreria is not Null";
        }

        /// <summary>
        /// Obtiene la lista de bancos de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de bancos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionBancos_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = $@"
                    Select 
                        L.cod_banco as item,
                        isnull(B.descripcion,'Sin Banco') as descripcion
                    From Fnd_Liquidacion L
                        left join Tes_Bancos B on L.cod_Banco = B.id_Banco
                    Where 
                        L.Fecha between @Desde and @Hasta
                        And {ObtenerEstadoWhere(param.Estado)}
                    Group by 
                        L.cod_banco,
                        B.descripcion";

                result.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    Desde = param.FechaDesde.Date,
                    Hasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de usuarios de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de usuarios.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionUsuarios_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = $@"
                    Select 
                        L.USUARIO as item,
                        L.USUARIO as descripcion
                    From Fnd_Liquidacion L
                    Where 
                        L.Fecha between @Desde and @Hasta
                        And {ObtenerEstadoWhere(param.Estado)}
                    Group by 
                        L.usuario";

                result.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    Desde = param.FechaDesde.Date,
                    Hasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de sistemas de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de sistemas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionSistemas_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = $@"
                    Select 
                        ISNULL(L.COD_APP,'App.ProductName') as descripcion,
                        ISNULL(L.COD_APP,'App.ProductName') as item
                    From Fnd_Liquidacion L
                    Where 
                        L.Fecha between @Desde and @Hasta
                        And {ObtenerEstadoWhere(param.Estado)}
                    Group by 
                        ISNULL(L.COD_APP,'App.ProductName')";

                result.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    Desde = param.FechaDesde.Date,
                    Hasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de tokens de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de tokens.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionTokens_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = $@"
                    Select 
                        ISNULL(L.ID_TOKEN,'') as descripcion,
                        ISNULL(L.ID_TOKEN,'') as item
                    From Fnd_Liquidacion L
                    Where 
                        L.Fecha between @Desde and @Hasta
                        And {ObtenerEstadoWhere(param.Estado)}
                    Group by 
                        ISNULL(L.ID_TOKEN,'')";

                result.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    Desde = param.FechaDesde.Date,
                    Hasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de oficinas de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de oficinas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionOficinas_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = $@"
                    Select 
                        rtrim(L.cod_Oficina) as item,
                        isnull(O.descripcion,'') as descripcion
                    From Fnd_Liquidacion L
                        left join SIF_Oficinas O on L.cod_oficina = O.cod_oficina
                    Where 
                        L.Fecha between @Desde and @Hasta
                        And {ObtenerEstadoWhere(param.Estado)}
                    Group by 
                        L.cod_Oficina,
                        O.descripcion";

                result.Result = connection.Query<DropDownListaGenericaModel>(query, new
                {
                    Desde = param.FechaDesde.Date,
                    Hasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Genera un nuevo token de tesorería.
        /// </summary>
        /// <param name="param">Parámetros para generación de token.</param>
        /// <returns>ErrorDto con el token generado.</returns>
        public ErrorDto<TesTokenNewResult> Tes_Token_New(TesTokenNewParams param)
        {
            var result = new ErrorDto<TesTokenNewResult>()
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Usuario = param.Usuario
                };

                // El SP no retorna nada, así que consultamos el último token generado por el usuario
                connection.Execute("spTes_Token_New", parameters, commandType: System.Data.CommandType.StoredProcedure);

                var query = @"
                    select top 1 ID_TOKEN, REGISTRO_FECHA, REGISTRO_USUARIO, ESTADO
                    from TES_TOKENS
                    where REGISTRO_USUARIO = @Usuario
                    order by REGISTRO_FECHA desc";

                result.Result = connection.QueryFirstOrDefault<TesTokenNewResult>(query, parameters);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el procedimiento para corregir traslados de tesorería (spFND_TrasladoTesoreria_Fix).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        public ErrorDto<FndTraspasoTesoreriaFixResult> TraspasoTesoreria_Fix(int codEmpresa)
        {
            var result = new ErrorDto<FndTraspasoTesoreriaFixResult>()
            {
                Code = 0,
                Description = "Ok",
                Result = new FndTraspasoTesoreriaFixResult()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
                using var connection = new SqlConnection(stringConn);

                connection.Execute("spFND_TrasladoTesoreria_Fix", commandType: System.Data.CommandType.StoredProcedure);

                result.Result.Success = true;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.Success = false;
            }
            return result;
        }

        /// <summary>
        /// Obtiene el valor de un parámetro desde SIF_parametros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codigo">Código del parámetro a consultar.</param>
        /// <returns>ErrorDto con el valor encontrado (string).</returns>
        public ErrorDto<string> TraspasoTesoreria_ParametroValor_Obtener(int codEmpresa, string codigo)
        {
            var result = new ErrorDto<string>()
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = @"Select valor from SIF_parametros where cod_parametro = @Codigo";
                result.Result = connection.QueryFirstOrDefault<string>(query, new { Codigo = codigo });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Consulta liquidaciones con filtros avanzados.
        /// </summary>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>ErrorDto con la lista de liquidaciones.</returns>
        public ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>> TraspasoTesoreria_LiquidacionConsulta(FndTraspasoTesoreriaLiquidacionConsultaParams param)
        {
            var result = new ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndTraspasoTesoreriaLiquidacionConsultaResult>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sql = @"
                    Select 
                        @Todos as Valor,
                        L.Consec,
                        C.Cedula,
                        S.nombre,
                        L.Cod_Plan,
                        L.Cod_Contrato,
                        case when L.Total_Girar is null 
                            then L.Aportes_Liq + L.Rendi_Liq - (isnull(L.multa_retiro,0) + isnull(L.ISR_MONTO,0) + isnull(L.OTROS_REBAJOS,0)) 
                            else L.Total_Girar 
                        end as Total_Girar,
                        L.Usuario,
                        isnull(L.cod_Oficina,'') as Oficina,
                        L.Tipo,
                        L.Cta_Ahorros,
                        B.descripcion,
                        L.Fecha,
                        dbo.fxTesSupervisa(C.cedula, S.nombre, isnull(L.Total_Girar, L.Aportes_Liq + L.Rendi_Liq - isnull(L.multa_retiro,0)), 0, 'C') as Duplicado,
                        TES_SUPERVISION_FECHA,
                        L.PAGO_TERCERO_APL,
                        L.PAGO_TERCERO_TIPO,
                        L.PAGO_TERCERO_ID,
                        L.PAGO_TERCERO_NOMBRE,
                        L.ID_TOKEN
                    From Fnd_Liquidacion L
                        inner join Fnd_Contratos C on L.Cod_Operadora = C.Cod_Operadora and L.Cod_Plan = C.Cod_Plan and L.Cod_Contrato = C.Cod_Contrato
                        inner join Socios S on C.cedula = S.cedula
                        left join Tes_Bancos B on L.cod_Banco = B.id_Banco
                    Where 
                        L.Fecha between @FechaDesde and @FechaHasta
                    ";

                var parameters = new DynamicParameters();
                parameters.Add("Todos", param.Todos);
                parameters.Add("FechaDesde", param.FechaDesde.Date);
                parameters.Add("FechaHasta", param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

                if (!string.IsNullOrEmpty(param.SifParam) && param.SifParam == "S")
                {
                    sql += " And L.Analista_Revision = 'S'";
                }

                if (!string.IsNullOrEmpty(param.Estado))
                {
                    if (param.Estado.StartsWith('P'))
                        sql += " And L.Traspaso_tesoreria is Null";
                    else
                        sql += " And L.Traspaso_tesoreria is not Null";
                }
                                if (param.Filtros)
                                {
                                    if (param.BancoId.HasValue)
                                        sql += " And L.cod_banco = @BancoId";
                                    if (!string.IsNullOrWhiteSpace(param.Oficina))
                                        sql += " And L.cod_oficina = @Oficina";
                                    if (!string.IsNullOrWhiteSpace(param.Usuario))
                                        sql += " And L.usuario like @Usuario";
                                    if (!string.IsNullOrWhiteSpace(param.Sistema))
                                        sql += $" And isnull(L.cod_app, @{AppProductNameParam}) like @Sistema";
                                    if (!string.IsNullOrWhiteSpace(param.TokenConsulta))
                                        sql += " And isnull(L.ID_Token, '') like @TokenConsulta";

                                    if (param.BancoId.HasValue)
                                        parameters.Add("BancoId", param.BancoId.Value);
                                    if (!string.IsNullOrWhiteSpace(param.Oficina))
                                        parameters.Add("Oficina", param.Oficina);
                                    if (!string.IsNullOrWhiteSpace(param.Usuario))
                                        parameters.Add("Usuario", param.Usuario + "%");
                                    if (!string.IsNullOrWhiteSpace(param.Sistema))
                                        parameters.Add("Sistema", param.Sistema + "%");
                                    if (!string.IsNullOrWhiteSpace(param.TokenConsulta))
                                        parameters.Add("TokenConsulta", param.TokenConsulta + "%");
                                    if (!string.IsNullOrWhiteSpace(param.AppProductName))
                                        parameters.Add(AppProductNameParam, param.AppProductName);
                                    else
                                        parameters.Add(AppProductNameParam, AppProductNameDot);
                                }
                                else
                                {
                                    parameters.Add(AppProductNameParam, AppProductNameDot);
                                }

                result.Result = connection.Query<FndTraspasoTesoreriaLiquidacionConsultaResult>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Consulta duplicados en la remesa de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>ErrorDto con la lista de duplicados.</returns>
        public ErrorDto<List<FndTraspasoTesoreriaDuplicadosResult>> RevisaDuplicadosEnLaRemesa(FndTraspasoTesoreriaDuplicadosParams param)
        {
            var result = new ErrorDto<List<FndTraspasoTesoreriaDuplicadosResult>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndTraspasoTesoreriaDuplicadosResult>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sql = @"
                    Select 
                        count(*) as Liquidaciones,
                        C.Cedula,
                        S.nombre,
                        L.Cta_Ahorros,
                        B.descripcion,
                        sum(case when L.Total_Girar is null 
                            then L.Aportes_Liq + L.Rendi_Liq - (isnull(L.multa_retiro,0) + isnull(L.ISR_MONTO,0) + isnull(L.OTROS_REBAJOS,0)) 
                            else L.Total_Girar end) as Total_Girar
                    From Fnd_Liquidacion L
                        inner join Fnd_Contratos C on L.Cod_Operadora = C.Cod_Operadora and L.Cod_Plan = C.Cod_Plan and L.Cod_Contrato = C.Cod_Contrato
                        inner join Socios S on C.cedula = S.cedula
                        left join Tes_Bancos B on L.cod_Banco = B.id_Banco
                    Where 
                        L.Fecha between @FechaDesde and @FechaHasta
                    ";

                var parameters = new DynamicParameters();
                parameters.Add("FechaDesde", param.FechaDesde.Date);
                parameters.Add("FechaHasta", param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

                if (!string.IsNullOrEmpty(param.SifParam) && param.SifParam == "S")
                {
                    sql += " And L.Analista_Revision = 'S'";
                }

                if (!string.IsNullOrEmpty(param.Estado))
                {
                    if (param.Estado.StartsWith('P'))
                        sql += " And L.Traspaso_tesoreria is Null";
                    else
                        sql += " And L.Traspaso_tesoreria is not Null";
                }

                if (param.Filtros)
                {
                    if (param.BancoId.HasValue)
                        sql += " And L.cod_banco = @BancoId";
                    if (!string.IsNullOrWhiteSpace(param.Oficina))
                        sql += " And L.cod_oficina = @Oficina";
                    if (!string.IsNullOrWhiteSpace(param.Usuario))
                        sql += " And L.usuario like @Usuario";
                    if (!string.IsNullOrWhiteSpace(param.Sistema))
                        sql += " And isnull(L.cod_app, @AppProductName) like @Sistema";
                    if (!string.IsNullOrWhiteSpace(param.TokenConsulta))
                        sql += " And isnull(L.ID_Token, '') like @TokenConsulta";

                    if (param.BancoId.HasValue)
                        parameters.Add("BancoId", param.BancoId.Value);
                    if (!string.IsNullOrWhiteSpace(param.Oficina))
                        parameters.Add("Oficina", param.Oficina);
                    if (!string.IsNullOrWhiteSpace(param.Usuario))
                        parameters.Add("Usuario", param.Usuario + "%");
                    if (!string.IsNullOrWhiteSpace(param.Sistema))
                        parameters.Add("Sistema", param.Sistema + "%");
                    if (!string.IsNullOrWhiteSpace(param.TokenConsulta))
                        parameters.Add("TokenConsulta", param.TokenConsulta + "%");
                    if (!string.IsNullOrWhiteSpace(param.AppProductName))
                        parameters.Add(AppProductNameParam, param.AppProductName);
                    else
                        parameters.Add(AppProductNameParam, AppProductNameDot);
                }
                else
                {
                    parameters.Add(AppProductNameParam, AppProductNameDot);
                }

                sql += @"
                     group by C.Cedula, S.nombre, L.Cta_Ahorros, B.descripcion
                     having count(*) > 1";

                result.Result = connection.Query<FndTraspasoTesoreriaDuplicadosResult>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el procedimiento de retiro de liquidación a tesorería.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, LiqNum, Usuario, Token.</param>
        /// <returns>ErrorDto con el resultado de la operación (true si fue exitosa, false si hubo error).</returns>
        public ErrorDto<bool> RetLiqTesoreria(FndRetLiqTesoreriaParams param)
        {
            var result = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    LiqNum = param.LiqNum,
                    Usuario = param.Usuario,
                    Token = param.Token
                };

                connection.Execute("spFndRetLiqTesoreria", parameters, commandType: System.Data.CommandType.StoredProcedure);
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// Actualiza los datos de traspaso y retención en Fnd_Liquidacion.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, Consec, Usuario, RetencionCodigo.</param>
        /// <returns>ErrorDto con el resultado de la operación (true si fue exitosa, false si hubo error).</returns>
        public ErrorDto<bool> TraspasoTesoreria_Update(FndTraspasoTesoreriaUpdateParams param)
        {
            var result = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = @"
                    Update Fnd_Liquidacion
                    set Traspaso_Tesoreria = dbo.MyGetdate(),
                        Traspaso_Usuario = @Usuario,
                        Solicitud_Tesoreria = 0,
                        RETENCION_CODIGO = @RetencionCodigo,
                        NOTAS = ''
                    Where Consec = @Consec";

                connection.Execute(query, new
                {
                    Usuario = param.Usuario,
                    RetencionCodigo = param.RetencionCodigo,
                    Consec = param.Consec
                });

                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado de retiro/liquidación unificado por persona.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, Cedula, Usuario, IdToken, FechaDesde, FechaHasta.</param>
        /// <returns>ErrorDto con el resultado de la operación (true si fue exitosa, false si hubo error).</returns>
        public ErrorDto<bool> RetLiqTesoreria_Unificado(FndRetLiqTesoreriaUnificadoParams param)
        {
            var result = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new
                {
                    Cedula = param.Cedula.Trim(),
                    Usuario = param.Usuario,
                    Token = param.IdToken,
                    FechaInicio = param.FechaDesde.Date,
                    FechaCorte = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                };

                connection.Execute("spFndRetLiqTesoreria_Unificado", parameters, commandType: System.Data.CommandType.StoredProcedure);
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// Consulta el detalle de liquidaciones por cédula y filtros avanzados.
        /// </summary>
        /// <param name="param">Parámetros: CodEmpresa, FechaDesde, FechaHasta, Cedula, SifParam, Estado, Filtros, BancoId, Oficina, Usuario, Sistema, TokenConsulta, AppProductName.</param>
        /// <returns>ErrorDto con la lista de liquidaciones detalle.</returns>
        public ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>> TraspasoTesoreria_LiquidacionDetalle(FndTraspasoTesoreriaDetalleParams param)
        {
            var result = new ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndTraspasoTesoreriaLiquidacionConsultaResult>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(param.CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var sql = @"
            SELECT
                L.Consec,
                C.Cedula,
                S.Nombre,
                L.Cod_Plan,
                L.Cod_Contrato,
                CASE 
                    WHEN L.Total_Girar IS NULL THEN
                        L.Aportes_Liq + L.Rendi_Liq
                        - (ISNULL(L.multa_retiro,0)
                           + ISNULL(L.ISR_MONTO,0)
                           + ISNULL(L.OTROS_REBAJOS,0))
                    ELSE
                        L.Total_Girar
                END AS Total_Girar,
                L.Usuario,
                ISNULL(L.cod_Oficina,'') AS Oficina,
                L.Tipo,
                L.Cta_Ahorros,
                B.Descripcion,
                L.Fecha,
                dbo.fxTesSupervisa(
                    C.cedula,
                    S.nombre,
                    ISNULL(
                        L.Total_Girar,
                        L.Aportes_Liq+L.Rendi_Liq - ISNULL(L.multa_retiro,0)
                    ),
                    0,
                    'C'
                ) AS Duplicado,
                TES_SUPERVISION_FECHA,
                L.PAGO_TERCERO_APL,
                L.PAGO_TERCERO_TIPO,
                L.PAGO_TERCERO_ID,
                L.PAGO_TERCERO_NOMBRE,
                L.ID_TOKEN
            FROM Fnd_Liquidacion L
                INNER JOIN Fnd_Contratos C
                    ON L.Cod_Operadora = C.Cod_Operadora
                   AND L.Cod_Plan      = C.Cod_Plan
                   AND L.Cod_Contrato  = C.Cod_Contrato
                INNER JOIN Socios S
                    ON C.cedula = S.cedula
                LEFT JOIN Tes_Bancos B
                    ON L.cod_Banco = B.id_Banco
            WHERE
                L.Fecha BETWEEN @FechaDesde AND @FechaHasta
                AND C.Cedula = @Cedula
        ";

                var parameters = new DynamicParameters();
                parameters.Add("FechaDesde", param.FechaDesde.Date);
                parameters.Add("FechaHasta", param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                parameters.Add("Cedula", param.Cedula);

                if (!string.IsNullOrEmpty(param.SifParam) && param.SifParam == "S")
                {
                    sql += " AND L.Analista_Revision = 'S'";
                }

                if (!string.IsNullOrEmpty(param.Estado))
                {
                    if (param.Estado.StartsWith('P'))
                        sql += " AND L.Traspaso_tesoreria is Null";
                    else
                        sql += " AND L.Traspaso_tesoreria is not Null";
                }

                if (param.Filtros)
                {
                    if (param.BancoId.HasValue)
                        sql += " AND L.cod_banco = @BancoId";
                    if (!string.IsNullOrWhiteSpace(param.Oficina))
                        sql += " AND L.cod_oficina = @Oficina";
                    if (!string.IsNullOrWhiteSpace(param.Usuario))
                        sql += " AND L.usuario like @Usuario";
                    if (!string.IsNullOrWhiteSpace(param.Sistema))
                        sql += " AND isnull(L.cod_app, @AppProductName) like @Sistema";
                    if (!string.IsNullOrWhiteSpace(param.TokenConsulta))
                        sql += " AND isnull(L.ID_Token, '') like @TokenConsulta";

                    if (param.BancoId.HasValue)
                        parameters.Add("BancoId", param.BancoId.Value);
                    if (!string.IsNullOrWhiteSpace(param.Oficina))
                        parameters.Add("Oficina", param.Oficina);
                    if (!string.IsNullOrWhiteSpace(param.Usuario))
                        parameters.Add("Usuario", param.Usuario + "%");
                    if (!string.IsNullOrWhiteSpace(param.Sistema))
                        parameters.Add("Sistema", param.Sistema + "%");
                    if (!string.IsNullOrWhiteSpace(param.TokenConsulta))
                        parameters.Add("TokenConsulta", param.TokenConsulta + "%");
                    if (!string.IsNullOrWhiteSpace(param.AppProductName))
                        parameters.Add(AppProductNameParam, param.AppProductName);
                    else
                        parameters.Add(AppProductNameParam, AppProductNameDot);
                }
                else
                {
                    parameters.Add(AppProductNameParam, AppProductNameDot);
                }

                result.Result = connection.Query<FndTraspasoTesoreriaLiquidacionConsultaResult>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
