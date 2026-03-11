using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAnulacionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora; 
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrx;
        private const int vModulo = 31;

        public FrmCxCCuentasAnulacionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
            _mRecibos = new MRecibos(config);
            _mProGrx = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene los datos de la operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxcOperacionAnulacionData?> CxcOperacion_Obtener(int codEmpresa, int operacion)
        {
            var sql = @"
                select
                    R.Operacion as operacion,
                    R.saldo as saldo,
                    R.num_documento as num_documento,
                    R.Tasa_Corriente as tasa_corriente,
                    R.dias_plazo as dias_plazo,
                    R.interesc as interesc,
                    R.amortiza as amortiza,
                    R.Fecha_UltMov as fecha_ultmov,
                    R.Cod_Concepto as cod_concepto,
                    R.cedula as cedula,
                    S.nombre as nombre,
                    C.descripcion as descripcion,
                    R.Activa_Fecha as activa_fecha,
                    R.Tipo_Plazo as tipo_plazo,
                    R.Proceso as proceso
                from CxC_Cuentas R
                inner join CxC_Conceptos C
                    on R.Cod_Concepto = C.Cod_Concepto
                inner join CxC_Personas S
                    on R.cedula = S.cedula
                where R.estado in ('A','C')
                  and R.Operacion = @operacion";

            return DbHelper.ExecuteSingleQuery<CxcOperacionAnulacionData>(
                _portalDb,
                codEmpresa,
                sql,
                default,
                new { operacion }
            );
        }

        /// <summary>
        /// Obtiene la lista de movimientos de una operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CxcOperacionMovimientoData>> CxcOperacionMovimientos_Lista_Obtener(int codEmpresa, int operacion)
        {
            var sql = @"
                select
                    linea as linea,
                    estado as estado,
                    dias as dias,
                    dias_mora as dias_mora,
                    mov_int_cor as mov_intcor,
                    mov_int_mor as mov_intmor,
                    mov_principal as mov_principal,
                    mov_cargos as mov_cargos
                from CXC_CUENTAS_MOV
                where estado = 'C'
                  and operacion = @operacion
                order by linea desc";

            return DbHelper.ExecuteListQuery<CxcOperacionMovimientoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { operacion }
            );
        }

        /// <summary>
        /// Anula operacion de abono a cuentas 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CxcCuentasAbono_Anular(int codEmpresa, CxcAbonoAnularParams req)
        {
            try
            {
                const string vTipoDoc = "CxC_ND";
                var fecha = DateTime.Now;
                string vCuenta = _mRecibos.FxDocumentoCuenta(codEmpresa, vTipoDoc);
                if (string.IsNullOrWhiteSpace(vCuenta?.Trim()))
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = "No se puede realizar movimiento porque no se especificó una cuenta contable válida para esta operación."
                    };
                }

                int lngRecibo = 0;

                if (req.generar_recibo)
                {
                    var respDoc = CxcDocumentoAbono_Generar(codEmpresa, req, vCuenta, vTipoDoc);
                    ErrorDto eeDoc;
                    if (FailIfError(respDoc, out eeDoc)) return eeDoc;
                }

                var respAnula = Exec(codEmpresa,
                    @"exec spCrdPlanPagoAnulaAbono
                        @operacion,
                        'CRD008',
                        @usuario,
                        'CxC_ND',
                        @recibo,
                        1,
                        @intcor,
                        @intmor,
                        @amortizacion,
                        @cargos,
                        @fecha,
                        ''",
                    new
                    {
                        operacion = req.operacion,
                        usuario = req.usuario,
                        recibo = lngRecibo,
                        intcor = req.intcor,
                        intmor = req.intmor,
                        amortizacion = req.amortizacion,
                        cargos = req.cargos,
                        fecha
                    });

                ErrorDto eeAnula;
                if (FailIfError(respAnula, out eeAnula)) return eeAnula;

                RegistrarBitacora(
                    codEmpresa,
                    req.usuario,
                    $"OP: {req.operacion} Doc.: {lngRecibo} Total : {req.total}",
                    "Anula"
                );

                return new ErrorDto
                {
                    Code = lngRecibo,
                    Description = $"Anulación realizada ... Con Nota Débito #{lngRecibo}"
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        #region helpers CxcAbono_Anular

        private static bool FailIfError(ErrorDto? resp, out ErrorDto err)
        {
            if (resp is { Code: not null } && resp.Code != 0)
            {
                err = resp;
                return true;
            }

            err = new ErrorDto { Code = 0, Description = "" };
            return false;
        }

        private ErrorDto Exec(int codEmpresa, string sqlString, object param)
        {
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlString, param);
        }

        public ErrorDto CxcDocumentoAbono_Generar(int codEmpresa, CxcAbonoAnularParams req, string vCuenta, string vTipoDoc)
        {
            try
            {
                long lngRecibo = _mRecibos.FxDocumentoConsecutivo(codEmpresa, vTipoDoc);
                int recibo = (int)lngRecibo;
                if (recibo <= 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = "No se pudo obtener el consecutivo del documento."
                    };
                }

                var opResp = CxcOperacionCtas_Consultar(codEmpresa, req.operacion);
                var opErrorDTO = new ErrorDto{ Code = opResp.Code, Description = opResp.Description };
                if (FailIfError(opErrorDTO, out var ee))
                    return new ErrorDto { Code = ee.Code, Description = ee.Description };

                var op = opResp.Result;
                if (op == null)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = "No se pudo obtener la información contable de la operación."
                    };
                }

                var strLinea1 = $"Saldo Actual      {op.saldo:N2}";
                var strLinea2 = $"Interes Corriente {(req.intcor * -1):N2}";
                var strLinea3 = $"Interes Moratorio {(req.intmor * -1):N2}";
                var strLinea4 = $"Amortización      {(req.amortizacion * -1):N2}";
                var strLinea5 = $"Cargos            {(req.cargos * -1):N2}";
                var strLinea6 = "";
                var strLinea7 = $"Nuevo Saldo       {(op.saldo + req.amortizacion):N2}";
                var strLinea8 = $"Operación /Linea  {req.operacion}_{req.cod_concepto_operacion}";
                var strLinea9 = "";
                var strLinea10 = $"Usuario           {req.usuario}";
                var strLinea11 = "Anulación";

                string gOficinaTitular = _mProGrx.sbSifParametrosInicializa(codEmpresa, req.usuario).Result!.GOficinaTitular;

                var respIns = SifTransaccion_Insertar(codEmpresa, new SifTransaccionInsertParams
                {
                    cod_transaccion = recibo,
                    tipo_documento = vTipoDoc,
                    registro_usuario = req.usuario,
                    cliente_identificacion = req.cedula,
                    cliente_nombre = req.nombre,
                    cod_concepto = "CRD008",
                    monto = req.intcor + req.intmor + req.amortizacion + req.cargos,
                    estado = "P",
                    referencia_01 = req.operacion.ToString(),
                    referencia_02 = req.cod_concepto_operacion,
                    referencia_03 = req.deposito ?? "",
                    cod_oficina = gOficinaTitular,
                    linea1 = strLinea1,
                    linea2 = strLinea2,
                    linea3 = strLinea3,
                    linea4 = strLinea4,
                    linea5 = strLinea5,
                    linea6 = strLinea6,
                    linea7 = strLinea7,
                    linea8 = strLinea8,
                    linea9 = strLinea9,
                    linea10 = strLinea10,
                    linea11 = strLinea11,
                    detalle = (req.detalle ?? "") + Environment.NewLine + "Depósito..:" + (req.deposito ?? "")
                });

                if (FailIfError(respIns, out ee))
                    return new ErrorDto { Code = ee.Code, Description = ee.Description };

                if (req.intcor > 0)
                {
                    var r = RegistrarAsiento(codEmpresa, req, recibo, req.intcor, "D", op.cod_divisa, op.cod_unidad, op.cod_centro_costo, op.ctaintc, op.operacion, vTipoDoc, op.cod_concepto, req.deposito);
                    if (FailIfError(r, out ee)) return new ErrorDto { Code = ee.Code, Description = ee.Description };
                }

                if (req.intmor > 0)
                {
                    var r = RegistrarAsiento(codEmpresa, req, recibo, req.intmor, "D", op.cod_divisa, op.cod_unidad, op.cod_centro_costo, op.ctaintm, op.operacion, vTipoDoc, op.cod_concepto, req.deposito);
                    if (FailIfError(r, out ee)) return new ErrorDto { Code = ee.Code, Description = ee.Description };
                }

                if (req.cargos > 0)
                {
                    var r = RegistrarAsiento(codEmpresa, req, recibo, req.cargos, "D", op.cod_divisa, op.cod_unidad, op.cod_centro_costo, op.ctacargos, op.operacion, vTipoDoc, op.cod_concepto, req.deposito);
                    if (FailIfError(r, out ee)) return new ErrorDto { Code = ee.Code, Description = ee.Description };
                }

                if (req.amortizacion > 0)
                {
                    var r = RegistrarAsiento(codEmpresa, req, recibo, req.amortizacion, "D", op.cod_divisa, op.cod_unidad, op.cod_centro_costo, op.ctaamortiza, op.operacion, vTipoDoc, op.cod_concepto, req.deposito);
                    if (FailIfError(r, out ee)) return new ErrorDto { Code = ee.Code, Description = ee.Description };
                }

                var total = req.intcor + req.intmor + req.amortizacion + req.cargos;
                if (total > 0)
                {
                    var r = RegistrarAsiento(codEmpresa, req, recibo, total, "C", op.cod_divisa, op.cod_unidad, op.cod_centro_costo, vCuenta, op.operacion, vTipoDoc, op.cod_concepto, req.deposito);
                    if (FailIfError(r, out ee)) return new ErrorDto  { Code = ee.Code, Description = ee.Description };
                }


                return new ErrorDto
                {
                    Code = recibo,
                    Description = ""
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        private ErrorDto RegistrarAsiento(
             int codEmpresa,
             CxcAbonoAnularParams req,
             int documento,
             decimal monto,
             string dc,
             string? cod_divisa,
             string? cod_unidad,
             string? cod_centro_costo,
             string? cuenta,
             int operacion,
             string vTipoDoc,
             string? cod_concepto,
             string? deposito)
        {
            int gEnlace = _mProGrx.sbSifParametrosInicializa(codEmpresa, req.usuario).Result!.GEnlace; 

            return SifDocsAsiento_Registrar(codEmpresa, new SifDocsAsientoParams
            {
                tipodoc = vTipoDoc,
                numdoc = documento.ToString(),
                monto = monto,
                dc = dc,
                cod_divisa = cod_divisa ?? "",
                tipo_cambio = 1,
                enlace = gEnlace,
                cod_unidad = cod_unidad ?? "",
                cod_centro_costo = cod_centro_costo ?? "",
                cuenta = cuenta ?? "",
                operacion = operacion,
                cod_concepto = cod_concepto ?? "",
                deposito = deposito ?? ""
            });
        }


        public ErrorDto<CxCOperacionCtasData?> CxcOperacionCtas_Consultar(int codEmpresa, int operacion)
        {
            try
            {
                var sql = @"exec spCxC_OperacionCtas @operacion";

                return DbHelper.ExecuteSingleQuery<CxCOperacionCtasData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    default,
                    new { operacion }
                );
            }
            catch (Exception ex)
            {
                return new ErrorDto<CxCOperacionCtasData?>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = null
                };
            }
        }

        public ErrorDto SifTransaccion_Insertar(int codEmpresa, SifTransaccionInsertParams param)
        {
            var sql = @"
                insert into SIF_TRANSACCIONES
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
                    linea1,
                    linea2,
                    linea3,
                    linea4,
                    linea5,
                    linea6,
                    linea7,
                    linea8,
                    linea9,
                    linea10,
                    linea11,
                    detalle
                )
                values
                (
                    @cod_transaccion,
                    @tipo_documento,
                    dbo.MyGetdate(),
                    @registro_usuario,
                    @cliente_identificacion,
                    @cliente_nombre,
                    @cod_concepto,
                    @monto,
                    @estado,
                    @referencia_01,
                    @referencia_02,
                    @referencia_03,
                    @cod_oficina,
                    @linea1,
                    @linea2,
                    @linea3,
                    @linea4,
                    @linea5,
                    @linea6,
                    @linea7,
                    @linea8,
                    @linea9,
                    @linea10,
                    @linea11,
                    @detalle
                )";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);
        }

        public ErrorDto SifDocsAsiento_Registrar(int codEmpresa, SifDocsAsientoParams param)
        {
            var sql = @"
                exec spSIFDocsAsiento
                    @tipodoc,
                    @numdoc,
                    @monto,
                    @dc,
                    @cod_divisa,
                    @tipo_cambio,
                    @enlace,
                    @cod_unidad,
                    @cod_centro_costo,
                    @cuenta,
                    @operacion,
                    @cod_concepto,
                    @deposito";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new
            {
                param.tipodoc,
                param.numdoc,
                param.monto,
                param.dc,
                param.cod_divisa,
                param.tipo_cambio,
                param.enlace,
                param.cod_unidad,
                param.cod_centro_costo,
                param.cuenta,
                param.operacion,
                param.cod_concepto,
                param.deposito
            });
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        #endregion
    }
}