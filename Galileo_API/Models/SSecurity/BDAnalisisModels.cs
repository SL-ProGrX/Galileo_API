namespace Galileo.Models.Security;

public sealed class BDAnalisisEstructuraDto
{
    public string Columna { get; set; } = string.Empty;
    public string TipoDato { get; set; } = string.Empty;
    public string Nulos { get; set; } = string.Empty;
    public string Tamano { get; set; } = string.Empty;
}
