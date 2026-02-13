using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAbonosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly MCajas _mCajas;
        private readonly MRecibos _mRecibos;
        private readonly int vModulo = 31;

        public FrmCxCCuentasAbonosDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config),
                 new MCajas(config),
                 new MRecibos(config))  
        {
        }

        public FrmCxCCuentasAbonosDb(PortalDB portalDB, MSecurityMainDb dbBitacora, MCajas mCajas, MRecibos mRecibos)
        {
            _portalDb = portalDB;
            DBBitacora = dbBitacora;
            _mCajas = mCajas;
            _mRecibos = mRecibos;
        }

        /// <summary>
        /// Obtiene informacion de la operacion 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
        public ErrorDto<CxCCuentasAbonosData> CxCCuentas_ConsultaOperacion_Obtener(int codEmpresa, string codCaja, int operacionId)
        {
            const string query = @"select R.Operacion,R.saldo,R.proceso,R.Tasa_Corriente,R.interesc,R.amortiza,
                    dbo.fxSIFFechaProcesoConvert(isnull(R.Fecha_UltMov,GETDATE())) as 'Fecha_UltMov'
                    , R.cuota,R.cod_concepto,R.cedula,datediff(m,R.Activa_Fecha,GETDATE()) as 'Meses'
                    , S.nombre,R.Activa_Fecha,R.Autoriza_Usuario 
                    , C.descripcion as 'ConceptoDesc',Ofi.descripcion as 'OficinaDesc', GETDATE() as 'FechaServer' 
                    , dbo.fxCajas_Valida_Auxiliar(@codCaja,'CxC',C.cod_Concepto) as 'caja_valida_concepto'
                    , dbo.fxCxC_Operacion_Facturas_Pending(R.Operacion) as 'Facturas' 
                    from CxC_Cuentas R inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto 
                    inner join CxC_Personas S on R.cedula = S.cedula 
                    left join Sif_Oficinas Ofi on R.cod_Oficina = Ofi.cod_Oficina 
                    left join vCxC_CuentasMora V on R.Operacion = V.Operacion 
                    where R.estado = 'A' and R.saldo > 0 and R.Operacion = @operacionId";

            var result = DbHelper.ExecuteSingleQuery<CxCCuentasAbonosData>(
                _portalDb, codEmpresa, query, new CxCCuentasAbonosData(), new { codCaja, operacionId });

            if (result.Result == null)
            {
                result.Result = new CxCCuentasAbonosData();
            }
            return result!;
        }

        /// <summary>
        /// Obtiene las cuotas activas de una operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCCuotasActivasData>> CxCCuentas_CuotasActivas_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"
                SELECT * , CASE WHEN Dias_Mora > 0 THEN 'En Mora' ELSE 'Al Día' END AS estado_desc 
                FROM CxC_Cuentas_Mov
                WHERE estado = 'A' AND Operacion = @operacionId
                ORDER BY Linea;";

            return DbHelper.ExecuteListQuery<CxCCuotasActivasData>(_portalDb, codEmpresa, query, new { operacionId });
        }

        /// <summary>
        /// Obtiene lista de operaciones activas 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CxCOperacionesActivasData>> CxCCuentas_OperacionesActivas_Obtener(int codEmpresa)
        {
            const string query = @"
                Select R.Operacion,R.COD_CONCEPTO,S.Cedula,S.Nombre,C.Descripcion 
                FROM CxC_Cuentas R 
                    INNER JOIN CxC_Personas S ON R.cedula = S.cedula 
                    INNER JOIN CxC_Conceptos C ON R.COD_CONCEPTO = C.COD_CONCEPTO 
                WHERE R.ESTADO = 'A'";

            return DbHelper.ExecuteListQuery<CxCOperacionesActivasData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los tipos de documentos 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="caja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_TipoDoc_Obtener(int codEmpresa, string caja)
        {
            const string query = @"select rTrim(C.tipo_documento) as item, rtrim(D.Descripcion) as descripcion
                from SIF_DOCUMENTOS D inner join CAJAS_DOCUMENTOS C on D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO  
                Where C.cod_caja = @caja and D.Tipo_Movimiento in('A','C') 
                order by C.tipo_documento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { caja });
        }

        /// <summary>
        /// Obtiene informacion de las cuotas de una operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="vOperacion"></param>
        /// <param name="vCuotas"></param>
        /// <returns></returns>
        public ErrorDto<CxCCuentaCuotasInfoData> CxCCuentas_CuotasInfo_Obtener(int codEmpresa, int vOperacion, int vCuotas)
        {
            const string sqlTotales = @"
            select 
                isnull(max(Linea),0) as seqX, 
                isnull(sum(Int_Cor + Int_Mor),0) as intCor, 
                isnull(sum(Principal),0) as principal,
                isnull(min(Saldo_Final),0) as saldo, 
                isnull(max(Fecha_Corte),0) as fecha_Proceso
            from CxC_Cuentas_Mov where Operacion = @OperacionId 
            and Linea in(select Top (@Cuotas) Linea from CxC_Cuentas_Mov
            where estado in('A','P') and Operacion =  @OperacionId  and Linea > 0  order by Linea);";

            var totales = DbHelper.ExecuteSingleQuery<CxCCuentaCuotasInfoData>(
                _portalDb,
                codEmpresa,
                sqlTotales,
                new CxCCuentaCuotasInfoData(),
                new
                {
                    OperacionId = vOperacion,
                    Cuotas = vCuotas
                }
            );

            if (totales.Result == null)
                totales.Result = new CxCCuentaCuotasInfoData();

            const string sqlCuota = @"
                select ISNULL(Monto, 0) AS Cuota
                from CxC_Cuentas_Mov where Linea = @Linea
                and Operacion = @OperacionId;";

            if (totales.Result.seqX > 0)
            {
                var cuotaRs = DbHelper.ExecuteSingleQuery<decimal>(
                    _portalDb,
                    codEmpresa,
                    sqlCuota,
                    0,
                    new
                    {
                        Linea = totales.Result.seqX,
                        OperacionId = vOperacion
                    }
                );

                if (cuotaRs == null || cuotaRs.Code != 0)
                    return new ErrorDto<CxCCuentaCuotasInfoData>
                    {
                        Code = cuotaRs?.Code,
                        Description = cuotaRs?.Description,
                        Result = totales.Result
                    };

                totales.Result.cuota = cuotaRs.Result;
            }

            return totales!;
        }

        /// <summary>
        /// Registra un abono en una cuenta por cobrar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CxCCuentas_Abono_Registrar(int codEmpresa, CxCCuentasRegistrarAbonoRequest req)
        {
            try
            {
                req.notas = MProGrxMain.sbSIFCleanTxtInject(req.notas);

                var verif = FxVerifica(codEmpresa, req);
                if (FailIfError(verif, out var ee)) return ee;

                var vNumDoc = _mRecibos.FxDocumentoConsecutivo(codEmpresa, req.tipodoc).ToString();

                var extraordinario = false;
                var respP = ProcesarRegistroAbono(codEmpresa, req, vNumDoc, ref extraordinario);
                if (FailIfError(respP, out ee)) return ee;

                // Indica si debe reprocesar el Plan de Pagos por registro de Abonos Extraordinario
                if (extraordinario)
                {
                    var spCPP = Exec(codEmpresa,
                        @"exec spCxC_CuentaPlanPagos @Operacion;",
                        new { Operacion = req.operacionid });

                    if (FailIfError(spCPP, out ee)) return ee;
                }

                var (bitacoraDesc, comprobanteConcepto) = ObtenerDescripcionComprobante(req);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (req.usuario ?? "").ToUpper(),
                    DetalleMovimiento = bitacoraDesc,
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                var respD = CxCCuentas_DocumentoAbono_Generar(codEmpresa, comprobanteConcepto, req, vNumDoc);

                if (FailIfError(respD, out ee)) return ee;

                var mensajeFinal = ProcesarRecibo(codEmpresa, req, vNumDoc);

                return new ErrorDto { Code = 0, Description = mensajeFinal + vNumDoc };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        #region helpers CxCCuentas_Abono_Registrar

        private static bool FailIfError(ErrorDto? resp, out ErrorDto err)
        {
            if (resp?.Code.HasValue == true && resp.Code != 0)
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

        public ErrorDto FxVerifica(int codEmpresa, CxCCuentasRegistrarAbonoRequest request)
        {
            try
            {
                var mensajes = new List<string>();

                ValidacionesSaldo(codEmpresa, request, mensajes);
                ValidacionCajas(codEmpresa, request, mensajes);

                return mensajes.Count > 0
                    ? new ErrorDto
                    {
                        Code = -2,
                        Description = string.Join(Environment.NewLine, mensajes.Select(m => $"- {m}"))
                    }
                    : new ErrorDto { Code = 0, Description = "" };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private void ValidacionesSaldo(
            int codEmpresa,
            CxCCuentasRegistrarAbonoRequest request,
            List<string> mensajes)
        {
            //Verifica que la diferencia del Monto a Cancelar no supere el Saldo
            if (request.diferencia < 0 && (request.saldo_nuevo + request.diferencia) < 0)
                mensajes.Add("La diferencia supera el saldo!, verifique...");

            if (request.operacionid == 0)
                mensajes.Add("Número de Operacion no es válido...");

            //Verifica Saldo Actual
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            if (!MCxCDb.fxCxCSaldoVerifica(conn, request.operacionid, request.saldo_anterior))
                mensajes.Add("Esta Operación ha sido modificada, actualice los datos nuevamente antes de realizar el abono...");

            if (request.datosamortiza > request.saldo_anterior)
                mensajes.Add("La Amortización es mayor al Saldo Actual...");

            if (request.totalcajas <= 0)
                mensajes.Add("Los valores Recibidos en Cajas no son válidos...verifique...!");
        }

        private void ValidacionCajas(
            int codEmpresa,
            CxCCuentasRegistrarAbonoRequest request,
            List<string> mensajes)
        {
            string estadoCaja = _mCajas.fxCajasAperturaEstado(codEmpresa, request.mcaja, request.mapertura);
            if (estadoCaja == "C")
                mensajes.Add($"La apertura ..:{request.mapertura} de esta caja ha sido cerrada!");

            const string sqlVal = @"
            exec spCajas_Transac_Validacion @caja, @Usuario, @apertura, @sesionid, 
              'CxC', @codigo, @monto, @tiquete;";

            var response = DbHelper.ExecuteSingleQuery<dynamic>(
                _portalDb,
                codEmpresa,
                sqlVal,
                null,
                new
                {
                    caja = request.mcaja,
                    Usuario = request.usuario,
                    apertura = request.mapertura,
                    sesionid = request.msesionid,
                    codigo = (request.codigo ?? "").Trim(),
                    monto = request.totalcajas,
                    tiquete = request.mtiquete
                }
            ).Result;

            string? validac = (response?.Validacion as string) ?? (response?.validacion as string);
            if (!string.IsNullOrWhiteSpace(validac))
                mensajes.Add(validac);
        }

        private ErrorDto ProcesarRegistroAbono(int codEmpresa, CxCCuentasRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            return req.tipoabono switch
            {
                AbonoTipo.Ordinario => RegistrarOrdinarioYAdelanto(codEmpresa, "CRD001", req, vNumDoc, ref extraordinario),
                AbonoTipo.Extraordinario => RegistrarExtraordinario(codEmpresa, req, vNumDoc, ref extraordinario),
                AbonoTipo.Cancelacion => RegistrarCancelacion(codEmpresa, req, vNumDoc),
                AbonoTipo.AdelantoCuotas => RegistrarOrdinarioYAdelanto(codEmpresa, "CRD004", req, vNumDoc, ref extraordinario),
                _ => new ErrorDto { Code = -2, Description = "Tipo de abono no soportado" }
            };
        }

        private ErrorDto RegistrarOrdinarioYAdelanto(int codEmpresa, string codTipo, CxCCuentasRegistrarAbonoRequest request, string vNumDoc, ref bool extraordinario)
        {
            if (!request.diferenciaaplenabled)
            {
                return Exec(codEmpresa,
                    @"exec spCxC_AbonoOrdinario @Operacion,@Codigo,@Usuario,@TipoDoc,@numDoc,@monto,@fecha,''",
                    new
                    {
                        Operacion = request.operacionid,
                        Codigo = codTipo,
                        Usuario = request.usuario,
                        TipoDoc = request.tipodoc,
                        numDoc = vNumDoc,
                        monto = request.totalcajas,
                        fecha = DateTime.Now
                    });
            }

            var r1 = Exec(codEmpresa,
                @"exec spCxC_AbonoOrdinario @Operacion,@Codigo,@Usuario,@TipoDoc,@numDoc,@monto,@fecha,''",
                new
                {
                    Operacion = request.operacionid,
                    Codigo = codTipo,
                    Usuario = request.usuario,
                    TipoDoc = request.tipodoc,
                    numDoc = vNumDoc,
                    monto = request.totalcancela,
                    fecha = DateTime.Now
                });

            if (r1?.Code.HasValue == true && r1.Code != 0)
                return r1;

            return AplicarDiferenciaOrdinario(codEmpresa, request, vNumDoc, ref extraordinario);
        }

        private ErrorDto AplicarDiferenciaOrdinario(int codEmpresa, CxCCuentasRegistrarAbonoRequest request, string vNumDoc, ref bool extraordinario)
        {
            if (request.diferenciaapltexto == "Adelanto de Cuota")
                return AplicarAdelantoCuota(codEmpresa, request, vNumDoc);

            if (request.diferenciaapltexto == "Abono Extraordinario")
                return AplicarExtraordinario(codEmpresa, request, vNumDoc, ref extraordinario);

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto AplicarAdelantoCuota(int codEmpresa, CxCCuentasRegistrarAbonoRequest request, string vNumDoc)
        {
            return Exec(codEmpresa,
                @"exec spCxC_AbonoOrdinario @Operacion,'CRD004',@Usuario,@TipoDoc,@numDoc,@monto,@fecha,''",
                new
                {
                    Operacion = request.operacionid,
                    Usuario = request.usuario,
                    TipoDoc = request.tipodoc,
                    numDoc = vNumDoc,
                    monto = Math.Abs(request.diferencia),
                    fecha = DateTime.Now
                });
        }

        private ErrorDto AplicarExtraordinario(int codEmpresa, CxCCuentasRegistrarAbonoRequest request, string vNumDoc, ref bool extraordinario)
        {
            var rAbono = Exec(codEmpresa,
                @"exec spCxC_AbonoExtraOrdinario @Operacion,'CRD002',@Usuario,@tipoDoc,@numDoc,
                  0, 0, @Diferencia, 0, @fecha,'',@recalcula",
                new
                {
                    Operacion = request.operacionid,
                    Usuario = request.usuario,
                    tipoDoc = request.tipodoc,
                    numDoc = vNumDoc,
                    Diferencia = request.diferencia,
                    fecha = DateTime.Now,
                    recalcula = request.recalculacuota ? 1 : 0
                });

            if (rAbono?.Code.HasValue == true && rAbono.Code != 0)
                return rAbono;

            extraordinario = true;
            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto RegistrarExtraordinario(int codEmpresa, CxCCuentasRegistrarAbonoRequest request, string vNumDoc, ref bool extraordinario)
        {
            var r = Exec(codEmpresa,
                @"exec spCxC_AbonoExtraOrdinario @operacion,'CRD002',@Usuario,@tipoDoc,@numDoc,
                    @dias,0,@totalPagar,0,@fecha,'',@recalcula",
                new
                {
                    operacion = request.operacionid,
                    Usuario = request.usuario,
                    tipoDoc = request.tipodoc,
                    numDoc = vNumDoc,
                    dias = request.diasactivo,
                    totalPagar = request.totalpagar,
                    fecha = DateTime.Now,
                    recalcula = request.recalculacuota ? 1 : 0
                });

            if (r?.Code.HasValue == true && r.Code != 0)
                return r;

            extraordinario = true;
            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto RegistrarCancelacion(int codEmpresa, CxCCuentasRegistrarAbonoRequest request, string vNumDoc)
        {
            return Exec(codEmpresa,
                @"exec spCxC_AbonoCancelacion @operacion,'CRD003',@Usuario,@tipoDoc,@numDoc,@totalCajas,@fecha,''",
                new
                {
                    operacion = request.operacionid,
                    Usuario = request.usuario,
                    tipoDoc = request.tipodoc,
                    numDoc = vNumDoc,
                    totalCajas = request.totalcajas,
                    fecha = request.fechacancelacion ?? DateTime.Now
                });
        }

        private static (string bitacoraDesc, string comprobanteConcepto) ObtenerDescripcionComprobante(CxCCuentasRegistrarAbonoRequest request)
        {
            return request.tipoabono switch
            {
                AbonoTipo.Ordinario => (
                    $"Abono Ordinario a la Operacion : {request.operacionid}",
                    "CRD001"
                ),
                AbonoTipo.Extraordinario => (
                    $"Abono ExtraOrd. {(request.recalculacuota ? "Con Recal." : "Sin Recal")} a la Op.: {request.operacionid}",
                    "CRD002"
                ),
                AbonoTipo.Cancelacion => (
                    $"Cancelación de la Operacion : {request.operacionid}",
                    "CRD003"
                ),
                AbonoTipo.AdelantoCuotas => (
                    $"Adelanto de Cuotas de la Operacion : {request.operacionid}",
                    "CRD004"
                ),
                _ => (
                    $"Movimiento no identificado para la Operacion : {request.operacionid}",
                    string.Empty
                )
            };
        }

        public ErrorDto CxCCuentas_DocumentoAbono_Generar(int codEmpresa, string pConcepto, CxCCuentasRegistrarAbonoRequest request, string vNumDoc)
        {
            DateTime? pFechaCancela = null;

            if (request.fechacancelacion_enabled)
            {
                pFechaCancela = request.fechacancelacion;
            }
            return Exec(codEmpresa,
                @"exec spCxC_Cuenta_Movimiento_Asiento @Operacion, @TipoAbono, @TipoDoc, @NumDoc, @Concepto, 
                    @Notas, @FechaCancela, @Usuario, @Cajas, @Apertura, @SesionId, @Tiquete",
                new
                {
                    Operacion = request.operacionid,
                    TipoAbono = request.tipoabono,
                    TipoDoc = request.tipodoc,
                    NumDoc = vNumDoc,
                    Concepto = pConcepto,
                    Notas = request.notas,
                    FechaCancela = pFechaCancela,
                    Usuario = request.usuario,
                    Cajas = request.mcaja,
                    Apertura = request.mapertura,
                    SesionId = request.msesionid,
                    Tiquete = request.mtiquete
                });
        }

        private string ProcesarRecibo(int codEmpresa, CxCCuentasRegistrarAbonoRequest request, string vNumDoc)
        {
            string mensaje;
            string documentoElectronico = "";

            //PROCESAR RECIBO
            if (request.recibo_digital)
            {
                DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa,
                    @"exec spCajasReciboDigital @numDoc, @tipoDoc, 'CxC'",
                    new
                    {
                        numDoc = vNumDoc,
                        tipoDoc = request.tipodoc
                    });

                mensaje =
                    documentoElectronico + Environment.NewLine + Environment.NewLine +
                    ">>> Recibo Digital enviado al cliente <<<" + Environment.NewLine +
                    $" - Abono aplicado, con : {request.tipodoc} ...No.: {vNumDoc}" + Environment.NewLine +
                    " - Desea Realizar Otra Transacción a esta Operación ?";
            }
            else
            {
                mensaje =
                    documentoElectronico + Environment.NewLine + Environment.NewLine +
                    $" - Abono aplicado, con : {request.tipodoc} ...No.: {vNumDoc}" + Environment.NewLine +
                    " - Desea Realizar Otra Transacción a esta Operación ?";
            }

            return mensaje;
        }
        #endregion
    }
}
