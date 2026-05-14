namespace Galileo.Models.ProGrX.Fondos
{
    public class FndLiqAutoParametroDto
    {
        public int idregistro { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string tipodato { get; set; } = string.Empty;
        public string usuarioactualiza { get; set; } = string.Empty;
        public DateTime? fechaactualiza { get; set; }
    }

    public class FndLiqAutoPlanesDto
    {
        public int idregistro { get; set; }
        public string operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool patrimonio { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }


    public class FndLiqAutoPlanesPatronalDto
    {
        public int idregistro { get; set; }
        public string operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool patrimonio { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class FndLiqAutoReporteDto
    {
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal saldo { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
    }
    public class FndLiqAutoPlanesAddRequestDto
    {
        public required int codEmpresa { get; set; }
        public string operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public required bool patrimonio { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string accion { get; set; } = string.Empty; // 'A' | 'B'
    }
}