using System;
using System.Collections.Generic;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConsultaFianzasConsultaRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = "X";
        public bool canceladas { get; set; } = false;
    }

    public class CrConsultaFianzasDetalleRequest
    {
        public string cedula_deudor { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
    }

    public class CrConsultaFianzasConsultaData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tipo { get; set; } = "X";
        public string titulo_resumen { get; set; } = string.Empty;
        public string titulo_lista { get; set; } = string.Empty;
        public string subtitulo { get; set; } = string.Empty;
        public decimal total_saldos { get; set; } = 0;
        public decimal total_cuotas { get; set; } = 0;
        public int total_casos { get; set; } = 0;
        public List<CrConsultaFianzasItemDto> lista { get; set; } = new();
    }

    public class CrConsultaFianzasItemDto
    {
        public string tipo { get; set; } = string.Empty;
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public int nfiadores { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public int moracta { get; set; } = 0;
        public decimal moramnt { get; set; } = 0;
        public string referencia { get; set; } = string.Empty;
        public string codigoref { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public bool resaltar { get; set; } = false;
        public string mora_desc { get; set; } = string.Empty;
    }

    public class CrConsultaFianzasDetalleData
    {
        public CrConsultaFianzasEstadoDeudorDto estado_deudor { get; set; } = new();
        public List<CrConsultaFianzasMoraDto> mora { get; set; } = new();
    }

    public class CrConsultaFianzasEstadoDeudorDto
    {
        public string cedula { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string membresia { get; set; } = string.Empty;
        public decimal cuotas { get; set; } = 0;
        public decimal saldos { get; set; } = 0;
        public int operaciones { get; set; } = 0;
        public string clasificacion { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public bool resaltar_categoria { get; set; }
    }

    public class CrConsultaFianzasMoraDto
    {
        public int linea { get; set; } = 0;
        public int n_cuota { get; set; } = 0;
        public DateTime? fecha_pago { get; set; }
        public decimal cuota { get; set; } = 0;
        public decimal intereses { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal total { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
    }

    internal sealed class CrConsultaFianzasEstadoDeudorQueryDto
    {
        public string cedula { get; set; } = string.Empty;
        public DateTime? fechaingreso { get; set; }
        public string estadoactual { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public int operaciones { get; set; } = 0;
        public decimal saldos { get; set; } = 0;
        public decimal cuotas { get; set; } = 0;
        public string clasificacion { get; set; } = string.Empty;
    }
}