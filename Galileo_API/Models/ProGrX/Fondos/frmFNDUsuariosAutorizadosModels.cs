namespace Galileo.Models.ProGrX.Fondos
{
    public class FndColaboradoresCcData
    {
        public string Usuario { get; set; } = string.Empty;
        public required bool Activo { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Actualiza_Fecha { get; set; }
        public string Actualiza_Usuario { get; set; } = string.Empty;
        public required bool isNew { get; set; } // Para saber si es inserción o actualización
    }

    public class FndColaboradoresCcValidaResult
    {
        public int Existe { get; set; }
    }
}