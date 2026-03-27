namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrConceptoDesembData
    {
        public int cod_condeb { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public bool retiene { get; set; } = false;
        public bool modifica { get; set; } = false;
        public bool difiere { get; set; } = false;
        public string difiere_cuenta { get; set; } = "";
        public bool activo { get; set; } = false;   
    }
}
