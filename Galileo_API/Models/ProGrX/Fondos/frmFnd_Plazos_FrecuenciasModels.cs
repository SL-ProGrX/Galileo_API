namespace Galileo.Models.ProGrX.Fondos
{
    public class FndPlazoVencimientoModel
    {
        public int IdPlazo { get; set; }
        public string Plazo { get; set; } = string.Empty;
        public int PlazoMeses { get; set; }
        public int PlazoDias { get; set; }
        public bool Estado { get; set; }
    }

    public class FndPlazoVencimientoSaveResult
    {
        public int Id { get; set; }
    }

    public class FndFrecuenciaCuponModel
    {
        public int IdFrecuenciaCupon { get; set; }
        public string Cupon { get; set; } = string.Empty;
        public int FrecuenciaMeses { get; set; }
        public int FrecuenciaDias { get; set; }
        public bool Estado { get; set; }
    }

    public class FndFrecuenciaCuponSaveResult
    {
        public int Id { get; set; }
    }
}
