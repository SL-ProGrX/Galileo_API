namespace Galileo.Models.ProGrX.Fondos
{
    public class FndSubCuentasData
    {
        public int idx { get; set; }
        public int cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public int cod_contrato { get; set; }
        public int? cod_beneficiario { get; set; }
        public string? estado { get; set; }
        public decimal? cuota { get; set; }
        public decimal? aportes { get; set; }
        public decimal? rendimiento { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public DateTime fechanac { get; set; }
        public string? telefono1 { get; set; }
        public string? telefono2 { get; set; }
        public string? email { get; set; }
        public string? direccion { get; set; }
        public string? apto_postal { get; set; }
        public string? notas { get; set; }
        public string? parentesco { get; set; }
        public string? parentesco_desc { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public bool isNew { get; set; }
    }
}