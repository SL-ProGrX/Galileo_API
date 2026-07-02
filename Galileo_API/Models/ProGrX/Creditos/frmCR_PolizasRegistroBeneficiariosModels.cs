namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrPolizasRegistroBeneficiariosEncabezadoData
    {
        public int operacion { get; set; } = 0;
        public int num_poliza { get; set; } = 0;
        public string num_contrato { get; set; } = string.Empty;
        public string cedula_deudor { get; set; } = string.Empty;
        public string nombre_deudor { get; set; } = string.Empty;
        public string codigo_linea { get; set; } = string.Empty;
        public string linea_descripcion { get; set; } = string.Empty;
        public string poliza_descripcion { get; set; } = string.Empty;
        public int id_solicitud_poliza { get; set; } = 0;
    }

    public class CrPolizasRegistroBeneficiariosListaData
    {
        public int num_poliza { get; set; } = 0;
        public string id_beneficiario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fecha_nac { get; set; }
        public string parentesco { get; set; } = string.Empty;
        public string parentesco_descripcion { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public string direccion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string telefono1 { get; set; } = string.Empty;
        public string telefono2 { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string apto_postal { get; set; } = string.Empty;
    }

    public class CrPolizasRegistroBeneficiariosNuevoData
    {
        public string id_beneficiario_sugerido { get; set; } = string.Empty;
        public DateTime? fecha_servidor { get; set; }
    }

    public class CrPolizasRegistroBeneficiariosDetalleData
    {
        public int operacion { get; set; } = 0;
        public int num_poliza { get; set; } = 0;
        public string codigo_linea { get; set; } = string.Empty;
        public string id_beneficiario_original { get; set; } = string.Empty;
        public string id_beneficiario { get; set; } = string.Empty;
        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string nombre_completo { get; set; } = string.Empty;
        public string parentesco { get; set; } = string.Empty;
        public string parentesco_descripcion { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public decimal porcentaje { get; set; } = 0;
        public string observacion { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string apartado_postal { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string telefono1 { get; set; } = string.Empty;
        public string telefono2 { get; set; } = string.Empty;
    }

    public class CrPolizasRegistroBeneficiariosGuardarRequest
    {
        public bool es_edicion { get; set; } = false;
        public int operacion { get; set; } = 0;
        public int num_poliza { get; set; } = 0;
        public string codigo_linea { get; set; } = string.Empty;
        public string id_beneficiario_original { get; set; } = string.Empty;
        public string id_beneficiario { get; set; } = string.Empty;
        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string parentesco { get; set; } = string.Empty;
        public DateTime? fecha_nacimiento { get; set; }
        public decimal? porcentaje { get; set; }
        public string observacion { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string apartado_postal { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string telefono1 { get; set; } = string.Empty;
        public string telefono2 { get; set; } = string.Empty;
    }

    public class CrPolizasRegistroBeneficiariosGuardarData
    {
        public bool guardado { get; set; } = false;
        public string id_beneficiario { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    internal sealed class CrPolizasRegistroBeneficiariosNombreData
    {
        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
}