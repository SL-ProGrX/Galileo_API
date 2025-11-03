namespace PgxAPI.Models
{
    public class UsDerechosNewDTO
    {

        public int COD_OPCION { get; set; }
        public string FORMULARIO { get; set; } = string.Empty;
        public int MODULO { get; set; }
        public string OPCION { get; set; } = string.Empty;
        public string OPCION_DESCRIPCION { get; set; } = string.Empty;
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
        public string PermisoEstado { get; set; } = string.Empty;
    }

    public class Crear_UsDerechosNewDTO
    {

        public int COD_OPCION { get; set; }
        public string COD_ROL { get; set; } = string.Empty;
        public string ESTADO { get; set; } = string.Empty;
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
    }

    public class PrimeTreeDto
    {

        public object Data { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ExpandedIcon { get; set; } = string.Empty;
        public string CollapsedIcon { get; set; } = string.Empty;
        public bool Expanded { get; set; }
        public bool Selectable { get; set; }
        public string StyleClass { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public List<PrimeTreeDto>? Children { get; set; }
        public bool leaf { get; set; }
    }

    public class UsRolDTO
    {

        public string COD_ROL { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int ACTIVO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
        public int? COD_EMPRESA { get; set; }

    }

    public class PrimeTreeDtoV2
    {

        public object Data { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ExpandedIcon { get; set; } = string.Empty;
        public string CollapsedIcon { get; set; } = string.Empty;
        public bool Expanded { get; set; }
        public bool Selectable { get; set; }
        public string StyleClass { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public List<PrimeTreeDtoV2>? Children { get; set; }
        public bool leaf { get; set; }
        public int[] modules { get; set; } = Array.Empty<int>();
        public int badge { get; set; } = 0;

    }

    public class UsMenuManual
    {
        public string Key { get; set; } = string.Empty;
        public string frame { get; set; } = string.Empty;
    }


}

