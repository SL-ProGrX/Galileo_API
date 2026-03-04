namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesContratosModels
    {
       
        public class ClientesContratosData
        {
            public string? Cod_Contrato { get; set; } = string.Empty;
            public string? Descripcion { get; set; } = string.Empty;
            public string? Notas { get; set; } = string.Empty;
            public string? Contrato_Num { get; set; } = string.Empty;
            public DateTime? Contrato_Vence { get; set; }
            public string? Contrato_Tipo { get; set; } = string.Empty;
            public int? Plazo { get; set; }
            public decimal Tasa_Corriente { get; set; } = 0;
            public decimal Tasa_Mora { get; set; } = 0;
            public string? Registro_Usuario { get; set; } = string.Empty;
            public DateTime? Registro_fecha { get; set; }
            public string? Actualiza_usuario { get; set; } = string.Empty;
            public DateTime? Actualiza_fecha { get; set; }
            public bool? Activo { get; set; }
            public string? Cedula { get; set; } = string.Empty;
            public bool IsNew { get; set; } = false;
        }
        public class PersonasContratosSuscripcionesData
        {
            public string? Cod_Contrato { get; set; } = string.Empty;
            public string? Cedula { get; set; } = string.Empty;
            public string? Descripcion { get; set; } = string.Empty;
            public string? Cod_cargo { get; set; } = string.Empty;
            public int? Frecuencia_dias { get; set; }
            public DateTime? Pago_ultimo { get; set; }
            public DateTime? Pago_proximo { get; set; }
            public decimal Recaudado { get; set; } = 0;
            public string? Frecuencia_tipo { get; set; } = string.Empty;
            public string? Tipo { get; set; } = string.Empty;
            public decimal Valor { get; set; } = 0;
            public int? Modifica { get; set; }
            public DateTime? Registro_fecha { get; set; }
            public string? Contrato_tipo { get; set; } = string.Empty; 
            public string? Registro_usuario { get; set; } = string.Empty;
            public bool Activo { get; set; } = true;
        }
        public class PersonasContratosPagadoresData
        {
            public string? Cod_Contrato { get; set; } = string.Empty;
            public string? Cedula { get; set; } = string.Empty;
            public string? Nombre { get; set; } = string.Empty;
            public string? Cedula_pagador { get; set; } = string.Empty;        
            public string? Registro { get; set; } = string.Empty;
            public bool Activo { get; set; } = true;
            
        }

        public class CxcPersonaContratosPagadorDto : PersonasContratosPagadoresData
        {
            public string? Nombre { get; set; } // de CxC_Personas
            public string Cedula_Pagador { get; set; } = string.Empty;
        }
    }
}
