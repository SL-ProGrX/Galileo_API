namespace Galileo.Models.Security
{
    public class PadronConsultarRequestDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string TInfo { get; set; } = string.Empty;
    }

    public class PadronConsultarResponseDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Apellido_1 { get; set; } = string.Empty;
        public string Apellido_2 { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Estado_Civil { get; set; } = string.Empty;
        public DateTime Fecha_Nacimiento { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string Email_01 { get; set; } = string.Empty;
        public string Email_02 { get; set; } = string.Empty;
        public string Email_03 { get; set; } = string.Empty;
        public string Profesion { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Canton { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public decimal Salario { get; set; }
    }

    public class PadronGeneralConsultarResponseDto : PadronConsultarResponseDto
    {
        public new string Identificacion { get; set; } = string.Empty;
        public new string Apellido_1 { get; set; } = string.Empty;
        public new string Apellido_2 { get; set; } = string.Empty;
        public new string Nombre { get; set; } = string.Empty;
        public new string Sexo { get; set; } = string.Empty;
        public new string Estado_Civil { get; set; } = string.Empty;
        public new DateTime Fecha_Nacimiento { get; set; }
        public new string Direccion { get; set; } = string.Empty;
        public new string Email_01 { get; set; } = string.Empty;
        public new string Email_02 { get; set; } = string.Empty;
        public new string Email_03 { get; set; } = string.Empty;
        public new string Profesion { get; set; } = string.Empty;
        public new string Pais { get; set; } = string.Empty;
        public new string Provincia { get; set; } = string.Empty;
        public new string Canton { get; set; } = string.Empty;
        public new string Distrito { get; set; } = string.Empty;
        public new decimal Salario { get; set; }
    }

    public class PadronTelefonosConsultarResponseDto : PadronConsultarResponseDto
    {
        public string Telefono_Tipo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string Atiende { get; set; } = string.Empty;
    }

    public class PadronDireccionesConsultarResponseDto : PadronConsultarResponseDto
    {
        public new string Direccion { get; set; } = string.Empty;
        public new string Pais { get; set; } = string.Empty;
        public new string Provincia { get; set; } = string.Empty;
        public new string Canton { get; set; } = string.Empty;
        public new string Distrito { get; set; } = string.Empty;
    }

    public class PadronEmpresasConsultarResponseDto : PadronConsultarResponseDto
    {
        public new string Nombre { get; set; } = string.Empty;
        public DateTime Fecha_Ingreso { get; set; }
        public new decimal Salario { get; set; }
        public DateTime Salario_Fecha { get; set; }
        public string Telefono_1 { get; set; } = string.Empty;
        public string Telefono_2 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Activo { get; set; } = 0;

    }

    public class ValidaCuentaRequestDto
    {
        public string Cuenta { get; set; } = string.Empty;
        public int CodEmpresa { get; set; }
    }

    public class ErrorGeneralDto
    {
        public string Description { get; set; } = string.Empty;
        public int Code { get; set; }
    }
}
