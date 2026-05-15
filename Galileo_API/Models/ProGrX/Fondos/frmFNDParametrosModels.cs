namespace Galileo.Models.ProGrX.Fondos
{
    public class FndParametrosDto
    {
        public required string cod_parametro { get; set; }        // VARCHAR(2) o similar
        public string? descripcion { get; set; }          // VARCHAR(255)
        public required string valor { get; set; }                // Puede ser string o decimal según uso (mantengo string por flexibilidad)
        public required string tipo { get; set; }                 // CHAR(3)
        public string? visible { get; set; } = string.Empty;              // CHAR(1)
        public string? notas { get; set; } = string.Empty;               // TEXT o VARCHAR largo
        public DateTime? inicio_fecha { get; set; } = null;       // DATETIME
        public DateTime? modifica_fecha { get; set; } = null;   // DATETIME NULLABLE
        public string? modifica_usuario { get; set; } = string.Empty;     // VARCHAR(50)
        public string? valorCuenta { get; set; } = string.Empty;
        public string? cuentaDesc { get; set; } = string.Empty;

    }
}