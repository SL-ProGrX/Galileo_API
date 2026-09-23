namespace Galileo.Models.ProGrX.Clientes;

public sealed class AfRenunciaConfiguracion
{
    public bool aporte_patronal { get; set; }
    public bool arreglo_pago { get; set; }
}

public sealed class AfRenunciaProceso : AfRenunciaLiquidacion
{
    public List<AfRenunciaPlan> Planes { get; set; } = [];
    public List<AfRenunciaAbono> Abonos { get; set; } = [];
    public List<string> Motivos { get; set; } = [];
}
