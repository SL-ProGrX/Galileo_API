namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrPlazosBonificacionPlanData
    {
        public string cod_plazo_bono { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrPlazosBonificacionDefinicionData
    {
        public string cod_plazo_bono { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }

    public class CrPlazosBonificacionDefinicionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public bool editar { get; set; } = false;
        public string codigo_original { get; set; } = string.Empty;
        public CrPlazosBonificacionDefinicionData definicion { get; set; } = new();
    }

    public class CrPlazosBonificacionDefinicionEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_plazo_bono { get; set; } = string.Empty;
    }

    public class CrPlazosBonificacionBonificacionData
    {
        public int linea { get; set; } = 0;
        public int inicio { get; set; } = 0;
        public int corte { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrPlazosBonificacionBonificacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_plazo_bono { get; set; } = string.Empty;
        public CrPlazosBonificacionBonificacionData bonificacion { get; set; } = new();
    }

    public class CrPlazosBonificacionBonificacionEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_plazo_bono { get; set; } = string.Empty;
        public int linea { get; set; } = 0;
    }

    public class CrPlazosBonificacionAsignacionData
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrPlazosBonificacionAsignacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_plazo_bono { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }
}