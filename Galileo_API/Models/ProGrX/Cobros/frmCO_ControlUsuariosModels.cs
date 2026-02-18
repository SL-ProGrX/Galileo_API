namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoControlUsuariosData
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public int estado { get; set; }
        public int aplica_comision { get; set; }
        public int operador_externo { get; set; } 

        public decimal porc_comision { get; set; }
        public long tiempo_resolucion_com { get; set; }

        public int cod_banco { get; set; }
        public string banco_desc { get; set; } = string.Empty;

        public string tipo_documento { get; set; } = string.Empty; 
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string modifica_fecha { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
    }

    public class CoControlUsuariosF4Item
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CoControlUsuariosCuentasData
    {
        public string cuenta { get; set; } = string.Empty;     
        public string banco { get; set; } = string.Empty;  
        public string tipo { get; set; } = string.Empty;     
        public string cod_divisa { get; set; } = string.Empty;
        public string interbanca { get; set; } = string.Empty;    
        public string destino { get; set; } = string.Empty;
        public string activa { get; set; } = string.Empty;  
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class CoControlUsuariosGrupoItem
    {
        public int id_grupo { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CoControlUsuariosCarteraItem
    {
        public string cod_clasificacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CoControlUsuariosListaResult<T>
    {
        public int total { get; set; }
        public List<T> lista { get; set; } = new();
    }

    public class CoControlUsuariosGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public int? estado { get; set; }
        public int? aplica_comision { get; set; } 
        public int? operador_externo { get; set; }

        public decimal? porc_comision { get; set; }
        public long? tiempo_resolucion_com { get; set; }

        public int? cod_banco { get; set; }
        public string tipo_documento { get; set; } = string.Empty;

        public bool? edita { get; set; }
        public string usuario_sesion { get; set; } = string.Empty;
    }

    public class CoControlUsuariosAsignacionRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string usuario_sesion { get; set; } = string.Empty;
        public bool? asignar { get; set; }
        public int? id_grupo { get; set; }
        public string? cod_clasificacion { get; set; }
    }
}
