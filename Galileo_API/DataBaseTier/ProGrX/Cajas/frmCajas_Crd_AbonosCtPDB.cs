using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cajas;
using System.Diagnostics;

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
            : this(
                new PortalDB(config),
                new MSecurityMainDb(config),
                new MCajas(config),
                new MProGrxMain(config),
                new MRecibos(config),
                new MAfilicacionDB(config))
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

        /// <summary>
        /// Obtiene los datos de la operacion de credito para abonos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodCaja"></param>
        /// <param name="OperacionId"></param>
        /// <returns></returns>
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
                    var updateSQl = "update reg_creditos set saldo_mes = saldo where id_solicitud = @IdSolicitud";
                    conn.Execute(updateSQl, new { saldo_mes = op.Saldo_mes, IdSolicitud = op.id_solicitud });

                    op.Saldo_mes = op.saldo;
                }

                return op;
            });
        }

        /// <summary>
        /// Obtiene lista de operaciones de credito para abonos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCrdAbonosCtPData>> CajasCrdAbonosCtP_Operaciones_Obtener(int CodEmpresa)
        {
            const string query = @"SELECT R.id_solicitud,R.Codigo,S.Cedula,S.Nombre,C.Descripcion 
                from REG_CREDITOS R inner join SOCIOS S on R.cedula = S.cedula 
                inner join Catalogo C on R.codigo = C.codigo 
                WHERE R.estado = 'A' ORDER BY R.cedula";

            return DbHelper.ExecuteListQuery<CajasCrdAbonosCtPData>(_portalDb, CodEmpresa, query);
        }

        /// <summary>
        /// Obtiene los tipos de documento para abonos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Caja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosCtP_TipoDoc_Obtener(int CodEmpresa, string Caja)
        {
            const string query = @"select rTrim(C.tipo_documento) as item, rtrim(D.Descripcion) as descripcion
                from SIF_DOCUMENTOS D inner join CAJAS_DOCUMENTOS C on D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO  
                Where C.cod_caja = @Caja and D.Tipo_Movimiento in('A','C') 
                order by C.tipo_documento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, CodEmpresa, query, new { Caja });
        }

        /// <summary>
        /// Obtiene las transacciones de la operacion de credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="IdSolicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCrdOperacionTransacData>> CajasCrdAbonosCtP_OperacionTransac_Obtener(int CodEmpresa, int IdSolicitud)
        {
            const string sql = @"select * from CRD_OPERACION_TRANSAC 
                where estado = 'A' and id_solicitud = @IdSolicitud 
                and Fecha_Inicio < GETDATE() order by ID_SEQ asc";

            return DbHelper.ExecuteListQuery<CajasCrdOperacionTransacData>(_portalDb, CodEmpresa, sql, new {IdSolicitud});
        }

        /// <summary>
        /// Obtiene los dias activos hasta una fecha determinada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene la informacion de cancelacion de la operacion de credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene la informacion de las cuotas de la operacion de credito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="vOperacion"></param>
        /// <param name="vCuotas"></param>
        /// <returns></returns>
        public ErrorDto<CajasCrdAbonosCuotasInfoData> CajasCrdAbonosCtP_CuotasInfo_Obtener(int codEmpresa, int vOperacion, int vCuotas)
        {
            const string sqlTotales = @"
            SELECT
                ISNULL(MAX(id_Seq),0)                AS seqX,
                ISNULL(SUM(IntCor + IntMor),0)       AS intCor,
                ISNULL(SUM(Principal),0)             AS principal,
                ISNULL(MIN(Saldo_Actual),0)          AS saldo,
                ISNULL(MAX(Fecha_Proceso),0)         AS fecha_Proceso,
                ISNULL(SUM(Poliza),0)                AS poliza,
                ISNULL(SUM(IVA),0)                   AS iva
            FROM CRD_OPERACION_PLAN_PAGOS
            WHERE id_solicitud = @OperacionId
              AND Id_Seq IN
              (
                  SELECT TOP (@Cuotas) Id_Seq
                  FROM CRD_OPERACION_PLAN_PAGOS
                  WHERE estado IN ('A','P')
                    AND id_solicitud = @OperacionId
                    AND num_cuota > 0
                  ORDER BY num_cuota, Id_Seq
              );
            ";


            var totales = DbHelper.ExecuteSingleQuery<CajasCrdAbonosCuotasInfoData>(
                _portalDb,
                codEmpresa,
                sqlTotales,
                new CajasCrdAbonosCuotasInfoData(),
                new
                {
                    OperacionId = vOperacion,
                    Cuotas = vCuotas
                }
            );

            if (totales.Result == null)
                totales.Result = new CajasCrdAbonosCuotasInfoData();

            const string sqlCuota = @"
                SELECT ISNULL(cuota, 0) AS Cuota
                FROM CRD_OPERACION_PLAN_PAGOS
                WHERE id_seq = @IdSeq
                  AND id_solicitud = @OperacionId;
            ";

            if (totales.Result.seqX > 0)
            {
                var cuotaRs = DbHelper.ExecuteSingleQuery<decimal>(
                    _portalDb,
                    codEmpresa,
                    sqlCuota,
                    0,
                    new
                    {
                        IdSeq = totales.Result.seqX,
                        OperacionId = vOperacion
                    }
                );

                if (cuotaRs == null || cuotaRs.Code != 0)
                    return new ErrorDto<CajasCrdAbonosCuotasInfoData>
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
        /// Obtener fecha de proceso siguiente o anterior
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Proceso"></param>
        /// <param name="Siguiente"></param>
        /// <returns></returns>
        public ErrorDto<int> CajasCrdAbonosCtP_FechaProceso_Obtener(int CodEmpresa, int Proceso, bool Siguiente)
        {
            string sql = "select dbo.fxSIFPrmProcesoSig(@Proceso) as 'Result'";
            if (!Siguiente)
            {
                sql = "select dbo.fxSIFPrmProcesoAnt(@Proceso) as 'Result'";
            }

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                CodEmpresa,
                sql, 0,
                new
                {
                    Proceso
                }
            );
        }

        /// <summary>
        /// Registra un abono a la operacion de credito
        /// </summary>
        /// <param name="codempresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CajasCrdAbonosCtP_Abono_Registrar(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            try
            {
                req.notas = MProGrxMain.sbSIFCleanTxtInject(req.notas);

                var ver = FxVerifica(codempresa, req);
                if (FailIfError(ver, out var e)) return e;

                var vNumDoc = _mRecibos.FxDocumentoConsecutivo(codempresa, req.tipodoc).ToString();

                var extraordinario = false;
                var r = ProcesarRegistroAbono(codempresa, req, vNumDoc, ref extraordinario);
                if (FailIfError(r, out e)) return e;

                // Reproceso por extraordinario
                if (extraordinario)
                {
                    var rp = Exec(codempresa,
                        @"exec spCrdPlanPagos @Operacion;
                  exec spCrdPlanPagosActivaCuota @Operacion,0;
                  exec spCrdPlanPagosMoraActualizaOp @Operacion;",
                        new { Operacion = req.operacionid });

                    if (FailIfError(rp, out e)) return e;
                }

                var(bitacoraDesc, comprobanteConcepto) = ObtenerDescripcionComprobante(req);

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codempresa,
                    Usuario = (req.usuario ?? "").ToUpper(),
                    DetalleMovimiento = bitacoraDesc,
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                var resp = CajasCrdAbonosCtP_DocumentoAbono_Generar(
                    codempresa, comprobanteConcepto, req, vNumDoc);

                if (FailIfError(resp, out e)) return e;

                // Documento electrónico (si aplica)
                var docElectronico = "";
                if (req.factura_visible)
                {
                    docElectronico = ProcesarDocumentoElectronico(codempresa, req, vNumDoc) ?? "";
                }

                var mensajeFinal = ProcesarRecibo(codempresa, req, docElectronico, vNumDoc);

                // Trazabilidad
                var codDoc = req.tipodoc == "CA" ? "07" : "05";
                var consecutivo = req.tipodoc == "CA" ? vNumDoc : $"{vNumDoc}-{req.tipodoc}";

                var rt = _mProGrxMain.sbTrazabilidad_Inserta(
                    codempresa, codDoc, consecutivo, vNumDoc, req.usuario ?? "", nuevo: true);

                if (FailIfError(rt, out e)) return e;

                return new ErrorDto { Code = 0, Description = mensajeFinal + vNumDoc };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private static bool FailIfError(ErrorDto? r, out ErrorDto err)
        {
            if (r?.Code.HasValue == true && r.Code != 0)
            {
                err = r;
                return true;
            }

            err = new ErrorDto { Code = 0, Description = "" };
            return false;
        }

        private ErrorDto ProcesarRegistroAbono(int CodEmpresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            return req.tipoabono switch
            {
                TipoAbono.Ordinario => RegistrarOrdinario(CodEmpresa, req, vNumDoc, ref extraordinario),
                TipoAbono.Extraordinario => RegistrarExtraordinario(CodEmpresa, req, vNumDoc, ref extraordinario),
                TipoAbono.Cancelacion => RegistrarCancelacion(CodEmpresa, req, vNumDoc),
                TipoAbono.AdelantoCuotas => RegistrarAdelantoCuotas(CodEmpresa, req, vNumDoc),
                _ => new ErrorDto { Code = -2, Description = "Tipo de abono no soportado" }
            };
        }

        private static (string bitacoraDesc, string comprobanteConcepto) ObtenerDescripcionComprobante(CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            return req.tipoabono switch
            {
                TipoAbono.Ordinario => (
                    $"Abono Ordinario a la Operacion : {req.operacionid}",
                    "CRD001"
                ),
                TipoAbono.Extraordinario => (
                    $"Abono ExtraOrd. {(req.recalculacuota ? "Con Recal." : "Sin Recal")} a la Op.: {req.operacionid}",
                    "CRD002"
                ),
                TipoAbono.Cancelacion => (
                    $"Cancelación de la Operacion : {req.operacionid}",
                    "CRD003"
                ),
                TipoAbono.AdelantoCuotas => (
                    $"Adelanto de Cuotas de la Operacion : {req.operacionid}",
                    "CRD004"
                ),
                _ => (
                    $"Movimiento no identificado para la Operacion : {req.operacionid}",
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
                @"exec spCajas_Credito_TE @tipoDoc, @numdoc, @tipo",
                new
                {
                    tipoDoc = req.tipodoc,
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
                    @"exec spCajasReciboDigital @numDoc, @tipoDoc, 'Creditos'",
                    new
                    {
                        numDoc = vNumDoc,
                        tipoDoc = req.tipodoc
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

        public ErrorDto FxVerifica(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            try
            {
                var mensajes = new List<string>();

                AgregarValidacionesControlYCajamov(codempresa, req, mensajes);
                AgregarValidacionesNegocio(req, mensajes);
                AgregarValidacionesMontos(req, mensajes);
                AgregarValidacionCajas(codempresa, req, mensajes);

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

        private void AgregarValidacionesControlYCajamov(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            List<string> mensajes)
        {
            const string sqlControl = @"
                select 
                  dbo.fxCrd_Operacion_Control(@Operacion) as control,
                  dbo.fxCrd_Operacion_Movimientos_Cajas_Acepta(@Operacion) as cajamov;";

            var data = DbHelper.ExecuteSingleQuery<dynamic>(
                _portalDb,
                codempresa,
                sqlControl,
                null,
                new { Operacion = req.operacionid }
            ).Result;

            decimal vcontrol = data?.control == null ? 0m : (decimal)data.control;
            bool vcajasmov = (data?.cajamov is not null) && Convert.ToInt32(data.cajamov) == 1;

            if (vcontrol != req.control)
                mensajes.Add("Esta Operación ha sido cambiada por otro proceso, vuelva a consultarla!");

            if (!vcajasmov)
                mensajes.Add("Esta Operación -> No Permite Movimientos en Cajas! Puede que sea recaudo de ahorros o porque el código de linea no lo admite, revise!");

            if (req.proceso == "J" && !_mCajas.fxCajasAbonosCbrJud(codempresa, req.mcaja, req.usuario))
                mensajes.Add("Esta CAJA no cuenta con permisos para realizar abonos a Creditos en Cobro Judicial, verifique...");

            if (req.diferencia < 0 && (req.saldo_nuevo + req.diferencia) < 0)
                mensajes.Add("La diferencia supera el saldo!, verifique...");

            if (_mAfiliacion.fxgCongelamiento(codempresa, req.cedula, "per_abono_cajas"))
                mensajes.Add("Esta Persona se encuentra CONGELADA, verifique...");

            if (req.operacionid == 0)
                mensajes.Add("Número de Operacion no es válido...");

            using var conn = DbHelper.OpenConnection(_portalDb, codempresa);
            if (!MCredito.fxCrdSaldoVerifica(conn, req.operacionid, req.saldo_anterior))
                mensajes.Add("Esta Operación ha sido modificada, actualice los datos nuevamente antes de realizar el abono...");
        }

        private static void AgregarValidacionesNegocio(
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            List<string> mensajes)
        {
            if (!req.retencion)
            {
                if (req.datosamortiza > req.saldo_anterior)
                    mensajes.Add("La Amortización es mayor al Saldo Actual...");
            }
            else if (req.plazo < 999 && req.datosamortiza > req.saldo_anterior)
            {
                mensajes.Add($"La Amortización es mayor que el Remanente a Recaudar : {req.saldo_anterior:N2}");
            }

            if (mensajes.Count > 0)
                return;

            if (req.tipoabono == TipoAbono.Extraordinario && req.diferencia != 0)
                mensajes.Add("El Monto detallado en formas de pago no cubre el compromiso de pago. SOLUCION: Copie el Monto detallado en el monto del abono extraordinario...!");

            if (req.tipoabono == TipoAbono.Cancelacion && req.diferencia != 0)
                mensajes.Add("El Monto detallado en formas de pago no cubre el compromiso de cancelación...!");
        }

        private static void AgregarValidacionesMontos(
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            List<string> mensajes)
        {
            if (req.totalpagar <= 0)
                mensajes.Add("El total a pagar no es un dato válido...verifique...!");

            if (req.totalcajas <= 0)
                mensajes.Add("Los valores Recibidos en Cajas no son válidos...verifique...!");

            if (mensajes.Count == 0 && req.totalcajas != req.totalpagar)
                mensajes.Add("Los valores Recibidos en Cajas son diferentes al monto a Pagar establecido para el Abono...!");
        }

        private void AgregarValidacionCajas(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            List<string> mensajes)
        {
            string estadoCaja = _mCajas.fxCajasAperturaEstado(codempresa, req.mcaja, req.mapertura);
            if (estadoCaja == "C")
                mensajes.Add($"La apertura ..:{req.mapertura} de esta caja ha sido cerrada!");

            const string sqlVal = @"
            exec spCajas_Transac_Validacion 
              @caja, @Usuario, @apertura, @sesionid, 
              'Crd', @codigo, @monto, @tiquete;";

            var val = DbHelper.ExecuteSingleQuery<dynamic>(
                _portalDb,
                codempresa,
                sqlVal,
                null,
                new
                {
                    caja = req.mcaja,
                    Usuario = req.usuario,
                    apertura = req.mapertura,
                    sesionid = req.msesionid,
                    codigo = (req.codigo ?? "").Trim(),
                    monto = req.totalcajas,
                    tiquete = req.mtiquete
                }
            ).Result;

            string? validacion = (val?.Validacion as string) ?? (val?.validacion as string);
            if (!string.IsNullOrWhiteSpace(validacion))
                mensajes.Add(validacion);
        }

        private ErrorDto RegistrarOrdinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            if (!req.diferenciaaplenabled)
            {
                return Exec(codempresa,
                    @"exec spCrdPlanPagoAbonoOrdinario 
                    @Operacion,'CRD001',@Usuario,@TipoDoc,@numdoc,@monto,@fecha,''",
                    new
                    {
                        Operacion = req.operacionid,
                        Usuario = req.usuario,
                        TipoDoc = req.tipodoc,
                        numdoc = vNumDoc,
                        monto = req.totalcajas,
                        fecha = DateTime.Now
                    });
            }

            var r1 = Exec(codempresa,
                @"exec spCrdPlanPagoAbonoOrdinario 
                    @Operacion,'CRD001',@Usuario,@TipoDoc,@numdoc,@monto,@fecha,''",
                new
                {
                    Operacion = req.operacionid,
                    Usuario = req.usuario,
                    TipoDoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = req.totalcancela,
                    fecha = DateTime.Now
                });

            if (r1?.Code.HasValue == true && r1.Code != 0)
                return r1;

            return AplicarDiferenciaOrdinario(codempresa, req, vNumDoc, ref extraordinario);
        }

        private ErrorDto AplicarDiferenciaOrdinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            if (req.diferenciaapltexto == "Adelanto de Cuota")
                return AplicarAdelantoCuota(codempresa, req, vNumDoc);

            if (req.diferenciaapltexto == "Abono Extraordinario")
                return AplicarExtraordinarioDesdeDiferencia(codempresa, req, vNumDoc, ref extraordinario);

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto AplicarAdelantoCuota(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            return Exec(codempresa,
                @"exec spCrdPlanPagoAbonoOrdinario 
          @Operacion,'CRD004',@Usuario,@TipoDoc,@numdoc,@monto,@fecha,''",
                new
                {
                    Operacion = req.operacionid,
                    Usuario = req.usuario,
                    TipoDoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = Math.Abs(req.diferencia),
                    fecha = DateTime.Now
                });
        }

        private ErrorDto AplicarExtraordinarioDesdeDiferencia(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            var info = GetInfoExtraordinario(codempresa, req);
            var rCargo = AplicarCargoSiCorresponde(codempresa, req, info.cargos, "Pago Anticipado");
            if (rCargo?.Code.HasValue == true && rCargo.Code != 0)
                return rCargo;

            var rAbono = Exec(codempresa,
                @"exec spCrdPlanPagoAbonoEC 
                  @Operacion,'CRD002',@Usuario,@tipoDoc,@numdoc,
                  @Dias,@Intereses,@Principal,@Cargos,@fecha,'',@recalcula",
                new
                {
                    Operacion = req.operacionid,
                    Usuario = req.usuario,
                    tipoDoc = req.tipodoc,
                    numdoc = vNumDoc,
                    Dias = info.dias,
                    Intereses = info.intereses,
                    Principal = info.principal,
                    Cargos = info.cargos,
                    fecha = DateTime.Now,
                    recalcula = req.recalculacuota ? 1 : 0
                });

            if (rAbono?.Code.HasValue == true && rAbono.Code != 0)
                return rAbono;

            extraordinario = true;
            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto RegistrarExtraordinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc, ref bool extraordinario)
        {
            var rCargo = AplicarCargoSiCorresponde(codempresa, req, req.datosanticipo, "Pago Anticipado");
            if (rCargo?.Code.HasValue == true && rCargo.Code != 0)
                return rCargo;

            var r = Exec(codempresa,
                @"exec spCrdPlanPagoAbonoEC 
          @operacion,'CRD002',@Usuario,@tipoDoc,@numdoc,
          @dias,@intereses,@principal,0,@fecha,'',@recalcula,
          1,0,@Iva",
                new
                {
                    operacion = req.operacionid,
                    Usuario = req.usuario,
                    tipoDoc = req.tipodoc,
                    numdoc = vNumDoc,
                    dias = req.diasactivo,
                    intereses = req.datosinteres,
                    principal = req.datosamortiza,
                    fecha = DateTime.Now,
                    recalcula = req.recalculacuota ? 1 : 0,
                    Iva = req.iva
                });

            if (r?.Code.HasValue == true && r.Code != 0)
                return r;

            extraordinario = true;
            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto RegistrarCancelacion(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            var r1 = Exec(codempresa,
                @"exec spCrdPlanPagosMoraActualizaOp 
          @operacion,@fecha",
                new
                {
                    operacion = req.operacionid,
                    fecha = req.fechacancelacion ?? DateTime.Now
                });

            if (r1?.Code.HasValue == true && r1.Code != 0)
                return r1;

            var rCargo = AplicarCargoSiCorresponde(codempresa, req, req.datosanticipo, "Cancelacion Anticipada");
            if (rCargo?.Code.HasValue == true && rCargo.Code != 0)
                return rCargo;

            return Exec(codempresa,
                @"exec spCrdPlanPagoAbonoCancelacion 
          @operacion,'CRD003',@Usuario,@tipoDoc,@numdoc,@monto,@fecha,''",
                new
                {
                    operacion = req.operacionid,
                    Usuario = req.usuario,
                    tipoDoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = req.totalcajas,
                    fecha = req.fechacancelacion ?? DateTime.Now
                });
        }

        private ErrorDto RegistrarAdelantoCuotas(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            return Exec(codempresa,
                @"exec spCrdPlanPagoAbonoOrdinario 
          @operacion,'CRD004',@Usuario,@tipoDoc,@numdoc,@monto,@fecha,''",
                new
                {
                    operacion = req.operacionid,
                    Usuario = req.usuario,
                    tipoDoc = req.tipodoc,
                    numdoc = vNumDoc,
                    monto = req.totalcajas,
                    fecha = DateTime.Now
                });
        }

        private CajasCrdInfoExtraordinarioData GetInfoExtraordinario(int codempresa, CajasCrdAbonosCtPRegistrarAbonoRequest req)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdInfoExtraordinarioData>(
                _portalDb,
                codempresa,
                @"exec spCrdPlanPagosInfoExtraordinario 
          @operacion,@monto,@fechacancel",
                new CajasCrdInfoExtraordinarioData(),
                new
                {
                    operacion = req.operacionid,
                    monto = Math.Abs(req.diferencia),
                    fechacancel = req.fechacancelacion ?? DateTime.Now
                }
            ).Result ?? new CajasCrdInfoExtraordinarioData();
        }

        private ErrorDto AplicarCargoSiCorresponde(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            decimal cargos,
            string motivo)
        {
            if (cargos <= 0)
                return new ErrorDto { Code = 0, Description = string.Empty };

            return Exec(codempresa,
                @"exec spCrdOperacionCargoAdd 
          @operacion,@cargos,@unidad,@centrocosto,
          @motivo,@Usuario,'CA','','',0",
                new
                {
                    operacion = req.operacionid,
                    cargos,
                    unidad = req.oficinaunidad,
                    centrocosto = req.oficinacentrocosto,
                    motivo,
                    Usuario = req.usuario
                });
        }

        private ErrorDto Exec(int codempresa, string sql, object param)
        {
            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private static string FxStringRelleno(string texto, int largo)
        {
            texto ??= "";
            return texto.Length >= largo ? texto[..largo] : texto.PadLeft(largo, ' ');
        }

        #endregion


        public ErrorDto CajasCrdAbonosCtP_DocumentoAbono_Generar(int CodEmpresa, string pConcepto, CajasCrdAbonosCtPRegistrarAbonoRequest req, string vNumDoc)
        {
            try
            {
                var tipocambio = _mCajas.fxCajasTipoCambio(CodEmpresa, 0, req.divisa);

                var ctas = ObtenerCuentasOperacion(CodEmpresa, req.operacionid);
                var afect = ObtenerAfectacionDocumento(CodEmpresa, req.tipodoc, vNumDoc);
                var prox = ObtenerProxPago(CodEmpresa, req.operacionid);

                var lineas = ConstruirLineas(req, afect, prox);

                var monto_total = CalcularMontoTotal(afect);

                var rInsert = InsertarTransaccion(CodEmpresa, pConcepto, req, lineas, monto_total, vNumDoc);
                if (rInsert?.Code.HasValue == true && rInsert.Code != 0) return rInsert;

                var factor = (decimal)MProGrxMain.fxSys_Tipo_Cambio_Apl(tipocambio);

                var rAsientos = RegistrarAsientos(CodEmpresa, req, ctas, afect, tipocambio, factor, vNumDoc);
                if (rAsientos?.Code.HasValue == true && rAsientos.Code != 0) return rAsientos;

                var rPago = RegistrarPagoFinalSiCorresponde(CodEmpresa, req, ctas, monto_total, vNumDoc);
                if (rPago?.Code.HasValue == true && rPago.Code != 0) return rPago;

                return new ErrorDto { Code = 0, Description = "Documento/Comprobante generado correctamente" };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        #region helpers CajasCrdAbonosCtP_DocumentoAbono_Generar 

        private CajasCrdOperacionCtasData ObtenerCuentasOperacion(int codempresa, int operacionid)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdOperacionCtasData>(
                _portalDb,
                codempresa,
                @"exec spCrdOperacionCtas @operacionid",
                new CajasCrdOperacionCtasData(),
                new { operacionid }
            ).Result ?? new CajasCrdOperacionCtasData();
        }

        private CajasCrdDocumentoAfectacionData ObtenerAfectacionDocumento(int codempresa, string tipodoc, string numdoc)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdDocumentoAfectacionData>(
                _portalDb,
                codempresa,
                @"exec spCrdDocumentoAfectacion @tipodocnum,@numdoc,'R'",
                new CajasCrdDocumentoAfectacionData(),
                new { tipodocnum = tipodoc, numdoc }
            ).Result ?? new CajasCrdDocumentoAfectacionData();
        }

        private CajasCrdOperacionProxPagoData ObtenerProxPago(int codempresa, int operacionid)
        {
            return DbHelper.ExecuteSingleQuery<CajasCrdOperacionProxPagoData>(
                _portalDb,
                codempresa,
                @"exec spCrdOperacionFechaProxPago @operacionid",
                new CajasCrdOperacionProxPagoData(),
                new { operacionid }
            ).Result ?? new CajasCrdOperacionProxPagoData();
        }

        private static decimal CalcularMontoTotal(CajasCrdDocumentoAfectacionData afect)
        {
            return afect.intcor + afect.intmor + afect.principal + afect.cargos + afect.polizas + afect.iva;
        }

        private static DocumentoLineasDto ConstruirLineas(
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdDocumentoAfectacionData afect,
            CajasCrdOperacionProxPagoData prox)
        {
            var curamortiza = afect.principal;

            var linea1 = $"Saldo Anterior    ..: {FxStringRelleno(req.saldo_anterior.ToString("N2"), 15)}";
            var saldoactual = req.saldo_anterior - curamortiza;
            var linea2 = $"Saldo Actual      ..: {FxStringRelleno(saldoactual.ToString("N2"), 15)}";
            var linea3 = $"Interes Corriente ..: {FxStringRelleno(afect.intcor.ToString("N2"), 15)}";
            var linea4 = $"Interes Atrasado  ..: {FxStringRelleno(afect.intmor.ToString("N2"), 15)}";
            var linea5 = $"Amortización      ..: {FxStringRelleno(afect.principal.ToString("N2"), 15)}";
            var linea6 = $"Cargos Totales    ..: {FxStringRelleno(afect.cargos.ToString("N2"), 15)}";
            var linea7 = $"Pólizas           ..: {FxStringRelleno(afect.polizas.ToString("N2"), 15)}";
            var linea8 = $"Operacion/Línea   ..: Op.:{req.operacionid} L.:{req.codigo} Ret.:{(req.retencion ? "SI" : "NO")}";

            var linea9 = req.diferenciaaplenabled
                ? $"Aplica Diferencia ..: {req.diferenciaapltexto}"
                : $"Descripción       ..: {req.descripcion}";

            var linea10 = $"Notas: {prox.notas}";

            var linea11 = ConstruirLineas11(req, afect.iva);

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

        private static string ConstruirLineas11(CajasCrdAbonosCtPRegistrarAbonoRequest req, decimal curiva)
        {
            if (curiva > 0)
                return $"Monto IVA         ..: {FxStringRelleno(curiva.ToString("N2"), 15)}";

            if (req.fechacancelacion_enabled && req.fechacancelacion.HasValue)
                return $"Fecha Real Abono  ..: {req.fechacancelacion.Value:dd/MM/yyyy}";

            return string.Empty;
        }

        private ErrorDto InsertarTransaccion(
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
              @numdoc, @tipoDoc, GetDate(), @Usuario,
              @cedula, @nombre, @Concepto, @monto_total, 'P',
              @operacionid, @Codigo, @ase_doc_deposito, @oficina_titular,
              @linea1,@linea2,@linea3,@linea4,@linea5,@linea6,@linea7,@linea8,@linea9,@linea10,@linea11,
              @detalle, @documento, @caja, @apertura, @sesionid
            );";

            var param = new
            {
                tipoDoc = req.tipodoc,
                numdoc = vNumDoc,
                Usuario = req.usuario,
                cedula = (req.cedula ?? "").Trim(),
                nombre = (req.nombre ?? "").Trim(),
                Concepto = concepto,
                monto_total,
                operacionid = req.operacionid.ToString(),
                Codigo = req.codigo,
                ase_doc_deposito = "",
                oficina_titular = "", // GLOBALES.gOficinaTitular
                lineas.linea1,
                lineas.linea2,
                lineas.linea3,
                lineas.linea4,
                lineas.linea5,
                lineas.linea6,
                lineas.linea7,
                lineas.linea8,
                lineas.linea9,
                lineas.linea10,
                lineas.linea11,
                detalle = req.notas ?? "",
                documento = "",
                caja = req.mcaja,
                apertura = req.mapertura,
                sesionid = req.msesionid
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto RegistrarAsientos(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            CajasCrdDocumentoAfectacionData afect,
            decimal tipocambio,
            decimal factor,
            string vNumDoc)
        {
            var r = RegistrarAsientoSiMonto(codempresa, req, ctas, tipocambio, afect.intcor * factor, ctas.ctaintc, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = RegistrarAsientoSiMonto(codempresa, req, ctas, tipocambio, afect.intmor * factor, ctas.ctaintm, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = RegistrarAsientoSiMonto(codempresa, req, ctas, tipocambio, afect.iva * factor, ctas.ctaiva, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = RegistrarAsientoSiMonto(codempresa, req, ctas, tipocambio, afect.principal * factor, ctas.ctaamortiza, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = RegistrarAsientos_cargos(codempresa, req, ctas, tipocambio, afect.cargos, factor, vNumDoc);
            if (r?.Code.HasValue == true && r.Code != 0) return r;

            r = RegistrarAsientos_polizas(codempresa, req, ctas, tipocambio, afect.polizas, factor, vNumDoc);
            return r;
        }

        private ErrorDto RegistrarAsientoSiMonto(
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
                         @tipoDoc,@numdoc,@monto,'C',@codDivisa,
                         @tipocambio,@enlace,@codUnidad,@codCentroCosto,@cuenta,
                         @idSolicitud,@codigo_cta,@ase_doc_deposito";

            var param = new
            {
                tipoDoc = req.tipodoc,
                numdoc = vNumDoc,
                monto,
                codDivisa = ctas.cod_divisa,
                tipocambio,
                enlace = 0, // GLOBALES.gEnlace
                codUnidad = ctas.cod_unidad,
                codCentroCosto = ctas.cod_centro_costo,
                cuenta,
                idSolicitud = ctas.id_solicitud,
                codigo_cta = ctas.codigo,
                ase_doc_deposito = ""
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto RegistrarAsientos_cargos(
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
                @"exec spCrdDocumentoAfectacionCargos @tipoDoc,@numDoc",
                new { tipoDoc = req.tipodoc, numDoc = vNumDoc }
            ).Result ?? [];

            foreach (var c in cargos)
            {
                var monto = c.mov_monto.HasValue ? c.mov_monto.Value * factor : curcargo;

                var r = RegistrarAsientoCargo(codempresa, req, ctas, tipocambio, monto, c, vNumDoc);
                if (r?.Code.HasValue == true && r.Code != 0) return r;
            }

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto RegistrarAsientoCargo(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal monto,
            CajasCrdDocAfectacionCargoRow cargo,
            string vNumDoc)
        {
            const string sql = @"exec spSIFDocsAsiento
                         @tipoDoc,@numdoc,@monto,'C',@codDivisa,
                         @tipocambio,@enlace,@codUnidad,@codCentroCosto,@cuenta,
                         @idSolicitud,@codigo_cta,@ase_doc_deposito";

            var param = new
            {
                tipoDoc = req.tipodoc,
                numdoc = vNumDoc,
                monto,
                codDivisa = ctas.cod_divisa,
                tipocambio,
                enlace = 0, // GLOBALES.gEnlace
                codUnidad = cargo.cod_unidad,
                codCentroCosto = cargo.cod_centro_costo,
                cuenta = cargo.cod_cuenta,
                idSolicitud = cargo.id_solicitud,
                codigo_cta = cargo.codigo,
                ase_doc_deposito = ""
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto RegistrarAsientos_polizas(
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
                @"exec spCrdDocumentoAfectacionPolizas @tipoDoc,@numDoc",
                new { tipoDoc = req.tipodoc, numDoc = vNumDoc }
            ).Result ?? [];

            foreach (var p in polizas)
            {
                var monto = p.mov_monto * factor;

                var r = RegistrarAsientoPoliza(codempresa, req, ctas, tipocambio, monto, p.cod_cuenta, vNumDoc);
                if (r?.Code.HasValue == true && r.Code != 0) return r;
            }

            return new ErrorDto { Code = 0, Description = string.Empty };
        }

        private ErrorDto RegistrarAsientoPoliza(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal tipocambio,
            decimal monto,
            string cuenta, 
            string vNumDoc)
        {
            const string sql = @"exec spSIFDocsAsiento
                         @tipoDoc,@numdoc,@monto,'C',@codDivisa,
                         @tipocambio,@enlace,@codUnidad,@codCentroCosto,@cuenta,
                         @idSolicitud,@codigo_cta,@ase_doc_deposito";

            var param = new
            {
                tipoDoc = req.tipodoc,
                numdoc = vNumDoc,
                monto,
                codDivisa = ctas.cod_divisa,
                tipocambio,
                enlace = 0, // GLOBALES.gEnlace
                codUnidad = ctas.cod_unidad,
                codCentroCosto = ctas.cod_centro_costo,
                cuenta,
                idSolicitud = ctas.id_solicitud,
                codigo_cta = ctas.codigo,
                ase_doc_deposito = ""
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codempresa, sql, param);
        }

        private ErrorDto RegistrarPagoFinalSiCorresponde(
            int codempresa,
            CajasCrdAbonosCtPRegistrarAbonoRequest req,
            CajasCrdOperacionCtasData ctas,
            decimal monto_total,
            string vNumDoc)
        {
            if (monto_total == 0)
                return new ErrorDto { Code = 0, Description = string.Empty };

            const string sql = @"exec spCajas_DesglocePagosDocFinal
                         @caja,@apertura,@tiquete,@Usuario,@tipoDoc,@numdoc,@unidad,@idSolicitud,@codigo_cta";

            var param = new
            {
                caja = req.mcaja,
                apertura = req.mapertura,
                tiquete = req.mtiquete,
                Usuario = req.usuario,
                tipoDoc = req.tipodoc,
                numdoc = vNumDoc,
                unidad = req.munidad,
                idSolicitud = ctas.id_solicitud,
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
