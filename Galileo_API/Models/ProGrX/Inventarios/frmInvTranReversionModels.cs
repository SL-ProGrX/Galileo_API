namespace Galileo.Models.INV
{
    public class TranReversionData
    {
        public string Boleta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cod_Entsal { get; set; } = string.Empty;
        public string Causa { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Plantilla { get; set; } = false;
        public string Documento { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Genera_User { get; set; } = string.Empty;
        public DateTime Genera_Fecha { get; set; }
        public string Autoriza_User { get; set; } = string.Empty;
        public DateTime Autoriza_Fecha { get; set; }
        public string Procesa_User { get; set; } = string.Empty;
        public DateTime Procesa_Fecha { get; set; }
        public float Total { get; set; }
        public string Asiento_Numero { get; set; } = string.Empty;
    }

    public class TranReversionInsert
    {
        public string Boleta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cod_Entsal { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
    }

    public class InvProducReversion
    {
        public int linea { get; set; }
        public string Cod_Producto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public float Cantidad { get; set; }
        public string Cod_Bodega { get; set; } = string.Empty;
        public string Bodega { get; set; } = string.Empty;
        public string? Cod_Bodega_Destino { get; set; } = string.Empty;
        public string? BodegaD { get; set; } = string.Empty;
        public float Precio { get; set; }
        public float Total { get; set; }
        public float Despacho { get; set; }
    }
}