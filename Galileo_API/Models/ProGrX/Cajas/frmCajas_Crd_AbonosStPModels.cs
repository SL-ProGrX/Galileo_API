namespace Galileo_API.Models.ProGrX.Cajas
{
    public sealed class CajasCrdAbonosStPDData
    {
        // reg_creditos
        public long id_solicitud { get; set; }

        public decimal saldo { get; set; }

        public decimal Saldo_mes { get; set; }

        public string proceso { get; set; } = string.Empty;

        public string Divisa { get; set; } = "COL";

        public decimal? interesv { get; set; }

        // Use @ to escape the reserved keyword 'int'
        public int @int { get; set; }   

        public int plazo { get; set; }

        public decimal interesc { get; set; }

        public decimal amortiza { get; set; }

        public long? fecult { get; set; }  // yyyymm (VB6 lo trata como Long)

        public long Prideduc { get; set; }

        public int? opex { get; set; }

        public decimal cuota { get; set; }

        public string codigo { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public int? cuotas_planilla { get; set; }

        public int? cuotas_directas { get; set; }

        public decimal montoApr { get; set; }

        public long? fechaforp { get; set; }

        public string Base_Calculo { get; set; } = string.Empty;

        // socios
        public string nombre { get; set; } = string.Empty;

        // catalogo
        public string descripcion { get; set; } = string.Empty;

        public string retencion { get; set; } = "N"; // 'S' / 'N'

        public string poliza { get; set; } = "N";    // 'S' / 'N'

        public decimal PORC_CARGO_CANCELACION { get; set; }

        // función
        public int Caja_Valida_Concepto { get; set; }
    }

    public class CajasCrdAbonoRequest
    {
        public long id_solicitud { get; set; }
        public int totalCajs { get; set; }
        public string tipoDoc { get; set; }
        public string numDoc { get; set; }
        public int concepto { get; set; }
        public string mUsuario { get; set; }
        public string mCaja { get; set; }
        public int mApertura { get; set; }
        public int chkRecalculaCuota { get; set; }
        public int datosAnticipo { get; set; }
        public string tipo { get; set; }
        public int datosInteres { get; set; }
        public DateTime FechaCancelacion { get; set; }

        public string vNotas { get; set; } = string.Empty;
    }

    public class CajasCrdAbonoMorosidadData
    {
        public int id_moro { get; set; }                 // rs!id_moro
        public int id_solicitud { get; set; }            // rs!Id_Solicitud
        public string fechap { get; set; }               // rs!fechap (formato "####-##")
        public decimal intc { get; set; }                // rs!IntC
        public decimal intm { get; set; }                // rs!IntM
        public decimal amortiza { get; set; }            // rs!Amortiza
        public decimal cargo { get; set; }               // rs!Cargo
        public decimal total { get; set; }               // rs!IntC + rs!IntM + rs!Amortiza + rs!Cargo
        public bool checked_ { get; set; } = true;       // itmX.Checked = True
    }

    public class CajasCrdAbonoCargaOperacionData
    {
        public int id_solicitud { get; set; }
        public decimal saldo { get; set; }
        public decimal saldo_mes { get; set; }
        public decimal interesv { get; set; }
        public decimal @int { get; set; }             // “int” es palabra reservada en C#, se escapa con @
        public int plazo { get; set; }
        public decimal interesc { get; set; }
        public decimal amortiza { get; set; }
        public DateTime fecult { get; set; }
        public string opex { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public int cuotas_planilla { get; set; }
        public int cuotas_directas { get; set; }
        public string retencion { get; set; } = string.Empty;
        public string poliza { get; set; } = string.Empty;
    }

}
