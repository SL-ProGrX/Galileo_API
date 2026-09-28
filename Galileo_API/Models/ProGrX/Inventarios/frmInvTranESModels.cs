namespace Galileo.Models.INV
{
    public class TranESData
    {
        public string boleta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_entsal { get; set; } = string.Empty;
        public string causa { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public bool plantilla { get; set; } = false;
        public string documento { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string genera_user { get; set; } = string.Empty;
        public DateTime? genera_fecha { get; set; }
        public string autoriza_user { get; set; } = string.Empty;
        public DateTime? autoriza_fecha { get; set; }
        public string procesa_user { get; set; } = string.Empty;
        public DateTime? procesa_fecha { get; set; }
        public decimal total { get; set; } = 0m;
        public string asiento_numero { get; set; } = string.Empty;
    }

    public class TranESUpdate
    {
        public string boleta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_entsal { get; set; } = string.Empty;
        public bool plantilla { get; set; } = false;
        public string documento { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public decimal total { get; set; } = 0m;
        public DateTime? fecha { get; set; }
    }

    public class InvProducLineas
    {
        public int linea { get; set; } = 0;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 0m;
        public string cod_bodega { get; set; } = string.Empty;
        public string bodega { get; set; } = string.Empty;
        public string cod_bodega_destino { get; set; } = string.Empty;
        public decimal precio { get; set; } = 0m;
        public decimal total { get; set; } = 0m;
        public decimal despacho { get; set; } = 0m;
    }

    public class InvProducLineasInsert
    {
        public int linea { get; set; } = 0;
        public string boleta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_bodega { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string cod_bodega_destino { get; set; } = string.Empty;
        public decimal cantidad { get; set; } = 0m;
        public decimal precio { get; set; } = 0m;
        public decimal despacho { get; set; } = 0m;
    }

    public class InvTranPlantilla
    {
        public string boleta { get; set; } = string.Empty;
        public string genera_user { get; set; } = string.Empty;
        public DateTime genera_fecha { get; set; }
        public string documento { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }
}