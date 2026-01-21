using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Microsoft.ReportingServices.ReportProcessing.OnDemandReportObjectModel;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Drawing;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosStpDB
    {
        private readonly PortalDB _portalDB;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrx;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly MCajas _mCajas;
        private readonly MCobroDb _mCobro;

        public FrmCajasCrdAbonosStpDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mRecibos = new MRecibos(config);
            _mProGrx = new MProGrxMain(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
            _mCajas = new MCajas(config);
            _mCobro = new MCobroDb(config);
        }

        /// <summary>
        /// Metodo para obtener los documentos de abono
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosSt_Documentos_Obtener(int CodEmpresa, string codCaja)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        RTRIM(C.tipo_documento) AS item,
                        RTRIM(D.Descripcion) AS descripcion
                    FROM SIF_DOCUMENTOS D
                    INNER JOIN CAJAS_DOCUMENTOS C 
                        ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                    WHERE 
                        C.cod_caja = @codCaja
                        AND D.Tipo_Movimiento IN ('A', 'C')
                    ORDER BY C.tipo_documento";

                return conn.Query<DropDownListaGenericaModel>(query, new { codCaja }).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener las operaciones de credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_Operaciones_Obtener(int CodEmpresa)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT 
                        R.id_solicitud ,
                        R.codigo,
                        S.cedula,
                        S.nombre,
                        C.descripcion
                    FROM REG_CREDITOS R
                    INNER JOIN SOCIOS S 
                        ON R.cedula = S.cedula
                    INNER JOIN CATALOGO C 
                        ON R.codigo = C.codigo
                    WHERE 
                        R.estado = 'A'
                    ORDER BY R.cedula";

                return conn.Query<CajasCrdAbonosStPDData>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener parametros de credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametro"></param>
        /// <returns></returns>
        public ErrorDto<int> CajasCrdAbonosSt_fxCrdParametro(int CodEmpresa, string parametro)
        {
            var iDias = MCredito.fxCrdParametro(DbHelper.OpenConnection(_portalDB, CodEmpresa), parametro);
            return DbHelper.CreateOkResponse(iDias != null ? int.Parse(iDias) : 0);
        }


        /// <summary>
        /// Metodo para consultar el credito por numero de operacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCaja"></param>
        /// <param name="OperacionId"></param>
        /// <returns></returns>
        public ErrorDto<CajasCrdAbonosStPDData> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"SELECT
                                                R.id_solicitud,
                                                R.saldo,
                                                R.saldo - ISNULL(V.amortiza, 0) AS Saldo_mes,
                                                R.proceso,
                                                ISNULL(R.cod_Divisa, 'COL') AS Divisa,
                                                R.interesv,
                                                R.int,
                                                R.plazo,
                                                R.interesc,
                                                R.amortiza,
                                                R.fecult,
                                                R.Prideduc,
                                                R.opex,
                                                R.cuota,
                                                R.codigo,
                                                R.cedula,
                                                R.cuotas_planilla,
                                                R.cuotas_directas,
                                                R.montoApr,
                                                S.nombre,
                                                C.descripcion,
                                                C.retencion,
                                                C.poliza,
                                                R.fechaforp,
                                                C.PORC_CARGO_CANCELACION,
                                                R.Base_Calculo,
                                                dbo.fxCajas_Valida_Auxiliar(@CodCaja, 'CRD', R.Codigo) AS Caja_Valida_Concepto
                                            FROM reg_creditos R
                                            INNER JOIN Catalogo C ON R.codigo = C.codigo
                                            INNER JOIN Socios S ON R.cedula = S.cedula
                                            LEFT JOIN vista_morosidad V ON R.id_solicitud = V.id_solicitud
                                            WHERE R.estado = 'A'
                                              AND R.saldo > 0
                                              AND R.ID_SOLICITUD = @OperacionId";

                var op = conn.QueryFirstOrDefault<CajasCrdAbonosStPDData>(query, new { CodCaja, OperacionId }) ?? new CajasCrdAbonosStPDData();

                op.Saldo_mes = op.Saldo_mes < 0 ? 0 : op.Saldo_mes;
                if (op.Saldo_mes == 0)
                {
                    var updateSQl = "update reg_creditos set saldo_mes = saldo where id_solicitud = @id_solicitud";
                    conn.Execute(updateSQl, new { Saldo_mes = op.Saldo_mes, id_solicitud = op.id_solicitud });

                    op.Saldo_mes = op.saldo;
                }

                return op;
            });
        }

        /// <summary>
        /// Metodo para consultar la mora del credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operacion"></param>
        /// <param name="FechaPago"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int CodEmpresa, int Operacion, DateTime FechaPago)
        {
            string sql = "exec spCajas_Crd_MoraConsulta @Operacion, @FechaPago";
            var parameters = new
            {
                Operacion = Operacion,
                FechaPago = FechaPago
            };
            return DbHelper.ExecuteListQuery<CajasCrdAbonoMorosidadData>(_portalDB, CodEmpresa, sql, parameters);
        }

        /// <summary>
        /// Metodo para cargar la operacion por cedula y codigo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CajasCrdAbonoCargaOperacionData> CajasCrdAbonosSt_CargaOperacionCodCed(int CodEmpresa, string cedula, string codigo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT 
                            R.id_solicitud,
                            R.saldo,
                            R.saldo_mes,
                            R.interesv,
                            R.int,
                            R.plazo,
                            R.interesc, 
                            R.amortiza,
                            R.fecult,
                            R.opex,
                            R.cuota,
                            R.codigo,
                            R.cedula,
                            R.cuotas_planilla,
                            R.cuotas_directas,
                            C.retencion,
                            C.poliza
                        FROM reg_creditos R
                        INNER JOIN catalogo C 
                            ON R.codigo = C.codigo
                        WHERE 
                            R.estado = 'A'
                            AND R.proceso <> 'N'
                            AND R.saldo > 0
                            AND R.cedula = @cedula
                            AND R.codigo = @codigo";

                return conn.QueryFirstAsync<CajasCrdAbonoCargaOperacionData>(query, new
                {
                    cedula,
                    codigo
                }).Result ?? new CajasCrdAbonoCargaOperacionData();
            });
        }


        /// <summary>
        /// Metodo para aplicar el abono al credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CajasCrdAbonosSt_Abono_Aplica(int CodEmpresa, CajasCrdAbonoRequest request)
        {
            try
            {

                long vNumDoc = _mRecibos.fxDocumentoConsecutivo(CodEmpresa, request.tipoDoc);

                decimal glngFechaCR = _mProGrx.glngFechaCR(CodEmpresa);

                if (request.lblFecUltMovR < glngFechaCR)
                {
                    request.lblFecUltMovR = (long)glngFechaCR;
                }
                var sql = @"exec spCajas_CrdAbono @Operacion, 
									@Abono , 
									@TipoDoc, 
									@NumDoc , 
									@Concepto ,
									@Usuario , 
									@Caja , 
									@Apertura, 
									@Recalcula,
									@CargoAnticipo , 
									@IntExtra ,
									@FechaPagoReal ";
                var parameters = new
                {
                    Operacion = request.id_solicitud,
                    Abono = request.totalCajs,
                    TipoDoc = request.tipoDoc,
                    NumDoc = vNumDoc,
                    Concepto = request.concepto,
                    Usuario = request.mUsuario,
                    Caja = request.mCaja,
                    Apertura = request.mApertura,
                    Recalcula = request.chkRecalculaCuota,
                    CargoAnticipo = request.datosAnticipo,
                    IntExtra = (request.tipo == "E") ? request.datosInteres : 0,
                    FechaPagoReal = request.FechaCancelacion
                };

                var result = DbHelper.ExecuteSingleQuery<dynamic>(_portalDB, CodEmpresa, sql, parameters).Result;
                if (result!.Pendiente > 0)
                {
                    return DbHelper.CreateErrorResponse("Quedó un monto pendiente de :" + result.Pendiente);
                }
            }
            catch (Exception ez)
            {
                return DbHelper.ErrorResponse("Error al aplicar el abono: " + ez.Message, -1);
            }

            return DbHelper.CreateOkResponse();

        }

        private ErrorDto fxVerifica(SqlConnection conn, CajasCrdAbonosStPDData request)
        {
            try
            {
                string mensaje = "";
                string vNotas = MProGrxMain.sbSIFCleanTxtInject(request.descripcion);

                //Verifica el proceso
                if (!VerificaProceso(conn, request))
                {
                    mensaje += "- Esta CAJA no cuenta con permisos para realizar abonos a Creditos en Cobro Judicial, verifique...";
                }

                return DbHelper.OkResponse(mensaje + vNotas);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Error en fxVerifica: " + ex.Message, -1);

            }
        }

        private bool VerificaProceso(SqlConnection conn, CajasCrdAbonosStPDData request)
        {
            try
            {
                if (fxVerifica(conn, request).Code == -1)
                {
                    return false;
                }

                if (request.proceso == "J")
                {
                    var sql = "select dbo.fxCajas_AbonoCbrJudAutorizada(@pCaja,@usuario) as Valor";
                    var result = conn.QueryFirstOrDefault<bool>(sql, new { pCaja = request.codigo, usuario = request.cedula });
                    return result;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public ErrorDto Bitacora(int CodEmpresa, string usuario, string detalle)
        {
            return _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = "Registra - WEB",
                Modulo = 5
            });
        }

        public ErrorDto sbDocumentoAbono(int CodEmpresa, CajasCrdAbonosStPDData solicitud, CajasCrdAbonosStpVariables variable)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                decimal pTipoCambio = _mCajas.fxCajasTipoCambio(CodEmpresa, 0, variable.vTipoDoc!);
                variable.tipoCambio = pTipoCambio;

                var DocAfectacion = spCrdDocumentoAfectacionStP(CodEmpresa, variable.vTipoDoc!, (long)variable.vNumDoc!, "R");

                var CuentaOperacion = spCrdOperacionCtas(CodEmpresa, (long)variable.id_solicutud!);

                var lineas = BuildLineas(DocAfectacion, variable, solicitud);

                //Control de Documentos v2
                var doc = ControlDocumentosV2_RegistrarAsync(CodEmpresa, solicitud, variable, DocAfectacion, CuentaOperacion, lineas);

                if (doc.Result.Code == -1)
                {
                    return DbHelper.ErrorResponse("Error al registrar el documento de abono: " + doc.Result.Description, -1);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("Error al registrar el documento de abono", -1);
            }

        }

        private CajasCrdAbonoAfectacionData spCrdDocumentoAfectacionStP(int CodEmpresa, string vTipoDoc, long vNumDoc, string Formato)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                return conn.QueryFirstOrDefault<CajasCrdAbonoAfectacionData>("exec spCrdDocumentoAfectacionStP @vTipoDoc, @vNumDoc, @Formato",
                    new { vTipoDoc, vNumDoc, Formato }) ?? new CajasCrdAbonoAfectacionData();
            }
            catch (Exception)
            {
                return new CajasCrdAbonoAfectacionData();
            }
        }

        private CajasCrdAbonooperacionCtas spCrdOperacionCtas(int CodEmpresa, long Operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                return conn.QueryFirstOrDefault<CajasCrdAbonooperacionCtas>("exec exec spCrdOperacionCtas @Operacion",
                    new { Operacion }) ?? new CajasCrdAbonooperacionCtas();
            }
            catch (Exception)
            {
                return new CajasCrdAbonooperacionCtas();
            }
        }

        private static string[] BuildLineas(
            CajasCrdAbonoAfectacionData DocAfectacion,
            CajasCrdAbonosStpVariables variable,
            CajasCrdAbonosStPDData solicitud)
        {
            var saldoActual = variable.vSaldoMes - DocAfectacion.Principal;

            string Fill(string label, decimal value) =>
                $"{label} ..: {value:#,0.00}"; // Reemplaza por tu fxStringRelleno si lo necesitas

            bool vRetencion = (solicitud.retencion == "N");

            var l = new string[12]; // usaremos 1..11 para mimetizar VB6
            l[1] = Fill("Saldo Anterior", (decimal)variable.vSaldoMes!);
            l[2] = Fill("Saldo Actual", (decimal)saldoActual!);
            l[3] = Fill("Interes Corriente", DocAfectacion.IntCor);
            l[4] = Fill("Interes Atrasado", DocAfectacion.IntMor);
            l[5] = Fill("Amortización", DocAfectacion.Principal);
            l[6] = Fill("Cargos Totales", DocAfectacion.Cargos);
            l[7] = Fill("Pólizas", DocAfectacion.Polizas);

            l[8] = $"Operacion/Línea   ..: Op.:{solicitud.id_solicitud} L.:{solicitud.codigo}-{(solicitud.opex.ToString() ?? string.Empty).ToUpperInvariant()}";
            l[9] = $"Descripción       ..: {solicitud.descripcion}";
            l[10] = $"Proc. Retencion   ..: {(vRetencion ? "SI" : "NO")}";
            l[11] = variable.FechaCancelacionEnable
                ? $"Fecha Real Abono {variable.FechaCancelacion:dd/MM/yyyy}"
                : string.Empty;

            return l;
        }


        public async Task<ErrorDto> ControlDocumentosV2_RegistrarAsync(
                 int codEmpresa,
                CajasCrdAbonosStPDData solicitud,
                CajasCrdAbonosStpVariables vars,
                CajasCrdAbonoAfectacionData afectacion,
                CajasCrdAbonooperacionCtas CuentaOperacion,
                string?[] lineas
            )
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (vars.vNumDoc is null || string.IsNullOrWhiteSpace(vars.vTipoDoc))
                    throw new InvalidOperationException("vNumDoc y vTipoDoc son requeridos.");

                if (lineas == null || lineas.Length < 11)
                    throw new InvalidOperationException("Se requieren 11 líneas (linea1..linea11).");

                // Total VB: curIntC + curIntM + curAmortiza + curCargo (ojo: VB no sumó poliza en el insert)
                var montoTransaccion = afectacion.IntCor + afectacion.IntMor + afectacion.Principal + afectacion.Cargos;

                using var tx = await conn.BeginTransactionAsync();

                // 1) INSERT SIF_TRANSACCIONES (parametrizado)
                const string sqlInsert = @"
                            INSERT INTO SIF_TRANSACCIONES
                            (
                                COD_TRANSACCION,
                                TIPO_DOCUMENTO,
                                REGISTRO_FECHA,
                                REGISTRO_USUARIO,
                                Cliente_IDENTIFICACION,
                                CLIENTE_NOMBRE,
                                cod_concepto,
                                monto,
                                estado,
                                Referencia_01,
                                Referencia_02,
                                Referencia_03,
                                cod_oficina,
                                linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,linea9,linea10,linea11,
                                detalle,
                                documento,
                                cod_caja,
                                cod_apertura
                            )
                            VALUES
                            (
                                @CodTransaccion,
                                @TipoDocumento,
                                dbo.MyGetdate(),
                                @Usuario,
                                @ClienteId,
                                @ClienteNombre,
                                @CodConcepto,
                                @Monto,
                                'P',
                                @Ref01,
                                @Ref02,
                                @Ref03,
                                @CodOficina,
                                @Linea1,@Linea2,@Linea3,@Linea4,@Linea5,@Linea6,@Linea7,@Linea8,@Linea9,@Linea10,@Linea11,
                                @Detalle,
                                @Documento,
                                @CodCaja,
                                @CodApertura
                            );";


                await conn.ExecuteAsync(
                    sqlInsert,
                    new
                    {
                        CodTransaccion = vars.vNumDoc.Value.ToString(), // en VB iba como string concatenado
                        TipoDocumento = vars.vTipoDoc,
                        Usuario = vars.usuarioRegistro,
                        ClienteId = (solicitud.cedula ?? string.Empty).Trim(),
                        ClienteNombre = (solicitud.nombre ?? string.Empty).Trim(),
                        CodConcepto = vars.vConcepto,
                        Monto = montoTransaccion,
                        Ref01 = solicitud.id_solicitud,
                        Ref02 = solicitud.codigo,
                        Ref03 = vars.vNumDoc,
                        CodOficina = vars.oficinaTitular,
                        Linea1 = lineas[0] ?? "",
                        Linea2 = lineas[1] ?? "",
                        Linea3 = lineas[2] ?? "",
                        Linea4 = lineas[3] ?? "",
                        Linea5 = lineas[4] ?? "",
                        Linea6 = lineas[5] ?? "",
                        Linea7 = lineas[6] ?? "",
                        Linea8 = lineas[7] ?? "",
                        Linea9 = lineas[8] ?? "",
                        Linea10 = lineas[9] ?? "",
                        Linea11 = lineas[10] ?? "",
                        Detalle = vars.notas ?? vars.detalle ?? "",
                        Documento = vars.vNumDoc,
                        CodCaja = vars.codCaja,
                        CodApertura = vars.codApertura
                    },
                    tx
                );

                // Helper local para spSIFDocsAsiento (equivalente a los 4 IF del VB)
                async Task ExecAsientoAsync(decimal monto, string cuenta)
                {
                    if (monto <= 0) return;

                    // En VB: monto * pTipoCambio, 'C', rs!cod_Divisa, pTipoCambio, gEnlace,
                    //        rs!Cod_Unidad, rs!Cod_Centro_Costo, cuentaX, rs!Id_Solicitud, rs!Codigo, vAseDocDeposito

                    const string sp = "spSIFDocsAsiento";

                    var p = new DynamicParameters();
                    p.Add("@TipoDocumento", vars.vTipoDoc, DbType.String);
                    p.Add("@CodTransaccion", vars.vNumDoc.Value.ToString(), DbType.String);
                    p.Add("@Monto", monto * vars.tipoCambio, DbType.Decimal);
                    p.Add("@Tipo", "C", DbType.String);
                    p.Add("@Divisa", solicitud.Divisa, DbType.String);
                    p.Add("@TipoCambio", vars.tipoCambio, DbType.Decimal);
                    p.Add("@Enlace", vars.enlace, DbType.Int32);


                    p.Add("@CodUnidad", vars.unidadCaja, DbType.String);         // VB: rs!Cod_Unidad (aprox)
                    p.Add("@CodCentroCosto", "", DbType.String);            // VB: rs!Cod_Centro_Costo (pendiente)

                    p.Add("@Cuenta", cuenta, DbType.String);                // VB: rs!ctaintc / ctaintm / CtaCargos / ctaamortiza
                    p.Add("@IdSolicitud", solicitud.id_solicitud, DbType.Int64);
                    p.Add("@Codigo", solicitud.codigo, DbType.String);
                    p.Add("@Documento", vars.vNumDoc, DbType.String);

                    await conn.ExecuteAsync(sp, p, tx, commandType: CommandType.StoredProcedure);
                }

                // 2) Asientos (según montos)
                var ctaIntC = CuentaOperacion.cta_int_c;
                var ctaIntM = CuentaOperacion.cta_int_m;
                var ctaCargo = CuentaOperacion.cta_cargos;
                var ctaAmort = CuentaOperacion.cta_amortiza;

                await ExecAsientoAsync(afectacion.IntCor, ctaIntC);
                await ExecAsientoAsync(afectacion.IntMor, ctaIntM);
                await ExecAsientoAsync(afectacion.Cargos, ctaCargo!);
                await ExecAsientoAsync(afectacion.Principal, ctaAmort);

                // 3) Desgloce pagos (VB: incluye poliza en condición)
                if ((afectacion.IntCor + afectacion.IntMor + afectacion.Principal + afectacion.Cargos) > 0)
                {
                    const string spPagos = "spCajas_DesglocePagosDocFinal";

                    var pPago = new DynamicParameters();
                    pPago.Add("@Caja", vars.codCaja, DbType.Int32);
                    pPago.Add("@Apertura", vars.codApertura, DbType.Int32);
                    pPago.Add("@Tiquete", vars.tiquete, DbType.String);
                    pPago.Add("@Usuario", vars.usuarioRegistro, DbType.String);
                    pPago.Add("@TipoDocumento", vars.vTipoDoc, DbType.String);
                    pPago.Add("@CodTransaccion", vars.vNumDoc.Value.ToString(), DbType.String);
                    pPago.Add("@Unidad", vars.unidadCaja, DbType.String);
                    pPago.Add("@IdSolicitud", solicitud.id_solicitud, DbType.Int64);
                    pPago.Add("@Codigo", solicitud.codigo, DbType.String);

                    await conn.ExecuteAsync(spPagos, pPago, tx, commandType: CommandType.StoredProcedure);
                }

                // 4) Commit
                await tx.CommitAsync();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


        public ErrorDto<decimal> fxFechaProcesoSiguiente(int CodEmpresa, decimal pProceso)
        {
            return new ErrorDto<decimal>()
            {
                Code = 0,
                Description = "Ok",
                Result = _mCobro.fxFechaProcesoSiguiente(CodEmpresa, pProceso)
            };
        }

        public ErrorDto<decimal> fxCalcula_Cuota(decimal Monto, int Plazo, object Interes, string? Frecuencia = "M")
        {
            return new ErrorDto<decimal>()
            {
                Code = 0,
                Description = "Ok",
                Result = MCobroDb.fxCalcula_Cuota(Monto, Plazo, Interes, Frecuencia)
            };
        }
    }
}
