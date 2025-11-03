namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class frmSYS_Contacto_ServicioModels
    {
        public class SysContactoServicioConsultaRequest
        {
            public string Identificacion { get; set; } = string.Empty;
            public string CodPais { get; set; } = "CRC"; 
        }
        public class SysContactoServicioGeneralLista
        {
            public int total { get; set; }
            public List<SysContactoServicioGeneralData> lista { get; set; } = new();
        }
        // ================
        // Sección: General
        // ================
        public class SysContactoServicioGeneralData
        {
            public string Identificacion { get; set; } = string.Empty;
            public string CodPais { get; set; } = "CRC";
            public int? Tipo_Id { get; set; }
            public string Apellido_1 { get; set; } = "";
            public string Apellido_2 { get; set; } = "";
            public string Nombre { get; set; } = "";
            public DateTime? Fecha_Nacimiento { get; set; }
            public DateTime? Fecha_Caducidad { get; set; }
            public int? Edad { get; set; }
            public string? Sexo { get; set; }
            public string? Estado_Civil { get; set; }
            public string? Profesion { get; set; }
            public decimal? Salario { get; set; }
            public string? Email_01 { get; set; }
            public string? Email_02 { get; set; }
            public string? Email_03 { get; set; }
            public int? Cod_Provincia { get; set; }
            public int? Cod_Canton { get; set; }
            public int? Cod_Distrito { get; set; }
            public string? Direccion { get; set; }
            public string? Provincia { get; set; }
            public string? Canton { get; set; }
            public string? Distrito { get; set; }
        }


        // =================
        // Sección: Teléfonos
        // =================

        public class SysContactoServicioTelefonoData
        {
            public string Identificacion { get; set; } = string.Empty;
            public string CodPais { get; set; } = "CRC";

            public int Num_Linea { get; set; }
            public string Telefono_Tipo { get; set; } = "";
            public string Telefono { get; set; } = "";
            public string? Extension { get; set; }
            public string? Atiende { get; set; }
        }



        // ==================
        // Sección: Direcciones
        // ==================
        public class SysContactoServicioDireccionData
        {
            public string Identificacion { get; set; } = string.Empty;
            public string CodPais { get; set; } = "CRC";
            public int? Num_Linea { get; set; }
            public int Cod_Provincia { get; set; }
            public int Cod_Canton { get; set; }
            public int Cod_Distrito { get; set; }

            public string Provincia { get; set; } = string.Empty;
            public string Canton { get; set; } = string.Empty;
            public string Distrito { get; set; } = string.Empty;

            public string Direccion { get; set; } = string.Empty;

            public bool isNew { get; set; } = false;
        }

        // ================
        // Sección: Empresas
        // ================
        public class SysContactoServicioEmpresaData
        {
            public string Identificacion { get; set; } = string.Empty;
            public string CodPais { get; set; } = "CRC";

            public int Cod_Empresa { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int? Cod_Provincia { get; set; }
            public int? Cod_Canton { get; set; }
            public string? Canton { get; set; }

            public DateTime? Fecha_Ingreso { get; set; }
            public string? Telefono_1 { get; set; }
            public string? Telefono_2 { get; set; }
            public string? Email { get; set; }
            public decimal? Salario { get; set; }
            public bool? Activo { get; set; }
        }
        public class SysContactoServicioTelefonoLista
        {
            public int total { get; set; }
            public List<SysContactoServicioTelefonoData> lista { get; set; } = new();
        }

        public class SysContactoServicioDireccionLista
        {
            public int total { get; set; }
            public List<SysContactoServicioDireccionData> lista { get; set; } = new();
        }

        public class SysContactoServicioEmpresaLista
        {
            public int total { get; set; }
            public List<SysContactoServicioEmpresaData> lista { get; set; } = new();
        }
        public class SysContactoServicioPersonaLookupDto
        {
            public string identificacion { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
        }
        public class SysContactoServicioPersonaLookupLista
        {
            public int total { get; set; }
            public List<SysContactoServicioPersonaLookupDto> lista { get; set; } = new();
        }
    }
}
