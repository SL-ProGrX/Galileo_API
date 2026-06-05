using System.Data;
using Dapper;
using static Galileo_API.Models.MProcesoMensualModels;

namespace Galileo_API.DataBaseTier
{
    public static class MProcesoMensualDb
    {
        public static string FxPlanillaTipoTransac(string? pTransaccion)
        {
            string transaccion = (pTransaccion ?? string.Empty).Trim();

            return transaccion switch
            {
                "01" => "Cambia Fecha de Proceso",
                "02" => "Genera deducciones",
                "02.1" => "Construye Archivo de Deducciones",
                "02.2" => "Deducciones Modificadas Manualmente",
                "03" => "Carga deducciones",
                "04" => "Desglosa deducciones",
                "05" => "Aplica Ahorros",
                "06" => "Inconsistencias de Ahorros",
                "07" => "Devoluciones de Ahorros",
                "08" => "Aplica Abonos",
                "08.2" => "Aplica Abonos x Inconsistencia",
                "08.3" => "Crea Fondos x Clientes Activos",
                "08.4" => "Crea Fondos x Clientes Inactivos",
                "09" => "Reporte de Inconsistencias",
                "10" => "Actualiza Intereses Moratorios",
                "11" => "Actualiza Saldo del Mes",
                _ => "No.Identificado"
            };
        }

        /// <summary>
        /// Método para registrar una transacción en la bitácora del proceso mensual.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaccion"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="proceso"></param>
        /// <param name="gestion"></param>
        /// <param name="usuario"></param>
        /// <param name="documento"></param>
        public static void SbBitacoraPlanilla(IDbConnection connection, string transaccion, int codInstitucion, decimal proceso, string gestion, string usuario, string documento = "",IDbTransaction? transaccionDb = null)
        {
            const string query = @"
                EXEC spPrm_Bitacora
                    @CodInstitucion,
                    @Proceso,
                    @Usuario,
                    @Transaccion,
                    @Gestion,
                    @Documento";

            connection.Execute(query, new
            {
                CodInstitucion = codInstitucion,
                Proceso = proceso,
                Usuario = usuario,
                Transaccion = transaccion,
                Gestion = gestion,
                Documento = documento ?? string.Empty
            }, 
            transaction: transaccionDb
            );
        }

        /// <summary>
        /// Método para registrar una transacción relacionada con la generación de asientos contables en la bitácora del proceso mensual.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="proceso"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="cuenta"></param>
        /// <param name="usuario"></param>
        /// <param name="numeroDocumento"></param>
        public static void SbFndAsiento(IDbConnection connection, ProcesoMensualFndAsientoRequest request)
        {
            const string query = @"
                EXEC spPrmFndAsiento
                    @Proceso,
                    @CodInstitucion,
                    @Operadora,
                    @Plan,
                    @Cuenta,
                    @NumeroDocumento,
                    @Usuario";

            connection.Execute(query, new
            {
                request.Proceso,
                request.CodInstitucion,
                request.Operadora,
                Plan = request.Plan.Trim(),
                Cuenta = request.Cuenta.Trim(),
                NumeroDocumento = request.NumeroDocumento ?? string.Empty,
                request.Usuario
            });
        }

        /// <summary>
        /// Metodo para ejecutar el proceso de envío a tránsito con planilla, que incluye la creación del espejo, la aplicación de masivo y abonos restantes en falso, y la generación de resultados para cuotas ordinarias y morosas.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="fechaProceso"></param>
        public static void SbCrEnviaConPlanillaTransito(IDbConnection connection, int codInstitucion, decimal glngFechaCR, decimal fechaProceso)
        {

            CrearEspejoPlanillaTransito(connection, codInstitucion, fechaProceso);
            AplicarMasivoEnFalso(connection, codInstitucion, fechaProceso);
            AplicarAbonosRestantesEnFalso(connection, codInstitucion, glngFechaCR);
            GenerarResultadosCuotasOrdinarias(connection, codInstitucion, fechaProceso);
            GenerarResultadosCuotasMorosas(connection, codInstitucion, fechaProceso);
        }
        private static void CrearEspejoPlanillaTransito(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @" EXEC spPrmCreditoEnviaTransitoEspejo @CodInstitucion, @FechaProceso";

            connection.Execute(query, new
            {
                CodInstitucion = codInstitucion,
                FechaProceso = fechaProceso
            });
        }
        private static void AplicarMasivoEnFalso(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @" EXEC spPrmCreditoEnviaTransitoAplicaMasivo @CodInstitucion, @FechaProceso";

            connection.Execute(query, new
            {
                CodInstitucion = codInstitucion,
                FechaProceso = fechaProceso
            });
        }
        private static void AplicarAbonosRestantesEnFalso(IDbConnection connection, int codInstitucion, decimal glngFechaCR)
        {
            var estado = EjecutarBloqueRestante(connection, codInstitucion, glngFechaCR);

            while (estado.Pendientes > 0)
            {
                estado = EjecutarBloqueRestante(connection, codInstitucion, glngFechaCR);
            }
        }
        private static SbCrEnviaConPlanillaTransitoModel EjecutarBloqueRestante(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrmCreditoEnviaTransitoAplicaRestante @CodInstitucion,  @FechaProceso,  @Bloque";

            return connection.QueryFirstOrDefault<SbCrEnviaConPlanillaTransitoModel>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso,
                    Bloque = 300
                }) ?? new SbCrEnviaConPlanillaTransitoModel();
        }
        private static void GenerarResultadosCuotasOrdinarias(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrmCreditoEnviaTransitoCuotaOrdinaria
                    @FechaProceso,
                    @CodInstitucion";

            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion
            });
        }
        private static void GenerarResultadosCuotasMorosas(IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"
                EXEC spPrmCreditoEnviaTransitoCuotaMorosa
                    @FechaProceso,
                    @CodInstitucion";

            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion
            });
        }

    }
}
