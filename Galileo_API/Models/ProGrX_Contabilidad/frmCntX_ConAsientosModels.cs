namespace Galileo_API.Models.ProGrX_Contabilidad
{

    public class CntxConAsientoDetalleDto
    {
        public string? cod_cuenta { get; set; }
        public string? descripcion { get; set; }
        public string? detalle { get; set; }
        public decimal debitos { get; set; }
        public decimal creditos { get; set; }
        public int linea { get; set; }


    }

    public class CntxConAsientoGuardarDto
    {
        public int cod_empresa { get; set; }

        public int cod_contabilidad { get; set; }

        public int cod_consolida { get; set; }

        public string cod_asiento { get; set; } = string.Empty;

        public DateTime fecha { get; set; }

        public string descripcion { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;

        public bool es_edicion { get; set; }

        public List<CntxConAsientoDetalleDto> detalle { get; set; } = new();
    }


}



