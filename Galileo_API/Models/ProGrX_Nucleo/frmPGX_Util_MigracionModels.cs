using System;

namespace PgxAPI.Models.ProGrX_Nucleo
{

    public class PgxMigracionData
    {
        public string codigo { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string formaliza { get; set; } = string.Empty;
        public string pri_deduccion { get; set; } = string.Empty;
        public string fec_ult { get; set; } = string.Empty;
        public string monto { get; set; } = string.Empty;
        public string plazo { get; set; } = string.Empty;
        public string tasa { get; set; } = string.Empty;
        public string cuota { get; set; } = string.Empty;
        public string saldo { get; set; } = string.Empty;
    }
        public class PgxMigracionData_2
    {
        public string codigo { get; set; } = string.Empty;
        public int id_comite { get; set; }
        public string cedula { get; set; } = string.Empty;
        public decimal montosol { get; set; }
        public decimal montoapr { get; set; }
        public decimal monto_girado { get; set; }
        public decimal saldo { get; set; }
        public decimal amortiza { get; set; }
        public decimal interesc { get; set; }
        public decimal saldo_mes { get; set; }
        public decimal cuota { get; set; }
        public decimal INT { get; set; }
        public decimal interesv { get; set; }
        public int plazo { get; set; }
        public string userrec { get; set; } = string.Empty;
        public string userres { get; set; } = string.Empty;
        public string userfor { get; set; } = string.Empty;
        public string usertesoreria { get; set; } = string.Empty;
        public string tesoreria { get; set; } = string.Empty;
        public DateTime fechasol { get; set; } 
        public DateTime fechares { get; set; }
        public DateTime fechaforp { get; set; }
        public DateTime fechaforf { get; set; }
        public DateTime fecha_calculo_int { get; set; }
        public string garantia { get; set; } = string.Empty;
        public string primer_cuota { get; set; } = string.Empty; 
        public string tdocumento { get; set; } = string.Empty;
        public decimal pagare { get; set; }  
        public int firma_deudor { get; set; }  
        public int premio { get; set; } 
        public string observacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal prideduc { get; set; }  
        public decimal fecult { get; set; }  
        public string ESTADOSOL { get; set; } = string.Empty;
        public string documento_referido { get; set; } = string.Empty;
    }
}
