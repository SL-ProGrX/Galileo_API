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
    }
}