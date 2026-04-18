namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfcdAprobacionDto
    {
        public int valorx { get; set; }
        public string noperacion { get; set; } = string.Empty;
        public string cod_comite { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public decimal total { get; set; }
    }

    public class AfcdAprobacionRequest
    {
        public int codEmpresa { get; set; }
        public List<string> operaciones { get; set; } = new();
        public string usuario { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
    }

    public class AfcdRechazoRequest
    {
        public int codEmpresa { get; set; }
        public List<string> operaciones { get; set; } = new();
    }

    public class OficinaUsuarioAprobacionDto
    {
        public string titular { get; set; } = string.Empty;
        public string apoyo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_unidad { get; set; }
        public int cod_centro_costo { get; set; }
        public int inconsistencia { get; set; }
    }
}
