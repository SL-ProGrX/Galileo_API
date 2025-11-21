namespace Galileo.Models.ProGrX.Clientes
{
    public class AfDepartamentosLista
    {
        public int total { get; set; } = 0;
        public List<AfDepartamentosDto> lista { get; set; } = new List<AfDepartamentosDto>();
    }

    public class AfDepartamentosDto
    {
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_institucion { get; set; }
        public string activo { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; } = null;
        public string? registro_usuario { get; set; } = string.Empty;
    }

    public class AfSeccionesLista
    {
        public int total { get; set; } = 0;
        public List<AfSeccionesDto> lista { get; set; } = new List<AfSeccionesDto>();
    }

    public class AfSeccionesDto
    {
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_institucion { get; set; } 
        public string cod_seccion { get; set; } = string.Empty;
        public string activo { get; set; } = string.Empty;
        public Nullable<DateTime> registro_fecha { get; set; } = null;
        public string? registro_usuario { get; set; } = string.Empty;
    }
}