using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCorreccionCreditosDb
    {
        /// <summary>
        /// Aplica una correccion sobre la operacion existente y registra la bitacora de credito.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Movimiento, nuevo valor, nota y usuario.</param>
        /// <returns>Resultado y documento generado cuando aplica.</returns>
        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Cambio_Aplicar(
            int codEmpresa,
            CrCorreccionCreditosAplicarRequest request)
        {
            var validacion = CR_CorreccionCreditos_Cambio_Validar(request);
            if (validacion is not null)
                return validacion;

            var globalesResponse = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, request.usuario);
            if (globalesResponse.Code != 0 || globalesResponse.Result is null)
                return CR_CorreccionCreditos_Aplicar_Error(globalesResponse.Description ?? "No fue posible obtener Globales.");

            var operacionResponse = CR_CorreccionCreditos_OperacionBase_Obtener(
                codEmpresa,
                request.operacion,
                globalesResponse.Result.GlngFechaCR);
            if (operacionResponse.Code != 0 || operacionResponse.Result is null)
                return CR_CorreccionCreditos_Aplicar_Error("La operación no se encontró activa.");

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                var contexto = new CrCorreccionCreditosCambioContext
                {
                    CodEmpresa = codEmpresa,
                    Conn = conn,
                    Tx = tx,
                    Request = request,
                    Operacion = operacionResponse.Result,
                    SysPlanPagos = globalesResponse.Result.SysPlanPagos,
                    FechaCredito = globalesResponse.Result.GlngFechaCR,
                    SysDocVersion = globalesResponse.Result.SysDocVersion,
                    Enlace = globalesResponse.Result.GEnlace,
                    OficinaTitular = globalesResponse.Result.GOficinaTitular
                };
                var resultado = CR_CorreccionCreditos_Cambio_Ejecutar(contexto);

                tx.Commit();
                CR_CorreccionCreditos_Bitacora_Registrar(codEmpresa, request, operacionResponse.Result);
                CR_CorreccionCreditos_Reporte_Adjuntar(codEmpresa, request.usuario, resultado);
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (DbException ex)
            {
                return CR_CorreccionCreditos_Aplicar_Error(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CR_CorreccionCreditos_Aplicar_Error(ex.Message);
            }
        }

        /// <summary>Distribuye la solicitud hacia el proceso específico del movimiento.</summary>
        /// <param name="contexto">Contexto transaccional y funcional del cambio.</param>
        /// <returns>Resultado del movimiento aplicado.</returns>
        private CrCorreccionCreditosResultado CR_CorreccionCreditos_Cambio_Ejecutar(
            CrCorreccionCreditosCambioContext contexto)
        {
            var request = contexto.Request;
            return request.movimiento switch
            {
                0 => CR_CorreccionCreditos_Plazo_Aplicar(contexto.Conn, contexto.Tx, request, contexto.Operacion, contexto.SysPlanPagos, contexto.FechaCredito),
                1 => CR_CorreccionCreditos_Tasa_Aplicar(contexto.Conn, contexto.Tx, request, contexto.Operacion, contexto.SysPlanPagos),
                2 => CR_CorreccionCreditos_Linea_Aplicar(contexto.Conn, contexto.Tx, request),
                3 => CR_CorreccionCreditos_Monto_Aplicar(contexto),
                4 => CR_CorreccionCreditos_Cuota_Aplicar(contexto.Conn, contexto.Tx, request, contexto.Operacion, contexto.SysPlanPagos),
                5 => CR_CorreccionCreditos_Mora_Eliminar(contexto.Conn, contexto.Tx, request, contexto.SysPlanPagos),
                6 => CR_CorreccionCreditos_UltimoAbono_Aplicar(contexto.Conn, contexto.Tx, request, contexto.SysPlanPagos),
                7 => CR_CorreccionCreditos_PrimeraDeduccion_Aplicar(contexto.Conn, contexto.Tx, request, contexto.SysPlanPagos),
                10 => CR_CorreccionCreditos_InteresesMoratorios_Eliminar(contexto.Conn, contexto.Tx, request, contexto.SysPlanPagos),
                11 or 12 or 13 or 14 or 18 or 19 => CR_CorreccionCreditos_Catalogo_Aplicar(contexto.Conn, contexto.Tx, request, contexto.SysPlanPagos),
                15 => CR_CorreccionCreditos_Cargos_Eliminar(contexto.Conn, contexto.Tx, request, contexto.SysPlanPagos),
                16 => CR_CorreccionCreditos_Oficina_Aplicar(contexto),
                _ => throw new InvalidOperationException("El movimiento seleccionado no se puede aplicar desde este formulario.")
            };
        }

        /// <summary>Aplica el cambio de plazo y recalcula la cuota.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos actuales.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <param name="fechaCredito">Proceso vigente.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Plazo_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            int sysPlanPagos,
            decimal fechaCredito)
        {
            var plazo = Convert.ToInt32(request.valor_numerico);
            if (plazo <= 0)
                throw new InvalidOperationException("El plazo debe ser mayor que cero.");

            var plazoCuota = plazo;
            int? primeraDeduccion = null;
            if (request.ajustar_primer_deduccion)
            {
                primeraDeduccion = conn.ExecuteScalar<int>(
                    "select convert(int, dbo.fxSIFPrmProcesoSig(@FechaCredito))",
                    new { FechaCredito = fechaCredito },
                    tx);
            }
            else
            {
                var transcurrido = operacion.plazo - operacion.plazo_restante;
                if (plazo <= transcurrido)
                    throw new InvalidOperationException(
                        "El plazo de cambio es igual o menor al tiempo transcurrido de la operación.");
                plazoCuota = plazo - transcurrido;
            }

            var cuota = MCobroDb.fxCalcula_Cuota(operacion.saldo, plazoCuota, operacion.interes);
            const string sql = @"
                update reg_creditos
                   set cuota_fija = 0,
                       plazo = @Plazo,
                       cuota = @Cuota,
                       prideduc = coalesce(@PrimeraDeduccion, prideduc)
                 where id_solicitud = @Operacion and estado = 'A';";
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(sql, new
            {
                Plazo = plazo,
                Cuota = cuota,
                PrimeraDeduccion = primeraDeduccion,
                Operacion = request.operacion
            }, tx));

            CR_CorreccionCreditos_PlanPagos_Regenerar(conn, tx, request.operacion, sysPlanPagos, false);
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Aplica el cambio de tasa y recalcula la cuota.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos actuales.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Tasa_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            int sysPlanPagos)
        {
            var tasa = request.tasa ?? -1;
            if (tasa < 0 || tasa > 99)
                throw new InvalidOperationException("La tasa debe estar entre 0 y 99.");

            var cuota = MCobroDb.fxCalcula_Cuota(operacion.saldo, operacion.plazo_restante, tasa);
            const string sql = @"
                update reg_creditos
                   set interesv = @Tasa,
                       cuota = @Cuota,
                       cuota_fija = 0,
                       TBP_PuntosAdd = @Puntos,
                       LiqTasa = @LiqTasa
                 where id_solicitud = @Operacion and estado = 'A';";
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(sql, new
            {
                Tasa = tasa,
                Cuota = cuota,
                Puntos = request.tasa_indizada_tbp ? request.tbp_puntos_add : null,
                LiqTasa = request.aplica_puntos_renuncia ? 1 : 0,
                Operacion = request.operacion
            }, tx));

            CR_CorreccionCreditos_PlanPagos_Regenerar(conn, tx, request.operacion, sysPlanPagos, true);
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Aplica el cambio de línea mediante el procedimiento oficial.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <returns>Resultado y documento generado.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Linea_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request)
        {
            var linea = request.valor.Trim().ToUpperInvariant();
            var existe = conn.ExecuteScalar<int>(
                "select count(1) from catalogo where codigo = @Linea",
                new { Linea = linea },
                tx);
            if (existe != 1)
                throw new InvalidOperationException("La línea indicada no existe.");

            var documento = conn.QueryFirstOrDefault<CrCorreccionCreditosDocumento>(
                "exec spCrd_Operacion_Cambio_Linea @Operacion, @Linea, @Notas, @Usuario",
                new
                {
                    Operacion = request.operacion,
                    Linea = linea,
                    Notas = CR_CorreccionCreditos_Texto_Limitar(request.notas, 500),
                    Usuario = request.usuario
                },
                tx);

            return new CrCorreccionCreditosResultado
            {
                mensaje = "Cambio realizado satisfactoriamente.",
                tipo_documento = documento?.TipoDoc ?? string.Empty,
                numero_documento = documento?.NumDoc ?? 0
            };
        }

        /// <summary>Aplica el cambio de cuota de la operación.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos actuales.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Cuota_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            int sysPlanPagos)
        {
            var cuota = request.valor_numerico ?? -1;
            if (cuota < 0)
                throw new InvalidOperationException("La cuota indicada no es válida.");

            var sql = operacion.retencion
                ? @"update reg_creditos set cuota = @Cuota, cuota_fija = @Cuota,
                         montoapr = @Cuota, saldo = @Cuota
                    where id_solicitud = @Operacion and estado = 'A'"
                : @"update reg_creditos set cuota = @Cuota, cuota_fija = @Cuota
                    where id_solicitud = @Operacion and estado = 'A'";
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(sql, new { Cuota = cuota, Operacion = request.operacion }, tx));
            CR_CorreccionCreditos_PlanPagos_Regenerar(conn, tx, request.operacion, sysPlanPagos, false);
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Actualiza el proceso del último abono.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_UltimoAbono_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            int sysPlanPagos)
        {
            var periodo = Convert.ToInt32(request.valor_numerico);
            CR_CorreccionCreditos_Periodo_Asegurar(periodo);
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(
                "update reg_creditos set fecult = @Periodo where id_solicitud = @Operacion and estado = 'A'",
                new { Periodo = periodo, Operacion = request.operacion },
                tx));

            if (sysPlanPagos == 1)
            {
                conn.Execute(
                    "exec spCrd_Operacion_Cambio_UltCta @Operacion, @Periodo, @Notas, @Usuario",
                    new
                    {
                        Operacion = request.operacion,
                        Periodo = periodo,
                        Notas = CR_CorreccionCreditos_Texto_Limitar(request.notas, 500),
                        Usuario = request.usuario
                    },
                    tx);
            }
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Actualiza el proceso de primera deducción.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_PrimeraDeduccion_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            int sysPlanPagos)
        {
            var periodo = Convert.ToInt32(request.valor_numerico);
            CR_CorreccionCreditos_Periodo_Asegurar(periodo);
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(
                "update reg_creditos set prideduc = @Periodo where id_solicitud = @Operacion and estado = 'A'",
                new { Periodo = periodo, Operacion = request.operacion },
                tx));
            CR_CorreccionCreditos_PlanPagos_Regenerar(conn, tx, request.operacion, sysPlanPagos, false);
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Elimina los intereses moratorios activos.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del proceso.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_InteresesMoratorios_Eliminar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            int sysPlanPagos)
        {
            var sql = sysPlanPagos == 1
                ? @"update T set intmor = 0, mora_dias = 0
                    from CRD_OPERACION_TRANSAC T
                    inner join reg_creditos R on T.id_solicitud = R.id_solicitud
                    where R.id_solicitud = @Operacion and R.estado = 'A' and R.proceso <> 'J' and T.estado = 'A';
                    update T set intmor = 0, mora_dias = 0
                    from CRD_OPERACION_PLAN_PAGOS T
                    inner join reg_creditos R on T.id_solicitud = R.id_solicitud
                    where R.id_solicitud = @Operacion and R.estado = 'A' and R.proceso <> 'J' and T.estado = 'A';"
                : @"update morosidad set intm = 0
                    where id_solicitud = @Operacion and estado = 'A' and estadoi <> 'J';";
            conn.Execute(sql, new { Operacion = request.operacion }, tx);
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Aplica un cambio respaldado por catálogo.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del cambio.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Catalogo_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            int sysPlanPagos)
        {
            var valor = request.valor.Trim();
            var sql = request.movimiento switch
            {
                11 => "update reg_creditos set garantia = @Valor where id_solicitud = @Operacion and estado = 'A'",
                12 => "update reg_creditos set cod_destino = @Valor where id_solicitud = @Operacion and estado = 'A'",
                13 => "update reg_creditos set cod_grupo = @Valor where id_solicitud = @Operacion and estado = 'A'",
                14 => "update reg_creditos set dia_pago = convert(int,@Valor) where id_solicitud = @Operacion and estado = 'A'",
                18 => "update reg_creditos set cod_actividad = @Valor where id_solicitud = @Operacion and estado = 'A'",
                19 => "update reg_creditos set id_promotor = convert(int,@Valor) where id_solicitud = @Operacion and estado = 'A'",
                _ => throw new InvalidOperationException("Movimiento de catálogo no válido.")
            };

            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(sql, new { Valor = valor, Operacion = request.operacion }, tx));
            if (request.movimiento == 14 && sysPlanPagos == 1)
            {
                conn.Execute(
                    "exec spCrdOperacionCambioDiaPago @Operacion, @DiaPago, @Usuario",
                    new { Operacion = request.operacion, DiaPago = Convert.ToInt32(valor), Usuario = request.usuario },
                    tx);
            }
            return CR_CorreccionCreditos_Resultado_Exito();
        }

        /// <summary>Revierte las cuotas en mora seleccionadas.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos y selección.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del proceso.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Mora_Eliminar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            int sysPlanPagos)
        {
            CR_CorreccionCreditos_Seleccion_Asegurar(request.seleccionados);
            var sql = sysPlanPagos == 1
                ? @"update CRD_OPERACION_TRANSAC set mora_dias = 0, intmor = 0
                    where id_solicitud = @Operacion and id_seq in @Ids;
                    update CRD_OPERACION_PLAN_PAGOS set mora_dias = 0, intmor = 0
                    where id_solicitud = @Operacion and id_seq in @Ids;"
                : @"update morosidad set estado = 'N'
                    where id_solicitud = @Operacion and estado = 'A' and id_moro in @Ids;";
            conn.Execute(sql, new { Operacion = request.operacion, Ids = request.seleccionados }, tx);
            return CR_CorreccionCreditos_Resultado_Exito("Reversiones realizadas satisfactoriamente.");
        }

        /// <summary>Elimina los cargos seleccionados.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos y selección.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <returns>Resultado del proceso.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Cargos_Eliminar(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosAplicarRequest request,
            int sysPlanPagos)
        {
            CR_CorreccionCreditos_Seleccion_Asegurar(request.seleccionados);
            if (sysPlanPagos == 1)
            {
                const string sql = @"
                    update T set T.cargos = T.cargos - C.monto
                    from CRD_OPERACION_TRANSAC T
                    inner join CRD_OPERACION_TRANSAC_CARGOS C
                      on T.id_solicitud = C.id_solicitud and T.id_seq = C.id_seq
                    where C.id_solicitud = @Operacion and C.linea in @Ids;
                    update P set P.cargos = P.cargos - C.monto
                    from CRD_OPERACION_PLAN_PAGOS P
                    inner join CRD_OPERACION_TRANSAC_CARGOS C
                      on P.id_solicitud = C.id_solicitud and P.id_seq = C.id_seq
                    where C.id_solicitud = @Operacion and C.linea in @Ids;
                    delete from CRD_OPERACION_TRANSAC_CARGOS
                    where id_solicitud = @Operacion and linea in @Ids;";
                conn.Execute(sql, new { Operacion = request.operacion, Ids = request.seleccionados }, tx);
            }
            else
            {
                const string sql = @"
                    update M set M.cargo = M.cargo - C.monto
                    from morosidad M
                    inner join morosidad_cargos C on M.id_moro = C.id_moro
                    where M.id_solicitud = @Operacion and C.id_cargo in @Ids;
                    delete C from morosidad_cargos C
                    inner join morosidad M on M.id_moro = C.id_moro
                    where M.id_solicitud = @Operacion and C.id_cargo in @Ids;";
                conn.Execute(sql, new { Operacion = request.operacion, Ids = request.seleccionados }, tx);
            }
            return CR_CorreccionCreditos_Resultado_Exito("Reversión realizada satisfactoriamente.");
        }

        /// <summary>Obtiene la base de datos requerida para aplicar o excluir una operación.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="operacion">Identificador de la operación.</param>
        /// <param name="fechaCredito">Proceso vigente.</param>
        /// <returns>Datos base de la operación.</returns>
        private ErrorDto<CrCorreccionCreditosOperacionBase?> CR_CorreccionCreditos_OperacionBase_Obtener(
            int codEmpresa,
            int operacion,
            decimal fechaCredito)
        {
            const string sql = @"
                select top 1 R.id_solicitud, rtrim(isnull(R.codigo,'')) as codigo,
                       rtrim(isnull(R.cedula,'')) as cedula,
                       rtrim(isnull(S.nombre,'')) as nombre,
                       rtrim(isnull(C.descripcion,'')) as descripcion,
                       case when isnull(R.opex,0)=1 then 'OPEX' else '' end as opex_descripcion,
                       rtrim(isnull(R.cod_oficina_r,'')) as cod_oficina_r,
                       rtrim(isnull(O.descripcion,'')) as oficina_descripcion,
                       isnull(R.montoapr,0) as montoapr,
                       isnull(R.amortiza,0) as amortiza,
                       isnull(R.saldo,0) as saldo, isnull(R.plazo,0) as plazo,
                       case when dbo.fxCrdPlazoRestante(R.plazo,R.prideduc,@FechaCredito) <= 0 then 1
                            else dbo.fxCrdPlazoRestante(R.plazo,R.prideduc,@FechaCredito) end as plazo_restante,
                       isnull(R.interesv,R.[int]) as interes,
                       convert(bit,case when C.retencion='S' or C.poliza='S' then 1 else 0 end) as retencion
                from reg_creditos R
                inner join catalogo C on R.codigo=C.codigo
                inner join socios S on R.cedula=S.cedula
                left join SIF_OFICINAS O on R.cod_oficina_r=O.cod_oficina
                where R.id_solicitud=@Operacion and R.estado='A';";
            return DbHelper.ExecuteSingleQuery<CrCorreccionCreditosOperacionBase>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion, FechaCredito = fechaCredito });
        }

        /// <summary>Registra la bitácora funcional del cambio aplicado.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del cambio.</param>
        /// <param name="operacion">Datos originales.</param>
        private void CR_CorreccionCreditos_Bitacora_Registrar(
            int codEmpresa,
            CrCorreccionCreditosAplicarRequest request,
            CrCorreccionCreditosOperacionBase operacion)
        {
            var codigoMovimiento = request.movimiento switch
            {
                0 => "01", 1 => "02", 2 => "03", 3 => "09", 4 => "10", 5 => "06",
                6 => "04", 7 => "05", 10 => "12", 11 => "16", 12 => "18", 13 => "19",
                14 => "20", 15 => "23", 16 => "21", 18 => "22", 19 => "24", _ => ""
            };
            if (string.IsNullOrEmpty(codigoMovimiento))
                return;

            MCredito.SbBitacoraCredito(_portalDb, codEmpresa, new MCredito.CrBitacoraCreditoRequest
            {
                usuario = request.usuario,
                movimiento = codigoMovimiento,
                detalle = $"Cambio por: {CR_CorreccionCreditos_Bitacora_Valor(request)}",
                tipo = operacion.retencion ? "R" : "C",
                operacion = request.operacion,
                codigo = operacion.codigo,
                notas = request.notas
            });
        }

        /// <summary>Normaliza y valida los datos comunes de un cambio.</summary>
        /// <param name="request">Solicitud recibida.</param>
        /// <returns>Error funcional o null.</returns>
        private static ErrorDto<CrCorreccionCreditosResultado>? CR_CorreccionCreditos_Cambio_Validar(
            CrCorreccionCreditosAplicarRequest request)
        {
            if (request is null || request.operacion <= 0)
                return CR_CorreccionCreditos_Aplicar_Error("Debe indicar una operación válida.");
            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.notas = CR_CorreccionCreditos_Texto_Limitar((request.notas ?? string.Empty).Trim(), 500);
            request.valor = (request.valor ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(request.usuario))
                return CR_CorreccionCreditos_Aplicar_Error("Debe indicar el usuario.");
            if (string.IsNullOrWhiteSpace(request.notas))
                return CR_CorreccionCreditos_Aplicar_Error("Especifique una nota al movimiento.");
            return null;
        }

        /// <summary>Regenera el plan de pagos cuando el esquema lo requiere.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="operacion">Identificador de operación.</param>
        /// <param name="sysPlanPagos">Indicador de planes.</param>
        /// <param name="cambioTasa">Indica recalculo por tasa.</param>
        private static void CR_CorreccionCreditos_PlanPagos_Regenerar(
            IDbConnection conn,
            IDbTransaction tx,
            int operacion,
            int sysPlanPagos,
            bool cambioTasa)
        {
            if (sysPlanPagos != 1)
                return;
            conn.Execute(
                cambioTasa ? "exec spCrdPlanPagos @Operacion, 1" : "exec spCrdPlanPagos @Operacion",
                new { Operacion = operacion },
                tx);
        }

        /// <summary>Garantiza que una actualización afectó la operación activa.</summary>
        /// <param name="filas">Cantidad de filas afectadas.</param>
        private static void CR_CorreccionCreditos_Actualizacion_Asegurar(int filas)
        {
            if (filas <= 0)
                throw new InvalidOperationException("La operación ya no se encuentra activa.");
        }

        /// <summary>Valida un proceso con formato AAAAMM.</summary>
        /// <param name="periodo">Proceso que se validará.</param>
        private static void CR_CorreccionCreditos_Periodo_Asegurar(int periodo)
        {
            var mes = periodo % 100;
            if (periodo < 190001 || mes is < 1 or > 12)
                throw new InvalidOperationException("El período debe tener formato AAAAMM.");
        }

        /// <summary>Valida una selección de registros de mora o cargos.</summary>
        /// <param name="ids">Identificadores seleccionados.</param>
        private static void CR_CorreccionCreditos_Seleccion_Asegurar(ICollection<int> ids)
        {
            if (ids is null || ids.Count == 0 || ids.Any(id => id <= 0))
                throw new InvalidOperationException("Seleccione al menos un registro válido.");
        }

        /// <summary>Resuelve el valor descriptivo que se guardará en bitácora.</summary>
        /// <param name="request">Datos del cambio.</param>
        /// <returns>Valor normalizado.</returns>
        private static string CR_CorreccionCreditos_Bitacora_Valor(CrCorreccionCreditosAplicarRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.valor))
                return request.valor;

            var valorNumerico = request.movimiento == 1
                ? request.tasa
                : request.valor_numerico;
            return Convert.ToString(valorNumerico) ?? string.Empty;
        }

        /// <summary>Limita un texto al tamaño permitido.</summary>
        /// <param name="valor">Texto original.</param>
        /// <param name="longitud">Longitud máxima.</param>
        /// <returns>Texto limitado.</returns>
        private static string CR_CorreccionCreditos_Texto_Limitar(string valor, int longitud)
            => valor[..Math.Min(valor.Length, longitud)];

        /// <summary>Crea un resultado exitoso sin documento.</summary>
        /// <param name="mensaje">Mensaje funcional.</param>
        /// <returns>Resultado homologado.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Resultado_Exito(
            string mensaje = "Cambio realizado satisfactoriamente.")
            => new() { mensaje = mensaje };

        /// <summary>Crea una respuesta funcional de error para aplicación.</summary>
        /// <param name="mensaje">Descripción del error.</param>
        /// <returns>Respuesta homologada.</returns>
        private static ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Aplicar_Error(string mensaje)
            => DbHelper.CreateErrorResponse<CrCorreccionCreditosResultado>(
                mensaje,
                -2,
                new CrCorreccionCreditosResultado());

        private sealed class CrCorreccionCreditosOperacionBase
        {
            public int id_solicitud { get; set; } = default;
            public string codigo { get; set; } = string.Empty;
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public string opex_descripcion { get; set; } = string.Empty;
            public string cod_oficina_r { get; set; } = string.Empty;
            public string oficina_descripcion { get; set; } = string.Empty;
            public decimal montoapr { get; set; } = default;
            public decimal amortiza { get; set; } = default;
            public decimal saldo { get; set; } = default;
            public int plazo { get; set; } = default;
            public int plazo_restante { get; set; } = default;
            public decimal interes { get; set; } = default;
            public bool retencion { get; set; } = default;
        }

        private sealed class CrCorreccionCreditosDocumento
        {
            public string TipoDoc { get; set; } = string.Empty;
            public int NumDoc { get; set; } = default;
        }

        private sealed class CrCorreccionCreditosCambioContext
        {
            public required int CodEmpresa { get; init; }
            public required IDbConnection Conn { get; init; }
            public required IDbTransaction Tx { get; init; }
            public required CrCorreccionCreditosAplicarRequest Request { get; init; }
            public required CrCorreccionCreditosOperacionBase Operacion { get; init; }
            public required int SysPlanPagos { get; init; }
            public required decimal FechaCredito { get; init; }
            public required int SysDocVersion { get; init; }
            public required int Enlace { get; init; }
            public required string OficinaTitular { get; init; }
        }
    }
}
