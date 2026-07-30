namespace Galileo.Models.AF
{
    public class AfiBeneGruposLista
    {
        public int total { get; set; }
        public List<AfiBeneGrupos> beneficios { get; set; } = new List<AfiBeneGrupos>();
    }

    /// <summary>
    /// Datos de negocio para registrar o retirar una asignación de grupo
    /// (estados, requisitos, motivos o accesos).
    /// </summary>
    public class AfiAsignacionRequest
    {
        /// <summary>Tipo de asignación: 0 Estados, 1 Requisitos, 2 Motivos, 3 Accesos.</summary>
        public int asigna { get; set; }

        /// <summary>Código del grupo al que pertenece la asignación.</summary>
        public string grupo { get; set; } = string.Empty;

        /// <summary>Valor asignado (estado, requisito, motivo o rol según el tipo).</summary>
        public string valor { get; set; } = string.Empty;

        /// <summary>Usuario que realiza el movimiento.</summary>
        public string usuario { get; set; } = string.Empty;

        /// <summary>Movimiento a aplicar: 'A' agrega, 'E' elimina.</summary>
        public string mov { get; set; } = string.Empty;
    }

    public class AfiBeneGrupos
    {
        public int cod_grupo { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string cod_categoria { get; set; } = string.Empty;
        public float monto { get; set; } = 0;
        public bool estado { get; set; } = false;
        public DateTime fecha { get; set; } = DateTime.Now;
        public string user_registra { get; set; } = string.Empty;
    }

    public class AfiBeneGruposAsigandosLista
    {
        public int Total { get; set; }
        public List<AfiBeneGruposAsigandosData> beneficios { get; set; } = new List<AfiBeneGruposAsigandosData>();
    }

    public class AfiBeneGruposAsigandosData
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int monto { get; set; } = 0;
        public int marca { get; set; } = 0;
        public bool activo { get; set; } = false;
    }

    public class AfiGrupoBeneficioData
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public int cod_grupo { get; set; } = 0;
    }

    public class AfiBeneLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfiBeneAsignacionesData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public bool estado { get; set; }
    }
}