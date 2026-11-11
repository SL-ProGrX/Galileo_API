namespace Galileo_API.Models.ProGrX.Cobros
{
    /// <summary>
    /// Trabajo encolado para procesar la aplicación de excedentes a mora por contratos en segundo plano.
    /// </summary>
    public sealed record CoAplExcContrProcesoTrabajo(Guid ProcesoId, int CodEmpresa, ExcContratosAplicarRequest Request);
}
