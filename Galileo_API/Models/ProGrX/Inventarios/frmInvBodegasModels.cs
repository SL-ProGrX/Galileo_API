namespace Galileo.Models.INV
{
    public class PermisosBodegasDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? Ult_Modificacion { get; set; }
        public bool E_Modifica { get; set; }
        public bool E_Autoriza { get; set; }
        public bool E_Procesa { get; set; }
    }

    public class BodegasDto
    {
        public string Cod_Bodega { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cta_Gastostf { get; set; } = string.Empty;
        public string Cod_Cta_Ingresostf { get; set; } = string.Empty;
        public int Permite_Entradas { get; set; }
        public int Permite_Salidas { get; set; }
        public int Utiliza_Permisos { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}