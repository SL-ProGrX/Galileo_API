namespace Galileo.Models.ProGrX.Cajas
{
    public class GestionEstadoDto
    {
        public int Gestion_Id { get; set; }
        public string? Gestion_Estado { get; set; }
    }

    public class FondosAporteAplicarDto
    {
        public string? cedula { get; set; }
        public int operadora { get; set; }
        public string? plan { get; set; }
        public required int contrato { get; set; }
        public required decimal aporte { get; set; }
        public string? tipodoc { get; set; }
        public string? usuario { get; set; }
        public string? caja { get; set; }
        public required int apertura { get; set; }
        public required int sesionid { get; set; }
        public string? tiquete { get; set; }
        public string? nombre { get; set; }
        public string? cod_divisa { get; set; }
        public decimal totalcajas { get; set; }
        public int gestionid { get; set; }
        public string? notas { get; set; }
        public string? oficina { get; set; }
        public int recibodigital { get; set; }
        public decimal montoautorizado { get; set; }
        public string? gestionestado { get; set; }

    }

    public class FondosContratoValidacionDto
    {
        public string? Estado { get; set; }
        public int Permite_Mov_Cajas { get; set; }
    }

    public class FondosAporteAplicarResultDto
    {
        public int Pass { get; set; }
        public string? NumDoc { get; set; }
        public string? Mensaje { get; set; }
        public string? Movimiento { get; set; }
    }

    public class FondosRequiereAutorizacionDto
    {
        public bool requiere { get; set; }
        public decimal montomaximo { get; set; }
    }

    public class FondosGestionRegistroAddDto
    {
        public string? cedula { get; set; }
        public string? tipo { get; set; }
        public string? operadora { get; set; }
        public string? plan { get; set; }
        public required int contrato { get; set; }
        public required decimal montoautorizado { get; set; }
        public required decimal aporte { get; set; }
        public string? usuario { get; set; }
        public string? nota { get; set; }
    }

    public class FondosGestionRegistroDto
    {
        public int gestion_id { get; set; }
        public string? gestion_estado { get; set; }
        public string? gestion_nota { get; set; }
    }

    public class FondosContratoDatosDto
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? plan_desc { get; set; }
        public string? operadora_desc { get; set; }
        public decimal monto { get; set; }
        public string? cod_plan { get; set; }
        public int cod_contrato { get; set; }
        public int cod_operadora { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public string? cod_moneda { get; set; }
        public decimal aportes { get; set; }
        public decimal inversion { get; set; }
        public int tipo_cdp { get; set; }
        public int caja_valida_concepto { get; set; }
    }

    public class FndSubCuentasDto
    {
        public int idx { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal valorfijo { get; set; } = 0;
    }
}
