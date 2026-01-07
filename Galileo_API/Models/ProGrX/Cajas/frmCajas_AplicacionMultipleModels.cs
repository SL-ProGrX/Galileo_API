namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasAmValidacionDto
    {
        public string advertencias { get; set; }
        public string validacion { get; set; }
    }

    public class CajasAmSeleccionadoDto
    {
        public long operacion { get; set; }
        public string? linea { get; set; }
        public decimal saldo { get; set; }
        public string? tipo { get; set; }
        public decimal abono { get; set; }
        public string? garantia { get; set; }
        public string? descripcion { get; set; }
        public long creditos_id { get; set; }
    }

    public class CajasCreditoPendienteDto
    {
        public long operacion { get; set; }
        public string? linea { get; set; }
        public decimal saldo { get; set; }
        public decimal abono { get; set; }
        public string? ultimoPago { get; set; }
        public string? garantia { get; set; }
        public string? descripcion { get; set; }
    }

    public class CajasAmAgregarRequestDto
    {
        public string? codCaja { get; set; }
        public int codApertura { get; set; }
        public string? tiquete { get; set; }
        public string? cedula { get; set; }
        public long operacion { get; set; }
        public string? linea { get; set; }
        public string? tipoAbono { get; set; }
        public DateTime fecha { get; set; }
        public decimal abono { get; set; }
        public decimal saldo { get; set; }
        public decimal intCor { get; set; }
        public decimal intMor { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public decimal polizas { get; set; }
    }

    public class CajasAmAplicarRequestDto
    {
        public string? codCaja { get; set; }
        public int codApertura { get; set; }
        public string? tiquete { get; set; }
        public string? usuario { get; set; }
        public string? cedula { get; set; }
        public decimal total { get; set; }
        public string? divisa { get; set; }
        public string? notas { get; set; }
        public int sesionId { get; set; }
        public string? tipoDocumento { get; set; }
    }

    public class CajasAMCreditosPendientesRequestDto
    {
        public string? cedula { get; set; }
        public string? codcaja { get; set; }
        public int codapertura { get; set; }
        public string? tiquete { get; set; }
        public DateTime fechacorte { get; set; }
        public string? tipomovimiento { get; set; }
        public DateTime? fechapago { get; set; }
    }




}