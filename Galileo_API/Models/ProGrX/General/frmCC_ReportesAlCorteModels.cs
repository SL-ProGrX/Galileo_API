namespace Galileo.Models.GEN
{
    public class CCGenericList
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class CbrAnalisisCubosData
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public string linea_codigo { get; set; } = string.Empty;
        public string linea_descripcion { get; set; } = string.Empty;
        public string retencion { get; set; } = string.Empty;
        public string poliza { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal ultimo_mov { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string recurso { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public int plazo_restante { get; set; }
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota_corte { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal primer_deduc { get; set; }
        public decimal mora_intereses { get; set; }
        public decimal mora_cargos { get; set; }
        public decimal mora_principal { get; set; }
        public int mora_cuotas { get; set; }
        public decimal mora_cta_antigua { get; set; }
        public string antiguedad { get; set; } = string.Empty;
        public string anterior { get; set; } = string.Empty;
        public decimal mora_financiera { get; set; }
        public decimal saldo_pa_cbr { get; set; }
        public decimal mora_legal { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public string provincia { get; set; } = string.Empty;
        public string comite_evaluador { get; set; } = string.Empty;
        public string depart_codigo { get; set; } = string.Empty;
        public string departamento { get; set; } = string.Empty;
        public int no_operacion { get; set; }
        public int membresia { get; set; }
        public string tipo_cartera { get; set; } = string.Empty;
        public string estado_persona { get; set; } = string.Empty;
        public string estado_laboral_desc { get; set; } = string.Empty;
        public int operaciones { get; set; }
    }

    public class CbrEstimacionData
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int operacion { get; set; }
        public string linea_codigo { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public string garantia_desc { get; set; } = string.Empty;
        public string cod_antiguedad { get; set; } = string.Empty;
        public string antiguedad_estado { get; set; } = string.Empty;
        public decimal est_garantia_mitigador { get; set; }
        public decimal est_porc_saldo_cubierto { get; set; }
        public decimal est_porc_saldo_descubierto { get; set; }
        public decimal saldo { get; set; }
        public int operaciones { get; set; }
        public decimal garantia_monto { get; set; }
        public decimal saldo_descubierto { get; set; }
        public decimal est_resultado { get; set; }
    }
}