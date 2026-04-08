namespace Galileo.Models.ProGrX_Procesos
{
    public class CcCaRemesasTiposLista
    {
        public int total { get; set; }
        public List<CcCaRemesasTiposData> lista { get; set; } = new List<CcCaRemesasTiposData>();
    }

    public class CcCaRemesasTiposData
    {
        public int? cod_remesa { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;

        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }

        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }

        public string cod_entidad { get; set; } = string.Empty;

        public bool isNew { get; set; } = false;
    }

    internal class CcCaRemesasTiposSpResult
    {
        public int Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
        public int IdLLave { get; set; }
    }
}