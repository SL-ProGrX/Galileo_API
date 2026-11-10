namespace Galileo_API.Models.ProGrX.Cobros
{
    /// <summary>
    /// Trabajo encolado para procesar la aplicación de excedentes en segundo plano.
    /// </summary>
    public sealed record CoAplExcProcesoTrabajo(Guid ProcesoId, int CodEmpresa, ExcedenteAplicarRequest Request);
}
