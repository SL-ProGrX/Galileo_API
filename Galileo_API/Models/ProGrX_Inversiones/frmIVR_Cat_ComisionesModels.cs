namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatComisionDto
    {
        public string Cod_Comision { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Cod_Cuenta_Desc { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Tipo_Valor { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Aplica_En { get; set; } = string.Empty;
        public string Aplica_Desc { get; set; } = string.Empty;
        public string Suman_En { get; set; } = string.Empty;
        public string Sumar_Desc { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
    }

    public sealed class IvrCatComisionGuardarRequest
    {
        public string Cod_Comision { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal? Valor { get; set; }
        public string Aplica_En { get; set; } = string.Empty;
        public string Suman_En { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
