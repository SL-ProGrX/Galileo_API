namespace Galileo_API.Models.ProGrX.Cobros
{
    /// <summary>
    /// Trabajo encolado para procesar la aplicación de fondos (Procesa) en segundo plano.
    /// </summary>
    public sealed record CoAplFndProcesoTrabajo(Guid ProcesoId, int CodEmpresa, FondosAplicarRequest Request);
}
