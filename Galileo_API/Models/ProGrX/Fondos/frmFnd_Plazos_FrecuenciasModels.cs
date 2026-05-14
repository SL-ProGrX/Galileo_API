namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPlazoVencimientoModel
    {
        public required int IdPlazo { get; set; }
        public string Plazo { get; set; } = string.Empty;
        public required int PlazoMeses { get; set; }
        public required int PlazoDias { get; set; }
        public required bool Estado { get; set; }
    }

    public class FndPlazoVencimientoSaveResult
    {
        public int Id { get; set; }
    }

    public class FndFrecuenciaCuponModel
    {
        public required int IdFrecuenciaCupon { get; set; }
        public string Cupon { get; set; } = string.Empty;
        public required int FrecuenciaMeses { get; set; }
        public required int FrecuenciaDias { get; set; }
        public required bool Estado { get; set; }
    }

    public class FndFrecuenciaCuponSaveResult
    {
        public int Id { get; set; }
    }
}
