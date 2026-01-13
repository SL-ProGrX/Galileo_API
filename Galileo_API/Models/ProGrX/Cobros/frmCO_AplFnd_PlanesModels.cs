namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FondosAplConfigPrioridadResult
    {
        public string? Cod_Plan { get; set; }
        public string? Descripcion { get; set; }
        public int? Orden { get; set; }
        public int? Activo { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
    }

    public class FondosAplConfigFondoDisponibleResult
    {
        public string? Cod_Plan { get; set; }
        public string? Descripcion { get; set; }
    }

    public class FondosAplConfigPrioridadAddParams
    {
        public string? Codigo { get; set; }
        public int? Orden { get; set; }
        public int? Activo { get; set; }
        public string? Usuario { get; set; }
    }

    public class FondosAplConfigPrioridadAddResult
    {
        public string? IdLlave { get; set; }
        public int? Pass { get; set; }
        public string? Mensaje { get; set; }
        public string? Movimiento { get; set; }
    }

    public class FondosAplConfigPrioridadDelParams
    {
        public string? Codigo { get; set; }
        public string? Usuario { get; set; }
    }

    public class FondosAplConfigPrioridadDelResult
    {
        public string? IdLlave { get; set; }
        public int? Pass { get; set; }
        public string? Mensaje { get; set; }
        public string? Movimiento { get; set; }
    }
}
