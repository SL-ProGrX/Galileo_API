namespace Galileo.Models.ProGrX.Bancos
{
    public class DropDownListaBancosGA
    {
        public string id_banco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DropDownListaTiposGA
    {
        public string itmy { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TesTransaccionesFiltros
    {
        public string cod_banco { get; set; } = string.Empty;
        public string tipo_doc { get; set; } = string.Empty;
        public string tipo_mov { get; set; } = string.Empty;
        public DateTime fecha_desde { get; set; }
        public DateTime fecha_hasta { get; set; }
        public bool chk_todasCuentas { get; set; }
        public bool chk_todosDocumentos { get; set; }
    }
    
    public class TesTrasladoTransaccionDto
    {
        public string nsolicitud { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime fecha_emision { get; set; }
        public string beneficiario { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string bancodesc { get; set; } = string.Empty;
        public decimal monto_total { get; set; }
    }
}