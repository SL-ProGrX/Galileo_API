namespace Galileo_API.Models.ProGrX_Polizas
{
    // Base para créditos y operaciones de póliza
    public class PolizaPersonaOperacionBaseDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Linea_Desc { get; set; } = string.Empty;
        public string Garantia { get; set; } = string.Empty;
        public string Garantia_Desc { get; set; } = string.Empty;
        public DateTime? FechaForp { get; set; }
        public decimal? MontoApr { get; set; }
        public decimal? InteresV { get; set; }
        public int? Plazo { get; set; }
        public decimal? Saldo { get; set; }
        public decimal? Cuota { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? Proceso { get; set; }
        public int? Prideduc { get; set; }
        public DateTime? Fecult { get; set; }
    }

    public class PolizaPersonaOperacionPolizaDto : PolizaPersonaOperacionBaseDto
    {
        public int? Crd_N_Operacion { get; set; }
        public string? Crd_Codigo { get; set; }
        public string? Cod_Poliza { get; set; }
        public string? Num_Poliza { get; set; }
    }

    public class PolizaPersonaFiltroDto
    {
        public string Cedula { get; set; } = string.Empty;
        public string Cedular { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class PolizaPersonaFiltroParams
    {
        public string Orden { get; set; } = "nombre"; // "nombre" o "cedula"
    }

    public class PolizaPersonaCreditoParams
    {
        public string Cedula { get; set; } = string.Empty;
    }

    public class PolizaPersonaOperacionPolizaParams
    {
        public int Operacion { get; set; }
    }

    public class PolizaPersonaReclamoDto
    {
        public int Id { get; set; }
        public int Id_Solicitud_Poliza { get; set; }
        public string Codigo_Poliza { get; set; } = string.Empty;
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Primer_Apellido { get; set; } = string.Empty;
        public string Segundo_Apellido { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public DateTime? Fecha_Nacimiento { get; set; }
        public decimal? Monto_Aprobado { get; set; }
        public byte Estado_Actual { get; set; }
        public string? Finca { get; set; }
        public byte? Tipo_Siniestro { get; set; }
        public byte? Causa_Siniestro { get; set; }
        public byte? Motivo_Reclamo { get; set; }
        public byte? Enfermedad { get; set; }
        public byte? Edad { get; set; }
        public byte Forma_Desembolso { get; set; }
        public byte Metodo_Pago { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string? Registro_Observaciones { get; set; }
        public DateTime? Recepcion_Fecha { get; set; }
        public string? Recepcion_Usuario { get; set; }
        public string? Recepcion_Observaciones { get; set; }
        public bool Fondo_Generado { get; set; }
        public int? Codigo_Fondo { get; set; }
        public bool Aportacion_Aplicada { get; set; }
        public bool Deposito_Recibido { get; set; }
        public DateTime? Recepcion_Deposito_Fecha { get; set; }
        public string? Placa { get; set; }
        public byte? Tipo_Siniestro_Vhc { get; set; }
        public byte? Causa_Siniestro_Vhc { get; set; }
        public string? Cod_Plan { get; set; }
        public string Estado_Desc { get; set; } = string.Empty;
        public string Nombre_Completo { get; set; } = string.Empty;
    }

    public class PolizaPersonaReclamoParams
    {
        public int Operacion { get; set; }
        public int OperacionPoliza { get; set; }
    }
}
