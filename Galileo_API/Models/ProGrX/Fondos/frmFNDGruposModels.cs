using System.Text.Json.Serialization;

namespace Galileo.Models.ProGrX.Fondos
{

    public class FndGrupoDto
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public required bool interno { get; set; }
        public required int prioridad { get; set; }
    }

    public class FndPlanGrupoDto
    {
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
    }

}