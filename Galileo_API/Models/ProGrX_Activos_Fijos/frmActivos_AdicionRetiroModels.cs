namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class ActivosDataLista
    {
        public int total { get; set; }
        public List<ActivosData> lista { get; set; } = new List<ActivosData>();
    }

    public class ActivosData
    {
        public string num_placa { get; set; } = string.Empty;
        public string Placa_Alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class ActivosRetiroAdicionData
    {
        public int  id_addret { get; set; } 
        public string num_placa { get; set; } = string.Empty;
        public string cod_justificacion { get; set; } = string.Empty;
        public string justificacion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string tipoDescripcion { get; set; } = string.Empty;
        public string compra_documento { get; set; } = string.Empty;
        public string tipo_vidautil { get; set; } = string.Empty;
        public int meses_calculo { get; set; }
        public decimal depreciacion_acum { get; set; }
        public decimal depreciacion_mes { get; set; }
        public DateTime? depreciacion_periodo { get; set; }
        public string venta_cliente { get; set; } = string.Empty;
        public string venta_documento { get; set; } = string.Empty;
        public string creacion_user { get; set; } = string.Empty;
        public DateTime creacion_fecha { get; set; }        
        public string nombre { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public decimal valor_libros { get; set; }

    }

    public class ActivosPeriodosData
    {
        public string estado { get; set; } = string.Empty;
        public DateTime periodoactual { get; set; }
    }

    public class ActivosPrincipalData
    {
        public string num_placa { get; set; } = string.Empty;
        public string tipo_activo { get; set; } = string.Empty;
        public string cod_departamento { get; set; } = string.Empty;
        public string cod_seccion { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal valor_historico { get; set; }
        public decimal valor_desecho { get; set; }
        public decimal depreciacion_mes { get; set; }
        public decimal depreciacion_acum { get; set; }
        public DateTime depreciacion_periodo { get; set; }
        public string depreciacionPeriodo { get; set; } = string.Empty;
        public decimal valor_libros { get; set; }


    }

    public class ActivosHistoricoData
    {
        public int id_addret { get; set; }
        public string TipoMov { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string Justifica { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    
    public class ActivosRetiroAdicionCierreData
    {
        public int anio { get; set; }
        public int mes { get; set; } 
        public decimal valor_libros { get; set; }
        public decimal depreciacion_ac { get; set; }
        public decimal depreciacion_mes { get; set; }
        public int ciclo { get; set; } 
    }
}