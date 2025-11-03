namespace PgxAPI.Models.CPR
{
    public class OrdenCompraDto
    {
        public int total { get; set; }
        public List<OrdenCompra> ordenes { get; set; } = new List<OrdenCompra>();
    }

    public class OrdenCompra
    {
        public string cod_orden { get; set; } = string.Empty;
        public string tipoOrdenDesc { get; set; } = string.Empty;
        public decimal total { get; set; }
        public string genera_user { get; set; } = string.Empty;
        public DateTime genera_fecha { get; set; }
        public string tipoOrden { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public bool seleccionado { get; set; } = false;
    }

    public class OrdenCompraRequestDto
    {
        public string tipo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool todosPendientes { get; set; }
        public string fechaInicio { get; set; } = string.Empty;
        public string fechaCorte { get; set; } = string.Empty;
    }

    public class OrdenCompraResolucionRequestDto
    {
        public string usuario { get; set; } = string.Empty;
        public string codigosOrden { get; set; } = string.Empty;
    }

}
