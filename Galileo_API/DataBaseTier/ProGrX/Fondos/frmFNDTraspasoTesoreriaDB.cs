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
        private const string vfiltro = "Los parámetros de filtro son requeridos.";
        private const string AppProductNameDot = "App.ProductName";

        private const string SqlFiltroEstadoLiquidacion = @"
                      AND (@EstadoPendiente IS NULL OR
                          (@EstadoPendiente = 1 AND L.Traspaso_tesoreria IS NULL) OR
                          (@EstadoPendiente = 0 AND L.Traspaso_tesoreria IS NOT NULL))";

        private const string SqlFiltroRevisionLiquidacion = @"
                      AND (@AplicaRevision = 0 OR L.Analista_Revision = 'S')";

        private const string SqlFiltroAvanzadoLiquidacion = @"
                      AND (@AplicarFiltros = 0 OR @BancoId IS NULL OR L.cod_banco = @BancoId)
                      AND (@AplicarFiltros = 0 OR @Oficina = '' OR L.cod_oficina = @Oficina)
                      AND (@AplicarFiltros = 0 OR @Usuario = '' OR L.usuario LIKE @Usuario)
                      AND (@AplicarFiltros = 0 OR @Sistema = '' OR ISNULL(L.cod_app, @AppProductName) LIKE @Sistema)
                      AND (@AplicarFiltros = 0 OR @TokenConsulta = '' OR ISNULL(L.ID_Token, '') LIKE @TokenConsulta)";

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


        private static readonly string SqlLiquidacionBancos = $@"
                    SELECT
                        L.cod_banco AS item,
                        ISNULL(B.descripcion, 'Sin Banco') AS descripcion
                    FROM dbo.Fnd_Liquidacion L
                    LEFT JOIN dbo.Tes_Bancos B
                        ON L.cod_Banco = B.id_Banco
                    WHERE L.Fecha BETWEEN @Desde AND @Hasta
                      {SqlFiltroEstadoLiquidacion}
                    GROUP BY
                        L.cod_banco,
                        B.descripcion;";

        private static readonly string SqlLiquidacionUsuarios = $@"
                    SELECT
                        L.USUARIO AS item,
                        L.USUARIO AS descripcion
                    FROM dbo.Fnd_Liquidacion L
                    WHERE L.Fecha BETWEEN @Desde AND @Hasta
                      {SqlFiltroEstadoLiquidacion}
                    GROUP BY L.usuario;";

        private static readonly string SqlLiquidacionSistemas = $@"
                    SELECT
                        ISNULL(L.COD_APP, @AppProductName) AS descripcion,
                        ISNULL(L.COD_APP, @AppProductName) AS item
                    FROM dbo.Fnd_Liquidacion L
                    WHERE L.Fecha BETWEEN @Desde AND @Hasta
                      {SqlFiltroEstadoLiquidacion}
                    GROUP BY ISNULL(L.COD_APP, @AppProductName);";

        private static readonly string SqlLiquidacionTokens = $@"
                    SELECT
                        ISNULL(L.ID_TOKEN, '') AS descripcion,
                        ISNULL(L.ID_TOKEN, '') AS item
                    FROM dbo.Fnd_Liquidacion L
                    WHERE L.Fecha BETWEEN @Desde AND @Hasta
                      {SqlFiltroEstadoLiquidacion}
                    GROUP BY ISNULL(L.ID_TOKEN, '');";

        private static readonly string SqlLiquidacionOficinas = $@"
                    SELECT
                        RTRIM(L.cod_Oficina) AS item,
                        ISNULL(O.descripcion, '') AS descripcion
                    FROM dbo.Fnd_Liquidacion L
                    LEFT JOIN dbo.SIF_Oficinas O
                        ON L.cod_oficina = O.cod_oficina
                    WHERE L.Fecha BETWEEN @Desde AND @Hasta
                      {SqlFiltroEstadoLiquidacion}
                    GROUP BY
                        L.cod_Oficina,
                        O.descripcion;";

        private static readonly string SqlLiquidacionConsulta = $@"
                    SELECT
                        @Todos AS Valor,
                        L.Consec,
                        C.Cedula,
                        S.nombre,
                        L.Cod_Plan,
                        L.Cod_Contrato,
                        CASE WHEN L.Total_Girar IS NULL
                            THEN L.Aportes_Liq + L.Rendi_Liq - (ISNULL(L.multa_retiro, 0) + ISNULL(L.ISR_MONTO, 0) + ISNULL(L.OTROS_REBAJOS, 0))
                            ELSE L.Total_Girar
                        END AS Total_Girar,
                        L.Usuario,
                        ISNULL(L.cod_Oficina, '') AS Oficina,
                        L.Tipo,
                        L.Cta_Ahorros,
                        B.descripcion,
                        L.Fecha,
                        dbo.fxTesSupervisa(C.cedula, S.nombre, ISNULL(L.Total_Girar, L.Aportes_Liq + L.Rendi_Liq - ISNULL(L.multa_retiro, 0)), 0, 'C') AS Duplicado,
                        TES_SUPERVISION_FECHA,
                        L.PAGO_TERCERO_APL,
                        L.PAGO_TERCERO_TIPO,
                        L.PAGO_TERCERO_ID,
                        L.PAGO_TERCERO_NOMBRE,
                        L.ID_TOKEN
                    FROM dbo.Fnd_Liquidacion L
                    INNER JOIN dbo.Fnd_Contratos C
                        ON L.Cod_Operadora = C.Cod_Operadora
                       AND L.Cod_Plan = C.Cod_Plan
                       AND L.Cod_Contrato = C.Cod_Contrato
                    INNER JOIN dbo.Socios S
                        ON C.cedula = S.cedula
                    LEFT JOIN dbo.Tes_Bancos B
                        ON L.cod_Banco = B.id_Banco
                    WHERE L.Fecha BETWEEN @FechaDesde AND @FechaHasta
                      {SqlFiltroRevisionLiquidacion}
                      {SqlFiltroEstadoLiquidacion}
                      {SqlFiltroAvanzadoLiquidacion};";

        private static readonly string SqlDuplicadosRemesa = $@"
                    SELECT
                        COUNT(*) AS Liquidaciones,
                        C.Cedula,
                        S.nombre,
                        L.Cta_Ahorros,
                        B.descripcion,
                        SUM(CASE WHEN L.Total_Girar IS NULL
                            THEN L.Aportes_Liq + L.Rendi_Liq - (ISNULL(L.multa_retiro, 0) + ISNULL(L.ISR_MONTO, 0) + ISNULL(L.OTROS_REBAJOS, 0))
                            ELSE L.Total_Girar
                        END) AS Total_Girar
                    FROM dbo.Fnd_Liquidacion L
                    INNER JOIN dbo.Fnd_Contratos C
                        ON L.Cod_Operadora = C.Cod_Operadora
                       AND L.Cod_Plan = C.Cod_Plan
                       AND L.Cod_Contrato = C.Cod_Contrato
                    INNER JOIN dbo.Socios S
                        ON C.cedula = S.cedula
                    LEFT JOIN dbo.Tes_Bancos B
                        ON L.cod_Banco = B.id_Banco
                    WHERE L.Fecha BETWEEN @FechaDesde AND @FechaHasta
                      {SqlFiltroRevisionLiquidacion}
                      {SqlFiltroEstadoLiquidacion}
                      {SqlFiltroAvanzadoLiquidacion}
                    GROUP BY C.Cedula, S.nombre, L.Cta_Ahorros, B.descripcion
                    HAVING COUNT(*) > 1;";

        private static readonly string SqlLiquidacionDetalle = $@"
                    SELECT
                        L.Consec,
                        C.Cedula,
                        S.Nombre,
                        L.Cod_Plan,
                        L.Cod_Contrato,
                        CASE WHEN L.Total_Girar IS NULL
                            THEN L.Aportes_Liq + L.Rendi_Liq - (ISNULL(L.multa_retiro, 0) + ISNULL(L.ISR_MONTO, 0) + ISNULL(L.OTROS_REBAJOS, 0))
                            ELSE L.Total_Girar
                        END AS Total_Girar,
                        L.Usuario,
                        ISNULL(L.cod_Oficina, '') AS Oficina,
                        L.Tipo,
                        L.Cta_Ahorros,
                        B.Descripcion,
                        L.Fecha,
                        dbo.fxTesSupervisa(C.cedula, S.nombre, ISNULL(L.Total_Girar, L.Aportes_Liq + L.Rendi_Liq - ISNULL(L.multa_retiro, 0)), 0, 'C') AS Duplicado,
                        TES_SUPERVISION_FECHA,
                        L.PAGO_TERCERO_APL,
                        L.PAGO_TERCERO_TIPO,
                        L.PAGO_TERCERO_ID,
                        L.PAGO_TERCERO_NOMBRE,
                        L.ID_TOKEN
                    FROM dbo.Fnd_Liquidacion L
                    INNER JOIN dbo.Fnd_Contratos C
                        ON L.Cod_Operadora = C.Cod_Operadora
                       AND L.Cod_Plan = C.Cod_Plan
                       AND L.Cod_Contrato = C.Cod_Contrato
                    INNER JOIN dbo.Socios S
                        ON C.cedula = S.cedula
                    LEFT JOIN dbo.Tes_Bancos B
                        ON L.cod_Banco = B.id_Banco
                    WHERE L.Fecha BETWEEN @FechaDesde AND @FechaHasta
                      AND C.Cedula = @Cedula
                      {SqlFiltroRevisionLiquidacion}
                      {SqlFiltroEstadoLiquidacion}
                      {SqlFiltroAvanzadoLiquidacion};";

        /// <summary>
        /// Obtiene la lista de bancos de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de bancos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionBancos_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionBancos,
                CrearParametrosFiltroLiquidacion(param));
        }

        /// <summary>
        /// Obtiene la lista de usuarios de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de usuarios.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionUsuarios_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionUsuarios,
                CrearParametrosFiltroLiquidacion(param));
        }

        /// <summary>
        /// Obtiene la lista de sistemas de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de sistemas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionSistemas_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionSistemas,
                CrearParametrosFiltroLiquidacion(param));
        }

        /// <summary>
        /// Obtiene la lista de tokens de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de tokens.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionTokens_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionTokens,
                CrearParametrosFiltroLiquidacion(param));
        }

        /// <summary>
        /// Obtiene la lista de oficinas de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro: CodEmpresa, FechaDesde, FechaHasta, Estado.</param>
        /// <returns>ErrorDto con la lista de oficinas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionOficinas_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionOficinas,
                CrearParametrosFiltroLiquidacion(param));
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
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<FndTraspasoTesoreriaLiquidacionConsultaResult>());
            }

            var result =  DbHelper.ExecuteListQuery<FndTraspasoTesoreriaLiquidacionConsultaResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionConsulta,
                CrearParametrosConsultaLiquidacion(param));

            return result;
        }

        /// <summary>
        /// Consulta duplicados en la remesa de liquidaciones según filtros.
        /// </summary>
        /// <param name="param">Parámetros de filtro.</param>
        /// <returns>ErrorDto con la lista de duplicados.</returns>
        public ErrorDto<List<FndTraspasoTesoreriaDuplicadosResult>> RevisaDuplicadosEnLaRemesa(FndTraspasoTesoreriaDuplicadosParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<FndTraspasoTesoreriaDuplicadosResult>());
            }

            return DbHelper.ExecuteListQuery<FndTraspasoTesoreriaDuplicadosResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlDuplicadosRemesa,
                CrearParametrosConsultaLiquidacion(param));
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
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(vfiltro, -2, new List<FndTraspasoTesoreriaLiquidacionConsultaResult>());
            }

            return DbHelper.ExecuteListQuery<FndTraspasoTesoreriaLiquidacionConsultaResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlLiquidacionDetalle,
                CrearParametrosDetalleLiquidacion(param));
        }

        private static object CrearParametrosFiltroLiquidacion(FndTraspasoTesoreriaFiltroParams param)
        {
            return new
            {
                Desde = param.FechaDesde.Date,
                Hasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                EstadoPendiente = ObtenerEstadoPendiente(param.Estado),
                AppProductName = ObtenerAppProductName(null)
            };
        }

        private static object CrearParametrosConsultaLiquidacion(FndTraspasoTesoreriaLiquidacionConsultaParams param)
        {
            return CrearParametrosLiquidacionBase(new LiquidacionFiltroBaseParams
            {
                FechaDesde = param.FechaDesde,
                FechaHasta = param.FechaHasta,
                SifParam = param.SifParam,
                Estado = param.Estado,
                Filtros = param.Filtros,
                BancoId = param.BancoId,
                Oficina = param.Oficina,
                Usuario = param.Usuario,
                Sistema = param.Sistema,
                TokenConsulta = param.TokenConsulta,
                AppProductName = param.AppProductName,
                Todos = param.Todos
            });
        }

        private static object CrearParametrosConsultaLiquidacion(FndTraspasoTesoreriaDuplicadosParams param)
        {
            return CrearParametrosLiquidacionBase(new LiquidacionFiltroBaseParams
            {
                FechaDesde = param.FechaDesde,
                FechaHasta = param.FechaHasta,
                SifParam = param.SifParam,
                Estado = param.Estado,
                Filtros = param.Filtros,
                BancoId = param.BancoId,
                Oficina = param.Oficina,
                Usuario = param.Usuario,
                Sistema = param.Sistema,
                TokenConsulta = param.TokenConsulta,
                AppProductName = param.AppProductName
            });
        }

        private static object CrearParametrosDetalleLiquidacion(FndTraspasoTesoreriaDetalleParams param)
        {
            return CrearParametrosLiquidacionBase(new LiquidacionFiltroBaseParams
            {
                FechaDesde = param.FechaDesde,
                FechaHasta = param.FechaHasta,
                SifParam = param.SifParam,
                Estado = param.Estado,
                Filtros = param.Filtros,
                BancoId = param.BancoId,
                Oficina = param.Oficina,
                Usuario = param.Usuario,
                Sistema = param.Sistema,
                TokenConsulta = param.TokenConsulta,
                AppProductName = param.AppProductName,
                Cedula = param.Cedula
            });
        }

        private sealed class LiquidacionFiltroBaseParams
        {
            public DateTime FechaDesde { get; init; }
            public DateTime FechaHasta { get; init; }
            public string? SifParam { get; init; }
            public string? Estado { get; init; }
            public bool Filtros { get; init; }
            public int? BancoId { get; init; }
            public string? Oficina { get; init; }
            public string? Usuario { get; init; }
            public string? Sistema { get; init; }
            public string? TokenConsulta { get; init; }
            public string? AppProductName { get; init; }
            public bool? Todos { get; init; }
            public string? Cedula { get; init; }
        }

        private static object CrearParametrosLiquidacionBase(LiquidacionFiltroBaseParams param)
        {
            return new
            {
                param.Todos,
                Cedula = NormalizarTexto(param.Cedula),
                FechaDesde = param.FechaDesde.Date,
                FechaHasta = param.FechaHasta.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AplicaRevision = NormalizarTexto(param.SifParam) == "S" ? 1 : 0,
                EstadoPendiente = ObtenerEstadoPendiente(param.Estado),
                AplicarFiltros = param.Filtros ? 1 : 0,
                param.BancoId,
                Oficina = NormalizarTexto(param.Oficina),
                Usuario = CrearLikeInicio(param.Usuario),
                Sistema = CrearLikeInicio(param.Sistema),
                TokenConsulta = CrearLikeInicio(param.TokenConsulta),
                AppProductName = ObtenerAppProductName(param.AppProductName)
            };
        }

        private static int? ObtenerEstadoPendiente(string? estado)
        {
            var estadoSeguro = NormalizarTexto(estado);
            if (string.IsNullOrWhiteSpace(estadoSeguro))
            {
                return null;
            }

            return estadoSeguro.StartsWith('P') ? 1 : 0;
        }

        private static string CrearLikeInicio(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? string.Empty : $"{texto}%";
        }

        private static string ObtenerAppProductName(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? AppProductNameDot : texto;
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
