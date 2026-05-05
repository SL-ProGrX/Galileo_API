namespace Galileo.Models.ProGrX.Cobros
{
    public class CoControlGestionesLista
    {
        public int total { get; set; }
        public List<CoControlGestionesData> lista { get; set; } = new List<CoControlGestionesData>();
    }

    public class CoControlGestionesData
    {
        public string cod_gestion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string codigo_referencia { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0m;
        public bool modifica_usuario { get; set; } = false;
        public decimal modifica_desviacion { get; set; } = 0m;
        public string cod_cuenta { get; set; } = string.Empty;
        public string nivel_gestion { get; set; } = "U";
        public bool acceso_restringido { get; set; } = false;
        public bool mrecuperacion { get; set; } = false;
        public decimal iva_porcentaje { get; set; } = 0m;
        public bool activo { get; set; } = true;
        public bool isNew { get; set; } = false;
    }

    public class CoControlGestionesSeguridadGestionData
    {
        public string cod_gestion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CoControlGestionesSeguridadUsuarioData
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CoControlGestionesSeguridadAsignacionDto
    {
        public string cod_gestion { get; set; } = string.Empty;
        public string usuario_asignado { get; set; } = string.Empty;
        public bool asignar { get; set; } = false;
    }
}
