using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdInformeEspecialPantallaData
    {
        public List<DropDownListaGenericaModel> zonas { get; set; } = new();
        public List<DropDownListaGenericaModel> comites { get; set; } = new();
        public List<DropDownListaGenericaModel> actividades { get; set; } = new();
        public List<DropDownListaGenericaModel> antiguedad { get; set; } = new();
    }
}