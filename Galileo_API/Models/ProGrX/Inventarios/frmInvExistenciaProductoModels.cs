namespace PgxAPI.Models.INV
{
    public class ExistenciaProductoDTO
    {
        public string Bodega { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Existencia { get; set; } = 0;

    }

    public class ExistenciaProducto_Filtros
    {
        public string? beneficio_id { get; set; }
        public string? beneficiario_nombre { get; set; }
        public string? solicita_id { get; set; }
        public string? solicita_nombre { get; set; }
        public string? estado_persona { get; set; }
        public string? institucion { get; set; }
        public string? usuario_registra { get; set; }
        public string? usuario_autoriza { get; set; }
        public string? unidad { get; set; }
        public string? oficina { get; set; }
        public string? estado { get; set; }
        public string? fecha { get; set; }
        public string? fecha_inicio { get; set; }
        public string? fecha_corte { get; set; }
        public string? Bodega { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? cod_Bodega { get; set; }
        public string? cod_Producto { get; set; }
    }

}
