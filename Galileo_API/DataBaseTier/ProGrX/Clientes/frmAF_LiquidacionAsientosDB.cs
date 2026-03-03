using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfLiquidacionAsientosDB
    {
        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _main;
        private readonly MTesoreria _mtes;

        public FrmAfLiquidacionAsientosDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
            _main = new MProGrxMain(config);
            _mtes = new MTesoreria(config);
        }

        /// <summary>
        /// Método: Obtiene los tipos de asiento para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqAsientosTipo_Obtener(int CodEmpresa, string accion)
        {
            string query = accion switch
            {
                "D" => @"
                    select id_banco as Item, rtrim(descripcion) + '  ' + rtrim(Cta) as descripcion
                    from Tes_Bancos
                    where estado = 'A'",
                "R" => @"
                    select RTRIM(RETENCION_CODIGO) as Item,
                           RTRIM(RETENCION_CODIGO) + ' - ' + rtrim(descripcion) + ' [' + rtrim(COD_CUENTA) + ']' as descripcion
                    from FND_RETENCION_CONCEPTOS
                    where ACTIVO = 1",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
                return DbHelper.CreateErrorResponse("Acción no válida para obtener tipos de asiento.", -1, new List<DropDownListaGenericaModel>());

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, CodEmpresa, query);
        }

        /// <summary>
        /// Método: Obtiene los tokens disponibles para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TokenConsultaModel>> AF_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// Método: Genera un nuevo token para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, usuario);
        }

        /// <summary>
        /// Busca Liquidaciones pendientes o generadas con ubicacion en tesoreria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<LiquidacionAsientoModel>> AF_LiquidacionAsiento_Obtener(int CodEmpresa, FiltrosSolicitud filtros)
        {
            var response = DbHelper.CreateOkResponse(new List<LiquidacionAsientoModel>());
            try
            {
                var (sql, p) = BuildLiquidacionAsientoSql(CodEmpresa, filtros);

                using var connection = _portalDb.CreateConnection(CodEmpresa);
                response.Result = connection.Query<LiquidacionAsientoModel>(sql, p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }



        private (string sql, DynamicParameters p) BuildLiquidacionAsientoSql(int codEmpresa, FiltrosSolicitud filtros)
        {
            var p = new DynamicParameters();
            p.Add("@fechaInicio", filtros.fechaInicio);
            p.Add("@fechaFin", filtros.fechaFin);
            p.Add("@todos", filtros.chkTodos ? 1 : 0);

            var builder = new WhereSqlBuilder();

            // Base mandatory conditions
            builder.Where("L.FecLiq between @fechaInicio and @fechaFin");
            builder.Where("L.Ubicacion = 'T'");
            builder.Where("L.Estado = 'P'");

            ApplyAnalistaRevision(builder, codEmpresa);
            ApplyAccionTipo(builder, p, filtros);
            ApplyEstadoAsiento(builder, filtros);
            ApplyTipoRenuncia(builder, filtros);
            ApplyFiltrosOpcionales(builder, p, filtros);

            string sql = builder.ApplyWhereTemplate(LiquidacionAsientoTemplateSql);
            return (sql, p);
        }

        private void ApplyAnalistaRevision(WhereSqlBuilder builder, int codEmpresa)
        {
            if (_main.FxSIFParametros(codEmpresa, "15") == "S")
                builder.Where("L.Analista_Revision = 'S'");
        }

        private static void ApplyAccionTipo(WhereSqlBuilder builder, DynamicParameters p, FiltrosSolicitud filtros)
        {
            if (filtros.accion == "D" && filtros.tipo != "T")
            {
                builder.Where("L.cod_Banco = @codBanco");
                p.Add("@codBanco", filtros.tipo);
            }
        }

        private static void ApplyEstadoAsiento(WhereSqlBuilder builder, FiltrosSolicitud filtros)
        {
            builder.Where(filtros.estado == "P" ? "L.EstadoAsiento = 'P'" : "L.EstadoAsiento = 'G'");
        }

        private static void ApplyTipoRenuncia(WhereSqlBuilder builder, FiltrosSolicitud filtros)
        {
            if (filtros.tipoRenuncia == "T")
                return;

            builder.Where(filtros.tipoRenuncia == "A" ? "L.ESTADOACTLIQ = 'A'" : "L.ESTADOACTLIQ = 'P'");
        }

        private static void ApplyFiltrosOpcionales(WhereSqlBuilder builder, DynamicParameters p, FiltrosSolicitud filtros)
        {
            if (!filtros.chkFiltros)
                return;

            if (filtros.id_banco != null)
            {
                builder.Where("L.cod_Banco = @fIdBanco");
                p.Add("@fIdBanco", filtros.id_banco);
            }

            if (!string.IsNullOrWhiteSpace(filtros.cod_oficina))
            {
                builder.Where("L.cod_oficina = @codOficina");
                p.Add("@codOficina", filtros.cod_oficina);
            }

            if (!string.IsNullOrWhiteSpace(filtros.usuario))
            {
                builder.Where("L.usuario = @usr");
                p.Add("@usr", filtros.usuario);
            }

            if (!string.IsNullOrWhiteSpace(filtros.id_token))
            {
                builder.Where("isnull(L.ID_Token,'') like @idToken");
                p.Add("@idToken", $"{filtros.id_token}%");
            }
        }

        private const string LiquidacionAsientoTemplateSql = @"Select @todos as valor,
                           L.consec,
                           S.cedula,
                           S.nombre,
                           L.TNeto,
                           L.cod_banco,
                           L.TDocumento,
                           case when L.EstadoActLiq = 'A' then 'Ren.Asociación'
                                when L.EstadoActLiq = 'P' then 'Ren.Patronal' end as Tipo,
                           isnull(L.cta_ahorros,0) as Cuenta,
                           L.FecLiq,
                           L.usuario,
                           B.Descripcion,
                           dbo.fxTesSupervisa(L.cedula,S.nombre,L.TNeto,0,'L') as Duplicado,
                           TES_SUPERVISION_FECHA,
                           isnull(B.Cod_Divisa,'') as Cod_Divisa,
                           L.Id_Token
                    from Liquidacion L
                    inner join Socios S on L.cedula = S.cedula
                    left join Tes_Bancos B on L.cod_Banco = B.id_Banco
                    /**where**/
                    order by L.consec";


        /// <summary>
        /// OBJETIVO:      Genera a Tesoreria las liquidaciones.
        /// 'REFERENCIAS:   AsientoLiquidacionTesoreria - (Genera el Asiento de la liquidacion en el
        /// '               modulo de Tesoreria)
        /// '               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        /// '               Procedimiento)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="token"></param>
        /// <param name="usuario"></param>
        /// <param name="liquidaciones"></param>
        /// <returns></returns>
        public ErrorDto Af_LiquidacionAsiento_Generar(int CodEmpresa, string usuario, FiltrosSolicitud filtros ,List<LiquidacionAsientoModel> liquidaciones)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0 };
            try
            {
                var msjError = new System.Text.StringBuilder();

                string vfecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                using (var connection = new SqlConnection(conn))
                {
                    foreach (var liq in liquidaciones)
                    {
                        if (liq.duplicado == 0)
                        {
                            if (filtros.accion == "D")
                            {
                                ErrorDto res = sbTesoreria(CodEmpresa, usuario, "OC", vfecha, filtros.token ?? string.Empty, liq);
                                if(res.Code == -1)
                                {
                                    msjError.AppendLine(res.Description);
                                }
                            }
                            else
                            {
                                //'Retener
                                var strDup = $@"Update Liquidacion set Fecha_Traspaso= dbo.MyGetdate(),EstadoAsiento = 'G',NDocumento= '0',Tdocumento = 'RT'
                                           ,Tesoreria_Solicitud = 0, Traspaso_Usuario = @usuario
                                            Where Consec = @consec";

                                connection.Execute(strDup, new { usuario = usuario, consec = liq.consec });
                            }
                        }
                    }

                    response.Code = msjError.Length == 0 ? 0 : -1;
                    response.Description = msjError.Length == 0 ? "OK" : msjError.ToString();
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
        /// 'OBJETIVO:      Genera el Asiento de la liquidacion en el modulo de Tesoreria, junto con su
        /// '               detalle y cambia el estado del asiento de la liquidacion a generado.
        /// 'REFERENCIAS:   fxFechaServidor - (Devuelve la fecha del servidor)
        /// 'OBSERVACIONES: Verifica que el monto al Debe este equilibrado con el monto al Haber.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vFecha"></param>
        /// <param name="pToken"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private ErrorDto sbTesoreria(int CodEmpresa,string usuario, string oficina ,string vFecha, string pToken, LiquidacionAsientoModel row)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0 };
            try
            {
                decimal curMonto = 0;
                string vDivisa = row.cod_divisa ?? string.Empty; //'Divisa del Banco
                decimal curDebitos = 0;
                decimal curCreditos = 0;
                long lngSolicitud = 0;
                string oDivisa = "";
                decimal oTipoCambio = 1;

                using (var connection = new SqlConnection(conn))
                {
                    var query = $@"select COD_DIVISA, isnull(TIPO_CAMBIO,1) as 'TIPO_CAMBIO' FROM LIQUIDACION WHERE CONSEC =  @consec";
                    var result = connection.QueryFirstOrDefault<(string CodDivisa, decimal TipoCambio)>(
                           query,
                           new { consec = row.consec }
                       );

                    if (result != default)
                    {
                        oDivisa = result.CodDivisa;
                        oTipoCambio = result.TipoCambio;
                    }

                    //'Control de Documentos v2
                    query = $@"Select isnull(SUM(MONTO),0) as MONTO 
                                    From SIF_Transacciones_Asiento 
                                    Where  Tipo_Documento = 'LIQ' and cod_transaccion = @cod_transaccion  And Tipo_Movimiento ='D'";
                    curDebitos = connection.QueryFirstOrDefault<decimal>(query, new { cod_transaccion = row.consec });

                    query = $@"Select isnull(SUM(MONTO),0) as MONTO 
                                    From SIF_Transacciones_Asiento 
                                     Where  Tipo_Documento = 'LIQ' and cod_transaccion = @cod_transaccion  And Tipo_Movimiento ='C'";
                    curCreditos = connection.QueryFirstOrDefault<decimal>(query, new { cod_transaccion = row.consec });


                    if (curDebitos != curCreditos)
                    {
                        response.Code = -1;
                        response.Description = $"La liquidación No. {row.consec}  -> No se Emite a Tesoreria porque se encuentra desbalanceada.!";
                        return response;
                    }
                    else
                    {
                        if (curCreditos == 0 || curDebitos == 0)
                        {
                            response.Code = -1;
                            response.Description = $"La liquidación No. {row.consec}  -> No se Emite a Tesoreria el asiento contable presenta problemas o no existe.!";
                            return response;
                        }
                    }

                    curMonto = row.valor;

                    //Revisar la Conversion en Multi Divisa
                    vTipoCambio = oTipoCambio;

                    curMonto = curMonto/ Convert.ToDecimal(MProGrxMain.fxSys_Tipo_Cambio_Apl(vTipoCambio));

                    //Busco concepto de afiliacion
                    query = $@"select VALOR from SIF_PARAMETROS where COD_PARAMETRO = 'AFICL'";
                    var concepto = connection.QueryFirstOrDefault<string>(query);

                    // 'Registra
                    query = $@"Insert Tes_Transacciones(
                                        ID_Banco,
                                        Tipo,
                                        Codigo,
                                        Beneficiario,
                                        Monto,
                                        Fecha_Solicitud,
                                        Estado,
                                        EstadoI,
                                        Modulo,
                                        Cta_Ahorros,
                                        Detalle1,
                                        Detalle2,
                                        Detalle3,
                                        Detalle4,
                                        Detalle5,
                                        SubModulo,
                                        Actualiza,
                                        cod_unidad,
                                        cod_concepto,
                                        user_solicita,
                                        ID_TOKEN ,
                                        REMESA_TIPO, 
                                        REMESA_ID, 
                                        COD_DIVISA, 
                                        TIPO_CAMBIO, 
                                        COD_APP )
                                        VALUES (
                                        @ID_Banco,
                                        @Tipo,
                                        @Codigo,
                                        @Beneficiario,
                                        @Monto,
                                        @Fecha_Solicitud,
                                        @Estado,
                                        @EstadoI,
                                        @Modulo,
                                        @Cta_Ahorros,
                                        @Detalle1,
                                        @Detalle2,
                                        @Detalle3,
                                        @Detalle4,
                                        @Detalle5,
                                        @SubModulo,
                                        @Actualiza,
                                        @cod_unidad,
                                        @cod_concepto,
                                        @user_solicita,
                                        @ID_TOKEN ,
                                        @REMESA_TIPO, 
                                        @REMESA_ID, 
                                        @COD_DIVISA, 
                                        @TIPO_CAMBIO, 
                                        @COD_APP
                                        )";

                    connection.Execute(query, new 
                    {
                        ID_Banco = row.cod_banco,
                        Tipo = row.tipo,
                        Codigo = row.consec,
                        Beneficiario = row.cedula,
                        Monto = curMonto,
                        Fecha_Solicitud = vFecha,
                        Estado = "P",
                        EstadoI = "P",
                        Modulo = "CC",
                        Cta_Ahorros = row.cuenta,
                        Detalle1 = "LIQ. DE PERSONA-AFILIACION",
                        Detalle2 = $"'#Liq: {row.consec}",
                        Detalle3 = $"'Tipo: {row.tdocumento}",
                        Detalle4 = $"'Fecha: {row.fecliq}",
                        Detalle5 = $"'Usuario: {row.usuario}",
                        SubModulo = "A",
                        Actualiza = "S",
                        cod_unidad = oficina,
                        cod_concepto = concepto,
                        user_solicita = usuario,
                        ID_TOKEN = pToken,
                        REMESA_TIPO = "LIQ",
                        REMESA_ID = 0,
                        COD_DIVISA = vDivisa,
                        TIPO_CAMBIO = vTipoCambio,
                        COD_APP = "ProGrX"
                    });

                    query = $@"Select Max(NSolicitud) as Solicitud from Tes_Transacciones Where Codigo = @codigo  and Fecha_Solicitud = @fechSolicitud ";
                    lngSolicitud = connection.QueryFirstOrDefault<long>(query, new { codigo = row.consec, fechSolicitud = vFecha });

                    //-Asiento
                    curMonto = curMonto * Convert.ToDecimal(MProGrxMain.fxSys_Tipo_Cambio_Apl(vTipoCambio));

                    query = $@"Select CTACONTA From Tes_Bancos where id_banco= @id_banco";
                    var vCtaBanco = connection.QueryFirstOrDefault<string>(query, new { id_banco = row.cod_banco });

                    if (!string.IsNullOrEmpty(vCtaBanco))
                    {
                        query = $@"Insert Into Tes_Trans_Asiento(NSolicitud,Cuenta_Contable,Monto,DebeHaber,Linea,COD_UNIDAD,Tipo_Cambio,cod_Divisa) 
                                                  Values
                                                                 (@NSolicitud,@Cuenta_Contable,@Monto,@DebeHaber,@Linea,@COD_UNIDAD,@Tipo_Cambio,@cod_Divisa)";

                        connection.Execute(query, new {
                            NSolicitud = lngSolicitud,
                            Cuenta_Contable = vCtaBanco,
                            Monto = curMonto,
                            DebeHaber = "H",
                            Linea = 1,
                            COD_UNIDAD = oficina,
                            Tipo_Cambio = vTipoCambio,
                            cod_Divisa = vDivisa
                        });
                    }

                    query = $@"Select CTA_LIQPAS From Par_AfAH Where cod_Divisa = @cod_divisa";
                    var vCtaLiqPas = connection.QueryFirstOrDefault<string>(query, new { cod_divisa = oDivisa });
                    if (!string.IsNullOrEmpty(vCtaLiqPas))
                    {
                        query = $@"Insert Into Tes_Trans_Asiento(NSolicitud,Cuenta_Contable,Monto,DebeHaber,Linea,COD_UNIDAD,Tipo_Cambio,cod_Divisa) 
                                                  Values
                                                                 (@NSolicitud,@Cuenta_Contable,@Monto,@DebeHaber,@Linea,@COD_UNIDAD,@Tipo_Cambio,@cod_Divisa)";

                        connection.Execute(query, new {
                            NSolicitud = lngSolicitud,
                            Cuenta_Contable = vCtaLiqPas,
                            Monto = curMonto,
                            DebeHaber = "D",
                            Linea = 2,
                            COD_UNIDAD = oficina,
                            Tipo_Cambio = oTipoCambio,
                            cod_Divisa = oDivisa
                        });
                    }

                    query = @"Update Liquidacion
                              set EstadoAsiento = 'G',
                                  Fecha_Traspaso = dbo.MyGetdate(),
                                  Traspaso_Usuario = @Traspaso_Usuario,
                                  ID_TOKEN = @token,
                                  Tesoreria_Solicitud = @nSolicitud
                              where consec = @consec";
                    connection.Execute(query, new { Traspaso_Usuario = usuario, token = pToken, nSolicitud = lngSolicitud, consec = row.consec });

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"La liquidación No. {row.consec}  -> {ex.Message}!";
            }
            return response;
        }

        private sealed class WhereSqlBuilder
        {
            private readonly List<string> _where = new();

            public void Where(string predicate)
            {
                if (!string.IsNullOrWhiteSpace(predicate))
                    _where.Add(predicate);
            }

            public string ApplyWhereTemplate(string templateSql)
            {
                // Template uses the marker /**where**/ which will be replaced.
                // All predicates are constant strings; runtime values are passed via parameters (Dapper).
                if (_where.Count == 0)
                    return templateSql.Replace("/**where**/", string.Empty);

                string whereSql = "where " + string.Join(" and ", _where);
                return templateSql.Replace("/**where**/", whereSql);
            }
        }
    }
}