namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXEfPersonalDto
    {
        public string Cod_Ef { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Activo { get; set; }
    }

    public class CntXEfPersonalSaveParams
    {
        public required int CodContabilidad { get; set; }
        public string CodEf { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short? Activo { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXEfPersonalDeleteParams
    {
        public required int CodContabilidad { get; set; }
        public string CodEf { get; set; } = string.Empty;
    }

    public class CntXEfSeccionDto
    {
        public string? ItemId { get; set; }
        public string? ItemIdMadre { get; set; }
        public string? Prioridad { get; set; }
        public int EsTitulo { get; set; }
        public int Totales { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
