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
        private readonly string movimientoBitacora = "Aplica - WEB";

        /// <summary>
        /// Inicializa una nueva instancia para gestionar la aplicación de créditos del proceso mensual.
        /// </summary>
        /// <param name="config">Configuración general de la aplicación.</param>
        public CcProcesoMensualAplicacionCreditosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);

        }

        /// <summary>
        /// Aplica abonos de créditos para la institución y proceso especificados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="fechaSistema">Fecha del sistema para los asientos.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la aplicación de abonos.</returns>
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
                                                    }, transaction);

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
                    Movimiento = movimientoBitacora,
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

        /// <summary>
        /// Marca y registra la generación del reporte de inconsistencias de créditos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la operación.</returns>
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
                    Movimiento = movimientoBitacora,
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

        /// <summary>
        /// Ejecuta el cálculo de intereses moratorios y actualiza su estado de proceso.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la operación.</returns>
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
                    Movimiento = movimientoBitacora,
                    Modulo = vModulo
                });
                ActualizarEstadoCreditosInteresesMoratorios(connection, codInstitucion);
                SbCrRecalculaCuotaEnMora(connection, codInstitucion, fechaProceso);

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

        /// <summary>
        /// Recalcula el saldo mensual de créditos y registra su bitácora de ejecución.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado de la operación.</returns>
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
                    Movimiento = movimientoBitacora,
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

        /// <summary>
        /// Actualiza el indicador de reporte de inconsistencias de créditos en la institución.
        /// </summary>
        /// <param name="connection">Conexión activa de base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void ActualizarEstadoCreditosDevoluciones(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_crInco = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }

        /// <summary>
        /// Actualiza el indicador de cálculo de mora de créditos en la institución.
        /// </summary>
        /// <param name="connection">Conexión activa de base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void ActualizarEstadoCreditosInteresesMoratorios(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_crMora = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(query, new { CodInstitucion = codInstitucion });
        }

        /// <summary>
        /// Ejecuta el procedimiento de recalculo de cuota en mora para créditos.
        /// </summary>
        /// <param name="connection">Conexión activa de base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        private static void SbCrRecalculaCuotaEnMora(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            connection.Execute(
                "spPrmCrdMoraIntCalcula",
                new
                {
                    Institucion=codInstitucion,
                    Proceso=fechaProceso
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Obtiene el código corto de documento para la institución.
        /// </summary>
        /// <param name="connection">Conexión activa de base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Código de documento para bitácora y asientos.</returns>
        private static string ObtenerCodigoDocumento(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
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

        /// <summary>
        /// Elimina créditos con abonos menores a uno para el proceso actual.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
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

        /// <summary>
        /// Ejecuta la aplicación masiva de abonos por pasos hasta completar el proceso.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        private static void AplicarAbonosMasivo(CreditoAplicacionContext context)
        {
            const int ultimoPaso = 5;

            var paso = 1;
        

            while (paso <= ultimoPaso)
            {
                var resultado = context.Connection.QuerySingleOrDefault<CreditoAplicaAbonosMasivoResult>(
                     "spPrmCreditoAplicaAbonosMasivo",
                     new
                     {
                         Institucion = context.CodInstitucion,
                         Proceso = context.FechaProceso,
                         context.Documento,
                         Paso = paso
                     },
                     context.Transaction,
                     commandTimeout: 5200,
                     commandType: CommandType.StoredProcedure);

                if (resultado is null)
                {
                    paso++;
                    continue;
                }
                 
                paso = resultado.PasoSiguiente;
            }
        }

        /// <summary>
        /// Aplica abonos en lotes mientras existan créditos pendientes por procesar.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
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
                        Institucion= context.CodInstitucion,
                        Proceso=context.FechaProceso,
                        context.Documento,
                        Top = cantidadPorLote
                    },
                    context.Transaction,
                    commandTimeout: 5200,
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Obtiene la cantidad de créditos pendientes de aplicación.
        /// </summary>
        /// <param name="connection">Conexión activa de base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha de proceso.</param>
        /// <returns>Total de créditos pendientes.</returns>
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

        /// <summary>
        /// Genera mora para créditos sin abono aplicado en el proceso.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        private static void GenerarMoraCreditosSinAbono(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrmCreditoMoraGenera",
                new
                {
                    Institucion= context.CodInstitucion,
                    Proceso=context.FechaProceso
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Ejecuta revisión de deducciones por porcentaje.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        private static void RevisarDeduccionesPorcentaje(CreditoAplicacionContext context)
        {
           context.Connection.Execute(
                "spPrm_Deducciones_Porc_Revision",
                new
                {
                    Institucion=context.CodInstitucion,
                    Proceso=context.FechaProceso,
                    context.Usuario
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Genera el asiento contable de créditos según el paso indicado.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        /// <param name="paso">Paso del procedimiento de asiento.</param>
        private static void GenerarAsientoCredito(CreditoAplicacionContext context, int paso)
        {
            context.Connection.Execute(
                "spPrmCreditoAsiento",
                new
                {
                    Tipo = "1",
                    context.Documento,
                    Fecha = context.FechaSistema,
                    context.Usuario,
                    Institucion= context.CodInstitucion,
                    Proceso=context.FechaProceso,
                    Paso= paso
                },
               context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Procesa sobrantes de créditos según el paso de ejecución.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        /// <param name="paso">Paso del procedimiento de sobrantes.</param>
        private static void ProcesarSobrantes(CreditoAplicacionContext context, int paso)
        {
            context.Connection.Execute(
                "spPrm_Sobrantes_Main",
                new
                {
                    Proceso= context.FechaProceso,
                    Institucion=context.CodInstitucion,
                    Comprobante=context.Documento,
                    context.Usuario,
                    Paso=paso
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Traslada retenciones de créditos hacia fondos.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        private static void TrasladarRetencionesAFondos(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrmFndTrasladoRetAFondo",
                new
                {
                    Proceso=context.FechaProceso,
                    Institucion=context.CodInstitucion,
                    Comprobante= context.Documento,
                    context.Usuario
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Ejecuta la revisión de deducciones asociadas a fondos.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        private static void RevisarDeduccionesFondos(CreditoAplicacionContext context)
        {
            context.Connection.Execute(
                "spPrm_Deducciones_Fondos_Revision",
                new
                {
                    Institucion= context.CodInstitucion,
                    Proceso= context.FechaProceso,
                    context.Usuario
                },
                context.Transaction,
                commandTimeout: 5200,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Marca la institución como crédito aplicado en el proceso mensual.
        /// </summary>
        /// <param name="context">Contexto de aplicación de créditos.</param>
        private static void MarcarCreditoAplicado(CreditoAplicacionContext context)
        {
            const string sql = @"
                    update instituciones
                    set pr_crAplica = 1
                    where cod_institucion = @CodInstitucion
                    ";

            context.Connection.Execute(
                sql,
                new { context.CodInstitucion },
                context.Transaction);
        }

        private sealed class CreditoAplicaAbonosMasivoResult
        {
            public int PasoSiguiente { get; set; } = 0;
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
