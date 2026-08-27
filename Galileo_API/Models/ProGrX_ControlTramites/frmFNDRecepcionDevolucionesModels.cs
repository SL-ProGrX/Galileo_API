using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class FndRecepcionDevolucionesInicializarData
    {
        public string Tag_Aplicado { get; set; } = string.Empty;
        public string Tag_Devolucion { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> Planes { get; set; } = [];
    }

    public sealed class FndRecepcionDevolucionesData
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cod_Operadora { get; set; }
        public string Cod_Plan { get; set; } = string.Empty;
        public long Cod_Contrato { get; set; }
    }

    public sealed class FndRecepcionDevolucionesContratoBusquedaData
    {
        public long Cod_Contrato { get; set; }
        public int Cod_Operadora { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionDevolucionesAplicarItem
    {
        public string Cod_Plan { get; set; } = string.Empty;
        public long Cod_Contrato { get; set; }
    }

    public sealed class FndRecepcionDevolucionesAplicarRequest
    {
        public List<FndRecepcionDevolucionesAplicarItem> Items { get; set; } = [];

        public string Usuario { get; set; } = string.Empty;
    }

    public sealed class FndRecepcionDevolucionesAplicarData
    {
        public int Registros_Aplicados { get; set; }
    }
}
