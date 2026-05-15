namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class ConsolidaMapeoImportaResultDto
    {
        public int Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class ConsolidaMapeoImportaResultadoDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cuenta_Map { get; set; } = string.Empty;
        public string Cod_Cuenta_Excluye { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Validacion { get; set; } = string.Empty;
        public string Descripcion_Actual { get; set; } = string.Empty;
    }

    public class ConsolidaMapeoImportaValidaDto
    {
        public int Casos_Erroneos { get; set; }
    }

    public class ConsolidaMapeoActualDto
    {
        public string Cod_Cuenta_Origen { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Cod_Cuenta_Excluye { get; set; } = string.Empty;
        public string Descripcion_Origen { get; set; } = string.Empty;
        public string Validacion { get; set; } = string.Empty;
        public string Descripcion_Actual { get; set; } = string.Empty;
    }

    public class ConsolidaContabilidadDto
    {
        public int Consolida_Ind { get; set; }
        public int Consolida_Conta { get; set; }
        public string Consolida_Unidad { get; set; } = string.Empty;
    }

    public class ConsolidaMapeoImportaCargadoRequestDto
    {
        public int? Consolidadora { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string CtaConsolida { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int? Linea { get; set; }
    }
}
