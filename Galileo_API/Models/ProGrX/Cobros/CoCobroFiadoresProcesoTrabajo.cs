namespace Galileo_API.Models.ProGrX.Cobros
{
    /// <summary>
    /// Trabajo encolado para procesar los cobros a fiadores en segundo plano.
    /// </summary>
    public sealed record CoCobroFiadoresProcesoTrabajo(Guid ProcesoId, int CodEmpresa, string Usuario, IReadOnlyList<int> Ids) : ICoProcesoTrabajo;
}
