namespace PgxAPI.Models
{
    public class MenuDto
    {
        public int MENU_NODO { get; set; }
        public int NODO_PADRE { get; set; }
        public string NODO_DESCRIPCION { get; set; } = string.Empty;
        public string TIPO { get; set; } = string.Empty;
        public string ICONO { get; set; } = string.Empty;
        public string MODO { get; set; } = string.Empty;
        public string MODAL { get; set; } = string.Empty;
        public string FORMULARIO { get; set; } = string.Empty;
        public int PRIORIDAD { get; set; }
        public int MODULO { get; set; }
        public int MIGRADO_WEB { get; set; }
        public string ICONO_WEB { get; set; } = string.Empty;
    }

    public class MenuDtoV2
    {
        public int MENU_NODO { get; set; }
        public int NODO_PADRE { get; set; }
        public string NODO_DESCRIPCION { get; set; } = string.Empty;
        public string TIPO { get; set; } = string.Empty;
        public string ICONO { get; set; } = string.Empty;
        public string MODO { get; set; } = string.Empty;
        public string MODAL { get; set; } = string.Empty;
        public string FORMULARIO { get; set; } = string.Empty;
        public int PRIORIDAD { get; set; }
        public int MODULO { get; set; }
        public int MIGRADO_WEB { get; set; }
        public string ICONO_WEB { get; set; } = string.Empty;
        public List<MenuDtoV2> NodoHijo { get; set; } = new List<MenuDtoV2>();
    }
}
