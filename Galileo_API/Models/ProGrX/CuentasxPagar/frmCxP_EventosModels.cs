namespace Galileo.Models.CxP
{
    public class CxPEventos
    {
        public string? cod_evento { get; set; }
        public string? descripcion { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_finaliza { get; set; }
        public int? comision_porc { get; set; }
        public string? cod_comision_cuenta { get; set; }
        public string? comision_cuenta { get; set; }
        public string? cod_linea_crd { get; set; }
        public string? notas { get; set; }
        public string? lugar_venta { get; set; }
        public bool? activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string? descripcion_linea_crd { get; set; }
    }

    public class CxPEventosProveedor
    {
        public int? cod_proveedor { get; set; }
        public string? descripcion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public int? comision_porc { get; set; }
        public int? asignado { get; set; }
        public bool activo { get; set; }
    }

    public class CxPEventosBusqueda
    {
        public int? cod_evento { get; set; }
        public string? descripcion { get; set; }

    }

    public class CxPEventosLineas
    {
        public string? crdcod { get; set; }
        public string? crddesc { get; set; }
    }
}