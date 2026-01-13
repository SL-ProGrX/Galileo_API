using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosStpDB
    {
        private readonly PortalDB _portalDB;

        public FrmCajasCrdAbonosStpDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
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
                        R.id_solicitud AS operacion,
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
        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
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

                var op = conn.Query<CajasCrdAbonosStPDData>(query, new { CodCaja, OperacionId }).ToList();

                foreach (var item in op)
                {
                    item.Saldo_mes = item.Saldo_mes < 0 ? 0 : item.Saldo_mes;
                    if (item.Saldo_mes == 0)
                    {
                      var updateSQl = "update reg_creditos set saldo_mes = saldo where id_solicitud = @id_solicitud";
                      conn.Execute(updateSQl, new { Saldo_mes = item.Saldo_mes, id_solicitud = item.id_solicitud });

                      item.Saldo_mes = item.saldo;
                    }
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


        public ErrorDto<List<CajasCrdAbonoCargaOperacionData>> CajasCrdAbonosSt_CargaOperacionCodCed(int CodEmpresa, string cedula, string codigo)
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

                return conn.Query<CajasCrdAbonoCargaOperacionData>(query).ToList();
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
                    NumDoc = request.numDoc,
                    Concepto = request.concepto,
                    Usuario = request.mUsuario,
                    Caja = request.mCaja,
                    Apertura = request.mApertura,
                    Recalcula = request.chkRecalculaCuota,
                    CargoAnticipo = request.datosAnticipo,
                    IntExtra = request.datosInteres,
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

        private ErrorDto fxVerifica(int CodEmpresa, CajasCrdAbonosStPDData request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string mensaje = "";
                string vNotas = MProGrxMain.sbSIFCleanTxtInject(request.descripcion);

                //Verifica el proceso
                if (!VerificaProceso(conn, request))
                {
                    mensaje += "- Esta CAJA no cuenta con permisos para realizar abonos a Creditos en Cobro Judicial, verifique...";
                }

                return DbHelper.CreateOkResponse();
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
                if (request.proceso == "J")
                {
                    var sql = "select dbo.fxCajas_AbonoCbrJudAutorizada(@pCaja,@usuario) as Valor";
                    var result = conn.QueryFirstOrDefault<int>(sql, new { pCaja = request.codigo, usuario = request.cedula });
                    return result == 1 ? true : false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        private bool VerificaCongelamiento(SqlConnection conn, CajasCrdAbonosStPDData request)
        {
            try
            {
                if (request.proceso == "J")
                {
                    var sql = "select dbo.fxCajas_AbonoCbrJudAutorizada(@pCaja,@usuario) as Valor";
                    var result = conn.QueryFirstOrDefault<int>(sql, new { pCaja = request.codigo, usuario = request.cedula });
                    return result == 1 ? true : false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

    }
}
