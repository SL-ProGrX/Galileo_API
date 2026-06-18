using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Globalization;
using static Galileo_API.Models.MProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualAplicacionCreditosDb
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly CcProcesoMensualGeneralDb _mGeneral;

        public CcProcesoMensualAplicacionCreditosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);

        }
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrAbonos_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, DateTime fechaSistema, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "08", "PRE", usuario, codInstitucion, fechaProceso);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {

                var codDocumento = ObtenerCodigoDocumento(connection, transaction, codInstitucion);

                var context = new CreditoAplicacionContext
                {
                    Connection = connection,
                    Transaction = transaction,
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso,
                    Usuario = usuario,
                    FechaSistema = fechaSistema,
                    Documento = $"{fechaProceso}.{codDocumento}.CRD"
                };

                EliminarCreditosConAbonoMenorUno(context);
                AplicarAbonosMasivo(context);
                AplicarAbonosPorLote(context);
                GenerarMoraCreditosSinAbono(context);
                RevisarDeduccionesPorcentaje(context);

                GenerarAsientoCredito(context, paso: 1);
                GenerarAsientoCredito(context, paso: 2);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "08",
                                                        CodInstitucion = codInstitucion,
                                                        Proceso = fechaProceso,
                                                        Gestion = "R",
                                                        Usuario = usuario,
                                                        Documento = $"{fechaProceso}.{codDocumento}.CRD"
                                                    });

                ProcesarSobrantes(context, paso: 1);
                ProcesarSobrantes(context, paso: 2);

                TrasladarRetencionesAFondos(context);
                RevisarDeduccionesFondos(context);
                MarcarCreditoAplicado(context);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM-CREDITO Aplica Abonos Inst: {codInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });





                transaction.Commit();

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "08", "POS", usuario, codInstitucion, fechaProceso);

                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualCreditosAplicacionResponse
                    {
                        Procesado = true,
                        Mensaje = "Información Aplicada ..."
                    });
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse<CcProcesoMensualCreditosAplicacionResponse>(
                    ex.Message,
                    -1,
                    new CcProcesoMensualCreditosAplicacionResponse());
            }



        }
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrdReporteInconsistencia_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "09", "PRE", usuario, codInstitucion, fechaProceso);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM-CREDITO Reporte Inconsistencias Inst: {codInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });
                ActualizarEstadoCreditosDevoluciones(connection, codInstitucion);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "09",
                                                        CodInstitucion = codInstitucion,
                                                        Proceso = fechaProceso,
                                                        Gestion = "R",
                                                        Usuario = usuario
                                                    });

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "09", "POS", usuario, codInstitucion, fechaProceso);

                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualCreditosAplicacionResponse
                    {
                        Procesado = true,
                        Mensaje = "Reporte de Inconsistencias generado correctamente."
                    });

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualCreditosAplicacionResponse>(
                 ex.Message,
                 -1,
                 new CcProcesoMensualCreditosAplicacionResponse());

            }
        }
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrdCalculoInteresesMoratorios_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "10", "PRE", usuario, codInstitucion, fechaProceso);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM-CREDITO Recalcula Mora Inst: {codInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });
                ActualizarEstadoCreditosInteresesMoratorios(connection, codInstitucion);
                sbCrRecalculaCuotaEnMora(connection, codInstitucion, fechaProceso);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "10",
                                                        CodInstitucion = codInstitucion,
                                                        Proceso = fechaProceso,
                                                        Gestion = "R",
                                                        Usuario = usuario
                                                    });

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "10", "POS", usuario, codInstitucion, fechaProceso);

                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualCreditosAplicacionResponse
                    {
                        Procesado = true,
                        Mensaje = "Actualización de Intereses Moratorios Realizado ..."
                    });

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualCreditosAplicacionResponse>(
                 ex.Message,
                 -1,
                 new CcProcesoMensualCreditosAplicacionResponse());

            }
        }
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrdRecalculoSaldoMes_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "11", "PRE", usuario, codInstitucion, fechaProceso);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"PRM-CREDITO Recalcula Mora Inst: {codInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });

                Helpers.CcProcesoMensualCreditosHelperDb.SbCrCalculaSaldoMes(connection, codInstitucion, 0);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "11",
                                                        CodInstitucion = codInstitucion,
                                                        Proceso = fechaProceso,
                                                        Gestion = "R",
                                                        Usuario = usuario
                                                    });

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, codEmpresa, "11", "POS", usuario, codInstitucion, fechaProceso);

                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualCreditosAplicacionResponse
                    {
                        Procesado = true,
                        Mensaje = "Recalculo del saldo del mes Realizado ..."
                    });

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualCreditosAplicacionResponse>(
                 ex.Message,
                 -1,
                 new CcProcesoMensualCreditosAplicacionResponse());

            }
        }
        private static void ActualizarEstadoCreditosDevoluciones(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_crInco = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }
        private static void ActualizarEstadoCreditosInteresesMoratorios(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_crMora = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }
        private static void sbCrRecalculaCuotaEnMora(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            connection.Execute(
                "spPrmCrdMoraIntCalcula",
                new
                {
                    codInstitucion,
                    fechaProceso
                },
                commandType: CommandType.StoredProcedure);
        }
        private string ObtenerCodigoDocumento(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
              select isnull(desc_Corta, convert(varchar(10), cod_institucion)) as CodDoc
                from instituciones
                where cod_institucion = @CodInstitucion";

            var codDocumento = connection.QuerySingleOrDefault<string>(
                query,
                new { CodInstitucion = codInstitucion },
                transaction);

            return codDocumento?.Trim() ?? codInstitucion.ToString(CultureInfo.InvariantCulture);
        }
        private static void EliminarCreditosConAbonoMenorUno(CreditoAplicacionContext context)
        {
            const string sql = @"delete prm_creditos where cod_institucion = @CodInstitucion  and fecha_proceso = @FechaProceso  and abono < 1  ";

            context.Connection.Execute(
                sql,
                new
                {
                    context.CodInstitucion,
                    context.FechaProceso
                },
                context.Transaction);
        }
        private static void AplicarAbonosMasivo(CreditoAplicacionContext context)
        {
            const int ultimoPaso = 5;

            var paso = 1;

            while (paso <= ultimoPaso)
            {
                var resultado = context.Connection.QuerySingle<CreditoAplicaAbonosMasivoResult>(
                    "spPrmCreditoAplicaAbonosMasivo",
                    new
                    {
                        context.CodInstitucion,
                        context.FechaProceso,
                        context.Documento,
                        Paso = paso
                    },
                    context.Transaction,
                    commandTimeout: 5200,
                    commandType: CommandType.StoredProcedure);

                paso = resultado.PasoSiguiente;
            }
        }
        private static void AplicarAbonosPorLote(CreditoAplicacionContext context)
        {
            const int cantidadPorLote = 150;

            var registrosPendientes = ObtenerTotalCreditosPendientesAplicacion(
                context.Connection,
                context.Transaction,
                context.CodInstitucion,
                context.FechaProceso);

            while (registrosPendientes > 0)
            {
                registrosPendientes = context.Connection.QuerySingle<long>(
                    "spPrmCreditoAplicaAbonos",
                    new
                    {
                        context.CodInstitucion,
                        context.FechaProceso,
                        context.Documento,
                        Cantidad = cantidadPorLote
                    },
                    context.Transaction,
                    commandTimeout: 5200,
                    commandType: CommandType.StoredProcedure);
            }
        }
        private static long ObtenerTotalCreditosPendientesAplicacion(IDbConnection connection, IDbTransaction transaction, int codInstitucion, decimal fechaProceso)
        {
            const string sql = @"
                        select count(*) + 1 as Total
                        from prm_creditos
                        where fecha_proceso = @FechaProceso
                          and id_aplicacion = 1
                          and ind_paso = 0
                          and cod_institucion = @CodInstitucion
                        ";

            return connection.QuerySingle<long>(
                sql,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso
                },
                transaction);
        }
        private static void GenerarMoraCreditosSinAbono(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrmCreditoMoraGenera",
                new
                {
                    context.CodInstitucion,
                    context.FechaProceso
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }
        private static void RevisarDeduccionesPorcentaje(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrm_Deducciones_Porc_Revision",
                new
                {
                    context.CodInstitucion,
                    context.FechaProceso,
                    context.Usuario
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }
        private static void GenerarAsientoCredito(CreditoAplicacionContext context, int paso)
        {
            context.Connection.Execute(
                "spPrmCreditoAsiento",
                new
                {
                    Tipo = "1",
                    context.Documento,
                    Fecha = context.FechaSistema.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                    context.Usuario,
                    context.CodInstitucion,
                    context.FechaProceso,
                    paso
                },
               context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }
        private static void ProcesarSobrantes(CreditoAplicacionContext context, int paso)
        {
            context.Connection.Execute(
                "spPrm_Sobrantes_Main",
                new
                {
                    context.FechaProceso,
                    context.CodInstitucion,
                    context.Documento,
                    context.Usuario,
                    paso
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }
        private static void TrasladarRetencionesAFondos(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrmFndTrasladoRetAFondo",
                new
                {
                    context.FechaProceso,
                    context.CodInstitucion,
                    context.Documento,
                    context.Usuario
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }
        private static void RevisarDeduccionesFondos(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrm_Deducciones_Fondos_Revision",
                new
                {
                    context.CodInstitucion,
                    context.FechaProceso,
                    context.Usuario
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }
        private static void MarcarCreditoAplicado(CreditoAplicacionContext context)
        {
            const string sql = @"
                    update instituciones
                    set pr_crAplica = 1
                    where cod_institucion = @CodInstitucion
                    ";

            context.Connection.Execute(
                sql,
                new { CodInstitucion = context.CodInstitucion },
                context.Transaction);
        }

        private sealed class CreditoAplicaAbonosMasivoResult
        {
            public int PasoSiguiente { get; set; }
            public long Pendientes { get; set; }
        }
        private sealed class CreditoAplicacionContext
        {
            public required IDbConnection Connection { get; init; }
            public required IDbTransaction Transaction { get; init; }
            public required int CodInstitucion { get; init; }
            public required decimal FechaProceso { get; init; }
            public required string Usuario { get; init; }
            public required DateTime FechaSistema { get; init; }
            public required string Documento { get; init; }
        }
    }
}
