namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrEnCobroCuotasInicialDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public decimal proceso { get; set; }
        public string proceso_format { get; set; } = string.Empty;
        public string frecuencia_id { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasProcesoScrollDto
    {
        public decimal proceso { get; set; }
        public string proceso_format { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasFiltroDto
    {
        public string texto { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasListaResult<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
        public decimal total_enviado { get; set; }
        public decimal total_recibido { get; set; }
        public decimal total_diferencia { get; set; }
        public decimal total_abono { get; set; }
    }

    public class CrEnCobroCuotasResumenData
    {
        public int operacion { get; set; }
        public string linea { get; set; } = string.Empty;
        public decimal envio { get; set; }
        public decimal recibido { get; set; }
        public decimal diferencia { get; set; }
        public string tipo_desc { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasDetalleData
    {
        public int operacion { get; set; }
        public string linea { get; set; } = string.Empty;
        public decimal proceso { get; set; }
        public string proceso_format { get; set; } = string.Empty;
        public decimal int_cor { get; set; }
        public decimal int_mor { get; set; }
        public decimal cargos { get; set; }
        public decimal principal { get; set; }
        public decimal total_abono { get; set; }
        public decimal enviado { get; set; }
        public decimal diferencia { get; set; }
        public DateTime? fecha { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string caso { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasEnvioData
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public int morosidad { get; set; }
        public string tipo_cuota { get; set; } = string.Empty;
        public string cod_deduccion { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasRecepcionData
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal abono { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string tipo_cuota { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasRecepcionResult
    {
        public int total { get; set; }
        public List<CrEnCobroCuotasRecepcionData> lista { get; set; } = new();
        public decimal total_planilla { get; set; }
        public decimal total_nc { get; set; }
        public decimal total_general { get; set; }
        public decimal total_recaudado { get; set; }
    }

    public class CrEnCobroCuotasHistorialData
    {
        public int proceso { get; set; }
        public string proceso_format { get; set; } = string.Empty;
        public decimal enviado { get; set; }
        public decimal recibido { get; set; }
        public decimal diferencia { get; set; }
        public string institucion { get; set; } = string.Empty;
    }

    public class CrEnCobroCuotasResumenDeductoraData
    {
        public int cod_institucion { get; set; }
        public string desc_corta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal enviado { get; set; }
        public decimal recibido { get; set; }
        public decimal diferencia { get; set; }
    }

    public class CrEnCobroCuotasBitacoraData
    {
        public int id_seq { get; set; }
        public string gestion { get; set; } = string.Empty;
        public string gestion_desc { get; set; } = string.Empty;
        public string transaccion { get; set; } = string.Empty;
        public string transaccion_desc { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }
    public class CrEnCobroCuotasConsultaRequest
    {
        public string cedula { get; set; } = string.Empty;
        public decimal? proceso { get; set; }
        public int? cod_institucion { get; set; }
        public int meses { get; set; } = 12;
        public string parametros { get; set; } = string.Empty;
    }
}