namespace Galileo.Models.ProGrX_Personas
{
    public class PerfilTransaccionalData
    {
        public int PT_Id { get; set; }
        public decimal Monto_Minimo { get; set; }
        public decimal Monto_Maximo { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Modifica_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
    }

    public class PerfilTransaccionalLista
    {
        public int Total { get; set; }
        public List<PerfilTransaccionalData> Lista { get; set; } = new List<PerfilTransaccionalData>();
    }

    public class PerfilTransaccionalValidate
    {
        public int PT_Id { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }
}