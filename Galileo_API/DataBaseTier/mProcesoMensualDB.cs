using System.Data;
using Dapper;

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
        public static void SbBitacoraPlanilla(IDbConnection connection, string transaccion, int codInstitucion, decimal proceso, string gestion, string usuario, string documento = "")
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
            });
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
        public static void SbFndAsiento(IDbConnection connection, decimal proceso, int codInstitucion, int operadora, string plan, string cuenta, string usuario, string numeroDocumento = "")
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
                Proceso = proceso,
                CodInstitucion = codInstitucion,
                Operadora = operadora,
                Plan = plan?.Trim() ?? string.Empty,
                Cuenta = cuenta?.Trim() ?? string.Empty,
                NumeroDocumento = numeroDocumento ?? string.Empty,
                Usuario = usuario
            });
        }
    }
}
