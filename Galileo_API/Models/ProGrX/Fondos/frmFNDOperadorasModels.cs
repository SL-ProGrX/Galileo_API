using System.Text.Json.Serialization;

namespace PgxAPI.Models.ProGrX.Fondos
{

    public class FndOperadoraDTO
    {
        public int? cod_operadora { get; set; }
        public string? descripcion { get; set; }
        public bool? activa { get; set; }
        public string? notas { get; set; }
        public string? ctaplan { get; set; }
        public string? ctaplandesc { get; set; }
        public string? ctaing { get; set; }
        public string? ctaingdesc { get; set; }
        public string? ctaret { get; set; }
        public string? ctaretdesc { get; set; }
        public decimal? multa_mnt_tope { get; set; }

    }


    public class OperadoraPlanDTO
    {
        public string cod_plan { get; set; }
        public string plan_desc { get; set; }
        public string cod_divisa { get; set; }
        public int contratos { get; set; }
        public decimal totallocal { get; set; }
        public decimal totaldivisa { get; set; }
    }


}
