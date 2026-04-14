namespace Galileo_API.Models.ProGrX.Cobros
{
    public class OperacionBusquedaDto
    {
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal saldo { get; set; }
    }

    public class OperacionConsultarDto
    {
        public int operacion { get; set; }
        public string descripcion { get; set; } = string.Empty; // NORMAL
        public string estado { get; set; } = string.Empty;      // NO

        public int codInstitucion { get; set; }

        public string? deductora { get; set; }

        public string linea { get; set; } = string.Empty;
        public string lineaDescripcion { get; set; } = string.Empty;

        public string identificacion { get; set; } = string.Empty;
        public string identificacionDescripcion { get; set; } = string.Empty;
    }

        public class CoEstadoDto
        {
            public string estado { get; set; } = string.Empty;
            public string antiguedad { get; set; } = string.Empty;

            public decimal monto { get; set; }
            public int plazo { get; set; }
            public decimal tasa1 { get; set; }
            public decimal tasa2 { get; set; }
            public decimal cuota { get; set; }
            public decimal amortizado { get; set; }
            public decimal interes_pagado { get; set; }

            public string garantia { get; set; } = string.Empty;
            public string documento { get; set; } = string.Empty;
            public string primer_cuota { get; set; } = string.Empty;
            public string ultima_cuota { get; set; } = string.Empty;

            public decimal saldo { get; set; }
            public decimal interes_corriente { get; set; }
            public decimal interes_moratorio { get; set; }
            public decimal principal_atrasado { get; set; }
            public decimal cargos { get; set; }
            public decimal polizas { get; set; }
            public decimal mora_financiera { get; set; }
            public decimal mora_legal { get; set; }
            public decimal total_deuda { get; set; }
            public decimal intereses_hoy { get; set; }

            public DateTime? fecha_corte { get; set; }
        }

}
