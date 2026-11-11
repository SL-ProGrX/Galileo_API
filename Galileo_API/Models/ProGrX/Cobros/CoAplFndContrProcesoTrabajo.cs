namespace Galileo_API.Models.ProGrX.Cobros
{
    /// <summary>
    /// Trabajo encolado para procesar la aplicación de fondos a mora en segundo plano.
    /// </summary>
    public sealed record CoAplFndContrProcesoTrabajo(Guid ProcesoId, int CodEmpresa, ContratosAplicarRequest Request) : ICoProcesoTrabajo;
}
