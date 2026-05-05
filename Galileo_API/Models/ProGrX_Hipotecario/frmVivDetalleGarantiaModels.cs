using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivDetalleGarantiaLista
    {
        public int total { get; set; }
        public List<VivDetalleGarantiaData> lista { get; set; } = new();
    }

    public class VivDetalleGarantiaData
    {
        public int id_garantia { get; set; }
        public short linea { get; set; }
        public string propietario { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string grado_hipoteca { get; set; } = string.Empty;
        public string desc_grado_hipoteca { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public bool isNew { get; set; }
    }

    public class VivDetalleGarantiaGuardarDto
    {
        public int? id_garantia { get; set; }
        public short? linea { get; set; }
        public string propietario { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public string grado_hipoteca { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public bool? isNew { get; set; }
    }

    public class VivDetalleGarantiaEliminarDto
    {
        public int? id_garantia { get; set; }
        public short? linea { get; set; }
        public string usuario { get; set; } = string.Empty;
    }
}