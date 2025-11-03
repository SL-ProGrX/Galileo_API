using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.Models.ProGrX_Nucleo
{
   
        public class SifOficinasLista
        {
            public int total { get; set; }
            public List<SifOficinasData> lista { get; set; } = new List<SifOficinasData>();
        }

        public class SifOficinasData
        {
            public string cod_oficina { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public string telefono_01 { get; set; } = string.Empty;
            public string telefono_02 { get; set; } = string.Empty;
            public string direccion { get; set; } = string.Empty;
            public string cod_unidad { get; set; } = string.Empty;
            public string cod_centro_costo { get; set; } = string.Empty;
            public string tipo { get; set; } = string.Empty;
            public bool estado { get; set; } = false;
            public DateTime? registro_fecha { get; set; }
            public string registro_usuario { get; set; } = string.Empty;
            public bool oficina_omision { get; set; } = false;
            public bool isNew { get; set; } = false;
        }



    public class SifOficinas
    {
        public string? item { get; set; }
        public string? descripcion { get; set; }
    }

    public class SifOficinasMiembros
    {
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public int asignado { get; set; }
        public bool asignadob => asignado == 1; 
        public string? fecha_ingreso { get; set; }
    }
    public class SifOficinasHistorial
    {
        public string? cod_oficina { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha_ingreso { get; set; }
        public DateTime? fecha_salida { get; set; }
        public string? calidad { get; set; }
        public string tipo => calidad == "T" ? "Titular" : "Apoyo";
        public string? usuario_ingresa { get; set; }
        public string? usuario_salida { get; set; }
    }

}
