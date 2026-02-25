namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrVerificaDatosListaResult<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CrVerificaDatosCabeceraDto
    {
        public string cod_institucion { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;

        public string cod_departamento { get; set; } = string.Empty;
        public string departamento { get; set; } = string.Empty;

        public string cod_seccion { get; set; } = string.Empty;
        public string seccion { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrVerificaDatosContactoDto
    {
        public string genero { get; set; } = string.Empty;
        public string estado_civil { get; set; } = string.Empty;
        public string nacionalidad { get; set; } = string.Empty;

        public string nacimiento { get; set; } = string.Empty;
        public string email_1 { get; set; } = string.Empty;
        public string email_2 { get; set; } = string.Empty;
        public string apto_postal { get; set; } = string.Empty;

        public string provincia { get; set; } = string.Empty;
        public string canton { get; set; } = string.Empty;
        public string distrito { get; set; } = string.Empty;

        public string direccion { get; set; } = string.Empty;
        public string notificaciones { get; set; } = string.Empty;
        public string provincia_cod { get; set; } = string.Empty;
        public string canton_cod { get; set; } = string.Empty;
        public string distrito_cod { get; set; } = string.Empty;
        public string estado_laboral { get; set; } = string.Empty;
        public string nombramiento_fecha { get; set; } = string.Empty;
        public string fecha_ingreso { get; set; } = string.Empty;
        public string estado_civil_cod { get; set; } = string.Empty;
        public string cod_nacionalidad { get; set; } = string.Empty;
    }

    public class CrVerificaDatosConyugeDto
    {
        public string conyuge_identificacion { get; set; } = string.Empty;
        public string conyuge_nombre { get; set; } = string.Empty;
        public string conyuge_trabajo { get; set; } = string.Empty;
        public string conyuge_extension { get; set; } = string.Empty;
        public string conyuge_movil { get; set; } = string.Empty;

        public string albacea_identificacion { get; set; } = string.Empty;
        public string albacea_nombre { get; set; } = string.Empty;
    }

    public class CrVerificaDatosNombramientoItem
    {
        public string estado { get; set; } = string.Empty;
        public string a_partir { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrVerificaDatosNombramientoAgregarRequest
    {
        public string? identificacion { get; set; }
        public string? estado { get; set; }
        public string? a_partir { get; set; }
        public int? caps { get; set; }
        public string? usuario_sesion { get; set; }
    }

    public class CrVerificaDatosAsignarCatalogoRequest
    {
        public string? identificacion { get; set; }
        public string? cod_item { get; set; } 
        public bool? asignar { get; set; }
        public string? usuario_sesion { get; set; }
        public string? tipo { get; set; }
    }

    public class CrVerificaDatosPersonaF4Item
    {
        public string item { get; set; } = string.Empty; 
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrVerificaDatosCompletoDto
    {
        public CrVerificaDatosCabeceraDto cabecera { get; set; } = new();
        public CrVerificaDatosContactoDto contacto { get; set; } = new();
        public CrVerificaDatosConyugeDto conyuge { get; set; } = new();

        public List<CrVerificaDatosChecklistItem> bienes { get; set; } = new();
        public List<CrVerificaDatosChecklistItem> canales { get; set; } = new();
        public List<CrVerificaDatosChecklistItem> gustos { get; set; } = new();
        public List<CrVerificaDatosChecklistItem> escolaridad { get; set; } = new();
    }

    public class CrVerificaDatosChecklistItem
    {
        public string cod_item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int asignado { get; set; }
    }
    public class CrVerificaDatosGuardarRequest
    {
        public string? identificacion { get; set; }

        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }
        public string? direccion { get; set; }

        public string? estado_civil { get; set; }        
        public string? cod_nacionalidad { get; set; }
        public string? sexo { get; set; }             
        public string? fecha_nac { get; set; }           

        public string? apto_postal { get; set; }
        public string? email_1 { get; set; }
        public string? email_2 { get; set; }

        public string? estado_laboral { get; set; }      
        public string? nombramiento_fecha { get; set; }  

        public string? conyuge_cedula { get; set; }
        public string? conyuge_nombre { get; set; }
        public string? conyuge_tel_cell { get; set; }
        public string? conyuge_tel_tra { get; set; }
        public string? conyuge_tel_tra_ext { get; set; }

        public string? albacea_cedula { get; set; }
        public string? albacea_nombre { get; set; }

        public string? notificaciones { get; set; }

        public string? usuario_sesion { get; set; }

        public bool? guardar_direccion_trabajo { get; set; }
        public string? tra_provincia { get; set; }
        public string? tra_canton { get; set; }
        public string? tra_distrito { get; set; }
        public string? tra_direccion { get; set; }
    }
}