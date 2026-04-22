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

    public class CoHistorialDto
    {
        public DateTime? fecha { get; set; }
        public string transaccion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class COGestionDto
    {
        public int cod_seg { get; set; }
        public DateTime fecha { get; set; }
        public int tiempo_resolucion { get; set; }
        public string gestion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string arreglo { get; set; } = string.Empty;
        public DateTime? arreglo_vence { get; set; }
        public string causa { get; set; } = string.Empty;
    }

    public class COCobroFiadorRowDto
    {
        public int operacion { get; set; }
        public string linea { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public decimal recaudo { get; set; }
        public decimal aplicado { get; set; }
        public decimal devuelto { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? ult_mov { get; set; }
        public string estado { get; set; } = string.Empty;
    }


    public class COTrasladoDeudaRowDto
    {
        public int operacion { get; set; }
        public string linea { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public decimal cuota { get; set; }
        public decimal tasa { get; set; }

        public int plazo { get; set; }

        public decimal interespendiente { get; set; }
        public decimal cargos { get; set; }
        public decimal polizas { get; set; }

        public string estado { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class COTrasladoDeudaRevertirRequestDto
    {
        public int operacion { get; set; }
        public string usuario { get; set; } = string.Empty;

        public decimal nuevomonto { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal tasapts { get; set; }

        public List<int> operacionesseleccionadas { get; set; } = new();
    }

    public class COContactoTelefonoDto
    {
        public string tipo { get; set; } = string.Empty;
        public string numero { get; set; } = string.Empty;
        public string ext { get; set; } = string.Empty;
        public string contacto { get; set; } = string.Empty;
    }

    public class COContactoDto
    {
        public List<COContactoItemDto> contactos { get; set; } = new();
    }

    public class COContactoItemDto
    {
        public string identificacion { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;

        public string calidad { get; set; } = string.Empty;

        public string registro { get; set; } = string.Empty;

        public List<COContactoTelefonoDto> telefonos { get; set; } = new();

        public string direccion { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string apartado { get; set; } = string.Empty;
    }

    public class COMoraDto
    {
        public string proceso { get; set; } = "";
        public DateTime? fecha { get; set; }

        public decimal intCor { get; set; }
        public decimal intMor { get; set; }
        public decimal cargo { get; set; }
        public decimal poliza { get; set; }
        public decimal principal { get; set; }

        public string tipo { get; set; } = "";
        public string ncon { get; set; } = "";
        public string usuario { get; set; } = "";
        public string concepto { get; set; } = "";
    }

    public class COEjecutivoDto
    {
        public DateTime? fecha { get; set; }
        public string oficial { get; set; } = string.Empty;
        public bool mantiene { get; set; }
        public bool rebajo { get; set; }
        public bool dobleMora { get; set; }
    }

}
