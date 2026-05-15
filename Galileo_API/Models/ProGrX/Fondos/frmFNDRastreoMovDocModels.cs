namespace Galileo.Models.ProGrX.Fondos
{
    public class FndRastreoMovDocFiltros
    {
        public DateTime fecha_inicio { get; set; } 
        public DateTime fecha_corte { get; set; }
        public int cod_contabilidad { get; set; }
        public int lineas { get; set; }
    }

    public class FndRastreoMovDocResumenData
    {
        public int total { get; set; }
        public string? descripcion { get; set; } 
        public string? fnd_Cuenta { get; set; } 
        public string? fnd_DebeHaber { get; set; } 
        public decimal movimiento { get; set; }
        public string? origen { get; set; }
    }

    public class FndRastreoMovDocDetalleData
    {
        public DateTime fecha { get; set; }
        public string? tipo { get; set; }          
        public string? referencia { get; set; }   
        public string? fnd_Cuenta { get; set; }   
        public decimal mDebe { get; set; }  
        public decimal mHaber { get; set; }
        public string? concepto { get; set; } 
        public string? cliente { get; set; } 
        public string? descripcion { get; set; } 
        public string? usuario { get; set; } 
        public string? extra { get; set; } 
        public string? origen { get; set; }  
    }

    public class FndRastreoMovDocArchivosData
    {
        public DateTime fecha { get; set; }
        public string? tipo { get; set; }
        public string? referencia { get; set; }  
        public string? cuenta { get; set; }          
        public decimal debe { get; set; }
        public decimal haber { get; set; }
        public string? concepto { get; set; }
        public string? cliente { get; set; }
        public string? descripcion { get; set; }
        public string? usuario { get; set; }
        public string? extra { get; set; }      
    }
}