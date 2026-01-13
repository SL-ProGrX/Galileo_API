namespace Galileo_API.Models.ProGrX.Cobros
{
    public class PromesasPagoConsultaParams
    {
        public int? CodEmpresa { get; set; }
        public string? Usuario { get; set; }
        public DateTime? FInicio { get; set; }
        public DateTime? FCorte { get; set; }
        public string? Filtro { get; set; }
    }

    public class PromesasPagoConsultaResult
    {
        public string? Id { get; set; }
        public string? Id_Solicitud { get; set; }
        public string? Usuario { get; set; }
        public DateTime? Fecha_Promesa { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Notas { get; set; }
        public string? Cod_Gestion { get; set; }
        public string? Gestion_Desc { get; set; }
        public string? Cod_Causa { get; set; }
        public string? Causa_Desc { get; set; }
        public string? Cod_Arreglo { get; set; }
    }
}
