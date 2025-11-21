namespace Galileo.Models.ProGrX.Bancos
{
    public class TesAnulacionDocData
    {
        public int nsolicitud { get; set; }
        public string? tipo { get; set; }
        public string? estado { get; set; }
        public string? ndocumento { get; set; }
        public string? id_banco { get; set; }
        public string? bancoX { get; set; }
        public string? tipoDocX { get; set; }
        public string? detalle_Anulacion { get; set; }
        public string? estado_Asiento { get; set; }
        public DateTime? fecha_emision { get; set; }
        public bool? verifica { get; set; }
    }

    public class TesAnulacionAnulaModel
    {
        public int nsolicitud { get; set; }
        public string? notas { get; set; }
        public string? usuario { get; set; }
        public bool? copia { get; set; }
        public int? cod_concepto_anulacion { get; set; } = 0;
    }
}