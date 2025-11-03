namespace PgxAPI.Models.ProGrX.Fondos
{

    public class Fnd_Calculadora_Planes
    {
        public string Tipo_Deduc { get; set; }
        public decimal Porc_Deduc { get; set; }
        public int Tipo_Cdp { get; set; }
        public bool Pago_Cupones { get; set; }
        public DateTime? Web_Vence { get; set; }
        public bool Capitaliza_Rendimientos { get; set; }
        public decimal Tasa_Margen_Negociacion { get; set; }
    }

    public class Fnd_Calculadora_Inversiones_FlujoData
    {
        public int Secuencia { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasReconocimiento { get; set; }
        public decimal Tasa { get; set; }
        public decimal BaseCalculo { get; set; }
        public decimal InteresesGanados { get; set; }
        public decimal AportacionExtra { get; set; }
        public decimal InteresGanadoAcumulado { get; set; }
        public decimal ISR { get; set; }
        public decimal TasaNeta { get; set; }
        public decimal InteresesGanadosNetos { get; set; }
        public decimal InteresGanadoAcumuladoNeto { get; set; }
        public decimal MontoInversionNeto { get; set; }
    }

    public class Filtros_Calculadora
    {
        public int pCalculoId { get; set; }
        public decimal txtInversion { get; set; }
        public decimal Plazo { get; set; }
        public decimal pTasa { get; set; }
        public decimal pTP_Sol { get; set; }
        public string pFrecuenciaPago { get; set; }
        public decimal txtMonto { get; set; }
        public bool chkCapitaliza { get; set; }
        public string Cedula { get; set; }
        public string Plan { get; set; }
        public string Usuario { get; set; }
    }
}
