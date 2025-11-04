namespace PgxAPI.Models.ProGrX.Fondos
{
    public class FndBeneficiariosContratosData
    {
        public int consec { get; set; }
        public int cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public int cod_contrato { get; set; }
        public string? cedula { get; set; }
        public string? cedulabn { get; set; }
        public string? nombre { get; set; }
        public DateTime fechanac { get; set; }
        public string? parentesco { get; set; }
        public decimal porcentaje { get; set; }
        public string? telefono1 { get; set; }
        public string? telefono2 { get; set; }
        public string? apto_postal { get; set; }
        public string? email { get; set; }
        public string? notas { get; set; }
        public string? direccion { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha { get; set; }
        public string? parentesco_desc { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public bool isNew { get; set; }
    }
}