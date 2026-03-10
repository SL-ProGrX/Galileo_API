namespace Galileo_API.Models.ProGrX_Polizas
{
    public class FrmCRPolizasRegistroBeneficiariosModels
    {

       
       

        public class CRPolizasRegistroBeneficiarios
        {
            public bool EsEdicion { get; set; } = false;
            public int? IdSolicitud { get; set; }
            public int? NumPoliza { get; set; }
            public string CodigoLinea { get; set; } = string.Empty;
            public string IdBeneficiarioOriginal { get; set; } = string.Empty;
            public string IdBeneficiario { get; set; } = string.Empty;
            public string Apellido1 { get; set; } = string.Empty;
            public string Apellido2 { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string NombreCompleto { get; set; } = string.Empty;
            public string Parentesco { get; set; } = string.Empty;
            public DateTime FechaNacimiento { get; set; }
            public decimal? Porcentaje { get; set; }
            public string Observacion { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
            public string ApartadoPostal { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Telefono1 { get; set; } = string.Empty;
            public string Telefono2 { get; set; } = string.Empty;
        }

    

        public class CRPolizasRegistroBeneficiariosEncabezadoResponse
        {
            public int? IdSolicitud { get; set; }
            public int? NumPoliza { get; set; }
            public string NumContrato { get; set; } = string.Empty;
            public string CedulaDeudor { get; set; } = string.Empty;
            public string NombreDeudor { get; set; } = string.Empty;
            public string CodigoLinea { get; set; } = string.Empty;
            public string LineaDescripcion { get; set; } = string.Empty;
            public string PolizaDescripcion { get; set; } = string.Empty;
            public string IdSolicitudPoliza { get; set; } = string.Empty;
        }

        public class CRPolizasRegistroBeneficiariosListaItem
        {
            public int? NumPoliza { get; set; }
            public string IdBeneficiario { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public DateTime? FechaNacimiento { get; set; }
            public string Parentesco { get; set; } = string.Empty;
            public decimal? Porcentaje { get; set; }
        }

     

        public class CRPolizasRegistroBeneficiariosNuevoResponse
        {
            public string IdBeneficiarioSugerido { get; set; } = string.Empty;
            public DateTime? FechaServidor { get; set; }
        }

        public class CRPolizasRegistroBeneficiariosGuardarResponse
        {
            public bool? Guardado { get; set; }
            public string IdBeneficiario { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
        }

    }
}
