namespace Galileo.Models.ProGrX.Creditos
{
    public class CrComitesSemaforoData
    {
        public int id_registro { get; set; }
        public int id_comite { get; set; }
        public string unidad_tiempo { get; set; } = string.Empty;
        public string unidad_tiempo_esp { get; set; } = string.Empty;
        public int alerta_roja { get; set; }
        public int alerta_amarilla { get; set; }
        public DateTime? fecha_inserta { get; set; }
        public string usuario_inserta { get; set; } = string.Empty;
        public DateTime? fecha_actualiza { get; set; }
        public string usuario_actualiza { get; set; } = string.Empty;
    }

    public class CrComitesSemaforoGuardarRequest
    {
        public int id_comite { get; set; }
        public string unidad_tiempo { get; set; } = string.Empty;
        public int alerta_roja { get; set; }
        public int alerta_amarilla { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComitesSemaforoEmailData
    {
        public int id_registro { get; set; }
        public int id_comite { get; set; }
        public string email { get; set; } = string.Empty;
        public DateTime? fecha_inserta { get; set; }
        public string usuario_inserta { get; set; } = string.Empty;
    }

    public class CrComitesSemaforoEmailLista
    {
        public int total { get; set; }
        public List<CrComitesSemaforoEmailData> lista { get; set; } = [];
    }

    public class CrComitesSemaforoEmailAgregarRequest
    {
        public int id_comite { get; set; }
        public string email { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComitesSemaforoEmailEliminarRequest
    {
        public List<int> ids_registro { get; set; } = [];
        public string usuario { get; set; } = string.Empty;
    }
}