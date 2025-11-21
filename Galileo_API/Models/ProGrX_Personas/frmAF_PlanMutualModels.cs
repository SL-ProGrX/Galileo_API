namespace Galileo.Models.ProGrX_Personas
{
    public class AfPlanMutualFiltros
    {
       public string? plan { get; set; }
       public string? cedula { get; set; }
       public string? idAlterna { get; set; }
       public string? nombre { get; set; }
       public string? estado { get; set; }
       public int lineas { get; set; } = 0;
    }

    public class AfPlanPersonaslLista
    {
        public int total { get; set; }
        public List<AfPlanMutualPersonasData>? lista { get; set; }
    }

    public class AfPlanMutualPersonasData
    {
        public string? cedula { get; set; }
        public string? id_alterna { get; set; }
        public string? nombre { get; set; }
        public bool excluye { get; set; } = false;
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
    }

    public class AfPlanMutualLista
    {
        public int total { get; set; }
        public List<AfPlanMutualDto>? lista { get; set; }
    }

    public class AfPlanMutualDto
    {
        public string? cod_plan { get; set; }
        public string? descripcion { get; set; }
        public decimal? monto { get; set; }
        public string? codigio_retencion { get; set; }
        public bool activo { get; set; } = false;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public bool isNew { get; set; } = false;
    }
}