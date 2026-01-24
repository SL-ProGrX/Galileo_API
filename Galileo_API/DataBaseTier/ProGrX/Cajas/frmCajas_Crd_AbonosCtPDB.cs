using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCrdAbonosCtPDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly MCajas _mCajas;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MRecibos _mRecibos;
        private readonly MAfilicacionDB _mAfiliacion;
        private readonly int vModulo = 5;

        public FrmCajasCrdAbonosCtPDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config), new MCajas(config), new MProGrxMain(config), new MRecibos(config), new MAfilicacionDB(config))
        {
        }

        public FrmCajasCrdAbonosCtPDb(PortalDB portalDb, MSecurityMainDb dbBitacora, MCajas mCajas, MProGrxMain mProGrxMain, MRecibos mRecibos, MAfilicacionDB mAfiliacion)
        {
            _portalDb = portalDb;
            DBBitacora = dbBitacora;
            _mCajas = mCajas;
            _mProGrxMain = mProGrxMain;
            _mRecibos = mRecibos;
            _mAfiliacion = mAfiliacion;
        }

        public ErrorDto<CajasCrdAbonosCtPData> CajasCrdAbonosCtP_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"select R.id_solicitud,R.saldo, R.saldo - isnull(V.amortiza,0) As Saldo_mes,R.proceso, isnull(R.cod_Divisa,'COL') as 'divisa'
                    ,R.interesv,R.int,R.plazo,R.interesc,R.amortiza,R.fecult,R.Prideduc, isnull(C.Impuesto,0) as 'IVA_Aplica'
                    , R.opex,R.cuota,R.codigo,R.cedula,R.cuotas_planilla,R.cuotas_directas, datediff(m,R.fechaforp,dbo.MyGetdate()) as 'Meses'
                    , S.nombre,C.descripcion,C.retencion,C.poliza,R.fechaforp,C.PORC_CARGO_CANCELACION,C.ANTICIPO_MESES,R.Base_Calculo
                    , dbo.fxCrdPlanPagosDiasActivo(@OperacionId) as 'DiasActivo', dbo.fxCrdOperacionTagReg(R.id_solicitud,'S15') as 'AutPagoAnt'
                    , C.descripcion as 'LineaDesc',Ofi.descripcion as 'OficinaDesc',Pre.Descripcion as 'RecursoDesc',dbo.MyGetdate() as 'FechaServer'
                    , dbo.fxCajas_Valida_Auxiliar(@CodCaja,'CRD',R.Codigo) as 'Caja_Valida_Concepto'
                    , dbo.fxCrd_Operacion_Control(R.id_solicitud) as 'Control'
                    , dbo.fxCrd_IVA_Porc() as 'IVA_Porc'
                    from reg_creditos R inner join Catalogo C on R.codigo = C.codigo 
                    inner join Socios S on R.cedula = S.cedula
                    left join Sif_Oficinas Ofi on R.cod_Oficina_R = Ofi.cod_Oficina
                    left join CATALOGO_GRUPOS Pre on R.cod_grupo = Pre.cod_grupo
                    left join vista_morosidad V on R.id_solicitud = V.id_solicitud
                    where R.estado = 'A' and R.saldo > 0
                    and R.ID_SOLICITUD = @OperacionId";

                var op = conn.QueryFirstOrDefault<CajasCrdAbonosCtPData>(query, new { CodCaja, OperacionId }) ?? new CajasCrdAbonosCtPData();

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

        public ErrorDto<List<CajasCrdAbonosCtPData>> CajasCrdAbonosCtP_Operaciones_Obtener(int CodEmpresa)
        {
            const string query = @"SELECT R.id_solicitud,R.Codigo,S.Cedula,S.Nombre,C.Descripcion 
                from REG_CREDITOS R inner join SOCIOS S on R.cedula = S.cedula 
                inner join Catalogo C on R.codigo = C.codigo 
                WHERE R.estado = 'A' ORDER BY R.cedula";

            return DbHelper.ExecuteListQuery<CajasCrdAbonosCtPData>(_portalDb, CodEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosCtP_TipoDoc_Obtener(int CodEmpresa, string Caja)
        {
            const string query = @"select rTrim(C.tipo_documento) as item, rtrim(D.Descripcion) as descripcion
                from SIF_DOCUMENTOS D inner join CAJAS_DOCUMENTOS C on D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO  
                Where C.cod_caja = @Caja and D.Tipo_Movimiento in('A','C') 
                order by C.tipo_documento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, CodEmpresa, query, new { Caja });
        }

        public ErrorDto<List<CajasCrdOperacionTransacData>> CajasCrdAbonosCtP_OperacionTransac_Obtener(int CodEmpresa, int IdSolicitud)
        {
            const string sql = @"select * from CRD_OPERACION_TRANSAC 
                where estado = 'A' and id_solicitud = @IdSolicitud 
                and Fecha_Inicio < GETDATE() order by ID_SEQ asc";

            return DbHelper.ExecuteListQuery<CajasCrdOperacionTransacData>(_portalDb, CodEmpresa, sql, new {IdSolicitud});
        }

        public ErrorDto<long> CajasCrdAbonosCtP_DiasActivoFecha_Obtener(int CodEmpresa, CajasCrdAbonoTipoRequest request)
        {
            const string sql = @"select dbo.fxCrdPlanPagosDiasActivoFecha(@OperacionId, @FechaCancelacion) as 'Dias'";

            return DbHelper.ExecuteSingleQuery<long>(
                _portalDb, 
                CodEmpresa, 
                sql, 0,
                new {
                    OperacionId = request.operacion_id,
                    FechaCancelacion = request.fecha_cancelacion
                }
            );
        }

        public ErrorDto<CajasCrdAbonosInfoCancelacionData> CajasCrdAbonosCtP_InfoCancelacion_Obtener(int CodEmpresa, CajasCrdAbonoTipoRequest request)
        {
            const string sql = @"exec spCrdPlanPagosInfoCancelacion @OperacionId, @FechaCancelacion";

            var result = DbHelper.ExecuteSingleQuery<CajasCrdAbonosInfoCancelacionData>
                (_portalDb, 
                CodEmpresa, 
                sql, 
                new CajasCrdAbonosInfoCancelacionData(), 
                new {
                    OperacionId = request.operacion_id, 
                    FechaCancelacion =  request.fecha_cancelacion 
                }
            );
            if (result.Result == null)
            {
                result.Result = new CajasCrdAbonosInfoCancelacionData();
            }
            return result!;
        }

        public ErrorDto CajasCrdAbonosCtP_Abono_Registrar(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            try
            {
                req.notas = MProGrxMain.sbSIFCleanTxtInject(req.notas);

                var vVerifica = fxVerifica(CodEmpresa, req);
                if (vVerifica?.Code.HasValue == true && vVerifica.Code != 0)
                    return vVerifica;

                var extraordinario = false;
                long numDoc = _mRecibos.fxDocumentoConsecutivo(CodEmpresa, req.tipodoc);
                string vNumDoc = numDoc.ToString();
                var r = ProcesarRegistroAbono(CodEmpresa, req, vNumDoc, ref extraordinario);

                if (r?.Code.HasValue == true && r.Code != 0)
                    return r;

                //Indica si debe reprocesar el Plan de Pagos por registro de Abonos Extraordinario
                if (extraordinario)
                {
                    var rp = exec(CodEmpresa,
                        @"exec spCrdPlanPagos @operacionid;
                        exec spCrdPlanPagosActivaCuota @operacionid,0;
                        exec spCrdPlanPagosMoraActualizaOp @operacionid;",
                        new { operacionid = req.operacionid });

                    if (rp?.Code.HasValue == true && rp.Code != 0)
                        return rp;
                }

                var (bitacoraDesc, comprobanteTitulo, comprobanteConcepto) = ObtenerDescripcionComprobante(req);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = req.usuario.ToUpper(),
                    DetalleMovimiento = bitacoraDesc,
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                var resp = CajasCrdAbonosCtP_DocumentoAbono_Generar(CodEmpresa, comprobanteTitulo, comprobanteConcepto, req, vNumDoc);
                if (resp?.Code.HasValue == true && resp.Code != 0)
                    return resp;

                string pDocumentoElectronico = string.Empty;

                if (req.factura_visible)
                {
                    pDocumentoElectronico = ProcesarDocumentoElectronico(CodEmpresa, req, vNumDoc);
                }

                var mensajeFinal = ProcesarRecibo(CodEmpresa, req,pDocumentoElectronico, vNumDoc);

                var codDoc = req.tipodoc == "CA" ? "07" : "05";
                var consecutivo = req.tipodoc == "CA" ? vNumDoc : $"{vNumDoc}-{req.tipodoc}";

                var rt = _mProGrxMain.sbTrazabilidad_Inserta(CodEmpresa, codDoc, consecutivo, vNumDoc, req.usuario, nuevo: true);
                if (rt?.Code.HasValue == true && rt.Code != 0)
                    return rt;

                return new ErrorDto
                {
                    Code = 0,
                    Description = mensajeFinal
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private ErrorDto ProcesarRegistroAbono(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            return req.tipoabono switch
            {
                TipoAbono.Ordinario => registrar_ordinario(CodEmpresa, req, vNumDoc, ref extraordinario),
                TipoAbono.Extraordinario => registrar_extraordinario(CodEmpresa, req, vNumDoc, ref extraordinario),
                TipoAbono.Cancelacion => registrar_cancelacion(CodEmpresa, req, vNumDoc),
                TipoAbono.AdelantoCuotas => registrar_adelanto_cuotas(CodEmpresa, req, vNumDoc),
                _ => new ErrorDto { Code = -2, Description = "Tipo de abono no soportado" }
            };
        }

        private static (string bitacoraDesc, string comprobanteTitulo, string comprobanteConcepto) ObtenerDescripcionComprobante(CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            return req.tipoabono switch
            {
                TipoAbono.Ordinario => (
                    $"Abono Ordinario a la Operacion : {req.operacionid}",
                    "ABONO ORDINARIO",
                    "CRD001"
                ),
                TipoAbono.Extraordinario => (
                    $"Abono ExtraOrd. {(req.recalculacuota ? "Con Recal." : "Sin Recal")} a la Op.: {req.operacionid}",
                    "ABONO EXTRAORDINARIO",
                    "CRD002"
                ),
                TipoAbono.Cancelacion => (
                    $"Cancelación de la Operacion : {req.operacionid}",
                    "CANCELACION DE DEUDA",
                    "CRD003"
                ),
                TipoAbono.AdelantoCuotas => (
                    $"Adelanto de Cuotas de la Operacion : {req.operacionid}",
                    "ADELANTO DE CUOTAS",
                    "CRD004"
                ),
                _ => (
                    $"Movimiento no identificado para la Operacion : {req.operacionid}",
                    string.Empty,
                    string.Empty
                )
            };
        }

        private string ProcesarDocumentoElectronico(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            string tipoDocumentoElectronico;
            string mensaje;

            if (req.tiquete_electronico == "T")
            {
                tipoDocumentoElectronico = "T";
                mensaje = "Tiquete Electrónico --> Emitido!";
            }
            else
            {
                tipoDocumentoElectronico = "F";
                mensaje = "Factura Electrónica --> Emitida!";
            }

            DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                @"exec spCajas_Credito_TE @tipodoc, @numdoc, @tipo",
                new
                {
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    tipo = tipoDocumentoElectronico
                }
            );

            return mensaje;
        }

        private string ProcesarRecibo(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string documentoElectronico, string vNumDoc)
        {
            string mensaje;

            //PROCESAR RECIBO
            if (req.recibo_digital)
            {
                DbHelper.ExecuteNonQuery(
                    _portalDb,
                    CodEmpresa,
                    @"exec spCajasReciboDigital @numdoc, @tipodoc, 'Creditos'",
                    new
                    {
                        numdoc = vNumDoc,
                        tipodoc = req.tipodoc
                    });

                mensaje =
                    documentoElectronico + Environment.NewLine + Environment.NewLine +
                    ">>> Recibo Digital enviado al cliente <<<" + Environment.NewLine +
                    $" - Abono aplicado, con : {req.tipodoc} ...No.: {vNumDoc}" + Environment.NewLine +
                    " - Desea Realizar Otra Transacción a esta Operación ?";
            }
            else
            {
                mensaje =
                    documentoElectronico + Environment.NewLine + Environment.NewLine +
                    $" - Abono aplicado, con : {req.tipodoc} ...No.: {vNumDoc}" + Environment.NewLine +
                    " - Desea Realizar Otra Transacción a esta Operación ?";
            }

            return mensaje;
        }


        #region helpers CajasCrdAbonosCtP_Abono_Registrar 

        public ErrorDto fxVerifica(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            var mensajes = new List<string>();

            try
            {
                const string sqlControl = @"
                select 
                  dbo.fxCrd_Operacion_Control(@operacionid) as control,
                  dbo.fxCrd_Operacion_Movimientos_Cajas_Acepta(@operacionid) as cajamov;";

                var data = DbHelper.ExecuteSingleQuery<dynamic>(
                    _portalDb,
                    CodEmpresa,
                    sqlControl,
                    null,
                    new { operacionid = req.operacionid }
                ).Result;

                var vcontrol = data?.control == null ? 0m : (decimal)data.control;
                var vcajasmov = data?.cajamov != null && Convert.ToInt32(data.cajamov) == 1;

                // Control de cambios
                if (vcontrol != req.control)
                    mensajes.Add("Esta Operación ha sido cambiada por otro proceso, vuelva a consultarla!");

                if (!vcajasmov)
                    mensajes.Add("Esta Operación -> No Permite Movimientos en Cajas! Puede que sea recaudo de ahorros o porque el código de linea no lo admite, revise!");

                // Verifica el proceso
                if (req.proceso == "J" && !_mCajas.fxCajasAbonosCbrJud(CodEmpresa, req.mcaja, req.usuario))
                    mensajes.Add("Esta CAJA no cuenta con permisos para realizar abonos a Creditos en Cobro Judicial, verifique...");

                // Verifica que la diferencia del Monto a Cancelar no supere el Saldo
                if (req.diferencia < 0 && (req.saldo_nuevo + req.diferencia) < 0)
                    mensajes.Add("La diferencia supera el saldo!, verifique...");

                // Verificar Congelamiento
                if (_mAfiliacion.fxgCongelamiento(CodEmpresa, req.cedula, "per_abono_cajas"))
                    mensajes.Add("Esta Persona se encuentra CONGELADA, verifique...");

       
                if (req.operacionid == 0)
                    mensajes.Add("Número de Operacion no es válido...");

                // Verifica Saldo Actual
                if (!MCredito.fxCrdSaldoVerifica(DbHelper.OpenConnection(_portalDb, CodEmpresa), req.operacionid, req.saldo_anterior))
                    mensajes.Add("Esta Operación ha sido modificada, actualice los datos nuevamente antes de realizar el abono...");

                if (!req.retencion)
                {
                    if (req.datosamortiza > req.saldo_anterior)
                        mensajes.Add("La Amortización es mayor al Saldo Actual...");
                }
                else if (req.plazo < 999 && req.datosamortiza > req.saldo_anterior)
                {
                    mensajes.Add($"La Amortización es mayor que el Remanente a Recaudar : {req.saldo_anterior:N2}");
                }

                if (req.totalpagar <= 0)
                    mensajes.Add("El total a pagar no es un dato válido...verifique...!");

                if (req.totalcajas <= 0)
                    mensajes.Add("Los valores Recibidos en Cajas no son válidos...verifique...!");

                if (!mensajes.Any() && req.totalcajas != req.totalpagar)
                    mensajes.Add("Los valores Recibidos en Cajas son diferentes al monto a Pagar establecido para el Abono...!");

                // Validacion para Abonos Extraordinarios
                if (!mensajes.Any() && req.tipoabono == TipoAbono.Extraordinario && req.diferencia != 0)
                    mensajes.Add("El Monto detallado en formas de pago no cubre el compromiso de pago. SOLUCION: Copie el Monto detallado en el monto del abono extraordinario...!");
                
                //Validacion para Cancelacion de Deudas
                if (!mensajes.Any() && req.tipoabono == TipoAbono.Cancelacion && req.diferencia != 0)
                    mensajes.Add("El Monto detallado en formas de pago no cubre el compromiso de cancelación...!");

                string estadoCaja = _mCajas.fxCajasAperturaEstado(CodEmpresa, req.mcaja, req.mapertura);
                if (estadoCaja == "C")
                    mensajes.Add($"La apertura ..:{req.mapertura} de esta caja ha sido cerrada!");

                // Cajas: Validación General sobre el Estado de la Caja, Aperturas, Sesiones, y Accesos
                const string sqlVal = @"
                exec spCajas_Transac_Validacion 
                  @caja, @usuario, @apertura, @sesionid, 
                  'Crd', @codigo, @monto, @tiquete;";

                var val = DbHelper.ExecuteSingleQuery<dynamic>(
                    _portalDb,
                    CodEmpresa,
                    sqlVal,
                    null,
                    new
                    {
                        caja = req.mcaja,
                        usuario = req.usuario,
                        apertura = req.mapertura,
                        sesionid = req.msesionid,
                        codigo = (req.codigo ?? "").Trim(),
                        monto = req.totalcajas,
                        tiquete = req.mtiquete
                    }
                ).Result;

                var validacion = (val?.Validacion as string) ?? (val?.validacion as string);
                if (!string.IsNullOrWhiteSpace(validacion))
                    mensajes.Add(validacion);

                // === Resultado final ===
                if (mensajes.Any())
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = string.Join(
                            Environment.NewLine,
                            mensajes.Select(m => $"- {m}")
                        )
                    };
                }

                return new ErrorDto { Code = 0, Description = "" };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private ErrorDto registrar_ordinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            if (!req.diferenciaaplenabled)
            {
                return exec(codempresa,
                    @"exec spCrdPlanPagoAbonoOrdinario 
                    @operacionid,'CRD001',@usuario,@tipodoc,@numdoc,@monto,@fecha,''",
                    new
                    {
                        operacionid = req.operacionid,
                        usuario = req.usuario,
                        tipodoc = req.tipodoc,
                        numdoc = vNumDoc,
                        monto = req.totalcajas,
                        fecha = DateTime.Now
                    });
            }

            var r1 = exec(codempresa,
                @"exec spCrdPlanPagoAbonoOrdinario 
                    @operacionid,'CRD001',@usuario,@tipodoc,@numdoc,@monto,@fecha,''",
                new
                {
                    operacionid = req.operacionid,
                    usuario = req.usuario,
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = req.totalcancela,
                    fecha = DateTime.Now
                });

            if (r1?.Code.HasValue == true && r1.Code != 0)
                return r1;

            return aplicar_diferencia_ordinario(codempresa, req, vNumDoc, ref extraordinario);
        }

        private ErrorDto aplicar_diferencia_ordinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            if (req.diferenciaapltexto == "Adelanto de Cuota")
                return aplicar_adelanto_cuota(codempresa, req, vNumDoc);

            if (req.diferenciaapltexto == "Abono Extraordinario")
                return aplicar_extraordinario_desde_diferencia(codempresa, req, vNumDoc, ref extraordinario);

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto aplicar_adelanto_cuota(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            return exec(codempresa,
                @"exec spCrdPlanPagoAbonoOrdinario 
          @operacionid,'CRD004',@usuario,@tipodoc,@numdoc,@monto,@fecha,''",
                new
                {
                    operacionid = req.operacionid,
                    usuario = req.usuario,
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = Math.Abs(req.diferencia),
                    fecha = DateTime.Now
                });
        }

        private ErrorDto aplicar_extraordinario_desde_diferencia(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            var info = get_info_extraordinario(codempresa, req);
            var rCargo = aplicar_cargo_si_corresponde(codempresa, req, info.cargos, "Pago Anticipado");
            if (rCargo?.Code.HasValue == true && rCargo.Code != 0)
                return rCargo;

            var rAbono = exec(codempresa,
                @"exec spCrdPlanPagoAbonoEC 
                  @operacionid,'CRD002',@usuario,@tipodoc,@numdoc,
                  @dias,@intereses,@principal,@cargos,@fecha,'',@recalcula",
                new
                {
                    operacionid = req.operacionid,
                    usuario = req.usuario,
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    dias = info.dias,
                    intereses = info.intereses,
                    principal = info.principal,
                    cargos = info.cargos,
                    fecha = DateTime.Now,
                    recalcula = req.recalculacuota ? 1 : 0
                });

            if (rAbono?.Code.HasValue == true && rAbono.Code != 0)
                return rAbono;

            extraordinario = true;
            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto registrar_extraordinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            var rCargo = aplicar_cargo_si_corresponde(codempresa, req, req.datosanticipo, "Pago Anticipado");
            if (rCargo?.Code.HasValue == true && rCargo.Code != 0)
                return rCargo;

            var r = exec(codempresa,
                @"exec spCrdPlanPagoAbonoEC 
          @operacionid,'CRD002',@usuario,@tipodoc,@numdoc,
          @dias,@intereses,@principal,0,@fecha,'',@recalcula,
          1,0,@iva",
                new
                {
                    operacionid = req.operacionid,
                    usuario = req.usuario,
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    dias = req.diasactivo,
                    intereses = req.datosinteres,
                    principal = req.datosamortiza,
                    fecha = DateTime.Now,
                    recalcula = req.recalculacuota ? 1 : 0,
                    iva = req.iva
                });

            if (r?.Code.HasValue == true && r.Code != 0)
                return r;

            extraordinario = true;
            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto registrar_cancelacion(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            var r1 = exec(codempresa,
                @"exec spCrdPlanPagosMoraActualizaOp 
          @operacionid,@fecha",
                new
                {
                    operacionid = req.operacionid,
                    fecha = req.fechacancelacion ?? DateTime.Now
                });

            if (r1?.Code.HasValue == true && r1.Code != 0)
                return r1;

            var rCargo = aplicar_cargo_si_corresponde(codempresa, req, req.datosanticipo, "Cancelacion Anticipada");
            if (rCargo?.Code.HasValue == true && rCargo.Code != 0)
                return rCargo;

            return exec(codempresa,
                @"exec spCrdPlanPagoAbonoCancelacion 
          @operacionid,'CRD003',@usuario,@tipodoc,@numdoc,@monto,@fecha,''",
                new
                {
                    operacionid = req.operacionid,
                    usuario = req.usuario,
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = req.totalcajas,
                    fecha = req.fechacancelacion ?? DateTime.Now
                });
        }

        private ErrorDto registrar_adelanto_cuotas(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            return exec(codempresa,
                @"exec spCrdPlanPagoAbonoOrdinario 
          @operacionid,'CRD004',@usuario,@tipodoc,@numdoc,@monto,@fecha,''",
                new
                {
                    operacionid = req.operacionid,
                    usuario = req.usuario,
                    tipodoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = req.totalcajas,
                    fecha = DateTime.Now
                });
        }

        private CajasCrdInfoExtraordinarioData get_info_extraordinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdInfoExtraordinarioData>(
                _portalDb,
                codempresa,
                @"exec spCrdPlanPagosInfoExtraordinario 
          @operacionid,@monto,@fechacancel",
                new CajasCrdInfoExtraordinarioData(),
                new
                {
                    operacionid = req.operacionid,
                    monto = Math.Abs(req.diferencia),
                    fechacancel = req.fechacancelacion ?? DateTime.Now
                }
            ).Result ?? new CajasCrdInfoExtraordinarioData();
        }

        private ErrorDto aplicar_cargo_si_corresponde(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            decimal cargos,
            string motivo)
        {
            if (cargos <= 0)
                return new ErrorDto { Code = 0, Description = string.Empty };

            return exec(codempresa,
                @"exec spCrdOperacionCargoAdd 
          @operacionid,@cargos,@unidad,@centrocosto,
          @motivo,@usuario,'CA','','',0",
                new
                {
                    operacionid = req.operacionid,
                    cargos,
                    unidad = req.oficinaunidad,
                    centrocosto = req.oficinacentrocosto,
                    motivo,
                    usuario = req.usuario
                });
        }

        private ErrorDto exec(int codempresa, string sql, object param)
        {
            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private static string fxStringRelleno(string texto, int largo)
        {
            texto ??= "";
            return texto.Length >= largo ? texto[..largo] : texto.PadLeft(largo, ' ');
        }

        #endregion


        public ErrorDto CajasCrdAbonosCtP_DocumentoAbono_Generar(int CodEmpresa, string pTipoAbono, string pConcepto, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            try
            {
                var tipocambio = _mCajas.fxCajasTipoCambio(CodEmpresa, 0, req.divisa);

                var ctas = obtener_cuentas_operacion(CodEmpresa, req.operacionid);
                var afect = obtener_afectacion_documento(CodEmpresa, req.tipodoc, vNumDoc);
                var prox = obtener_prox_pago(CodEmpresa, req.operacionid);

                var lineas = construir_lineas(req, afect, prox);

                var monto_total = calcular_monto_total(afect);

                var rInsert = insertar_transaccion(CodEmpresa, pConcepto, req, lineas, monto_total, vNumDoc);
                if (rInsert?.Code.HasValue == true && rInsert.Code != 0) return rInsert;

                var factor = (decimal)MProGrxMain.fxSys_Tipo_Cambio_Apl(tipocambio);

                var rAsientos = registrar_asientos(CodEmpresa, req, ctas, afect, tipocambio, factor, vNumDoc);
                if (rAsientos?.Code.HasValue == true && rAsientos.Code != 0) return rAsientos;

                var rPago = registrar_pago_final_si_corresponde(CodEmpresa, req, ctas, monto_total, vNumDoc);
                if (rPago?.Code.HasValue == true && rPago.Code != 0) return rPago;

                return new ErrorDto { Code = 0, Description = "Documento/Comprobante generado correctamente" };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        #region helpers CajasCrdAbonosCtP_DocumentoAbono_Generar 

        private CajasCrdOperacionCtasData obtener_cuentas_operacion(int codempresa, int operacionid)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdOperacionCtasData>(
                _portalDb,
                codempresa,
                @"exec spCrdOperacionCtas @operacionid",
                new CajasCrdOperacionCtasData(),
                new { operacionid }
            ).Result ?? new CajasCrdOperacionCtasData();
        }

        private CajasCrdDocumentoAfectacionData obtener_afectacion_documento(int codempresa, string tipodoc, string numdoc)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdDocumentoAfectacionData>(
                _portalDb,
                codempresa,
                @"exec spCrdDocumentoAfectacion @tipodocnum,@numdoc,'R'",
                new CajasCrdDocumentoAfectacionData(),
                new { tipodocnum = tipodoc, numdoc }
            ).Result ?? new CajasCrdDocumentoAfectacionData();
        }

        private CajasCrdOperacionProxPagoData obtener_prox_pago(int codempresa, int operacionid)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdOperacionProxPagoData>(
                _portalDb,
                codempresa,
                @"exec spCrdOperacionFechaProxPago @operacionid",
                new CajasCrdOperacionProxPagoData(),
                new { operacionid }
            ).Result ?? new CajasCrdOperacionProxPagoData();
        }

        private static decimal calcular_monto_total(CajasCrdDocumentoAfectacionData afect)
        {
            return afect.intcor + afect.intmor + afect.principal + afect.cargos + afect.polizas + afect.iva;
        }

        private DocumentoLineasDto construir_lineas(
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdDocumentoAfectacionData afect,
            CajasCrdOperacionProxPagoData prox)
        {
            var curamortiza = afect.principal;

            var linea1 = $"Saldo Anterior    ..: {fxStringRelleno(req.saldo_anterior.ToString("N2"), 15)}";
            var saldoactual = req.saldo_anterior - curamortiza;
            var linea2 = $"Saldo Actual      ..: {fxStringRelleno(saldoactual.ToString("N2"), 15)}";
            var linea3 = $"Interes Corriente ..: {fxStringRelleno(afect.intcor.ToString("N2"), 15)}";
            var linea4 = $"Interes Atrasado  ..: {fxStringRelleno(afect.intmor.ToString("N2"), 15)}";
            var linea5 = $"Amortización      ..: {fxStringRelleno(afect.principal.ToString("N2"), 15)}";
            var linea6 = $"Cargos Totales    ..: {fxStringRelleno(afect.cargos.ToString("N2"), 15)}";
            var linea7 = $"Pólizas           ..: {fxStringRelleno(afect.polizas.ToString("N2"), 15)}";
            var linea8 = $"Operacion/Línea   ..: Op.:{req.operacionid} L.:{req.codigo} Ret.:{(req.retencion ? "SI" : "NO")}";

            var linea9 = req.diferenciaaplenabled
                ? $"Aplica Diferencia ..: {req.diferenciaapltexto}"
                : $"Descripción       ..: {req.descripcion}";

            var linea10 = $"Notas: {prox.notas}";

            var linea11 = construir_linea11(req, afect.iva);

            return new DocumentoLineasDto
            {
                linea1 = linea1,
                linea2 = linea2,
                linea3 = linea3,
                linea4 = linea4,
                linea5 = linea5,
                linea6 = linea6,
                linea7 = linea7,
                linea8 = linea8,
                linea9 = linea9,
                linea10 = linea10,
                linea11 = linea11
            };
        }

        private static string construir_linea11(CajasCrdAbonosCtPRegistrarAbonoRequest req, decimal curiva)
        {
            if (curiva > 0)
                return $"Monto IVA         ..: {fxStringRelleno(curiva.ToString("N2"), 15)}";

            if (req.fechacancelacion_enabled && req.fechacancelacion.HasValue)
                return $"Fecha Real Abono  ..: {req.fechacancelacion.Value:dd/MM/yyyy}";

            return string.Empty;
        }

        private ErrorDto insertar_transaccion(
            int codempresa,
            string concepto,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            DocumentoLineasDto lineas,
            decimal monto_total, 
            string vNumDoc)
        {
            const string sql = @"
            insert SIF_TRANSACCIONES
            (
              COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
              Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
              Referencia_01, Referencia_02, Referencia_03, cod_oficina,
              linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,linea9,linea10,linea11,
              detalle, documento, cod_caja, cod_Apertura, id_Sesion
            )
            values
            (
              @numdoc, @tipodoc, dbo.MyGetdate(), @usuario,
              @cedula, @nombre, @concepto, @monto_total, 'P',
              @operacionid, @codigo, @ase_doc_deposito, @oficina_titular,
              @linea1,@linea2,@linea3,@linea4,@linea5,@linea6,@linea7,@linea8,@linea9,@linea10,@linea11,
              @detalle, @documento, @caja, @apertura, @sesionid
            );";

            var param = new
            {
                tipodoc = req.tipodoc,
                numdoc = vNumDoc,
                usuario = req.usuario,
                cedula = (req.cedula ?? "").Trim(),
                nombre = (req.nombre ?? "").Trim(),
                concepto = concepto,
                monto_total,
                operacionid = req.operacionid.ToString(),
                codigo = req.codigo,
                ase_doc_deposito = "",
                oficina_titular = "", // GLOBALES.gOficinaTitular
                linea1 = lineas.linea1,
                linea2 = lineas.linea2,
                linea3 = lineas.linea3,
                linea4 = lineas.linea4,
                linea5 = lineas.linea5,
                linea6 = lineas.linea6,
                linea7 = lineas.linea7,
                linea8 = lineas.linea8,
                linea9 = lineas.linea9,
                linea10 = lineas.linea10,
                linea11 = lineas.linea11,
                detalle = req.notas ?? "",
                documento = "",
                caja = req.mcaja,
                apertura = req.mapertura,
                sesionid = req.msesionid
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto registrar_asientos(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            CajasCrdDocumentoAfectacionData afect,
            decimal tipocambio,
            decimal factor,
            string vNumDoc)
        {
            var r = registrar_asiento_si_monto(codempresa, req, ctas, tipocambio, afect.intcor * factor, ctas.ctaintc, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = registrar_asiento_si_monto(codempresa, req, ctas, tipocambio, afect.intmor * factor, ctas.ctaintm, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = registrar_asiento_si_monto(codempresa, req, ctas, tipocambio, afect.iva * factor, ctas.ctaiva, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = registrar_asiento_si_monto(codempresa, req, ctas, tipocambio, afect.principal * factor, ctas.ctaamortiza, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = registrar_asientos_cargos(codempresa, req, ctas, tipocambio, afect.cargos, factor, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = registrar_asientos_polizas(codempresa, req, ctas, tipocambio, afect.polizas, factor, vNumDoc);
            return r;
        }

        private ErrorDto registrar_asiento_si_monto(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal monto,
            string cuenta,
            string vNumDoc)
        {
            if (monto == 0 || string.IsNullOrWhiteSpace(cuenta))
                return new ErrorDto { Code = 0, Description = string.Empty };

            const string sql = @"exec spSIFDocsAsiento
                         @tipodoc,@numdoc,@monto,'C',@cod_divisa,
                         @tipocambio,@enlace,@cod_unidad,@cod_centro_costo,@cuenta,
                         @id_solicitud,@codigo_cta,@ase_doc_deposito";

            var param = new
            {
                tipodoc = req.tipodoc,
                numdoc = vNumDoc,
                monto,
                cod_divisa = ctas.cod_divisa,
                tipocambio,
                enlace = 0, // GLOBALES.gEnlace
                cod_unidad = ctas.cod_unidad,
                cod_centro_costo = ctas.cod_centro_costo,
                cuenta,
                id_solicitud = ctas.id_solicitud,
                codigo_cta = ctas.codigo,
                ase_doc_deposito = ""
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto registrar_asientos_cargos(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal curcargo,
            decimal factor,
            string vNumDoc)
        {
            if (curcargo == 0)
                return new ErrorDto { Code = 0, Description = string.Empty };

            var cargos = DbHelper.ExecuteListQuery<CajasCrdDocAfectacionCargoRow>(
                _portalDb,
                codempresa,
                @"exec spCrdDocumentoAfectacionCargos @tipodoc,@numdoc",
                new { tipodoc = req.tipodoc, numdoc = vNumDoc }
            ).Result ?? new List<CajasCrdDocAfectacionCargoRow>();

            foreach (var c in cargos)
            {
                var monto = c.mov_monto.HasValue ? c.mov_monto.Value * factor : curcargo;

                var r = registrar_asiento_cargo(codempresa, req, ctas, tipocambio, monto, c, vNumDoc);
                if (r?.Code.HasValue == true && r.Code != 0) return r;
            }

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto registrar_asiento_cargo(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal monto,
            CajasCrdDocAfectacionCargoRow cargo,
            string vNumDoc)
        {
            const string sql = @"exec spSIFDocsAsiento
                         @tipodoc,@numdoc,@monto,'C',@cod_divisa,
                         @tipocambio,@enlace,@cod_unidad,@cod_centro_costo,@cuenta,
                         @id_solicitud,@codigo_cta,@ase_doc_deposito";

            var param = new
            {
                tipodoc = req.tipodoc,
                numdoc = vNumDoc,
                monto,
                cod_divisa = ctas.cod_divisa,
                tipocambio,
                enlace = 0, // GLOBALES.gEnlace
                cod_unidad = cargo.cod_unidad,
                cod_centro_costo = cargo.cod_centro_costo,
                cuenta = cargo.cod_cuenta,
                id_solicitud = cargo.id_solicitud,
                codigo_cta = cargo.codigo,
                ase_doc_deposito = ""
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto registrar_asientos_polizas(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal curpoliza,
            decimal factor,
            string vNumDoc)
        {
            if (curpoliza <= 0)
                return new ErrorDto { Code = 0, Description = string.Empty };

            var polizas = DbHelper.ExecuteListQuery<CajasCrdDocAfectacionPolizaRow>(
                _portalDb,
                codempresa,
                @"exec spCrdDocumentoAfectacionPolizas @tipodoc,@numdoc",
                new { tipodoc = req.tipodoc, numdoc = vNumDoc }
            ).Result ?? new List<CajasCrdDocAfectacionPolizaRow>();

            foreach (var p in polizas)
            {
                var monto = p.mov_monto * factor;

                var r = registrar_asiento_poliza(codempresa, req, ctas, tipocambio, monto, p.cod_cuenta, vNumDoc);
                if (r?.Code.HasValue == true && r.Code != 0) return r;
            }

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto registrar_asiento_poliza(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal monto,
            string cuenta, 
            string vNumDoc)
        {
            const string sql = @"exec spSIFDocsAsiento
                         @tipodoc,@numdoc,@monto,'C',@cod_divisa,
                         @tipocambio,@enlace,@cod_unidad,@cod_centro_costo,@cuenta,
                         @id_solicitud,@codigo_cta,@ase_doc_deposito";

            var param = new
            {
                tipodoc = req.tipodoc,
                numdoc = vNumDoc,
                monto,
                cod_divisa = ctas.cod_divisa,
                tipocambio,
                enlace = 0, // GLOBALES.gEnlace
                cod_unidad = ctas.cod_unidad,
                cod_centro_costo = ctas.cod_centro_costo,
                cuenta,
                id_solicitud = ctas.id_solicitud,
                codigo_cta = ctas.codigo,
                ase_doc_deposito = ""
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto registrar_pago_final_si_corresponde(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal monto_total,
            string vNumDoc)
        {
            if (monto_total == 0)
                return new ErrorDto { Code = 0, Description = string.Empty };

            const string sql = @"exec spCajas_DesglocePagosDocFinal
                         @caja,@apertura,@tiquete,@usuario,@tipodoc,@numdoc,@unidad,@id_solicitud,@codigo_cta";

            var param = new
            {
                caja = req.mcaja,
                apertura = req.mapertura,
                tiquete = req.mtiquete,
                usuario = req.usuario,
                tipodoc = req.tipodoc,
                numdoc = vNumDoc,
                unidad = req.munidad,
                id_solicitud = ctas.id_solicitud,
                codigo_cta = ctas.codigo
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private sealed class DocumentoLineasDto
        {
            public string linea1 { get; set; } = "";
            public string linea2 { get; set; } = "";
            public string linea3 { get; set; } = "";
            public string linea4 { get; set; } = "";
            public string linea5 { get; set; } = "";
            public string linea6 { get; set; } = "";
            public string linea7 { get; set; } = "";
            public string linea8 { get; set; } = "";
            public string linea9 { get; set; } = "";
            public string linea10 { get; set; } = "";
            public string linea11 { get; set; } = "";
        }

        #endregion


    }
}
