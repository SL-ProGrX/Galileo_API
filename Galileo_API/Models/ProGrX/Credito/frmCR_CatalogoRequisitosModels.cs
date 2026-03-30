namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrRequisitosData
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool visible { get; set; } = false;
        public bool opcionalX { get; set; } = false;
        public bool existe { get; set; } = false;
    }

    public class CrRequisitoAsignacionRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string nivel { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string codRequisito { get; set; } = string.Empty;
        public bool opcional { get; set; } = false;
        public bool isChecked { get; set; } = false;
        public int columna { get; set; } = 4;
    }

    public sealed class RequisitoNivelConfig
    {
        public string Tabla { get; }
        public string CampoCatalogo { get; }
        public string DescripcionBitacora { get; }

        public RequisitoNivelConfig(string tabla, string campoCatalogo, string descripcionBitacora)
        {
            Tabla = tabla;
            CampoCatalogo = campoCatalogo;
            DescripcionBitacora = descripcionBitacora;
        }
    }
}
