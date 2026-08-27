using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class AfRecepcionDevolucionesBeneTagsInicializarData
    {
        public string Tag_Aplicado { get; set; } = string.Empty;
        public string Tag_Devolucion { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> Beneficios { get; set; } = [];
    }

    public sealed class AfRecepcionDevolucionesBeneTagsData
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public long Consec { get; set; }
        public string Cod_Beneficio { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionDevolucionesBeneTagsAplicarItem
    {
        public string Cod_Beneficio { get; set; } = string.Empty;
        public long Consec { get; set; }
    }

    public sealed class AfRecepcionDevolucionesBeneTagsAplicarRequest
    {
        public List<AfRecepcionDevolucionesBeneTagsAplicarItem> Items { get; set; } = [];

        public string Usuario { get; set; } = string.Empty;
    }

    public sealed class AfRecepcionDevolucionesBeneTagsAplicarData
    {
        public int Registros_Aplicados { get; set; }
    }
}
