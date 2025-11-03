namespace PgxAPI.Models.CxP
{
    public class PlantillaDTO
    {
        public string Cod_Plantilla { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Registro_Fecha { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public bool Activo { get; set; }

    }

    public class Plantilla_AsientoDTO
    {
        public int Linea { get; set; }
        public string Cod_Plantilla { get; set; } = string.Empty;
        public int Cod_Contabilidad { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public decimal Tipo_Cambio { get; set; }
        public decimal Porcentaje { get; set; }
        public string Desc_Cuenta { get; set; } = string.Empty;
    }

    public class Unidad
    {
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class Centro_Costo
    {
        public string Cod_Centro_Costo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

}
